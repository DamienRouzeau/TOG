// Created by Ronis Vision. All rights reserved
// 04.10.2020.

using System;
using RVModules.RVSmartAI;
using RVModules.RVUtilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RVHonorAI
{
    /// <summary>
    /// Used to store informations about target like last seen position, is it currently visible etc
    /// </summary>
    [Serializable] public class TargetInfo : IPoolable, IUnityComponent
    {
        #region Fields

        /// <summary>
        /// target casted on Unity object, for debug(inspector) only, editor only(wont be set in build)
        /// </summary>
        [SerializeField]
        private Object targetObject;
        
        [SerializeField]
        private Vector3 lastSeenPosition;

        [SerializeField]
        private float lastSeenTime;
        
        [SerializeField]
        private bool visible;

        private ITarget target;

        #endregion

        #region Properties

        public Action OnSpawn { get; set; }
        public Action OnDespawn { get; set; }

        public ITarget Target
        {
            get => target;
            set
            {
                target = value;
#if UNITY_EDITOR
                targetObject = target as Object;
#endif
            }
        }

        public Vector3 LastSeenPosition
        {
            get => lastSeenPosition;
            set => lastSeenPosition = value;
        }

        public float LastSeenTime
        {
            get => lastSeenTime;
            set => lastSeenTime = value;
        }

        public bool Visible
        {
            get => visible;
            set => visible = value;
        }

        #endregion

        public TargetInfo()
        {
            OnSpawn += () =>
            {
                Target = null;
#if UNITY_EDITOR
                targetObject = null;
#endif
                LastSeenPosition = Vector3.zero;
                LastSeenTime = 0;
                Visible = false;
            };
        }

        public Component ToUnityComponent() => targetObject as Component;
    }
}