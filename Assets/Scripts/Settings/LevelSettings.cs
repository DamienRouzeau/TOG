using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/LevelSettings", order = 1)]
public class LevelSettings : ScriptableObject
{
	#region Enums

	[System.Flags]
	public enum GameMode
	{
		Normal,
		Endless,
		Mission
	}

	#endregion

	#region Classes

	[System.Serializable]
	public class Level
	{
		public string id;
		public string sceneName;
		public string textId;
		public GameMode mode;
		public int unlockNext; // For USE_STANDALONE
	}

	[System.Serializable]
	public class LevelList
	{
		public string id;
		public List<string> levelIds;
		public bool loop;
	}

	#endregion

	#region Properties

	public List<Level> levels;
	public List<LevelList> levelLists;

	#endregion

	#region Public Functions

	public string GetSceneName(string listId, int index)
	{
		string levelId = GetLevelIdFromListIndex(listId, index);
		if (levelId != null)
		{
			Level level = GetLevelFromId(levelId);
			if (level != null)
			{
				return level.sceneName;
			}
		}
		return null;
	}

	public string GetLevelIdFromListIndex(string listId, int index)
	{
		foreach (LevelList list in levelLists)
		{
			if (list.id == listId)
			{
				if (index < list.levelIds.Count)
				{
					return list.levelIds[index];
				}
				else if (list.loop)
				{
					return list.levelIds[index % list.levelIds.Count];
				}
			}
		}
		return null;
	}

	public Level GetLevelFromListIndex(string listId, int index)
	{
		string levelId = GetLevelIdFromListIndex(listId, index);
		return GetLevelFromId(levelId);
	}

	public Level GetLevelFromId(string id)
	{
		foreach (Level level in levels)
		{
			if (level.id == id)
				return level;
		}
		return null;
	}

	public Level GetLevelFromSceneName(string sceneName)
	{
		foreach (Level level in levels)
		{
			if (level.sceneName == sceneName)
				return level;
		}
		return null;
	}

	public LevelList GetLevelListFromId(string listId)
	{
		foreach (LevelList list in levelLists)
		{
			if (list.id == listId)
			{
				return list;
			}
		}
		return null;
	}

	#endregion
}
