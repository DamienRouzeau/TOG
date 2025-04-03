using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ColliderDetection : MonoBehaviour
{
    [SerializeField]
    private LayerMask _layersToDetect = default;
    [SerializeField]
    private UnityEvent _onEnterEvent = null;
    [SerializeField]
    private UnityEvent _onExitEvent = null;
    [SerializeField]
    private bool _onlyCheckYourPlayer = false;
    [SerializeField]
    private bool _canOnlyTriggerOneTime = false;
    [SerializeField]
    private bool _checkHand = false;
    [SerializeField]
    private UnityEvent _onLeftHandEvent = null;
    [SerializeField]
    private UnityEvent _onRightHandEvent = null;

    [Header("Only for this client")]
    [SerializeField]
    private UnityEvent _onLocalEnterEvent = null;
    [SerializeField]
    private UnityEvent _onLocalExitEvent = null;

    private Dictionary<string, bool> _isTriggeredDic = null;
    private Collider _collider = null;
    private PhotonView _photonView = null;

    private bool _isAlreadyTriggerEnter = false;
    private bool _isAlreadyTriggerExit = false;

    private void Start()
	{
        _collider = GetComponent<Collider>();
        _photonView = GetComponent<PhotonView>();
    }

	private void Update()
	{
		if (_collider != null && _isTriggeredDic != null)
		{
            if (!_collider.enabled)
			{
                DisableTriggers();
            }
		}
	}

	private void OnDisable()
	{
        DisableTriggers();
    }

    private void DisableTriggers()
	{
        if (_isTriggeredDic != null)
        {
            List<string> toDisable = new List<string>();
            foreach (var keyval in _isTriggeredDic)
            {
                if (keyval.Value)
                {
                    toDisable.Add(keyval.Key);
                }
            }
            foreach (string key in toDisable)
            {
                TriggerExit(key, true);
            }
        }
    }

	private void OnTriggerEnter(Collider other)
    {
        string id = GetIdFromCollider(other);

        RegisterTriggerId(id);

        if (CanEnter(id))
        {
            if (HasLayer(other.gameObject.layer))
            {
                if (_onlyCheckYourPlayer)
			    {
                    if (other.gameObject.GetComponentInParent<Player>() == null)
				    {
                        return;
				    }
                    if (_checkHand)
					{
                        pointfromhand hand = other.gameObject.GetComponent<pointfromhand>();
                        if (hand != null)
						{
                            Debug.Log("HAND DETECTED AT " + (hand.righthand ? "RIGHT" : "LEFT"));
                            if (hand.righthand)
                                _onRightHandEvent?.Invoke();
                            else
                                _onLeftHandEvent?.Invoke();
						}
                    }
			    }

                TriggerEnter(id, true);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        string id = GetIdFromCollider(other);

        RegisterTriggerId(id);

        if (CanEnter(id))
        {
            if (HasLayer(other.gameObject.layer))
            {
                if (_onlyCheckYourPlayer)
                {
                    if (other.gameObject.GetComponentInParent<Player>() == null)
                    {
                        return;
                    }
                }

                TriggerEnter(id, true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        string id = GetIdFromCollider(other);

        if (CanExit(id))
        {
            if (HasLayer(other.gameObject.layer))
            {
                if (_onlyCheckYourPlayer)
                {
                    if (other.gameObject.GetComponentInParent<Player>() == null)
                    {
                        return;
                    }
                }

                TriggerExit(id, true);
            }
        }
    }

    private bool HasLayer(int layer)
	{
        return (_layersToDetect.value & (1 << layer)) > 0;
    }

    private string GetIdFromCollider(Collider col)
    {
        if (_canOnlyTriggerOneTime)
            return "0";
        else
            return "" + col.GetInstanceID();
    }

    private void RegisterTriggerId(string id)
    {
        if (_isTriggeredDic == null)
            _isTriggeredDic = new Dictionary<string, bool>();
        if (!_isTriggeredDic.ContainsKey(id))
            _isTriggeredDic.Add(id, false);
    }

    private bool CanEnter(string id)
	{
        if (_canOnlyTriggerOneTime)
            return !_isAlreadyTriggerEnter;
        return !_isTriggeredDic[id];
	}

    private bool CanExit(string id)
    {
        if (_canOnlyTriggerOneTime)
            return !_isAlreadyTriggerExit;
        return _isTriggeredDic != null && _isTriggeredDic[id];
    }

    private void TriggerEnter(string id, bool local = false)
	{
        //Debug.Log($"[COL] TriggerEnter {id} {local}");
        _isTriggeredDic[id] = true;

        _onEnterEvent?.Invoke();

        if (local)
            _onLocalEnterEvent?.Invoke();

        _isAlreadyTriggerEnter = true;

        if (_photonView != null && local)
        {
            if (_onlyCheckYourPlayer)
                _photonView.RPC("RpcTriggerEnter", RpcTarget.Others, $"{id}_{PhotonNetwork.LocalPlayer.ActorNumber}");
            else
                _photonView.RPC("RpcTriggerEnter", RpcTarget.Others, id);
        }
    }

    private void TriggerExit(string id, bool local = false)
    {
        //Debug.Log($"[COL] TriggerExit {id} {local}");
        _isTriggeredDic[id] = false;

        _onExitEvent?.Invoke();

        if (local)
            _onLocalExitEvent?.Invoke();

        _isAlreadyTriggerExit = true;

        if (_photonView != null && local)
        {
            if (_onlyCheckYourPlayer)
                _photonView.RPC("RpcTriggerExit", RpcTarget.Others, $"{id}_{PhotonNetwork.LocalPlayer.ActorNumber}");
            else
                _photonView.RPC("RpcTriggerExit", RpcTarget.Others, id);
        }
    }

    [PunRPC]
    private void RpcTriggerEnter(string id)
	{
        RegisterTriggerId(id);

        if (CanEnter(id))
        {
            TriggerEnter(id);
        }
    }

    [PunRPC]
    private void RpcTriggerExit(string id)
    {
        if (CanExit(id))
        {
            TriggerExit(id);
        }
    }
}
