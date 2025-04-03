using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Use RayFire libs only in editor and standalone
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
using System.Resources;
using RayFire.DotNet;

// Rayfire classes
namespace RayFire
{
    // Static class to handle all shatter methods
    public static class RFFragment
    {
        /// /////////////////////////////////////////////////////////
        /// Shatter
        /// /////////////////////////////////////////////////////////

        // Cache for shatter
        public static void CacheMeshes(ref Mesh[] meshes, ref Vector3[] pivots, ref List<RFDictionary> origSubMeshIdsRf, RayfireShatter scrShatter)
        {
            // TODO check vars by type: slice list, etc
            
            // Turn off fast mode for tets and slices
            int shatterMode = GetShatterMode(scrShatter);
         
            // Get mesh
            Mesh mesh = scrShatter.skinnedMeshRend != null 
                ? RFMesh.BakeMesh(scrShatter.skinnedMeshRend) 
                : scrShatter.meshFilter.sharedMesh;
            
            // Set up shatter
            RFShatter shatter = SetFragmentCommon(
                shatterMode, 
                mesh, 
                scrShatter.transform, 
                scrShatter.interior, 
                scrShatter.decompose, 
                scrShatter.removeCollinear, 
                scrShatter.seed, 
                scrShatter.mode, 
                scrShatter.excludeInnerFragments);
            
            // Failed input
            if (shatter == null)
                return;
            
            // Get innerSubId
            Material[] mats = scrShatter.skinnedMeshRend != null 
                ? scrShatter.skinnedMeshRend.sharedMaterials 
                : scrShatter.meshRenderer.sharedMaterials;
            int innerSubId = RFSurface.InnerSubId(scrShatter.interior, mats);
            
            // Set fragmentation properties
            SetFragmentProperties (shatter, scrShatter, null);
            
            // Calculate fragments
            List<Dictionary<int, int>> origSubMeshIds = new List<Dictionary<int, int>>();
            bool successState = Compute(
                shatterMode, 
                shatter, 
                scrShatter.transform, 
                ref meshes, 
                ref pivots, 
                mesh, 
                innerSubId, 
                ref origSubMeshIds, 
                scrShatter);
            
            // Create RF dictionary
            origSubMeshIdsRf = new List<RFDictionary>();
            foreach (var dictionary in origSubMeshIds)
                origSubMeshIdsRf.Add(new RFDictionary(dictionary));
            
            // Failed fragmentation. Increase bad mesh 
            if (successState == false)
                Debug.Log("Bad shatter output mesh: " + scrShatter.name);
            else
                for (int i = 0; i < meshes.Length; i++)
                    meshes[i].name = scrShatter.name + "_" + i;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Rigid
        /// /////////////////////////////////////////////////////////
        
        // Prepare rigid component to cache fragment meshes
        public static bool PrepareCacheMeshes(RayfireRigid scr)
        {
            // Save rotation at caching to fix fragments rotation at demolition
            scr.cacheRotation = scr.transForm.rotation;
            
            // Turn off fast mode for tets and slices
            scr.meshDemolition.shatterMode = GetShatterMode(scr.meshDemolition.scrShatter);

            // Get innerSubId
            scr.meshDemolition.mesh = scr.skinnedMeshRend != null 
                ? RFMesh.BakeMesh(scr.skinnedMeshRend) 
                : scr.meshFilter.sharedMesh;
            
            // Set up shatter
            scr.meshDemolition.rfShatter = SetFragmentCommon(
                scr.meshDemolition.shatterMode, 
                scr.meshDemolition.mesh, 
                scr.transform, 
                scr.materials, 
                scr.meshDemolition.properties.decompose, 
                scr.meshDemolition.properties.removeCollinear, 
                scr.meshDemolition.seed);

            // Failed input. Instant bad mesh.
            if (scr.meshDemolition.rfShatter == null)
            {
                scr.meshDemolition.badMesh += 10;
                return false;
            }

            // Get innerSubId
            Material[] mats = scr.skinnedMeshRend != null 
                ? scr.skinnedMeshRend.sharedMaterials 
                : scr.meshRenderer.sharedMaterials;
            scr.meshDemolition.innerSubId = RFSurface.InnerSubId(scr.materials, mats);
            
            // Set fragmentation properties
            SetFragmentProperties (scr.meshDemolition.rfShatter, scr.meshDemolition.scrShatter, scr);

            return true;
        }
        
        // Cache for rigid
        public static void CacheMeshesInst(ref Mesh[] meshes, ref Vector3[] pivots, ref List<RFDictionary> origSubMeshIdsRf, RayfireRigid scrRigid)
        {
            // Local data lists
            List<Dictionary<int, int>> origSubMeshIds = new List<Dictionary<int, int>>();
            
            // Calculate fragments
            bool successState = Compute(
                scrRigid.meshDemolition.shatterMode, 
                scrRigid.meshDemolition.rfShatter, 
                scrRigid.transform, 
                ref meshes, 
                ref pivots, 
                scrRigid.meshDemolition.mesh, 
                scrRigid.meshDemolition.innerSubId, 
                ref origSubMeshIds, 
                scrRigid);

            // Create RF dictionary
            origSubMeshIdsRf.Clear();
            foreach (var dictionary in origSubMeshIds)
                origSubMeshIdsRf.Add(new RFDictionary(dictionary));
            
            // Final ops
            FinalCacheMeshes (ref meshes, scrRigid, successState);
        }
        
        // Cache for rigid
        public static void CacheMeshesMult(Transform tmSaved, ref List<Mesh> meshesList, ref List<Vector3> pivotsList, ref List<RFDictionary> subList, RayfireRigid scrRigid, List<int> batchAmount, int batchInd)
        {
            // Get list of meshes to calc
            List<int> markedElements = RFRuntimeCaching.GetMarkedElements (batchInd, batchAmount);
            
            // Local iteration data lists
            Mesh[]    meshesLocal = new Mesh[batchAmount.Count];
            Vector3[] pivotsLocal = new Vector3[batchAmount.Count];
            List<Dictionary<int, int>> origSubMeshIds = new List<Dictionary<int, int>>();
            
            // Compute
            bool state = scrRigid.meshDemolition.rfShatter.SimpleCompute(
                tmSaved, 
                ref meshesLocal, 
                ref pivotsLocal, 
                scrRigid.meshDemolition.mesh, 
                scrRigid.meshDemolition.innerSubId, 
                ref origSubMeshIds, 
                markedElements, 
                batchInd == 0);
            
            // Set names
            if (state == false || meshesLocal == null || meshesLocal.Length == 0)
                return;

            // Set names
            for (int i = 0; i < meshesLocal.Length; i++)
            {
                meshesLocal[i].RecalculateTangents();
                meshesLocal[i].name = scrRigid.name + "_" + markedElements[i].ToString();
            }

            // Add data to main lists
            foreach (var dictionary in origSubMeshIds)
                subList.Add(new RFDictionary(dictionary));
            meshesList.AddRange (meshesLocal);
            pivotsList.AddRange (pivotsLocal);
        }
        
        // Final step Cache for rigid
        static void FinalCacheMeshes (ref Mesh[] meshes, RayfireRigid scrRigid, bool successState)
        {
            // Failed fragmentation. Increase bad mesh 
            if (successState == false)
            {
                scrRigid.meshDemolition.badMesh++;
                Debug.Log("Bad mesh: " + scrRigid.name);
            }
            else
                for (int i = 0; i < meshes.Length; i++)
                    meshes[i].name = scrRigid.name + "_" + i;
        }

        /// /////////////////////////////////////////////////////////
        /// Slice
        /// /////////////////////////////////////////////////////////
        
        // Cache for slice
        public static void SliceMeshes(ref Mesh[] meshes, ref Vector3[] pivots, ref List<RFDictionary> origSubMeshIdsRf, RayfireRigid scrRigid, List<Vector3> sliceData)
        {
            // Get mesh
            Mesh mesh = scrRigid.skinnedMeshRend != null 
                ? RFMesh.BakeMesh(scrRigid.skinnedMeshRend) 
                : scrRigid.meshFilter.sharedMesh;
            
            // Set up shatter
            RFShatter shatter = SetFragmentCommon(
                2, 
                mesh, 
                scrRigid.transform, 
                scrRigid.materials, 
                true, 
                scrRigid.meshDemolition.properties.removeCollinear, 
                scrRigid.meshDemolition.seed, 
                FragmentMode.Runtime, 
                false);

            // Failed input
            if (shatter == null)
            {
                scrRigid.meshDemolition.badMesh++;
                return;
            }

            // Get innerSubId
            Material[] mats = scrRigid.skinnedMeshRend != null 
                ? scrRigid.skinnedMeshRend.sharedMaterials 
                : scrRigid.meshRenderer.sharedMaterials;
            int innerSubId = RFSurface.InnerSubId(scrRigid.materials, mats);
            
            // Get slice data
            List<Vector3> points = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            for (int i = 0; i < sliceData.Count; i++)
            {
                points.Add(sliceData[i]);
                norms.Add(sliceData[i+1]);
                i++;
            }
            
            // Set params
            shatter.SetBricksParams(points.ToArray(), norms.ToArray(), scrRigid.transform);
            
            // Calculate fragments
            List<Dictionary<int, int>> origSubMeshIds = new List<Dictionary<int, int>>();
            bool successState = Compute(
                2, 
                shatter, 
                scrRigid.transform, 
                ref meshes, 
                ref pivots, 
                mesh, 
                innerSubId, 
                ref origSubMeshIds, 
                scrRigid.gameObject);
            
            // Create RF dictionary
            origSubMeshIdsRf = new List<RFDictionary>();
            foreach (var dictionary in origSubMeshIds)
                origSubMeshIdsRf.Add(new RFDictionary(dictionary));
            
            // Failed fragmentation. Increase bad mesh 
            if (successState == false)
            {
                scrRigid.meshDemolition.badMesh++;
                Debug.Log("Bad mesh: " + scrRigid.name, scrRigid.gameObject);
            }
            else
                for (int i = 0; i < meshes.Length; i++)
                    meshes[i].name = scrRigid.name + "_" + i;
        }

        /// /////////////////////////////////////////////////////////
        /// Compute
        /// /////////////////////////////////////////////////////////
        
        // Compute
        static bool Compute(int shatterMode, RFShatter shatter, Transform tm, ref Mesh[] meshes, ref Vector3[] pivots, 
            Mesh mesh, int innerSubId, ref List<Dictionary<int, int>> subIds, Object obj, List<int> markedElements = null)
        {
            
            //Debug.Log ();
            
            // Compute fragments
            bool state = shatterMode == 0 
                ? shatter.Compute(tm, ref meshes, ref pivots, mesh, innerSubId, ref subIds) 
                : shatter.SimpleCompute(tm, ref meshes, ref pivots, mesh, innerSubId, ref subIds, markedElements);

            // Failed fragmentation
            if (state == false)
            {
                meshes = null;
                pivots = null;
                return false;
            }
            
            // Null check
            if (meshes == null)
            {
                Debug.Log("Null mesh warning", obj);
                meshes = null;
                pivots = null;
                return false;
            }

            // Empty mesh fix
            if (EmptyMeshState(meshes) == true)
            {
                List<Mesh> meshList = new List<Mesh>();
                List<Vector3> pivotList = new List<Vector3>();
                for (int i = 0; i < meshes.Length; i++)
                {
                    if (meshes[i].vertexCount > 3)
                    {
                        meshList.Add(meshes[i]);
                        pivotList.Add(pivots[i]);
                    }
                }

                pivots = pivotList.ToArray();
                meshes = meshList.ToArray();
                Debug.Log("EmptyMeshState warning", obj);
            }
            
            // Single mesh after mesh fix check
            if (meshes.Length <= 1)
            {
                Debug.Log("Mesh amount warning " + meshes.Length, obj);
                meshes = null;
                pivots = null;
                return false;
            }

            // TODO set in library
            foreach (var m in meshes)
                m.RecalculateTangents();
            
            return true;
        }
        
        // Get shatter mode
        static int GetShatterMode(RayfireShatter scrShatter = null)
        {
            // Simple voronoi
            if (scrShatter == null)
                return 1;
            
            // Turn off fast mode for tests and radial
            int shatterMode = scrShatter.shatterMode;
            if (scrShatter.type == FragType.Slices) 
                shatterMode = 2;
            if (scrShatter.type == FragType.Tets) 
                shatterMode = 0;

            // Classic way for clustering. Not for slices
            if (shatterMode != 2 && scrShatter.gluing.enable == true)
                shatterMode = 0;
            
            return shatterMode;
        }

        // Check for at least one empty mesh in cached meshes
        static bool EmptyMeshState(Mesh[] meshes)
        {
            for (int i = 0; i < meshes.Length; i++)
                if (meshes[i].vertexCount < 4)
                    return true; 
            return false;
        }
        
         // Set fragmentation properties
        static void SetFragmentProperties(RFShatter shatter, RayfireShatter scrShatter, RayfireRigid scrRigid)
        {
            // Rigid demolition without shatter. Set and exit.
            if (scrRigid != null && scrShatter == null)
            {
                // Get final amount
                int percVar = Random.Range(0, scrRigid.meshDemolition.amount * scrRigid.meshDemolition.variation / 100);
                scrRigid.meshDemolition.totalAmount = scrRigid.meshDemolition.amount + percVar;
                
                // Set Voronoi Uniform properties
                SetVoronoi (shatter, scrRigid.meshDemolition.totalAmount, scrRigid.transform, scrRigid.contactPoint, scrRigid.meshDemolition.contactBias);
                return;
            }

            // Rigid demolition with shatter. 
            if (scrRigid != null && scrShatter != null)
            {
                // Set Contact point to shatter component
                scrShatter.centerPosition = scrRigid.transForm.InverseTransformPoint (scrRigid.contactPoint);
                
                // Set total amount by rigid component
                if (scrShatter.type == FragType.Voronoi)
                    scrRigid.meshDemolition.totalAmount = scrShatter.voronoi.Amount;
                else if (scrShatter.type == FragType.Splinters)
                    scrRigid.meshDemolition.totalAmount = scrShatter.splinters.Amount;
                else if (scrShatter.type == FragType.Slabs)
                    scrRigid.meshDemolition.totalAmount = scrShatter.slabs.Amount;
                else if (scrShatter.type == FragType.Radial)
                    scrRigid.meshDemolition.totalAmount = scrShatter.radial.rings * scrShatter.radial.rays;
            }
            
            // Shatter fragmentation
            if (scrShatter != null)
            {
                // Center position and direction
                Vector3 centerPos = scrShatter.transform.TransformPoint (scrShatter.centerPosition);

                // Set properties
                if (scrShatter.type == FragType.Voronoi)
                    SetVoronoi (shatter, scrShatter.voronoi.Amount, scrShatter.transform, centerPos, scrShatter.voronoi.centerBias);
                else if (scrShatter.type == FragType.Splinters)
                    SetSplinters (shatter, scrShatter.splinters, scrShatter.transform, centerPos, scrShatter.splinters.centerBias);
                else if (scrShatter.type == FragType.Slabs)
                    SetSlabs (shatter, scrShatter.slabs, scrShatter.transform, centerPos, scrShatter.splinters.centerBias);
                else if (scrShatter.type == FragType.Radial)
                    SetRadial (shatter, scrShatter.radial, scrShatter.transform, centerPos, scrShatter.centerDirection);
                else if (scrShatter.type == FragType.Slices)
                    SetSlices (shatter, scrShatter.transform, scrShatter.slice);
                else if (scrShatter.type == FragType.Tets)
                    SetTet (shatter, scrShatter.tets);
                else if (scrShatter.type == FragType.Decompose)
                    SetVoronoi (shatter, 1, scrShatter.transform, Vector3.zero, 0f);

                // Clustering
                if (scrShatter.gluing.enable == true)
                    SetGluing (shatter, scrShatter.gluing);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Properties setup
        /// /////////////////////////////////////////////////////////

        // Set common fragmentation properties
        static RFShatter SetFragmentCommon (int shatterMode, Mesh mesh, Transform transform, RFSurface interior, bool decompose, bool deleteCol, int seed = 1, FragmentMode mode = FragmentMode.Runtime, bool exInside = false)
        {
            // Creating shatter
            RFShatter shatter = new RFShatter((RFShatter.RFShatterMode)shatterMode, true);

            // Unsafe mode for Shatter component
            shatter.DisableSafeMode (mode == FragmentMode.Editor);
            
            // Safe/unsafe properties
            if (mode == FragmentMode.Editor)
            {
                shatter.SetGeneralParameter(RFShatter.GeneralParams.unsafe_pre_cap,                    false);
                shatter.SetGeneralParameter(RFShatter.GeneralParams.unsafe_separate_only,              false);
                shatter.SetGeneralParameter(RFShatter.GeneralParams.unsafe_elliminateCollinears_maxIterFuse, 150);

                // Size filtering TODO consider scale, put in UI
                float percSize = 3f;
                float sizeFilter = mesh.bounds.size.magnitude * percSize / 100f;
                shatter.SetGeneralParameter(RFShatter.GeneralParams.unsfafe_min_bbox_diag_size_filter, sizeFilter);
                
                shatter.SetGeneralParameter(RFShatter.GeneralParams.unsafe_exclude_inside, exInside);
            }
            else
            {
                shatter.SetGeneralParameter(RFShatter.GeneralParams.pre_shatter, true);
                shatter.SetGeneralParameter(RFShatter.GeneralParams.pre_cap,     true);
                shatter.SetGeneralParameter(RFShatter.GeneralParams.pre_weld,    true);
            }
            
            // Set properties
            shatter.SetFragmentParameter(RFShatter.FragmentParams.seed, seed);
            shatter.SetGeneralParameter(RFShatter.GeneralParams.pre_weld_threshold, 0.001f);
            shatter.SetGeneralParameter(RFShatter.GeneralParams.delete_collinear, deleteCol);
            
            // Other
            shatter.SetGeneralParameter(RFShatter.GeneralParams.maping_scale, interior.mappingScale);
            shatter.SetGeneralParameter(RFShatter.GeneralParams.restore_normals, true);

            // Detach by elements
            shatter.DecomposeResultMesh(decompose);
            
            // Setting shatter params
            bool inputState = shatter.SetInputMesh(transform, mesh);

            // Failed input
            if (inputState == false)
            {
                Debug.Log("Bad input mesh: " + transform.name, transform.gameObject);
                return null;
            }

            return shatter;
        }

        // Decompose to elements
        static void SetDecompose(RFShatter shatter)
        {
            shatter.DisableSafeMode (true);
            shatter.SetGeneralParameter(RFShatter.GeneralParams.unsafe_separate_only, true);
        }
        
        // Set Uniform
        static void SetVoronoi(RFShatter shatter, int numFragments, Transform tm, Vector3 centerPos, float centerBias)
        {
            // Get amount
            int amount = numFragments;
            if (amount < 1)
                amount = 1;
            if (amount > 20000)
                amount = 2;

            // Set properties
            shatter.SetFragmentParameter(RFShatter.FragmentParams.type, (int)RFShatter.FragmentType.voronoi);
            shatter.SetFragmentParameter(RFShatter.FragmentParams.voronoi_type, (int)RFShatter.VoronoiType.irregular);
            shatter.SetFragmentParameter(RFShatter.FragmentParams.voronoi_irr_num, amount);
            
            // Set bias to center
            if (centerBias > 0)
            {
                shatter.SetFragmentParameter(RFShatter.FragmentParams.voronoi_irr_bias, centerBias);
                shatter.SetCenterParameter(centerPos, tm, Vector3.forward);
            }
        }

        // Set Splinters
        static void SetSplinters(RFShatter shatter, RFSplinters splint, Transform tm, Vector3 centerPos, float centerBias)
        {
            // Set properties
            shatter.SetFragmentParameter(RFShatter.FragmentParams.type, (int)RFShatter.FragmentType.voronoi);
            shatter.SetFragmentParameter(RFShatter.FragmentParams.voronoi_type, (int)RFShatter.VoronoiType.irregular);
            shatter.SetFragmentParameter(RFShatter.FragmentParams.voronoi_irr_num, splint.Amount);

            // Set center
            shatter.SetFragmentParameter(RFShatter.FragmentParams.voronoi_irr_bias, centerBias);
            shatter.SetCenterParameter(centerPos, tm, Vector3.forward);

            // Get direction axis
            Vector3 stretchDir = DirectionAxis(splint.axis);
            Vector3 stretchVector = stretchDir * Mathf.Lerp(40f, 99f, splint.strength);
            shatter.SetPoint3Parameter((int)RFShatter.FragmentParams.stretching, stretchVector);
        }

        // Set Slabs
        static void SetSlabs(RFShatter shatter, RFSplinters slabs, Transform tm, Vector3 centerPos, float centerBias)
        {
            // Set properties
            shatter.SetFragmentParameter(RFShatter.FragmentParams.type, (int)RFShatter.FragmentType.voronoi);
            shatter.SetFragmentParameter(RFShatter.FragmentParams.voronoi_type, (int)RFShatter.VoronoiType.irregular);
            shatter.SetFragmentParameter(RFShatter.FragmentParams.voronoi_irr_num, slabs.Amount);

            // Set center
            shatter.SetFragmentParameter(RFShatter.FragmentParams.voronoi_irr_bias, centerBias);
            shatter.SetCenterParameter(centerPos, tm, Vector3.forward);
            
            // Get slab vector
            Vector3 stretchDir = DirectionAxis(slabs.axis);
            Vector3 slabVector = new Vector3();
            if (stretchDir.x <= 0) slabVector.x = 1f;
            if (stretchDir.x >= 1f) slabVector.x = 0;
            if (stretchDir.y <= 0) slabVector.y = 1f;
            if (stretchDir.y >= 1f) slabVector.y = 0;
            if (stretchDir.z <= 0) slabVector.z = 1f;
            if (stretchDir.z >= 1f) slabVector.z = 0;
            
            // Set stretch vector
            Vector3 stretchVector = slabVector * Mathf.Lerp(40f, 99f, slabs.strength);
            shatter.SetPoint3Parameter((int)RFShatter.FragmentParams.stretching, stretchVector);
        }

        // Set Radial
        static void SetRadial(RFShatter shatter, RFRadial radial, Transform tm, Vector3 centerPos, Quaternion centerDirection)
        {
            // Set radial properties
            shatter.SetFragmentParameter(RFShatter.FragmentParams.type, (int)RFShatter.FragmentType.voronoi);
            shatter.SetFragmentParameter(RFShatter.FragmentParams.voronoi_type, (int)RFShatter.VoronoiType.radial);
            shatter.SetFragmentParameter(RFShatter.FragmentParams.voronoi_rad_radius, radial.radius);
            shatter.SetFragmentParameter(RFShatter.FragmentParams.voronoi_rad_divergence, radial.divergence);
            shatter.SetFragmentParameter(RFShatter.FragmentParams.voronoi_rad_restrict, radial.restrictToPlane);
            shatter.SetFragmentParameter(RFShatter.FragmentParams.voronoi_rad_rings_count, radial.rings);
            shatter.SetFragmentParameter(RFShatter.FragmentParams.voronoi_rad_rings_focus, radial.focus);
            shatter.SetFragmentParameter(RFShatter.FragmentParams.voronoi_rad_rings_strenght, radial.focusStr);
            shatter.SetFragmentParameter(RFShatter.FragmentParams.voronoi_rad_rings_random, radial.randomRings);
            shatter.SetFragmentParameter(RFShatter.FragmentParams.voronoi_rad_rays_count, radial.rays);
            shatter.SetFragmentParameter(RFShatter.FragmentParams.voronoi_rad_rays_random, radial.randomRays);
            shatter.SetFragmentParameter(RFShatter.FragmentParams.voronoi_rad_rays_twist, radial.twist);

            // Get direction axis
            Vector3 directionAxis = DirectionAxis(radial.centerAxis);
            Vector3 centerRot = tm.rotation * centerDirection * directionAxis;
            shatter.SetCenterParameter(centerPos, tm, centerRot);
        }

        // Set custom point cloud
        static void SetCustom(RFShatter shatter, RFCustom custom, MeshFilter mf)
        {
            shatter.SetFragmentParameter(RFShatter.FragmentParams.type, (int)RFShatter.FragmentType.voronoi);
            shatter.SetFragmentParameter(RFShatter.FragmentParams.voronoi_type, (int)RFShatter.VoronoiType.custom);
            
            // Multiply bound by scale
            MeshRenderer mr = mf.gameObject.GetComponent<MeshRenderer>();
            Bounds meshBounds = mr.bounds;
            Debug.Log(meshBounds.extents);
           
            // Filter not inside
            List<Vector3> worldPoints = new List<Vector3> ();
            foreach (var point in custom.localPoints)
                if (meshBounds.Contains(point) == true)
                    worldPoints.Add(mf.transform.position + point);

            // TODO stop if no points
            if (worldPoints.Count <= 1)
               return;
            
            foreach (var point in worldPoints)
                Debug.Log( point);
            
            // Set points
            shatter.SetVoroCustomPoints(worldPoints.ToArray(), mf.transform);
        }

        // Set slicing objects
        static void SetSlices(RFShatter shatter, Transform tm, RFSlice slices)
        {
            // Filter 
            List<Transform> list = new List<Transform>();
            foreach (Transform slice in slices.sliceList)
                if (slice != null)
                    list.Add(slice);

            // No objects
            if (list.Count == 0)
                return;

            // Get slice data
            Vector3[] points = list.Select(t => t.position).ToArray();
            Vector3[] norms = list.Select(t => slices.Axis(t)).ToArray();

            // Set params
            shatter.SetBricksParams(points, norms, tm);
        }

        // Set Custom Voronoi properties
        static void SetTet(RFShatter shatter, RFTets tets)
        {
            Vector3 density = tets.density;
            if (density.x > 30)  density.x = 30;
            if (density.x < 1) density.x = 1;
            if (density.y > 30)  density.y = 30;
            if (density.y < 1) density.y = 1;
            if (density.z > 30)  density.z = 30;
            if (density.z < 1) density.z = 1;
            
            // TODO add average for size mode.
            // TODO calculate final density by tets.mult
            
            shatter.SetFragmentParameter(RFShatter.FragmentParams.type, (int)RFShatter.FragmentType.tetra);
            shatter.SetFragmentParameter(RFShatter.FragmentParams.tetra_noise, tets.noise);
            shatter.SetPoint3Parameter((int)RFShatter.FragmentParams.tetra1_density, density);
        }
        
        // Set gluing
        static void SetGluing(RFShatter shatter, RFGlue gluing)
        {
            shatter.InitClustering(true);
            shatter.SetClusterParameter(RFShatter.ClusterParams.enabled, true);
            shatter.SetClusterParameter(RFShatter.ClusterParams.by_pcloud_count, gluing.amount);
            shatter.SetClusterParameter(RFShatter.ClusterParams.options_seed, gluing.seed);
            
            // Glue props
            shatter.SetGeneralParameter(RFShatter.GeneralParams.glue, true);
            shatter.SetGeneralParameter(RFShatter.GeneralParams.glue_weld_threshold, 0.001f);
            shatter.SetGeneralParameter(RFShatter.GeneralParams.relax, gluing.relax);
        }

        // Get axis by type
        static Vector3 DirectionAxis(AxisType axisType)
        {
            if (axisType == AxisType.YGreen)
                return Vector3.up;
            if (axisType == AxisType.ZBlue)
                return Vector3.forward;
            return Vector3.right;
        }
    }
}

// Static dummy class for other platforms
#else 

namespace RayFire
{
    public static class RFFragment
    {
        public static bool PrepareCacheMeshes(RayfireRigid scrRigid) 
        {
            return false;
        }
        public static void CacheMeshesMult(Transform tmSaved, ref List<Mesh> meshesList, ref List<Vector3> pivotsList, ref List<RFDictionary> subList, RayfireRigid scrRigid, List<int> batchAmount, int batchInd) {}

        public static void CacheMeshesInst(ref Mesh[] meshes, ref Vector3[] pivots, ref List<RFDictionary> origSubMeshIdsRf, RayfireRigid scrRigid){}
        public static void CacheMeshes(ref Mesh[] meshes, ref Vector3[] pivots, ref List<RFDictionary> origSubMeshIdsRf, RayfireShatter scrShatter) {}
        public static void SliceMeshes(ref Mesh[] meshes, ref Vector3[] pivots, ref List<RFDictionary> origSubMeshIdsRf, RayfireRigid scrRigid, List<Vector3> sliceData) {}
    }

    public class RFShatter
    {
      
    }
}

#endif