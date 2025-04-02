using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDP01_DebrisCameraController : MonoBehaviour {

	private Transform myTransform;
	private Vector3 currentRotationEulers;
	private float currentYRotation = 0;

	public Transform CameraHeightTransform;
	private Vector3 heightEulers;

	public Transform CameraTransform;
	private Vector3 cameraLocalPosition;

	public Camera MainCamera;

	public float ZoomMIN = 5;
	public float ZoomMAX = 35;
	private float currentZoomLevel = -10;

	public float UpDownMIN = -90;
	public float UpDownMAX = 90;
	private float currentUpDownLevel = 0;

	public float LerpSpeed = 1.0f;

	public KeyCode ZoomOutKey = KeyCode.Q;
	public KeyCode ZoomInKey = KeyCode.E;

	public KeyCode RotateLeftKey = KeyCode.A;
	public KeyCode RotateRightKey = KeyCode.D;

	public KeyCode RotateUpKey = KeyCode.W;
	public KeyCode RotateDownKey = KeyCode.S;

	public float ZoomInOutSpeed = 15.0f;
	public float RotateUpDownSpeed = 10.0f;
	public float RotateLeftRightSpeed = 25.0f;

	// Use this for initialization
	void Start () {
		myTransform = gameObject.transform;
		if (CameraTransform != null) {
			cameraLocalPosition = CameraTransform.localPosition;
			currentYRotation = myTransform.rotation.eulerAngles.y;
			currentZoomLevel = Mathf.Abs(cameraLocalPosition.z);
			currentUpDownLevel = CameraHeightTransform.rotation.eulerAngles.x;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey (ZoomInKey)) {
			if (currentZoomLevel > ZoomMIN) {
				currentZoomLevel -= ZoomInOutSpeed * Time.deltaTime;
			}
		}
		if (Input.GetKey (ZoomOutKey)) {
			if (currentZoomLevel < ZoomMAX) {
				currentZoomLevel += ZoomInOutSpeed * Time.deltaTime;
			}
		}

		if (Input.GetKey (RotateLeftKey)) {
			currentYRotation += RotateLeftRightSpeed * Time.deltaTime;
		}
		if (Input.GetKey (RotateRightKey)) {
			currentYRotation -= RotateLeftRightSpeed * Time.deltaTime;
		}

		if (Input.GetKey (RotateDownKey)) {
			if (currentUpDownLevel > UpDownMIN) {
				currentUpDownLevel -= RotateUpDownSpeed * Time.deltaTime;
			}
		}
		if (Input.GetKey (RotateUpKey)) {
			if (currentUpDownLevel < UpDownMAX) {
				currentUpDownLevel += RotateUpDownSpeed * Time.deltaTime;
			}
		}

		// Update Camera Position and Rotation
		heightEulers = new Vector3(currentUpDownLevel, 0, 0);
		CameraHeightTransform.localRotation = Quaternion.LerpUnclamped (CameraHeightTransform.localRotation, Quaternion.Euler (heightEulers), LerpSpeed * Time.deltaTime);
		currentRotationEulers = new Vector3(0, currentYRotation, 0);
		myTransform.rotation = Quaternion.LerpUnclamped(myTransform.rotation, Quaternion.Euler(currentRotationEulers), LerpSpeed * Time.deltaTime);
		cameraLocalPosition.z = -currentZoomLevel;
		CameraTransform.localPosition = Vector3.LerpUnclamped(CameraTransform.localPosition, cameraLocalPosition, LerpSpeed * Time.deltaTime);
	}
}
