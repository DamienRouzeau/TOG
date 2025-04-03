using UnityEngine;

public class UI_CabinLevelSelection : MonoBehaviour
{
    [SerializeField]
    private UI_CabinLevel _cabinLevel = null;
    [SerializeField]
    private ChoiceButtons _choiceButtons = null;

	private void Start()
	{
		Init();
	}

	public int GetLevelIndex()
	{
		return _choiceButtons.levelIndex;
	}

	public bool IsAvailable()
	{
		return _choiceButtons.gameObject.activeSelf;
	}

	public void HideInfoAndButton(bool available, bool isMaster)
	{
		_cabinLevel.ShowSkulls(false);
		_cabinLevel.ShowHeader(available);
		_cabinLevel.SetJustUnlockedLevel(available);
		_cabinLevel.ShowBestScore(0);
		_cabinLevel.SetLocked(false, false);
		_choiceButtons.gameObject.SetActive(available && isMaster);
	}

	public void Init()
	{
		Debug.Assert(_cabinLevel != null);
		Debug.Assert(_choiceButtons != null);
#if USE_STANDALONE

		_cabinLevel.gameObject.SetActive(true);

		LevelSettings.Level levelData = gamesettings_general.myself.levelSettings.GetLevelFromListIndex(_choiceButtons.levelListId, _choiceButtons.levelIndex);
		Debug.Assert(levelData != null);
		string levelId = levelData.id;
		Debug.Assert(levelId != null);

		SaveManager.ProgressionLevelData level = null;
		if (SaveManager.myself.profileCount > 0)
			level = SaveManager.myself.profile.progression.GetLevelFromId(levelId);

		bool locked = level == null;

		// Ckeck locked
		if (_choiceButtons.levelIndex > 0)
		{
			LevelSettings.Level prevLevelData = gamesettings_general.myself.levelSettings.GetLevelFromListIndex(_choiceButtons.levelListId, _choiceButtons.levelIndex-1);
			Debug.Assert(prevLevelData != null);
			int unlockNext = prevLevelData.unlockNext;
			if (level != null)
			{
				SaveManager.ProgressionLevelData prevLevel = SaveManager.myself.profile.progression.GetLevelFromId(prevLevelData.id);
				locked = prevLevel.GetUnlockedCount() < unlockNext;
				_cabinLevel.SetPreviousRaceName(prevLevelData.textId);
				_cabinLevel.SetGoalToUnlock(unlockNext.ToString() + "/" + prevLevel.unlockedSkulls.Length);
			}
			else
			{
				locked = true;
			}
		}

		bool canPlay = !locked && level != null;

		_choiceButtons.gameObject.SetActive(canPlay);

		// Best score
		_cabinLevel.ShowBestScore(canPlay ? level.bestScore : 0);

		// Skulls
		
		_cabinLevel.ShowSkulls(canPlay);
		if (canPlay)
		{
			for (int i = 0; i < level.unlockedSkulls.Length; ++i)
				_cabinLevel.SetSkullDisplay(i, level.unlockedSkulls[i]);
		}

		// Locked
		_cabinLevel.SetLocked(locked, _choiceButtons.levelIndex > 0);
		_cabinLevel.SetJustUnlockedLevel(canPlay && level.bestScore == 0);

		// Header
		_cabinLevel.ShowHeader(!locked, levelData.textId);
#else
		_cabinLevel.gameObject.SetActive(false);
#endif
	}
}
