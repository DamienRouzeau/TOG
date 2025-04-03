//#define DEBUG_PHOTON_DATA
//#define DEBUG_CORD
//#define DEBUG_FREEZE
#define DEBUG_STATES
//#define USE_SLOTS

using MiniJSON;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class gameflowmultiplayer : GameflowBase
{
    #region Delegates

    public delegate void OnPlayerEventDelegate(PlayerEvent pEvent, string data);

    #endregion

    #region Enums

    public enum GameMode
    {
        Classic,
        Endless
    }

    public enum GameState
    {
        Cabin,
        NameValidated,
        ChooseLevel,
        LevelValidated,
        GoToLobby,
        LobbyLoaded,
        TeamSelected,
        ForceTeamSelection,
        TeamValidated,
        ForceTeamValidation,
        ShowRaceWorld,
        PlayersToBoat,
        RaceLoaded,
        OpenDoors,
        RaceStarted,
        FinishLine, // TODO
        EndGame,
        QuitRace,
        WaitLoadScene,
        StopWithFade,
        WaitQuitFromLauncher
    }

    public enum PlayerEvent
    {
        MasterMustChooseLevel
    }

    #endregion

    #region struct

    public struct PlayerEventData
	{
        public PlayerEvent playerEvent;
        public string data;
	}

    #endregion

    public static GameMode gameMode = GameMode.Classic;

    // Data to be sent
    public GameState gameState
    {
        get => _gameState;
#if DEBUG_STATES
        set 
        {
            _gameState = value;
            if (ownedobject)
                Debug.Log($"[STATES] gameState {_gameState}");
        }
#else
        set { _gameState = value; }
#endif
    }
    private GameState _gameState = GameState.Cabin;
    public bool teamSelected => gameState >= GameState.TeamSelected;
    public bool teamValidated => gameState >= GameState.TeamValidated;
    public bool gameStarted => gameState >= GameState.RaceStarted;
    public bool isInCabin => gameState < GameState.GoToLobby;
    public override bool canPlayerShoot => gameState >= GameState.LobbyLoaded;

    private bool _gamestartload = false;
    private bool _gameEndGame = false;

    // Data to be sent

    public static float gameRemainingTime = -1.0f;
    public static float gamePreviousRemainingTime = -1.0f;
    public static float startcountdown = -1f;
    public static string levelListId = null;
    public static int levelIndex = 0;
    public static string levelToLaunch = null;
    public static string currentLevelName = null;
    public static bool areAllTeamsSelected = false;
    public static bool areAllRacesLoaded = false;
    public static bool gameplayEndRace = false;

    public static bool forceGhostBoat = false;
    public static bool magneticBoatBlue = false;
    public static bool magneticBoatRed = false;

    public static bool[] unlockedSkullsInRace = new bool[5];

    public static string myGameState
	{
        get
		{
            if (myself != null)
                return myself.gameState.ToString();
            else
                return "Disconnected";
		}
	}

    public float showstartcountdown = 0.0f;
    
    public string showedData;

    public static OnPlayerEventDelegate onPlayerEventDelegate = null;

    public static gameflowmultiplayer myself => instance as gameflowmultiplayer;

    public static int [] cordPlayerId = new int[nrteam];
    public static bool [] cordpulling = new bool [nrteam];
    public static float [] corddistance = new float [nrteam];
    static boat_followdummy[] boatroot = new boat_followdummy[nrteam];
    public static float[] boatDistances = new float[nrteam];

    public static int bucketlifted = 0;
    static int[] _allbuckets = new int[nrplayersmax];
    public GameObject gamedata;

    public static int[] finishTeams = new int[] { -1, -1 };
    public static bool finishLineCross = false;
    public static bool amWinnerTeam = false;
    public static GameObject myCaptain = null;

    private static List<boat_canon> _boatCanons = null;
    private static Vector2[] _boatCanonsOrientation = null;
    public boat_followdummy myboat = null;

    private static List<boat_pump> _boatPumps = null;
    private static float[] _boatPumpValues = null;

    protected static List<PlayerEventData> _playerEvents = new List<PlayerEventData>();

    public int pathchanger = -1;
    int _pathchanger_boat1 = -1;
    int _pathchanger_boat2 = -1;
    public static boat_followdummy[] allBoats = null;
    private static float[] _pathSplineValues = new float[nrteam];

    private bool _attachPlayerToCabin = false;
    private bool _isQuittingRace = false;
    private float _pathSplineDistance = 0f;

    private SandClock _sandClock = null;

    /// <summary>
    /// Multiplayer terms
    /// </summary>
    const string mpt_sink0 = "s0";
    const string mpt_sink1 = "s1";
    const string mpt_health0 = "h0";
    const string mpt_health1 = "h1";
    const string mpt_healths = "hs";
    const string mpt_wonGold0 = "wg0";
    const string mpt_wonGold1 = "wg1";
    const string mpt_stolenGold0 = "sg0";
    const string mpt_stolenGold1 = "sg1";
    const string mpt_pathSpline0 = "p0";
    const string mpt_pathSpline1 = "p1";
    const string mpt_pathSplineDistance = "pd";
    const string mpt_myteam = "mt";
    const string mpt_bucket = "bu";
    const string mpt_corddistance = "cd";
    const string mpt_corddistance0 = "c0";
    const string mpt_corddistance1 = "c1";
    const string mpt_gameplayrunning = "gr";
    const string mpt_gameplayendrace = "ge";
    const string mpt_netid = "ni";
    const string mpt_playername = "pn";
    const string mpt_gamestate = "gs";
    const string mpt_playerstate = "ps";
#if USE_SLOTS
    const string mpt_playerSlots = "sl";
#endif
    const string mpt_playerEvent = "pe";
    const string mpt_level = "lv";
    const string mpt_startcountdown = "sc";
    const string mpt_teamlistA = "tA";
    const string mpt_teamlistB = "tB";
    const string mpt_handleftitem = "hl";
    const string mpt_handrightitem = "hr";
    const string mpt_beltleftitem = "bl";
    const string mpt_beltrightitem = "br";
    const string mpt_beltfrontitem = "bf";
    const string mpt_beltbackitem = "bb";
    const string mpt_remainingTime = "rt";
    const string mpt_stats = "st";
    const string mpt_finishTeam = "ft";
    const string mpt_canons = "cn";
    const string mpt_pumps = "pu";
    //const string mpt_death = "dt";
    //const string mpt_health = "he";
    const string mpt_skin = "sk";
    const string mpt_customHat = "ch";
    const string mpt_changepath = "cp";
    const string mpt_randomSeed = "rs";

    protected override void Start()
    {
        base.Start();
        if (ownedobject)
        {
#if USE_STANDALONE
            string name = myPirateName;
            if (PhotonNetworkController.soloMode)
                myId = 0;
            else
                myId = PhotonNetwork.LocalPlayer.ActorNumber - 1;
            myPirateName = name;
#else
            myId = PhotonNetwork.LocalPlayer.ActorNumber - 1;
            Player.myplayer.UpdateThemeMaterial();
#endif
            if (myPirateName == null)
                myPirateName = gamesettings_player.myself.GetNameFromIndex(myId);

            Reset(true);

#if USE_STANDALONE
            SaveManager.ProfileData data = SaveManager.myself.GetProfile(SaveManager.myself.profileIdx);
            SetMySkin(data.avatar);
            SetMyCustomHat(data.custom.hat);
#endif

            RaceManager.myself?.gamemusic.Stop();

            ChoiceButtons.onSetChoiceCallback += OnChoiceButtons;
#if USE_STANDALONE
            PhotonNetworkController.onPhotonEventCallback += OnPhotonEvents;
#endif
        }
    }

    protected override void OnDestroy()
    {
        if (ownedobject)
        {
            Debug.Log("[STATES] Gameflowmultiplayer destroyed!");
            ChoiceButtons.onSetChoiceCallback -= OnChoiceButtons;
#if USE_STANDALONE
            PhotonNetworkController.onPhotonEventCallback -= OnPhotonEvents;
#endif
        }
        base.OnDestroy();
    }

    public static boat_followdummy GetBoat(int team)
	{
        if (boatroot == null || boatroot.Length <= team)
            return null;
        return boatroot[team];
	}

    public override void Reset(bool resetToCabin)
	{
        base.Reset(resetToCabin);
#if DEBUG_STATES
        Debug.Log($"[STATE] - Reset resetToCabin {resetToCabin} ownedobject {ownedobject}");
#endif

        if (ownedobject)
        {
            for (int i = 0; i < nrplayersmax; i++)
            {
                _allbuckets[i] = 0;
                piratedeaths[i] = false;
                piratehealths[i] = -1.0f;
                if (resetToCabin)
                {
                    pirateskins[i] = gamesettings_player.myself.GetSkinName(0);
                    piratesCustomHats[i] = null;
                }
            }

            allteams.Clear();
            
            boatroot[0] = null;
            boatroot[1] = null;
            finishTeams[0] = -1;
            finishTeams[1] = -1;
            boatDistances[0] = 0f;
            boatDistances[1] = 0f;
            _boatCanons = null;
            _boatPumps = null;
            finishLineCross = false;
            amWinnerTeam = false;
            _isInTutorial = false;
            CheckRandomSeed();
            if (resetToCabin)
            {
                allFlows = null;
                allBoats = null;
                currentLevelName = null;
            }

            SetPullingCord(false, 0);
            SetPullingCord(false, 1);
            SetCordDistanceRatio(-1f, 0);
            SetCordDistanceRatio(-1f, 1);
            SetCordPlayerId(-1, 0);
            SetCordPlayerId(-1, 1);

            for (int i = 0; i < nrplayersperteam; i++)
            {
                SetInTeamA(i, -1);
                SetInTeamB(i, -1);
            }

            _gamestartload = false;
            _gameEndGame = false;

            if (resetToCabin)
                gameState = GameState.Cabin;

            if (_sandClock != null)
            {
                _sandClock.onSandClockOverCbk -= OnSandClockOver;
                _sandClock = null;
            }

            gameRemainingTime = -1.0f;
            gamePreviousRemainingTime = -1.0f;
            startcountdown = -1.0f;
            areAllTeamsSelected = false;
            areAllRacesLoaded = false;
            areAllRacesStarted = false;
            gameplayrunning = false;
            gameplayEndRace = false;
            myTeam = 0;
            _isQuittingRace = false;

            for (int i = 0; i < unlockedSkullsInRace.Length; ++i)
                unlockedSkullsInRace[i] = false;

            // Reset voice over max priority
            voicepriority.playinprio = 0;
            //Debug.Log($"[DEBUG_VOICEOVER] Reset on loadscene current prio {voicepriority.playinprio}");
        }
    }

    protected override void OnTeamSelected(int team)
    {
        gameState = GameState.TeamSelected;
    }

    public override bool AreAvatarsVisibles()
    {
        return gameState >= GameState.PlayersToBoat;
    }

    public override void OnSceneGoToLobby()
    {
        base.OnSceneGoToLobby();
        apicalls.myself?.StartGameCounter();
    }

    private void OnChoiceButtons(ChoiceButtons.ChoiceType type, object data)
	{
#if DEBUG_STATES
        Debug.Log($"[STATES] OnChoiceButtons {type} data {data}");
#endif
        switch (type)
        {
            case ChoiceButtons.ChoiceType.Cabin:
                if (gameState < GameState.LevelValidated)
                {
                    Dictionary<string, object> dic = data as Dictionary<string, object>;
                    levelListId = dic["list"] as string;
                    levelIndex = (int)dic["index"];
                    levelToLaunch = gamesettings_general.myself.levelSettings.GetSceneName(levelListId, levelIndex);
                    gameState = GameState.LevelValidated;
                }
                break;
            case ChoiceButtons.ChoiceType.EndRace:
                if (gameState == GameState.EndGame)
                {
                    switch ((gamesettings.EndButtons)data)
                    {
                        case gamesettings.EndButtons.END_REPLAY_RACE:
                            levelToLaunch = gamesettings_general.myself.levelSettings.GetSceneName(levelListId, levelIndex);
                            break;
                        case gamesettings.EndButtons.END_NEXT_RACE:
                            levelIndex++;
                            levelToLaunch = gamesettings_general.myself.levelSettings.GetSceneName(levelListId, levelIndex);
                            break;
                        case gamesettings.EndButtons.END_QUIT_RACE:
                            if (InitGameData.instance != null)
                            {
                                gameState = GameState.StopWithFade;
                                return;
                            }
                            else
							{
                                levelToLaunch = gamesettings_general.myself.levelStart;
                            }
                            break;
                    }
                    gameState = GameState.QuitRace;
                }
                break;
            case ChoiceButtons.ChoiceType.TeamSelection:
                if (gameState == GameState.LobbyLoaded || gameState == GameState.TeamSelected)
				{
                    if (!gameplayrunning && startcountdown == -1f && !areAllTeamsSelected)
                    {
                        switch ((boat_followdummy.TeamColor)data)
                        {
                            case boat_followdummy.TeamColor.Blue:
                                SetMyTeam(0, true);
                                break;
                            case boat_followdummy.TeamColor.Red:
                                SetMyTeam(1, true);
                                break;
                        }
                    }
                }
                break;
            case ChoiceButtons.ChoiceType.TeamValidation:
                if ((gameState == GameState.TeamSelected || gameState == GameState.ForceTeamSelection) && areAllTeamsSelected)
                {
                    if (!gameplayrunning)
                    {
                        switch ((int)data)
                        {
                            case 0:
                                if (GetActorCountInTeam(1 - myTeam) < nrplayersperteam)
                                    SetMyTeam(1 - myTeam, true);
                                SetMyValidated(true, true);
                                break;
                            case 1:
                                SetMyValidated(true, true);
                                break;
                        }
                    }
                }
                break;
        }
    }

#if USE_STANDALONE
    private void OnPhotonEvents(PhotonNetworkController.PhotonEvent photonEvent, string msg)
	{
        switch (photonEvent)
		{
            case PhotonNetworkController.PhotonEvent.PlayerLeaveRoom:
                QuitRace();
                break;
		}
	}
#endif

    string MakeAttachJsonLine(string jsonterm, List<attachobject> objects)
    {
        foreach(attachobject obj in objects)
        {
            if (obj.gameObject.activeInHierarchy)
            {
//                Debug.Log("Keeping "+jsonterm+" "+ obj.gameObject.name);
                return ("\"" + jsonterm + "\":\"" + obj.gameObject.name + "\",");
            }
        }
        return ("");
    }

    void VerifyJsonOnAttached(IDictionary js, string jsonterm, List<attachobject> objects)
    {
        string findname = "";
        if (js[jsonterm] != null) findname = (string)js[jsonterm];

        foreach (attachobject obj in objects)
        {
            if (obj.gameObject.name == findname)
                obj.gameObject.SetActive(true);
            else
                obj.gameObject.SetActive(false);
        }
    }

    public override string OnSendingData()
    {
        if (multiplayerlobby.AmILauncher()) return("{}");
        if (ownedobject)
        {                                   // send info from the player
            string jsontext = "{";
            jsontext += "\"" + mpt_gamestate + "\":" + (int)gameState + ",";
            jsontext += "\"" + mpt_playerstate + "\":" + (int)_playerState + ",";

            int netId = 1;
            if (_aiBot != null)
            {
                netId = _actorNum = + 1;
            }
            else
			{
                netId = PhotonNetwork.LocalPlayer.ActorNumber;
                _actorNum = netId - 1;
            }

            jsontext += "\"" + mpt_netid + "\":" + netId + ",";
            jsontext += "\"" + mpt_myteam + "\":" + myTeam + ",";

#if USE_SLOTS
            string jsonSlots = "";
            for (int i = 0; i < nrplayersmax; ++i)
            {
                if (i == 0)
                {
                    jsonSlots = playerSlots[0].ToString();
                }
                else
                {
                    jsonSlots += "," + playerSlots[i];
                }
            }

            jsontext += "\"" + mpt_playerSlots + "\":[" + jsonSlots + "],";
#endif


            if (!gameplayrunning)
                jsontext += "\"" + mpt_playername + "\":\"" + myPirateName + "\",";
            if (gameplayrunning)
                jsontext += "\"" + mpt_bucket + "\":" + bucketlifted + ",";

            if (PhotonNetwork.IsMasterClient)
            {
                if (pathchanger != -1)
                {
                    if (myTeam == 0)
                        _pathchanger_boat1 = pathchanger;
                    else
                        _pathchanger_boat2 = pathchanger;
                    TriggerRaceEvent(myTeam, true, gamesettings.STAT_SPLINES, pathchanger);
                    pathchanger = -1;
                }

                if (gameState == GameState.LevelValidated || gameState == GameState.QuitRace)
                {
                    jsontext += "\"" + mpt_level + "\":\"" + levelToLaunch + "\",";
                }

                if (areAllRacesStarted && boatroot[0] != null)
                {
                    if (_aiBot != null)
                    {
                        // TODO
                    }
                    else
                    {
                        jsontext += "\"" + mpt_sink0 + "\":" + boatroot[0].sinkposition.ToString("F3") + ",";
                        jsontext += "\"" + mpt_sink1 + "\":" + boatroot[1].sinkposition.ToString("F3") + ",";
                        jsontext += "\"" + mpt_health0 + "\":" + boatroot[0].gameObject.GetComponent<Health>().GetCurrentHealth() + ",";
                        jsontext += "\"" + mpt_health1 + "\":" + boatroot[1].gameObject.GetComponent<Health>().GetCurrentHealth() + ",";
                        jsontext += "\"" + mpt_wonGold0 + "\":" + boatroot[0].wonGold + ",";
                        jsontext += "\"" + mpt_wonGold1 + "\":" + boatroot[1].wonGold + ",";
                        jsontext += "\"" + mpt_stolenGold0 + "\":" + boatroot[0].stolenGold + ",";
                        jsontext += "\"" + mpt_stolenGold1 + "\":" + boatroot[1].stolenGold + ",";
                        jsontext += "\"" + mpt_pathSpline0 + "\":" + boatroot[0].pathFollower.pathCurrentValue.ToString("F5") + ",";
                        jsontext += "\"" + mpt_pathSpline1 + "\":" + boatroot[1].pathFollower.pathCurrentValue.ToString("F5") + ",";
                    }
                }
                if (gameplayrunning)
                {
                    jsontext += "\"" + mpt_gameplayrunning + "\":true,";

                    jsontext += "\"" + mpt_corddistance0 + "\":\"" + GetCordPlayerId(0) + "|" + GetCordDistanceRatio(0).ToString("F3") + "\",";
                    jsontext += "\"" + mpt_corddistance1 + "\":\"" + GetCordPlayerId(1) + "|" + GetCordDistanceRatio(1).ToString("F3") + "\",";
                    if (!areAllRacesStarted)
                    {
                        jsontext += "\"" + mpt_pathSplineDistance + "\":" + PathHall.averagePathLength + ",";
                    }
                }
                else
                {
                    jsontext += "\"" + mpt_gameplayrunning + "\":false,";

                    string jsonTeamListA = null;
                    string jsonTeamListB = null;
                    for (int i = 0; i < nrplayersperteam; ++i)
					{
                        if (i == 0)
						{
                            jsonTeamListA = teamlistA[0].ToString();
                            jsonTeamListB = teamlistB[0].ToString();
                        }
                        else
						{
                            jsonTeamListA += "," + teamlistA[i];
                            jsonTeamListB += "," + teamlistB[i];
                        }
					}

                    jsontext += "\"" + mpt_teamlistA + "\":[" + jsonTeamListA + "],";
                    jsontext += "\"" + mpt_teamlistB + "\":[" + jsonTeamListB + "],";
                    jsontext += "\"" + mpt_randomSeed + "\":" + randomSeed + ",";
                }

                if (gameplayEndRace)
                    jsontext += "\"" + mpt_gameplayendrace + "\":true,";
                else
                    jsontext += "\"" + mpt_gameplayendrace + "\":false,";

                if (finishTeams[0] != -1)
                {
                    jsontext += "\"" + mpt_finishTeam + "\":[" + finishTeams[0] + "," + finishTeams[1] + "],";
                }
                if (gameState >= GameState.TeamValidated && gameState < GameState.RaceLoaded)
                    jsontext += "\"" + mpt_startcountdown + "\":" + startcountdown.ToString("F3") + ",";
                jsontext += "\"" + mpt_remainingTime + "\":" + gameRemainingTime.ToString("F3") + ",";
            }
            else
            {
                if (gameplayrunning)
                {
                    if (pathchanger != -1)
                        jsontext += "\"" + mpt_changepath + "\":\"" + pathchanger + "\",";

                    if (IsCordPlayerIdMyId(myTeam))
                        jsontext += "\"" + mpt_corddistance + "\":" + GetCordDistanceRatio(myTeam) + ",";
                }
            }

            if (gameplayEndRace)
            {
                Dictionary<string,double> stats = RRStats.RRStatsManager.instance.GetStats(myStatsName);
                if (stats != null && stats.Count > 0)
                    jsontext += "\"" + mpt_stats + "\":" + MakeJsonFromDictionnary(stats) + ",";
            }
            
            if (gameplayrunning && _boatCanons != null)
			{
                for (int bc = 0; bc < _boatCanons.Count; ++bc)
                {
                    boat_canon can = _boatCanons[bc];
                    if (can != null && can.gameObject.activeInHierarchy)
                    {
                        Vector2 orientation = can.orientation;
                        bool differentOrientation = Vector2.SqrMagnitude(orientation - _boatCanonsOrientation[bc]) > 0.01f;
                        if (differentOrientation)
                        {
                            string adder = "\"" + mpt_canons + "_" + can.id + "\":\"";
                            adder += can.orientationH.ToString("F3") + ",";
                            adder += can.orientationV.ToString("F3") + "";
                            adder += "\",";
                            jsontext += adder;
                            _boatCanonsOrientation[bc] = orientation;
                        }
                    }
                }
            }

            // Pumps
            if (gameplayrunning && _boatPumps != null)
			{
                for (int i = 0; i < _boatPumps.Count; ++i)
				{
                    boat_pump pump = _boatPumps[i];
                    if (pump != null)
					{
                        float val = pump.GetValue();
                        if (val != _boatPumpValues[i])
						{
                            string pumpJson = "\"" + mpt_pumps + "_" + i + "\":\"" + val + "\",";
                            jsontext += pumpJson;
                            _boatPumpValues[i] = val;
                        }
					}
				}
			}

            // Healths
            if (gameplayrunning)
			{
                if (_healthEvents.Count > 0)
				{
                    string healthData = "";
                    foreach (Health.HealthById data in _healthEvents)
					{
                        healthData += data.healthId + "_" + data.playerId + "_" + data.value.ToString("F3") + "_";
                    }
                    jsontext += "\"" + mpt_healths + "\":\"" + healthData + "\",";
                    _healthEvents.Clear();
                }
			}

            if (_playerEvents.Count > 0)
			{
                string playerEventMsg = "";
                foreach (var data in _playerEvents)
                {
                    playerEventMsg += data.playerEvent + "_" + data.data + "_";
                }
                jsontext += "\"" + mpt_playerEvent + "\":\"" + playerEventMsg + "\",";
                _playerEvents.Clear();
            }

            if (!gameplayrunning)
            {
                jsontext += "\"" + mpt_skin + "\":\"" + pirateskins[myId] + "\",";
                jsontext += "\"" + mpt_customHat + "\":\"" + piratesCustomHats[myId] + "\",";
            }
            //if (piratedeaths[myId])
            //    jsontext += "\"" + mpt_death + "\":1,";
            //if (healthvalue != -1.0f)
            //    jsontext += "\"" + mpt_health + "\":\"" + healthvalue + "\",";
            if (Player.myplayer != null)
            {
                jsontext += MakeAttachJsonLine(mpt_handleftitem, Player.myplayer.left_hand_objects);
                jsontext += MakeAttachJsonLine(mpt_handrightitem, Player.myplayer.right_hand_objects);
                jsontext += MakeAttachJsonLine(mpt_beltleftitem, Player.myplayer.belt_left_objects);
                jsontext += MakeAttachJsonLine(mpt_beltrightitem, Player.myplayer.belt_right_objects);
                jsontext += MakeAttachJsonLine(mpt_beltfrontitem, Player.myplayer.belt_front_objects);
                jsontext += MakeAttachJsonLine(mpt_beltbackitem, Player.myplayer.belt_back_objects);
            }

            jsontext += "\"d\":false";
            jsontext += "}";

#if UNITY_EDITOR && DEBUG_PHOTON_DATA
            Debug.Log("Photon data send: " + jsontext);
#endif
            return jsontext;
        }
        return null;
    }

    public override void OnReceivingData(string data, bool forceReceive = false)
    {
        if (multiplayerlobby.AmILauncher()) return;
#if UNITY_EDITOR && DEBUG_PHOTON_DATA
        Debug.Log("Photon data receive: " + data);
#endif
        if (forceReceive || !ownedobject)         // get info from the OTHER player
        {
            if (!string.IsNullOrEmpty(data))
            {
                IDictionary received = (IDictionary)Json.Deserialize(data);
                if (received != null)
                {
                    long _netid = -1;
                    long _myteam = 0;

                    if (received[mpt_myteam] != null)
                    {
                        _myteam = (long)received[mpt_myteam];
                    }

                    if (received[mpt_gameplayrunning] != null)
                    {
                        //if (gameplayrunning == false)
                        {
                            gameplayrunning = (bool)received[mpt_gameplayrunning];
                        }
                    }
                    if (received[mpt_gameplayendrace] != null)
                    {
                        //if (gameplayEndRace == false)
                        {
                            gameplayEndRace = (bool)received[mpt_gameplayendrace];
                        }
                    }
                    if (received[mpt_netid] != null)
                    {
                        _netid = (long)received[mpt_netid];
                        _actorNum = (int)_netid - 1;
                        allteams[actorNum] = (int)_myteam;

                        Player_avatar[] allplayerstotest = Player.myplayer.avatars;
                        foreach (Player_avatar pl in allplayerstotest)
                        {
                            if (pl.actornumber == (actorNum))
                            {
                                VerifyJsonOnAttached(received, mpt_handleftitem, pl.left_hand_objects);
                                VerifyJsonOnAttached(received, mpt_handrightitem, pl.right_hand_objects);
                                VerifyJsonOnAttached(received, mpt_beltleftitem, pl.belt_left_objects);
                                VerifyJsonOnAttached(received, mpt_beltrightitem, pl.belt_right_objects);
                                VerifyJsonOnAttached(received, mpt_beltfrontitem, pl.belt_front_objects);
                                VerifyJsonOnAttached(received, mpt_beltbackitem, pl.belt_back_objects);
                            }
                        }

#if USE_SLOTS
                        if (received[mpt_playerSlots] != null)
                        {
                            List<object> ll = (List<object>)received[mpt_playerSlots];
                            int ind = 0;
                            foreach (long elm in ll)
                            {
                                playerSlots[ind] = (int)elm;
                                ind++;
                            }
                        }
#endif

                        if (received[mpt_playername] != null)
                        {
                            string name = (string)received[mpt_playername];
                            piratenames[actorNum] = name;
                            pirateStatsNames[actorNum] = $"{name}_{actorNum}";
                        }

                        if (PhotonNetwork.IsMasterClient && !forceReceive)
                        {
                            if (received[mpt_corddistance] != null)
                            {
                                float cordDistanceRatio = Helper.GetDictionaryValue(received, mpt_corddistance);
                                int team = (int)_myteam;
                                if (GetCordPlayerId(0) != myId)
                                {
                                    SetCordDistanceRatio(cordDistanceRatio, team);
                                    int playerId = actorNum;
                                    if (cordDistanceRatio < -0.5f)
                                        playerId = -1;
                                    if (GetCordPlayerId((int)_myteam) != playerId)
                                        SetCordPlayerId(playerId, (int)_myteam);
                                }
                            }
                        }
                        else
                        {
                            if (received[mpt_corddistance0] != null)
                            {
                                string cord0 = (string)received[mpt_corddistance0];
                                string[] splitCord0 = cord0.Split('|');
                                if (splitCord0.Length == 2)
								{
                                    int cordPlayerId = int.Parse(splitCord0[0]);
                                    if (cordPlayerId != myId)
                                    {
                                        float cordDistanceRatio = float.Parse(splitCord0[1]);
                                        if (GetCordPlayerId(0) != myId || cordDistanceRatio > 0f)
                                        {
                                            if (GetCordPlayerId(0) != cordPlayerId)
                                                SetCordPlayerId(cordPlayerId, 0);
                                            SetCordDistanceRatio(cordDistanceRatio, 0);
                                        }
                                    }
                                }                                
                            }
                            if (received[mpt_corddistance1] != null)
                            {
                                string cord1 = (string)received[mpt_corddistance1];
                                string[] splitCord1 = cord1.Split('|');
                                if (splitCord1.Length == 2)
                                {
                                    int cordPlayerId = int.Parse(splitCord1[0]);
                                    if (cordPlayerId != myId)
                                    {
                                        float cordDistanceRatio = float.Parse(splitCord1[1]);
                                        if (GetCordPlayerId(1) != myId || cordDistanceRatio > 0f)
                                        {
                                            if (GetCordPlayerId(1) != cordPlayerId)
                                                SetCordPlayerId(cordPlayerId, 1);
                                            SetCordDistanceRatio(cordDistanceRatio, 1);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (received[mpt_bucket] != null)
                    {
                        int newbucket = Helper.GetDictionaryInt(received, mpt_bucket);
                        int diffBuckets = newbucket - _allbuckets[actorNum];
                        if (diffBuckets > 0 && boatroot[(int)_myteam] != null)
                        {
                            boatroot[(int)_myteam].OneBucket();
                            _allbuckets[actorNum] += diffBuckets;
                        }
                    }

                    //if (received[mpt_death] != null)
                    //{
                    //    piratedeaths[actorNum] = true;
                    //}

                    if (received[mpt_skin] != null)
                    {
                        pirateskins[actorNum] = (string)received[mpt_skin];
                        Player_avatar.SetSkin(actorNum, pirateskins[actorNum]);
                    }

                    if (received[mpt_customHat] != null)
                    {
                        piratesCustomHats[actorNum] = (string)received[mpt_customHat];
                        Player_avatar.SetCustomHat(actorNum, piratesCustomHats[actorNum], gamesettings_general.myself.customPackSetting);
                    }

                    //if (received[mpt_health] != null)
                    //{
                    //    string tmpstr = (string)received[mpt_health];
                    //    float.TryParse(tmpstr, out piratehealths[actorNum]);
                    //}

                    if (received[mpt_gamestate] != null)
                    {
                        gameState = (GameState)((long)received[mpt_gamestate]);
                        if (PhotonNetwork.IsMasterClient && myself.gameState != GameState.ForceTeamValidation 
                            && InitGameData.instance == null)
                        {
                            switch (gameState)
                            {
                                case GameState.LobbyLoaded:
                                case GameState.TeamSelected:
                                case GameState.ForceTeamSelection:
                                case GameState.TeamValidated:
                                    SetTeamTables(gameState != GameState.LobbyLoaded, _myteam, actorNum);
                                    break;
                            }
                        }

                        if (InitGameData.instance != null && gameState == GameState.StopWithFade)
						{
                            myself.gameState = GameState.StopWithFade;
                        }
                    }

                    if (received[mpt_playerstate] != null)
                    {
                        PlayerState oldPlayerState = _playerState;
                        _playerState = (PlayerState)((long)received[mpt_playerstate]);
                        if (oldPlayerState != _playerState)
						{
                            foreach (Player_avatar avatar in Player.myplayer.avatars)
                            {
                                if (avatar.actornumber == actorNum)
                                {
                                    avatar.SetInPause(_playerState == PlayerState.InPause);
                                }
                            }
                        }
                    }

                    if (received[mpt_changepath] != null)
                    {
                        if (_myteam == 0)
                        {
                            int.TryParse((string)received[mpt_changepath], out _pathchanger_boat1);
                            TriggerRaceEvent(0, false, gamesettings.STAT_SPLINES, _pathchanger_boat1);
                        }
                        else
                        {
                            int.TryParse((string)received[mpt_changepath], out _pathchanger_boat2);
                            TriggerRaceEvent(1, false, gamesettings.STAT_SPLINES, _pathchanger_boat2);
                        }
                    }

                    if (received[mpt_startcountdown] != null)
                    {
                        startcountdown = Helper.GetDictionaryValue(received, mpt_startcountdown);
                    }
                    if (received[mpt_remainingTime] != null)
                    {
                        float _remainTime = Helper.GetDictionaryValue(received, mpt_remainingTime);
                        if (_remainTime != -1.0f)
                        {
                            gameRemainingTime = _remainTime;

                            if (gamesettings.myself != null)
                            {
                                int previousTime = (int)gamePreviousRemainingTime;
                                int currentTime = (int)gameRemainingTime;

                                if (multiplayerlobby.IsInEndlessRace && previousTime <= 0 && currentTime > 0)
								{
                                    // start race
                                    foreach (boat_followdummy bfd in allBoats)
                                    {
                                        bfd.boatEndlessUI.gameObject.SetActive(true);
                                        bfd.boatEndlessUI.StartTimer((int)gameRemainingTime);
                                        bfd.boatEndlessUI.SetDistanceMax(_pathSplineDistance);
                                    }
                                }

                                if ((currentTime != previousTime && currentTime <= gamesettings.myself.chronoLimitTime && previousTime > gamesettings.myself.chronoLimitTime)
                                    || (Mathf.Abs(currentTime - gamesettings.myself.chronoLimitTime) < 2 && gamesettings.myself.chronoLimitTime == gamesettings.myself.gameSessionTime))// alarm
                                {
                                    UI_Chrono.ShowChrono();
                                }
                            }

                            gamePreviousRemainingTime = gameRemainingTime;
                        }
                    }
                    if (received[mpt_stats] != null)
                    {
                        Dictionary<string, object> recestats = (Dictionary<string, object>)received[mpt_stats];
                        foreach (KeyValuePair<string, object> pair in recestats)
                        {
                            double d = (double)Helper.GetDictionaryValue(recestats, pair.Key);
                            string name = pirateStatsNames[actorNum];
                            if (!string.IsNullOrEmpty(name))
                                RRStats.RRStatsManager.instance.SetStat(name, pair.Key, d);
                        }
                    }
                    if (!_freezeTeams)
                    {
                        if (received[mpt_teamlistA] != null)
                        {
                            List<object> ll = (List<object>)received[mpt_teamlistA];
                            int ind = 0;
                            foreach (long elm in ll)
                            {
                                SetInTeamA(ind, elm);
                                if (gameState == GameState.ForceTeamSelection && elm == myId)
                                {
                                    bool alreadyValidated = myself.gameState == GameState.TeamValidated;
                                    SetMyTeam(0);
                                    if (alreadyValidated)
                                        SetMyValidated(true);
                                }
                                if (gameState == GameState.ForceTeamValidation && elm == myId)
                                {
                                    if (myself.gameState != GameState.TeamValidated)
                                    {
                                        SetMyTeam(0);
                                        SetMyValidated(true);
                                    }
                                }
                                ind++;
                            }
                        }
                        if (received[mpt_teamlistB] != null)
                        {
                            List<object> ll = (List<object>)received[mpt_teamlistB];
                            int ind = 0;
                            foreach (long elm in ll)
                            {
                                SetInTeamB(ind, elm);
                                if (gameState == GameState.ForceTeamSelection && elm == myId)
                                {
                                    SetMyTeam(1);
                                }
                                if (gameState == GameState.ForceTeamValidation && elm == myId)
                                {
                                    if (myself.gameState != GameState.TeamValidated)
                                    {
                                        SetMyTeam(1);
                                        SetMyValidated(true);
                                    }
                                }
                                ind++;
                            }
                        }
                    }
                    if (received[mpt_finishTeam] != null)
                    {
                        List<object> ll = (List<object>)received[mpt_finishTeam];
                        int ind = 0;
                        foreach (object elm in ll)
                        {
                            finishTeams[ind] = Convert.ToInt32(elm);
                            ind++;
                        }
                        if (!finishLineCross && finishTeams[0] != -1)
                        {
                            TriggerFinishLine();
                        }
                    }
                    if (received[mpt_randomSeed] != null)
                    {
                        randomSeed = Helper.GetDictionaryInt(received, mpt_randomSeed);
                    }
                    if (received[mpt_pathSplineDistance] != null)
					{
                        _pathSplineDistance = Helper.GetDictionaryValue(received, mpt_pathSplineDistance);
                    }
                    if (received[mpt_level] != null)
                    {
                        levelToLaunch = (string)received[mpt_level];
                    }
                    if (boatroot[0] != null)
                    {
                        if (received[mpt_sink0] != null)
                            boatroot[0].sinkposition = Helper.GetDictionaryValue(received, mpt_sink0);
                        if (received[mpt_sink1] != null)
                            boatroot[1].sinkposition = Helper.GetDictionaryValue(received, mpt_sink1);
                        if (received[mpt_health0] != null)
                            boatroot[0].health.ForceCurrentHealth(Helper.GetDictionaryValue(received, mpt_health0));
                        if (received[mpt_health1] != null)
                            boatroot[1].health.ForceCurrentHealth(Helper.GetDictionaryValue(received, mpt_health1));
                        if (received[mpt_wonGold0] != null)
                            boatroot[0].ForceWonGold(Helper.GetDictionaryInt(received, mpt_wonGold0));
                        if (received[mpt_wonGold1] != null)
                            boatroot[1].ForceWonGold(Helper.GetDictionaryInt(received, mpt_wonGold1));
                        if (received[mpt_stolenGold0] != null)
                            boatroot[0].ForceStolenGold(Helper.GetDictionaryInt(received, mpt_stolenGold0));
                        if (received[mpt_stolenGold1] != null)
                            boatroot[1].ForceStolenGold(Helper.GetDictionaryInt(received, mpt_stolenGold1));
                        if (received[mpt_pathSpline0] != null && received[mpt_pathSpline1] != null)
                        {
                            _pathSplineValues[0] = Helper.GetDictionaryValue(received, mpt_pathSpline0);
                            _pathSplineValues[1] = Helper.GetDictionaryValue(received, mpt_pathSpline1);

                            if (multiplayerlobby.IsInEndlessRace)
                            {
                                // start race
                                foreach (boat_followdummy bfd in allBoats)
                                {
                                    bfd.boatEndlessUI.SetDistancesInSplineRatio(_pathSplineValues[0], _pathSplineValues[1]);                                    
                                }
                                boatDistances[0] = _pathSplineValues[0] * _pathSplineDistance;
                                boatDistances[1] = _pathSplineValues[1] * _pathSplineDistance;
                            }
                        }
                    }
                    
                    if (_boatCanons != null)
                    {
                        for (int bc = 0; bc < _boatCanons.Count; ++bc)
                        {
                            boat_canon can = _boatCanons[bc];
                            if (can != null && can.gameObject.activeInHierarchy)
                            {
                                string canonname = mpt_canons + "_" + can.id;
                                if (received[canonname] != null)
                                {
                                    string[] split = ((string)received[canonname]).Split(',');
                                    if (split.Length == 2)
                                    {
                                        float oH = 0.5f;
                                        float oV = 0.5f;
                                        float.TryParse(split[0], out oH);
                                        float.TryParse(split[1], out oV);
                                        can.SetOrientationWithTargetRatio(oH, oV);
                                        _boatCanonsOrientation[bc] = can.orientation;
                                    }
                                }
                            }
                        }
                    }

                    if (_boatPumps != null)
                    {
                        for (int i = 0; i < _boatPumps.Count; ++i)
                        {
                            boat_pump pump = _boatPumps[i];
                            if (pump != null)
                            {
                                string pumpname = mpt_pumps + "_" + i;
                                if (received[pumpname] != null)
                                {
                                    string valString = ((string)received[pumpname]);
                                    float val;
                                    if (float.TryParse(valString, out val))
									{
                                        pump.SetValue(val);
                                        _boatPumpValues[i] = val;
									}
                                }
                            }
                        }
                    }

                    if (received[mpt_healths] != null)
                    {
                        string valString = ((string)received[mpt_healths]);
                        ReceiveHealthsData(valString);
                    }

                    if (received[mpt_playerEvent] != null)
                    {
                        string valString = ((string)received[mpt_playerEvent]);
                        string[] split = valString.Split('_');
                        int messageCount = split.Length / 2;
                        for (int i = 0; i < messageCount; ++i)
                        {
                            if (Enum.TryParse(split[i], out PlayerEvent pEvent))
                            {
                                string msgData = split[i + 1];
                                onPlayerEventDelegate?.Invoke(pEvent, msgData);
                            }
                        }
                    }
                }
            }
        }
    }

#if DEBUG_FREEZE
    private System.Diagnostics.Stopwatch _stopWatch = new System.Diagnostics.Stopwatch();
#endif

    protected override void Update()
    {
        base.Update();

        showstartcountdown = startcountdown;

#if UNITY_EDITOR
        showedData = _localName + " - actor " + _actorNum + " - state " + gameState;
#endif

        if (ownedobject)         // Only from our player
        {
#if UNITY_EDITOR
            if (!Player.myplayer.cam.gameObject.activeInHierarchy)
                Debug.LogError("!!! CAM_VR NOT VISIBLE !!!!");
#endif
            //#if UNITY_EDITOR
            //            // Direct from Race
            //            if (StarterHub.myself == null)
            //            {
            //                SetMyTeam(0);
            //                gameState = GameState.RaceLoaded;
            //            }
            //#endif

#if DEBUG_FREEZE
            _stopWatch.Stop();

			if (Time.deltaTime > 0.25f || _stopWatch.ElapsedMilliseconds > 250)
				Debug.Log($"[FREEZE] freeze during {Time.deltaTime} - {_stopWatch.ElapsedMilliseconds}");

			_stopWatch.Reset();
            _stopWatch.Start();
#endif

            if (gameState == GameState.Cabin)
            {
                if (StarterHub.myself != null && StarterHub.myself.gameObject.activeInHierarchy)
                {
                    PlayersCabin playersCabin = StarterHub.myself.GetComponentInChildren<PlayersCabin>();
                    if (playersCabin != null)
                    {
                        if (!_attachPlayerToCabin)
                            _attachPlayerToCabin = avatar_root.AttachPlayersToStartHub();
#if USE_STANDALONE
                        playersCabin.SetButtonsInteractables(true);
                        gameState = GameState.NameValidated;
#else
                        playersCabin.SetButtonsInteractables(false);
                        UI_ChooseYourName chooseYourName = StarterHub.myself.GetComponentInChildren<UI_ChooseYourName>(true);
                        if (chooseYourName != null && chooseYourName.isNameConfirmed)
                        {
                            gameState = GameState.NameValidated;
                            Player.myplayer.DeactivateMallets();
                        }
#endif
                    }
                    else if (GameLoader.myself.isLaunchedFromLobby)
                    {
                        gameState = GameState.LobbyLoaded;
                    }
                }
                else if (GameLoader.myself.isLaunchedFromRace)
                {
                    if (GameLoader.myself.useGhostBoat)
                        myTeam = (int)GameLoader.myself.teamColor;
                    else
                        SetMyTeam((int)GameLoader.myself.teamColor);
                    SetMyValidated(true);
                    areAllRacesStarted = true;
				}
                else if (_aiBot != null && _aiBot.botState == AIBot.BotState.NameChoosen)
                {
                    gameState = GameState.NameValidated;
                }
            }
            else if (gameState == GameState.NameValidated)
            {
                if (AreAllFlowsAtState(GameState.NameValidated, true))
                {
                    gameState = GameState.ChooseLevel;
                    levelToLaunch = null;
                    if (PhotonNetworkController.IsMaster() && aiBot == null)
                    {
                        if (StarterHub.myself != null)
                        {
                            PlayersCabin playersCabin = StarterHub.myself.GetComponentInChildren<PlayersCabin>();
                            if (playersCabin != null)
                                playersCabin.SetButtonsInteractables(true);
                        }
                    }
                }
            }
            else if (gameState == GameState.ChooseLevel)
            {
                if (!string.IsNullOrEmpty(levelToLaunch))
                {
                    gameState = GameState.LevelValidated;
                }
            }
            else if (gameState == GameState.LevelValidated)
            {
                if (AreAllFlowsAtState(GameState.LevelValidated, true))
                {
                    if (StarterHub.myself != null)
                    {
                        LaunchSceneASync launcher = LaunchSceneASync.myself;
                        if (launcher != null)
                        {
                            launcher.sceneName = levelToLaunch;
                            launcher.loadSceneMode = UnityEngine.SceneManagement.LoadSceneMode.Single;
                            launcher.fadeOutDuration = 0.5f;
                            launcher.waitToActivateScene = false;
                            launcher.LaunchScene();
                            gameState = GameState.GoToLobby;
                        }

                        //PlayersCabin playersCabin = StarterHub.myself.GetComponentInChildren<PlayersCabin>();
                        //if (playersCabin != null)
                        //{
                        //    Button button = playersCabin.GetLevelButtonFromIdx(levelchoosen);
                        //    if (button != null)
                        //    {
                        //        button.GetComponent<LaunchSceneASync>().LaunchScene();
                        //        gameState = GameState.GoToLobby;
                        //    }
                        //}
                    }
                    else if (_aiBot != null && _aiBot.botState == AIBot.BotState.LevelChoosen)
                    {
                        gameState = GameState.GoToLobby;
                    }
                }
            }
            else if (gameState == GameState.GoToLobby)
            {
                if (StarterHub.myself != null && StarterHub.myself.gameObject.activeInHierarchy)
                {
                    if (StarterHub.myself.hubType == StarterHub.HubType.BeforeRace && StarterHub.myself.isStarterHubInit)
                    {
                        avatar_root.AttachPlayersToStartHub();
                        gameState = GameState.LobbyLoaded;
                        _sandClock = StarterHub.myself.GetComponentInChildren<SandClock>(true);
                        if (_sandClock != null)
                        {
                            float sandClockDuration = StarterHub.myself.durationSandclock;
                            if (InitGameData.instance != null)
                                sandClockDuration = 30f;
                            _sandClock.gameObject.SetActive(true);
                            _sandClock.StartTime(sandClockDuration);
                            _sandClock.onSandClockOverCbk += OnSandClockOver;
                        }
#if USE_STANDALONE
                        if (StarterHub.myself.pupitre != null)
                            StarterHub.myself.pupitre.gameObject.SetActive(true);
#endif
                    }
                }
                else if (_aiBot != null && _aiBot.botState == AIBot.BotState.GoToLobby)
                {
                    SetMyTeam(myId % 2);
                    SetMyValidated(true);
                }
                else if (StarterHub.myself == null)
                {
                    SetMyTeam(0);
                    SetMyValidated(true);
                    _isInTutorial = true;
                }
            }
            else if (gameState == GameState.RaceLoaded)
            {
                levelToLaunch = null;
            }
            else if (gameState == GameState.EndGame)
            {
                if (levelToLaunch != null && (UI_EndRaceResult.myself != null || _aiBot != null))
                {
                    gameState = GameState.QuitRace;
                }
            }
            else if (gameState == GameState.QuitRace)
            {
                if (AreAllFlowsAtState(GameState.QuitRace))
                {
                    if (UI_EndRaceResult.myself != null)
                        UI_EndRaceResult.myself.ApplyChoiceButton(levelToLaunch);
                    gameState = GameState.WaitLoadScene;
                }
            }
            else if (gameState == GameState.WaitLoadScene)
            {
                if (!(LaunchSceneASync.myself.isLoadingScene || LaunchSceneASync.myself.isStartingLoadingScene))
                    gameState = levelToLaunch == gamesettings_general.myself.levelStart ? GameState.Cabin : GameState.GoToLobby;
			}
            else if (gameState == GameState.StopWithFade)
            {
                if (AreAllFlowsAtState(GameState.StopWithFade))
                {
                    gamesettings_screen gsScreen = gamesettings_screen.myself;
                    gsScreen.FadeOut(0.5f);
                    gameState = GameState.WaitQuitFromLauncher;
                }
            }

            if (gameState != GameState.WaitLoadScene)
            {
                if (gameplayEndRace && gameStarted)
                {
                    if (!_gameEndGame)
                    {
                        _gameEndGame = true;
                        StartCoroutine(EndTimeReached());
                    }
                }
                else if (gameplayrunning && teamValidated)
                {
                    if (!_gamestartload)
                    {
                        _gamestartload = true;
                        StartCoroutine(LoadMyScene());
                    }
                }

                if (!gameplayrunning && AreAllFlowsAtState(GameState.TeamSelected, true))
                {
                    areAllTeamsSelected = true;
                }
            }

            if (PhotonNetworkController.IsMaster() && aiBot == null)
            {
                if (boatroot != null && boatroot.Length == 2 && boatroot[0] != null && boatroot[0].pathFollower != null)
                {
                    gamesettings_boat gsBoat = gamesettings_boat.myself;
                    gamesettings_coefs gsCoefs = gamesettings_coefs.myself;

                    for (int i = 0; i < boatroot.Length; ++i)
                    {
                        boat_followdummy boat = boatroot[i];
                        if (boat != null && boat.pathFollower != null && !boat.pathFollower.hasSpeedModifierFromCord)
                        {
                            float cordRatio = GetCordDistanceRatio(i);
                            if (cordRatio == 1f)
                            {
                                AnimationCurve curve = gsBoat.boat_speed_modifier_curve;
                                float duration = gsBoat.boat_speed_modifier_duration;
                                bool additive = gsBoat.is_boat_speed_modifier_additive;
                                boat.pathFollower.SetModifierFromCord(curve, duration, additive);
                            }
                        }
                    }

                    if (gsBoat.boat_life_regeneration > 0f)
                    {
                        float life_regen = gsBoat.boat_life_regeneration * gsCoefs.boat_life_regeneration_coef;
                        float sunken_regen = gsBoat.boat_life_sunken_regeneration * gsCoefs.boat_life_sunken_regeneration_coef;
                        for (int i = 0; i < boatroot.Length; ++i)
                        {
                            boat_followdummy boat = boatroot[i];
                            if (boat.canRegenerateHealth && boat.health.currentHealth < boat.health.maxHealth)
                            {
                                if (boat.isSinking && boat.isSunken)
                                    boat.boatSinking.SetSinkPositionFromLife(boat.health.currentHealth + sunken_regen * Time.deltaTime);
                                else if (!boat.isSinking && !boat.isSunken)
                                    boat.health.SetCurrentHealth(boat.health.currentHealth + life_regen * Time.deltaTime);
                            }
                        }
                    }

                    float pos0 = boatroot[0].pathFollower.pathCurrentValue;
                    float pos1 = boatroot[1].pathFollower.pathCurrentValue;
                    float diff = pos0 - pos1;

                    if (multiplayerlobby.IsInEndlessRace)
                    {
                        boatroot[0].boatEndlessUI.SetDistancesInSplineRatio(pos0, pos1);
                        boatroot[1].boatEndlessUI.SetDistancesInSplineRatio(pos0, pos1);
                        boatDistances[0] = pos0 * PathHall.averagePathLength;
                        boatDistances[1] = pos1 * PathHall.averagePathLength;
                    }

                    if (gsBoat.use_boat_speed_alteration)
                    {
                        float absDiff = Mathf.Abs(diff);
                        float ratio = Mathf.InverseLerp(gsBoat.boat_distance_threshold_min, gsBoat.boat_distance_threshold_max, absDiff);
                        float coefFirst = Mathf.Lerp(1f, gsBoat.boat_first_speed_coef, ratio);
                        float coefLast = Mathf.Lerp(1f, gsBoat.boat_last_speed_coef, ratio);
                        if (pos0 > pos1)
                        {
                            boatroot[0].pathFollower.SetSpeedAlterationCoef(coefFirst);
                            boatroot[1].pathFollower.SetSpeedAlterationCoef(coefLast);
                        }
                        else
                        {
                            boatroot[0].pathFollower.SetSpeedAlterationCoef(coefLast);
                            boatroot[1].pathFollower.SetSpeedAlterationCoef(coefFirst);
                        }
                    }

                    // AIBoats update
                    AIBoat aiBoat0 = boatroot[0].aiBoat;
                    AIBoat aiBoat1 = boatroot[1].aiBoat;
                    if (aiBoat0 != null || aiBoat1 != null)
                    {
                        float distanceBetweenBoats = PathHall.averagePathLength * diff;
                        if (aiBoat0 != null)
                        {
                            aiBoat0.SetDistanceToOtherBoat(distanceBetweenBoats);
                            aiBoat0.SetOtherBoatPosition(boatroot[1].transform.position);
                        }
                        if (aiBoat1 != null)
                        {
                            aiBoat1.SetDistanceToOtherBoat(-distanceBetweenBoats);
                            aiBoat1.SetOtherBoatPosition(boatroot[0].transform.position);
                        }
                    }

                    // Minimaps
                    if (boatroot[0].boatMinimap != null && boatroot[1].boatMinimap != null)
                    {
                        float valBlue = 0f;
                        float valRed = 0f;
                        if (multiplayerlobby.IsInEndlessRace)
                        {
                            float averagePos = (pos0 + pos1) / 2f;
                            float absDiff = 0.01f;
                            valBlue = Mathf.InverseLerp(averagePos - absDiff, averagePos + absDiff, pos0);
                            valRed = Mathf.InverseLerp(averagePos - absDiff, averagePos + absDiff, pos1);
                        }
                        else
                        {
                            float finishLineSpline = gamesettings.myself.finishLineOnSpline;
                            valBlue = Mathf.InverseLerp(0f, finishLineSpline, pos0);
                            valRed = Mathf.InverseLerp(0f, finishLineSpline, pos1);
                        }
                        boatroot[0].boatMinimap.SetSliders(valBlue, valRed);
                        boatroot[1].boatMinimap.SetSliders(valBlue, valRed);
                    }
                }

                if (gameState != GameState.WaitLoadScene && !gameplayrunning && AreAllFlowsAtState(GameState.TeamValidated, true))
                {
                    if (gamesettings.myself != null)
                    {
                        if (startcountdown == -1.0f)
                        {
                            if (_isInTutorial)
                                startcountdown = 0.1f;
                            else
                                startcountdown = gamesettings.myself.startcountdownsize;
                        }
                        else
                        {
                            startcountdown = startcountdown - Time.deltaTime;
                            if (startcountdown < 0.0f)
                            {
                                startcountdown = 0.0f;
                                gameplayrunning = true;
                                _healthEvents.Clear();
                            }
                        }
                    }
                    else if (_aiBot != null && _aiBot.botState == AIBot.BotState.ValidateTeam)
                    {
                        startcountdown = 0.0f;
                        gameplayrunning = true;
                        _healthEvents.Clear();
                    }
                }
                else
                {
                    startcountdown = -1.0f;
                }

                if (gameRemainingTime > 0f)
                {
                    int previousTime = (int)gameRemainingTime;
                    gameRemainingTime = gameRemainingTime - Time.deltaTime;
                    int currentTime = (int)gameRemainingTime;

                    if (gamesettings.myself != null)
                    {
                        // TODO : show for all clients
                        if ((currentTime != previousTime && currentTime <= gamesettings.myself.chronoLimitTime && previousTime > gamesettings.myself.chronoLimitTime)
                                    || (Mathf.Abs(currentTime - gamesettings.myself.chronoLimitTime) < 2 && gamesettings.myself.chronoLimitTime == gamesettings.myself.gameSessionTime))// alarm
                        {
                            UI_Chrono.ShowChrono();
                        }
                    }

                    //gameRemainingTime = gameRemainingTime - Time.fixedDeltaTime;
                    if (gameRemainingTime < 0.0f)
                    {
                        gameplayEndRace = true;
                    }
                }
            }
            else
            {
                if (boatroot != null && boatroot.Length == 2 && boatroot[0] != null)
                {
                    // Minimaps
                    if (boatroot[0].boatMinimap != null && boatroot[1].boatMinimap != null)
                    {
                        float valBlue = 0f;
                        float valRed = 0f;
                        if (multiplayerlobby.IsInEndlessRace)
                        {
                            float averagePos = (_pathSplineValues[0] + _pathSplineValues[1]) / 2f;
                            float absDiff = 0.01f;
                            valBlue = Mathf.InverseLerp(averagePos - absDiff, averagePos + absDiff, _pathSplineValues[0]);
                            valRed = Mathf.InverseLerp(averagePos - absDiff, averagePos + absDiff, _pathSplineValues[1]);
                        }
                        else
                        {
                            float finishLineSpline = gamesettings.myself.finishLineOnSpline;
                            valBlue = Mathf.InverseLerp(0f, finishLineSpline, _pathSplineValues[0]);
                            valRed = Mathf.InverseLerp(0f, finishLineSpline, _pathSplineValues[1]);
                        }
                        boatroot[0].boatMinimap.SetSliders(valBlue, valRed);
                        boatroot[1].boatMinimap.SetSliders(valBlue, valRed);
                    }

                }
            }

#if UNITY_EDITOR && DEBUG_PHOTON_DATA
            if (allFlows != null && allFlows.Length < 2)
            {
                for (int i = 0; i < allFlows.Length; ++i)
                {
                    string data = allFlows[i].OnSendingData();
                    allFlows[i].OnReceivingData(data, true);
                }
            }
#endif
        }
        else if (aiBot != null)
        {
            if (gameState == GameState.Cabin)
            {
                if (_aiBot.botState == AIBot.BotState.NameChoosen)
                {
                    gameState = GameState.NameValidated;
                }
            }
            else if (gameState == GameState.NameValidated)
            {
                if (AreAllFlowsAtState(GameState.NameValidated, true))
                {
                    gameState = GameState.ChooseLevel;
                    levelToLaunch = null;
                }
            }
            else if (gameState == GameState.ChooseLevel)
            {
                if (!string.IsNullOrEmpty(levelToLaunch))
                {
                    gameState = GameState.LevelValidated;
                }
                else if (_aiBot.botState == AIBot.BotState.ChooseLevel && GameLoader.myself.isLaunchedFromRace)
                {
                    levelToLaunch = SceneManager.GetActiveScene().name;
                }
            }
            else if (gameState == GameState.LevelValidated)
            {
                if (AreAllFlowsAtState(GameState.LevelValidated, true))
                {
                    if (_aiBot.botState == AIBot.BotState.LevelChoosen)
                    {
                        gameState = GameState.GoToLobby;
                    }
                }
            }
            else if (gameState == GameState.GoToLobby)
            {
                if (_aiBot.botState == AIBot.BotState.GoToLobby)
                {
                    int botTeam = localBotsOnSameBoat ? (_actorNum <= nrplayersmax ? 0 : 1) : _actorNum % 2;
                    _aiBot.SetTeam(botTeam);
                    SetTeamTables(true, botTeam, _actorNum);
                    gameState = GameState.TeamValidated;
                }
            }
            else if (gameState == GameState.RaceLoaded)
            {
                levelToLaunch = null;
            }
            else if (gameState == GameState.EndGame)
            {
                if (levelToLaunch != null)
                {
                    gameState = GameState.QuitRace;
                }
            }
            else if (gameState == GameState.QuitRace)
            {
                if (AreAllFlowsAtState(GameState.QuitRace))
                {
                    gameState = GameState.WaitLoadScene;
                }
            }
            else if (gameState == GameState.WaitLoadScene)
            {
                gameState = levelToLaunch == gamesettings_general.myself.levelStart ? GameState.Cabin : GameState.GoToLobby;
            }

            if (gameState != GameState.WaitLoadScene)
            {
                if (gameplayEndRace && gameStarted)
                {
                    if (!_gameEndGame)
                    {
                        _gameEndGame = true;
                        StartCoroutine(EndTimeReached());
                    }
                }
                else if (gameplayrunning && teamValidated)
                {
                    if (!_gamestartload)
                    {
                        _gamestartload = true;
                        StartCoroutine(LoadMyScene());
                    }
                }

                if (!gameplayrunning && AreAllFlowsAtState(GameState.TeamSelected, true))
                {
                    areAllTeamsSelected = true;
                }
            }
        }
    }

    public static AIBoat GetAIBoat()
	{
        for (int i = 0; i < boatroot.Length; ++i)
		{
            boat_followdummy boat = boatroot[i];
            if (boat?.aiBoat != null)
			{
                return boat.aiBoat;
            }
		}
        return null;
	}

    private void OnSandClockOver()
	{
        if (StarterHub.myself?.pupitre != null && !multiplayerlobby.IsInKidMode)
            StarterHub.myself.pupitre.gameObject.SetActive(true);

        if (PhotonNetworkController.IsMaster() && aiBot == null)
        {
            if (!AreAllFlowsAtState(GameState.TeamSelected))
            {
                if (!allteams.ContainsKey(myId))
                    allteams.Add(myId, myTeam);

                List<int> allIds = new List<int>();
                foreach (var item in allteams)
                    allIds.Add(item.Key);

                foreach (var numActor in allIds)
				{
                    if (!IsActorInTeam(numActor,teamlistA) && !IsActorInTeam(numActor, teamlistB))
					{
                        int actorCountTeamA = GetActorCountInTeam(teamlistA);
                        int actorCountTeamB = GetActorCountInTeam(teamlistB);
                        
                        int numTeam;
                        if (actorCountTeamA == 0 && actorCountTeamB == 0)
                            numTeam = UnityEngine.Random.Range(0, 2);
                        else if (actorCountTeamB == 0)
                            numTeam = 0;
                        else if (actorCountTeamA == 0)
                            numTeam = 1;
                        else if (actorCountTeamA < actorCountTeamB)
                            numTeam = 0;
                        else if (actorCountTeamA > actorCountTeamB)
                            numTeam = 1;
                        else
                            numTeam = UnityEngine.Random.Range(0, 2);

                        if (numActor == myId)
                            SetMyTeam(numTeam);
                        else
                            SetTeamTables(true, numTeam, numActor);
                    }
                }
                if (multiplayerlobby.IsInKidMode)
                    gameState = GameState.ForceTeamValidation;
                else
                    gameState = GameState.ForceTeamSelection;
                startcountdown = 1f;
            }
        }
	}

    public static bool AreAllBoatsWithPathToFollow()
    {
        foreach(boat_followdummy boat in allBoats)
        {
            if (boat.pathFollower == null)
                return false;
        }
        return true;
    }

    public static float GetCordDistanceRatio(int team = 0)
    {
        return corddistance[team];
    }

    public static void SetCordDistanceRatio(float val, int team = 0)
    {
#if DEBUG_CORD
        if (val == 0f || val == 1f || val == -1f || corddistance[team] == 0f || corddistance[team] == 1f || corddistance[team] == -1f)
			if (corddistance[team] != val)
				Debug.Log($"[CORD] SetCordDistanceRatio {val} team {team}");
#endif
        corddistance[team] = val;
    }

    public static bool IsPullingCord(int team = 0)
    {
        return cordpulling[team];
    }

    public static void SetPullingCord(bool val, int team = 0)
    {
#if DEBUG_CORD
        Debug.Log($"[CORD] SetPullingCord {val} team {team}");
#endif
        cordpulling[team] = val;
    }

    public static int GetCordPlayerId(int team = 0)
    {
        return cordPlayerId[team];
    }

    public static void SetCordPlayerId(int playerId, int team = 0)
    {
#if DEBUG_CORD
        Debug.Log($"[CORD] SetCordPlayerId {playerId} team {team}");
#endif
        cordPlayerId[team] = playerId;
    }

    public static bool IsCordPlayerIdMyId(int team = 0)
    {
        return GetCordPlayerId(team) == myId;
    }

    public static bool CanUseCord(int team = 0)
    {
        if (!areAllRacesStarted || gameplayEndRace)
            return false;
        if (GetCordDistanceRatio(team) <= 0f)
            return true;
        int playerId = GetCordPlayerId(team);
        return playerId == -1 || playerId == myId;
    }

    public static bool AreAllFlowsAtState(GameState state, bool useFind = false)
    {
        if (useFind)
        {
            SearchAllFlows();
        }
        else
        {
            if (allFlows == null || allFlows.Length == 0)
                return false;
        }

        foreach (gameflowmultiplayer flow in allFlows)
        {
            if (flow.gameState < state)
                return false;
        }
        return true;
    }

    public static void SetTeamFinish(int team )
    {
        // Endless mode : don't use the finish line
        if (gameMode == GameMode.Endless)
            return;

        if (PhotonNetworkController.IsMaster())
        {
            for (int i = 0; i < finishTeams.Length; ++i)
            {
                if (finishTeams[i] == -1)
                {
                    finishTeams[i] = team;
                    break;
                }
                else if (finishTeams[i] == team)
                {
                    break;
                }
            }

            if (!finishLineCross && finishTeams[0] != -1)
            {
                TriggerFinishLine();
            }
        }
    }

    public static void SetTeamEndGoals(EndlessGoals.ResultGame resultGame)
    {
        //Debug.Log($"SetTeamEndGoals {resultGame} {PhotonNetwork.IsMasterClient}");
        // Endless mode only
        if (gameMode == GameMode.Endless && PhotonNetworkController.IsMaster())
        {
            switch (resultGame)
            {
                case EndlessGoals.ResultGame.Accomplished:
                    finishTeams[0] = 0;
                    finishTeams[1] = 1;
                    break;
                case EndlessGoals.ResultGame.Failed:
                    finishTeams[0] = 1;
                    finishTeams[1] = 0;
                    break;
            }
            if (!finishLineCross)
            {
                TriggerFinishLine();
            }
            gameplayEndRace = true;
        }
    }

    public static void TriggerFinishLine()
	{
        finishLineCross = true;
        if (gamesettings.myself.maxTimeAfterFinishLine > 0f)
		{
            if (gameRemainingTime > gamesettings.myself.maxTimeAfterFinishLine)
                gameRemainingTime = gamesettings.myself.maxTimeAfterFinishLine;
        }
        UI_Chrono.ShowChrono();
        if (RaceManager.myself != null)
        {
            int team = finishTeams[0];
            bool win = Player.myplayer.team == team;
            RaceManager.myself.TriggerFinishLine(win, team);
        }
    }

    public static int GetTeamPosition( int team )
    {
        if (boatDistances[0] > 0f && boatDistances[1] > 0f)
		{
            if (boatDistances[team] > boatDistances[1 - team])
                return 0;
            return 1;
		}

        int pos = 0;
        bool found = false;
        while( !found && pos < finishTeams.Length)
        {
            if(finishTeams[pos] == team )
            {
                found = true;
            }
            else
            {
                pos++;
            }
        }
        if( found )
        {
            return pos;
        }
        return -1;
    }

    public static void SetMyValidated(bool set, bool freeze = false)
    {
        if (gameplayrunning)
            return;
#if DEBUG_STATES
        Debug.Log("[STATE] SetMyValidated " + set);
#endif
        if (freeze)
            myself.FreezeTeams();
        myself.gameState = set ? GameState.TeamValidated : GameState.TeamSelected;
        if (StarterHub.myself != null && set)
		{
            StarterHub.myself.TriggerDeactivateAtTeamValidation();
        }
    }

    public static void TriggerPlayerEvent(PlayerEvent playerEvent, string data="")
    {
        PlayerEventData ped = new PlayerEventData();
        ped.playerEvent = playerEvent;
        ped.data = data;
        _playerEvents.Add(ped);
    }

    IEnumerator LoadMyScene()
    {
#if DEBUG_STATES
        Debug.Log($"[STATES] LoadMyScene start!");
#endif

        StarterHub starterhub = StarterHub.myself;
        if (starterhub != null)
        {
            if (starterhub.NeedToActivateRaceScene())
            {
                //Debug.Log("[CAM] ActivateRaceScene");
                starterhub.ActivateRaceScene();
            }
            else
            {
                starterhub.LaunchRaceScene();
                DontDestroyOnLoad(starterhub.gameObject);
            }
        }

        while (RaceManager.myself == null && _aiBot == null)
            yield return null;

        //Debug.Log("[CAM] RaceManager is valid");

        if (starterhub != null)
        {
            while (starterhub.IsLoadingRaceScene())
                yield return null;

            if (starterhub != null)
            {
                starterhub.hubmusic.Stop();
                starterhub.gameObject.SetActive(false);
            }
        }

        GameObject Body_Tuto = Player.myplayer.bodyTuto;
        if (Body_Tuto)
            Body_Tuto.SetActive(false);

        yield return null;

        if (_aiBot != null)
		{
            while (_aiBot.botState != AIBot.BotState.ValidateTeam)
                yield return null;
		}

        gameState = GameState.ShowRaceWorld;

        // wait here 
        while (!AreAllFlowsAtState(gameState))
            yield return null;

        if (_aiBot != null)
        {
            yield return new WaitForSeconds(5f);
            myboat = boatroot[_aiBot.team];
        }
        else
        {
            gamedata = RaceManager.myself.worldRoot;
            gamedata.SetActive(true);

            allFlows = FindObjectsOfType<gameflowmultiplayer>();
            allBoats = gamedata.GetComponentsInChildren<boat_followdummy>(true);

            Debug.Assert(allBoats != null && allBoats.Length > 0, "Boats not found!");

            if (allBoats.Length == 2)
            {
                if (allBoats[0].teamColor == boat_followdummy.TeamColor.Red)
                {
                    boatroot[1] = allBoats[0];
                    boatroot[0] = allBoats[1];
                }
                else
                {
                    boatroot[0] = allBoats[0];
                    boatroot[1] = allBoats[1];
                }

                myboat = boatroot[myTeam];

                gamesettings_boat gsBoat = gamesettings_boat.myself;

                if (myTeam == 1)
                {
                    Debug.Log("Destroy1");
                    Destroy(RaceManager.myself.boat02voiceobjects);
                    Destroy(boatroot[0].gameObject.FindInChildren(gsBoat.boatvoicename));
                    Destroy(myboat.gameObject.FindInChildren(gsBoat.boat01objecttoremove));
                    myCaptain = (myboat.gameObject.FindInChildren(gsBoat.boat02objecttoremove));
                    Destroy(boatroot[0].gameObject.FindInChildren(gsBoat.boat02objecttoremove));
                    VoiceManager.myself?.GoToRedVoiceRoom();
                    navigationfloor[] floors = boatroot[0].gameObject.GetComponentsInChildren<navigationfloor>(true);
                    foreach (navigationfloor floor in floors)
                        GameObject.Destroy(floor.gameObject);
                }
                if (myTeam == 0)
                {
                    Debug.Log("Destroy0");
                    Destroy(RaceManager.myself.boat01voiceobjects);
                    Destroy(boatroot[1].gameObject.FindInChildren(gsBoat.boatvoicename));
                    Destroy(myboat.gameObject.FindInChildren(gsBoat.boat02objecttoremove));
                    myCaptain = (myboat.gameObject.FindInChildren(gsBoat.boat01objecttoremove));
                    Destroy(boatroot[1].gameObject.FindInChildren(gsBoat.boat01objecttoremove));
                    navigationfloor[] floors = boatroot[1].gameObject.GetComponentsInChildren<navigationfloor>(true);
                    foreach (navigationfloor floor in floors)
                        GameObject.Destroy(floor.gameObject);
                }
            }
            else
			{
                boatroot[0] = allBoats[0];
                boatroot[1] = allBoats[0];
                myboat = boatroot[0];
            }

            while (boatroot[0].dummy == null)
                yield return null;
            while (boatroot[1].dummy == null)
                yield return null;

            // Set boat life
            float life = gamesettings_boat.myself.boat_start_life * gamesettings_coefs.myself.boat_start_life_coef;
            boatroot[0].SetStartingLife(life);
            boatroot[1].SetStartingLife(life);

            if (RaceManager.myself.gamemusic != null)
                RaceManager.myself.gamemusic.Play();

            yield return null;

            foreach (boat_followdummy boat in allBoats)
            {
                GameObject createboat = boat.gameObject.GetComponentInChildren<boat_sinking>().gameObject;
                //            GameObject createboat = Instantiate(Resources.Load("Boat_Pirate02", typeof(GameObject))) as GameObject;
                //            createboat.transform.SetParent(dum.transform);
                createboat.transform.localPosition = Vector3.zero;
                createboat.transform.localScale = Vector3.one;
                createboat.transform.localEulerAngles = new Vector3(0, 90, 0);
                Health hel = boat.gameObject.GetComponent<Health>();
                hel.reactionanimations = createboat.GetComponent<Animator>();
                switch (boat.teamColor)
                {
                    case boat_followdummy.TeamColor.Blue:
                        if (forceGhostBoat || teamlistA[0] == -1)
                            boat.SetAiBoat();
                        break;
                    case boat_followdummy.TeamColor.Red:
                        if (forceGhostBoat || teamlistB[0] == -1)
                            boat.SetAiBoat();
                        break;
                }
            }
        }

		yield return null;

        gameState = GameState.PlayersToBoat;

        // wait here 
        while (!AreAllFlowsAtState(gameState))
            yield return null;

        if (_aiBot != null)
        {
            GetSpawnpointOnBoat(_actorNum, _aiBot.team);
            yield return new WaitForSeconds(2);
        }
        else
        {
            float waitTeleportDuration = 2f;
            float startTime = Time.time;
            while (Time.time - startTime < waitTeleportDuration)
			{
                if (!Player.myplayer.IsOneAvatarInTeleportation())
                    break;
                yield return null;
			}

            if (Player.myplayer.IsOneAvatarInTeleportation()) 
			{
                // Bad teleportation detected
                Player.myplayer.ShowAllAvatars();
            }

            avatar_root.AttachPlayersToBoats(boatroot);
            spawnpoint spawnPointPlayer = GetSpawnpointOnBoat(_actorNum, myTeam);
            if (spawnPointPlayer != null)
            {
                Player.myplayer.Teleport(spawnPointPlayer.transform.position, spawnPointPlayer.transform.rotation);
                Player.myplayer.SetStartSpawnPoint(spawnPointPlayer.transform);
            }
            else
			{
                Debug.LogError("No Boat Spawnpoint found!!!");
			}
        }

        // Compute spline distance
        if (PhotonNetworkController.IsMaster() && aiBot == null)
            PathHall.InitAllPathAverage();

        gameState = GameState.RaceLoaded;
        currentLevelName = levelToLaunch;

        // wait here 
        while (!AreAllFlowsAtState(gameState))
            yield return null;

        areAllRacesLoaded = true;
#if DEBUG_STATES
        Debug.Log($"[STATES] areAllRacesLoaded reached!");
#endif

        if (_aiBot != null)
        {
            yield return new WaitForSeconds(2f);
        }
        else
        {
            if (RaceManager.myself != null)
            {
                yield return RaceManager.myself.ComputeGoldEnum();
                Debug.Log($"ComputeTotalGold RaceManager {RaceManager.myself.totalGold}");
            }
        }

        RRStats.RRStatsManager.instance.SetStat(_statsName, gamesettings.STAT_POINTS, 0);
        RRStats.RRStatsManager.instance.SetStat(_statsName, gamesettings.STAT_FIRE, 0);
        RRStats.RRStatsManager.instance.SetStat(_statsName, gamesettings.STAT_TOUCH, 0);
        RRStats.RRStatsManager.instance.SetStat(_statsName, gamesettings.STAT_DEATH, 0);
        RRStats.RRStatsManager.instance.SetStat(_statsName, gamesettings.STAT_KILLS, 0);
        RRStats.RRStatsManager.instance.SetStat(_statsName, gamesettings.STAT_CHESTS, 0);
        RRStats.RRStatsManager.instance.SetStat(_statsName, gamesettings.STAT_TURRETS, 0);
        RRStats.RRStatsManager.instance.SetStat(_statsName, gamesettings.STAT_BIRDS, 0);
        RRStats.RRStatsManager.instance.SetStat(_statsName, gamesettings.STAT_HEARTS, 0);
        RRStats.RRStatsManager.instance.SetStat(_statsName, gamesettings.STAT_SHIELDS, 0);
        RRStats.RRStatsManager.instance.SetStat(_statsName, gamesettings.STAT_SPLINES, 0);
        RRStats.RRStatsManager.instance.SetStat(_statsName, gamesettings.STAT_MERMAIDS, 0);
        RRStats.RRStatsManager.instance.SetStat(_statsName, gamesettings.STAT_MONSTERS, 0);
        RRStats.RRStatsManager.instance.SetStat(_statsName, gamesettings.STAT_SKULLS, 0);
        RRStats.RRStatsManager.instance.SetStat(_statsName, gamesettings.STAT_ARCHIVES, 0);
        RRStats.RRStatsManager.instance.SetStat(_statsName, gamesettings.STAT_SCIENTISTS, 0);

        if (_aiBot != null)
        {
            yield return new WaitForSeconds(2f);
        }
        else
        {
            _boatCanons = new List<boat_canon>();
            _boatCanons.AddRange(boatroot[0].gameObject.GetComponentsInChildren<boat_canon>(true));
            _boatCanons.AddRange(boatroot[1].gameObject.GetComponentsInChildren<boat_canon>(true));

            int canonCount = _boatCanons.Count;

            _boatCanonsOrientation = new Vector2[canonCount];

            for (int ac = 0; ac < canonCount; ac++)
            {
                _boatCanonsOrientation[ac] = Vector2.one * 0.5f;
            }

            _boatPumps = new List<boat_pump>();
            _boatPumps.AddRange(boatroot[0].gameObject.GetComponentsInChildren<boat_pump>(true));
            _boatPumps.AddRange(boatroot[1].gameObject.GetComponentsInChildren<boat_pump>(true));

            int pumpCount = _boatPumps.Count;

            _boatPumpValues = new float[pumpCount];
            for (int pump = 0; pump < pumpCount; ++pump)
			{
                _boatPumpValues[pump] = _boatPumps[pump].GetValue();
            }

            if (!_isInTutorial)
                yield return new WaitForSeconds(0.5f);

            gamesettings_general.myself.FadeInMusicVolume();

            if (!Player.myplayer.isInPause)
            {
                gamesettings_screen gsScreen = gamesettings_screen.myself;
                gsScreen.FadeIn();
                while (gsScreen.faderunning)
                    yield return null;
            }

            if (!_isInTutorial)
                yield return new WaitForSeconds(gamesettings.myself.delayAfterLoadToOpenDoors);

            if (PhotonNetworkController.IsMaster() && allBoats.Length == 2 && aiBot == null)
            {
                while (!AreAllBoatsWithPathToFollow())
                    yield return null;
            }
        }

        gameState = GameState.OpenDoors;

        while (!AreAllFlowsAtState(gameState))
            yield return null;

        
        if (_aiBot != null)
        {
            yield return new WaitForSeconds(5f);
        }
        else
        {
            if (allBoats.Length == 2 && !_isInTutorial && RaceManager.myself != null)
            {
                Animator[] doorAnimators = RaceManager.myself.doorAnimators;
                if (doorAnimators != null && doorAnimators.Length > 0)
                {
                    foreach (Animator anim in doorAnimators)
                    {
                        if (anim != null)
                            anim.SetBool("RaceStarts", true);
                    }
                }
                else
                {
                    Debug.LogError("Missing Door Animators Reference in RaceManager!");
                }

                yield return new WaitForSeconds(gamesettings.myself.delayAfterOpendDoorsToStartRace);
            }
        }

        if (_aiBot != null)
        {
            while (_aiBot.botState != AIBot.BotState.LoadRace)
                yield return null;
        }

        gameState = GameState.RaceStarted;

        while (!AreAllFlowsAtState(gameState))
            yield return null;

        areAllRacesStarted = true;
#if DEBUG_STATES
        Debug.Log($"[STATES] areAllRacesStarted reached!");
#endif

        if (RaceManager.myself != null && _aiBot == null && allBoats.Length == 2)
        {
            if (PhotonNetworkController.IsMaster() && aiBot == null)
            {
                // set timer 
                if (multiplayerlobby.IsInEndlessRace)
                    gameRemainingTime = multiplayerlobby.endlessDuration;
                else
                    gameRemainingTime = gamesettings.myself.gameSessionTime;
#if DEBUG_STATES
                Debug.Log($"[STATES] gameRemainingTime {gameRemainingTime}!");
#endif
                // start race
                foreach (boat_followdummy bfd in allBoats)
                {
                    bfd.pathFollower.gameplayrunning = true;
                    if (multiplayerlobby.IsInEndlessRace)
                    {
                        bfd.boatEndlessUI.gameObject.SetActive(true);
                        bfd.boatEndlessUI.StartTimer((int)gameRemainingTime);
                        bfd.boatEndlessUI.SetDistanceMax(PathHall.averagePathLength);
                    }
                }
            }
        }
    }

    private spawnpoint GetSpawnpointOnBoat(int actorNum, int team)
	{
        int searchspawn = GetActorIndexInTeam(actorNum, team);
#if DEBUG_STATES
        Debug.Log($"[STATES] GetActorIndexInTeam: " + searchspawn);
#endif
        int nrspawn = 0;
        spawnpoint[] allspawn = myboat.GetComponentsInChildren<spawnpoint>();

#if DEBUG_STATES
        Debug.Log($"[STATES] Spawnpoint count on boat: " + allspawn.Length);
#endif

        foreach (spawnpoint sp in allspawn)
        {
            if (sp.gamespawn)
            {
                if (sp.id == searchspawn)
                    break;
            }
            nrspawn++;
        }

        nrspawn %= allspawn.Length;

        return allspawn[nrspawn];
    }

    public IEnumerator EndTimeReached()
    {
        gameState = GameState.EndGame;

        // Check if all stats are here
        if (!AreAllStatsReceived())
            yield return null;

        if (_aiBot != null)
            yield break;

        if (!_isQuittingRace && RaceManager.myself != null)
        {
            yield return RaceManager.myself.MoveBoatsAtEndOfRace();

            if (RaceManager.myself.finalObjectCondition == null || RaceManager.myself.finalObjectCondition.activeInHierarchy)
            {
                RaceManager.myself.finalOnEndRace?.Invoke();

                if (RaceManager.myself.finalDelayToShowEndRaceScreen > 0f)
                    yield return new WaitForSeconds(RaceManager.myself.finalDelayToShowEndRaceScreen);
            }
        }

        GameObject Treasure = null;
        bool isResultWithOneTeam = false;
        foreach (boat_followdummy bfd in allBoats)
        {
            // set end race
            PathFollower pf = bfd.pathFollower;
            if (pf != null)
            {
                pf.raceended = true;
            }

            if (bfd.team == myTeam)
            {
                Treasure = bfd.gameObject.FindInChildren("Treasure_Display");
                Treasure.SetActive(true);
            }
            /*
            else
			{
                isResultWithOneTeam = bfd.aiBoat != null;
            }
            */

            long[] teamList = bfd.teamColor == boat_followdummy.TeamColor.Blue ? teamlistA : teamlistB;
            int playerCount = CountPlayersInBoat(teamList);
            if (playerCount == 0)
                isResultWithOneTeam = true;

#if !USE_STANDALONE
            if (bfd.stolenGold > 0 && bfd.wonGold > 0)
			{
                float missingGoldRatio = (float)(bfd.wonGold - bfd.goldOnBoat) / (float)bfd.wonGold;
                for (int i = 0; i < playerCount; ++i)
				{
                    string sName = pirateStatsNames[teamList[i]];
                    Debug.Log($"pirateStatsNames in {bfd.teamColor} {sName}");
                    Dictionary<string, double> stats = RRStats.RRStatsManager.instance.GetStats(sName);
                    if (stats != null)
                    {
                        float treasures = stats.ContainsKey(gamesettings.STAT_POINTS) ? (float)stats[gamesettings.STAT_POINTS] : 0;
                        treasures -= treasures * missingGoldRatio;
                        stats[gamesettings.STAT_POINTS] = Mathf.FloorToInt(treasures);
                    }
                }
            }
#endif
        }

        if (Treasure != null)
        {
            // show result screen
            GameObject screenRoot = Treasure.FindInChildren("End_Race_ResultRoot");
            if (screenRoot != null)
            {
                UI_EndRaceResult prefab = isResultWithOneTeam ? gamesettings.myself.endRaceResultPrefabOneTeam : gamesettings.myself.endRaceResultPrefab;
                _endRaceScreen = GameObject.Instantiate<UI_EndRaceResult>(prefab, screenRoot.transform);
                _endRaceScreen.transform.localPosition = Vector3.zero;
                _endRaceScreen.transform.localRotation = Quaternion.Euler(Vector3.zero);
                if (_isQuittingRace)
				{
                    _endRaceScreen.GetComponent<Canvas>().enabled = false;
                    _endRaceScreen.playCaptainAudioAtInitScreen = false;
                }
            }
        }
    }

    public int CountPlayersInBoat(long[] teamList)
	{
        int nCount = 0;
        for (int i = 0; i < teamList.Length; ++i)
		{
            if (teamList[i] >= 0)
                nCount++;
		}
        return nCount;
	}
   
    public void ReplayRace()
    {
        levelToLaunch = gamesettings_general.myself.levelSettings.GetSceneName(levelListId, levelIndex);
        ApplyEndButtonChoice();
    }

    public void NextRace()
    {
        levelToLaunch = gamesettings_general.myself.levelSettings.GetSceneName(levelListId, levelIndex + 1);
        ApplyEndButtonChoice();
    }

    public override void QuitRace(bool killProcess = false)
    {
        base.QuitRace(killProcess);
        if (!killProcess)
        {
            levelToLaunch = gamesettings_general.myself.levelStart;
            ApplyEndButtonChoice();
        }
    }

    private void ApplyEndButtonChoice()
	{
        StopAllCoroutines();
        if (_aiBot != null)
        {
            gameState = GameState.QuitRace;
            _aiBot.CheckQuitGame();
            return;
        }
        else
		{
            apicalls.myself?.StopGameCounter();
        }
        StartCoroutine(QuitRaceEnum(levelToLaunch));
    }

    private IEnumerator QuitRaceEnum(string level)
	{
        Debug.Log($"QuitRaceEnum to {level} state {gameState}");
        if (gameState < GameState.RaceLoaded)
        {
            string levelName = gamesettings_general.myself.levelStart;
            if (!string.IsNullOrEmpty(levelName))
            {
                LaunchSceneASync launcher = LaunchSceneASync.myself;
                if (launcher != null)
                {
                    if (launcher.isLoadingScene && launcher.waitToActivateScene)
                        launcher.ActivateScene();
                    while (launcher.isLoadingScene)
                        yield return null;
                    launcher.sceneName = levelName;
                    launcher.loadSceneMode = LoadSceneMode.Single;
                    launcher.fadeOutDuration = 0.5f;
                    launcher.waitToActivateScene = false;
                    launcher.LaunchScene();
                }
            }
        }
        else
        {
            _isQuittingRace = true;
            gameplayEndRace = true;
            gameState = GameState.EndGame;
            while (_endRaceScreen == null)
                yield return null;
            _endRaceScreen.playCaptainAudioAtInitScreen = false;
            levelToLaunch = level;
            _isQuittingRace = false;
            gameState = GameState.QuitRace;
        }
    }
}
