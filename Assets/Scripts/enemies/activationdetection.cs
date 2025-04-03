using UnityEngine;

public class activationdetection : MonoBehaviour
{
    [SerializeField]
    private GameObject _receiver = null;
    [SerializeField]
    private GameObject[] _otherReceivers = null;
    private bool _isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        SendMessage("ZoneEntered", other.gameObject);
        _isTriggered = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!_isTriggered)
        {
            SendMessage("ZoneEntered", other.gameObject);
            _isTriggered = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        SendMessage("ZoneLeft", other.gameObject);
        _isTriggered = false;
    }

    private void SendMessage(string message, GameObject detectedGO)
	{
        if (_receiver != null)
            _receiver.SendMessage(message, detectedGO, SendMessageOptions.DontRequireReceiver);
        if (_otherReceivers != null)
        {
            foreach (GameObject go in _otherReceivers)
            {
                if (go != null)
                    go.SendMessage(message, detectedGO, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}
