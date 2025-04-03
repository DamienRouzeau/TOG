using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class togglemaster : MonoBehaviour
{
    public string default_value = "";
    toggleelement[] allelements;
    public string selected_value = "";

    // Start is called before the first frame update
    void Awake()
    {
        allelements = gameObject.GetComponentsInChildren<toggleelement>();
        Clicked(default_value);
    }

    public void Clicked(string change)
    {
        selected_value = change;
        foreach (toggleelement elm in allelements)
        {
            Image img = elm.gameObject.GetComponent<Image>();
            if (elm.elementvalue == selected_value)
                img.color = Color.white;
            else
                img.color = Color.gray;
        }
    }
}
