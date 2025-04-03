using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class debug_collision : MonoBehaviour
{
    public GameObject enter = null;
    public GameObject exit = null;
    public GameObject stay = null;

    private void OnTriggerEnter(Collider other)
    {
        enter = other.gameObject;
    }
    private void OnTriggerExit(Collider other)
    {
        exit = other.gameObject;
    }
    private void OnTriggerStay(Collider other)
    {
        stay = other.gameObject;
    }
}
