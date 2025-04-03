using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class selectavatar : MonoBehaviour
{
    public string avatar_skin_name = "";
    Animator anm;

    private void Awake()
    {
        anm = gameObject.GetComponent<Animator>();
        anm.StopPlayback();
    }

    IEnumerator SwappingSkin(Player pl)
    {
        anm.SetTrigger("SkinA");
        GameObject skin = pl.ForceSkin(avatar_skin_name);
        Animation eff = skin.FindInChildren("FX_Skin_Swap").GetComponent<Animation>();
        eff.gameObject.SetActive(true);
        eff.Stop();
        eff.Rewind();
        eff.Play();
        StartCoroutine(DisappearEffect(eff.gameObject));
        yield return new WaitForSeconds(1);
    }

    IEnumerator DisappearEffect(GameObject eff)
    {
        yield return new WaitForSeconds(2.5f);
        eff.SetActive(false);
    }

    public void SelectSkin()
    {
        if (!gameflowmultiplayer.myself.teamValidated)
        {
            ToggleSkin(Player.myplayer);
        }
    }

    public void ToggleSkin(Player pl)
    {
        if (pl.HasDaggerInLeftHand() || pl.HasDaggerInRightHand())
            return;
        StartCoroutine(SwappingSkin(pl));
    }
}
