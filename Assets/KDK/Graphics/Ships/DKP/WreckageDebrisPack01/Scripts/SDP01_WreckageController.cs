using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDP01_WreckageController : MonoBehaviour {

	private Transform myTransform;

	public bool RandomizeDebris = false;

	public bool DebrisFloats = true;
	public float DriftSpeedMIN = -0.05f;
	public float DriftSpeedMAX = 0.05f;
	public float DriftRotateSpeedMIN = -1f;
	public float DriftRotateSpeedMAX = 1f;

	public GameObject[] DebrisClusterPrefabs;
	public float ClusterScatterDistanceMIN = 2.5f;
	public float ClusterScatterDistanceMAX = 5.5f;

	public bool GenerateDebrisClouds = true;
	public int NumberOfCloudsMIN = 1;
	public int NumberOfCloudsMAX = 4;
	public GameObject[] DebrisCloudPrefabs;
	private List<GameObject> debrisCloudGOs;

	public int NumberOfClusters = 0;
	private SDP01_ClusterRandomizer[] clusterScripts;

	// Debris Piece Variables
	public bool SpawnDebrisPieces = true;
	public int NumberDebrisPiecesMIN = 5;
	public int NumberDebrisPiecesMAX = 15;
	public float PieceDistanceMIN = 8.0f;
	public float PieceDistanceMAX = 20.0f;
	public float PieceScaleMIN = 0.5f;
	public float PieceScaleMAX = 1.5f;
	public GameObject[] DebrisPiecePrefabs;
	public Transform PiecePlacementTransform;

	public int DebrisPieceCount = 0;
	private List<GameObject> debrisPiecesGOList;

	private void DoSpawnDebrisPieces() {

		if (PiecePlacementTransform != null) {
			if (DebrisPiecePrefabs.Length > 0) {				
				Vector3 randomRotationEulers = new Vector3 (0, 0, 0);
				float randomDistanceFromCenter = 0;
				float randomScale = 0;
				int numberPiecesToSpawn = Random.Range (NumberDebrisPiecesMIN, NumberDebrisPiecesMAX);
				for (int i = 0; i < numberPiecesToSpawn; i++) {
					Vector3 piecePosition = Vector3.zero;
					Vector3 pieceRotationEulers = Vector3.zero;

					// Calculate Debris Piece Location and Rotation
					piecePosition = myTransform.position;
					PiecePlacementTransform.position = piecePosition;
					randomRotationEulers.x = Random.Range (-360.0f, 360.0f);
					randomRotationEulers.y = Random.Range (-360.0f, 360.0f);
					randomRotationEulers.z = Random.Range (-360.0f, 360.0f);
					randomDistanceFromCenter = Random.Range (PieceDistanceMIN, PieceDistanceMAX);
					PiecePlacementTransform.rotation = Quaternion.Euler (randomRotationEulers);
					PiecePlacementTransform.position += PiecePlacementTransform.forward * randomDistanceFromCenter;
					piecePosition = PiecePlacementTransform.position;
					randomRotationEulers.x = Random.Range (-360.0f, 360.0f);
					randomRotationEulers.y = Random.Range (-360.0f, 360.0f);
					randomRotationEulers.z = Random.Range (-360.0f, 360.0f);
					pieceRotationEulers = randomRotationEulers;
					randomScale = Random.Range (PieceScaleMIN, PieceScaleMAX);
					SpawnDebrisPiece (piecePosition, pieceRotationEulers, i, randomScale);
				}
				DebrisPieceCount = debrisPiecesGOList.Count;
			}
		}

	}
	private void SpawnDebrisPiece(Vector3 piecePosition, Vector3 pieceRotationEulers, int pieceNumber, float pieceScale) {
		int randomPieceIndex = Random.Range (0, DebrisPiecePrefabs.Length);
		GameObject newDebrisPiece = GameObject.Instantiate (DebrisPiecePrefabs [randomPieceIndex], piecePosition, Quaternion.Euler(pieceRotationEulers)) as GameObject;
		newDebrisPiece.name = "DebrisPiece_" + pieceNumber.ToString();
		newDebrisPiece.transform.parent = myTransform;
		newDebrisPiece.transform.localScale = new Vector3 (pieceScale, pieceScale, pieceScale);
		debrisPiecesGOList.Add (newDebrisPiece);
	}

	// Debris Piece Variables
	public bool SpawnDebrisQuadPieces = true;
	public int NumberDebrisQuadPiecesMIN = 5;
	public int NumberDebrisQuadPiecesMAX = 15;
	public float QuadPieceDistanceMIN = 8.0f;
	public float QuadPieceDistanceMAX = 20.0f;
	public float QuadPieceScaleMIN = 0.5f;
	public float QuadPieceScaleMAX = 1.5f;
	public GameObject[] DebrisQuadPiecePrefabs;

	public int DebrisQuadPieceCount = 0;
	private List<GameObject> debrisQuadPiecesGOList;

	private void DoSpawnDebrisQuadPieces() {

		if (PiecePlacementTransform != null) {
			if (DebrisQuadPiecePrefabs.Length > 0) {				
				Vector3 randomRotationEulers = new Vector3 (0, 0, 0);
				float randomDistanceFromCenter = 0;
				float randomScale = 0;
				int numberPiecesToSpawn = Random.Range (NumberDebrisQuadPiecesMIN, NumberDebrisQuadPiecesMAX);
				for (int i = 0; i < numberPiecesToSpawn; i++) {
					Vector3 pieceQuadPosition = Vector3.zero;
					Vector3 pieceQuadRotationEulers = Vector3.zero;

					// Calculate Debris Piece Location and Rotation
					pieceQuadPosition = myTransform.position;
					PiecePlacementTransform.position = pieceQuadPosition;
					randomRotationEulers.x = Random.Range (-360.0f, 360.0f);
					randomRotationEulers.y = Random.Range (-360.0f, 360.0f);
					randomRotationEulers.z = Random.Range (-360.0f, 360.0f);
					randomDistanceFromCenter = Random.Range (PieceDistanceMIN, PieceDistanceMAX);
					PiecePlacementTransform.rotation = Quaternion.Euler (randomRotationEulers);
					PiecePlacementTransform.position += PiecePlacementTransform.forward * randomDistanceFromCenter;
					pieceQuadPosition = PiecePlacementTransform.position;
					randomRotationEulers.x = Random.Range (-360.0f, 360.0f);
					randomRotationEulers.y = Random.Range (-360.0f, 360.0f);
					randomRotationEulers.z = Random.Range (-360.0f, 360.0f);
					pieceQuadRotationEulers = randomRotationEulers;
					randomScale = Random.Range (QuadPieceScaleMIN, QuadPieceScaleMAX);
					SpawnDebrisQuadPiece (pieceQuadPosition, pieceQuadRotationEulers, i, randomScale);
				}
				DebrisQuadPieceCount = debrisQuadPiecesGOList.Count;
			}
		}

	}
	private void SpawnDebrisQuadPiece(Vector3 piecePosition, Vector3 pieceRotationEulers, int pieceNumber, float pieceScale) {
		int randomQuadPieceIndex = Random.Range (0, DebrisQuadPiecePrefabs.Length);
		GameObject newDebrisQuadPiece = GameObject.Instantiate (DebrisQuadPiecePrefabs [randomQuadPieceIndex], piecePosition, Quaternion.Euler(pieceRotationEulers)) as GameObject;
		newDebrisQuadPiece.name = "DebrisQuadPiece_" + pieceNumber.ToString();
		newDebrisQuadPiece.transform.parent = myTransform;
		newDebrisQuadPiece.transform.localScale = new Vector3 (pieceScale, pieceScale, pieceScale);
		debrisQuadPiecesGOList.Add (newDebrisQuadPiece);
	}

	// Use this for initialization
	void Start () {
		myTransform = gameObject.transform;

		// Create Debris Clusters
		Vector3 clusterLocalPosition = Vector3.zero;
		Vector3 clusterLocalRotationEulers = Vector3.zero;
		if (DebrisClusterPrefabs.Length > 0) {
			for (int i = 0; i < DebrisClusterPrefabs.Length; i++) {
				if (Random.Range (0, 100) < 35) {
					float randomMoveAmountX = Random.Range (-10.0f, 10f);
					float randomMoveAmountY = Random.Range (-10.0f, 10f);
					float randomMoveAmountZ = Random.Range (-10.0f, 10f);
					float randomRotationAmountX = Random.Range (-180f, 180f);
					float randomRotationAmountY = Random.Range (-180f, 180f);
					float randomRotationAmountZ = Random.Range (-180f, 180f);
					clusterLocalPosition = new Vector3 (randomMoveAmountX, randomMoveAmountY, randomMoveAmountZ);
					clusterLocalRotationEulers = new Vector3 (randomRotationAmountX, randomRotationAmountY, randomRotationAmountZ);
					GenerateDebrisCluster (i, clusterLocalPosition, clusterLocalRotationEulers);
				}
			}
		}
		RandomizeDebris = true;

		// Store Debris Cluster Scripts
		clusterScripts = gameObject.GetComponentsInChildren<SDP01_ClusterRandomizer> ();
		NumberOfClusters = clusterScripts.Length;

		// Set Floating Flag
		if (DebrisFloats) {
			if (clusterScripts.Length > 0) {
				for (int i = 0; i < clusterScripts.Length; i++) {
					clusterScripts [i].DriftSpeedMIN = DriftSpeedMIN;
					clusterScripts [i].DriftSpeedMAX = DriftSpeedMAX;
					clusterScripts [i].DriftRotateSpeedMIN = DriftRotateSpeedMIN;
					clusterScripts [i].DriftRotateSpeedMAX = DriftRotateSpeedMAX;
					clusterScripts [i].ClusterFloats = true;
				}
			}
		}

		// Generate Debris Clouds
		if (GenerateDebrisClouds) {
			debrisCloudGOs = new List<GameObject> ();

			Vector3 cloudLocalPosition = Vector3.zero;
			Vector3 cloudLocalRotationEulers = Vector3.zero;
			if (DebrisCloudPrefabs.Length > 0) {
				int numberOfCloudsToGenerate = Random.Range (NumberOfCloudsMIN, NumberOfCloudsMAX);
				for (int i = 0; i < numberOfCloudsToGenerate; i++) {
					float randomMoveAmountX = Random.Range (-5.0f, 5f);
					float randomMoveAmountY = Random.Range (-5.0f, 5f);
					float randomMoveAmountZ = Random.Range (-5.0f, 5f);
					float randomRotationAmountX = Random.Range (-180f, 180f);
					float randomRotationAmountY = Random.Range (-180f, 180f);
					float randomRotationAmountZ = Random.Range (-180f, 180f);
					cloudLocalPosition = new Vector3 (randomMoveAmountX, randomMoveAmountY, randomMoveAmountZ);
					cloudLocalRotationEulers = new Vector3 (randomRotationAmountX, randomRotationAmountY, randomRotationAmountZ);
					GenerateDebrisCloud (i, cloudLocalPosition, cloudLocalRotationEulers);
				}
			}
		}

		// Spawn Debris Pieces
		if (SpawnDebrisPieces) {
			debrisPiecesGOList = new List<GameObject> ();
			DoSpawnDebrisPieces ();
		}

		// Spawn Debris Quad Pieces
		if (SpawnDebrisQuadPieces) {
			debrisQuadPiecesGOList = new List<GameObject> ();
			DoSpawnDebrisQuadPieces ();
		}
	}
	
	// Update is called once per frame
	void Update () {
		// Randomize Debris Field
		if (RandomizeDebris) {

			// Randomize Debris Clouds
			if (GenerateDebrisClouds) {
				if (debrisCloudGOs.Count > 0) {
					for (int i = 0; i < debrisCloudGOs.Count; i++) {
						Destroy (debrisCloudGOs [i]);
					}
					debrisCloudGOs.Clear ();
				}

				// Generate New Debris Clouds
				Vector3 cloudLocalPosition = Vector3.zero;
				Vector3 cloudLocalRotationEulers = Vector3.zero;
				if (DebrisCloudPrefabs.Length > 0) {
					int numberOfCloudsToGenerate = Random.Range (NumberOfCloudsMIN, NumberOfCloudsMAX);
					for (int i = 0; i < numberOfCloudsToGenerate; i++) {
						float randomMoveAmountX = Random.Range (-5.0f, 5f);
						float randomMoveAmountY = Random.Range (-5.0f, 5f);
						float randomMoveAmountZ = Random.Range (-5.0f, 5f);
						float randomRotationAmountX = Random.Range (-180f, 180f);
						float randomRotationAmountY = Random.Range (-180f, 180f);
						float randomRotationAmountZ = Random.Range (-180f, 180f);
						cloudLocalPosition = new Vector3 (randomMoveAmountX, randomMoveAmountY, randomMoveAmountZ);
						cloudLocalRotationEulers = new Vector3 (randomRotationAmountX, randomRotationAmountY, randomRotationAmountZ);
						GenerateDebrisCloud (i, cloudLocalPosition, cloudLocalRotationEulers);
					}
				}
			}

			// Randomize Debris Field Objects
			Vector3 clusterLocalPosition = Vector3.zero;
			Vector3 clusterLocalRotationEulers = Vector3.zero;
			if (clusterScripts.Length > 0) {
				for (int i = 0; i < clusterScripts.Length; i++) {
					float randomMoveAmountX = Random.Range (-10.0f, 10f);
					float randomMoveAmountY = Random.Range (-10.0f, 10f);
					float randomMoveAmountZ = Random.Range (-10.0f, 10f);
					float randomRotationAmountX = Random.Range (-180f, 180f);
					float randomRotationAmountY = Random.Range (-180f, 180f);
					float randomRotationAmountZ = Random.Range (-180f, 180f);
					clusterLocalPosition = new Vector3 (randomMoveAmountX, randomMoveAmountY, randomMoveAmountZ);
					clusterLocalRotationEulers = new Vector3 (randomRotationAmountX, randomRotationAmountY, randomRotationAmountZ);
					clusterScripts [i].gameObject.transform.localPosition = clusterLocalPosition;
					clusterScripts [i].gameObject.transform.localRotation = Quaternion.Euler(clusterLocalRotationEulers);
					clusterScripts [i].RandomizePositionAndRotation (ClusterScatterDistanceMIN, ClusterScatterDistanceMAX);
				}
			}

			// Spawn Debris Pieces
			if (SpawnDebrisPieces) {
				if (debrisPiecesGOList.Count > 0) {
					for (int i = 0; i < debrisPiecesGOList.Count; i++) {
						Destroy (debrisPiecesGOList [i]);
					}
					debrisPiecesGOList.Clear ();
				}
				DoSpawnDebrisPieces ();
			}

			RandomizeDebris = false;
		}
	}

	private void GenerateDebrisCluster(int debrisIndex, Vector3 debrisLocalPosition, Vector3 clusterLocalRotationEulers) {
		GameObject newDebrisCluster = GameObject.Instantiate (DebrisClusterPrefabs [debrisIndex], myTransform.position, myTransform.rotation) as GameObject;
		newDebrisCluster.name = "DebrisCluster_" + debrisIndex.ToString();
		newDebrisCluster.transform.parent = myTransform;
		// Position and Rotate Debris Cluster
		newDebrisCluster.transform.localPosition = debrisLocalPosition;
		newDebrisCluster.transform.localRotation = Quaternion.Euler(clusterLocalRotationEulers);
	}

	private void GenerateDebrisCloud(int cloudIndex, Vector3 cloudLocalPosition, Vector3 cloudLocalRotationEulers) {
		int randomDebrisCloudToGenerate = Random.Range (0, DebrisCloudPrefabs.Length);
		GameObject newDebrisCloud = GameObject.Instantiate (DebrisCloudPrefabs [randomDebrisCloudToGenerate], myTransform.position, myTransform.rotation) as GameObject;
		newDebrisCloud.name = "DebrisCloud_" + cloudIndex.ToString();
		newDebrisCloud.transform.parent = myTransform;
		// Position and Rotate Debris Cloud
		newDebrisCloud.transform.localPosition = cloudLocalPosition;
		newDebrisCloud.transform.localRotation = Quaternion.Euler(cloudLocalRotationEulers);
		debrisCloudGOs.Add (newDebrisCloud);
	}
}
