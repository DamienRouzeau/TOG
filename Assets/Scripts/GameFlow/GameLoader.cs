#if USE_STANDALONE
#define SAVE_LANGUAGE
#endif

using CurvedUI;
using Photon.Pun;
#if USE_STEAM
using Steamworks;
#endif
using UnityEngine;
using Valve.VR.InteractionSystem;

public class GameLoader : MonoBehaviour
{
	public enum Product
	{
		Arcade,
		Standalone
	}

	public enum AudioLanguage
	{
		Default,
		French,
		English,
		Spanish,
		German
	}

	public enum WrittenLanguage
	{
		Default,
		French,
		English,
		Spanish,
		German
	}

	[System.Serializable]
	public class SettingsCoefsByProduct : RREnumArray<Product, gamesettings_coefs> { }

#region Properties

	public static GameLoader myself = null;
#if USE_DEMO_MODE
	public static string demoKey = "unity.desc";
#endif
#if SAVE_LANGUAGE
	public static string langKeyWritten = "unity.lang";
	public static string langKeyAudio = "unity.lang.audio";
#endif

	[Header("Prefabs to instantiate if needed")]
	public SaveManager saveManagerPrefab = null;
	public CurvedUIEventSystem eventSystemPrefab = null;
	public gamesettings_general gameSettingsGeneralPrefab = null;
	public gamesettings_ui gameSettingsUiPrefab = null;
	public gamesettings_player gameSettingsPlayerPrefab = null;
	public gamesettings_screen gameSettingsSceenPrefab = null;
	public gamesettings_difficulty gameSettingsDifficultyPrefab = null;
	public gamesettings_endscreen gameSettingsEndScreenPrefab = null;
	public gamesettings_boat gameSettingsBoatPrefab = null;
	public gamesettings gameSettingsRacePrefab = null;
	public SettingsCoefsByProduct gamesettingsCoefsPrefabs = null;
	public poolhelper poolHelperPrefab = null;
	public PhotonNetworkController multiplayerPrefab = null;
    public CheatCodeManager cheatCodeManagerPrefab = null;
	public PlayMakerGUI playMakerGuiPrefab = null;
	public VoiceManager voiceManagerPrefab = null;
	public avatar_root avatarRootPrefab = null;
	public LaunchSceneASync launchScenePrefab = null;
	public SynthesisArcadeObject synthesisPrefab = null;

	[Header("Start from Race parameters")]
	public boat_followdummy.TeamColor teamColor = boat_followdummy.TeamColor.Blue;
	public bool useGhostBoat = false;

	[Header("License parameters")]
    public bool multiplayerUseLauncher = true;
    public string multiplayerRoomName = "TestRoomName";
	public string multiplayerRegion = "eu";
	public AudioLanguage forcedAudioLanguage = AudioLanguage.Default;
	public WrittenLanguage forcedWrittenLanguage = WrittenLanguage.Default;
	public string license_overridemachineid = "";
	public string machineName = "";
	public int startingLevel = 0;
	public int startingWave = 0;
	public string editorArcadeCode = null;
	public bool useVoip = true;
	public apicalls licencePrefab = null;
	public bool useLiveServer = false;
	[Range(0, 60)]
	public int endlessModeDurationInMin = 0;
	public multiplayerlobby.GameMode gameMode = multiplayerlobby.GameMode.Normal;
	public multiplayerlobby.SkinTheme theme = multiplayerlobby.SkinTheme.Normal;

	[Header("Bots")]
	public bool isBotScene = false;
	public bool simulateLocalBots = false;
	public int localBotCount = 15;
	public bool localBotsOnSameBoat = false;

	public bool isInit => _isInit;
	private bool _isInit = false;

	public bool isLaunchedFromRace { get; private set; } = false;
	public bool isLaunchedFromLobby { get; private set; } = false;

	private static bool _isVoipLoaded = false;

#endregion

	private void Awake()
	{
		myself = this;
#if !UNITY_EDITOR
		simulateLocalBots = false;
#if USE_LAUNCHER
		multiplayerUseLauncher = true;
#else
		multiplayerUseLauncher = false;
#endif
#endif
		Debug.Log($"[LAUNCHER] {(multiplayerUseLauncher ? "Activated!" : "Deactivated!")}");
	}

	// Start is called before the first frame update
	void Start()
	{
		LoadAllPrefabs();
	}

	//private void Update()
	//{
	//	string[] array = Input.GetJoystickNames();
	//	if (array != null)
	//	{
	//		foreach (string joystick in array)
	//		{
	//			if (!string.IsNullOrEmpty(joystick))
	//				Debug.Log($"[JOYSTICKS] {joystick}");
	//		}
	//	}
	//}

