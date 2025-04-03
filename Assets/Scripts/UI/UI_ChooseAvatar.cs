using UnityEngine;
using UnityEngine.UI;

public class UI_ChooseAvatar : MonoBehaviour
{
    [SerializeField]
    private Button[] _avatarButtons = null;
    [SerializeField]
    private GameObject[] _avatarHighlights = null;

	private void Awake()
	{
		if (_avatarButtons != null)
		{
			for (int i = 0; i < _avatarButtons.Length; ++i)
			{
				int numAvatar = i;
				_avatarButtons[i].onClick.AddListener(() => ChooseAvatar(numAvatar));
			}
		}
	}

	private void OnDestroy()
	{
		if (_avatarButtons != null)
		{
			for (int i = 0; i < _avatarButtons.Length; ++i)
			{
				_avatarButtons[i].onClick.RemoveAllListeners();
			}
		}
	}

	public void ChooseAvatar(int num)
	{
		for (int i = 0; i < _avatarHighlights.Length; ++i)
			_avatarHighlights[i].SetActive(i == num);
		for (int i = 0; i < _avatarHighlights.Length; ++i)
			_avatarButtons[i].interactable = i != num;
		string avatarName = gamesettings_player.myself.GetSkinName(num);
		Player.myplayer.ForceSkin(avatarName);
	}

}
