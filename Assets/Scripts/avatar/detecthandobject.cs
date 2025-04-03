using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class detecthandobject : MonoBehaviour
{
    public enum _action
    {
        none,
        appearonce
    }

    public string handobject = "";
    public GameObject target;
    public _action whataction;


    void Update()
    {
        if (whataction != _action.none)
        {
            if (handobject != "")
            {
                switch (whataction)
                {
                    case _action.none:
                        break;
                    case _action.appearonce:
                        if ((Player.pointleft != null) && (Player.pointleft.my_hand_objects != null))
                        {
                            foreach (attachobject att in Player.pointleft.my_hand_objects)
                            {
                                if ((att.gameObject.activeInHierarchy) && (att.gameObject.name == handobject))
                                {
                                    target.SetActive(true);
                                    whataction = _action.none;
                                }
                            }
                        }
                        if ((Player.pointright != null) && (Player.pointright.my_hand_objects != null))
                        {
                            foreach (attachobject att in Player.pointright.my_hand_objects)
                            {
                                if ((att.gameObject.activeInHierarchy) && (att.gameObject.name == handobject))
                                {
                                    target.SetActive(true);
                                    whataction = _action.none;
                                }
                            }
                        }
                        break;

                }
            }
        }
    }
}
