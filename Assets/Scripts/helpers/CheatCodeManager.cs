using IngameDebugConsole;
using UnityEngine;
using UnityEngine.SceneManagement;
using static CurvedUIInputModule;

public class CheatCodeManager : MonoBehaviour
{
	#region Enums

	public enum CheatCodeType
	{
		NO_VR,
		SPECTATOR,
		GOD_BLUE,
		GOD_RED,
		MAGNETIC_BLUE,
		MAGNETIC_RED,
		GHOST,
		QUIT_RACE,
		REPLAY_RACE,
		NEXT_RACE,
		END_RACE,
		PAUSE,
		CONSOLE,
		TUTO_NEXT,
		TUTO_RESET,
		SAVE,
		LOAD,
		ADD_COINS,
		NO_LIFE,
		TEAM_GOLD,
		GUNS,
		INVINCIBLE,
		AUTOSHOOT,
		KILL_ALL,
		OUTRO,
		TIME_OVER,
		ROTATE,
		TOUCH,
		STICK,
		FIX_POS,
		MAGNET,
		Count
	}

	public enum SpecialCheatCodeType
	{
		SPEED_X,
		MAP_X,
		SKULL_XX,
		SKULLN_XX,
		COINS_XXXXX,
		WAVE_X,
		Count
	}

	#endregion

	#region Properties

	public static CheatCodeManager myself = null;

	public DebugLogManager debugLogManagerPrefab = null;

	private CheatCodes _cheatCodes = null;
	private DebugLogManager _debugLogManager = null;

	#endregion

	private void Awake()
	{
		myself = this;
	}

	// Use this for initialization
	void Start()
	{
		// CheatCodes Init
		_cheatCodes = gameObject.AddComponent<CheatCodes>();
		_cheatCodes.m_eTriggerMode = CheatCodes.TriggerMode.Corners;
		_cheatCodes.AddCheatCodeCbk(OnCheatCode);
		_cheatCodes.AddSpecialCheatCodeCbk(OnSpecialCheatCode);
		for (int i = 0; i < (int)CheatCodeType.Count; i++)
		{
			string sInput = ((CheatCodeType)i).ToString().ToLower().Replace("_", string.Empty);
			_cheatCodes.m_sCodeList.Add(sInput);
		}
		for (int i = 0; i < (int)SpecialCheatCodeType.Count; i++)
		{
			string sKey;
			int nLength;
			if (ConvertSpecialCheatCodeToKey((SpecialCheatCodeType)i, out sKey, out nLength))
			{
				_cheatCodes.m_dictSpecialCodes.Add(sKey, nLength);
			}
		}
	}

	protected void OnDestroy()
	{
		_cheatCodes.RemoveCheatCodeCbk(OnCheatCode);
		_cheatCodes.RemoveSpecialCheatCodeCbk(OnSpecialCheatCode);
	}

