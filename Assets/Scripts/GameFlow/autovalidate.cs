using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class autovalidate : MonoBehaviour
{

    IEnumerator Start()
    {
        yield return new WaitForSeconds(20.0f);
        GameObject obj = gameObject.FindInChildren("pupitre");
        obj.SetActive(true);
        yield return new WaitForSeconds(10.0f);
        obj = gameObject.FindInChildren("Right_Off");
        detect3dtouch touch = obj.GetComponent<detect3dtouch>();
        touch.touchme = true;

    }
}
