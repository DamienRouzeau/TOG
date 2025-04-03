using UnityEngine;
using ObjectRandomization;
using System.Collections;
using System.Collections.Generic;

public class ObjectRandomizer : MonoBehaviour {

	public List<ObjectInfo> objectInfos = new List<ObjectInfo>();

	// the total su of all weightings in the list
	float weightSum;

	// Use this for initialization
	void Awake () {
		weightSum = WeightSum();
		CheckForValidList();
		UpdateRanges();
	}

	// go through the list of prefabs and tell if there is something not assigned that should be
	void CheckForValidList() {
		// make sure that there are prefabs
		if (objectInfos.Count == 0) {
			Debug.LogError("ObjectRandomizer does not have any objects to choose from. Please assign at least one object.");
		}

		// go through all the prefabs
		int i = 1;
		foreach (ObjectInfo objectInfo in objectInfos) {
			if (objectInfo.obj == null) Debug.LogError("Element " + i + " in object list does not have an object reference assigned.");
			//if (prefabInfo.weighting == 0) Debug.LogWarning("Element " + i + " in prefab list has a weighting of 0.");
			i++;
		}
	}

	// return a random prefab
	public Object RandomObject () {
		// make sure that there is at least one object to choose from
		if (objectInfos.Count == 0) {
			Debug.LogError("ObjectRandomizer does not have any objects to choose from. Please assign at least one object.");
			return null;
		}

		// pick a random value within the range of all weights
		float r = Random.Range(0f, weightSum);
		foreach (ObjectInfo prefabInfo in objectInfos) {
			if (prefabInfo.RangeContains(r)) {
				return prefabInfo.obj;
			}
		}

		// if no objects assigned range has matched the random value, throw an error
		Debug.LogError("No object has a range that contains " + r + ". This should never happen.");
		return objectInfos[0].obj;
	}

	// assign a range to each of the prefabs
	void UpdateRanges () {
		float currentNumber = 0f;
		foreach(ObjectInfo prefabInfo in objectInfos) {
			prefabInfo.RangeStart = currentNumber;
			currentNumber += prefabInfo.weighting;
			prefabInfo.RangeEnd = currentNumber;
		}
	}

	// returns the sum of all weighting values of the prefabs
	public float WeightSum () {
		float weightSum = 0f;
		foreach (ObjectInfo objInfo in objectInfos) {
			weightSum += objInfo.weighting;
		}
		
		// catch the case of no weight
		if (weightSum == 0) {
			if (objectInfos.Count > 0) Debug.LogWarning("None of the listed objects have any weighting. Setting all weightings to 1.");
			foreach (ObjectInfo objInfo in objectInfos) {
				objInfo.weighting = 1f;
			}
			return objectInfos.Count; // never return zero
		}

		return weightSum;
	}
}
