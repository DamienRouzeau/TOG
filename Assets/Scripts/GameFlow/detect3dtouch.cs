using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class detect3dtouch : MonoBehaviour
{
    public enum DetectionMethod
	{
        Trigger,
        Distance
	}

    public bool touchme = false;
    public string neededhandobject = "";
    public string removefromhand = "";
    public bool needgrabtoactivate = false;
    public DetectionMethod detectionMethod = DetectionMethod.Trigger;
    public float distanceThreshold = 1f;


    private void Update()
    {
        if (touchme)
        {
            touchme = false;
            gameObject.transform.parent.gameObject.SendMessage("Touched", gameObject);
        }

        if (detectionMethod == DetectionMethod.Distance)
		{
            attachobject objTouched = null;
            attachobject obj = null;
            obj = Player.myplayer.GetAttachObjectOfPlace(Player.WeaponPlace.LeftHand);
            if (obj != null && (string.IsNullOrEmpty(neededhandobject) || obj.name == neededhandobject))
			{
                if (CheckDistance(transform, obj.transform, distanceThreshold))
                    objTouched = obj;
            }
            obj = Player.myplayer.GetAttachObjectOfPlace(Player.WeaponPlace.RightHand);
            if (obj != null && (string.IsNullOrEmpty(neededhandobject) || obj.name == neededhandobject))
            {
                if (CheckDistance(transform, obj.transform, distanceThreshold))
                    objTouched = obj;
            }

            if (objTouched != null)
			{
                gameObject.transform.parent.gameObject.SendMessage("Touched", gameObject, SendMessageOptions.DontRequireReceiver);
                if (objTouched.gameObject.name == removefromhand)
                {
                    objTouched.gameObject.SetActive(false);
                }
            }
		}
    }

    private bool CheckDistance(Transform tr1, Transform tr2, float distance)
	{
        Vector3 pos1 = tr1.position;
        Vector3 pos2 = tr2.position;
        float sqrMagnitude = (pos1 - pos2).sqrMagnitude;
        return sqrMagnitude < distance * distance;

	}

    private void OnTriggerExit(Collider other)
    {
        if (detectionMethod != DetectionMethod.Trigger) 
            return;

        if (gameObject.GetComponent<attachobject>() || gameObject.GetComponent<InventorySlot>())        // object that can be attached to hands
        {
            pointfromhand pth = other.gameObject.GetComponent<pointfromhand>();
            if (pth != null)
                pth.belttouched = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (detectionMethod != DetectionMethod.Trigger)
            return;

        //if (pointfromhand.noputback > 0.0f) return;

        if (gameObject.GetComponent<attachobject>() || gameObject.GetComponent<InventorySlot>())        // object that can be attached to hands
        {
            pointfromhand pth = other.gameObject.GetComponent<pointfromhand>();
            if (pth != null)
                pth.belttouched = true;
            other.gameObject.SendMessage("HandTouched", gameObject, SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            if (neededhandobject != "")     // Detect what object is in hand and compare
            {
                pointfromhand pnt = other.gameObject.GetComponent<pointfromhand>();
                if (pnt)
                {
                    bool validate = false;
                    if ((!needgrabtoactivate) && (!pnt.triggerstate)) validate = true;
                    if ((needgrabtoactivate) && (pnt.triggerstate_trigger)) validate = true;
                    //                    if ((!needgrabtoactivate) && (!pnt.pinchstate)) validate = true;
                    //                    if ((needgrabtoactivate) && (pnt.pinchstate_trigger)) validate = true;
                    if (validate)
                    {
                        foreach (attachobject obj in pnt.my_hand_objects)
                        {
                            if ((obj.gameObject.name == neededhandobject) && (obj.gameObject.activeInHierarchy))
                            {
                                // allowed to activate
                                if (obj.gameObject.name == removefromhand)
                                {
                                    obj.gameObject.SetActive(false);
                                }
                                gameObject.transform.parent.gameObject.SendMessage("Touched", gameObject, SendMessageOptions.DontRequireReceiver);
                            }
                        }
                    }
                }
            }
            else
            {
                gameObject.transform.parent.gameObject.SendMessage("Touched", gameObject, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}