	public void LoadAllPrefabs()
	{
		Product product = Product.Arcade;
#if USE_STANDALONE
		product = Product.Standalone;
#endif
		isLaunchedFromRace = false;
		isLaunchedFromLobby = false;

#if USE_STEAM
		Debug.Log("[USE_STEAM] Activated!");
		if (SteamManager.Initialized)
		{
			Debug.Log("[USE_STEAM] Initialized! " + SteamFriends.GetPersonaName());
		}
		else
		{
			Debug.Log("[USE_STEAM] Not initialized!");
			Application.Quit();
		}
#endif

#if USE_VIVE
		Debug.Log("[USE_VIVE] Activated!");
		ViveportManager.Init((result)=>
		{
			if (result)
			{
				Debug.Log("[USE_VIVE] Initialized!");
			}
			else
			{
				Debug.Log("[USE_VIVE] Not initialized!");
				Application.Quit();
			}
		});
#endif

		if (SaveManager.myself == null)
		{
			GameObject.Instantiate(saveManagerPrefab);
		}
#if !USE_STANDALONE
		if (apicalls.myself == null)
		{
			apicalls obj = GameObject.Instantiate(licencePrefab);
#if UNITY_EDITOR
			obj._debugInLiveServer = useLiveServer;
#endif
			LoadMachineId(obj);
		}
#endif
		if (gamesettings_coefs.myself == null)
		{
			gamesettings_coefs prefab =  gamesettingsCoefsPrefabs[product];
			if (prefab != null)
				GameObject.Instantiate(prefab);
		}
		if (gamesettings_general.myself == null)
		{
			GameObject.Instantiate(gameSettingsGeneralPrefab);
		}
		if (gamesettings_ui.myself == null)
		{
			GameObject.Instantiate(gameSettingsUiPrefab);
		}
		if (gamesettings_player.myself == null)
		{
			GameObject.Instantiate(gameSettingsPlayerPrefab);
		}
		if (gamesettings_screen.myself == null)
		{
			GameObject.Instantiate(gameSettingsSceenPrefab);
		}
		if (gamesettings_difficulty.myself == null)
		{
			GameObject.Instantiate(gameSettingsDifficultyPrefab);
		}
		if (gameSettingsBoatPrefab != null && gamesettings_boat.myself == null)
		{
			GameObject.Instantiate(gameSettingsBoatPrefab);
		}
		if (gameSettingsEndScreenPrefab != null && gamesettings_endscreen.myself == null)
		{
			GameObject.Instantiate(gameSettingsEndScreenPrefab);
		}
		if (gameSettingsRacePrefab != null && gamesettings.myself == null)
		{
			GameObject.Instantiate(gameSettingsRacePrefab);
		}
		if (poolhelper.myself == null)
		{
			GameObject.Instantiate(poolHelperPrefab);
		}
#if UNITY_EDITOR
		if (StarterHub.myself == null)
		{
			if (pointfromhand.teleporttarget == null)
			{
				if (avatar_root.myself == null)
					GameObject.Instantiate(avatarRootPrefab);
				// Special case -> begin from race scene
				isLaunchedFromRace = !isBotScene;
				isLaunchedFromLobby = false;
			}
		}
		else
		{
			isLaunchedFromRace = false;
			isLaunchedFromLobby = StarterHub.myself.hubType == StarterHub.HubType.BeforeRace;
		}
#endif
		if (Player.myplayer == null)
		{
			GameObject goPlayer = GameObject.Instantiate(gamesettings_player.myself.playerPrefab);
			DontDestroyOnLoad(goPlayer);
		}
		if (CurvedUIEventSystem.instance == null)
		{
			GameObject.Instantiate(eventSystemPrefab);
		}
		if (PhotonNetworkController.myself == null)
		{
			PhotonNetworkController net = GameObject.Instantiate(multiplayerPrefab);
			if (multiplayerUseLauncher)
			{
				net.UseLauncher = true;
				net.GetComponent<multiplayerlobby>().enabled = true;
			}
			else
			{
#if USE_DEMO_MODE
				Debug.Log("[USE_DEMO_MODE] Activated!");
				int count = GetDemoCounter();
				Debug.Log("Demo counter " + count);
#endif
				LoadRoomId();
				LoadGameMode();
				LoadSkinTheme();
				LoadEndlessDuration();
				LoadStartWave();
				LoadStartLevel();
			}
		}
		if (CheatCodeManager.myself == null)
		{
			GameObject.Instantiate(cheatCodeManagerPrefab);
		}
		if (PlayMakerGUI.Instance == null)
		{
			GameObject.Instantiate(playMakerGuiPrefab);
		}
		else
		{
			if (PlayMakerGUI.Instance.gameObject.GetComponent<DontDestroyOnLoad>() == null)
				PlayMakerGUI.Instance.gameObject.AddComponent<DontDestroyOnLoad>();
		}
#if !USE_STANDALONE
		if (VoiceManager.myself == null)
		{
			LoadVoip();
			if (multiplayerlobby.useVoip)
				GameObject.Instantiate(voiceManagerPrefab);
		}
#endif
		if (LaunchSceneASync.myself == null)
		{
			GameObject.Instantiate(launchScenePrefab);
		}
#if USE_SYNTHESIS && IS_MASTER
		Debug.Log("[USE_SYNTHESIS] Activated!");
		if (SynthesisArcadeObject.Instance == null)
		{
			GameObject.Instantiate(synthesisPrefab);
		}
#endif
		_isInit = true;
	}

