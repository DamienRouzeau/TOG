using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buttoncover : MonoBehaviour
{
    private void OnTriggerExit(Collider other)
    {
        gameObject.transform.parent.gameObject.SendMessage("Released");
    }
}
