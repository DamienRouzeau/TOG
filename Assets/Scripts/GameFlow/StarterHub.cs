using System.Collections;
using UnityEngine;
using TMPro;
using RRLib;
using UnityEngine.UI;
using Photon.Pun;

public class StarterHub : MonoBehaviour
{
	#region Enums

	public enum HubType
	{
		Cabin,
		BeforeRace
	}

	#endregion

	#region Properties

	public static StarterHub myself = null;

	public AudioSource hubmusic = null;
	public avatar_root avatarRoot = null;
	public spawnpoint spawn = null;
	public HubType hubType = HubType.Cabin;

	public GameObject[] gameObjectsToActivateAtInit = null;
	public GameObject[] gameObjectsToActivateAtSpawnedPlayer = null;
	public GameObject[] gameObjectsToActivateAtGameFlow = null;
	public GameObject[] gameObjectsToActivateAtKeyboardValidation = null;
	public GameObject[] gameObjectsToDeactivateAtKeyboardValidation = null;
	public TextMeshProUGUI[] textToActivateAtKeyboardValidation = null;
	public GameObject[] gameObjectsToDeactivateAtTeamValidation = null;
	public GameObject[] gameObjectsToDeactivateAtInitData = null;
	public VoiceOver voiceOverAtKeyboardValidationMaster = null;
	public VoiceOver voiceOverAtKeyboardValidationClient = null;
	public SandClock sandClock = null;
	public float overrideDurationForSandClock = 0f;

	public LaunchSceneASync raceLauncher = null;
	[SerializeField]
	private bool _forceWeapons = false;
	[SerializeField]
	private Player.WeaponType _forceWeaponTypeAtLeft = Player.WeaponType.None;
	[SerializeField]
	private Player.WeaponType _forceWeaponTypeAtRight = Player.WeaponType.None;
	[SerializeField]
	private startgamebutton _pupitre = null;
	[SerializeField]
	private UI_ChooseYourName _chooseYourName = null;

	[Header("Languages panel")]
	[SerializeField]
	private GameObject _languagesPanel = null;
	[SerializeField]
	private Button _validLanguagesBtn = null;

	[Header("Player Settings panel")]
	[SerializeField]
	private GameObject _playerSettingsPanel = null;
	[SerializeField]
	private Button _validPlayerSettingsBtn = null;
	[SerializeField]
	private Button _cancelPlayerSettingsBtn = null;
	[SerializeField]
	private UI_Volumes _uiVolumes = null;
	[SerializeField]
	private UI_GameConfiguration _uiGameConfig = null;

	[Header("Levels")]
	[SerializeField]
	private Button[] _directLevelBtnArray = null;

	[Header("Avatar")]
	[SerializeField]
	private UI_ChooseAvatar _chooseAvatar = null;

	public startgamebutton pupitre => _pupitre;

	public bool isStarterHubInit => _isStarterHubInit;
	private bool _isStarterHubInit = false;

	public float durationSandclock => overrideDurationForSandClock > 0f ? overrideDurationForSandClock : gamesettings_general.myself.timerMaxDurationInLobby;

	#endregion

	private void Awake()
    {
		if (myself != null)
			Destroy(myself.gameObject);

		myself = this;

		if (gameObjectsToActivateAtInit != null)
		{
			for (int i = 0; i < gameObjectsToActivateAtInit.Length; ++i)
			{
				gameObjectsToActivateAtInit[i].SetActive(false);
			}
		}

		if (gameObjectsToActivateAtSpawnedPlayer != null)
		{
			for (int i = 0; i < gameObjectsToActivateAtSpawnedPlayer.Length; ++i)
			{
				gameObjectsToActivateAtSpawnedPlayer[i].SetActive(false);
			}
		}

		if (gameObjectsToActivateAtKeyboardValidation != null)
		{
			for (int i = 0; i < gameObjectsToActivateAtKeyboardValidation.Length; ++i)
			{
				gameObjectsToActivateAtKeyboardValidation[i].SetActive(false);
			}
		}

		if (_languagesPanel != null)
		{
			_languagesPanel.SetActive(true);
			if (_playerSettingsPanel != null)
				_playerSettingsPanel.SetActive(false);
			if (_uiGameConfig != null)
				_uiGameConfig.gameObject.SetActive(false);
		}
		if (_validLanguagesBtn != null)
			_validLanguagesBtn.onClick.AddListener(OnValidLanguageClicked);
		if (_validPlayerSettingsBtn != null)
			_validPlayerSettingsBtn.onClick.AddListener(OnValidPlayerSettingsClicked);
		if (_cancelPlayerSettingsBtn != null)
			_cancelPlayerSettingsBtn.onClick.AddListener(OnCancelPlayerSettingsClicked);
	}

