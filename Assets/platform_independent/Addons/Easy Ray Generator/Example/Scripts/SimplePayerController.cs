using UnityEngine;
using System.Collections;

public class SimplePayerController : MonoBehaviour {

	// PUBLIC
	public float walkSpeed = 5f;
	public float angualrSpeed = 5f;

	public Transform laserTrans;
	public Transform cameraPivot;

	public LaserControl laserControl;

	// PRIVATE
	private Vector3 prevMousePos;

	void Start()
	{
		prevMousePos = Input.mousePosition;
	}

	void FixedUpdate ()
	{
		// *****************************
		// move the player with keyboard
		// *****************************
		if(Input.GetKey(KeyCode.W))
		{
			// FORWARD
			transform.position = transform.position + (transform.forward * walkSpeed * Time.deltaTime);
		}

		if(Input.GetKey(KeyCode.S))
		{
			// BACKWARD
			transform.position = transform.position - (transform.forward * walkSpeed * Time.deltaTime);
		}

		if(Input.GetKey(KeyCode.A))
		{
			// LEFT
			transform.position = transform.position - (transform.right * walkSpeed * Time.deltaTime);
		}

		if(Input.GetKey(KeyCode.D))
		{
			// LEFT
			transform.position = transform.position + (transform.right * walkSpeed * Time.deltaTime);
		}

		// **************************
		// look around with the mouse
		// **************************
		transform.localEulerAngles = new Vector3(0f,
			transform.localEulerAngles.y - (prevMousePos - Input.mousePosition).x * Time.deltaTime * angualrSpeed,
			0f);

		cameraPivot.localEulerAngles = new Vector3(cameraPivot.localEulerAngles.x - (prevMousePos - Input.mousePosition).y * Time.deltaTime * angualrSpeed,
			0f,
			0f);

		// **************************
		// rotate the laser with the mouse
		// **************************
		laserTrans.localEulerAngles = new Vector3(laserTrans.localEulerAngles.x - (prevMousePos - Input.mousePosition).y * Time.deltaTime * angualrSpeed,
			0f,
			0f);

		prevMousePos = Input.mousePosition;

		// **************************
		// activate laser pressing Space
		// **************************
		if(Input.GetKeyDown(KeyCode.Space))
			laserControl.Activate = true;
		
		if(Input.GetKeyUp(KeyCode.Space))
			laserControl.Activate = false;

	}
}
