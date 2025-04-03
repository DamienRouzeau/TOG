using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class dagger_stack : MonoBehaviour
{
    public bool leftteam = false;

    public GameObject[] daggers;

    void Awake()
    {
        foreach(GameObject dag in daggers)
            dag.SetActive(false);
    }

    void Update()
    {
        long[] reference = gameflowmultiplayer.teamlistA;
        if (!leftteam)
            reference = gameflowmultiplayer.teamlistB;

        for (int i=0;i<gameflowmultiplayer.nrplayersperteam;i++)
        {
            if (reference[i]!= -1)
            {
                if (reference[i] == PhotonNetworkController.GetPlayerId())
                {
                    if (daggers[i].activeInHierarchy)
                        daggers[i].SetActive(false);
                }
                else
                {
                    if (!daggers[i].activeInHierarchy)
                        daggers[i].SetActive(true);
                }
            }
            else
            {
                if (daggers[i].activeInHierarchy)
                    daggers[i].SetActive(false);
            }
        }
    }
}
