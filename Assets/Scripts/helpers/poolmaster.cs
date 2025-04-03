using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class poolmaster : MonoBehaviour
{
    public bullet_tag[] mybullets;
    public impactparticles_tag[] myimpacts;

    private void Awake()
    {
        if (mybullets.Length == 0)
            mybullets = gameObject.GetComponentsInChildren<bullet_tag>(true);
        if (myimpacts.Length == 0)
            myimpacts = gameObject.GetComponentsInChildren<impactparticles_tag>(true);
    }

    void Start()
    {
        mybullets = gameObject.GetComponentsInChildren<bullet_tag>(true);
        myimpacts = gameObject.GetComponentsInChildren<impactparticles_tag>(true);
    }

    public void ReStart()
    {
        Start();
    }
}
