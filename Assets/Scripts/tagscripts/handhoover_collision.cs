using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class handhoover_collision : MonoBehaviour
{
    public GameObject hooverobject = null;

    private void Awake()
    {
        if (hooverobject)        hooverobject.SetActive(false);
    }

    void SetHandState(pointfromhand hand,bool state)
    {
        bool holding = false;
        foreach (attachobject handatt in hand.my_hand_objects)
        {
            if (handatt.gameObject.activeInHierarchy) holding = true;
        }
        if (!holding)
        {
            Animator anim = hand.player.currentskin.GetComponent<Animator>();
            if (hand.righthand)
            {
                anim.ResetTrigger("Weapon_R");
                anim.ResetTrigger("Handle_R");
                if (state)
                {
                    anim.ResetTrigger("Idle_R");
                    anim.SetTrigger("Hand_Interaction_R");
                }
                else
                {
                    anim.SetTrigger("Idle_R");
                    anim.ResetTrigger("Hand_Interaction_R");
                }
            }
            else
            {
                anim.ResetTrigger("Weapon");
                anim.ResetTrigger("Handle");
                if (state)
                {
                    anim.ResetTrigger("Idle");
                    anim.SetTrigger("Hand_Interaction");
                }
                else
                {
                    anim.SetTrigger("Idle");
                    anim.ResetTrigger("Hand_Interaction");
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        pointfromhand hand = other.gameObject.GetComponent<pointfromhand>();
        if (hand != null)
        {
            SetHandState(hand, true);
            if (hooverobject) hooverobject.SetActive(true);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        pointfromhand hand = other.gameObject.GetComponent<pointfromhand>();
        if (hand != null)
        {
            SetHandState(hand, false);
            if (hooverobject) hooverobject.SetActive(false);
        }
    }
}
