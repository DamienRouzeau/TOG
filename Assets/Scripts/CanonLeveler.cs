using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanonLeveler : MonoBehaviour
{
    public bool isHoldByPlayer = false;
    public GameObject horizontalCanon;
    [SerializeField] private GameObject _parent = null;

    private GameObject _playerHand = null;
    private float _oldHandPos = 0;
    private ProjectileCannonWithError _parentScript;
    private float _originalLevelerPosZ;

    public void Start()
    {
        _parentScript = _parent.GetComponent<ProjectileCannonWithError>();
        _originalLevelerPosZ = transform.localPosition.z;
    }

    void Update()
    {
        if (isHoldByPlayer && _playerHand != null)
            MoveLeveler();
    }

    private void MoveLeveler()
    {
       float movementToMake = _oldHandPos - _playerHand.transform.localPosition.z;
        _oldHandPos = _playerHand.transform.localPosition.z;
        // 0.325 is the min max for the leverposition
        if (transform.localPosition.x + movementToMake >= -0.325 && transform.localPosition.x + movementToMake < 0.325) // Otherwise we can't move the lever further
        {
            transform.Translate(0, 0, movementToMake);
            float power = ((_parentScript.minMaxAngle.x * -1) + _parentScript.minMaxAngle.y) * 1.5f;
            float rot = transform.localPosition.x * power;
            if (rot > _parentScript.minMaxAngle.x && rot < _parentScript.minMaxAngle.y)
                horizontalCanon.transform.rotation = Quaternion.Euler(rot * -1, 0, 0);
        }
    }

    public void TakeLeveler(GameObject hand)
    {
        isHoldByPlayer = true;
        _playerHand = hand;
        _oldHandPos = _playerHand.transform.localPosition.z;
    }

    public void LeaveLeveler()
    {
        isHoldByPlayer = false;
        _playerHand = null;
    }
}
