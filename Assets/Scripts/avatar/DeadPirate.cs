using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadPirate : MonoBehaviour
{
    private float _timeLeft;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Death(float timeLeft)
    {
        _timeLeft = timeLeft + 3f;
        StartCoroutine(DisableDeadPirate());
    }

    private IEnumerator DisableDeadPirate()
    {
        yield return new WaitForSeconds(_timeLeft);
        PunDisableDeadPirate();
        yield return null;
    }

    [PunRPC]
    private void PunDisableDeadPirate()
    {
        PhotonNetworkController.DestroySoloOrMulti(this.gameObject);
    }

}
