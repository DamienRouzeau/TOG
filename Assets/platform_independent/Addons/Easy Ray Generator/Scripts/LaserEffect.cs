using UnityEngine;
using System.Collections;

public class LaserEffect : MonoBehaviour {

	// PUBLIC

	// PRIVATE
	private Material myMaterial;
	private LineRenderer myLinerender;

	void Awake ()
	{
		// set the references
		myLinerender = GetComponent<LineRenderer>();
		myMaterial = myLinerender.material;
	}

	/// <summary>The method set the lenght of the beam.</summary>
	/// <param name="value">The length in units</param>
	public void SetLength(float value)
	{
		myMaterial.SetTextureScale("_MainTex",new Vector2(value - transform.localPosition.z,1f) );
		myLinerender.SetPosition(1,new Vector3(0f,0f,value - transform.localPosition.z));
	}

	void Update()
	{
		// shift the texture every frame
		Vector2 offset = myMaterial.GetTextureOffset("_MainTex");
		offset.x -= Time.deltaTime * 5f;
		myMaterial.SetTextureOffset("_MainTex",offset);


	}

}
