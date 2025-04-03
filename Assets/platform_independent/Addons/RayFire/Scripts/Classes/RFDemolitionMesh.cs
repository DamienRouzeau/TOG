using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
using RayFire.DotNet;
#endif

// Rayfire classes
namespace RayFire
{
    [Serializable]
    public class RFDemolitionMesh
    {
        [Header ("  Fragments")]
        [Space (1)]
        
        [Tooltip ("Defines amount of new fragments after demolition.")]
        [Range (3, 300)]
        public int amount;

        [Tooltip ("Defines additional amount variation for object in percents.")]
        [Range (0, 100)]
        public int variation;

        [Tooltip ("Amount multiplier for next Depth level. Allows to decrease fragments amount of every next demolition level.")]
        [Range (0.01f, 1f)]
        public float depthFade;

        [Space (3)]
        [Tooltip ("Higher value allows to create more tiny fragments closer to collision contact point and bigger fragments far from it.")]
        [Range (0f, 1f)]
        public float contactBias;

        [Tooltip ("Defines Seed for fragmentation algorithm. Same Seed will produce same fragments for same object every time.")]
        [Range (1, 50)]
        public int seed;
        
        [Tooltip ("Allows to use RayFire Shatter properties for fragmentation. Works only if object has RayFire Shatter component.")]
        public bool useShatter;
        
        [Header ("  Advanced")]
        [Space (1)]
        
        public RFFragmentProperties properties;
        public RFRuntimeCaching runtimeCaching;
        
        // Hidden
        [HideInInspector] public int badMesh;
        [HideInInspector] public int shatterMode;
        [HideInInspector] public RFShatter rfShatter;
        [HideInInspector] public int totalAmount;
        [HideInInspector] public int innerSubId;
        [HideInInspector] public Mesh mesh;
        [HideInInspector] public bool compressPrefab;
        [HideInInspector] public Quaternion cacheRotationStart; 
        [HideInInspector] public RayfireShatter scrShatter;
        
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
        
        // Constructor
        public RFDemolitionMesh()
        {
            amount             = 15;
            variation          = 50;
            depthFade          = 0.5f;
            seed               = 1;
            contactBias        = 0.75f;
            useShatter         = false;
            runtimeCaching     = new RFRuntimeCaching();
            properties         = new RFFragmentProperties();
            
            // Hidden
            badMesh            = 0;
            shatterMode        = 1;
            rfShatter          = null;
            totalAmount        = 0;
            innerSubId         = 0;
            mesh               = null;
            compressPrefab     = true;
            cacheRotationStart = Quaternion.identity;
        }

        // Copy from
        public void CopyFrom (RFDemolitionMesh demolition)
        {
            amount         = demolition.amount;
            variation      = demolition.variation;
            depthFade      = demolition.depthFade;
            seed           = demolition.seed;
            contactBias    = demolition.contactBias;
            useShatter     = false;
            badMesh        = 0;
            shatterMode    = demolition.shatterMode;
            runtimeCaching = new RFRuntimeCaching();
            properties.CopyFrom (demolition.properties);
            rfShatter   = null;
            totalAmount = 0;
            innerSubId  = 0;
            mesh        = null;
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////
        
        // Get layer for fragments
        public int GetLayer (RayfireRigid scr)
        {
            // Inherit layer
            if (properties.layer.Length == 0)
                return scr.gameObject.layer;
            
            // No custom layer
            if (RayfireMan.inst.layers.Contains (properties.layer) == false)
                return 0;

            // Get custom layer
            return LayerMask.NameToLayer (properties.layer);
        }
        
        // Cor to fragment mesh over several frames
        public IEnumerator RuntimeCachingCor (RayfireRigid scr)
        {
            // Object should be demolished when cached all meshes but not during caching
            bool demolitionShouldLocal = scr.limitations.demolitionShould == true;
            scr.limitations.demolitionShould = false;
            
            // Input mesh, setup, record time
            float t1 = Time.realtimeSinceStartup;
            if (RFFragment.PrepareCacheMeshes (scr) == false)
                yield break;
                        
            // Set list with amount of mesh for every frame
            List<int> batchAmount = runtimeCaching.type == CachingType.ByFrames
                ? RFRuntimeCaching.GetBatchByFrames(runtimeCaching.frames, totalAmount)
                : RFRuntimeCaching.GetBatchByFragments(runtimeCaching.fragments, totalAmount);
            
            // Caching in progress
            runtimeCaching.inProgress = true;

            // Wait next frame if input took too much time or long batch
            float t2 = Time.realtimeSinceStartup - t1;
            if (t2 > 0.025f || batchAmount.Count > 5)
                yield return null;

            // Save tm for multi frame caching
            GameObject tmRefGo = RFRuntimeCaching.CreateTmRef (scr);

            // Start rotation
            cacheRotationStart = scr.transForm.rotation;
            
            // Iterate every frame. Calc local frame meshes
            List<Mesh>         meshesList = new List<Mesh>();
            List<Vector3>      pivotsList = new List<Vector3>();
            List<RFDictionary> subList    = new List<RFDictionary>();
            for (int i = 0; i < batchAmount.Count; i++)
            {
                RFFragment.CacheMeshesMult (tmRefGo.transform, ref meshesList, ref pivotsList, ref subList, scr, batchAmount, i);
                // TODO create fragments for current batch
                // TODO record time and decrease batches amount if less 30 fps
                yield return null;
            }
            
            // Set to main data vars
            scr.meshes = meshesList.ToArray();
            scr.pivots = pivotsList.ToArray();
            scr.subIds = subList;

            // Clear
            scr.DestroyObject (tmRefGo);
            scr.meshDemolition.scrShatter = null;
            
            // Set demolition ready state
            if (runtimeCaching.skipFirstDemolition == false && demolitionShouldLocal == true)
                scr.limitations.demolitionShould = true;
            
            // Reset damage
            if (runtimeCaching.skipFirstDemolition == true && demolitionShouldLocal == true)
                scr.damage.Reset();
            
            // Caching finished
            runtimeCaching.inProgress = false;
            runtimeCaching.wasUsed = true;
        }
    }
}