using UnityEngine;
using System.Collections;

/// <summary>
/// Spawns GameObjects and throws them somewhere. Uses object references from the object randomizer.
/// </summary>

public class ObjectSpawner : MonoBehaviour {

	public ObjectRandomizer prefabRandomizer;
	public ObjectRandomizer normalMapRandomizer;

	public float force;
	public float maxTorque;
	public float directionVariance;

	public float spawnInterval;

	Transform myTransform;

	// Use this for initialization
	void Start () {
		myTransform = GetComponent<Transform>();
		if (spawnInterval > 0)
			InvokeRepeating("SpawnObject", spawnInterval, spawnInterval);
		else
			SpawnObject(); // Just spawn object one time
	}
	
	// Update is called once per frame
	//void Update () {
	//	if (Input.anyKeyDown) SpawnObject();
	//	if (Input.GetKeyDown(KeyCode.Escape)) CancelInvoke("SpawnObject");
	//}

	// spaws 
	void SpawnObject() {
		// get a random prefab from the object randomizer and instantiate it
		GameObject randomPrefab = (GameObject)prefabRandomizer.RandomObject();
		GameObject newObject = (GameObject)Instantiate(randomPrefab, transform.position, transform.rotation);

		// if the normal map randomizer is in use, change the normal map of the object
		if (normalMapRandomizer != null && normalMapRandomizer.isActiveAndEnabled) {
			Texture randomNormalMap = (Texture)normalMapRandomizer.RandomObject();
			newObject.GetComponent<Renderer>().material.SetTexture("_BumpMap", randomNormalMap);
		}

		// parent the object to the object spawner
		newObject.transform.parent = myTransform;

		// get the rigidbody
		Rigidbody newRigidBody = newObject.GetComponent<Rigidbody>();
		if (newRigidBody == null)
			return;

		// calculate force / direction vector to add
		Vector3 direction = Vector3.up;
		direction += new Vector3(Random.Range(-directionVariance, directionVariance),
		                                      0,
		                         Random.Range(-directionVariance, directionVariance));
		direction.Normalize();
		direction *= force;

		// calculate torque to add
		Vector3 torque = new Vector3(Random.Range (-maxTorque, maxTorque), Random.Range (-maxTorque, maxTorque), Random.Range (-maxTorque, maxTorque));

		// add force and torque
		newRigidBody.AddForce(direction);
		newRigidBody.AddTorque(torque);

	}
}
