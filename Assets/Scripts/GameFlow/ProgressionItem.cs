using UnityEngine;

public class ProgressionItem : MonoBehaviour
{
	[SerializeField]
	private Health _health = null;
	[SerializeField]
    private int _index = 0;

	private string _currentLevelId = null;

	private void Start()
	{
		if (PhotonNetworkController.soloMode)
		{
			_currentLevelId = gamesettings_general.myself.levelSettings.GetLevelIdFromListIndex(gameflowmultiplayer.levelListId, gameflowmultiplayer.levelIndex);
			if (SaveManager.myself.profile.progression.GetLevelFromId(_currentLevelId).unlockedSkulls[_index])
			{
				GameObject.Destroy(gameObject);
			}
			else if (_health != null)
			{
				_health.SetProgressionItem(this);
			}
		}
	}

	private void OnDestroy()
	{
		_health = null;
		_index = 0;
	}

	public void CollectItem()
	{
		if (PhotonNetworkController.soloMode)
		{
			SaveManager.myself.profile.progression.GetLevelFromId(_currentLevelId).unlockedSkulls[_index] = true;
			gameflowmultiplayer.unlockedSkullsInRace[_index] = true;
			if (RaceManager.myself != null)
				RaceManager.myself.UpdateQuestDisplay();
		}
	}
}
