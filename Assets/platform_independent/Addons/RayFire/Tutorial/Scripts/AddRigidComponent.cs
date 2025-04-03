using UnityEngine;
using RayFire;

public class AddRigidComponent : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
		RayfireRigid rigidComponent = gameObject.AddComponent<RayfireRigid>();
		rigidComponent.simulationType = SimType.Sleeping;
		rigidComponent.demolitionType = DemolitionType.Runtime;
		rigidComponent.objectType = ObjectType.Mesh;
		rigidComponent.Initialize();
	}
	
	// Update is called once per frame
	void Update () {
		// Button press
	}
}
