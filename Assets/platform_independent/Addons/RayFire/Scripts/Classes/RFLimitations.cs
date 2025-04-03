using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
using RayFire.DotNet;
#endif

// Rayfire classes
namespace RayFire
{
    [Serializable]
    public class RFLimitations
    {
        // Hidden
        [HideInInspector]
        public bool demolished;
        [HideInInspector]
        public float birthTime;
        [HideInInspector]
        public float bboxSize;
        [HideInInspector]
        public int currentDepth;
        
        [Tooltip ("Local Object solidity multiplier for object. Low Solidity makes object more fragile.")]
        [Range (0.0f, 10f)]
        public float solidity;

        [Tooltip ("Defines how deep object can be demolished. Depth is limitless if set to 0.")]
        [Range (0, 7)]
        public int depth;

        [Tooltip ("Safe time. Measures in seconds and allows to prevent fragments from being demolished right after they were just created.")]
        [Range (0.05f, 10f)]
        public float time;

        [Tooltip ("Prevent objects with bounding box size less than defined value to be demolished.")]
        [Range (0.01f, 5f)]
        public float size;
        
        //[Tooltip ("")]
        //[Range (1, 100)]
        // TODO public int probability;
        
        [Tooltip ("Allows object to be sliced by object with RayFire Blade component.")]
        public bool sliceByBlade;

        [Header ("Hidden")]
        [HideInInspector] public List<Vector3> slicePlanes;
        [HideInInspector] public bool available;
        [HideInInspector] public bool demolitionShould;

        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
        
        // Constructor
        public RFLimitations()
        {
            currentDepth     = 0;
            solidity         = 0.1f;
            depth            = 1;
            size             = 0.1f;
            time             = 1f;
            sliceByBlade     = false;
            slicePlanes      = new List<Vector3>();
            available        = true;
            demolitionShould = false;
        }

        // Copy from
        public void CopyFrom (RFLimitations limitations)
        {
            // Do not copy currentDepth. Set in other place
            solidity         = limitations.solidity;
            depth            = limitations.depth;
            size             = limitations.size;
            time             = limitations.time; // Add random value to spread new fragments safe time
            sliceByBlade     = limitations.sliceByBlade;
            slicePlanes      = new List<Vector3>();
            available        = true;
            demolitionShould = false;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////
        
        // Check for user mistakes
        public static void Checks (RayfireRigid scr)
        {
   
            // ////////////////
            // Object Type
            // ////////////////
            
            // Object can not be simulated as mesh
            if (scr.objectType == ObjectType.Mesh)
            {
                if (scr.meshFilter == null || scr.meshFilter.sharedMesh == null)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Object Type set to " + scr.objectType.ToString() + " but object has no mesh. Object Excluded from simulation.", scr.gameObject);
                    scr.physics.exclude = true;
                }
            }
            
            // Object can not be simulated as cluster
            else if (scr.objectType == ObjectType.NestedCluster || scr.objectType == ObjectType.ConnectedCluster)
            {
                if (scr.transForm.childCount == 0)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Object Type set to " + scr.objectType.ToString() + " but object has no children. Object Excluded from simulation.", scr.gameObject);
                    scr.physics.exclude = true;
                }
            }
            
            // Object can not be simulated as mesh
            else if (scr.objectType == ObjectType.SkinnedMesh)
            {
                if (scr.skinnedMeshRend == null)
                    Debug.Log ("RayFire Rigid: " + scr.name + " Object Type set to " + scr.objectType.ToString() + " but object has no SkinnedMeshRenderer. Object Excluded from simulation.", scr.gameObject);
                
                // Excluded from sim by default
                scr.physics.exclude = true;
            }
            
            // ////////////////
            // Demolition Type
            // ////////////////
            
            // Demolition checks
            if (scr.demolitionType != DemolitionType.None)
            {
                // Static
                if (scr.simulationType == SimType.Static)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Simulation Type set to " + scr.simulationType.ToString() + " but Demolition Type is " + scr.demolitionType.ToString() + ". Demolition Type set to None.", scr.gameObject);
                    scr.demolitionType = DemolitionType.None;
                }
                
