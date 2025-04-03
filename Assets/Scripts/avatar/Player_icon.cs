using UnityEngine;

public class Player_icon : MonoBehaviour
{
    [SerializeField]
    private GameObject[] _goPlayerIcons = null;

    public void SetPlayerIcon(int numSkin)
	{
        for (int i = 0; i < _goPlayerIcons.Length; ++i)
		{
			_goPlayerIcons[i].SetActive(i == numSkin);
		}
	}
}
