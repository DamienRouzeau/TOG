using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_Race_Map : MonoBehaviour
{
    [System.Serializable]
    public class Boat 
    {
        public GameObject root;
        public TMP_Text label;
    }

    public Vector3 vLocalOrigin;
    public Vector3 vLocalScale;
    public Boat[] boats;

    private boat_followdummy[] boatFollowDummys;

    private void Start()
    {
        boatFollowDummys = GameObject.FindObjectsOfType<boat_followdummy>();
    }

    private void Update()
    {
        int count = Mathf.Min(boats.Length, boatFollowDummys.Length);
        for( int i=0; i<count; i++ )
        {
            Vector3 vPos = ComputeMapLocalPosition(boatFollowDummys[i].transform.position);
            vPos.y = boats[i].root.transform.localPosition.y;
            boats[i].root.transform.localPosition = vPos;
            float fYAngle = boatFollowDummys[i].transform.rotation.eulerAngles.y + 90f;
            boats[i].root.transform.localRotation = Quaternion.Euler(0f, fYAngle, 0f);
        }
    }

    private Vector3 ComputeMapLocalPosition( Vector3 vWorldPos )
    {
        Vector3 vLocal = new Vector3(vWorldPos.x / vLocalScale.x, 0f, vWorldPos.z / vLocalScale.z);
        vLocal += vLocalOrigin;
        return vLocal;
    }

}