                // Set runtime demolition for clusters and skinned mesh
                if (scr.objectType == ObjectType.SkinnedMesh ||
                    scr.objectType == ObjectType.NestedCluster ||
                    scr.objectType == ObjectType.ConnectedCluster)
                {
                    if (scr.demolitionType != DemolitionType.Runtime && scr.demolitionType != DemolitionType.ReferenceDemolition)
                    {
                        Debug.Log ("RayFire Rigid: " + scr.name + " Object Type set to " + scr.objectType.ToString() + " but Demolition Type is " + scr.demolitionType.ToString() + ". Demolition Type set to Runtime.", scr.gameObject);
                        scr.demolitionType = DemolitionType.Runtime;
                    }
                }
                
                // No Shatter component for runtime demolition with Use Shatter on
                if (scr.meshDemolition.scrShatter == null && scr.meshDemolition.useShatter == true)
                {
                    if (scr.demolitionType == DemolitionType.Runtime ||
                        scr.demolitionType == DemolitionType.AwakePrecache ||
                        scr.demolitionType == DemolitionType.AwakePrefragment)
                    {
                        
                        Debug.Log ("RayFire Rigid: " + scr.name + "Demolition Type is " + scr.demolitionType.ToString() + ". Has no Shatter component, but Use Shatter property is On. Use Shatter property was turned Off.", scr.gameObject);
                        scr.meshDemolition.useShatter = false;
                    }
                }
            }
            
            // None check
            if (scr.demolitionType == DemolitionType.None)
            {
                if (scr.HasMeshes == true)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to None. Had manually precached meshes which were destroyed.", scr.gameObject);
                    scr.DeleteCache();
                }

