using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using MiniJSON;

public class mp_dum : MonoBehaviour
{
    int netid = 0;
    string uniqueidentifier = "";
    string myname = "";
    bool updatedata = false;

    GameObject head_src = null;
    GameObject left_src = null;
    GameObject right_src = null;

    GameObject avatar_dst = null;
    GameObject head_dst = null;
    GameObject left_dst = null;
    GameObject right_dst = null;

    GameObject deathanim = null;
    GameObject skin = null;
    Renderer[] allrenddeathanim;
    Image hudmybar;

    void Start()
    {
        PhotonView pV = transform.GetComponent<PhotonView>();
        object[] data = pV.InstantiationData;
        uniqueidentifier = (string)data[0];
        IDictionary js = (IDictionary)Json.Deserialize(uniqueidentifier);
        netid = (int)((long)js["nr"]);
        myname = (string)js["name"];
        gameObject.name = myname + netid;
        if (netid != (PhotonNetwork.LocalPlayer.ActorNumber - 1))
            StartCoroutine("Initialize");
    }

    private void OnDestroy()
    {
        if (avatar_dst != null)
        {
            Player_avatar pa = avatar_dst.GetComponent<Player_avatar>();
            if (pa != null)
                pa.PoolAvatar();
            avatar_dst.SetActive(false);
            Player.myplayer?.NonPlayersPool?.Add(avatar_dst);
        }
    }

    IEnumerator Initialize()
    {
        // launcher dont need init
        if (PhotonNetworkController.myself == null) yield break;

        gameObject.transform.SetParent(PhotonNetworkController.myself.gameObject.transform);
        yield return new WaitForSeconds(1.0f);

        if (myname.Contains("_b_"))
        {
            foreach (GameObject av in Player.myplayer.NonPlayersPool)
            {
                if (!av.activeInHierarchy)
                {
                    avatar_dst = av;
                    break;
                }
            }
            if (avatar_dst)
            {
                Player.myplayer.NonPlayersPool.Remove(avatar_dst);

                GameObject avatarhud = avatar_dst.FindInChildren("UI_PLayers_Infos");

                hudmybar = avatarhud.FindInChildren("UI_LifeJauge_Fill").GetComponent<Image>();
                hudmybar.fillAmount = 1.0f;
                avatar_dst.name = "UsedAvatar_" + netid;
                Player_avatar pa = avatar_dst.GetComponent<Player_avatar>();
                Debug.Assert(pa != null, "Player_avatar script missing!");
                pa.SetName(gamesettings_player.myself.GetNameFromIndex(netid));
                pa.actornumber = netid;
                GameObject father = PhotonNetworkController.myself.gameObject;
                // Find destinations
                head_src = father.FindInChildren(gameObject.name.Replace("_b_", "_h_"));
                left_src = father.FindInChildren(gameObject.name.Replace("_b_", "_l_"));
                right_src = father.FindInChildren(gameObject.name.Replace("_b_", "_r_"));
                // find sources
                head_dst = avatar_dst.FindInChildren("VR_Camera");
                left_dst = avatar_dst.FindInChildren("LeftHand");
                right_dst = avatar_dst.FindInChildren("RightHand");

                avatar_dst.transform.SetParent(Player.myplayer.transform.parent);

                avatar_dst.transform.position = gameObject.transform.position;
                avatar_dst.transform.rotation = gameObject.transform.rotation;

                RootMotion.FinalIK.VRIK[] allvrik = avatar_dst.GetComponentsInChildren<RootMotion.FinalIK.VRIK>(true);
                foreach (RootMotion.FinalIK.VRIK vrik in allvrik)
                {
                    if (vrik.gameObject.name == GameflowBase.pirateskins[netid])
                    {
                        skin = vrik.gameObject;
                        deathanim = skin.FindInChildren("death");
                        allrenddeathanim = skin.GetComponentsInChildren<Renderer>();
                        skin.SetActive(true);
                        pa.InitVRIK(vrik);
                    }
                }
                pa.SetMyAttachedObjects();
                // choose avatar look
                if (GameflowBase.instance != null)
                    avatar_dst.SetActive(GameflowBase.instance.AreAvatarsVisibles() && GameflowBase.IsNumActorVisible(netid));
                else
                    avatar_dst.SetActive(true);
                updatedata = true;
            }
        }
        Debug.Log("OBJECT SPAWNED:" + uniqueidentifier);
        yield return null;
    }
    private void Update()
    {
        if (updatedata)
        {
            avatar_dst.transform.localPosition = gameObject.transform.localPosition; //            Vector3.Lerp(avatar_dst.transform.localPosition, gameObject.transform.localPosition, speed);
            avatar_dst.transform.localRotation = gameObject.transform.localRotation;

            head_dst.transform.localPosition = head_src.transform.localPosition; //            Vector3.Lerp(head_dst.transform.localPosition, head_src.transform.localPosition, speed);
            head_dst.transform.localRotation = head_src.transform.localRotation;

            left_dst.transform.localPosition = left_src.transform.localPosition; //            Vector3.Lerp(left_dst.transform.localPosition, left_src.transform.localPosition, speed);
            left_dst.transform.localRotation = left_src.transform.localRotation;

            right_dst.transform.localPosition = right_src.transform.localPosition; //            Vector3.Lerp(right_dst.transform.localPosition, right_src.transform.localPosition, speed);
            right_dst.transform.localRotation = right_src.transform.localRotation;
        }
    }
}