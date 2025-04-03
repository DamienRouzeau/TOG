using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class detectobjecttouch : MonoBehaviour
{
    public string neededhandobject = "";
    public string removefromhand = "";
    public bool needgrabtoactivate = false;

    
    GameObject makesureIleft = null;
    float timesinceIleft = 0.0f;

    private void Update()
    {
        if (makesureIleft != null)
        {
            timesinceIleft -= Time.deltaTime;
            if (timesinceIleft < 0.0f)
            {
                pointfromhand.noputback = 0.0f;
                pointfromhand.noputbackobject = null;
                timesinceIleft = 0;
                makesureIleft = null;
            }
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<attachobject>())
        {
            if (other.gameObject == pointfromhand.noputbackobject)
            {
                makesureIleft = other.gameObject;
                timesinceIleft = gamesettings_ui.myself.dagger_collision_exit_delay;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetComponent<attachobject>())
        {
            makesureIleft = null;
            if (pointfromhand.noputback > 0.0f) return;

            GameObject son = gameObject.FindInChildren(other.name);
            if (son)
            {
                Animator anim = Player.myplayer.currentskin.GetComponent<Animator>();

                pointfromhand pt = Player.pointright;
                bool left = false;
                if (other.gameObject.transform.parent.name.Contains(" L "))
                {
                    left = true;
                    pt = Player.pointleft;
                }

                if ((needgrabtoactivate) && (!pt.pinchstate_trigger)) return;
                if ((!needgrabtoactivate) && (pt.pinchstate)) return;

                bool validdetect = true;
                excluder [] allex = gameObject.GetComponentsInChildren<excluder>();
                foreach(excluder ex in allex)
                {
                    Collider exc = ex.gameObject.GetComponent<Collider>();
                    if (exc.bounds.Contains(other.bounds.center))
                    {
                        validdetect = false;
                    }
                }
                if (validdetect)
                {
                    if (!left)
                    {
                        anim.ResetTrigger("Weapon_R");
                        anim.ResetTrigger("Handle_R");
                        anim.ResetTrigger("Hand_Interaction_R");
                        anim.SetTrigger("Idle_R");
                    }
                    else
                    {
                        anim.ResetTrigger("Weapon");
                        anim.ResetTrigger("Handle");
                        anim.ResetTrigger("Hand_Interaction");
                        anim.SetTrigger("Idle");
                    }

                    attachobject ao = son.GetComponent<attachobject>();
                    ao.timeout = 0.2f;
                    /*
                    Vector3 vec = son.transform.position;
                    vec.x = other.transform.position.x;
                    vec.z = other.transform.position.z;
                    son.transform.position = vec;
                    */
                    son.transform.rotation = other.transform.rotation;
                    son.transform.position = other.transform.position;

                    other.gameObject.SetActive(false);
                    son.SetActive(true);
                }
            }
        }
    }
}
