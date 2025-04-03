using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using MiniJSON;

public class MultiPlayer_Helper : MonoBehaviour
{
    public string uniqueidentifier = "";
    MultiPlayer_Tag mytag = null;
    PhotonTransformMPObject mpo = null;

    void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            PhotonView pV = transform.GetComponent<PhotonView>();
            object[] data = pV.InstantiationData;
            uniqueidentifier = (string)data[0];
        }
        MultiPlayer_Tag[] alltags = GameObject.FindObjectsOfType<MultiPlayer_Tag>();
        foreach (MultiPlayer_Tag tag in alltags)
        {
            if (uniqueidentifier == tag.uniqueidentifier)
            {
                mytag = tag;
                transform.SetParent(tag.gameObject.transform);
                mpo = gameObject.GetComponent<PhotonTransformMPObject>();
                mpo.rotationstoapply = mytag.linkobjects;
                mpo.m_Angles = new float[mpo.rotationstoapply.Length];
                mpo.m_NetworkRotations = new Quaternion[mpo.rotationstoapply.Length];
                mpo.dataready = true;
            }
        }
    }

    public static bool InitiateMultiplayer(GameObject father)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            string unique = father.GetComponent<MultiPlayer_Tag>().uniqueidentifier;
            object[] instanceData = new object[1];
            instanceData[0] = unique;
            GameObject dummyobject = PhotonNetwork.Instantiate("MultiPlayer_Helper", father.transform.position, father.transform.rotation,0, instanceData);
            MultiPlayer_Helper mph = dummyobject.GetComponent<MultiPlayer_Helper>();
            mph.uniqueidentifier = unique;
            return false;
        }
        return true;        // 
    }
}
