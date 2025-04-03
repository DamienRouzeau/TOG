//#define DEBUG_PHOTON_DATA
#define DEBUG_STATES
//#define USE_SLOTS

using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using UnityEngine;

public class GameflowBase : MonoBehaviour
{
    #region Delegates

    public delegate void OnRaceEventDelegate(int team, bool isMe, string statId, object param = null);
    public delegate void OnHealthEventDelegate(long healthId, float value);

    #endregion


    #region Enums

    public enum PlayerState
	{
        Ingame,
        InPause,
        Disconnected
	}

    #endregion

    public bool ownedobject = false;        // to test if the running object is owned
    public AIBot aiBot => _aiBot;
    public int actorNum => _actorNum;
    public string localName => _localName;
    public string statsName => _statsName;
    public UI_EndRaceResult endRaceScreen => _endRaceScreen;
    public bool pausedplayer
	{
        get { return _playerState == PlayerState.InPause; }
        set
		{
            if (_playerState != PlayerState.Disconnected)
            {
                PlayerState state = value ? PlayerState.InPause : PlayerState.Ingame;
                if (_playerState != state)
                {
                    _playerState = state;
                    Player.myplayer.SetInPause(value);
                }
            }
        }
	}

    protected string _statsName = null;

    public virtual bool canPlayerShoot => true;
    public virtual bool nameValidated => false;
    public static bool isInSpectatorView = false;
    public static bool isInNoVR = false;
    public static bool areAllRacesStarted = false;
#if USE_KDK
    public static int nrplayersperteam = 16;
    public static int nrteam = 1;
#elif USE_BOD
    public static int nrplayersperteam = 8;
    public static int nrteam = 1;
#else
    public static int nrplayersperteam = 10;
    public static int nrteam = 2;
#endif
    public static int nrplayersmax = nrplayersperteam * nrteam;
    public static GameflowBase instance = null;
    public static Dictionary<int, int> allteams = new Dictionary<int, int>();
    public static int randomSeed = 0;
    public static bool amIAlone => allFlows == null || allFlows.Length == 1;
    public static int playerCount => allFlows != null ? allFlows.Length : 1;
    public static GameflowBase[] allFlows = null;
    public static bool isBotScene = false;
    public static bool simulateLocalBots = false;
    public static int localBotCount = 15;
    public static bool localBotsOnSameBoat = false;
    public static bool gameplayrunning = false;
    public static int myTeam = 0;
    public static int myId = 0;
    public static string[] piratenames = new string[nrplayersmax];
    public static string[] pirateskins = new string[nrplayersmax];
    public static string[] piratesCustomHats = new string[nrplayersmax];
    public static string[] pirateStatsNames = new string[nrplayersmax];
    public static bool[] piratedeaths = new bool[nrplayersmax];
    public static float[] piratehealths = new float[nrplayersmax];
    public static long[] teamlistA = new long[nrplayersperteam];
    public static long[] teamlistB = new long[nrplayersperteam];
#if USE_SLOTS
    public static int mySlot = 0;
    public static int[] playerSlots = new int[nrplayersmax];
#endif

    public static OnRaceEventDelegate onRaceEventDelegate = null;
    public static OnHealthEventDelegate onHealthEventDelegate = null;

    public static string myPirateName
    {
        get => _myPirateName;
        set
        {
            _myPirateName = value;
            if (instance != null)
			{
                instance.SetPlayerId(myId);
                instance.SetLocalName(value);
            }
            piratenames[myId] = _myPirateName;
            pirateStatsNames[myId] = $"{_myPirateName}_{myId}";
        }
    }
    private static string _myPirateName = null;

    public static string myStatsName => pirateStatsNames[myId];

    protected static List<Health.HealthById> _healthEvents = new List<Health.HealthById>();
    protected static bool _freezeTeams = false;    

    protected UI_EndRaceResult _endRaceScreen = null;
    protected bool _isInTutorial = false;
    protected PlayerState _playerState = PlayerState.Ingame;
    protected string _localName = null;
    protected int _actorNum = 0;
    protected PhotonTransformGameView _trans = null;
    protected PhotonView _photonView = null;
    
    protected AIBot _aiBot = null;
    protected bool _isLocalBot = false;

    protected float _healthvalue = -1.0f;

    protected virtual void Awake()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        DontDestroyOnLoad(gameObject);

