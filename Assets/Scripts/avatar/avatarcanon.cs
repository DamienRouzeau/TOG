using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class avatarcanon : MonoBehaviour
{
    public string canonid = "";
    public int team = 0;

    void Start()
    {
        GameObject obj = gameObject;
        boat_followdummy fd = obj.GetComponent<boat_followdummy>();
        while (fd == null)
        {
            if (obj.transform.parent == null) break;
            obj = obj.transform.parent.gameObject;
            fd = obj.GetComponent<boat_followdummy>();
        }
        if (fd != null)
            team = fd.team;
    }

    // Update is called once per frame
    void Update()
    {
/*
        if (team == 0)
        {
            if (canonid == "front")
            {
                Debug.Log("OBJECT LOCAL:" + transform.localRotation.x + " " + transform.localRotation.y + " " + transform.localRotation.z + " " + transform.localRotation.w + " ");
            }
        }
        */
    }
}
