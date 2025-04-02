using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDP01_StarshipGraveyard : MonoBehaviour {
	
	public GameObject DebrisFieldPrefab;
	public float scatterAmount = 50;
	public int NumberOfDebrisFieldsX = 4;
	public int NumberOfDebrisFieldsZ = 4;
	private int totalDebrisFieldCount = 0;

	public float OffsetMIN = -15;
	public float OffsetMAX = 15;

	private List<Vector3> possiblePositions;

	// Use this for initialization
	void Start () {
		// Generate Debris Fields
		possiblePositions = new List<Vector3>();
		totalDebrisFieldCount = NumberOfDebrisFieldsX * NumberOfDebrisFieldsZ;
		float startingX = -((float)(NumberOfDebrisFieldsX * scatterAmount) / 2);
		float startingZ = -((float)(NumberOfDebrisFieldsZ * scatterAmount) / 2);
		for (int x = 0; x < NumberOfDebrisFieldsX; x++) {
			for (int z = 0; z < NumberOfDebrisFieldsZ; z++) {
				Vector3 newPosition = new Vector3 (startingX + (x * scatterAmount), 0, startingZ + (z * scatterAmount));
				possiblePositions.Add (newPosition);
			}
		}
		for (int i = 0; i < totalDebrisFieldCount; i++) {
			GenerateDebrisField (possiblePositions[i], i);
		}
	}

	private void GenerateDebrisField (Vector3 fieldPosition, int debrisIndex) {
		fieldPosition.x += Random.Range (OffsetMIN, OffsetMAX);
		fieldPosition.y = Random.Range (OffsetMIN, OffsetMAX);
		fieldPosition.z += Random.Range (OffsetMIN, OffsetMAX);
		GameObject newDebrisField = GameObject.Instantiate (DebrisFieldPrefab, fieldPosition, Quaternion.identity) as GameObject;
		newDebrisField.name = "DebrisField_" + debrisIndex.ToString();
		newDebrisField.transform.parent = gameObject.transform;
	}

}
