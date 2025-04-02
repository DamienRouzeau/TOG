using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDP01_LookAtObject : MonoBehaviour {

	private Transform myTransform;
	public Transform TargetToLookAt;

	// Use this for initialization
	void Start () {
		myTransform = gameObject.transform;
	}
	
	// Update is called once per frame
	void Update () {
		if (TargetToLookAt != null) {
			myTransform.LookAt (TargetToLookAt.position);
		}
	}
}
