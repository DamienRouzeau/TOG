using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDP01_ClusterRandomizer : MonoBehaviour {

	private Transform myTransform;
	private Transform[] childTransforms;
	private Vector3[] originalLocalPositions;
	private Vector3[] originalLocalRotationEulers;

	// Random Cluster Drifts and Spins
	public bool ClusterFloats = false;
	private bool clusterFloatSetup = false;

	public float DriftSpeedMIN = 0;
	public float DriftSpeedMAX = 0;
	public float DriftRotateSpeedMIN = 0;
	public float DriftRotateSpeedMAX = 0;

	private List<SDP01_DriftObject> objectsToDrift;

	// Use this for initialization
	void Awake () {

		myTransform = gameObject.transform;
		childTransforms = gameObject.GetComponentsInChildren<Transform> ();

		// Store Original Positions and Rotations
		originalLocalPositions = new Vector3[childTransforms.Length];
		originalLocalRotationEulers = new Vector3[childTransforms.Length];
		if (childTransforms.Length > 0) {
			for (int i = 0; i < childTransforms.Length; i++) {
				originalLocalPositions [i] = childTransforms [i].localPosition;
				originalLocalRotationEulers [i] = childTransforms [i].localRotation.eulerAngles;
			}
		}
	}

	public void RandomizePositionAndRotation(float scatterDistanceMIN, float scatterDistanceMAX) {
		// Reset To Orignial Positions and Rotations
		if (childTransforms.Length > 0) {
			for (int i = 0; i < childTransforms.Length; i++) {
				childTransforms [i].localPosition = originalLocalPositions [i];
				childTransforms [i].localRotation = Quaternion.Euler(originalLocalRotationEulers [i]);
			}
		}

		// Randomize Positions and Rotations of Transform Cluster
		Vector3 objectLocalPosition = Vector3.zero;
		Vector3 objectLocalRotationEulers = Vector3.zero;
		if (childTransforms.Length > 0) {
			for (int i = 0; i < childTransforms.Length; i++) {
				// Randomize Debris Cluster Object
				objectLocalPosition = childTransforms[i].localPosition;
				objectLocalRotationEulers = childTransforms [i].localRotation.eulerAngles;

				float randomMoveAmount = Random.Range (scatterDistanceMIN, scatterDistanceMAX);
				float randomRotationAmountX = Random.Range (-75f, 75f);
				float randomRotationAmountY = Random.Range (-75f, 75f);
				float randomRotationAmountZ = Random.Range (-75f, 75f);
				Vector3 directionToCenter = childTransforms [i].position - myTransform.position;
				objectLocalPosition += directionToCenter * randomMoveAmount;
				objectLocalRotationEulers = new Vector3 (randomRotationAmountX, randomRotationAmountY, randomRotationAmountZ);
				childTransforms [i].localPosition = objectLocalPosition;
				childTransforms [i].localRotation = Quaternion.Euler (objectLocalRotationEulers);
			}
		}
	}

	// Update is called once per frame
	void Update () {
		if (ClusterFloats) {
			// Random Object Drifts and Spins
			if (!clusterFloatSetup) {
				// Gather Random Objects to Drift
				objectsToDrift = new List<SDP01_DriftObject> ();
				if (childTransforms.Length > 0) {
					for (int i = 0; i < childTransforms.Length; i++) {
						if (Random.Range (0, 100) < 35) {
							SDP01_DriftObject driftScript = childTransforms [i].gameObject.AddComponent<SDP01_DriftObject> ();
							if (driftScript != null) {
								objectsToDrift.Add (driftScript);
							}
						}
					}
				}
				// Setup Drift Objects
				if (objectsToDrift.Count > 0) {
					for (int i = 0; i < objectsToDrift.Count; i++) {
						objectsToDrift [i].DriftSpeedX = Random.Range(DriftSpeedMIN, DriftSpeedMAX);
						objectsToDrift [i].DriftSpeedY = Random.Range(DriftSpeedMIN, DriftSpeedMAX);
						objectsToDrift [i].DriftSpeedZ = Random.Range(DriftSpeedMIN, DriftSpeedMAX);
						objectsToDrift [i].DriftRotateSpeedX = Random.Range(DriftRotateSpeedMIN, DriftRotateSpeedMAX);
						objectsToDrift [i].DriftRotateSpeedY = Random.Range(DriftRotateSpeedMIN, DriftRotateSpeedMAX);
						objectsToDrift [i].DriftRotateSpeedZ = Random.Range(DriftRotateSpeedMIN, DriftRotateSpeedMAX);
						objectsToDrift [i].Setup = true;
					}
				}
				clusterFloatSetup = true;
			}
		}
	}
}