	void Start()
	{
		StartCoroutine(InitStarterHubEnum());
#if USE_KDK || USE_BOD
		selectflag.onSelectFlag += OnSelectFlag;
#endif
	}

	private void OnDestroy()
	{
#if USE_KDK || USE_BOD
		selectflag.onSelectFlag -= OnSelectFlag;
#endif
		if (_validLanguagesBtn != null)
			_validLanguagesBtn.onClick.RemoveListener(OnValidLanguageClicked);
		if (_validPlayerSettingsBtn != null)
			_validPlayerSettingsBtn.onClick.RemoveListener(OnValidPlayerSettingsClicked);
		if (_cancelPlayerSettingsBtn != null)
			_cancelPlayerSettingsBtn.onClick.RemoveListener(OnCancelPlayerSettingsClicked);

		if (sandClock != null && hubType == HubType.Cabin)
		{
			sandClock.onSandClockOverCbk -= OnSandClockOver;
		}
	}

	private IEnumerator InitStarterHubEnum()
	{
		while (!GameLoader.myself.isInit)
			yield return null;

		while (Player.myplayer == null)
			yield return null;

		while (!Player.myplayer.isInit)
			yield return null;

		if (_chooseAvatar != null)
			_chooseAvatar.ChooseAvatar(0);

		if (Player.myplayer != null && avatarRoot != null)
		{
			Player.myplayer.InitTeleporterTarget(avatarRoot);
			Player.myplayer.SetStartSpawnPoint(spawn.transform);
			Player.myplayer.TeleportOnSpawnPoint();
		}

		if (Player.myplayer != null && _forceWeapons)
		{
			Player.myplayer.ChangeTypeOfWeaponData(0, _forceWeaponTypeAtLeft);
			Player.myplayer.ChangeTypeOfWeaponData(1, _forceWeaponTypeAtRight);
		}

		if (gameObjectsToActivateAtInit != null)
		{
			for (int i = 0; i < gameObjectsToActivateAtInit.Length; ++i)
			{
				gameObjectsToActivateAtInit[i].SetActive(true);
			}
		}

		yield return new WaitForSeconds(0.5f);

#if USE_KDK || USE_BOD
		if (_uiVolumes != null)
		{
			_uiVolumes.SetMusicVolume(0.33f);
			_uiVolumes.SetVoicesVolume(1f);
			_uiVolumes.SetSFXVolume(0.66f);
		}
#endif

		gamesettings_general.myself.FadeInMusicVolume();

		if (!Player.myplayer.isInPause)
		{
			gamesettings_screen.myself.FadeIn();
			while (gamesettings_screen.myself.faderunning)
				yield return null;
		}

		if (gameObjectsToActivateAtSpawnedPlayer != null)
		{
			for (int i = 0; i < gameObjectsToActivateAtSpawnedPlayer.Length; ++i)
			{
#if USE_STANDALONE
				if (!PhotonNetworkController.soloMode && gameObjectsToActivateAtSpawnedPlayer[i].name.ToLower().Contains("tuto"))
					continue;
#endif
				gameObjectsToActivateAtSpawnedPlayer[i].SetActive(true);
			}
		}

		VoiceManager.myself?.ReturnToGlobalVoiceRoom();

		if (hubType == HubType.Cabin)
			StartCoroutine(WaitForKeyboardValidation());

		if (hubType == HubType.BeforeRace && InitGameData.instance != null)
		{
			foreach (InitGameData.InitPlayerData playerData in InitGameData.instance.playerDatas)
			{
				if (playerData.id == GameflowBase.myId)
					GameflowBase.myTeam = playerData.team;

				GameflowBase.SetTeamTables(true, playerData.team, playerData.id);
			}

			if (_pupitre != null)
				_pupitre.gameObject.SetActive(true);

			if (gameObjectsToDeactivateAtInitData != null)
			{
				for (int i = 0; i < gameObjectsToDeactivateAtInitData.Length; ++i)
				{
					gameObjectsToDeactivateAtInitData[i].SetActive(false);
				}
			}
		}

		_isStarterHubInit = true;
	}

