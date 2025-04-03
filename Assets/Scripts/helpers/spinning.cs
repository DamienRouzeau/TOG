using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spinning : MonoBehaviour
{
    void Update()
    {
        Vector3 v = transform.localEulerAngles;
        v.z -= 36.0f * Time.fixedDeltaTime;
        transform.localEulerAngles = v;
    }
}
