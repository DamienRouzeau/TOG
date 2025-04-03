using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [System.Serializable]
    public class ProgressionLevelData
    {
        public string levelId;
        public bool[] unlockedSkulls = new bool[5];
        public int bestScore;

        public int GetUnlockedCount()
		{
            int count = 0;
            for (int i = 0; i < unlockedSkulls.Length; ++i)
                count += unlockedSkulls[i] ? 1 : 0;
            return count;
		}

        public void ResetSkulls()
		{
            for (int i = 0; i < unlockedSkulls.Length; ++i)
                unlockedSkulls[i] = false;
        }
    }

    [System.Serializable]
    public class ProgressionData
	{
        public List<ProgressionLevelData> levels = null;

        public ProgressionLevelData GetLevelFromId(string id)
		{
            if (levels == null)
                levels = new List<ProgressionLevelData>();

            foreach (ProgressionLevelData level in levels)
			{
                if (level.levelId == id)
                    return level;
			}
            ProgressionLevelData newLevel = new ProgressionLevelData();
            newLevel.levelId = id;
            levels.Add(newLevel);
            return newLevel;
		}

        public int GetUnlockedSkulls()
		{
            int count = 0;
            if (levels != null && levels.Count > 0)
            {
                foreach (ProgressionLevelData level in levels)
                {
                    count += level.GetUnlockedCount();
                }
            }
            return count;
        }
    }

    [System.Serializable]
    public class PackData
    {
        public string name = null;
        public bool unlocked = false;
        public int cost = 0;

        public void Create(string n)
		{
            name = n;
            unlocked = false;
            cost = 0;
		}

        public void Unlock(int c)
		{
            unlocked = true;
            cost = c;
		}
    }

    [System.Serializable]
    public class FeatureData
    {
        public string name = null;
        public bool unlocked = false;
        public int cost = 0;

        public void Create(string n)
        {
            name = n;
            unlocked = false;
            cost = 0;
        }

        public void Unlock(int c)
        {
            unlocked = true;
            cost = c;
        }
    }

    [System.Serializable]
    public class CustomData
    {
        public List<PackData> packs = null;
        public List<FeatureData> features = null;
        public string hat = null;

        public PackData GetPack(string name)
		{
            if (packs != null)
			{
                foreach (PackData pack in packs)
				{
                    if (pack.name == name)
                        return pack;
				}
			}
            return null;
		}

        public FeatureData GetFeature(string name)
        {
            if (features != null)
            {
                foreach (FeatureData feature in features)
                {
                    if (feature.name == name)
                        return feature;
                }
            }
            return null;
        }

        public void UnlockPack(string name, int cost)
		{
            if (packs == null)
                packs = new List<PackData>();
            PackData pack = GetPack(name);
            if (pack == null)
			{
                pack = new PackData();
                pack.Create(name);
                packs.Add(pack);
            }
            pack.Unlock(cost);
		}

        public void UnlockFeature(string name, int cost)
        {
            if (features == null)
                features = new List<FeatureData>();
            FeatureData feature = GetFeature(name);
            if (feature == null)
            {
                feature = new FeatureData();
                feature.Create(name);
                features.Add(feature);
            }
            feature.Unlock(cost);
        }

        public bool IsPackUnlocked(string name)
        {
            PackData pack = GetPack(name);
            return pack != null && pack.unlocked;
        }

        public bool IsFeatureUnlocked(string name)
        {
            FeatureData feature = GetFeature(name);
            return feature != null && feature.unlocked;
        }
    }

    [System.Serializable]
    public class ProfileData
	{
        public ProgressionData progression;
        public string name;
        public string avatar;
        public int coins;
        public float musicVolume;
        public float voicesVolume;
        public float sfxVolume;
        public CustomData custom;

        public ProfileData()
        {
            progression = new ProgressionData();
            custom = new CustomData();
            name = "";
            avatar = "Matey_Man01";
            musicVolume = 0.33f;
            voicesVolume = 1f;
            sfxVolume = 0.66f;
        }

        public ProfileData(string profileName, string profileAvatar)
		{
            progression = new ProgressionData();
            custom = new CustomData();
            name = profileName;
            avatar = profileAvatar;
        }

        public void UnlockPack(string name, int cost)
        {
            if (custom == null)
                custom = new CustomData();
            if (coins >= cost)
            {
                custom.UnlockPack(name, cost);
                coins -= cost;
            }
        }

        public PackData GetPack(string name)
        {
            if (custom == null)
                custom = new CustomData();
            return custom.GetPack(name);
        }

        public bool IsPackUnlocked(string name)
		{
            if (custom == null)
                custom = new CustomData();
            return custom.IsPackUnlocked(name);
        }

        public void UnlockFeature(string name, int cost)
        {
            if (custom == null)
                custom = new CustomData();
            if (coins >= cost)
            {
                custom.UnlockFeature(name, cost);
                coins -= cost;
            }
        }

        public FeatureData GetFeature(string name)
        {
            if (custom == null)
                custom = new CustomData();
            return custom.GetFeature(name);
        }

        public bool IsFeatureUnlocked(string name)
        {
            if (custom == null)
                custom = new CustomData();
            return custom.IsFeatureUnlocked(name);
        }
    }

    public static SaveManager myself = null;

    public int profileCount => _profiles?.Count ?? 0;
    public int profileIdx => _currentProfileIdx;

    private const string PROFILES = "Profiles";
    private const string CURRENT_PROFILE_IDX = "CurrentProfileIndex";

    private List<ProfileData> _profiles = null;
    private int _currentProfileIdx = 0;

    public ProfileData profile
    {
        get
        {
#if USE_STANDALONE
            if (profileCount == 0)
                NewProfile();
            return _profiles[_currentProfileIdx];
#else
            return null;
#endif
        }
    }

    private void Awake()
    {
        myself = this;
        DontDestroyOnLoad(gameObject);
#if USE_STANDALONE
        _currentProfileIdx = ES3.Load(CURRENT_PROFILE_IDX, 0);
        Load();
#endif
    }

	public void Load()
	{
#if USE_STANDALONE
        if (ES3.FileExists())
        {
            _profiles = ES3.Load(PROFILES) as List<ProfileData>;
        }
        if (_profiles == null)
            _profiles = new List<ProfileData>();
        if (_profiles.Count > 1)
            _currentProfileIdx = Mathf.Clamp(_currentProfileIdx, 0, _profiles.Count - 1);
        else
            _currentProfileIdx = 0;
#endif
    }

    public void Save()
	{
#if USE_STANDALONE
        ES3.Save(PROFILES, _profiles);
#endif
    }

    public ProfileData GetProfile(int idx)
	{
        if (_profiles == null)
            return null;
        if (idx < 0 || idx >= _profiles.Count)
            return null;
        return _profiles[idx];
    }

    public void NewProfile()
    {
#if USE_STANDALONE
        if (_profiles == null)
            _profiles = new List<ProfileData>();
        _profiles.Add(new ProfileData());
#endif
    }

    public void SetCurrentProfileIndex(int idx)
	{
#if USE_STANDALONE
        _currentProfileIdx = idx;
        ES3.Save(CURRENT_PROFILE_IDX, _currentProfileIdx);
#endif
    }

    public void DeleteProfile(int idx)
	{
        if (GetProfile(idx) != null)
        {
            _profiles.RemoveAt(idx);
            Save();
        }
        if (_currentProfileIdx >= idx && _currentProfileIdx > 0)
        {
            SetCurrentProfileIndex(_currentProfileIdx - 1);
        }
    }
}
