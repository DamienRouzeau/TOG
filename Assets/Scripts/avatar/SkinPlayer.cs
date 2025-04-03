using UnityEngine;

public class SkinPlayer : MonoBehaviour
{
    public PlayerBody skinPlayerBodyScript;
    public GameObject skinPlayerBodyObject;
    public GameObject skinPlayerHead;
    public GameObject skinPlayerHat;
    public GameObject skinPlayerBody;
    public GameObject skinPlayerBodyTuto;
    public GameObject skinPelvis;

    public void ShowPlayer(bool show)
	{
        skinPlayerHead?.SetActive(show);
        skinPlayerHat?.SetActive(show);
        skinPlayerBody?.SetActive(show);
        skinPelvis?.SetActive(show);
    }
}
