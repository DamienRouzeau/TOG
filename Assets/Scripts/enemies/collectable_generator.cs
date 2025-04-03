using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collectable_generator : MonoBehaviour
{
    public bool hidewhencollissioned = true;
    Health health;
    int validcounter = 0;

    void Awake()
    {
        health = gameObject.GetComponent<Health>();
        if (hidewhencollissioned)
            health.reactionanimations.SetTrigger("Reveal");
        else
            health.reactionanimations.SetTrigger("Hidden");
    }

    public void ZoneEntered(GameObject who)
    {
        validcounter++;
        if (!hidewhencollissioned)
            health.reactionanimations.SetTrigger("Reveal");
        else
            health.reactionanimations.SetTrigger("Hidden");
    }

    public void ZoneLeft(GameObject who)
    {
        validcounter--;
        if (validcounter < 0) validcounter = 0;
        if (validcounter == 0)
        {
            if (hidewhencollissioned)
                health.reactionanimations.SetTrigger("Reveal");
            else
                health.reactionanimations.SetTrigger("Hidden");
        }
    }
}
