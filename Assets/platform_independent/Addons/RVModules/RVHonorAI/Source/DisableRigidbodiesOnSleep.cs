// Created by Ronis Vision. All rights reserved
// 16.10.2020.

using RVModules.RVLoadBalancer;
using UnityEngine;
using UnityEngine.Events;

namespace RVHonorAI
{
    /// <summary>
    /// Check if all rigidbodies on this game object hierarchy are sleeping and then disables them (set to kinematic or removed)
    /// and optionally removes their colliders
    /// optional timeout will disable them all after set time
    /// </summary>
    public class DisableRigidbodiesOnSleep : LoadBalancedBehaviour
    {
        private Rigidbody[] rigidbodies;

        [SerializeField]
        [Tooltip("Rigidbody will be disabled after x seconds")]
        private int forceDisableAfter = 20;

        [SerializeField]
        [Tooltip("If false, rigidbody will be set to kinematic, if true it will be removed")]
        private bool removeRigidbody;

        [SerializeField]
        [Tooltip("Should colliders on this game objects be removed")]
        private bool removeColliders;

        [SerializeField]
        [Tooltip("Time after all rigidbodies will be disabled. Set 0 to disable this feature")]
        private float elapsedTime;

        public UnityEvent onAllRigidbodiesSleep;

        private void Awake()
        {
            rigidbodies = GetComponentsInChildren<Rigidbody>();
            elapsedTime = 0;
        }

        protected override LoadBalancerConfig LoadBalancerConfig => new LoadBalancerConfig("DisableRigidbodiesOnSleep", LoadBalancerType.EveryXFrames, 0, true);

        protected override void LoadBalancedUpdate(float _deltaTime)
        {
            elapsedTime += _deltaTime;

            bool allSleepOrTimeout = true;
            bool timeout = elapsedTime > forceDisableAfter && forceDisableAfter >= 0;

            foreach (var rb in rigidbodies)
            {
                if (timeout || rb.IsSleeping())
                {
                    if (removeColliders)
                    {
                        foreach (var component in rb.GetComponents<Collider>()) Destroy(component);
                    }

                    if (removeRigidbody)
                    {
                        foreach (var cj in rb.GetComponents<CharacterJoint>()) Destroy(cj);
                        Destroy(rb);
                    }
                    else
                        rb.isKinematic = true;
                }
                else
                {
                    allSleepOrTimeout = false;
                }
            }

            if (allSleepOrTimeout)
            {
                onAllRigidbodiesSleep.Invoke();
                Destroy(this);
            }
        }
    }
}