	public void InitVoiceManager()
	{
		if (VoiceManager.myself == null)
		{
			GameObject.Instantiate(voiceManagerPrefab);
		}
	}

	public void DestroyVoiceManager()
	{
		if (VoiceManager.myself != null)
		{
			GameObject.Destroy(VoiceManager.myself.gameObject);
		}
	}

	public void LoadRoomId()
	{
#if USE_DEMO_MODE
		Debug.Log("[USE_DEMO_MODE] Activated!");
		string roomId = "TOG_DEMO_RoomId_" + System.Guid.NewGuid().ToString("F");
#elif UNITY_EDITOR
		string roomId = multiplayerRoomName;
#else
		// RoomId
		string roomId = null;
		string path = Application.streamingAssetsPath + "/mp_roomId.txt";
		if (System.IO.File.Exists(path))
		{
			roomId = System.IO.File.ReadAllText(path);
		}
#endif
		if (!string.IsNullOrEmpty(roomId))
		{
			Debug.Log("[FORCE_ROOM_ID] " + roomId);
			PhotonNetworkController.myself.roomname = roomId;
		}
	}

	public void LoadMachineName()
	{
		// Machine Name
#if UNITY_EDITOR
		string name = machineName;
#else
		string name = null;
		string path = Application.streamingAssetsPath + "/mp_machineName.txt";
		if (System.IO.File.Exists(path))
		{
			name = System.IO.File.ReadAllText(path);
		}
#endif
		if (!string.IsNullOrEmpty(name))
		{
			Debug.Log("[FORCE_MACHINE_NAME] " + name);
			multiplayerlobby.machineName = name;
		}
	}

	public void LoadMachineId(apicalls obj)
	{
		// Machine Id
#if USE_DEMO_MODE
		string machineId = "";
#elif UNITY_EDITOR
		string machineId = license_overridemachineid;
#else
		string machineId = null;
		string path = Application.streamingAssetsPath + "/mp_machineId.txt";
		if (System.IO.File.Exists(path))
		{
			machineId = System.IO.File.ReadAllText(path);
		}
#endif
		if (!string.IsNullOrEmpty(machineId))
		{
			Debug.Log("[FORCE_MACHINE_ID] " + machineId);
			obj.overridemachineid = machineId;
		}
	}

	public void LoadStartLevel()
	{
#if UNITY_EDITOR
		int startLevel = startingLevel;
#else
		int startLevel = 0;
		string path = Application.streamingAssetsPath + "/mp_level.txt";
		if (System.IO.File.Exists(path))
		{
			string levelText = System.IO.File.ReadAllText(path);
			if (int.TryParse(levelText, out int level))
			{
				startLevel = level;
			}
		}
#endif
		if (startLevel > 0)
		{
			Debug.Log("[FORCE_LEVEL] " + startLevel);
			multiplayerlobby.startLevel = startLevel - 1;
		}
	}

	public void LoadStartWave()
	{
#if UNITY_EDITOR
		int startWave = startingWave;
#else
		int startWave = 0;
		string path = Application.streamingAssetsPath + "/mp_wave.txt";
		if (System.IO.File.Exists(path))
		{
			string waveText = System.IO.File.ReadAllText(path);
			if (int.TryParse(waveText, out int wave))
			{
				startWave = wave;
			}
		}
#endif
		if (startWave > 0)
		{
			Debug.Log("[FORCE_WAVE] " + startWave);
			multiplayerlobby.startWave = startWave - 1;
		}
	}

