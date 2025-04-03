using UnityEngine;

public class ProgressionValidator : MonoBehaviour
{
	[SerializeField]
	private GameObject[] _targetsToEnable = null;
	[SerializeField]
	private GameObject[] _targetsToDisable = null;
	[SerializeField]
	private int _index = 0;

	private string _currentLevelId = null;

	private void Start()
	{
		UpdateDisplay();
	}

	public void UpdateDisplay()
	{
#if USE_STANDALONE
		_currentLevelId = gamesettings_general.myself.levelSettings.GetLevelIdFromListIndex(gameflowmultiplayer.levelListId, gameflowmultiplayer.levelIndex);
		bool unlocked = SaveManager.myself.profile.progression.GetLevelFromId(_currentLevelId).unlockedSkulls[_index];
		EnableTargets(_targetsToEnable, unlocked);
		EnableTargets(_targetsToDisable, !unlocked);
#else
		_index = 0;
		_currentLevelId = null;
		_targetsToEnable = null;
		_targetsToDisable = null;
#endif
	}

#if USE_STANDALONE
	private void EnableTargets(GameObject[] targets, bool enable)
	{
		if (targets != null)
		{
			foreach (GameObject go in targets)
			{
				go.SetActive(enable);
			}
		}
	}
#endif
}
