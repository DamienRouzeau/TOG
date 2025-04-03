#define DEBUG_PLAYERCABIN

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayersCabin : MonoBehaviour
{
    [SerializeField]
    private Button[] _levelButtons = null;
	[SerializeField]
	private List<GameObject> _goListToActivateWithMulti = null;
	[SerializeField]
	private List<GameObject> _goListToActivateForEndlessMode = null;
	[SerializeField]
	private List<GameObject> _goListToActivateForNormalMode = null;
	[SerializeField]
	private List<GameObject> _goListToActivateForKidMode = null;
	[SerializeField]
	private UI_ChooseYourName _chooseYourName = null;

#if USE_STANDALONE
	private void Start()
	{
		foreach (GameObject go in _goListToActivateWithMulti)
			go.SetActive(true);
	}
#else
	private IEnumerator Start()
	{
		SetButtonsInteractables(false);

#if DEBUG_PLAYERCABIN
		Debug.Log("[PLAYER_CABIN] PlayersCabin start");
#endif

		foreach (GameObject go in _goListToActivateWithMulti)
			go.SetActive(false);

		while (GameflowBase.instance == null)
			yield return null;

#if DEBUG_PLAYERCABIN
		Debug.Log("[PLAYER_CABIN] PlayersCabin gameflowmultiplayer.myself ok");
		if (multiplayerlobby.IsInKidMode)
			Debug.Log("[PLAYER_CABIN] Mode Kid");
		else if (multiplayerlobby.IsInEndlessRace)
			Debug.Log("[PLAYER_CABIN] Mode Endless : " + multiplayerlobby.endlessDuration);
		else
			Debug.Log("[PLAYER_CABIN] Mode Normal");
#endif
		foreach (GameObject go in _goListToActivateForEndlessMode)
			go.SetActive(multiplayerlobby.IsInEndlessRace);
		foreach (GameObject go in _goListToActivateForNormalMode)
			go.SetActive(multiplayerlobby.IsInNormalMode);
		foreach (GameObject go in _goListToActivateForKidMode)
			go.SetActive(multiplayerlobby.IsInKidMode);

		while (string.IsNullOrEmpty(GameflowBase.myPirateName))
			yield return null;

		if (_chooseYourName != null)
			_chooseYourName.Init(UI_ChooseYourName.NameType.Player, GameflowBase.myPirateName);

#if DEBUG_PLAYERCABIN
		Debug.Log("[PLAYER_CABIN] PlayersCabin GameflowBase.myPirateName " + GameflowBase.myPirateName);
#endif

		yield return new WaitForSeconds(1f);

#if DEBUG_PLAYERCABIN
		Debug.Log("[PLAYER_CABIN] PlayersCabin _goListToActivateWithMulti activated");
#endif
		
		foreach (GameObject go in _goListToActivateWithMulti)
			go.SetActive(true);
	}
#endif

	public void SetButtonsInteractables(bool enabled)
	{
		foreach (Button button in _levelButtons)
		{
			button.interactable = enabled;
			button.GetComponent<OnHover>().enabled = enabled;
		}
	}

	public Button GetLevelButtonFromIdx(int idx)
	{
		if (_levelButtons != null && idx >= 0 & idx < _levelButtons.Length)
		{
			return _levelButtons[idx];
		}
		return null;
	}

}
