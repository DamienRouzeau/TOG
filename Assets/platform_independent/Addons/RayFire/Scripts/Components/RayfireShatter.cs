using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace RayFire
{
    [AddComponentMenu("RayFire/Rayfire Shatter")]
    [HelpURL("http://rayfirestudios.com/unity-online-help/unity-shatter-component/")]
    public class RayfireShatter : MonoBehaviour
    {
        [Header("Center")]
        [HideInInspector] public bool showCenter = false;
        [HideInInspector] public Vector3 centerPosition;
        [HideInInspector] public Quaternion centerDirection;
        [HideInInspector] public Vector3 centerRotation;

        [Header("Components")]
        [HideInInspector] public Transform transForm;
        [HideInInspector] public MeshFilter meshFilter;
        [HideInInspector] public MeshRenderer meshRenderer;
        [HideInInspector] public SkinnedMeshRenderer skinnedMeshRend;

        [Header("Variables")]
        [HideInInspector] public Mesh[] meshes = null;
        [HideInInspector] public Vector3[] pivots = null;
        [HideInInspector] public List<Transform> rootChildList = new List<Transform>();
        [HideInInspector] public List<GameObject> fragmentsAll = new List<GameObject>();
        [HideInInspector] public List<GameObject> fragmentsLast = new List<GameObject>();
        [HideInInspector] public List<RFDictionary> origSubMeshIdsRF = new List<RFDictionary>();

        // Hidden
        [HideInInspector] public int shatterMode = 1;
        [HideInInspector] public bool colorPreview = false;
        [HideInInspector] public bool scalePreview = true;
        [HideInInspector] public float previewScale = 0f;
        
        [Header ("Fragments")]
        public FragType type = FragType.Voronoi;
        public RFVoronoi   voronoi   = new RFVoronoi();
        public RFSplinters splinters = new RFSplinters();
        public RFSplinters slabs     = new RFSplinters();
        public RFRadial    radial    = new RFRadial();
        [HideInInspector]
        public RFCustom custom = new RFCustom();
        public RFSlice slice = new RFSlice();
        public RFTets  tets  = new RFTets();

        [Header ("Common Properties")]
        [Tooltip ("Editor: Allows to fragment complex multi element hi poly meshes with topology issues like open edges and unwelded vertices.")]
        public FragmentMode mode = FragmentMode.Editor;
        [Range (0, 100)] public int seed = 1;
        public bool decompose       = false;
        public bool removeCollinear = true;
        public bool copyComponents  = false;
        public RFSurface interior = new RFSurface();
        public RFGlue    gluing   = new RFGlue();
        
        [Header ("Editor Mode Properties")]
        public bool excludeInnerFragments = false;

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        // Reset
        private void Reset()
        {
            ResetCenter();
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////

        // Cache variables
        private void DefineComponents()
        {
            // Get components
            transForm = GetComponent<Transform>();
            origSubMeshIdsRF = new List<RFDictionary>();
            
            // Skinned mesh
            skinnedMeshRend = GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRend != null)
                return;
            
            // Mesh Filter
            meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
                meshFilter = gameObject.AddComponent<MeshFilter>();
        
            // Mesh renderer
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null)
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        
        // Fragment this object by shatter properties
        public void Fragment()
        {
            // Check if prefab
            if (gameObject.scene.rootCount == 0)
            {
                Debug.Log ("Shatter component unable to fragment prefab because prefab unable to store Unity mesh. Fragment prefab in scene.");
                return;
            }
            
            // Cache variables
            DefineComponents();
            
            // Define meshfilter
            if (meshFilter == null && skinnedMeshRend == null)
            {
                Debug.Log("Warning: Object has no mesh");
                return;
            }

            // Cache fragments
            meshes = null;
            pivots = null;
            fragmentsLast.Clear();

            // Cache
            RFFragment.CacheMeshes(ref meshes, ref pivots, ref origSubMeshIdsRF, this);

            // Stop
            if (meshes == null)
                return;
            
            // Create fragments
            fragmentsLast = CreateFragments();

            // Collect to all fragments
            fragmentsAll.AddRange(fragmentsLast);
        }

        // Create fragments by mesh and pivots array
        private List<GameObject> CreateFragments()
        {
            // No mesh were cached
            if (meshes == null)
                return null;

            // Clear array for new fragments
            GameObject[] fragArray = new GameObject[meshes.Length];

            // Vars 
            string goName = gameObject.name;
            string baseName = goName + "_sh_";
            
            // Create root object
            GameObject root = new GameObject(goName + "_root");
            root.transform.position = transForm.position;
            root.transform.rotation = transForm.rotation;
            root.transform.localScale = Vector3.one;
            root.transform.parent = transForm.parent;
            rootChildList.Add(root.transform);

            // Create instance for fragments
            GameObject fragInstance;
            if (copyComponents == true)
            {
                fragInstance = Instantiate(gameObject);
                fragInstance.transform.rotation = Quaternion.identity;
                fragInstance.transform.localScale = Vector3.one;

                // Destroy shatter
                DestroyImmediate(fragInstance.GetComponent<RayfireShatter>());
            }
            else
            {
                fragInstance = new GameObject();
                fragInstance.AddComponent<MeshFilter>();
                fragInstance.AddComponent<MeshRenderer>();
            }
            
            // Get original mats
            Material[] mats = skinnedMeshRend != null 
                ? skinnedMeshRend.sharedMaterials 
                : meshRenderer.sharedMaterials;
            
            // Create fragment objects
            for (int i = 0; i < meshes.Length; ++i)
            {
                // Skip if no verts
                if (meshes[i].vertexCount == 0)
                    continue;

                // Instantiate. IMPORTANT do not parent when Instantiate
                GameObject fragGo = Instantiate(fragInstance);

                // Set multymaterial
                MeshRenderer targetRend = fragGo.GetComponent<MeshRenderer>();
                RFSurface.SetMaterial(origSubMeshIdsRF, mats, interior, targetRend, i, meshes.Length);
                
                // Set fragment object name and tm
                fragGo.name = baseName + (i + 1);
                fragGo.transform.position = transForm.position + pivots[i];
                fragGo.transform.parent = root.transform;
                
                // Set fragment mesh
                MeshFilter mf = fragGo.GetComponent<MeshFilter>();
                mf.sharedMesh = meshes[i];
                mf.sharedMesh.name = fragGo.name;

                // Set mesh collider
                MeshCollider mc = fragGo.GetComponent<MeshCollider>();
                if (mc != null)
                    mc.sharedMesh = meshes[i];

                // Add in array
                fragArray[i] = fragGo;
            }

            // Destroy instance
            DestroyImmediate(fragInstance);

            // Empty lists
            meshes = null;
            pivots = null;

            return fragArray.ToList();
        }

        // Delete fragments from last Fragment method
        public void DeleteFragmentsLast()
        {
            // Stop if zero
            if (rootChildList.Count <= 0) 
                return;
            
            // Check for all roots
            rootChildList.RemoveAll(t => t.Equals(null));

            // No roots
            if (rootChildList.Count == 0)
                return;  
            
            // Destroy root with fragments
            Transform rootTm = rootChildList[rootChildList.Count - 1];
            DestroyImmediate(rootTm.gameObject);

            // Remove from list
            rootChildList.RemoveAt(rootChildList.Count - 1);
            fragmentsLast.Clear();
            
            // Check for all fragments list
            fragmentsAll.RemoveAll(t => t.Equals(null));
        }

        // Delete all fragments and roots
        public void DeleteFragmentsAll()
        {
            if (rootChildList.Count > 0)
            {
                // Check for all fragments list
                rootChildList.RemoveAll(t => t.Equals(null));

                // No roots TODO update fragments info
                if (rootChildList.Count == 0)
                    return;  
                
                // Delete roots
                foreach (Transform root in rootChildList)
                    DestroyImmediate(root.gameObject);
                rootChildList.Clear();

                // Clear lists
                fragmentsLast.Clear();
                fragmentsAll.Clear();
            }
        }

        // Reset center helper
        public void ResetCenter()
        {
            centerPosition = Vector3.zero;
            centerDirection = Quaternion.identity;
        }
    }
}