	// ----------------------------------------------------------------------------------
	// Created Raph 02/10/2015
	// ----------------------------------------------------------------------------------
	private void OnCheatCode(int nCode)
	{
		string sAdditionalLog = string.Empty;

		CheatCodeType eCode = (CheatCodeType)nCode;
		switch (eCode)
		{
			case CheatCodeType.NO_VR:
				if (!GameflowBase.isInNoVR)
				{
					GameObject goNoVR = new GameObject("StartWithoutVR");
					goNoVR.AddComponent<StartWithoutVR>();
					DontDestroyOnLoad(goNoVR);
					GameflowBase.isInNoVR = true;
				}
				else
				{
					StartWithoutVR novr = FindObjectOfType<StartWithoutVR>();
					if (novr != null)
						Destroy(novr.gameObject);
					GameflowBase.isInNoVR = false;
				}
				sAdditionalLog = $"isInNoVR: {GameflowBase.isInNoVR}";
				break;
			case CheatCodeType.SPECTATOR:
				if (GameflowBase.isInSpectatorView)
				{
					GameflowBase.isInSpectatorView = false;
				}
				else
				{
					SpectatorManager spectatorMgr = FindObjectOfType<SpectatorManager>();
					if (spectatorMgr != null)
					{
						spectatorMgr.Init();
					}
					else
					{
						gameObject.AddComponent<SpectatorManager>().Init();
					}
				}
				sAdditionalLog = $"isInSpectatorView: {GameflowBase.isInSpectatorView}";
				break;
			case CheatCodeType.GOD_BLUE:
                gamesettings_general.myself.CheatBoatBlue = true;
				break;
			case CheatCodeType.GOD_RED:
                gamesettings_general.myself.CheatBoatRed = true;
                break;
			case CheatCodeType.MAGNETIC_BLUE:
				gameflowmultiplayer.magneticBoatBlue = !gameflowmultiplayer.magneticBoatBlue;
				break;
			case CheatCodeType.MAGNETIC_RED:
				gameflowmultiplayer.magneticBoatRed = !gameflowmultiplayer.magneticBoatRed;
				break;
			case CheatCodeType.GHOST:
				gameflowmultiplayer.forceGhostBoat = !gameflowmultiplayer.forceGhostBoat;
				sAdditionalLog = $"forceGhostBoat: {gameflowmultiplayer.forceGhostBoat}";
				break;
			case CheatCodeType.QUIT_RACE:
				gameflowmultiplayer.myself.QuitRace();
				break;
			case CheatCodeType.REPLAY_RACE:
				gameflowmultiplayer.myself.ReplayRace();
				break;
			case CheatCodeType.NEXT_RACE:
				gameflowmultiplayer.myself.NextRace();
				break;
			case CheatCodeType.END_RACE:
				gameflowmultiplayer.gameplayEndRace = true;
				if (TowerDefManager.myself != null)
					TowerDefManager.myself.SetStateEvent(TowerDefManager.TowerDefState.END, 9, 9);
				break;
			case CheatCodeType.PAUSE:
				GameflowBase.instance.pausedplayer = !GameflowBase.instance.pausedplayer;
				break;
			case CheatCodeType.CONSOLE:
				if (_debugLogManager == null)
				{
					_debugLogManager = Instantiate(debugLogManagerPrefab);
					CurvedUIInputModule.ControlMethod = CUIControlMethod.MOUSE;
				}
				else
				{
					GameObject.Destroy(_debugLogManager.gameObject);
					_debugLogManager = null;
					CurvedUIInputModule.ControlMethod = CUIControlMethod.STEAMVR_2;
				}
				break;
			case CheatCodeType.TUTO_NEXT:
				if (UI_Tutorial.myself != null)
					UI_Tutorial.myself.NextStep();
				break;
			case CheatCodeType.TUTO_RESET:
				if (UI_Tutorial.myself != null)
					UI_Tutorial.myself.ResetStep();
				break;
			case CheatCodeType.SAVE:
				SaveManager.myself.Save();
				break;
			case CheatCodeType.LOAD:
				SaveManager.myself.Load();
				break;
			case CheatCodeType.ADD_COINS:
				SaveManager.myself.profile.coins += 100000;
				SaveManager.myself.Save();
				break;
			case CheatCodeType.NO_LIFE:
				if (TowerDefManager.myself != null)
					TowerDefManager.myself.SetNoLife();
				break;
			case CheatCodeType.TEAM_GOLD:
				Player.myplayer.SetCollectedTeamGold(1000000);
				break;
			case CheatCodeType.GUNS:
				Player.myplayer.UpgradeGun(true, 0, 0);
				Player.myplayer.UpgradeGun(false, 0, 0);
				Player.myplayer.UpgradeGun(true, 1, 0);
				Player.myplayer.UpgradeGun(false, 1, 0);
				break;
			case CheatCodeType.INVINCIBLE:
				TowerDefManager.isInvincible = !TowerDefManager.isInvincible;
				sAdditionalLog = ": " + TowerDefManager.isInvincible;
				break;
			case CheatCodeType.AUTOSHOOT:
				AIPlayer.autoShoot = !AIPlayer.autoShoot;
				sAdditionalLog = ": " + AIPlayer.autoShoot;
				break;
			case CheatCodeType.KILL_ALL:
				if (TowerDefManager.myself != null)
					TowerDefManager.myself.KillAllEnemies();
				break;
			case CheatCodeType.OUTRO:
				if (TowerDefManager.myself != null)
					TowerDefManager.myself.SetStateEvent(TowerDefManager.TowerDefState.OUTRO, 9, 9);
				break;
			case CheatCodeType.TIME_OVER:
				if (TowerDefManager.myself != null)
					TowerDefManager.myself.SetGameDuration(1f);
				break;
			case CheatCodeType.ROTATE:
				gamesettings_player.myself.rotationWithStickEnable = !gamesettings_player.myself.rotationWithStickEnable;
				sAdditionalLog = ": " + gamesettings_player.myself.rotationWithStickEnable;
				break;
			case CheatCodeType.STICK:
				gamesettings_player.myself.teleportWithStickEnable = !gamesettings_player.myself.teleportWithStickEnable;
				sAdditionalLog = ": " + gamesettings_player.myself.teleportWithStickEnable;
				break;
			case CheatCodeType.TOUCH:
				gamesettings_player.myself.teleportWithTouchEnable = !gamesettings_player.myself.teleportWithTouchEnable;
				sAdditionalLog = ": " + gamesettings_player.myself.teleportWithTouchEnable;
				break;
			case CheatCodeType.FIX_POS:
				Player.fixedPosition = !Player.fixedPosition;
				sAdditionalLog = ": " + Player.fixedPosition;
				break;
			case CheatCodeType.MAGNET:
				Player.magnet = !Player.magnet;
				sAdditionalLog = ": " + Player.magnet;
				break;
		}
		Debug.Log("CC " + eCode + " " + sAdditionalLog);
	}

