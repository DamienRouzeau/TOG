using System.Collections.Generic;
using UnityEngine;

namespace RayFire
{
    [System.Serializable]
    public class RFReferenceDemolition
    {
        [Header ("  Source")]
        [Space (1)]
        
        public GameObject reference;
        public List<GameObject> randomList;
        
        [Header ("  Properties")]
        [Space (1)]
        
        //public AlignType type;
        
        [Tooltip ("Add RayFire Rigid component to reference with mesh")]
        public bool addRigid;
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
        
        // Constructor
        public RFReferenceDemolition()
        {
            addRigid = true;
            reference = null;
            randomList = new List<GameObject>();
        }

        // Copy from
        public void CopyFrom (RFReferenceDemolition referenceDemolitionDml)
        {
            addRigid = referenceDemolitionDml.addRigid;;
            reference = referenceDemolitionDml.reference;
            randomList = referenceDemolitionDml.randomList;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////   
        
        // Get reference
        public GameObject GetReference()
        {
            // Return single ref
            if (reference != null && randomList.Count == 0)
                return reference;

            // Get random ref
            List<GameObject> refs = new List<GameObject>();
            if (randomList.Count > 0)
            {
                foreach (var r in randomList)
                    if (r != null)
                        refs.Add (r);
                if (refs.Count > 0)
                    return refs[Random.Range (0, refs.Count)];
            }

            return null;
        }
    }
}