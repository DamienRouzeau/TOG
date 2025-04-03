using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanonFireTrigger : MonoBehaviour
{
    private ProjectileCannonWithError _canonFather;

    public void Init(GameObject canonFather)
    {
        _canonFather = canonFather.GetComponent<ProjectileCannonWithError>();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Canon_Bait")
        {
            Torch torch = other.gameObject.GetComponent<Torch>();
            if (torch.isOn && _canonFather.isInCoolDown == false)
            {
                torch.TorchActivation(false);
                _canonFather.FireCannon();
            }
        }
    }
}
