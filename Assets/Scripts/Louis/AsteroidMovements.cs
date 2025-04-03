using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidMovements : MonoBehaviour
{
    void Update()
    {
        Vector3 euler = transform.eulerAngles;
        euler.z = Random.Range(-10f, 10f);
        euler.y = Random.Range(0f, 360f);
        euler.x = Random.Range(-10f, 10f);
        transform.eulerAngles = euler * Time.deltaTime;
    }
}