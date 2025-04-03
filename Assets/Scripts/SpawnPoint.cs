using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public bool playerInside = false;

    private void FixedUpdate()
    {
//        Debug.Log(playerInside);   
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "PlayerBody")
            playerInside = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "PlayerBody")
            playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "PlayerBody")
            playerInside = false;
    }
}
