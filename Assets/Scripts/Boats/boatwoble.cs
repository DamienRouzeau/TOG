using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boatwoble : MonoBehaviour
{
    public float multiplier = 1.0f;
    float xitem = 0.0f;
    float zitem = 0.0f;

    void Update()
    {
        xitem += 0.1f;
        zitem += 0.12f;
        Vector3 vec = gameObject.transform.eulerAngles;
        vec.x = Mathf.Sin(xitem) * multiplier;
        vec.z = Mathf.Sin(zitem) * multiplier;
        gameObject.transform.eulerAngles = vec;
    }

}
