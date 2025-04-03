using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	public class VRIKPlatform : MonoBehaviour {

		public VRIK ik = null;

        private Vector3 lastPosition;
		private Quaternion lastRotation = Quaternion.identity;

		void OnEnable() {
            lastPosition = transform.position;
			lastRotation = transform.rotation;
		}
		
		void LateUpdate () {
            if (ik == null)
                Debug.Log("VRIKPlatform on "+gameObject.name+" has no VRIK object attached.");
            else
            {
                // Adding the motion of this Transform to VRIK
                ik.solver.AddPlatformMotion(transform.position - lastPosition, transform.rotation * Quaternion.Inverse(lastRotation), transform.position);
            }

            lastRotation = transform.rotation;
			lastPosition = transform.position;
		}
	}
}
