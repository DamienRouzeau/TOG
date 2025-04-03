using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ignoreprojectilesfromchilds : MonoBehaviour
{
    public GameObject father = null;

    void Awake()
    {
        if (father == null)            father = gameObject;
    }
}
