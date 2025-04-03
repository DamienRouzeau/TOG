using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KDK_bullet_pool_creator : MonoBehaviour
{
    public int numberofbullets = 20;
    public enum _bullettype
    {
        musketbulletkdk,
        biggunbulletkdk,
        trigunbulletkdk,
        cannonbulletkdk,
        cannonbigbulletkdk,
        confettikdk,
        losebulletkdk
    }
    public _bullettype mytype = _bullettype.musketbulletkdk;

    public static _bullettype GetBulletTypeFromName(string name)
	{
        switch (name)
        {
            case "bulletforpoolkdk":
                return _bullettype.musketbulletkdk;
            case "bulletforpoolbkdk":
                return _bullettype.biggunbulletkdk;
            case "bulletforpoolc":
                return _bullettype.trigunbulletkdk;
            case "cannonballforpool":
                return _bullettype.cannonbulletkdk;
            case "cannonballbigforpool":
                return _bullettype.cannonbigbulletkdk;
            case "confettiforpool":
                return _bullettype.confettikdk;
            case "loseconfettiforpool":
            default:
                return _bullettype.losebulletkdk;
        }
    }

    IEnumerator Start()
    {
        while (PhotonNetworkController.myself == null || !PhotonNetworkController.myself.ready)
            yield return null;

        if (GameLoader.myself.multiplayerUseLauncher)
        {
            while (!(multiplayerlobby.myself.icreated || multiplayerlobby.myself.insideroom))
                yield return null;
        }

        yield return new WaitForSeconds(Random.Range(1f, 2f));

        for(int i=0;i< numberofbullets;i++)
        {
            string bullname = "bulletforpoolkdk";
            switch (mytype)
            {
                case _bullettype.musketbulletkdk:
                    break;
                case _bullettype.biggunbulletkdk:
                    bullname = "bulletforpoolbkdk";
                    break;
                case _bullettype.trigunbulletkdk:
                    bullname = "bulletforpoolckdk";
                    break;
                case _bullettype.cannonbulletkdk:
                    bullname = "cannonballforpoolkdk";
                    break;
                case _bullettype.cannonbigbulletkdk:
                    bullname = "cannonballbigforpoolkdk";
                    break;
                case _bullettype.confettikdk:
                    bullname = "confettiforpoolkdk";
                    break;
                case _bullettype.losebulletkdk:
                    bullname = "loseconfettiforpoolkdk";
                    break;
            }

            GameObject obj = PhotonNetworkController.InstantiateSoloOrMulti(bullname, Vector3.zero, Quaternion.identity);
            obj.transform.SetParent(transform);
            obj.FindInChildren("object").SetActive(false);
            yield return null;
        }
        poolmaster pm = gameObject.GetComponent<poolmaster>();
        if (pm)            pm.ReStart();
    }

}
