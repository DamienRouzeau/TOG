// Created by Ronis Vision. All rights reserved
// 23.08.2019.

using System.Collections.Generic;
using RVModules.RVUtilities;
using UnityEngine;

namespace RVModules.RVSmartAI.Content.Scanners
{
    /// <summary>
    /// Use Physx for spatial queries
    /// For object to be added it has to have MonoBehaviour implementing IScannable on the same game object as it's collider
    /// </summary>
    public class PhysxEnvironmentScanner : MonoBehaviour, IEnvironmentScanner
    {
        #region Fields

        /// <summary>
        /// Mask used for scanning environment
        /// </summary>
        [SerializeField]
        private LayerMask scannerLayerMask;

        /// <summary>
        /// How many objects will scanner be able to get in one scan
        /// </summary>
        [SerializeField]
        private int bufferSize = 100;

        // buffer, serialized to show in inspector
        [SerializeField]
        private Collider[] resultsBuffer;

        [SerializeField]
        private List<Object> objects = new List<Object>();

        #endregion

        #region Public methods

        public List<Object> ScanEnvironment(Vector3 _position, float _range)
        {
            objects.Clear();

            // clear buffer
            for (var i = 0; i < resultsBuffer.Length; i++) resultsBuffer[i] = null;

            Physics.OverlapSphereNonAlloc(_position, _range, resultsBuffer, scannerLayerMask);
            for (var index = 0; index < resultsBuffer.Length; index++)
            {
                var result = resultsBuffer[index];
                if (result == null) continue;
                var scannable = result.GetComponent<IScannable>();
                if (scannable == null) continue;
                objects.Add(scannable.GetObject);
            }

            return objects;
        }

        #endregion

        #region Not public methods

        private void Awake()
        {
            resultsBuffer = new Collider[bufferSize];
        }

        #endregion
    }
}