        _trans = gameObject.GetComponent<PhotonTransformGameView>();
        _trans.onSendingDataCbk += OnSendingData;
        _trans.onReceivingDataCbk += OnReceivingData;

        _photonView = gameObject.GetAddComponent<PhotonView>();
    }

    protected virtual void Start()
    {
        if (ownedobject)
        {
            myId = PhotonNetworkController.GetPlayerId();
#if USE_SLOTS
            int slot = GetSlotFromId(myId);
            if (slot == -1)
			{
                slot = GetFirstFreeSlot();
                playerSlots[slot] = myId;
            }
#endif
            LaunchSceneASync.onLoadSceneCallback += OnLoadScene;
        }
    }
 #if USE_SLOTS
    private int GetSlotFromId(int id)
	{
        for (int i = 0; i < playerSlots.Length; ++i)
            if (playerSlots[i] == id)
                return i;
        return -1;
	}

    private int GetFirstFreeSlot()
    {
        for (int i = 0; i < playerSlots.Length; ++i)
            if (playerSlots[i] == -1)
                return i;
        return -1;
    }
#endif
    protected virtual void OnDestroy()
    {
        _photonView = null;
        _endRaceScreen = null;

        if (_trans != null)
        {
            _trans.onSendingDataCbk -= OnSendingData;
            _trans.onReceivingDataCbk -= OnReceivingData;
            _trans = null;
        }

        if (ownedobject)
        {
            Debug.Log("[STATES] GameflowBase destroyed!");
            LaunchSceneASync.onLoadSceneCallback -= OnLoadScene;
            if (instance == this)
                instance = null;
        }
    }

    public static void CheckRandomSeed()
    {
        if (randomSeed == 0)
            randomSeed = UnityEngine.Random.Range(1, 1000000);
    }

    protected virtual void OnTeamSelected(int team)
	{
	}

    public virtual void ResetStats()
	{
	}

    public virtual void Reset(bool resetToCabin)
	{
#if DEBUG_STATES
        Debug.Log($"[STATE] - Reset resetToCabin {resetToCabin} ownedobject {ownedobject}");
#endif

        if (ownedobject)
        {
            allteams.Clear();
            
            _isInTutorial = false;
            CheckRandomSeed();
            if (resetToCabin)
            {
                allFlows = null;
            }

            for (int i = 0; i < nrplayersperteam; i++)
            {
                SetInTeamA(i, -1);
                SetInTeamB(i, -1);
            }

            // Reset voice over max priority
            voicepriority.playinprio = 0;
            //Debug.Log($"[DEBUG_VOICEOVER] Reset on loadscene current prio {voicepriority.playinprio}");
        }
    }

    public void SetPlayerId(int id)
	{
        _actorNum = id;
	}

    public void SetLocalName(string name)
	{
        _localName = name;
        _statsName = $"{_localName}_{_actorNum}";
    }

    public virtual bool AreAvatarsVisibles()
	{
        return true;
    }

    private void OnLoadScene(string prevScene, string newScene)
	{
#if DEBUG_STATES
        Debug.Log($"[STATES] OnLoadScene { prevScene} -> { newScene}");
#endif
        // Return to cabin
        if (newScene == gamesettings_general.myself.levelStart)
		{
            OnSceneReturnToCabin();
        }
        // go or return to lobby
        else if (gamesettings_general.myself.levelSettings.GetLevelFromSceneName(newScene) != null || newScene.ToLower().Contains("starterhub"))
		{
            OnSceneGoToLobby();
        }
    }

    public void OnSceneReturnToCabin()
	{
        Player.myplayer?.Reset();
        pausedplayer = false;
        if (!PhotonNetworkController.soloMode)
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.Disconnect();
        }
        Destroy(PhotonNetworkController.myself.gameObject);
        Destroy(poolhelper.myself.gameObject);
        if (apicalls.myself != null)
            Destroy(apicalls.myself.gameObject);
        if (pointfromhand.teleporttarget != null)
            Destroy(pointfromhand.teleporttarget);
        Reset(true);
        Destroy(gameObject);
    }

    public virtual void OnSceneGoToLobby()
	{
        if (pointfromhand.teleporttarget != null)
            Destroy(pointfromhand.teleporttarget);
        Player.myplayer?.PrepareToLobby();
        Reset(false);
    }

    public static void IncrementPlayerStat(string statId, double increment)
    {
        if (!string.IsNullOrEmpty(instance.statsName))
            RRStats.RRStatsManager.instance.IncrementStat(instance.statsName, statId, increment);
    }

    public static float GetPlayerAccuracy(string statTouchId, string statFireId)
    {
        if (instance != null && !string.IsNullOrEmpty(instance.statsName))
        {
            float touch = (float)RRStats.RRStatsManager.instance.GetStat(instance.statsName, statTouchId);
            float fire = (float)RRStats.RRStatsManager.instance.GetStat(instance.statsName, statFireId);
            if (fire > 0)
                return touch / fire;
            return 1f;
        }
        return 1f;
    }

    public static float GetPlayerStat(string statId)
    {
        if (instance != null && !string.IsNullOrEmpty(instance.statsName))
        {
            return (float)RRStats.RRStatsManager.instance.GetStat(instance.statsName, statId);
        }
        return 0f;
    }

    public static void SetInTeamA(int idx, long team)
	{
        //if (teamlistA[idx] != team)
        //    Debug.Log($"Raph - SetInTeamA {idx} {team}");
        teamlistA[idx] = team;
	}

    public static void SetInTeamB(int idx, long team)
    {
        //if (teamlistB[idx] != team)
        //    Debug.Log($"Raph - SetInTeamB {idx} {team}");
        teamlistB[idx] = team;
    }

    public static void SetTeamTables(bool attach,long _myteam,long playerId)
    {
        if (gameplayrunning)
            return;
        //Debug.Log($"[INIT_DATA] SetTeamTables {attach} {_myteam} {playerId}");
        if (attach)
        {
            for (int rT = 0; rT < nrplayersperteam; rT++)
            {
                if ((_myteam == 0) && (teamlistA[rT] == playerId)) return;
                if ((_myteam == 1) && (teamlistB[rT] == playerId)) return;
                if ((_myteam == 1) && (teamlistA[rT] == playerId)) teamlistA[rT] = -1;
                if ((_myteam == 0) && (teamlistB[rT] == playerId)) teamlistB[rT] = -1;
            }
            for (int rT = 0; rT < nrplayersperteam; rT++)
            {
                if ((_myteam == 0) && (teamlistA[rT] == -1))
                {
                    SetInTeamA(rT, playerId);
                    allteams[(int)playerId] = (int)_myteam;
                    break;
                }
                if ((_myteam == 1) && (teamlistB[rT] == -1))
                {
                    SetInTeamB(rT, playerId);
                    allteams[(int)playerId] = (int)_myteam;
                    break;
                }
            }
        }
        else
        {
            // clean when not longer valid
            for (int rT = 0; rT < nrplayersperteam; rT++)
            {
                if (teamlistA[rT] == playerId)
                {
                    Debug.Log($"SetTeamTables remove id {playerId} from team A!");
                    SetInTeamA(rT, -1);
                }
                if (teamlistB[rT] == playerId)
                {
                    Debug.Log($"SetTeamTables remove id {playerId} from team B!");
                    SetInTeamB(rT, -1);
                }
            }
            allteams[(int)playerId] = -1;
        }
        // NOW clean teamlistA and teamlistB
        for (int rT = 0; rT < nrplayersperteam - 1; rT++)
        {
            if (teamlistA[rT] == -1)
            {
                SetInTeamA(rT, teamlistA[rT + 1]);
                SetInTeamA(rT + 1, -1);
            }
            if (teamlistB[rT] == -1)
            {
                SetInTeamB(rT, teamlistB[rT + 1]);
                SetInTeamB(rT + 1, -1);
            }
        }
    }

    protected spawnpoint GetSpawnpointOnBoat(GameObject root, int actorNum, int team)
    {
        int searchspawn = GetActorIndexInTeam(actorNum, team);
#if DEBUG_STATES
        Debug.Log($"[STATES] GetActorIndexInTeam: " + searchspawn);
#endif
        int nrspawn = 0;
        spawnpoint[] allspawn = root.GetComponentsInChildren<spawnpoint>();

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

    public virtual string OnSendingData()
    {
        return null;
    }

    public virtual void OnReceivingData(string data, bool forceReceive = false)
    {
    }

    protected virtual void Update()
    {
        if (ownedobject)         // Only from our player
        {
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
           
        }
    }

    public static string GetMySkinName()
    {
        if (PhotonNetworkController.soloMode)
            return pirateskins[0];
        if (PhotonNetworkController.myself.ready)
            return pirateskins[myId];
        else
            return null;
    }

    public static void SetMySkin(string skinname)
    {
        if (PhotonNetworkController.soloMode)
            pirateskins[0] = skinname;
        if (PhotonNetworkController.myself.ready)
        {
            pirateskins[myId] = skinname;
        }
    }

    public static void SetMyCustomHat(string customHat)
    {
        if (PhotonNetworkController.soloMode)
            piratesCustomHats[0] = customHat;
        if (PhotonNetworkController.myself.ready)
        {
            piratesCustomHats[myId] = customHat;
        }
    }

    public static string GetMyCustomHat()
    {
        if (PhotonNetworkController.soloMode)
            return piratesCustomHats[0];
        if (PhotonNetworkController.myself.ready)
            return piratesCustomHats[myId];
        else
            return null;
    }

    public static bool IsActorInTeam(int numActor, long[] teamList)
	{
        int listCount = teamList.Length;
        for (int i = 0; i < listCount; ++i)
		{
            if (teamList[i] == numActor)
                return true;
		}
        return false;
	}

    public static int GetActorCountInTeam(int team)
    {
        return GetActorCountInTeam(team == 0 ? teamlistA : teamlistB);
    }

    public static int GetActorCountInTeam(long[] teamList)
    {
        int actorCount = 0;
        int listCount = teamList.Length;
        for (int i = 0; i < listCount; ++i)
        {
            if (teamList[i] >= 0)
                actorCount++;
        }
        return actorCount;
    }

    public static int GetActorIndexInTeam(int numActor, int team)
    {
        return GetActorIndexInTeam(numActor, team == 0 ? teamlistA : teamlistB);
    }

    public static int GetActorIndexInTeam(int numActor, long[] teamList)
    {
        int listCount = teamList.Length;
        for (int i = 0; i < listCount; ++i)
        {
            if (teamList[i] == numActor)
                return i;
        }
        return -1;
    }

    public void SetAIBot(AIBot bot, string name, int num, bool local)
	{
        _aiBot = bot;
        SetLocalName(name);
        _actorNum = num;
        _isLocalBot = local;
	}

    protected string MakeJsonFromDictionnary(Dictionary<string, double> dico )
    {
        string s = "{";
        bool first = true;
        foreach( KeyValuePair<string, double> pair in dico )
        {
            if (!first) s += ",";
            first = false;
            s += "\"" + pair.Key + "\":" + pair.Value;
        }
        s += "}";
        return s;
    }

    public static void TriggerRaceEvent(int team, bool isMe, string statId, object param = null)
    {
        //Debug.Log($"TriggerRaceEvent {team} {isMe} {statId} {param}");
        if (onRaceEventDelegate != null)
            onRaceEventDelegate(team, isMe, statId, param);
    }

    public static void TriggerHealthEvent(long healthId, int playerId, float value)
    {
        onHealthEventDelegate?.Invoke(healthId, value);
        bool prepareHealthEvents = !amIAlone;
#if UNITY_EDITOR && DEBUG_PHOTON_DATA
        prepareHealthEvents = true;
#endif
        if (prepareHealthEvents)
        {
            Health.HealthById data = new Health.HealthById();
            data.healthId = healthId;
            data.playerId = playerId;
            data.value = value;
            _healthEvents.Add(data);
        }
    }

    public static void SearchAllFlows()
	{
        allFlows = GameObject.FindObjectsOfType<GameflowBase>();
    }

    public static void SetMyTeam(int team, bool freeze = false)
    {
        if (gameplayrunning)
            return;
#if DEBUG_STATES
        Debug.Log("[STATES] SetMyTeam " + team);
#endif
        if (freeze)
            instance.FreezeTeams();
        SetTeamTables(true, team, myId);
        myTeam = team;
        instance.OnTeamSelected(team);
    }

    public void FreezeTeams()
	{
        _freezeTeams = true;
        StartCoroutine(FreezeTeamsEnum(1f));
    }

    private IEnumerator FreezeTeamsEnum(float duration)
    {
        yield return new WaitForSeconds(duration);
        _freezeTeams = false;
    }

    public bool AreAllStatsReceived()
    {
        return AreAllStatsReceivedFromTeam(teamlistA) && AreAllStatsReceivedFromTeam(teamlistB);
    }

    public bool AreAllStatsReceivedFromTeam(long[] teamList)
    {
        foreach (long idx in teamList)
        {
            if (idx >= 0)
            {
                string name = pirateStatsNames[idx];
                Dictionary<string, double> stats = RRStats.RRStatsManager.instance.GetStats(name);
                if (stats == null) // no stats yet
                {
                    return false;
                }
            }
        }
        return true;
    }

    public virtual void QuitRace(bool killProcess = false)
	{
        if (killProcess)
		{
            Debug.Log("Kill App!");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
	}

    public virtual void SendPlayerHealth()
	{
	}

    public virtual void SendDamageOnOtherPlayer(int playerId, float damage)
    {
    }

    public virtual void SetTowerDefReady()
	{
	}

    public virtual bool AreAllTowerDefReady()
	{
        return false;
	}

    public void SendPlayerIsDead()
    {
        _photonView.RPC("RpcPlayerIsDead", RpcTarget.All, myId);
    }

    [PunRPC]
    protected void RpcPlayerIsDead(int playerId)
    {
        Debug.Log("RpcPlayerIsDead " + playerId);
        DeadPirateApparition(playerId);
    }

    private void DeadPirateApparition(int playerId)
    {
        piratedeaths[playerId] = true;

        GameObject currentskin;
        if (playerId == myId)
            currentskin = Player.myplayer.currentskin;
        else
            currentskin = Player.myplayer.GetAvatar(playerId)?.gameObject;

        Renderer[] allrenddeathanim = currentskin.GetComponentsInChildren<Renderer>();

        GameObject deathanim = null;
        if (playerId != myId)
        {
            deathanim = currentskin.FindInChildren("death");
            Animator anm = deathanim.GetComponent<Animator>();
            deathanim.SetActive(true);
            deathanim.transform.SetParent(currentskin.transform.parent);
            anm.Play("", 0, 0);
        }
        
        foreach (Renderer rend in allrenddeathanim)
            rend.gameObject.SetActive(false);

        StartCoroutine(DeadAnimationTracker(playerId, currentskin, allrenddeathanim, deathanim));
    }

    private IEnumerator DeadAnimationTracker(int playerId, GameObject currentskin, Renderer[] allrenddeathanim, GameObject deathanim)
    {
        yield return new WaitForSeconds(2.0f);
        GameflowBase.piratedeaths[playerId] = false;
        yield return new WaitForSeconds(1.0f);
        
        foreach (Renderer rend in allrenddeathanim)
            rend.gameObject.SetActive(true);

        if (deathanim != null)
        {
            deathanim.SetActive(false);
            deathanim.transform.SetParent(currentskin.transform);
            deathanim.transform.localPosition = Vector3.zero;
            deathanim.transform.localEulerAngles = Vector3.zero;
        }
    }

    protected void ReceiveHealthsData(string data)
	{
        if (!string.IsNullOrEmpty(data))
        {
            string[] split = data.Split('_');
            int dataCount = split.Length / 3;
            for (int i = 0; i < dataCount; ++i)
            {
                if (long.TryParse(split[i * 3], out long id))
                {
                    if (int.TryParse(split[i * 3 + 1], out int playerId))
                    {
                        if (float.TryParse(split[i * 3 + 2], out float val))
                        {
                            Health h = Health.GetHealthFromId(id);
                            if (h != null)
                            {
                                if (h.currentHealth > val || playerId == PhotonNetwork.MasterClient.ActorNumber - 1)
                                    h.ForceCurrentHealth(val, Player.myplayer.GetAvatar(playerId)?.gameObject);
                            }
                            else
                            {
                                Debug.Log($"Health NOT FOUND - {data} - force {val} - from player {playerId} - master {PhotonNetwork.MasterClient.ActorNumber - 1}");
                            }
                        }
                        else
                        {
                            Debug.Log($"VAL NOT FOUND - {data}");
                        }
                    }
                    else
                    {
                        Debug.Log($"PLAYER ID NOT FOUND  - {data}");
                    }
                }
                else
                {
                    Debug.Log($"ID NOT FOUND  - {data}");
                }
            }
        }
    }

    public static GameflowBase GetFlowForNumActor(int num)
	{
        if (allFlows != null)
		{
            int count = playerCount;
            for (int i = 0; i < count; ++i)
			{
                GameflowBase flow = allFlows[i];
                if (flow.actorNum == num)
                    return flow;
            }
		}
        return null;
	}

    public static bool IsNumActorVisible(int num)
    {
        GameflowBase flow = GetFlowForNumActor(num);
        if (flow == null)
            return false;
        return flow.AreAvatarsVisibles();
    }

}
