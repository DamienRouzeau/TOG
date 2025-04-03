/// <summary>
/// TimedObjectDestroyer.cs
/// Author: MutantGopher
/// This script destroys a GameObject after the number of seconds specified in
/// the lifeTime variable.  Useful for things like explosions and rockets.
/// </summary>

using UnityEngine;
using System.Collections;
using Photon.Pun;

public class TimedObjectDestroyer : MonoBehaviour
{
    public float lifeTime = 10.0f;
    public bool isFromServer = false;
    private bool isGoingToDie = false;

    private void Start()
    {
        if (isFromServer == false)
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (isGoingToDie == false)
        {
            isGoingToDie = true;
            StartCoroutine(Death());
        }
    }

    IEnumerator Death()
    {
        yield return new WaitForSeconds(lifeTime);
        bool isMine = PhotonNetworkController.soloMode || GetComponent<PhotonView>().IsMine;
        if (isMine)
            PhotonNetworkController.DestroySoloOrMulti(this.gameObject);
    }
}
