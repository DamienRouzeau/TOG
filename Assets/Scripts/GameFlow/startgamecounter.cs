using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class startgamecounter : MonoBehaviour
{
    TextMesh tm;
    Text te;
    TextMeshPro tmpro;
    TextMeshProUGUI tmprogu;

    void Awake()
    {
        tm = gameObject.GetComponent<TextMesh>();
        te = gameObject.GetComponent<Text>();
        tmpro = gameObject.GetComponent<TextMeshPro>();
        tmprogu = gameObject.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (gameflowmultiplayer.startcountdown == -1.0f)
        {
            SetText("");
        }
        else
        {
            SetText(gameflowmultiplayer.startcountdown.ToString("F0"));
        }
    }

    void SetText(string tt)
    {
        if (tm != null) tm.text = tt;
        if (te != null) te.text = tt;
        if (tmpro != null) tmpro.text = tt;
        if (tmprogu != null) tmprogu.text = tt;
    }
}
