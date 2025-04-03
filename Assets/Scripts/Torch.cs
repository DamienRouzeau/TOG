using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torch : MonoBehaviour
{
    public bool isOn = true;

    public bool isActivatedOnStart = true;

    [SerializeField] private GameObject _torchVisualEffect = null;

    public void Start()
    {
        TorchActivation(isActivatedOnStart);    
    }

    [PunRPC]
    public void TorchActivation(bool activation)
    {
        if (_torchVisualEffect != null)
        {
            _torchVisualEffect.SetActive(activation);
            isOn = activation;
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Fire")
            TorchActivation(true);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Fire")
            TorchActivation(true);
    }
}
