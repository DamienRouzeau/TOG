using UnityEngine;
using UnityEngine.Events;

public class hand_on_shield : MonoBehaviour
{
    public UnityEvent eventsOnShieldButtonDown = null;

    private void OnTriggerStay(Collider other)
    {
        //Debug.Log($"Raph - hand_on_shield OnTriggerStay {other}");

        pointfromhand hand = other.gameObject.GetComponent<pointfromhand>();
        if (hand != null)
        {
            //Debug.Log($"Raph - hand_on_shield hand {hand}");
        }
    }

    public void OnShieldButtonDown()
	{
        if (eventsOnShieldButtonDown != null)
            eventsOnShieldButtonDown.Invoke();

    }
}
