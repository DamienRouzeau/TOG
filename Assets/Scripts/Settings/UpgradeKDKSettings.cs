using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/UpgradeKDKSettings", order = 1)]
public class UpgradeKDKSettings : ScriptableObject
{
	#region Enums

	public enum UpgradeType
	{
		LeftGun,
		RightGun,
		AntennaRecovery
	}

	#endregion

	#region Classes

	[Serializable]
	public class UpgradeDataByGold
	{
		public string name;
		public string textId;
		public Sprite sprite;
		public int gold;
		public UpgradeType upType;
		public float value;
		public int restartLevel;
	}

	[Serializable]
	public class UpgradeDataByWave
	{
		public string name;
		public string textId;
		public Sprite sprite;
		public int numWave;
		public int level;
		public UpgradeType upType;
		public float value;
	}

	#endregion

	#region Properties

	public List<UpgradeDataByGold> _upgradedByGold = null;
	public List<UpgradeDataByWave> _upgradedByWave = null;

	#endregion

	public UpgradeDataByGold GetUpgradeByGold(int numUpgrade)
	{
		if (numUpgrade >= 0 && numUpgrade < _upgradedByGold.Count)
			return _upgradedByGold[numUpgrade];
		return null;
	}

	public List<UpgradeDataByWave> GetUpgradesByWave(int wave)
	{
		List<UpgradeDataByWave> upgrades = new List<UpgradeDataByWave>();
		foreach (UpgradeDataByWave upWave in _upgradedByWave)
		{
			if (upWave.numWave - 1 == wave)
				upgrades.Add(upWave);
		}
		return upgrades;
	}

	public UpgradeDataByGold[] GetUpgradesByGoldUntilWave(int wave)
	{
		List<UpgradeDataByGold> upgrades = new List<UpgradeDataByGold>();
		foreach (UpgradeDataByGold up in _upgradedByGold)
		{
			if (up.restartLevel - 1 <= wave)
				upgrades.Add(up);
		}
		if (upgrades.Count == 0)
			return null;
		return upgrades.ToArray();
	}

	public List<UpgradeDataByWave> GetUpgradesUntilWave(int wave)
	{
		List<UpgradeDataByWave> upgrades = new List<UpgradeDataByWave>();
		foreach (UpgradeDataByWave upWave in _upgradedByWave)
		{
			if (upWave.numWave - 1 < wave)
				upgrades.Add(upWave);
		}
		return upgrades;
	}
}
