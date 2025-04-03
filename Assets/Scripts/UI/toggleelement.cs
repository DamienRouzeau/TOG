using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class toggleelement : MonoBehaviour
{
    public string elementvalue = "";

    public void Clicked()
    {
        togglemaster master = gameObject.transform.parent.gameObject.GetComponent<togglemaster>();
        master.Clicked(elementvalue);
    }
}
