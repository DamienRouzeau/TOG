using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class attachobject : MonoBehaviour
{
    public string show_in_hand = "";
    public float timeout = 0.0f;
    public bool displayfromstart = false;

    private void Update()
    {
        if (timeout > 0.0f)
        {
            timeout -= Time.deltaTime;
            if (timeout < 0) timeout = 0;
        }
    }

    private void OnEnable()
    {
        timeout = 0.1f;
    }

    private void OnTriggerStay(Collider other)
    {
        if (timeout > 0.0f) return;
        if (gameObject.GetComponent<attachobject>() || gameObject.GetComponent<InventorySlot>())        // object that can be attached to hands
            other.gameObject.SendMessage("HandTouched", gameObject, SendMessageOptions.DontRequireReceiver);
        else
            gameObject.transform.parent.gameObject.SendMessage("Touched", gameObject, SendMessageOptions.DontRequireReceiver);
    }
}
