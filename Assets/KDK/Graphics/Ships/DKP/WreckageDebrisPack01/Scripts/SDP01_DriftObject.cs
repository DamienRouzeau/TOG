using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDP01_DriftObject : MonoBehaviour {

	private Transform myTransform;

	public bool Setup = false;
	public float DriftSpeedX = 0;
	public float DriftSpeedY = 0;
	public float DriftSpeedZ = 0;
	public float DriftRotateSpeedX = 0;
	public float DriftRotateSpeedY = 0;
	public float DriftRotateSpeedZ = 0;

	private Vector3 currentLocalPosition;
	private Vector3 currentLocalRotationEulers;

	// Use this for initialization
	void Start () {
		myTransform = gameObject.transform;
		currentLocalPosition = myTransform.localPosition;
		currentLocalRotationEulers = myTransform.localRotation.eulerAngles;
	}
	
	// Update is called once per frame
	void Update () {
		// Drift and Spin Object
		if (Setup) {
			currentLocalPosition.x += DriftSpeedX * Time.deltaTime;
			currentLocalPosition.y += DriftSpeedY * Time.deltaTime;
			currentLocalPosition.z += DriftSpeedZ * Time.deltaTime;
			currentLocalRotationEulers.x += DriftRotateSpeedX * Time.deltaTime;
			currentLocalRotationEulers.y += DriftRotateSpeedY * Time.deltaTime;
			currentLocalRotationEulers.z += DriftRotateSpeedZ * Time.deltaTime;
			myTransform.localPosition = currentLocalPosition;
			myTransform.localRotation = Quaternion.Euler(currentLocalRotationEulers);
		}
	}
}
