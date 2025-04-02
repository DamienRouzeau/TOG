using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDP01_DebrisQuad : MonoBehaviour {

	private Transform myTransform;
	public Transform Quad2Transform;

	// Use this for initialization
	void Start () {
		// Randomize Rotation of Quads
		myTransform = gameObject.transform;
		Vector3 randomRotationEulers = Vector3.zero;
		randomRotationEulers.x = Random.Range (-360.0f, 360.0f);
		randomRotationEulers.y = Random.Range (-360.0f, 360.0f);
		randomRotationEulers.z = Random.Range (-360.0f, 360.0f);
		myTransform.rotation = Quaternion.Euler (randomRotationEulers);
		randomRotationEulers.x = Random.Range (-360.0f, 360.0f);
		randomRotationEulers.y = Random.Range (-360.0f, 360.0f);
		randomRotationEulers.z = Random.Range (-360.0f, 360.0f);
		if (Quad2Transform != null) {			
			Quad2Transform.localRotation = Quaternion.Euler (randomRotationEulers);
			float randomScale = Random.Range (0.75f, 1.25f);
			Quad2Transform.localScale = myTransform.localScale * randomScale;
		}
	}
}
