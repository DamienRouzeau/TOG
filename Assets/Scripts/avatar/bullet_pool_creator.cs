using System.Collections;
using UnityEngine;

public class bullet_pool_creator : MonoBehaviour
{
    public int numberofbullets = 20;
    public enum _bullettype
    {
        musketbullet,
        biggunbullet,
        trigunbullet,
        cannonbullet,
        cannonbigbullet,
        confetti,
        losebullet
    }
    public _bullettype mytype = _bullettype.musketbullet;

    public static _bullettype GetBulletTypeFromName(string name)
	{
        switch (name)
        {
            case "bulletforpool":
                return _bullettype.musketbullet;
            case "bulletforpoolb":
                return _bullettype.biggunbullet;
            case "bulletforpoolc":
                return _bullettype.trigunbullet;
            case "cannonballforpool":
                return _bullettype.cannonbullet;
            case "cannonballbigforpool":
                return _bullettype.cannonbigbullet;
            case "confettiforpool":
                return _bullettype.confetti;
            case "loseconfettiforpool":
            default:
                return _bullettype.losebullet;
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

        bool isInSoloMode = PhotonNetworkController.soloMode;

        yield return new WaitForSeconds(Random.Range(1f, 2f));

        for(int i=0;i< numberofbullets;i++)
        {
            string bullname = "bulletforpool";
            switch (mytype)
            {
                case _bullettype.musketbullet:
                    break;
                case _bullettype.biggunbullet:
                    bullname = "bulletforpoolb";
                    break;
                case _bullettype.trigunbullet:
                    bullname = "bulletforpoolc";
                    break;
                case _bullettype.cannonbullet:
                    bullname = "cannonballforpool";
                    break;
                case _bullettype.cannonbigbullet:
                    bullname = "cannonballbigforpool";
                    break;
                case _bullettype.confetti:
                    bullname = "confettiforpool";
                    break;
                case _bullettype.losebullet:
                    bullname = "loseconfettiforpool";
                    break;
            }

            GameObject obj = PhotonNetworkController.InstantiateSoloOrMulti(isInSoloMode, bullname, Vector3.zero, Quaternion.identity);
            obj.transform.SetParent(transform);
            obj.FindInChildren("object").SetActive(false);
            yield return null;
        }
        poolmaster pm = gameObject.GetComponent<poolmaster>();
        if (pm)            pm.ReStart();
    }

}