	public void LoadEndlessDuration()
	{
		// Endless duration
#if UNITY_EDITOR
		int timeInSecond = endlessModeDurationInMin * 60;
#else
		int timeInSecond = 0;
		string path = Application.streamingAssetsPath + "/mp_endless.txt";
		if (System.IO.File.Exists(path))
		{
			string duration = System.IO.File.ReadAllText(path);
			if (!string.IsNullOrEmpty(duration))
			{
				int result;
				if (int.TryParse(duration, out result))
				{
					timeInSecond = result * 60;
				}
			}
				
		}
#endif
		if (timeInSecond > 0)
		{
			multiplayerlobby.endlessDuration = timeInSecond;
			multiplayerlobby.gameMode = multiplayerlobby.GameMode.Endless;
			Debug.Log("[FORCE_ENDLESS] " + timeInSecond);
		}
		else
		{
			multiplayerlobby.endlessDuration = 0;
		}
	}

	public void LoadGameMode()
	{
		// Game Mode
#if UNITY_EDITOR
		multiplayerlobby.gameMode = gameMode;
		if (gameMode == multiplayerlobby.GameMode.Endless)
			multiplayerlobby.endlessDuration = endlessModeDurationInMin * 60;
		else
			multiplayerlobby.endlessDuration = 0;
#else
		string path = Application.streamingAssetsPath + "/mp_gamemode.txt";
		if (System.IO.File.Exists(path))
		{
			string text = System.IO.File.ReadAllText(path);
			if (!string.IsNullOrEmpty(text))
			{
				string[] split = text.Split( new char[] {' ', '\n'});
				if (split.Length > 0)
				{
					if (!System.Enum.TryParse(split[0], out multiplayerlobby.gameMode))
					{
						multiplayerlobby.gameMode = multiplayerlobby.GameMode.Normal;
					}

					Debug.Log("[FORCE_GAMEMODE] " + multiplayerlobby.gameMode);

					if (multiplayerlobby.gameMode == multiplayerlobby.GameMode.Endless)
					{
						multiplayerlobby.endlessDuration = 300;
						if (split.Length > 1)
						{
							int result;
							if (int.TryParse(split[1], out result))
							{
								multiplayerlobby.endlessDuration = result * 60;
								Debug.Log("[FORCE_DURATION] " + multiplayerlobby.endlessDuration);
							}
						}
					}
					else
					{
						multiplayerlobby.endlessDuration = 0;
					}
				}
			}	
		}
#endif
	}

	public void LoadSkinTheme()
	{
		// Skin Theme
#if UNITY_EDITOR
		multiplayerlobby.theme = theme;
#else
		string path = Application.streamingAssetsPath + "/mp_theme.txt";
		if (System.IO.File.Exists(path))
		{
			string text = System.IO.File.ReadAllText(path);
			if (!System.Enum.TryParse(text, out multiplayerlobby.theme))
			{
				multiplayerlobby.theme = multiplayerlobby.SkinTheme.Normal;
			}

			Debug.Log("[FORCE_THEME] " + multiplayerlobby.theme);
		}
#endif
	}

	public static string LoadWrittenLanguage(string lang)
	{
#if UNITY_EDITOR
		if (myself != null)
		{
			switch (myself.forcedWrittenLanguage)
			{
				case WrittenLanguage.French:
					lang = "fr-FR";
					break;
				case WrittenLanguage.English:
					lang = "en-US";
					break;
				case WrittenLanguage.Spanish:
					lang = "es-ES";
					break;
				case WrittenLanguage.German:
					lang = "de-DE";
					break;
			}
		}
#else
		lang = null;
#endif
#if SAVE_LANGUAGE
		// Language
		if (PlayerPrefs.HasKey(langKeyWritten))
		{
			lang = PlayerPrefs.GetString(langKeyWritten, null);
		}
#endif

#if !UNITY_EDITOR
		if (string.IsNullOrEmpty(lang))
		{
			string path = Application.streamingAssetsPath + "/mp_lang.txt";
			if (System.IO.File.Exists(path))
			{
				lang = System.IO.File.ReadAllText(path);
			}
		}
#endif

		if (!string.IsNullOrEmpty(lang))
		{
			Debug.Log("[FORCE_WRITTEN_LANG] " + lang);
		}
		return lang;
	}