	private IEnumerator WaitForKeyboardValidation()
	{
		// Wait Keyboard apparition
		while (GameflowBase.instance == null)
			yield return null;

		if (gameObjectsToActivateAtGameFlow != null)
		{
			for (int i = 0; i < gameObjectsToActivateAtGameFlow.Length; ++i)
			{
				gameObjectsToActivateAtGameFlow[i].SetActive(true);
			}
		}

		if (InitGameData.instance != null)
		{
			if (_chooseYourName != null)
			{
				while (!_chooseYourName.gameObject.activeInHierarchy)
					yield return null;

				yield return new WaitForSeconds(0.2f);

				string playerName = InitGameData.instance.GetPlayerName(GameflowBase.myId);
				if (!string.IsNullOrEmpty(playerName))
				{
					_chooseYourName.Init(UI_ChooseYourName.NameType.Player, playerName);
					_chooseYourName.OnNameConfirmed();
				}
				if (multiplayerlobby.product == multiplayerlobby.Product.TOG && PhotonNetworkController.IsMaster())
				{
					while (GameflowBase.playerCount < InitGameData.instance.playerDatas.Count)
						yield return null;

					while (!gameflowmultiplayer.AreAllFlowsAtState(gameflowmultiplayer.GameState.ChooseLevel))
						yield return null;

					ChoiceButtons.SetCabinLevelChoice(InitGameData.instance.GetLevelListId(), InitGameData.instance.initLevel);
				}
				yield break;
			}
		}

#if USE_KDK
		// Wait keyboard validated
		while (GameflowKDK.myself != null && GameflowKDK.myself.gameState < GameflowKDK.GameState.NameValidated)
			yield return null;
#elif USE_BOD
		// Wait keyboard validated
		while (GameflowBOD.myself != null && GameflowBOD.myself.gameState < GameflowBOD.GameState.NameValidated)
			yield return null;
#else
		if (sandClock != null)
		{
			sandClock.StartTime(gamesettings_general.myself.timerMaxDurationInCabin);
			sandClock.onSandClockOverCbk += OnSandClockOver;
		}

		// Wait keyboard validated
		while (gameflowmultiplayer.myself != null && gameflowmultiplayer.myself.gameState < gameflowmultiplayer.GameState.NameValidated)
			yield return null;
#endif
		if (gameObjectsToActivateAtKeyboardValidation != null)
		{
			for (int i = 0; i < gameObjectsToActivateAtKeyboardValidation.Length; ++i)
			{
				gameObjectsToActivateAtKeyboardValidation[i].SetActive(true);
			}
		}

		if (gameObjectsToDeactivateAtKeyboardValidation != null)
		{
			for (int i = 0; i < gameObjectsToDeactivateAtKeyboardValidation.Length; ++i)
			{
				gameObjectsToDeactivateAtKeyboardValidation[i].SetActive(false);
			}
		}

		if (textToActivateAtKeyboardValidation != null)
		{
			string textId = PhotonNetworkController.IsMaster() ? "str_lvlselection_helper" : "str_lvlselection_helperClients";

			for (int i = 0; i < textToActivateAtKeyboardValidation.Length; ++i)
			{
				RRLocalizedTextMP localizedText = textToActivateAtKeyboardValidation[i].GetComponent<RRLocalizedTextMP>();
				if (localizedText == null)
					localizedText = textToActivateAtKeyboardValidation[i].gameObject.AddComponent<RRLocalizedTextMP>();
				localizedText.SetTextId(textId);
			}
		}

		if (voiceOverAtKeyboardValidationMaster != null && PhotonNetworkController.IsMaster())
		{
			VoiceOver vo = voiceOverAtKeyboardValidationMaster;
			if (string.IsNullOrEmpty(voiceOverAtKeyboardValidationMaster.gameObject.scene.name))
				vo = GameObject.Instantiate<VoiceOver>(voiceOverAtKeyboardValidationMaster);
			if (vo != null)
				vo.gameObject.SetActive(true);
		}
		if (voiceOverAtKeyboardValidationClient != null && !PhotonNetworkController.IsMaster())
		{
			VoiceOver vo = voiceOverAtKeyboardValidationClient;
			if (string.IsNullOrEmpty(voiceOverAtKeyboardValidationClient.gameObject.scene.name))
				GameObject.Instantiate<VoiceOver>(voiceOverAtKeyboardValidationClient);
			if (vo != null)
				vo.gameObject.SetActive(true);
		}

	}

