using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDP01_DerelictStarship : MonoBehaviour {
	
	private Renderer[] myRenderers;

	public int RandomMatChangePer = 20;
	public Material[] OriginalMaterials;
	public Material[] AltMaterials;

	// Use this for initialization
	void Start () {
		myRenderers = gameObject.GetComponentsInChildren<Renderer> ();

		RandomizeMaterials ();
	}

	private void RandomizeMaterials() {
		if (myRenderers.Length > 0) {
			for (int i = 0; i < myRenderers.Length; i++) {
				if (Random.Range (0, 100) < RandomMatChangePer) {
					// Change Material
					for (int j = 0; j < OriginalMaterials.Length; j++) {
						if (myRenderers [i].sharedMaterial == OriginalMaterials [j]) {
							Debug.Log ("Changing Material.");
							myRenderers [i].sharedMaterial = AltMaterials [j];
						}
					}
				}
			}
		}
	}

}