	private bool OnSpecialCheatCode(string sLeft, string sRight)
	{
		SpecialCheatCodeType eCheatCode = ConvertSpecialCheatCode(sLeft, sRight);
		bool bProcessed = OnSpecialCheatCode(eCheatCode, sRight);
		if (bProcessed)
			Debug.Log("CC " + sLeft.ToUpper() + " " + sRight);
		return bProcessed;
	}

	private bool OnSpecialCheatCode(SpecialCheatCodeType eCheatCode, string sRight)
	{
		switch (eCheatCode)
		{
			case SpecialCheatCodeType.SPEED_X:
				int speed;
				if (int.TryParse(sRight, out speed))
				{
                    if (speed == 0)
                        gamesettings_general.myself.GameTimeScale = 50f;
					else
                        gamesettings_general.myself.GameTimeScale = speed;
					return true;
				}
				break;
			case SpecialCheatCodeType.MAP_X:
				int map;
				if (int.TryParse(sRight, out map))
				{
					switch (map)
					{
						default:
						case 0:
							SceneManager.LoadSceneAsync("Cabin");
							return true;
						case 1:
							SceneManager.LoadSceneAsync("Starterhub_Poseidon");
							return true;
						case 2:
							SceneManager.LoadSceneAsync("Starterhub_Kraken");
							return true;
						case 3:
							SceneManager.LoadSceneAsync("Starterhub_World03");
							return true;
					}
				}
				break;
			case SpecialCheatCodeType.SKULL_XX:
				if (int.TryParse(sRight, out int skull))
				{
					int level = skull / 5;
					int idx = skull % 5;
					SaveManager.myself.profile.progression.GetLevelFromId($"Standalone{level+1:00}").unlockedSkulls[idx] = true;
					SaveManager.myself.Save();
					UI_CabinLevelSelection[] items = FindObjectsOfType<UI_CabinLevelSelection>();
					foreach (UI_CabinLevelSelection item in items)
						item.Init();
					return true;
				}
				break;
			case SpecialCheatCodeType.SKULLN_XX:
				if (int.TryParse(sRight, out int skulls))
				{
					for (int i = 0; i < skulls; ++i)
					{
						int level = i / 5;
						int idx = i % 5;
						SaveManager.myself.profile.progression.GetLevelFromId($"Standalone{level + 1:00}").unlockedSkulls[idx] = true;
					}
					SaveManager.myself.Save();
					UI_CabinLevelSelection[] items = FindObjectsOfType<UI_CabinLevelSelection>();
					foreach (UI_CabinLevelSelection item in items)
						item.Init();
					return true;
				}
				break;
			case SpecialCheatCodeType.COINS_XXXXX:
				if (int.TryParse(sRight, out int coins))
				{
					SaveManager.myself.profile.coins = coins;
					SaveManager.myself.Save();
					return true;
				}
				break;
			case SpecialCheatCodeType.WAVE_X:
				if (int.TryParse(sRight, out int wave))
				{
					if (TowerDefManager.myself != null)
						TowerDefManager.myself.ForceWave(wave-1);
					return true;
				}
				break;
		}
		return false;
	}

	public static SpecialCheatCodeType ConvertSpecialCheatCode(string sLeft, string sRight)
	{
		int nRightLength = sRight.Length;

		SpecialCheatCodeType[] cheatCodes = (SpecialCheatCodeType[])System.Enum.GetValues(typeof(SpecialCheatCodeType));
		int cheatCodeIndex = 0;
		SpecialCheatCodeType result = SpecialCheatCodeType.Count;
		while (cheatCodeIndex < cheatCodes.Length && result == SpecialCheatCodeType.Count)
		{
			string sKey;
			int nLength;
			if (ConvertSpecialCheatCodeToKey(cheatCodes[cheatCodeIndex], out sKey, out nLength))
			{
				if (string.CompareOrdinal(sLeft, sKey) == 0 && nRightLength == nLength - sKey.Length)
				{
					result = cheatCodes[cheatCodeIndex];
				}
			}

			++cheatCodeIndex;
		}

		return result;
	}

	public static bool ConvertSpecialCheatCodeToKey(SpecialCheatCodeType cheatCode, out string sKey, out int nLength)
	{
		string sCheat = cheatCode.ToString();
		string[] sSplit = sCheat.Split('_');
		if (sSplit.Length >= 2)
		{
			sKey = string.Join("", sSplit, 0, sSplit.Length - 1).ToLower();
			nLength = sKey.Length + sSplit[sSplit.Length - 1].Length;
			return true;
		}
		else
		{
			sKey = null;
			nLength = 0;
			return false;
		}
	}

}