                if (scr.HasFragments == true)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to None. Had manually prefragmented objects which were destroyed.", scr.gameObject);
                    scr.DeleteFragments();
                }

                if (scr.HasRfMeshes == true)
                {
                    Debug.Log ("RayFire Rigid:" + scr.name + " Demolition Type set to None. Had manually precached serialized meshes which were destroyed.", scr.gameObject);
                    scr.DeleteCache();
                }
            }

            // Runtime check
            else if (scr.demolitionType == DemolitionType.Runtime)
            {
                if (scr.HasMeshes == true)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to Runtime. Had manually precached meshes which were destroyed.", scr.gameObject);
                    scr.DeleteCache();
                }

                if (scr.HasFragments == true)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to Runtime. Had manually prefragmented objects which were destroyed.", scr.gameObject);
                    scr.DeleteFragments();
                }

                if (scr.HasRfMeshes == true)
                {
                    Debug.Log ("RayFire Rigid:" + scr.name + " Demolition Type set to Runtime. Had manually precached serialized meshes which were destroyed.", scr.gameObject);
                    scr.DeleteCache();
                }
                
                // No runtime caching for rigid with shatter with tets/slices/glue
                if (scr.meshDemolition.useShatter == true && scr.meshDemolition.runtimeCaching.type != CachingType.Disable)
                {
                    if (scr.meshDemolition.scrShatter.type == FragType.Decompose ||
                        scr.meshDemolition.scrShatter.type == FragType.Tets ||
                        scr.meshDemolition.scrShatter.type == FragType.Slices || 
                        scr.meshDemolition.scrShatter.gluing.enable == true)
                    {
                        Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type is Runtime, Use Shatter is On. Unsupported fragments type. Runtime Caching supports only Voronoi, Splinters, Slabs and Radial fragmentation types. Runtime Caching was Disabled.", scr.gameObject);
                        scr.meshDemolition.runtimeCaching.type = CachingType.Disable;
                    }
                }
            }

            // Awake precache check
            else if (scr.demolitionType == DemolitionType.AwakePrecache)
            {
                if (scr.HasMeshes == true)
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to Awake Precache. Had manually precached meshes which were overwritten.", scr.gameObject);
                if (scr.HasFragments == true)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to Awake Precache. Had manually prefragmented objects which were destroyed.", scr.gameObject);
                    scr.DeleteFragments();
                }

                if (scr.HasRfMeshes == true)
                {
                    Debug.Log ("RayFire Rigid:" + scr.name + " Demolition Type set to Awake Precache. Had manually precached serialized meshes which were destroyed.", scr.gameObject);
                    scr.rfMeshes = null;
                }
            }

            // Awake prefragmented check
            else if (scr.demolitionType == DemolitionType.AwakePrefragment)
            {
                if (scr.HasFragments == true)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to Awake Prefragment. Had manually prefragmented objects which were overwritten.", scr.gameObject);
                    scr.demolitionType = DemolitionType.None;
                }

                if (scr.HasMeshes == true)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to Awake Prefragment. Had manually precached meshes which were destroyed.", scr.gameObject);
                    scr.DeleteCache();
                }

                if (scr.HasRfMeshes == true)
                {
                    Debug.Log ("RayFire Rigid:" + scr.name + " Demolition Type set to Awake Prefragment. Had manually precached serialized meshes which were destroyed.", scr.gameObject);
                    scr.rfMeshes = null;
                }
            }

            // Prefab precache check
            else if (scr.demolitionType == DemolitionType.ManualPrefabPrecache)
            {
                if (scr.HasRfMeshes == false)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to Prefab Precache. Has no precached serialized meshes, Demolition Type set to None.", scr.gameObject);
                    scr.demolitionType = DemolitionType.None;
                }

                if (scr.HasMeshes == true)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to Prefab Precache. Had manually precached meshes which were destroyed.", scr.gameObject);
                    scr.meshes = null;
                }

                if (scr.HasFragments == true)
                {
                    Debug.Log ("RayFire Rigid:" + scr.name + " Demolition Type set to Prefab Precache. Had manually prefragmented objects which were destroyed.", scr.gameObject);
                    scr.DeleteFragments();
                }
            }

            // Manual precache check
            else if (scr.demolitionType == DemolitionType.ManualPrecache)
            {
                if (scr.HasMeshes == false)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to Manual Precache. Has no manually precached meshes, Demolition Type set to None.", scr.gameObject);
                    scr.demolitionType = DemolitionType.None;
                }

                if (scr.HasFragments == true)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to Manual Precache. Had manually prefragmented objects which were destroyed.", scr.gameObject);
                    scr.DeleteFragments();
                }

                if (scr.HasRfMeshes == true)
                {
                    Debug.Log ("RayFire Rigid:" + scr.name + " Demolition Type set to Manual Precache. Had manually precached serialized meshes which were destroyed.", scr.gameObject);
                    scr.rfMeshes = null;
                }
            }

            // Manual prefragmented check
            else if (scr.demolitionType == DemolitionType.ManualPrefragment)
            {
                if (scr.HasFragments == false)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to Manual Prefragment. Has no manually prefragmented objects, Demolition Type set to None.", scr.gameObject);
                    scr.demolitionType = DemolitionType.None;
                }

                if (scr.HasMeshes == true)
                {
                    Debug.Log ("RayFire Rigid: " + scr.name + " Demolition Type set to Manual Prefragment. Had manually precached meshes which were destroyed.", scr.gameObject);
                    scr.DeleteCache();
                }

                if (scr.HasRfMeshes == true)
                {
                    Debug.Log ("RayFire Rigid:" + scr.name + " Demolition Type set to Manual Prefragment. Had manually precached serialized meshes which were destroyed.", scr.gameObject);
                    scr.rfMeshes = null;
                }
            }
            
            // TODO Tag and Layer check
        }
        
        // Create root
        public static void CreateRoot(RayfireRigid rfScr)
        {
           GameObject root = new GameObject();
           root.transform.parent = RayfireMan.inst.transForm;
           root.name             = rfScr.gameObject.name + "_root";
           rfScr.rootChild             = root.transform;
           rfScr.rootChild.position    = rfScr.transForm.position;
           rfScr.rootChild.rotation    = rfScr.transForm.rotation;
        }
    }
}