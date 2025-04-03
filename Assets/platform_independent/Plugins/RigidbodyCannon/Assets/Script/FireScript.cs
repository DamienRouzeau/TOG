using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireScript : MonoBehaviour
{
    [SerializeField] private Transform aimer;
    [SerializeField] private float spinSpeed;
    [SerializeField] private ProjectileCannon cannonScript;
    private float angle;
    void Update()
    {
        angle += spinSpeed * Time.deltaTime;
        RaycastHit hit;
        Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(screenRay,out hit, 100))
        {
           aimer.position = hit.point;
           aimer.forward = hit.normal;
           aimer.localEulerAngles = new Vector3(aimer.localEulerAngles.x,aimer.localEulerAngles.y,angle);
        }
        
        //if(Input.GetMouseButtonDown(0))cannonScript.FireCannon();
        
        //if(Input.GetKeyDown(KeyCode.R))cannonScript.ChangeFireMode();
    }
}