	public static string LoadAudioLanguage(string lang)
	{
#if UNITY_EDITOR
		if (myself != null)
		{
			switch (myself.forcedAudioLanguage)
			{
				case AudioLanguage.French:
					lang = "fr-FR";
					break;
				case AudioLanguage.English:
					lang = "en-US";
					break;
				case AudioLanguage.Spanish:
					lang = "es-ES";
					break;
				case AudioLanguage.German:
					lang = "de-DE";
					break;
			}
		}
#else
		lang = null;
#endif
#if SAVE_LANGUAGE
		// Language
		if (PlayerPrefs.HasKey(langKeyAudio))
		{
			lang = PlayerPrefs.GetString(langKeyAudio, null);
		}
#endif

#if !UNITY_EDITOR
		if (string.IsNullOrEmpty(lang))
		{
			string path = Application.streamingAssetsPath + "/mp_langAudio.txt";
			if (System.IO.File.Exists(path))
			{
				lang = System.IO.File.ReadAllText(path);
			}
		}
#endif

		if (!string.IsNullOrEmpty(lang))
		{
			Debug.Log("[FORCE_AUDIO_LANG] " + lang);
		}
		return lang;
	}

	public static void SaveWrittenLanguage(string lang)
	{
#if SAVE_LANGUAGE
		PlayerPrefs.SetString(langKeyWritten, lang);
		PlayerPrefs.Save();
#endif
	}

	public static void SaveAudioLanguage(string lang)
	{
#if SAVE_LANGUAGE
		PlayerPrefs.SetString(langKeyAudio, lang);
		PlayerPrefs.Save();
#endif
	}

	public static string LoadArcadeCode(string code = null)
	{
#if USE_SYNTHESIS
		code = "äÎÙÃßÒÄÞÄöÅÔÖÓÒçÖÄÄôØÓÒèãøðÝßÐÖÆÄÑÓÿöÚ^ÛÞÒãòóÿý";
#endif

		if (string.IsNullOrEmpty(code))
		{
#if UNITY_EDITOR
			if (myself != null)
				return myself.editorArcadeCode;
#else
			string path = Application.streamingAssetsPath + "/mp_arcadeCode.txt";
			if (System.IO.File.Exists(path))
			{
				code = System.IO.File.ReadAllText(path);
			}
#endif
		}
		if (!string.IsNullOrEmpty(code))
		{
			Debug.Log("[FORCE_ARCADE_CODE] detected");
			return UtilsString.Decrypt(code);
		}
		return null;
	}

	public static void LoadRegion()
	{
		string multiplayerRegion = null;

		if (myself != null)
			multiplayerRegion = myself.multiplayerRegion;

		// Region
#if !UNITY_EDITOR
        string path = Application.streamingAssetsPath + "/mp_region.txt";
        if (System.IO.File.Exists(path))
        {
            multiplayerRegion = System.IO.File.ReadAllText(path);
        }
#endif
		if (!string.IsNullOrEmpty(multiplayerRegion))
		{
			Debug.Log("[FORCE_REGION] " + multiplayerRegion);
			PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = multiplayerRegion;
		}
	}

	public static void LoadVoip()
	{
		if (_isVoipLoaded)
			return;

		string voip = null;

#if UNITY_EDITOR
		if (myself != null)
			voip = myself.useVoip ? "1" : "0";
#else
		// VOIP
        string path = Application.streamingAssetsPath + "/mp_voip.txt";
        if (System.IO.File.Exists(path))
        {
            voip = System.IO.File.ReadAllText(path);
        }
#endif
		if (!string.IsNullOrEmpty(voip))
		{
			Debug.Log("[FORCE_VOIP] " + voip);
			multiplayerlobby.useVoip = voip == "1";
		}

		_isVoipLoaded = true;
	}

#if UNITY_EDITOR

	[ContextMenu("Enable OpenVR")]
	public void EnableOpenVR()
	{
		string[] sdkArray = new string[] { "OpenVR", "None" };
		UnityEditorInternal.VR.VREditor.SetVREnabledDevicesOnTargetGroup(UnityEditor.BuildTargetGroup.Standalone, sdkArray);
	}

	[ContextMenu("Disable OpenVR")]
	public void DisableOpenVR()
	{
		string[] sdkArray = new string[] { "None", "OpenVR" };
		UnityEditorInternal.VR.VREditor.SetVREnabledDevicesOnTargetGroup(UnityEditor.BuildTargetGroup.Standalone, sdkArray);
	}
#endif

#if USE_DEMO_MODE
	public static int GetDemoCounter()
	{
		if (PlayerPrefs.HasKey(demoKey))
		{
			string value = PlayerPrefs.GetString(demoKey);
			string countString = UtilsString.Decrypt(value);
			int count;
			if (int.TryParse(countString, out count))
			{
				return count;
			}
		}
		else
		{
			int count = 8;
			SetDemoCounter(count);
			return count;
		}
		return 0;
	}

	public static void SetDemoCounter(int count)
	{
		string countString = UtilsString.Encrypt(count.ToString());
		PlayerPrefs.SetString(demoKey, countString);
		PlayerPrefs.Save();
	}
#endif
}
