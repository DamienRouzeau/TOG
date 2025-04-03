using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hands_filler : MonoBehaviour
{
    public bool actiononactivate = false;
    public string[] show_lefthand_items;
    public string[] show_righthand_items;
    public string[] show_leftbelt_items;
    public string[] show_rightbelt_items;

    public string[] hide_lefthand_items;
    public string[] hide_righthand_items;
    public string[] hide_leftbelt_items;
    public string[] hide_rightbelt_items;

    private void OnEnable()
    {
        if (actiononactivate) RunHandsFiller();
    }


    int CheckAttachedFather(GameObject obj)
    {
        if (obj.transform.parent.gameObject.name.Contains(" L ")) return 0;
        if (obj.transform.parent.gameObject.name.Contains(" R ")) return 1;
        if (obj.transform.parent.gameObject.name.Contains("Left")) return 2;
        if (obj.transform.parent.gameObject.name.Contains("Right")) return 3;
        if (obj.transform.parent.gameObject.name.Contains("Front")) return 4;
        return -1;
    }

    IEnumerator _RunHandsFiller()
    {
        while(Player.myplayer == null)        yield return null;
        attachobject[] allobjects = Player.myplayer.gameObject.GetComponentsInChildren<attachobject>(true);
        foreach (attachobject obj in allobjects)
        {
            GameObject LeftHand = Player.myplayer.gameObject.FindInChildren("LeftHand");
            foreach (string nm in show_lefthand_items)
            {
                GameObject ob = LeftHand.FindInChildren(nm);
                if (ob != null)     ob.SetActive(true);
            }
            foreach (string nm in hide_lefthand_items)
            {
                GameObject ob = LeftHand.FindInChildren(nm);
                if (ob != null) ob.SetActive(false);
            }

            GameObject RightHand = Player.myplayer.gameObject.FindInChildren("RightHand");
            foreach (string nm in show_righthand_items)
            {
                GameObject ob = RightHand.FindInChildren(nm);
                if (ob != null) ob.SetActive(true);
            }
            foreach (string nm in hide_righthand_items)
            {
                GameObject ob = RightHand.FindInChildren(nm);
                if (ob != null) ob.SetActive(false);
            }

            switch (CheckAttachedFather(obj.gameObject))
            {
                case 0:
                    foreach (string nm in show_lefthand_items)
                    {
                        if (obj.gameObject.name == nm) obj.gameObject.SetActive(true);
                    }
                    foreach (string nm in hide_lefthand_items)
                    {
                        if (obj.gameObject.name == nm) obj.gameObject.SetActive(false);
                    }
                    break;
                case 1:
                    foreach (string nm in show_righthand_items)
                    {
                        if (obj.gameObject.name == nm) obj.gameObject.SetActive(true);
                    }
                    foreach (string nm in hide_righthand_items)
                    {
                        if (obj.gameObject.name == nm) obj.gameObject.SetActive(false);
                    }
                    break;
                case 2:
                    foreach (string nm in show_leftbelt_items)
                    {
                        if (obj.gameObject.name == nm) obj.gameObject.SetActive(true);
                    }
                    foreach (string nm in hide_leftbelt_items)
                    {
                        if (obj.gameObject.name == nm) obj.gameObject.SetActive(false);
                    }
                    break;
                case 3:
                    foreach (string nm in show_rightbelt_items)
                    {
                        if (obj.gameObject.name == nm) obj.gameObject.SetActive(true);
                    }
                    foreach (string nm in hide_rightbelt_items)
                    {
                        if (obj.gameObject.name == nm) obj.gameObject.SetActive(false);
                    }
                    break;
            }
        }
    }
    public void RunHandsFiller()
    {
        StartCoroutine("_RunHandsFiller");
    }
}
