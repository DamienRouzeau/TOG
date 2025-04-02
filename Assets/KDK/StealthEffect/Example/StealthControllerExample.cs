using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StealthControllerExample : MonoBehaviour {


	Material stealthMaterial;

	float stealth;

	// Use this for initialization

	void Start () {
		stealthMaterial = GetComponent<Renderer> ().material;

	}
			
	// Update is called once per frame
	void Update () {

		if (Input.GetButtonDown ("Jump")) {
			StopAllCoroutines ();
			StartCoroutine ("StealthIn");
		}
	}



	IEnumerator StealthIn() {
		stealth = 0;
		while(stealth < 1 ){
			stealth += 0.5f * Time.deltaTime;
			stealthMaterial.SetFloat ("_Stealth",stealth);
			yield return null;
		}
	}
}