	public void LaunchRaceScene(bool waitToActivateScene = false)
	{
		if (raceLauncher != null)
		{
			raceLauncher.waitToActivateScene = waitToActivateScene;
			raceLauncher.LaunchScene();
		}
	}

	public void ActivateRaceScene()
	{
		if (raceLauncher != null)
			raceLauncher.ActivateScene();
	}

	public bool IsLoadingRaceScene()
	{
		return raceLauncher != null && raceLauncher.isLoadingScene;
	}

	public bool NeedToActivateRaceScene()
	{
		return IsLoadingRaceScene() && raceLauncher.waitToActivateScene;
	}

	public void TriggerDeactivateAtTeamValidation()
	{
		if (gameObjectsToDeactivateAtTeamValidation != null)
		{
			for (int i = 0; i < gameObjectsToDeactivateAtTeamValidation.Length; ++i)
			{
				gameObjectsToDeactivateAtTeamValidation[i].SetActive(false);
			}
		}
	}

	private void OnSandClockOver()
	{
		UI_ChooseYourName chooseName = gameObject.GetComponentInChildren<UI_ChooseYourName>(true);
		if (chooseName != null)
			chooseName.OnNameConfirmed();
	}

	private void OnValidLanguageClicked()
	{
		if (_languagesPanel != null)
			_languagesPanel.SetActive(false);
		if (_playerSettingsPanel != null)
			_playerSettingsPanel.SetActive(true);
		if (_uiGameConfig != null)
			_uiGameConfig.gameObject.SetActive(false);
	}

	private void OnValidPlayerSettingsClicked()
	{
		if (_playerSettingsPanel != null)
			_playerSettingsPanel.SetActive(false);
		if (_uiGameConfig != null)
		{
			_uiGameConfig.gameObject.SetActive(true);
			_uiGameConfig.SetStartCallback(InvokeLevelButton);
		}
		else
		{
			InvokeLevelButton(); 
		}
	}

	private void OnCancelPlayerSettingsClicked()
	{
		if (_playerSettingsPanel != null)
			_playerSettingsPanel.SetActive(false);
		if (_languagesPanel != null)
			_languagesPanel.SetActive(true);
	}

	public void InvokeLevelButton()
	{
		int level = multiplayerlobby.startLevel;
		if (_directLevelBtnArray != null && _directLevelBtnArray.Length > level)
			_directLevelBtnArray[level].onClick.Invoke();
#if (USE_KDK || USE_BOD) && USE_SYNTHESIS
		PhotonNetwork.CurrentRoom.IsOpen = false;
#endif
	}

	private void OnSelectFlag(string country, selectflag.LanguageType languageType, bool selected)
	{
		Debug.Log($"OnSelectFlag {country} {languageType} {selected}");
		if (selected)
			OnValidLanguageClicked();
	}
}
