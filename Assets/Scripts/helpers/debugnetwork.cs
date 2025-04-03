using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class debugnetwork : MonoBehaviour
{
    Text debugtextinfront = null;

    // Start is called before the first frame update
    void Awake()
    {
        debugtextinfront = GameObject.Find("debugtextinfront").GetComponent<Text>();

    }

    private void Update()
    {
        if (debugtextinfront != null)
        {
            debugtextinfront.text = "[" + Photon.Pun.PhotonNetwork.CurrentRoom.Name + "] " + Photon.Pun.PhotonNetwork.CountOfPlayers;
        }
    }
}
