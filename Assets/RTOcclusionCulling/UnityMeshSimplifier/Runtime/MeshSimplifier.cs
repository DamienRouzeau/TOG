﻿#region License
/*
MIT License

Copyright(c) 2017 Mattias Edlund

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion

#region Original License
/////////////////////////////////////////////
//
// Mesh Simplification Tutorial
//
// (C) by Sven Forstmann in 2014
//
// License : MIT
// http://opensource.org/licenses/MIT
//
//https://github.com/sp4cerat/Fast-Quadric-Mesh-Simplification
#endregion

#if UNITY_2018_2 || UNITY_2018_3 || UNITY_2018_4 || UNITY_2019
#define UNITY_8UV_SUPPORT
#endif

#if UNITY_2017_3 || UNITY_2017_4 || UNITY_2018 || UNITY_2019
#define UNITY_MESH_INDEXFORMAT_SUPPORT
#endif

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityMeshSimplifier
{
    /// <summary>
    /// The mesh simplifier.
    /// Deeply based on https://github.com/sp4cerat/Fast-Quadric-Mesh-Simplification but rewritten completely in C#.
    /// </summary>
    public sealed class MeshSimplifier
    {
        #region Consts
        private const double DoubleEpsilon = 1.0E-3;
        private const int UVChannelCount = MeshUtils.UVChannelCount;
        #endregion

        #region Classes
        #region Triangle
        private struct Triangle
        {
            #region Fields
            public int v0;
            public int v1;
            public int v2;
            public int subMeshIndex;

            public int va0;
            public int va1;
            public int va2;

            public double err0;
            public double err1;
            public double err2;
            public double err3;

            public bool deleted;
            public bool dirty;
            public Vector3d n;
            #endregion

            #region Properties
            public int this[int index]
            {
                get
                {
                    return (index == 0 ? v0 : (index == 1 ? v1 : v2));
                }
                set
                {
                    switch (index)
                    {
                        case 0:
                            v0 = value;
                            break;
                        case 1:
                            v1 = value;
                            break;
                        case 2:
                            v2 = value;
                            break;
                        default:
                            throw new IndexOutOfRangeException();
                    }
                }
            }
            #endregion

            #region Constructor
            public Triangle(int v0, int v1, int v2, int subMeshIndex)
            {
                this.v0 = v0;
                this.v1 = v1;
                this.v2 = v2;
                this.subMeshIndex = subMeshIndex;

                this.va0 = v0;
                this.va1 = v1;
                this.va2 = v2;

                err0 = err1 = err2 = err3 = 0;
                deleted = dirty = false;
                n = new Vector3d();
            }
            #endregion

            #region Public Methods
            public void GetAttributeIndices(int[] attributeIndices)
            {
                attributeIndices[0] = va0;
                attributeIndices[1] = va1;
                attributeIndices[2] = va2;
            }

            public void SetAttributeIndex(int index, int value)
            {
                switch (index)
                {
                    case 0:
                        va0 = value;
                        break;
                    case 1:
                        va1 = value;
                        break;
                    case 2:
                        va2 = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }

            public void GetErrors(double[] err)
            {
                err[0] = err0;
                err[1] = err1;
                err[2] = err2;
            }
            #endregion
        }
        #endregion

        #region Vertex
        private struct Vertex
        {
            public Vector3d p;
            public int tstart;
            public int tcount;
            public SymmetricMatrix q;
            public bool borderEdge;
            public bool uvSeamEdge;
            public bool uvFoldoverEdge;

            public Vertex(Vector3d p)
            {
                this.p = p;
                this.tstart = 0;
                this.tcount = 0;
                this.q = new SymmetricMatrix();
                this.borderEdge = true;
                this.uvSeamEdge = false;
                this.uvFoldoverEdge = false;
            }
        }
        #endregion

        #region Ref
        private struct Ref
        {
            public int tid;
            public int tvertex;

            public void Set(int tid, int tvertex)
            {
                this.tid = tid;
                this.tvertex = tvertex;
            }
        }
        #endregion

        #region UV Channels
        private class UVChannels<TVec>
        {
            private ResizableArray<TVec>[] channels = null;
            private TVec[][] channelsData = null;

            public TVec[][] Data
            {
                get
                {
                    for (int i = 0; i < UVChannelCount; i++)
                    {
                        if (channels[i] != null)
                        {
                            channelsData[i] = channels[i].Data;
                        }
                        else
                        {
                            channelsData[i] = null;
                        }
                    }
                    return channelsData;
                }
            }

            /// <summary>
            /// Gets or sets a specific channel by index.
            /// </summary>
            /// <param name="index">The channel index.</param>
            public ResizableArray<TVec> this[int index]
            {
                get { return channels[index]; }
                set { channels[index] = value; }
            }

            public UVChannels()
            {
                channels = new ResizableArray<TVec>[UVChannelCount];
                channelsData = new TVec[UVChannelCount][];
            }

            /// <summary>
            /// Resizes all channels at once.
            /// </summary>
            /// <param name="capacity">The new capacity.</param>
            /// <param name="trimExess">If exess memory should be trimmed.</param>
            public void Resize(int capacity, bool trimExess = false)
            {
                for (int i = 0; i < UVChannelCount; i++)
                {
                    if (channels[i] != null)
                    {
                        channels[i].Resize(capacity, trimExess);
                    }
                }
            }
        }
        #endregion

        #region Border Vertex
        private struct BorderVertex
        {
            public int index;
            public int hash;

            public BorderVertex(int index, int hash)
            {
                this.index = index;
                this.hash = hash;
            }
        }
        #endregion

        #region Border Vertex Comparer
        private class BorderVertexComparer : IComparer<BorderVertex>
        {
            public static readonly BorderVertexComparer instance = new BorderVertexComparer();

            public int Compare(BorderVertex x, BorderVertex y)
            {
                return x.hash.CompareTo(y.hash);
            }
        }
        #endregion
        #endregion

        #region Fields
        private bool preserveBorderEdges = false;
        private bool preserveUVSeamEdges = false;
        private bool preserveUVFoldoverEdges = false;
        private bool enableSmartLink = true;
        private int maxIterationCount = 100;
        private double agressiveness = 7.0;
        private bool verbose = false;

        private double vertexLinkDistanceSqr = double.Epsilon;

        private int subMeshCount = 0;
        private int[] subMeshOffsets = null;
        private ResizableArray<Triangle> triangles = null;
        private ResizableArray<Vertex> vertices = null;
        private ResizableArray<Ref> refs = null;

        private ResizableArray<Vector3> vertNormals = null;
        private ResizableArray<Vector4> vertTangents = null;
        private UVChannels<Vector2> vertUV2D = null;
        private UVChannels<Vector3> vertUV3D = null;
        private UVChannels<Vector4> vertUV4D = null;
        private ResizableArray<Color> vertColors = null;

        // Pre-allocated buffers
        private double[] errArr = new double[3];
        private int[] attributeIndexArr = new int[3];
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets if the border edges should be preserved.
        /// Default value: false
        /// </summary>
        [Obsolete("Use the 'MeshSimplifier.PreserveBorderEdges' property instead.", false)]
        public bool PreserveBorders
        {
            get { return this.PreserveBorderEdges; }
            set { this.PreserveBorderEdges = value; }
        }

        /// <summary>
        /// Gets or sets if the border edges should be preserved.
        /// Default value: false
        /// </summary>
        public bool PreserveBorderEdges
        {
            get { return preserveBorderEdges; }
            set { preserveBorderEdges = value; }
        }

        /// <summary>
        /// Gets or sets if the UV seam edges should be preserved.
        /// Default value: false
        /// </summary>
        [Obsolete("Use the 'MeshSimplifier.PreserveUVSeamEdges' property instead.", false)]
        public bool PreserveSeams
        {
            get { return this.PreserveUVSeamEdges; }
            set { this.PreserveUVSeamEdges = value; }
        }

        /// <summary>
        /// Gets or sets if the UV seam edges should be preserved.
        /// Default value: false
        /// </summary>
        public bool PreserveUVSeamEdges
        {
            get { return preserveUVSeamEdges; }
            set { preserveUVSeamEdges = value; }
        }

        /// <summary>
        /// Gets or sets if the UV foldover edges should be preserved.
        /// Default value: false
        /// </summary>
        [Obsolete("Use the 'MeshSimplifier.PreserveUVFoldoverEdges' property instead.", false)]
        public bool PreserveFoldovers
        {
            get { return this.PreserveUVFoldoverEdges; }
            set { this.PreserveUVFoldoverEdges = value; }
        }

        /// <summary>
        /// Gets or sets if the UV foldover edges should be preserved.
        /// Default value: false
        /// </summary>
        public bool PreserveUVFoldoverEdges
        {
            get { return preserveUVFoldoverEdges; }
            set { preserveUVFoldoverEdges = value; }
        }

        /// <summary>
        /// Gets or sets if a feature for smarter vertex linking should be enabled, reducing artifacts in the
        /// decimated result at the cost of a slightly more expensive initialization by treating vertices at
        /// the same position as the same vertex while separating the attributes.
        /// Default value: true
        /// </summary>
        public bool EnableSmartLink
        {
            get { return enableSmartLink; }
            set { enableSmartLink = value; }
        }

        /// <summary>
        /// Gets or sets the maximum iteration count. Higher number is more expensive but can bring you closer to your target quality.
        /// Sometimes a lower maximum count might be desired in order to lower the performance cost.
        /// Default value: 100
        /// </summary>
        public int MaxIterationCount
        {
            get { return maxIterationCount; }
            set { maxIterationCount = value; }
        }

        /// <summary>
        /// Gets or sets the agressiveness of the mesh simplification. Higher number equals higher quality, but more expensive to run.
        /// Default value: 7.0
        /// </summary>
        public double Agressiveness
        {
            get { return agressiveness; }
            set { agressiveness = value; }
        }

        /// <summary>
        /// Gets or sets if verbose information should be printed to the console.
        /// Default value: false
        /// </summary>
        public bool Verbose
        {
            get { return verbose; }
            set { verbose = value; }
        }

        /// <summary>
        /// Gets or sets the maximum distance between two vertices in order to link them.
        /// Note that this value is only used if EnableSmartLink is true.
        /// </summary>
        public double VertexLinkDistance
        {
            get { return Math.Sqrt(vertexLinkDistanceSqr); }
            set { vertexLinkDistanceSqr = (value > double.Epsilon ? value * value : double.Epsilon); }
        }

        /// <summary>
        /// Gets or sets the maximum squared distance between two vertices in order to link them.
        /// Note that this value is only used if EnableSmartLink is true.
        /// Default value: double.Epsilon
        /// </summary>
        public double VertexLinkDistanceSqr
        {
            get { return vertexLinkDistanceSqr; }
            set { vertexLinkDistanceSqr = value; }
        }

        /// <summary>
        /// Gets or sets the vertex positions.
        /// </summary>
        public Vector3[] Vertices
        {
            get
            {
                int vertexCount = this.vertices.Length;
                var vertices = new Vector3[vertexCount];
                var vertArr = this.vertices.Data;
                for (int i = 0; i < vertexCount; i++)
                {
                    vertices[i] = (Vector3)vertArr[i].p;
                }
                return vertices;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                vertices.Resize(value.Length);
                var vertArr = vertices.Data;
                for (int i = 0; i < value.Length; i++)
                {
                    vertArr[i] = new Vertex(value[i]);
                }
            }
        }

        /// <summary>
        /// Gets the count of sub-meshes.
        /// </summary>
        public int SubMeshCount
        {
            get { return subMeshCount; }
        }

        /// <summary>
        /// Gets or sets the vertex normals.
        /// </summary>
        public Vector3[] Normals
        {
            get { return (vertNormals != null ? vertNormals.Data : null); }
            set
            {
                InitializeVertexAttribute(value, ref vertNormals, "normals");
            }
        }

        /// <summary>
        /// Gets or sets the vertex tangents.
        /// </summary>
        public Vector4[] Tangents
        {
            get { return (vertTangents != null ? vertTangents.Data : null); }
            set
            {
                InitializeVertexAttribute(value, ref vertTangents, "tangents");
            }
        }

        /// <summary>
        /// Gets or sets the vertex 2D UV set 1.
        /// </summary>
        public Vector2[] UV1
        {
            get { return GetUVs2D(0); }
            set { SetUVs(0, value); }
        }

        /// <summary>
        /// Gets or sets the vertex 2D UV set 2.
        /// </summary>
        public Vector2[] UV2
        {
            get { return GetUVs2D(1); }
            set { SetUVs(1, value); }
        }

        /// <summary>
        /// Gets or sets the vertex 2D UV set 3.
        /// </summary>
        public Vector2[] UV3
        {
            get { return GetUVs2D(2); }
            set { SetUVs(2, value); }
        }

        /// <summary>
        /// Gets or sets the vertex 2D UV set 4.
        /// </summary>
        public Vector2[] UV4
        {
            get { return GetUVs2D(3); }
            set { SetUVs(3, value); }
        }

#if UNITY_8UV_SUPPORT
        /// <summary>
        /// Gets or sets the vertex 2D UV set 5.
        /// </summary>
        public Vector2[] UV5
        {
            get { return GetUVs2D(4); }
            set { SetUVs(4, value); }
        }

        /// <summary>
        /// Gets or sets the vertex 2D UV set 6.
        /// </summary>
        public Vector2[] UV6
        {
            get { return GetUVs2D(5); }
            set { SetUVs(5, value); }
        }

        /// <summary>
        /// Gets or sets the vertex 2D UV set 7.
        /// </summary>
        public Vector2[] UV7
        {
            get { return GetUVs2D(6); }
            set { SetUVs(6, value); }
        }

        /// <summary>
        /// Gets or sets the vertex 2D UV set 8.
        /// </summary>
        public Vector2[] UV8
        {
            get { return GetUVs2D(7); }
            set { SetUVs(7, value); }
        }
#endif

        /// <summary>
        /// Gets or sets the vertex colors.
        /// </summary>
        public Color[] Colors
        {
            get { return (vertColors != null ? vertColors.Data : null); }
            set
            {
                InitializeVertexAttribute(value, ref vertColors, "colors");
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new mesh simplifier.
        /// </summary>
        public MeshSimplifier()
        {
            triangles = new ResizableArray<Triangle>(0);
            vertices = new ResizableArray<Vertex>(0);
            refs = new ResizableArray<Ref>(0);
        }

        /// <summary>
        /// Creates a new mesh simplifier.
        /// </summary>
        /// <param name="mesh">The original mesh to simplify.</param>
        public MeshSimplifier(Mesh mesh)
            : this()
        {
            if (mesh != null)
            {
                Initialize(mesh);
            }
        }
        #endregion

        #region Private Methods
        #region Initialize Vertex Attribute
        private void InitializeVertexAttribute<T>(T[] attributeValues, ref ResizableArray<T> attributeArray, string attributeName)
        {
            if (attributeValues != null && attributeValues.Length == vertices.Length)
            {
                if (attributeArray == null)
                {
                    attributeArray = new ResizableArray<T>(attributeValues.Length, attributeValues.Length);
                }
                else
                {
                    attributeArray.Resize(attributeValues.Length);
                }

                var arrayData = attributeArray.Data;
                Array.Copy(attributeValues, 0, arrayData, 0, attributeValues.Length);
            }
            else
            {
                if (attributeValues != null && attributeValues.Length > 0)
                {
                    Debug.LogErrorFormat("Failed to set vertex attribute '{0}' with {1} length of array, when {2} was needed.", attributeName, attributeValues.Length, vertices.Length);
                }
                attributeArray = null;
            }
        }
        #endregion

        #region Calculate Error
        private double VertexError(ref SymmetricMatrix q, double x, double y, double z)
        {
            return q.m0 * x * x + 2 * q.m1 * x * y + 2 * q.m2 * x * z + 2 * q.m3 * x + q.m4 * y * y
                + 2 * q.m5 * y * z + 2 * q.m6 * y + q.m7 * z * z + 2 * q.m8 * z + q.m9;
        }

        private double CalculateError(ref Vertex vert0, ref Vertex vert1, out Vector3d result)
        {
            // compute interpolated vertex
            SymmetricMatrix q = (vert0.q + vert1.q);
            bool borderEdge = (vert0.borderEdge & vert1.borderEdge);
            double error = 0.0;
            double det = q.Determinant1();
            if (det != 0.0 && !borderEdge)
            {
                // q_delta is invertible
                result = new Vector3d(
                    -1.0 / det * q.Determinant2(),  // vx = A41/det(q_delta)
                    1.0 / det * q.Determinant3(),   // vy = A42/det(q_delta)
                    -1.0 / det * q.Determinant4()); // vz = A43/det(q_delta)
                error = VertexError(ref q, result.x, result.y, result.z);
            }
            else
            {
                // det = 0 -> try to find best result
                Vector3d p1 = vert0.p;
                Vector3d p2 = vert1.p;
                Vector3d p3 = (p1 + p2) * 0.5f;
                double error1 = VertexError(ref q, p1.x, p1.y, p1.z);
                double error2 = VertexError(ref q, p2.x, p2.y, p2.z);
                double error3 = VertexError(ref q, p3.x, p3.y, p3.z);
                error = MathHelper.Min(error1, error2, error3);
                if (error == error3)
                {
                    result = p3;
                }
                else if (error == error2)
                {
                    result = p2;
                }
                else if (error == error1)
                {
                    result = p1;
                }
                else
                {
                    result = p3;
                }
            }
            return error;
        }
        #endregion

        #region Calculate Barycentric Coordinates
        private static void CalculateBarycentricCoords(ref Vector3d point, ref Vector3d a, ref Vector3d b, ref Vector3d c, out Vector3 result)
        {
            Vector3 v0 = (Vector3)(b - a), v1 = (Vector3)(c - a), v2 = (Vector3)(point - a);
            float d00 = Vector3.Dot(v0, v0);
            float d01 = Vector3.Dot(v0, v1);
            float d11 = Vector3.Dot(v1, v1);
            float d20 = Vector3.Dot(v2, v0);
            float d21 = Vector3.Dot(v2, v1);
            float denom = d00 * d11 - d01 * d01;
            float v = (d11 * d20 - d01 * d21) / denom;
            float w = (d00 * d21 - d01 * d20) / denom;
            float u = 1f - v - w;
            result = new Vector3(u, v, w);
        }
        #endregion

        #region Normalize Tangent
        private static Vector4 NormalizeTangent(Vector4 tangent)
        {
            var tangentVec = new Vector3(tangent.x, tangent.y, tangent.z);
            tangentVec.Normalize();
            return new Vector4(tangentVec.x, tangentVec.y, tangentVec.z, tangent.w);
        }
        #endregion

        #region Flipped
        /// <summary>
        /// Check if a triangle flips when this edge is removed
        /// </summary>
        private bool Flipped(ref Vector3d p, int i0, int i1, ref Vertex v0, bool[] deleted)
        {
            int tcount = v0.tcount;
            var refs = this.refs.Data;
            var triangles = this.triangles.Data;
            var vertices = this.vertices.Data;
            for (int k = 0; k < tcount; k++)
            {
                Ref r = refs[v0.tstart + k];
                if (triangles[r.tid].deleted)
                    continue;

                int s = r.tvertex;
                int id1 = triangles[r.tid][(s + 1) % 3];
                int id2 = triangles[r.tid][(s + 2) % 3];
                if (id1 == i1 || id2 == i1)
                {
                    deleted[k] = true;
                    continue;
                }

                Vector3d d1 = vertices[id1].p - p;
                d1.Normalize();
                Vector3d d2 = vertices[id2].p - p;
                d2.Normalize();
                double dot = Vector3d.Dot(ref d1, ref d2);
                if (System.Math.Abs(dot) > 0.999)
                    return true;

                Vector3d n;
                Vector3d.Cross(ref d1, ref d2, out n);
                n.Normalize();
                deleted[k] = false;
                dot = Vector3d.Dot(ref n, ref triangles[r.tid].n);
                if (dot < 0.2)
                    return true;
            }

            return false;
        }
        #endregion

        #region Update Triangles
        /// <summary>
        /// Update triangle connections and edge error after a edge is collapsed.
        /// </summary>
        private void UpdateTriangles(int i0, int ia0, ref Vertex v, ResizableArray<bool> deleted, ref int deletedTriangles)
        {
            Vector3d p;
            int tcount = v.tcount;
            var triangles = this.triangles.Data;
            var vertices = this.vertices.Data;
            for (int k = 0; k < tcount; k++)
            {
                Ref r = refs[v.tstart + k];
                int tid = r.tid;
                Triangle t = triangles[tid];
                if (t.deleted)
                    continue;

                if (deleted[k])
                {
                    triangles[tid].deleted = true;
                    ++deletedTriangles;
                    continue;
                }

                t[r.tvertex] = i0;
                if (ia0 != -1)
                {
                    t.SetAttributeIndex(r.tvertex, ia0);
                }

                t.dirty = true;
                t.err0 = CalculateError(ref vertices[t.v0], ref vertices[t.v1], out p);
                t.err1 = CalculateError(ref vertices[t.v1], ref vertices[t.v2], out p);
                t.err2 = CalculateError(ref vertices[t.v2], ref vertices[t.v0], out p);
                t.err3 = MathHelper.Min(t.err0, t.err1, t.err2);
                triangles[tid] = t;
                refs.Add(r);
            }
        }
        #endregion

        #region Interpolate Vertex Attributes
        private void InterpolateVertexAttributes(int dst, int i0, int i1, int i2, ref Vector3 barycentricCoord)
        {
            if (vertNormals != null)
            {
                vertNormals[dst] = Vector3.Normalize((vertNormals[i0] * barycentricCoord.x) + (vertNormals[i1] * barycentricCoord.y) + (vertNormals[i2] * barycentricCoord.z));
            }
            if (vertTangents != null)
            {
                vertTangents[dst] = NormalizeTangent((vertTangents[i0] * barycentricCoord.x) + (vertTangents[i1] * barycentricCoord.y) + (vertTangents[i2] * barycentricCoord.z));
            }
            if (vertUV2D != null)
            {
                for (int i = 0; i < UVChannelCount; i++)
                {
                    var vertUV = vertUV2D[i];
                    if (vertUV != null)
                    {
                        vertUV[dst] = (vertUV[i0] * barycentricCoord.x) + (vertUV[i1] * barycentricCoord.y) + (vertUV[i2] * barycentricCoord.z);
                    }
                }
            }
            if (vertUV3D != null)
            {
                for (int i = 0; i < UVChannelCount; i++)
                {
                    var vertUV = vertUV3D[i];
                    if (vertUV != null)
                    {
                        vertUV[dst] = (vertUV[i0] * barycentricCoord.x) + (vertUV[i1] * barycentricCoord.y) + (vertUV[i2] * barycentricCoord.z);
                    }
                }
            }
            if (vertUV4D != null)
            {
                for (int i = 0; i < UVChannelCount; i++)
                {
                    var vertUV = vertUV4D[i];
                    if (vertUV != null)
                    {
                        vertUV[dst] = (vertUV[i0] * barycentricCoord.x) + (vertUV[i1] * barycentricCoord.y) + (vertUV[i2] * barycentricCoord.z);
                    }
                }
            }
            if (vertColors != null)
            {
                vertColors[dst] = (vertColors[i0] * barycentricCoord.x) + (vertColors[i1] * barycentricCoord.y) + (vertColors[i2] * barycentricCoord.z);
            }
        }
        #endregion

        #region Are UVs The Same
        private bool AreUVsTheSame(int channel, int indexA, int indexB)
        {
            if (vertUV2D != null)
            {
                var vertUV = vertUV2D[channel];
                if (vertUV != null)
                {
                    var uvA = vertUV[indexA];
                    var uvB = vertUV[indexB];
                    return uvA == uvB;
                }
            }

            if (vertUV3D != null)
            {
                var vertUV = vertUV3D[channel];
                if (vertUV != null)
                {
                    var uvA = vertUV[indexA];
                    var uvB = vertUV[indexB];
                    return uvA == uvB;
                }
            }

            if (vertUV4D != null)
            {
                var vertUV = vertUV4D[channel];
                if (vertUV != null)
                {
                    var uvA = vertUV[indexA];
                    var uvB = vertUV[indexB];
                    return uvA == uvB;
                }
            }

            return false;
        }
        #endregion

        #region Remove Vertex Pass
        /// <summary>
        /// Remove vertices and mark deleted triangles
        /// </summary>
        private void RemoveVertexPass(int startTrisCount, int targetTrisCount, double threshold, ResizableArray<bool> deleted0, ResizableArray<bool> deleted1, ref int deletedTris)
        {
            var triangles = this.triangles.Data;
            int triangleCount = this.triangles.Length;
            var vertices = this.vertices.Data;

            Vector3d p;
            Vector3 barycentricCoord;
            for (int tid = 0; tid < triangleCount; tid++)
            {
                if (triangles[tid].dirty || triangles[tid].deleted || triangles[tid].err3 > threshold)
                    continue;

                triangles[tid].GetErrors(errArr);
                triangles[tid].GetAttributeIndices(attributeIndexArr);
                for (int edgeIndex = 0; edgeIndex < 3; edgeIndex++)
                {
                    if (errArr[edgeIndex] > threshold)
                        continue;

                    int nextEdgeIndex = ((edgeIndex + 1) % 3);
                    int i0 = triangles[tid][edgeIndex];
                    int i1 = triangles[tid][nextEdgeIndex];

                    // Border check
                    if (vertices[i0].borderEdge != vertices[i1].borderEdge)
                        continue;
                    // Seam check
                    else if (vertices[i0].uvSeamEdge != vertices[i1].uvSeamEdge)
                        continue;
                    // Foldover check
                    else if (vertices[i0].uvFoldoverEdge != vertices[i1].uvFoldoverEdge)
                        continue;
                    // If borders should be preserved
                    else if (preserveBorderEdges && vertices[i0].borderEdge)
                        continue;
                    // If seams should be preserved
                    else if (preserveUVSeamEdges && vertices[i0].uvSeamEdge)
                        continue;
                    // If foldovers should be preserved
                    else if (preserveUVFoldoverEdges && vertices[i0].uvFoldoverEdge)
                        continue;

                    // Compute vertex to collapse to
                    CalculateError(ref vertices[i0], ref vertices[i1], out p);
                    deleted0.Resize(vertices[i0].tcount); // normals temporarily
                    deleted1.Resize(vertices[i1].tcount); // normals temporarily

                    // Don't remove if flipped
                    if (Flipped(ref p, i0, i1, ref vertices[i0], deleted0.Data))
                        continue;
                    if (Flipped(ref p, i1, i0, ref vertices[i1], deleted1.Data))
                        continue;

                    // Calculate the barycentric coordinates within the triangle
                    int nextNextEdgeIndex = ((edgeIndex + 2) % 3);
                    int i2 = triangles[tid][nextNextEdgeIndex];
                    CalculateBarycentricCoords(ref p, ref vertices[i0].p, ref vertices[i1].p, ref vertices[i2].p, out barycentricCoord);

                    // Not flipped, so remove edge
                    vertices[i0].p = p;
                    vertices[i0].q += vertices[i1].q;

                    // Interpolate the vertex attributes
                    int ia0 = attributeIndexArr[edgeIndex];
                    int ia1 = attributeIndexArr[nextEdgeIndex];
                    int ia2 = attributeIndexArr[nextNextEdgeIndex];
                    InterpolateVertexAttributes(ia0, ia0, ia1, ia2, ref barycentricCoord);

                    if (vertices[i0].uvSeamEdge)
                    {
                        ia0 = -1;
                    }

                    int tstart = refs.Length;
                    UpdateTriangles(i0, ia0, ref vertices[i0], deleted0, ref deletedTris);
                    UpdateTriangles(i0, ia0, ref vertices[i1], deleted1, ref deletedTris);

                    int tcount = refs.Length - tstart;
                    if (tcount <= vertices[i0].tcount)
                    {
                        // save ram
                        if (tcount > 0)
                        {
                            var refsArr = refs.Data;
                            Array.Copy(refsArr, tstart, refsArr, vertices[i0].tstart, tcount);
                        }
                    }
                    else
                    {
                        // append
                        vertices[i0].tstart = tstart;
                    }

                    vertices[i0].tcount = tcount;
                    break;
                }

                // Check if we are already done
                if ((startTrisCount - deletedTris) <= targetTrisCount)
                    break;
            }
        }
        #endregion

        #region Update Mesh
        /// <summary>
        /// Compact triangles, compute edge error and build reference list.
        /// </summary>
        /// <param name="iteration">The iteration index.</param>
        private void UpdateMesh(int iteration)
        {
            var triangles = this.triangles.Data;
            var vertices = this.vertices.Data;

            int triangleCount = this.triangles.Length;
            int vertexCount = this.vertices.Length;
            if (iteration > 0) // compact triangles
            {
                int dst = 0;
                for (int i = 0; i < triangleCount; i++)
                {
                    if (!triangles[i].deleted)
                    {
                        if (dst != i)
                        {
                            triangles[dst] = triangles[i];
                        }
                        dst++;
                    }
                }
                this.triangles.Resize(dst);
                triangles = this.triangles.Data;
                triangleCount = dst;
            }

            UpdateReferences();

            // Identify boundary : vertices[].border=0,1
            if (iteration == 0)
            {
                var refs = this.refs.Data;

                var vcount = new List<int>(8);
                var vids = new List<int>(8);
                int vsize = 0;
                for (int i = 0; i < vertexCount; i++)
                {
                    vertices[i].borderEdge = false;
                    vertices[i].uvSeamEdge = false;
                    vertices[i].uvFoldoverEdge = false;
                }

                int ofs;
                int id;
                int borderVertexCount = 0;
                double borderMinX = double.MaxValue;
                double borderMaxX = double.MinValue;
                for (int i = 0; i < vertexCount; i++)
                {
                    int tstart = vertices[i].tstart;
                    int tcount = vertices[i].tcount;
                    vcount.Clear();
                    vids.Clear();
                    vsize = 0;

                    for (int j = 0; j < tcount; j++)
                    {
                        int tid = refs[tstart + j].tid;
                        for (int k = 0; k < 3; k++)
                        {
                            ofs = 0;
                            id = triangles[tid][k];
                            while (ofs < vsize)
                            {
                                if (vids[ofs] == id)
                                    break;

                                ++ofs;
                            }

                            if (ofs == vsize)
                            {
                                vcount.Add(1);
                                vids.Add(id);
                                ++vsize;
                            }
                            else
                            {
                                ++vcount[ofs];
                            }
                        }
                    }

                    for (int j = 0; j < vsize; j++)
                    {
                        if (vcount[j] == 1)
                        {
                            id = vids[j];
                            vertices[id].borderEdge = true;
                            ++borderVertexCount;

                            if (enableSmartLink)
                            {
                                if (vertices[id].p.x < borderMinX)
                                {
                                    borderMinX = vertices[id].p.x;
                                }
                                if (vertices[id].p.x > borderMaxX)
                                {
                                    borderMaxX = vertices[id].p.x;
                                }
                            }
                        }
                    }
                }

                if (enableSmartLink)
                {
                    // First find all border vertices
                    var borderVertices = new BorderVertex[borderVertexCount];
                    int borderIndexCount = 0;
                    double borderAreaWidth = borderMaxX - borderMinX;
                    for (int i = 0; i < vertexCount; i++)
                    {
                        if (vertices[i].borderEdge)
                        {
                            int vertexHash = (int)(((((vertices[i].p.x - borderMinX) / borderAreaWidth) * 2.0) - 1.0) * int.MaxValue);
                            borderVertices[borderIndexCount] = new BorderVertex(i, vertexHash);
                            ++borderIndexCount;
                        }
                    }

                    // Sort the border vertices by hash
                    Array.Sort(borderVertices, 0, borderIndexCount, BorderVertexComparer.instance);

                    // Calculate the maximum hash distance based on the maximum vertex link distance
                    double vertexLinkDistance = Math.Sqrt(vertexLinkDistanceSqr);
                    int hashMaxDistance = Math.Max((int)((vertexLinkDistance / borderAreaWidth) * int.MaxValue), 1);

                    // Then find identical border vertices and bind them together as one
                    for (int i = 0; i < borderIndexCount; i++)
                    {
                        int myIndex = borderVertices[i].index;
                        if (myIndex == -1)
                            continue;

                        var myPoint = vertices[myIndex].p;
                        for (int j = i + 1; j < borderIndexCount; j++)
                        {
                            int otherIndex = borderVertices[j].index;
                            if (otherIndex == -1)
                                continue;
                            else if ((borderVertices[j].hash - borderVertices[i].hash) > hashMaxDistance) // There is no point to continue beyond this point
                                break;

                            var otherPoint = vertices[otherIndex].p;
                            var sqrX = ((myPoint.x - otherPoint.x) * (myPoint.x - otherPoint.x));
                            var sqrY = ((myPoint.y - otherPoint.y) * (myPoint.y - otherPoint.y));
                            var sqrZ = ((myPoint.z - otherPoint.z) * (myPoint.z - otherPoint.z));
                            var sqrMagnitude = sqrX + sqrY + sqrZ;

                            if (sqrMagnitude <= vertexLinkDistanceSqr)
                            {
                                borderVertices[j].index = -1; // NOTE: This makes sure that the "other" vertex is not processed again
                                vertices[myIndex].borderEdge = false;
                                vertices[otherIndex].borderEdge = false;

                                if (AreUVsTheSame(0, myIndex, otherIndex))
                                {
                                    vertices[myIndex].uvFoldoverEdge = true;
                                    vertices[otherIndex].uvFoldoverEdge = true;
                                }
                                else
                                {
                                    vertices[myIndex].uvSeamEdge = true;
                                    vertices[otherIndex].uvSeamEdge = true;
                                }

                                int otherTriangleCount = vertices[otherIndex].tcount;
                                int otherTriangleStart = vertices[otherIndex].tstart;
                                for (int k = 0; k < otherTriangleCount; k++)
                                {
                                    var r = refs[otherTriangleStart + k];
                                    triangles[r.tid][r.tvertex] = myIndex;
                                }
                            }
                        }
                    }

                    // Update the references again
                    UpdateReferences();
                }

                // Init Quadrics by Plane & Edge Errors
                //
                // required at the beginning ( iteration == 0 )
                // recomputing during the simplification is not required,
                // but mostly improves the result for closed meshes
                for (int i = 0; i < vertexCount; i++)
                {
                    vertices[i].q = new SymmetricMatrix();
                }

                int v0, v1, v2;
                Vector3d n, p0, p1, p2, p10, p20, dummy;
                SymmetricMatrix sm;
                for (int i = 0; i < triangleCount; i++)
                {
                    v0 = triangles[i].v0;
                    v1 = triangles[i].v1;
                    v2 = triangles[i].v2;

                    p0 = vertices[v0].p;
                    p1 = vertices[v1].p;
                    p2 = vertices[v2].p;
                    p10 = p1 - p0;
                    p20 = p2 - p0;
                    Vector3d.Cross(ref p10, ref p20, out n);
                    n.Normalize();
                    triangles[i].n = n;

                    sm = new SymmetricMatrix(n.x, n.y, n.z, -Vector3d.Dot(ref n, ref p0));
                    vertices[v0].q += sm;
                    vertices[v1].q += sm;
                    vertices[v2].q += sm;
                }

                for (int i = 0; i < triangleCount; i++)
                {
                    // Calc Edge Error
                    var triangle = triangles[i];
                    triangles[i].err0 = CalculateError(ref vertices[triangle.v0], ref vertices[triangle.v1], out dummy);
                    triangles[i].err1 = CalculateError(ref vertices[triangle.v1], ref vertices[triangle.v2], out dummy);
                    triangles[i].err2 = CalculateError(ref vertices[triangle.v2], ref vertices[triangle.v0], out dummy);
                    triangles[i].err3 = MathHelper.Min(triangles[i].err0, triangles[i].err1, triangles[i].err2);
                }
            }
        }
        #endregion

        #region Update References
        private void UpdateReferences()
        {
            int triangleCount = this.triangles.Length;
            int vertexCount = this.vertices.Length;
            var triangles = this.triangles.Data;
            var vertices = this.vertices.Data;

            // Init Reference ID list
            for (int i = 0; i < vertexCount; i++)
            {
                vertices[i].tstart = 0;
                vertices[i].tcount = 0;
            }

            for (int i = 0; i < triangleCount; i++)
            {
                ++vertices[triangles[i].v0].tcount;
                ++vertices[triangles[i].v1].tcount;
                ++vertices[triangles[i].v2].tcount;
            }

            int tstart = 0;
            for (int i = 0; i < vertexCount; i++)
            {
                vertices[i].tstart = tstart;
                tstart += vertices[i].tcount;
                vertices[i].tcount = 0;
            }

            // Write References
            this.refs.Resize(tstart);
            var refs = this.refs.Data;
            for (int i = 0; i < triangleCount; i++)
            {
                int v0 = triangles[i].v0;
                int v1 = triangles[i].v1;
                int v2 = triangles[i].v2;
                int start0 = vertices[v0].tstart;
                int count0 = vertices[v0].tcount;
                int start1 = vertices[v1].tstart;
                int count1 = vertices[v1].tcount;
                int start2 = vertices[v2].tstart;
                int count2 = vertices[v2].tcount;

                refs[start0 + count0].Set(i, 0);
                refs[start1 + count1].Set(i, 1);
                refs[start2 + count2].Set(i, 2);

                ++vertices[v0].tcount;
                ++vertices[v1].tcount;
                ++vertices[v2].tcount;
            }
        }
        #endregion

        #region Compact Mesh
        /// <summary>
        /// Finally compact mesh before exiting.
        /// </summary>
        private void CompactMesh()
        {
            int dst = 0;
            var vertices = this.vertices.Data;
            int vertexCount = this.vertices.Length;
            for (int i = 0; i < vertexCount; i++)
            {
                vertices[i].tcount = 0;
            }

            var vertNormals = (this.vertNormals != null ? this.vertNormals.Data : null);
            var vertTangents = (this.vertTangents != null ? this.vertTangents.Data : null);
            var vertUV2D = (this.vertUV2D != null ? this.vertUV2D.Data : null);
            var vertUV3D = (this.vertUV3D != null ? this.vertUV3D.Data : null);
            var vertUV4D = (this.vertUV4D != null ? this.vertUV4D.Data : null);
            var vertColors = (this.vertColors != null ? this.vertColors.Data : null);

            int lastSubMeshIndex = -1;
            subMeshOffsets = new int[subMeshCount];

            var triangles = this.triangles.Data;
            int triangleCount = this.triangles.Length;
            for (int i = 0; i < triangleCount; i++)
            {
                var triangle = triangles[i];
                if (!triangle.deleted)
                {
                    if (triangle.va0 != triangle.v0)
                    {
                        int iDest = triangle.va0;
                        int iSrc = triangle.v0;
                        vertices[iDest].p = vertices[iSrc].p;
                        triangle.v0 = triangle.va0;
                    }
                    if (triangle.va1 != triangle.v1)
                    {
                        int iDest = triangle.va1;
                        int iSrc = triangle.v1;
                        vertices[iDest].p = vertices[iSrc].p;
                        triangle.v1 = triangle.va1;
                    }
                    if (triangle.va2 != triangle.v2)
                    {
                        int iDest = triangle.va2;
                        int iSrc = triangle.v2;
                        vertices[iDest].p = vertices[iSrc].p;
                        triangle.v2 = triangle.va2;
                    }
                    int newTriangleIndex = dst++;
                    triangles[newTriangleIndex] = triangle;

                    vertices[triangle.v0].tcount = 1;
                    vertices[triangle.v1].tcount = 1;
                    vertices[triangle.v2].tcount = 1;

                    if (triangle.subMeshIndex > lastSubMeshIndex)
                    {
                        for (int j = lastSubMeshIndex + 1; j < triangle.subMeshIndex; j++)
                        {
                            subMeshOffsets[j] = newTriangleIndex;
                        }
                        subMeshOffsets[triangle.subMeshIndex] = newTriangleIndex;
                        lastSubMeshIndex = triangle.subMeshIndex;
                    }
                }
            }

            triangleCount = dst;
            for (int i = lastSubMeshIndex + 1; i < subMeshCount; i++)
            {
                subMeshOffsets[i] = triangleCount;
            }

            this.triangles.Resize(triangleCount);
            triangles = this.triangles.Data;

            dst = 0;
            for (int i = 0; i < vertexCount; i++)
            {
                var vert = vertices[i];
                if (vert.tcount > 0)
                {
                    vert.tstart = dst;
                    vertices[i] = vert;

                    if (dst != i)
                    {
                        vertices[dst].p = vert.p;
                        if (vertNormals != null) vertNormals[dst] = vertNormals[i];
                        if (vertTangents != null) vertTangents[dst] = vertTangents[i];
                        if (vertUV2D != null)
                        {
                            for (int j = 0; j < UVChannelCount; j++)
                            {
                                var vertUV = vertUV2D[j];
                                if (vertUV != null)
                                {
                                    vertUV[dst] = vertUV[i];
                                }
                            }
                        }
                        if (vertUV3D != null)
                        {
                            for (int j = 0; j < UVChannelCount; j++)
                            {
                                var vertUV = vertUV3D[j];
                                if (vertUV != null)
                                {
                                    vertUV[dst] = vertUV[i];
                                }
                            }
                        }
                        if (vertUV4D != null)
                        {
                            for (int j = 0; j < UVChannelCount; j++)
                            {
                                var vertUV = vertUV4D[j];
                                if (vertUV != null)
                                {
                                    vertUV[dst] = vertUV[i];
                                }
                            }
                        }
                        if (vertColors != null) vertColors[dst] = vertColors[i];
                    }
                    ++dst;
                }
            }

            for (int i = 0; i < triangleCount; i++)
            {
                var triangle = triangles[i];
                triangle.v0 = vertices[triangle.v0].tstart;
                triangle.v1 = vertices[triangle.v1].tstart;
                triangle.v2 = vertices[triangle.v2].tstart;
                triangles[i] = triangle;
            }

            vertexCount = dst;
            this.vertices.Resize(vertexCount);
            if (vertNormals != null) this.vertNormals.Resize(vertexCount, true);
            if (vertTangents != null) this.vertTangents.Resize(vertexCount, true);
            if (vertUV2D != null) this.vertUV2D.Resize(vertexCount, true);
            if (vertUV3D != null) this.vertUV3D.Resize(vertexCount, true);
            if (vertUV4D != null) this.vertUV4D.Resize(vertexCount, true);
            if (vertColors != null) this.vertColors.Resize(vertexCount, true);
        }
        #endregion

        #region Calculate Sub Mesh Offsets
        private void CalculateSubMeshOffsets()
        {
            int lastSubMeshIndex = -1;
            subMeshOffsets = new int[subMeshCount];

            var triangles = this.triangles.Data;
            int triangleCount = this.triangles.Length;
            for (int i = 0; i < triangleCount; i++)
            {
                var triangle = triangles[i];
                if (triangle.subMeshIndex > lastSubMeshIndex)
                {
                    for (int j = lastSubMeshIndex + 1; j < triangle.subMeshIndex; j++)
                    {
                        subMeshOffsets[j] = i;
                    }
                    subMeshOffsets[triangle.subMeshIndex] = i;
                    lastSubMeshIndex = triangle.subMeshIndex;
                }
            }

            for (int i = lastSubMeshIndex + 1; i < subMeshCount; i++)
            {
                subMeshOffsets[i] = triangleCount;
            }
        }
        #endregion
        #endregion

        #region Public Methods
        #region Sub-Meshes
        /// <summary>
        /// Returns the triangle indices for all sub-meshes.
        /// </summary>
        /// <returns>The triangle indices for all sub-meshes.</returns>
        public int[][] GetAllSubMeshTriangles()
        {
            var indices = new int[subMeshCount][];
            for (int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
            {
                indices[subMeshIndex] = GetSubMeshTriangles(subMeshIndex);
            }
            return indices;
        }

        /// <summary>
        /// Returns the triangle indices for a specific sub-mesh.
        /// </summary>
        /// <param name="subMeshIndex">The sub-mesh index.</param>
        /// <returns>The triangle indices.</returns>
        public int[] GetSubMeshTriangles(int subMeshIndex)
        {
            if (subMeshIndex < 0)
                throw new ArgumentOutOfRangeException("The sub-mesh index is negative.");

            // First get the sub-mesh offsets
            if (subMeshOffsets == null)
            {
                CalculateSubMeshOffsets();
            }

            if (subMeshIndex >= subMeshOffsets.Length)
                throw new ArgumentOutOfRangeException("The sub-mesh index is greater than or equals to the sub mesh count.");
            else if (subMeshOffsets.Length != subMeshCount)
                throw new InvalidOperationException("The sub-mesh triangle offsets array is not the same size as the count of sub-meshes. This should not be possible to happen.");

            var triangles = this.triangles.Data;
            int triangleCount = this.triangles.Length;

            int startOffset = subMeshOffsets[subMeshIndex];
            if (startOffset >= triangleCount)
                return new int[0];

            int endOffset = ((subMeshIndex + 1) < subMeshCount ? subMeshOffsets[subMeshIndex + 1] : triangleCount);
            int subMeshTriangleCount = endOffset - startOffset;
            if (subMeshTriangleCount < 0) subMeshTriangleCount = 0;
            int[] subMeshIndices = new int[subMeshTriangleCount * 3];

            Debug.AssertFormat(startOffset >= 0, "The start sub mesh offset at index {0} was below zero ({1}).", subMeshIndex, startOffset);
            Debug.AssertFormat(endOffset >= 0, "The end sub mesh offset at index {0} was below zero ({1}).", subMeshIndex + 1, endOffset);
            Debug.AssertFormat(startOffset < triangleCount, "The start sub mesh offset at index {0} was higher or equal to the triangle count ({1} >= {2}).", subMeshIndex, startOffset, triangleCount);
            Debug.AssertFormat(endOffset <= triangleCount, "The end sub mesh offset at index {0} was higher than the triangle count ({1} > {2}).", subMeshIndex + 1, endOffset, triangleCount);

            for (int triangleIndex = startOffset; triangleIndex < endOffset; triangleIndex++)
            {
                var triangle = triangles[triangleIndex];
                int offset = (triangleIndex - startOffset) * 3;
                subMeshIndices[offset] = triangle.v0;
                subMeshIndices[offset + 1] = triangle.v1;
                subMeshIndices[offset + 2] = triangle.v2;
            }

            return subMeshIndices;
        }

        /// <summary>
        /// Clears out all sub-meshes.
        /// </summary>
        public void ClearSubMeshes()
        {
            subMeshCount = 0;
            subMeshOffsets = null;
            triangles.Resize(0);
        }

        /// <summary>
        /// Adds a sub-mesh triangle indices for a specific sub-mesh.
        /// </summary>
        /// <param name="triangles">The triangle indices.</param>
        public void AddSubMeshTriangles(int[] triangles)
        {
            if (triangles == null)
                throw new ArgumentNullException();
            else if ((triangles.Length % 3) != 0)
                throw new ArgumentException("The index array length must be a multiple of 3 in order to represent triangles.");

            int subMeshIndex = subMeshCount++;
            int triangleIndex = this.triangles.Length;
            int subMeshTriangleCount = triangles.Length / 3;
            this.triangles.Resize(this.triangles.Length + subMeshTriangleCount);
            var trisArr = this.triangles.Data;
            for (int i = 0; i < subMeshTriangleCount; i++)
            {
                int offset = i * 3;
                int v0 = triangles[offset];
                int v1 = triangles[offset + 1];
                int v2 = triangles[offset + 2];
                trisArr[triangleIndex + i] = new Triangle(v0, v1, v2, subMeshIndex);
            }

            triangleIndex += subMeshTriangleCount;
        }

        /// <summary>
        /// Adds several sub-meshes at once with their triangle indices for each sub-mesh.
        /// </summary>
        /// <param name="triangles">The triangle indices for each sub-mesh.</param>
        public void AddSubMeshTriangles(int[][] triangles)
        {
            if (triangles == null)
                throw new ArgumentNullException();

            int totalTriangleCount = 0;
            for (int i = 0; i < triangles.Length; i++)
            {
                if (triangles[i] == null)
                    throw new ArgumentException(string.Format("The index array at index {0} is null.", i));
                else if ((triangles[i].Length % 3) != 0)
                    throw new ArgumentException(string.Format("The index array length at index {0} must be a multiple of 3 in order to represent triangles.", i));

                totalTriangleCount += triangles[i].Length / 3;
            }

            int triangleIndex = this.triangles.Length;
            this.triangles.Resize(this.triangles.Length + totalTriangleCount);
            var trisArr = this.triangles.Data;

            for (int i = 0; i < triangles.Length; i++)
            {
                int subMeshIndex = subMeshCount++;
                var subMeshTriangles = triangles[i];
                int subMeshTriangleCount = subMeshTriangles.Length / 3;
                for (int j = 0; j < subMeshTriangleCount; j++)
                {
                    int offset = j * 3;
                    int v0 = subMeshTriangles[offset];
                    int v1 = subMeshTriangles[offset + 1];
                    int v2 = subMeshTriangles[offset + 2];
                    trisArr[triangleIndex + j] = new Triangle(v0, v1, v2, subMeshIndex);
                }

                triangleIndex += subMeshTriangleCount;
            }
        }
        #endregion

        #region UV Sets
        #region Getting
        /// <summary>
        /// Returns the UVs (2D) from a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <returns>The UVs.</returns>
        public Vector2[] GetUVs2D(int channel)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException();

            if (vertUV2D != null && vertUV2D[channel] != null)
            {
                return vertUV2D[channel].Data;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the UVs (3D) from a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <returns>The UVs.</returns>
        public Vector3[] GetUVs3D(int channel)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException();

            if (vertUV3D != null && vertUV3D[channel] != null)
            {
                return vertUV3D[channel].Data;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the UVs (4D) from a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <returns>The UVs.</returns>
        public Vector4[] GetUVs4D(int channel)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException();

            if (vertUV4D != null && vertUV4D[channel] != null)
            {
                return vertUV4D[channel].Data;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the UVs (2D) from a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <param name="uvs">The UVs.</param>
        public void GetUVs(int channel, List<Vector2> uvs)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException();
            else if (uvs == null)
                throw new ArgumentNullException();

            uvs.Clear();
            if (vertUV2D != null && vertUV2D[channel] != null)
            {
                var uvData = vertUV2D[channel].Data;
                if (uvData != null)
                {
                    uvs.AddRange(uvData);
                }
            }
        }

        /// <summary>
        /// Returns the UVs (3D) from a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <param name="uvs">The UVs.</param>
        public void GetUVs(int channel, List<Vector3> uvs)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException();
            else if (uvs == null)
                throw new ArgumentNullException();

            uvs.Clear();
            if (vertUV3D != null && vertUV3D[channel] != null)
            {
                var uvData = vertUV3D[channel].Data;
                if (uvData != null)
                {
                    uvs.AddRange(uvData);
                }
            }
        }

        /// <summary>
        /// Returns the UVs (4D) from a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <param name="uvs">The UVs.</param>
        public void GetUVs(int channel, List<Vector4> uvs)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException();
            else if (uvs == null)
                throw new ArgumentNullException();

            uvs.Clear();
            if (vertUV4D != null && vertUV4D[channel] != null)
            {
                var uvData = vertUV4D[channel].Data;
                if (uvData != null)
                {
                    uvs.AddRange(uvData);
                }
            }
        }
        #endregion

        #region Setting
        /// <summary>
        /// Sets the UVs (2D) for a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <param name="uvs">The UVs.</param>
        public void SetUVs(int channel, Vector2[] uvs)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException();

            if (uvs != null && uvs.Length > 0)
            {
                if (vertUV2D == null)
                    vertUV2D = new UVChannels<Vector2>();

                int uvCount = uvs.Length;
                var uvSet = vertUV2D[channel];
                if (uvSet != null)
                {
                    uvSet.Resize(uvCount);
                }
                else
                {
                    uvSet = new ResizableArray<Vector2>(uvCount, uvCount);
                    vertUV2D[channel] = uvSet;
                }

                var uvData = uvSet.Data;
                uvs.CopyTo(uvData, 0);
            }
            else
            {
                if (vertUV2D != null)
                {
                    vertUV2D[channel] = null;
                }
            }

            if (vertUV3D != null)
            {
                vertUV3D[channel] = null;
            }
            if (vertUV4D != null)
            {
                vertUV4D[channel] = null;
            }
        }

        /// <summary>
        /// Sets the UVs (3D) for a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <param name="uvs">The UVs.</param>
        public void SetUVs(int channel, Vector3[] uvs)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException();

            if (uvs != null && uvs.Length > 0)
            {
                if (vertUV3D == null)
                    vertUV3D = new UVChannels<Vector3>();

                int uvCount = uvs.Length;
                var uvSet = vertUV3D[channel];
                if (uvSet != null)
                {
                    uvSet.Resize(uvCount);
                }
                else
                {
                    uvSet = new ResizableArray<Vector3>(uvCount, uvCount);
                    vertUV3D[channel] = uvSet;
                }

                var uvData = uvSet.Data;
                uvs.CopyTo(uvData, 0);
            }
            else
            {
                if (vertUV3D != null)
                {
                    vertUV3D[channel] = null;
                }
            }

            if (vertUV2D != null)
            {
                vertUV2D[channel] = null;
            }
            if (vertUV4D != null)
            {
                vertUV4D[channel] = null;
            }
        }

        /// <summary>
        /// Sets the UVs (4D) for a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <param name="uvs">The UVs.</param>
        public void SetUVs(int channel, Vector4[] uvs)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException();

            if (uvs != null && uvs.Length > 0)
            {
                if (vertUV4D == null)
                    vertUV4D = new UVChannels<Vector4>();

                int uvCount = uvs.Length;
                var uvSet = vertUV4D[channel];
                if (uvSet != null)
                {
                    uvSet.Resize(uvCount);
                }
                else
                {
                    uvSet = new ResizableArray<Vector4>(uvCount, uvCount);
                    vertUV4D[channel] = uvSet;
                }

                var uvData = uvSet.Data;
                uvs.CopyTo(uvData, 0);
            }
            else
            {
                if (vertUV4D != null)
                {
                    vertUV4D[channel] = null;
                }
            }

            if (vertUV2D != null)
            {
                vertUV2D[channel] = null;
            }
            if (vertUV3D != null)
            {
                vertUV3D[channel] = null;
            }
        }

        /// <summary>
        /// Sets the UVs (2D) for a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <param name="uvs">The UVs.</param>
        public void SetUVs(int channel, List<Vector2> uvs)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException();

            if (uvs != null && uvs.Count > 0)
            {
                if (vertUV2D == null)
                    vertUV2D = new UVChannels<Vector2>();

                int uvCount = uvs.Count;
                var uvSet = vertUV2D[channel];
                if (uvSet != null)
                {
                    uvSet.Resize(uvCount);
                }
                else
                {
                    uvSet = new ResizableArray<Vector2>(uvCount, uvCount);
                    vertUV2D[channel] = uvSet;
                }

                var uvData = uvSet.Data;
                uvs.CopyTo(uvData, 0);
            }
            else
            {
                if (vertUV2D != null)
                {
                    vertUV2D[channel] = null;
                }
            }

            if (vertUV3D != null)
            {
                vertUV3D[channel] = null;
            }
            if (vertUV4D != null)
            {
                vertUV4D[channel] = null;
            }
        }

        /// <summary>
        /// Sets the UVs (3D) for a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <param name="uvs">The UVs.</param>
        public void SetUVs(int channel, List<Vector3> uvs)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException();

            if (uvs != null && uvs.Count > 0)
            {
                if (vertUV3D == null)
                    vertUV3D = new UVChannels<Vector3>();

                int uvCount = uvs.Count;
                var uvSet = vertUV3D[channel];
                if (uvSet != null)
                {
                    uvSet.Resize(uvCount);
                }
                else
                {
                    uvSet = new ResizableArray<Vector3>(uvCount, uvCount);
                    vertUV3D[channel] = uvSet;
                }

                var uvData = uvSet.Data;
                uvs.CopyTo(uvData, 0);
            }
            else
            {
                if (vertUV3D != null)
                {
                    vertUV3D[channel] = null;
                }
            }

            if (vertUV2D != null)
            {
                vertUV2D[channel] = null;
            }
            if (vertUV4D != null)
            {
                vertUV4D[channel] = null;
            }
        }

        /// <summary>
        /// Sets the UVs (4D) for a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <param name="uvs">The UVs.</param>
        public void SetUVs(int channel, List<Vector4> uvs)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException();

            if (uvs != null && uvs.Count > 0)
            {
                if (vertUV4D == null)
                    vertUV4D = new UVChannels<Vector4>();

                int uvCount = uvs.Count;
                var uvSet = vertUV4D[channel];
                if (uvSet != null)
                {
                    uvSet.Resize(uvCount);
                }
                else
                {
                    uvSet = new ResizableArray<Vector4>(uvCount, uvCount);
                    vertUV4D[channel] = uvSet;
                }

                var uvData = uvSet.Data;
                uvs.CopyTo(uvData, 0);
            }
            else
            {
                if (vertUV4D != null)
                {
                    vertUV4D[channel] = null;
                }
            }

            if (vertUV2D != null)
            {
                vertUV2D[channel] = null;
            }
            if (vertUV3D != null)
            {
                vertUV3D[channel] = null;
            }
        }

        /// <summary>
        /// Sets the UVs for a specific channel and automatically detects the used components.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <param name="uvs">The UVs.</param>
        public void SetUVsAuto(int channel, List<Vector4> uvs)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException();

            if (uvs != null && uvs.Count > 0)
            {
                int usedComponents = MeshUtils.GetUsedUVComponents(uvs);
                if (usedComponents <= 2)
                {
                    var uv2D = MeshUtils.ConvertUVsTo2D(uvs);
                    SetUVs(channel, uv2D);
                }
                else if (usedComponents == 3)
                {
                    var uv3D = MeshUtils.ConvertUVsTo3D(uvs);
                    SetUVs(channel, uv3D);
                }
                else
                {
                    SetUVs(channel, uvs);
                }
            }
            else
            {
                if (vertUV2D != null)
                {
                    vertUV2D[channel] = null;
                }
                if (vertUV3D != null)
                {
                    vertUV3D[channel] = null;
                }
                if (vertUV4D != null)
                {
                    vertUV4D[channel] = null;
                }
            }
        }
        #endregion
        #endregion

        #region Initialize
        /// <summary>
        /// Initializes the algorithm with the original mesh.
        /// </summary>
        /// <param name="mesh">The mesh.</param>
        public void Initialize(Mesh mesh)
        {
            if (mesh == null)
                throw new ArgumentNullException();

            this.Vertices = mesh.vertices;
            this.Normals = mesh.normals;
            this.Tangents = mesh.tangents;

            this.Colors = mesh.colors;

            for (int channel = 0; channel < UVChannelCount; channel++)
            {
                var uvs = MeshUtils.GetMeshUVs(mesh, channel);
                SetUVsAuto(channel, uvs);
            }

            ClearSubMeshes();

            int subMeshCount = mesh.subMeshCount;
            var subMeshTriangles = new int[subMeshCount][];
            for (int i = 0; i < subMeshCount; i++)
            {
                subMeshTriangles[i] = mesh.GetTriangles(i);
            }
            AddSubMeshTriangles(subMeshTriangles);
        }
        #endregion

        #region Simplify Mesh

        public delegate void DelProgress(float pos);
        /// <summary>
        /// Simplifies the mesh to a desired quality.
        /// </summary>
        /// <param name="quality">The target quality (between 0 and 1).</param>
        public void SimplifyMesh(float quality, DelProgress func=null)
        {
            quality = Mathf.Clamp01(quality);

            int deletedTris = 0;
            ResizableArray<bool> deleted0 = new ResizableArray<bool>(20);
            ResizableArray<bool> deleted1 = new ResizableArray<bool>(20);
            var triangles = this.triangles.Data;
            int triangleCount = this.triangles.Length;
            int startTrisCount = triangleCount;
            var vertices = this.vertices.Data;
            int targetTrisCount = Mathf.RoundToInt(triangleCount * quality);

            for (int iteration = 0; iteration < maxIterationCount; iteration++)
            {
                if (func != null)
                    func((float)iteration / maxIterationCount);

                if ((startTrisCount - deletedTris) <= targetTrisCount)
                    break;

                // Update mesh once in a while
                if ((iteration % 5) == 0)
                {
                    UpdateMesh(iteration);
                    triangles = this.triangles.Data;
                    triangleCount = this.triangles.Length;
                    vertices = this.vertices.Data;
                }

                // Clear dirty flag
                for (int i = 0; i < triangleCount; i++)
                {
                    triangles[i].dirty = false;
                }

                // All triangles with edges below the threshold will be removed
                //
                // The following numbers works well for most models.
                // If it does not, try to adjust the 3 parameters
                double threshold = 0.000000001 * Math.Pow(iteration + 3, agressiveness);

                if (verbose)
                {
                    Debug.LogFormat("iteration {0} - triangles {1} threshold {2}", iteration, (startTrisCount - deletedTris), threshold);
                }

                // Remove vertices & mark deleted triangles
                RemoveVertexPass(startTrisCount, targetTrisCount, threshold, deleted0, deleted1, ref deletedTris);
            }

            CompactMesh();

            if (verbose)
            {
                Debug.LogFormat("Finished simplification with triangle count {0}", this.triangles.Length);
            }

            if (func != null)
                func(1.0f);
        }

        /// <summary>
        /// Simplifies the mesh without losing too much quality.
        /// </summary>
        public void SimplifyMeshLossless()
        {
            int deletedTris = 0;
            ResizableArray<bool> deleted0 = new ResizableArray<bool>(0);
            ResizableArray<bool> deleted1 = new ResizableArray<bool>(0);
            var triangles = this.triangles.Data;
            int triangleCount = this.triangles.Length;
            int startTrisCount = triangleCount;
            var vertices = this.vertices.Data;

            for (int iteration = 0; iteration < 9999; iteration++)
            {
                // Update mesh constantly
                UpdateMesh(iteration);
                triangles = this.triangles.Data;
                triangleCount = this.triangles.Length;
                vertices = this.vertices.Data;

                // Clear dirty flag
                for (int i = 0; i < triangleCount; i++)
                {
                    triangles[i].dirty = false;
                }

                // All triangles with edges below the threshold will be removed
                //
                // The following numbers works well for most models.
                // If it does not, try to adjust the 3 parameters
                double threshold = DoubleEpsilon;

                if (verbose)
                {
                    Debug.LogFormat("Lossless iteration {0} - triangles {1}", iteration, triangleCount);
                }

                // Remove vertices & mark deleted triangles
                RemoveVertexPass(startTrisCount, 0, threshold, deleted0, deleted1, ref deletedTris);

                if (deletedTris <= 0)
                    break;

                deletedTris = 0;
            }

            CompactMesh();

            if (verbose)
            {
                Debug.LogFormat("Finished simplification with triangle count {0}", this.triangles.Length);
            }
        }
        #endregion

        #region To Mesh
        /// <summary>
        /// Returns the resulting mesh.
        /// </summary>
        /// <returns>The resulting mesh.</returns>
        public Mesh ToMesh()
        {
            var vertices = this.Vertices;
            var normals = this.Normals;
            var tangents = this.Tangents;
            var colors = this.Colors;
            var indices = GetAllSubMeshTriangles();

            List<Vector2>[] uvs2D = null;
            List<Vector3>[] uvs3D = null;
            List<Vector4>[] uvs4D = null;
            if (vertUV2D != null)
            {
                uvs2D = new List<Vector2>[UVChannelCount];
                for (int channel = 0; channel < UVChannelCount; channel++)
                {
                    if (vertUV2D[channel] != null)
                    {
                        var uvs = new List<Vector2>(vertices.Length);
                        GetUVs(channel, uvs);
                        uvs2D[channel] = uvs;
                    }
                }
            }

            if (vertUV3D != null)
            {
                uvs3D = new List<Vector3>[UVChannelCount];
                for (int channel = 0; channel < UVChannelCount; channel++)
                {
                    if (vertUV3D[channel] != null)
                    {
                        var uvs = new List<Vector3>(vertices.Length);
                        GetUVs(channel, uvs);
                        uvs3D[channel] = uvs;
                    }
                }
            }

            if (vertUV4D != null)
            {
                uvs4D = new List<Vector4>[UVChannelCount];
                for (int channel = 0; channel < UVChannelCount; channel++)
                {
                    if (vertUV4D[channel] != null)
                    {
                        var uvs = new List<Vector4>(vertices.Length);
                        GetUVs(channel, uvs);
                        uvs4D[channel] = uvs;
                    }
                }
            }

            return MeshUtils.CreateMesh(vertices, indices, normals, tangents, colors, uvs2D, uvs3D, uvs4D);
        }
        #endregion
        #endregion
    }
}