using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class PlayerCounterEvent : MonoBehaviour
{
    [SerializeField]
    private bool _countAllPlayers = true;
    [SerializeField]
    private int _countSpecificTarget = 0;
    [SerializeField]
    private UnityEvent _event = null;

    [Header("Multi")]
    [SerializeField]
    private PhotonView _photonView = null;

    private int _counter = 0;
    private bool _triggered = false;

    public void IncrementCounter()
	{
        if (_triggered)
            return;

        _counter++;
        Debug.Log("PlayerCounterEvent - IncrementCounter " + _counter);
        SetCounter(_counter);
        if (_photonView != null)
            _photonView.RPC("RpcSetCounter", RpcTarget.Others, _counter);
    }

    public void DecrementCounter()
    {
        if (_triggered)
            return;

        _counter--;
        if (_counter < 0)
            _counter = 0;
        Debug.Log("PlayerCounterEvent - DecrementCounter " + _counter);
        SetCounter(_counter);
        if (_photonView != null)
            _photonView.RPC("RpcSetCounter", RpcTarget.Others, _counter);
    }

    private void SetCounter(int counter)
	{
        if (_triggered)
            return;
        _counter = counter;
        int target = _countAllPlayers ? GameflowBase.playerCount : _countSpecificTarget;
        Debug.Log($"PlayerCounterEvent - SetCounter {_counter}/{target}");
        if (_counter == target)
        {
            Debug.Log("PlayerCounterEvent - call invoke!");
            _event?.Invoke();
            _triggered = true;
        }
    }

    [PunRPC]
    private void RpcSetCounter(int counter)
	{
        Debug.Log("PlayerCounterEvent - RpcSetCounter " + counter);
        SetCounter(counter);
    }
}
