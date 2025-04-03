// Created by Ronis Vision. All rights reserved
// 07.03.2021.

using System;
using RVModules.RVLoadBalancer;
using RVModules.RVUtilities;

namespace RVHonorAI
{
    /// <summary>
    /// Destroys game object after set amount of time
    /// </summary>
    public class DestroyGameObjectAfter : LoadBalancedBehaviour
    {
        private float creationTime;
        public float destroyAfter = float.MaxValue;

        private void Awake()
        {
            creationTime = UnityTime.Time;
        }

        protected override void LoadBalancedUpdate(float _deltaTime)
        {
           if(UnityTime.Time > creationTime + destroyAfter) Destroy(gameObject);
        }
    }
}