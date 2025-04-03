using System.Collections.Generic;
using UnityEngine;

#if UNITY_2018
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
#endif

#if UNITY_2018_2_OR_NEWER
using UnityEngine.Rendering;
#else
using UnityEngine.Experimental.Rendering;
#endif

namespace ThreeEyedGames.Mercator
{
    [ExecuteInEditMode]
    [AddComponentMenu("Mercator/Terrain Master", 0)]
    public class MercatorTerrainMaster : MonoBehaviour
    {
        public Terrain Terrain;

        private List<MercatorTerrainElement> _affectors = new List<MercatorTerrainElement>();
        private RenderTexture _heightMap;
        private int _heightMapRes;
        private float[,] _heightData;

#if UNITY_2018
        private AsyncGPUReadbackRequest _readbackRequest;
        private GCHandle _heightGCHandle;
#else
        private Texture2D _heightMap2D;
#endif

        #region Public API
        /// <summary>
        /// Create a terrain stamp in the size of the full terrain using given height and mask texture.
        /// </summary>
        /// <param name="name">The name of the created GameObject.</param>
        /// <param name="heightTexture">The texture to be used as the height data source.</param>
        /// <param name="maskTexture">The texture to be used as mask.</param>
        /// <returns>The newly created MercatorTerraiStamp component.</returns>
        public MercatorTerrainStamp CreateTerrainStamp(string name, Texture2D heightTexture, Texture2D maskTexture)
        {
            if (Terrain != null)
            {
                TerrainData data = Terrain.terrainData;
                if (data != null)
                {
                    Vector3 position = Terrain.transform.position + data.size / 2.0f;
                    Quaternion rotation = Quaternion.identity;
                    Vector3 scale = data.size;
                    return CreateTerrainStamp(name, heightTexture, maskTexture, position, rotation, scale);
                }
            }

            return null;
        }

        /// <summary>
        /// Create a terrain stamp with desired transform using given height and mask texture.
        /// </summary>
        /// <param name="name">The name of the created GameObject.</param>
        /// <param name="heightTexture">The texture to be used as the height data source.</param>
        /// <param name="maskTexture">The texture to be used as mask.</param>
        /// <param name="position">The world-space position of the stamp.</param>
        /// <param name="rotation">The world-space rotation of the stamp.</param>
        /// <param name="scale">The world-space scale of the stamp.</param>
        /// <returns>The newly created MercatorTerraiStamp component.</returns>
        public MercatorTerrainStamp CreateTerrainStamp(string name, Texture2D heightTexture, Texture2D maskTexture,
            Vector3 position, Quaternion rotation, Vector3 scale)
        {
            // Create the stamp and wire up the required components
            GameObject go = new GameObject(name);
            MercatorTerrainStamp stamp = go.AddComponent<MercatorTerrainStamp>();
            MercatorTexture height = go.AddComponent<MercatorTexture>();
            height.Texture = heightTexture;
            MercatorTexture mask = go.AddComponent<MercatorTexture>();
            height.Texture = maskTexture;

            // Set the textures and values
            stamp.SetHeightTexture(height);
            stamp.SetMaskTexture(mask);

            Transform stampTransform = stamp.transform;
            stampTransform.localPosition = position;
            stampTransform.localRotation = rotation;
            stampTransform.localScale = scale;
            stampTransform.SetParent(transform, true);

            return stamp;
        }
        #endregion

        private void OnDisable()
        {
            Cleanup();
        }

        private void Update()
        {
            if (Terrain == null || Terrain.terrainData == null)
                return;

            UpdateRenderTexture();
            ClearTerrain();
            ApplyStamps();

#if UNITY_2018
            _readbackRequest = AsyncGPUReadback.Request(_heightMap);
            _readbackRequest.WaitForCompletion();

            if (!_readbackRequest.hasError)
            {
                unsafe
                {
                    // Get pointer to native array
                    var src = _readbackRequest.GetData<float>();
                    var srcPtr = NativeArrayUnsafeUtility.GetUnsafePtr(src);

                    // Get pointer to _heightData array
                    var destPtr = _heightGCHandle.AddrOfPinnedObject().ToPointer();

                    // Copy data
                    UnsafeUtility.MemCpy(destPtr, srcPtr, src.Length * sizeof(float));
                }
            }
#else
            RenderTexture.active = _heightMap;
            _heightMap2D.ReadPixels(new Rect(0, 0, _heightMapRes, _heightMapRes), 0, 0);
            RenderTexture.active = null;
            Color[] colors = _heightMap2D.GetPixels();
            for (int y = 0; y < _heightMapRes; ++y)
                for (int x = 0; x < _heightMapRes; ++x)
                    _heightData[y, x] = colors[x + _heightMapRes * y].r;
#endif

            // Assign to terrain
            Terrain.terrainData.SetHeights(0, 0, _heightData);
        }

        private bool CheckDirty()
        {
            bool dirty = false;

            dirty |= _heightMap == null || !_heightMap.IsCreated() || _heightMap.height != _heightMapRes || _heightMap.width != _heightMapRes;
            dirty |= _heightData == null;

#if UNITY_2018
            dirty |= !_heightGCHandle.IsAllocated;
#endif

            return dirty;
        }

        private void UpdateRenderTexture()
        {
            _heightMapRes = Terrain.terrainData.heightmapResolution;
            if (CheckDirty())
            {
                Cleanup();

                RenderTextureFormat format;
#if UNITY_2018
                format = RenderTextureFormat.RFloat;
#else
                format = RenderTextureFormat.ARGBFloat;
#endif

                _heightMap = new RenderTexture(_heightMapRes, _heightMapRes, 0, format, RenderTextureReadWrite.Linear);
                _heightMap.enableRandomWrite = true;
                _heightMap.Create();

                _heightData = new float[_heightMapRes, _heightMapRes];

#if UNITY_2018
                _heightGCHandle = GCHandle.Alloc(_heightData, GCHandleType.Pinned);
#else
                _heightMap2D = new Texture2D(_heightMapRes, _heightMapRes, TextureFormat.RGBAFloat, false, true);
#endif
            }
        }

        private void ClearTerrain()
        {
            RenderTexture.active = _heightMap;
            GL.Clear(false, true, Color.black);
        }

        private void ApplyStamps()
        {
            _affectors.Clear();
            GetComponentsInChildren(false, _affectors);
            foreach (MercatorTerrainElement stamp in _affectors)
            {
                stamp.Apply(_heightMap, Terrain.transform.position, Terrain.terrainData.size);
            }
        }

        private void Cleanup()
        {
            if (_heightMap)
            {
                _heightMap.Release();
            }

#if UNITY_2018
            if (_heightGCHandle.IsAllocated)
            {
                _heightGCHandle.Free();
            }
#else
            if (_heightMap2D != null)
            {
                DestroyImmediate(_heightMap2D);
                _heightMap2D = null;
            }
#endif
        }
    }
}