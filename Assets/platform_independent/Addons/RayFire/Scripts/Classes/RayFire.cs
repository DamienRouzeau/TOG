using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Rayfire classes
namespace RayFire
{
    public class RFTri
    {
        public int meshId;
        public int subMeshId = -1;
        public List<int> ids = new List<int>();
        public List<Vector3> vpos = new List<Vector3>();
        public List<Vector3> vnormal = new List<Vector3>();
        public List<Vector2> uvs = new List<Vector2>();
        public List<Vector4> tangents = new List<Vector4>();
        public List<RFTri> neibTris = new List<RFTri>();
    }

    [Serializable]
    public class RFDictionary
    {
        public List<int> keys;
        public List<int> values;

        // Constructor
        public RFDictionary(Dictionary<int, int> dictionary)
        {
            keys = new List<int>();
            values = new List<int>();
            keys = dictionary.Keys.ToList();
            values =  dictionary.Values.ToList();
        }
    }

    /// /////////////////////////////////////////////////////////
    /// Rigid
    /// /////////////////////////////////////////////////////////

    // Damage
    [Serializable]
    public class RFDamage
    {
        [Tooltip("Allows to demolish object by it's own floating Damage value.")]
        public bool enable;
        
        [Tooltip("Allows to accumulate damage value by collisions during dynamic simulation.")]
        public bool collectCollisions;
        
        [Tooltip("Defines maximum allowed damage for object before it will be demolished.")]
        public float maxDamage;
        
        [Tooltip("Shows current damage value. Can be increased by public method: \nApplyDamage(float damageValue, Vector3 damagePosition)")]
        public float currentDamage;
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////

        // Constructor
        public RFDamage()
        {
            enable = false;
            maxDamage = 100f;
            currentDamage = 0f;
            collectCollisions = false;
        }

        // Copy from
        public void CopyFrom(RFDamage damage)
        {
            enable = damage.enable;
            maxDamage = damage.maxDamage;
            currentDamage = 0f;
            collectCollisions = damage.collectCollisions;
        }

       /// /////////////////////////////////////////////////////////
       /// Methods
       /// /////////////////////////////////////////////////////////     
        
        // Reset damage
        public void Reset()
        {
            currentDamage = 0f;
        }

        // Add damage
        public bool ApplyDamage(float damageValue)
        {
            // Add damage
            currentDamage += damageValue;

            // Check
            if (enable == true && currentDamage >= maxDamage)
                return true;

            return false;
        }
    }
    
    // Gluing
    [Serializable]
    public class RFGlue
    {
        // Variables
        public bool enable;
        [Range(2, 200)] public int amount;
        [Range(0, 100)] public int seed;
        [Range(0f, 1f)] public float relax;

        // Constructor
        public RFGlue()
        {
            enable = false;
            amount = 10;
            seed = 1;
            relax = 0.5f;
        }
    }

    /// /////////////////////////////////////////////////////////
    /// Shatter
    /// /////////////////////////////////////////////////////////

    [Serializable]
    public class RFVoronoi
    {
        public int amount = 30;
        [Range(0f, 1f)] public float centerBias = 0f;
        
        // Amount
        public int Amount
        {
            get
            {
                if (amount < 1)
                    return 1;
                if (amount > 20000)
                    return 2;
                return amount;
            }
        }
    }

    [Serializable]
    public class RFSplinters
    {
        public AxisType axis = AxisType.YGreen;
        public int amount = 30;
        [Range(0f, 1f)] public float strength = 0.7f;
        [Range(0f, 1f)] public float centerBias = 0f;

        // Amount
        public int Amount
        {
            get
            {
                if (amount < 2)
                    return 2;
                if (amount > 20000)
                    return 2;
                return amount;
            }
        }
    }

    [Serializable]
    public class RFRadial
    {
        public AxisType centerAxis = AxisType.YGreen;
        [Range(0.01f, 30f)] public float radius = 1f;
        [Range(0f, 1f)] public float divergence = 1f;
        public bool restrictToPlane = true;

        [Header("Rings")]
        [Range(3, 60)] public int rings = 10;
        [Range(0, 100)] public int focus = 0;
        [Range(0, 100)] public int focusStr = 50;
        [Range(0, 100)] public int randomRings = 50;

        [Header("Rays")]
        [Range(3, 60)] public int rays = 10;
        [Range(0, 100)] public int randomRays = 0;
        [Range(-90, 90)] public int twist = 0;
    }

    [Serializable]
    public class RFSlice
    {
        public PlaneType plane = PlaneType.XZ;
        // [Range(0, 30f)] public float rotation = 0f;
        public List<Transform> sliceList;

        // Get axis
        public Vector3 Axis (Transform tm)
        {
            if (plane == PlaneType.YZ)
                return tm.right;
            if (plane == PlaneType.XZ)
                return tm.up;
            return tm.forward;
        }
    }

    [Serializable]
    public class RFTets
    {
        public Vector3Int density = new Vector3Int(3, 3, 3);
        [Range(0, 100)] public int noise = 100;
        //public float multiplier = 1f;
    }

    [Serializable]
    public class RFCustom
    {
        public List<Vector3> localPoints = new List<Vector3>();
        //public UnityEngine.Object[] objects = null;
        //[Range(1, 100)] public int percents = 100;
    }
}

