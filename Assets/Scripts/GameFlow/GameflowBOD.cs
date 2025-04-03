//#define DEBUG_PHOTON_DATA
//#define DEBUG_CORD
//#define DEBUG_FREEZE
#define DEBUG_STATES 

using MiniJSON;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameflowBOD : GameflowBase
{
    #region Enums

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
        LevelLoaded,
        LevelStarted,
        TowerDef,
        EndGame,
        QuitRace,
        WaitLoadScene,
        StopWithFade,
        WaitQuitFromLauncher
    }

    public enum PlayerEvent
	{
        RecoverAntenna,
        PlayerHealth,
        PlayerDamage
    }

    #endregion

    #region struct

    public struct TowerDefEventData
    {
        public TowerDefManager.TowerDefState state;
        public int numWave;
        public int relWave;
    }

    public struct PlayerEventData
    {
        public PlayerEvent playerEvent;
        public int playerId;
        public object data;
    }

    #endregion


    protected static List<TowerDefEventData> _towerDefEvents = new List<TowerDefEventData>();
    protected static List<PlayerEventData> _playerEvents = new List<PlayerEventData>();

    // Data to be sent
    public GameState gameState
    {
        get => _gameState;
#if DEBUG_STATES
        set 
        {
            _gameState = value;
            if (ownedobject)
                Debug.Log($"[STATES] gameState {_gameState} player {myId}");
        }
#else
        set { _gameState = value; }
#endif
    }
    private GameState _gameState = GameState.Cabin;
    public bool teamSelected => gameState >= GameState.TeamSelected;
    public bool teamValidated => gameState >= GameState.TeamValidated;
    public bool gameStarted => gameState >= GameState.LevelStarted;
    public bool isInCabin => gameState < GameState.GoToLobby;

    public override bool nameValidated => gameState >= GameState.NameValidated;

    private bool gamestartload = false;
    private bool _resetDone = false;

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
    public static float completion = 0.0f;

    public static bool[] unlockedSkullsInRace = new bool[5];

    public static GameflowBOD myself => instance as GameflowBOD;

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

    public GameObject gamedata;

    private bool _isQuittingRace = false;
    private bool _needToSendStats = false;

    /// <summary>
    /// Multiplayer terms
    /// </summary>
    const string MPT_HEALTHS = "hs";
    const string MPT_MYTEAM = "mt";
    const string MPT_GAMEPLAYRUNNING = "gr";
    const string MPT_GAMEPLAYENDRACE = "ge";
    const string MPT_NETID = "ni";
    const string MPT_PLAYERNAME = "pn";
    const string MPT_GAMESTATE = "gs";
    const string MPT_PLAYERSTATE = "ps";
    const string MPT_PLAYER_EVENTS = "pe";
    const string MPT_LEVEL = "lv";
    const string MPT_STARTCOUNTDOWN = "sc";
    const string MPT_TEAMLISTA = "tA";
    const string MPT_TEAMLISTB = "tB";
    const string MPT_HANDLEFTITEM = "hl";
    const string MPT_HANDRIGHTITEM = "hr";
    //const string MPT_BELTLEFTITEM = "bl";
    //const string MPT_BELTRIGHTITEM = "br";
    //const string MPT_BELTFRONTITEM = "bf";
    //const string MPT_BELTBACKITEM = "bb";
    const string MPT_REMAININGTIME = "rt";
    const string MPT_STATS = "st";
    const string MPT_SKIN = "sk";
    const string MPT_RANDOMSEED = "rs";
    const string MPT_TOWERDEFEVENTS = "te";
    const string MPT_TIME = "ti";

    protected override void Start()
    {
        base.Start();
        if (ownedobject)
        {
#if USE_STANDALONE
            string name = myPirateName;
            myId = PhotonNetworkController.GetPlayerId();
            if (!string.IsNullOrEmpty(name))
            {
                myPirateName = name;
            }
#else
            myId = PhotonNetwork.LocalPlayer.ActorNumber - 1;
            Player.myplayer.UpdateThemeMaterial();
#endif
            Player.myplayer.SetId(myId);

#if USE_BOD
            myPirateName = gamesettings_player.myself.GetNameFromIndex(myId);
#else
            if (myPirateName == null)
                myPirateName = gamesettings_player.myself.GetNameFromIndex(myId);
#endif
            Reset(true);

            if (!string.IsNullOrEmpty(Player.myplayer.currentSkinName))
                SetMySkin(Player.myplayer.currentSkinName);

            TowerDefManager.myself?.gamemusic?.Stop();

            ChoiceButtons.onSetChoiceCallback += OnChoiceButtons;
            TowerDefManager.onTowerDefState += OnTowerDefState;

            StartCoroutine(WaitToChooseYourNameEnum());
        }
    }

    protected override void OnDestroy()
    {
        Debug.Log("[STATES] GameflowBOD destroyed!");
        if (ownedobject)
        {
            ChoiceButtons.onSetChoiceCallback -= OnChoiceButtons;
            TowerDefManager.onTowerDefState -= OnTowerDefState;
        }
        base.OnDestroy();
    }

    private IEnumerator WaitToChooseYourNameEnum()
	{
        while (UI_ChooseYourName.myself == null)
            yield return null;
        myPirateName = gamesettings_player.myself.GetNameFromIndex(myId);
        UI_ChooseYourName.myself.Init(UI_ChooseYourName.NameType.Player, myPirateName);
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
                piratedeaths[i] = false;
                piratehealths[i] = -1.0f;
                if (resetToCabin)
                    pirateskins[i] = gamesettings_player.myself.GetSkinName(0);
            }

            allteams.Clear();
            
            _isInTutorial = false;
            CheckRandomSeed();
            if (resetToCabin)
            {
                allFlows = null;
                currentLevelName = null;
            }

            for (int i = 0; i < nrplayersperteam; i++)
            {
                SetInTeamA(i, -1);
                SetInTeamB(i, -1);
            }

            gamestartload = false;

            if (resetToCabin)
                gameState = GameState.Cabin;
            else
                _resetDone = true;

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

            if (!TowerDefManager.keepGoldScoreAndGuns)
            {
                if (Player.myplayer != null)
                {
                    Player.myplayer.ResetGoldAndScore();
                    Player.myplayer.ResetGuns();
                }
            }

            // Reset voice over max priority
            voicepriority.playinprio = 0;
            //Debug.Log($"[DEBUG_VOICEOVER] Reset on loadscene current prio {voicepriority.playinprio}");
        }
    }

    public override bool AreAvatarsVisibles()
    {
        return gameState >= GameState.NameValidated;
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
#if DEBUG_STATES
                    Debug.Log($"[STATES] levelListId {levelListId} levelIndex {levelIndex} levelToLaunch {levelToLaunch}");
#endif
                    gameState = GameState.LevelValidated;
                }
                break;
            case ChoiceButtons.ChoiceType.EndRace:
                if (gameState == GameState.EndGame)
                {
                    if (string.IsNullOrEmpty(levelListId))
                        levelListId = "TowerDef_Endless";
                    switch ((gamesettings.EndButtons)data)
                    {
                        case gamesettings.EndButtons.END_REPLAY_RACE:
                            levelToLaunch = gamesettings_general.myself.levelSettings.GetSceneName(levelListId, levelIndex);
                            if (levelToLaunch == null)
                                levelToLaunch = SceneManager.GetActiveScene().name;
                            TowerDefManager.keepGoldScoreAndGuns = true;
                            TowerDefManager.maxGunsUpgradeOnIntro = 2;
                            break;
                        case gamesettings.EndButtons.END_NEXT_RACE:
                            levelIndex++;
                            levelToLaunch = gamesettings_general.myself.levelSettings.GetSceneName(levelListId, levelIndex);
                            TowerDefManager.keepGoldScoreAndGuns = true;
                            TowerDefManager.maxGunsUpgradeOnIntro = 1;
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
                                TowerDefManager.keepGoldScoreAndGuns = false;
                            }                                
                            break;
                        case gamesettings.EndButtons.END_CONTINUE:
                            TowerDefManager.myself.ContinueGame();
                            levelToLaunch = null;
                            TowerDefManager.keepGoldScoreAndGuns = true;
                            TowerDefManager.maxGunsUpgradeOnIntro = 2;
                            return;
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

    private void OnTowerDefState(TowerDefManager.TowerDefState state, int numWave, int relativeWave, int level)
	{
        if (PhotonNetworkController.IsMaster())
		{
            TriggerTowerDefEvent(state, numWave, relativeWave);
        }
        else
		{
            _needToSendStats = true;
        }
	}

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
            jsontext += "\"" + MPT_GAMESTATE + "\":" + (int)gameState + ",";
            jsontext += "\"" + MPT_PLAYERSTATE + "\":" + (int)_playerState + ",";

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

            jsontext += "\"" + MPT_NETID + "\":" + netId + ",";
            if (gameState < GameState.TowerDef)
            {
                jsontext += "\"" + MPT_MYTEAM + "\":" + myTeam + ",";
                jsontext += "\"" + MPT_PLAYERNAME + "\":\"" + myPirateName + "\",";
                jsontext += "\"" + MPT_SKIN + "\":\"" + pirateskins[myId] + "\",";
            }
            else
			{
                if (_healthEvents.Count > 0)
                {
                    string msgData = "";
                    foreach (Health.HealthById data in _healthEvents)
                    {
                        msgData += data.healthId + "_" + data.playerId + "_" + data.value.ToString("F3") + "_";
                    }
                    msgData = msgData.Remove(msgData.Length - 1);
                    jsontext += "\"" + MPT_HEALTHS + "\":\"" + msgData + "\",";
                    _healthEvents.Clear();
                }

                if (_playerEvents.Count > 0)
                {
                    string msgData = "";
                    foreach (PlayerEventData data in _playerEvents)
                    {
                        msgData += data.playerEvent.ToString() + "_" + data.playerId + "_" + data.data + "_";
                    }
                    msgData = msgData.Remove(msgData.Length - 1);
                    jsontext += "\"" + MPT_PLAYER_EVENTS + "\":\"" + msgData + "\",";
                    _playerEvents.Clear();
                }

            }

            if (PhotonNetwork.IsMasterClient)
            {
                if (gameState == GameState.LevelValidated || gameState == GameState.QuitRace)
                {
                    jsontext += "\"" + MPT_LEVEL + "\":\"" + levelToLaunch + "\",";
                }

                if (gameplayrunning)
                {
                    //jsontext += "\"" + MPT_GAMEPLAYRUNNING + "\":true,";
                }
                else
                {
                    //jsontext += "\"" + MPT_GAMEPLAYRUNNING + "\":false,";

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

                    jsontext += "\"" + MPT_TEAMLISTA + "\":[" + jsonTeamListA + "],";
                    jsontext += "\"" + MPT_TEAMLISTB + "\":[" + jsonTeamListB + "],";
                    jsontext += "\"" + MPT_RANDOMSEED + "\":" + randomSeed + ",";
                }

                //if (gameplayEndRace)
                //    jsontext += "\"" + MPT_GAMEPLAYENDRACE + "\":true,";
                //else
                //    jsontext += "\"" + MPT_GAMEPLAYENDRACE + "\":false,";

                //if (gameState >= GameState.TeamValidated && gameState < GameState.LevelLoaded)
                //    jsontext += "\"" + MPT_STARTCOUNTDOWN + "\":" + startcountdown.ToString("F3") + ",";
                //jsontext += "\"" + MPT_REMAININGTIME + "\":" + gameRemainingTime.ToString("F3") + ",";

                if (_towerDefEvents.Count > 0)
                {
                    string eventMsg = "";
                    foreach (var data in _towerDefEvents)
                    {
                        eventMsg += data.state + "_" + data.numWave + "_" + data.relWave + "_";
                    }
                    eventMsg = eventMsg.Remove(eventMsg.Length - 1);
                    jsontext += "\"" + MPT_TOWERDEFEVENTS + "\":\"" + eventMsg + "\",";
                    Debug.Log("[MPT_TOWERDEFEVENTS] send " + eventMsg);
                    _towerDefEvents.Clear();
                }
            }

            if (gameplayEndRace || _needToSendStats)
            {
                _needToSendStats = false;
                Dictionary<string,double> stats = RRStats.RRStatsManager.instance.GetStats(myStatsName);
                if (stats != null && stats.Count > 0)
                    jsontext += "\"" + MPT_STATS + "\":" + MakeJsonFromDictionnary(stats) + ",";
            }
                
            if (Player.myplayer != null && gameState < GameState.TowerDef)
            {
                jsontext += MakeAttachJsonLine(MPT_HANDLEFTITEM, Player.myplayer.left_hand_objects);
                jsontext += MakeAttachJsonLine(MPT_HANDRIGHTITEM, Player.myplayer.right_hand_objects);
                //jsontext += MakeAttachJsonLine(MPT_BELTLEFTITEM, Player.myplayer.belt_left_objects);
                //jsontext += MakeAttachJsonLine(MPT_BELTRIGHTITEM, Player.myplayer.belt_right_objects);
                //jsontext += MakeAttachJsonLine(MPT_BELTFRONTITEM, Player.myplayer.belt_front_objects);
                //jsontext += MakeAttachJsonLine(MPT_BELTBACKITEM, Player.myplayer.belt_back_objects);
                jsontext += "\"" + MPT_TIME + "\":" + Mathf.FloorToInt(Time.time) + ",";
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

                    if (received[MPT_MYTEAM] != null)
                    {
                        _myteam = (long)received[MPT_MYTEAM];
                    }

                    if (received[MPT_GAMEPLAYRUNNING] != null)
                    {
                        //if (gameplayrunning == false)
                        //{
                        //    gameplayrunning = (bool)received[MPT_GAMEPLAYRUNNING];
                        //}
                    }
                    if (received[MPT_GAMEPLAYENDRACE] != null)
                    {
                        //if (gameplayEndRace == false)
                        //{
                        //    gameplayEndRace = (bool)received[MPT_GAMEPLAYENDRACE];
                        //}
                    }
                    if (received[MPT_NETID] != null)
                    {
                        _netid = (long)received[MPT_NETID];
                        _actorNum = (int)_netid - 1;
                        allteams[actorNum] = (int)_myteam;

                        if (received[MPT_HANDLEFTITEM] != null)
                        {
                            Player_avatar[] allplayerstotest = Player.myplayer.avatars;
                            foreach (Player_avatar pl in allplayerstotest)
                            {
                                if (pl.actornumber == actorNum)
                                {
                                    if (pl.left_hand_objects.Count == 0)
                                        pl.SetMyAttachedObjects();
                                    VerifyJsonOnAttached(received, MPT_HANDLEFTITEM, pl.left_hand_objects);
                                    VerifyJsonOnAttached(received, MPT_HANDRIGHTITEM, pl.right_hand_objects);
                                    //VerifyJsonOnAttached(received, MPT_BELTLEFTITEM, pl.belt_left_objects);
                                    //VerifyJsonOnAttached(received, MPT_BELTRIGHTITEM, pl.belt_right_objects);
                                    //VerifyJsonOnAttached(received, MPT_BELTFRONTITEM, pl.belt_front_objects);
                                    //VerifyJsonOnAttached(received, MPT_BELTBACKITEM, pl.belt_back_objects);
                                }
                            }
                        }
                    }

                    if (received[MPT_PLAYERNAME] != null)
                    {
                        string name = (string)received[MPT_PLAYERNAME];
                        _localName = name;
                        _statsName = $"{name}_{actorNum}";
                        piratenames[actorNum] = name;
                        pirateStatsNames[actorNum] = _statsName;
                    }

                    if (received[MPT_SKIN] != null)
                    {
                        pirateskins[actorNum] = (string)received[MPT_SKIN];
                        Player_avatar.SetSkin(actorNum, pirateskins[actorNum]);
                    }

                    if (received[MPT_GAMESTATE] != null)
                    {
                        gameState = (GameState)((long)received[MPT_GAMESTATE]);
                        if (PhotonNetwork.IsMasterClient && myself.gameState != GameState.ForceTeamValidation)
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
                        // Check avatar visibility
                        if (gameState == GameState.NameValidated || gameState == GameState.ChooseLevel)
						{
                            foreach (Player_avatar pl in Player.myplayer.avatars)
                            {
                                if (pl.actornumber > 0)
                                {
                                    bool visible = IsNumActorVisible(pl.actornumber);
                                    pl.gameObject.SetActive(visible);
                                }
                            }
                        }

                        if (InitGameData.instance != null && gameState == GameState.StopWithFade)
                        {
                            myself.gameState = GameState.StopWithFade;
                        }
                    }

                    if (received[MPT_PLAYERSTATE] != null)
                    {
                        PlayerState oldPlayerState = _playerState;
                        _playerState = (PlayerState)((long)received[MPT_PLAYERSTATE]);
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

                    if (received[MPT_TOWERDEFEVENTS] != null)
                    {
                        if (TowerDefManager.myself != null)
                        {
                            string valString = ((string)received[MPT_TOWERDEFEVENTS]);
                            Debug.Log("[MPT_TOWERDEFEVENTS] receive " + valString);
                            string[] split = valString.Split('_');
                            int messageCount = split.Length / 3;
                            for (int i = 0; i < messageCount; ++i)
                            {
                                if (System.Enum.TryParse(split[i*3], out TowerDefManager.TowerDefState state))
                                {
                                    string msgData = split[i * 3 + 1];
                                    if (int.TryParse(msgData, out int numWave))
                                    {
                                        string msgData2 = split[i * 3 + 2];
                                        if (int.TryParse(msgData2, out int relativeWave))
                                        {
                                            TowerDefManager.myself.SetStateEvent(state, numWave, relativeWave);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (received[MPT_STARTCOUNTDOWN] != null)
                    {
                        startcountdown = Helper.GetDictionaryValue(received, MPT_STARTCOUNTDOWN);
                    }
                    if (received[MPT_REMAININGTIME] != null)
                    {
                        float _remainTime = Helper.GetDictionaryValue(received, MPT_REMAININGTIME);
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
                                    //foreach (boat_followdummy bfd in allBoats)
                                    //{
                                    //    bfd.boatEndlessUI.gameObject.SetActive(true);
                                    //    bfd.boatEndlessUI.StartTimer((int)gameRemainingTime);
                                    //    bfd.boatEndlessUI.SetDistanceMax(_pathSplineDistance);
                                    //}
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
                    if (received[MPT_STATS] != null)
                    {
                        Dictionary<string, object> recestats = (Dictionary<string, object>)received[MPT_STATS];
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
                        if (received[MPT_TEAMLISTA] != null)
                        {
                            List<object> ll = (List<object>)received[MPT_TEAMLISTA];
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
                        if (received[MPT_TEAMLISTB] != null)
                        {
                            List<object> ll = (List<object>)received[MPT_TEAMLISTB];
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
                    if (received[MPT_RANDOMSEED] != null)
                    {
                        randomSeed = Helper.GetDictionaryInt(received, MPT_RANDOMSEED);
                    }
                    if (received[MPT_LEVEL] != null)
                    {
                        levelToLaunch = (string)received[MPT_LEVEL];
                    }

                    if (received[MPT_HEALTHS] != null)
                    {
                        string valString = ((string)received[MPT_HEALTHS]);
                        ReceiveHealthsData(valString);
                    }

                    if (received[MPT_PLAYER_EVENTS] != null)
                    {
                        string valString = ((string)received[MPT_PLAYER_EVENTS]);
                        string[] split = valString.Split('_');
                        string[] splitData;
                        int dataCount = split.Length / 3;
                        for (int i = 0; i < dataCount; ++i)
                        {
                            if (System.Enum.TryParse(split[i * 3], out PlayerEvent playerEvent))
                            {
                                if (int.TryParse(split[i * 3 + 1], out int playerId))
                                {
                                    string playerEventData = split[i * 3 + 2];
                                    switch (playerEvent)
									{
                                        case PlayerEvent.RecoverAntenna:
                                            splitData = playerEventData.Split('-');
                                            if (splitData.Length == 2)
                                            {
                                                bool activate = splitData[0] == "ON";
                                                if (int.TryParse(splitData[1], out int numAntenna))
                                                {
                                                    TowerDefManager.myself.UpdateRecoveryAntennaByPlayer(numAntenna, playerId, activate);
                                                }
                                                else
                                                {
                                                    Debug.Log($"NUM ANTENNA NOT FOUND - {splitData[1]}");
                                                }
                                            }
                                            else
                                            {
                                                Debug.Log($"SPLIT DATA NOT CORRECT - {playerEventData}");
                                            }
                                            break;
                                        case PlayerEvent.PlayerHealth:
                                            if (float.TryParse(playerEventData, out float playerHealth))
                                            {
                                                Player_avatar avatar = Player.myplayer.GetAvatar(playerId);
                                                if (avatar != null)
												{
                                                    avatar.SetHealth(playerHealth);
                                                    piratehealths[playerId] = playerHealth;
                                                    piratedeaths[playerId] = playerHealth <= 0f;
                                                }
                                                else
												{
                                                    Debug.Log($"AVATAR NOT FOUND - {playerId}");
                                                }
                                            }
                                            else
                                            {
                                                Debug.Log($"PLAYER HEALTH NOT CORRECT - {playerEventData}");
                                            }
                                            break;
                                        case PlayerEvent.PlayerDamage:
                                            splitData = playerEventData.Split('-');
                                            if (splitData.Length == 2)
                                            {
                                                if (int.TryParse(splitData[0], out int otherPlayerId))
                                                {
                                                    if (float.TryParse(splitData[1], out float damage))
                                                    {
                                                        if (otherPlayerId == myId)
														{
                                                            Player.myplayer.playerBody.AddDamage(damage, playerId);
														}
                                                    }
                                                    else
                                                    {
                                                        Debug.Log($"DAMMAGE NOT FOUND - {splitData[1]}");
                                                    }
                                                }
                                                else
                                                {
                                                    Debug.Log($"OTHER PLAYER ID NOT FOUND - {splitData[0]}");
                                                }
                                            }
                                            else
                                            {
                                                Debug.Log($"SPLIT DATA NOT CORRECT - {playerEventData}");
                                            }
                                            break;
									}
                                }
                                else
                                {
                                    Debug.Log($"PLAYER ID NOT FOUND  - {valString}");
                                }
                            }
                            else
                            {
                                Debug.Log($"PLAYER EVENT NOT FOUND  - {valString}");
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
                    UI_ChooseYourName chooseYourName = StarterHub.myself.GetComponentInChildren<UI_ChooseYourName>(true);
                    if (chooseYourName != null && chooseYourName.isNameConfirmed)
                    {
                        gameState = GameState.NameValidated;
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
                    avatar_root.AttachPlayersToBase();
                    apicalls.myself?.StartGameCounter();
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
                            launcher.loadSceneMode = LoadSceneMode.Single;
                            launcher.fadeOutDuration = 0.5f;
                            launcher.waitToActivateScene = false;
                            launcher.LaunchScene();
							gameState = GameState.GoToLobby;
						}
                    }
                    else if (_aiBot != null && _aiBot.botState == AIBot.BotState.LevelChoosen)
                    {
                        gameState = GameState.GoToLobby;
                    }
                }
            }
            else if (gameState == GameState.GoToLobby)
            {
                if (_resetDone)
                {
                    SetMyTeam(0);
                    SetMyValidated(true);
                    _isInTutorial = true;
                    _resetDone = false;
                }
            }
            else if (gameState == GameState.LevelLoaded)
            {
                levelToLaunch = null;
            }
            else if (gameState == GameState.EndGame)
            {
                if (levelToLaunch != null && (UI_EndRaceResult.myself != null || _aiBot != null))
                {
#if DEBUG_STATES
                    Debug.Log("[STATES] QuitRace with levelToLaunch " + levelToLaunch);
#endif
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
               if (gameplayrunning && teamValidated)
                {
                    if (!gamestartload)
                    {
                        gamestartload = true;
                        StartCoroutine(LoadMyScene());
                    }
                }

                if (!gameplayrunning && AreAllFlowsAtState(GameState.TeamSelected, true))
                {
                    areAllTeamsSelected = true;
                }
            }

            if (aiBot == null && gameState != GameState.WaitLoadScene && !gameplayrunning && AreAllFlowsAtState(GameState.TeamValidated, true))
            {
                if (startcountdown == -1.0f)
                {
                    startcountdown = 2f;
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
            else if (gameState == GameState.LevelLoaded)
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
                if (gameplayrunning && teamValidated)
                {
                    if (!gamestartload)
                    {
                        gamestartload = true;
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

    public static void TriggerTowerDefEvent(TowerDefManager.TowerDefState state, int numWave, int relativeWave)
    {
        bool sendEvent = !amIAlone;
#if DEBUG_PHOTON_DATA
        sendEvent = true;
#endif
        if (sendEvent)
        {
            TowerDefEventData data = new TowerDefEventData();
            data.state = state;
            data.numWave = numWave;
            data.relWave = relativeWave;
            _towerDefEvents.Add(data);
        }
    }

    public static void TriggerPlayerEvent(PlayerEvent playerEvent, object data)
    {
        bool sendEvent = !amIAlone;
#if DEBUG_PHOTON_DATA
        sendEvent = true;
#endif
        if (sendEvent)
        {
            PlayerEventData eventData = new PlayerEventData();
            eventData.playerEvent = playerEvent;
            eventData.playerId = myId;
            eventData.data = data;
            _playerEvents.Add(eventData);
        }
    }

    //private void OnGUI()
    //{
    //       GUILayout.TextArea("Player gold: " + Player.myplayer.teamGold);
    //       GUILayout.TextArea("Player score: " + Player.myplayer.score);
    //   }

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

        foreach (GameflowBOD flow in allFlows)
        {
            if (flow.gameState < state)
                return false;
        }
        return true;
    }

    public static void SetMyValidated(bool set, bool freeze = false)
    {
        if (gameplayrunning)
            return;
#if DEBUG_STATES
        Debug.Log("[STATES] SetMyValidated " + set);
#endif
        if (freeze)
            myself.FreezeTeams();
        myself.gameState = set ? GameState.TeamValidated : GameState.TeamSelected;
        if (StarterHub.myself != null && set)
		{
            StarterHub.myself.TriggerDeactivateAtTeamValidation();
        }
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

        while (TowerDefManager.myself == null && _aiBot == null)
            yield return null;

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
        }
        else
        {
            gamedata = TowerDefManager.myself.worldRoot;
            gamedata.SetActive(true);

            allFlows = FindObjectsOfType<GameflowBOD>();

            if (TowerDefManager.myself.gamemusic != null)
                TowerDefManager.myself.gamemusic.Play();

            yield return null;
        }

		yield return null;

        if (_aiBot != null)
        {
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

            avatar_root.AttachPlayersToBase();

            TowerDefManager.myself.TeleportPlayerToStartSpawnpoint();
        }

        gameState = GameState.LevelLoaded;
        currentLevelName = levelToLaunch;

        // wait here 
        while (!AreAllFlowsAtState(gameState))
            yield return null;

        areAllRacesLoaded = true;
#if DEBUG_STATES
        Debug.Log($"[STATES] areAllRacesLoaded reached!");
#endif
        GameObject.DontDestroyOnLoad(RRStats.RRStatsManager.instance);
        ResetStats();

        gamesettings_general.myself.FadeInMusicVolume();

        if (!Player.myplayer.isInPause)
        {
            gamesettings_screen gsScreen = gamesettings_screen.myself;
            gsScreen.FadeIn();
            while (gsScreen.faderunning)
                yield return null;
        }

        if (_aiBot != null)
        {
            while (_aiBot.botState != AIBot.BotState.LoadRace)
                yield return null;
        }

        gameState = GameState.LevelStarted;

        while (!AreAllFlowsAtState(gameState))
            yield return null;

        areAllRacesStarted = true;
#if DEBUG_STATES
        Debug.Log($"[STATES] areAllRacesStarted reached!");
#endif

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
        }

        yield return new WaitForSeconds(1f);

        while (!AreAllFlowsAtState(GameState.TowerDef))
            yield return null;

#if DEBUG_STATES
        Debug.Log($"[STATES] all TowerDef reached!");
#endif
    }

    public IEnumerator EndTimeReached()
    {
        gameState = GameState.EndGame;

        // Check if all stats are here
        if (!AreAllStatsReceived())
            yield return null;

        if (_aiBot != null)
            yield break;

        GameObject Treasure = null;
        bool isResultWithOneTeam = false;
        
        // set end race
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

    public override void SetTowerDefReady()
    {
        gameState = GameState.TowerDef;
    }

    public override bool AreAllTowerDefReady()
    {
        return AreAllFlowsAtState(GameState.TowerDef);
    }

    public override void ResetStats()
	{
        RRStats.RRStatsManager stats = RRStats.RRStatsManager.instance;
        stats.ResetStats();
        stats.SetStat(_statsName, gamesettings.STAT_POINTS, 0);
        stats.SetStat(_statsName, gamesettings.STAT_FIRE, 0);
        stats.SetStat(_statsName, gamesettings.STAT_TOUCH, 0);
        stats.SetStat(_statsName, gamesettings.STAT_DEATH, 0);
        stats.SetStat(_statsName, gamesettings.STAT_KILLS, 0);
        stats.SetStat(_statsName, gamesettings.STAT_CHESTS, 0);
        stats.SetStat(_statsName, gamesettings.STAT_TURRETS, 0);
        stats.SetStat(_statsName, gamesettings.STAT_BIRDS, 0);
        stats.SetStat(_statsName, gamesettings.STAT_HEARTS, 0);
        stats.SetStat(_statsName, gamesettings.STAT_SHIELDS, 0);
        stats.SetStat(_statsName, gamesettings.STAT_SPLINES, 0);
        stats.SetStat(_statsName, gamesettings.STAT_MERMAIDS, 0);
        stats.SetStat(_statsName, gamesettings.STAT_MONSTERS, 0);
        stats.SetStat(_statsName, gamesettings.STAT_SKULLS, 0);
        stats.SetStat(_statsName, gamesettings.STAT_ARCHIVES, 0);
        stats.SetStat(_statsName, gamesettings.STAT_SCIENTISTS, 0);
        stats.SetStat(_statsName, Health.HealtObjectType.drone.ToString(), 0);
        stats.SetStat(_statsName, Health.HealtObjectType.mine.ToString(), 0);
        stats.SetStat(_statsName, Health.HealtObjectType.superDrone.ToString(), 0);
        stats.SetStat(_statsName, Health.HealtObjectType.megaDroid.ToString(), 0);
        stats.SetStat(_statsName, Health.HealtObjectType.plasmaBomb.ToString(), 0);
        stats.SetStat(_statsName, Health.HealtObjectType.bomber.ToString(), 0);
        stats.SetStat(_statsName, Health.HealtObjectType.conveyor.ToString(), 0);
        stats.SetStat(_statsName, Health.HealtObjectType.droneUltra.ToString(), 0);
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
        if (level == gamesettings_general.myself.levelStart)
        {
            LaunchSceneASync launcher = LaunchSceneASync.myself;
            if (launcher != null)
            {
                if (launcher.isLoadingScene && launcher.waitToActivateScene)
                    launcher.ActivateScene();
                while (launcher.isLoadingScene)
                    yield return null;
                launcher.sceneName = level;
                launcher.loadSceneMode = UnityEngine.SceneManagement.LoadSceneMode.Single;
                launcher.fadeOutDuration = 0.5f;
                launcher.waitToActivateScene = false;
                launcher.LaunchScene();
            }
        }
        else
        {
            _isQuittingRace = true;
            gameplayEndRace = true;
            gameState = GameState.EndGame;
            //while (_endRaceScreen == null)
            //    yield return null;
            //_endRaceScreen.playCaptainAudioAtInitScreen = false;
            levelToLaunch = level;
            _isQuittingRace = false;
            gameState = GameState.QuitRace;
        }
    }

	public override void SendPlayerHealth()
	{
        TriggerPlayerEvent(PlayerEvent.PlayerHealth, Player.myplayer.health.ToString("F3"));
	}

    public override void SendDamageOnOtherPlayer(int playerId, float damage)
    {
        TriggerPlayerEvent(PlayerEvent.PlayerDamage, playerId.ToString() + "-" + damage.ToString("F3"));
    }
}
