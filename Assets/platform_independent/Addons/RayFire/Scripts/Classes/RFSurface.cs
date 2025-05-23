﻿using System.Collections.Generic;
using UnityEngine;

namespace RayFire
{
    [System.Serializable]
    public class RFSurface
    {
        [Header("  Inner surface")]
        [Space (1)]
        
        [Tooltip("Defines material for fragment's inner surface.")]
        public Material innerMaterial;
        [Space (1)]
       
        [Tooltip("Defines mapping scale for inner surface.")]
        [Range(0.01f, 5f)]public float mappingScale;

        [Header("  Outer surface")]
        [Space (1)]
        
        public Material outerMaterial;
            
        // Hidden
        [HideInInspector] public bool needNewMat;
                    
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
         
        // Constructor
        public RFSurface()
        {
            innerMaterial = null;
            mappingScale = 0.1f;
            needNewMat = false;
            outerMaterial = null;
        }

        // Copy from
        public void CopyFrom(RFSurface interior)
        {
            innerMaterial = interior.innerMaterial;
            mappingScale = interior.mappingScale;
            needNewMat = interior.needNewMat;
            outerMaterial = interior.outerMaterial;
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////
        
        // Set material to fragment by it's interior properties and parent material
        public static void SetMaterial(List<RFDictionary> origSubMeshIdsRF, Material[] sharedMaterials, RFSurface interior, MeshRenderer targetRend, int i, int amount)
        {
            if (origSubMeshIdsRF != null && origSubMeshIdsRF.Count == amount)
            {
                Material[] newMaterials = new Material[origSubMeshIdsRF[i].values.Count];
                
                // TODO implement in fragmentation to avoid calcs
                if (interior.outerMaterial != null)
                {
                    for (int j = 0; j < newMaterials.Length; j++)
                        newMaterials[j] = interior.outerMaterial;
                }
                else
                {
                    for (int j = 0; j < origSubMeshIdsRF[i].values.Count; j++)
                    {
                        int matId = origSubMeshIdsRF[i].values[j];
                        if (matId < sharedMaterials.Length)
                            newMaterials[j] = sharedMaterials[matId];
                        else
                            newMaterials[j] = interior.innerMaterial;
                    }
                }

                targetRend.sharedMaterials = newMaterials;
            }
        }

        // Get inner faces sub mesh id
        public static int InnerSubId(RFSurface interior, Material[] mats)
        {
            if (interior.innerMaterial == null) return 0;
            for (int i = 0; i < mats.Length; i++)
                if (mats[i] == interior.innerMaterial)
                    return i;
            return -1;
        }
    }
}

