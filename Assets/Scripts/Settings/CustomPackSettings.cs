using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CustomPackSettings", order = 1)]
public class CustomPackSettings : ScriptableObject
{
	#region Classes

	[Serializable]
	public class CustomFeature
	{
		public string name;
		public int cost;
		public List<string> packNames;
		public List<string> itemNames;
	}

	[Serializable]
	public class CustomPack
	{
		public string name;
		public int cost;
		public List<string> itemNames;
	}

	[Serializable]
	public class CustomItem
	{
		public string name;
		public GameObject prefab;
		public int cost;
	}

	#endregion

	#region Properties

	public List<CustomFeature> features;
	public List<CustomPack> packs;
	public List<CustomItem> items;

	#endregion

	public CustomFeature GetFeature(int i)
	{
		if (i >= 0 && i < features.Count)
			return features[i];
		return null;
	}

	public CustomFeature GetFeature(string name)
	{
		for (int i = 0; i < features.Count; ++i)
		{
			CustomFeature feature = features[i];
			if (feature.name == name)
				return feature;
		}
		return null;
	}

	public CustomPack GetPack(int i)
	{
		if (i >= 0 && i < packs.Count)
			return packs[i];
		return null;
	}

	public CustomPack GetPack(string name)
	{
		for (int i = 0; i < packs.Count; ++i)
		{
			CustomPack pack = packs[i];
			if (pack.name == name)
				return pack;
		}
		return null;
	}

	public CustomItem GetItem(string name)
	{
		for (int i = 0; i < items.Count; ++i)
		{
			CustomItem item = items[i];
			if (item.name == name)
				return item;
		}
		return null;
	}
}
