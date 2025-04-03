using System;
using System.Collections;
using UnityEngine;

// Rayfire classes
namespace RayFire
{ 
    [Serializable]
    public class RFActivation
    {
        [Space(2)]
        [Tooltip("Inactive object will be activated when it's velocity will be higher than By Velocity value when pushed by other dynamic objects.")]
        public float byVelocity;
        
        [Space(1)]
        [Tooltip("Inactive object will be activated if will be pushed from it's original position farther than By Offset value.")]
        public float byOffset;

        [Space(1)]
        [Tooltip("Inactive object will be activated if will get total damage higher than this value.")]
        public float byDamage;
        
        [Space(1)]
        [Tooltip("Inactive object will be activated by overlapping with object with RayFire Activator component.")]
        public bool byActivator;
        
        [Space(1)]
        [Tooltip("Inactive object will be activated when it will be shot by RayFireGun component.")]
        public bool byImpact;
        
        [Space(1)]
        [Tooltip("Inactive object will be activated by Connectivity component if it will not be connected with Unyielding zone.")] 
        public bool byConnectivity;
       
        [Space(3)]
        public bool unyielding;        
        
        // Hidden
        [NonSerialized] public RayfireConnectivity connect;

        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////

        // Constructor
        public RFActivation()
        {
            byActivator = false;
            byImpact = false;
            byVelocity = 0f;
            byOffset = 0f;
            byDamage = 0f;
            byConnectivity = false;
            unyielding = false;
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////
        
        // Copy from
        public void CopyFrom(RFActivation act)
        {
            byActivator = act.byActivator;
            byImpact = act.byImpact;
            byVelocity = act.byVelocity;
            byOffset = act.byOffset;
            byDamage = act.byDamage;
            byConnectivity = act.byConnectivity;
            unyielding = act.unyielding;
        }

        // Turn of all activation properties
        public void TurnOff()
        {
            byActivator = false;
            byImpact = false;
            byVelocity = 0f;
            byOffset = 0f;
            byDamage = 0f;
            byConnectivity = false;
        }
        
        // Connectivity check
        public void CheckConnectivity()
        {
            if (byConnectivity == true && connect != null)
            {
                connect.checkNeed = true;
                connect = null;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Coroutines
        /// /////////////////////////////////////////////////////////
        
        // Check velocity for activation
        public IEnumerator ActivationVelocityCor (RayfireRigid scr)
        {
            while (byVelocity > 0)
            {
                if (scr.physics.rigidBody.velocity.magnitude > byVelocity)
                    scr.Activate();
                yield return null;
            }
        }

        // Check offset for activation
        public IEnumerator ActivationOffsetCor (RayfireRigid scr)
        {
            while (byOffset > 0)
            {
                if (Vector3.Distance (scr.transForm.position, scr.physics.birthPos) > byOffset)
                    scr.Activate();
                yield return null;
            }
        }
        
        // Exclude from simulation, move under ground, destroy
        public IEnumerator InactiveCor (RayfireRigid scr)
        {
            while (scr.simulationType == SimType.Inactive)
            {
                scr.physics.rigidBody.velocity        = Vector3.zero;
                scr.physics.rigidBody.angularVelocity = Vector3.zero;
                yield return null;
            }
        }
    }
}