#define DEBUG_APICALLS

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJSON;
using System.Threading;
using System.Globalization;
#if USE_SYNTHESIS && IS_MASTER
using SynthesisvrArcade;
#endif

public class apicalls : MonoBehaviour
{
    public const string LAST_REGISTERED_END_TIME = "LastRegisteredEndTime_";
    public const string LAST_REGISTERED_END_TIME_VALUE = "LastRegisteredEndTimeValue_";
    public const string LAST_REGISTERED_END_TIME_PAUSE = "LastRegisteredEndTimePause_";
    public const string LAST_REGISTERED_PAUSE = "LastRegisteredPause_";

    public string overridemachineid = "";
    public static string testprefix = "X";
    public static bool isDemoOver => isDemoGame && (isExpiratedDemo || demoGameSessions >= maxdemosessions);
    public static bool isDemoGame = false;
    public static bool isExpiratedDemo = false;
    public static int demoGameSessions = 0;
    public static string id_salle = "";
    public static string last_error_message = "";

    public static string server_url = "https://tests.trailsofgold.fr/api/";
    
    public static string machineid = "";
    public static int instanceOnMachine = 0;

    public static int maxdemosessions = 3;
    public static apicalls myself;
    public static bool LicenseLiveServer = false;    
    public string debugPdfSession = null;
    public string debugCodeAccess = null;
    public bool _debugInLiveServer = false;
    public multiplayerlobby.Product debugProduct = multiplayerlobby.Product.TOG;
    public gamesettings_player gsPlayerTOG = null;
    public gamesettings_player gsPlayerKDK = null;
    public gamesettings_player gsPlayerBOD = null;
    public UI_EndRaceResult endRaceTOG_1 = null;
    public UI_EndRaceResult endRaceTOG_2 = null;
    public UI_EndRaceResult endRaceKDK_1 = null;
    public UI_EndRaceResult endRaceBOD_1 = null;

    bool GameEndIsBusy = false;
    public string savejsonstring = "";

    private bool _isGameStarted = false;

    private void Awake()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        isDemoGame = false;
        isExpiratedDemo = false;
        myself = this;
        DontDestroyOnLoad(gameObject);
        AddInstanceMachineId();
        Debug.Log($"VERSION {AllMyScripts.Common.Version.Version.GetVersion()} - DATE {DateTime.Now}");
    }

    private void OnDestroy()
	{
        RemoveInstanceMachineId();
    }

	public static bool ErrorTestRegister(string text)
    {
        IDictionary res = (IDictionary)Json.Deserialize(text);
        if ((string)res["result"] == "error")
        {
            last_error_message = (string)res["message"];
            switch (last_error_message)
            {
                case "CLIENT_NOT_FOUND":
                    break;
            }
            return true;
        }
        else
        {
            string result = (string)res["result"];
            if (result == "demo")
            {
                isDemoGame = true;
                isExpiratedDemo = false;
                string tmp = (string)res["num_sessions"];
                int.TryParse(tmp, out demoGameSessions);
                Debug.Log("[DEMO] demoGameSessions " + demoGameSessions);

                if (res.Contains("first_date"))
                {
                    tmp = (string)res["first_date"];
                    if (DateTime.TryParse(tmp, out DateTime dateTime))
                    {
                        double hours = (DateTime.Now - dateTime).TotalHours;
                        Debug.Log("[DEMO] hours " + hours);
                        if (hours > 72)
                            isExpiratedDemo = true;
                    }
                }
            }
            else
			{
                isDemoGame = false;
                isExpiratedDemo = false;
            }
            Debug.Log("[DEMO] isDemoGame " + isDemoGame + ", isExpiratedDemo " + isExpiratedDemo);

            multiplayerlobby.myself.UpdateDemoMode();

            if (res.Contains("id_salle"))
            {
                id_salle = (string)res["id_salle"];
                if (string.IsNullOrEmpty(id_salle))
                {
                    last_error_message = "NO_ROOM_FOUND";
                    return true;
                }
#if DEBUG_APICALLS
                if (!string.IsNullOrEmpty(id_salle))
                    Debug.Log("[APICALLS] Found room:" + id_salle);
#endif
            }
        }
        return false;
    }

    public static bool ErrorTest(string text)
    {
        IDictionary res = (IDictionary)Json.Deserialize(text);
        if ((string)res["result"] == "error")
        {
            string message = (string)res["message"];
            switch (message)
            {
                case "CLIENT_NOT_FOUND":
                    break;
            }
            return true;
        }
        return false;
    }

    public static string CreateDate(DateTime dt)
    {
        return (dt.ToString("yyyy-MM-dd\\THH:mm:ss\\Z"));
    }

    IEnumerator Start()
    {
#if USE_LIVE_SERVER && !UNITY_EDITOR
        LicenseLiveServer = true;
#else
        LicenseLiveServer = false;
#endif
        Debug.Log($"[APICALLS] LicenseLiveServer {(LicenseLiveServer ? "Activated!" : "Deactivated!")}");

        UpdateServerURL(true, _debugInLiveServer);

#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(debugPdfSession))
        {
            multiplayerlobby.product = debugProduct;
            UpdateServerURL(false, _debugInLiveServer);
            StartCoroutine(CreateAllHOF(debugPdfSession, true));
            yield break;
        }

        if (!string.IsNullOrEmpty(overridemachineid))
		{
            machineid = overridemachineid;
        }
#endif
        if (string.IsNullOrEmpty(machineid))
        {
            if (GameObject.Find("MasterCanvas") != null)
            {
                machineid = GetLauncherMachineId();
            }
            else if (string.IsNullOrEmpty(overridemachineid))
            {
                machineid = GetInstanceMachineId();
            }
            else
            {
                machineid = overridemachineid;
            }
        }

#if DEBUG_APICALLS
        Debug.Log("[APICALLS] machineid " + machineid);
#endif

        if (multiplayerlobby.AmILauncher())
            yield break;

        GameLoader.myself.LoadMachineName();

        string code = GameLoader.LoadArcadeCode();

        string json = "{\"id_machine\":\""+ machineid + "\"}";

        if (!string.IsNullOrEmpty(code))
        {
            json = "{" +
            "\"id_machine\":\"" + machineid + "\"," +
            "\"codeaccess\":\"" + code + "\"" +
            "}";
        }

#if DEBUG_APICALLS
        Debug.Log("[APICALLS] Machine register:" + machineid);
#endif
        WWWForm form = new WWWForm();
        form.AddField("data", json);

        double id = 0;
        while (id == 0)
        {
            id = connection.myself.Post(server_url + "register", form);
            yield return null;
        }
        while (connection.myself.Wait(id))
            yield return null;

        string errortext = connection.myself.Error(id);
        string wwwtext = connection.myself.Result(id);
        if (errortext != "")
        {
        }
        else
        {
#if DEBUG_APICALLS
            Debug.Log("[APICALLS] " + wwwtext);
#endif
            if (ErrorTestRegister(wwwtext))
            {
                /*
                if (PlayerPrefs.HasKey("license_passcode"))
                {
                    StartCoroutine(RegisterWithPasscode(PlayerPrefs.GetString("license_passcode")));
                }
                else
                {
// NOT REGISTERED !! BLOCK!!
                }
                */
            }
            else
            {
// All OK
            }

        }

        if (PlayerPrefs.HasKey("trytosavegame"))
        {
            StartCoroutine(GameEnd(PlayerPrefs.GetString("trytosavegame")));
        }
    }

    public static void UpdateServerURL(bool checkGlobalDefines = false, bool testLive = false)
	{
        if (checkGlobalDefines)
		{
            multiplayerlobby.SetProductWithGlobalDefines();
        }

#if !UNITY_EDITOR
        testLive = false;
#endif

        if (LicenseLiveServer || testLive)
        {
            switch (multiplayerlobby.product)
			{
                case multiplayerlobby.Product.KDK:
                    server_url = "https://kdk.trailsofgold.fr/api/";
                    break;
                case multiplayerlobby.Product.BOD:
                    server_url = "https://bod.trailsofgold.fr/api/";
                    break;
                case multiplayerlobby.Product.TOG:
                    server_url = "https://tog.trailsofgold.fr/api/";
                    break;
            }
        }
        else
        {
            server_url = "https://tests.trailsofgold.fr/api/";
        }
#if DEBUG_APICALLS
        Debug.Log("[APICALLS] Update server URL: " + server_url);
#endif
    }

    private string GetLauncherMachineId()
    {
        string id = SystemInfo.deviceUniqueIdentifier;
        return testprefix + id + "-Launcher";
    }

    private string GetInstanceMachineId()
    {
        string id = SystemInfo.deviceUniqueIdentifier;
#if USE_SYNTHESIS
        id += "-synthesis"; 
#endif
#if UNITY_EDITOR
        id += "-editor"; 
#endif
        if (GameflowBase.isBotScene)
            return testprefix + id + "-bot-" + instanceOnMachine;
        bool useInstance = false;
#if USE_TESTS
        useInstance = true;
#endif
        if (useInstance)
            return testprefix + id + "-" + instanceOnMachine;
        else
            return testprefix + id;
        
    }

    private void AddInstanceMachineId()
	{
#if UNITY_EDITOR
        instanceOnMachine = 0;
#else
        string key = "MachineIdInstanceCount";
        int num = PlayerPrefs.GetInt(key, 0) + 1;
        PlayerPrefs.SetInt(key, num);
        PlayerPrefs.Save();
        instanceOnMachine = num;
#endif
    }

    private void RemoveInstanceMachineId()
	{
#if !UNITY_EDITOR
        string key = "MachineIdInstanceCount";
        int num = Mathf.Max(0, PlayerPrefs.GetInt(key, 0) - 1);
        PlayerPrefs.SetInt(key, num);
        PlayerPrefs.Save();
#endif
    }

    public int registercode_return = 0;
    public IEnumerator RegisterWithPasscode(string code)
    {
        registercode_return = 0;
        string json = "{" +
            "\"id_machine\":\"" + machineid + "\"," +
            "\"codeaccess\":\"" + code + "\"" +
            "}";
#if DEBUG_APICALLS
        Debug.Log("[APICALLS] RegisterWithPasscode:" + machineid);
#endif
        WWWForm form = new WWWForm();
        form.AddField("data", json);

        double id = 0;
        while (id == 0)
        {
            id = connection.myself.Post(server_url + "register", form);
            yield return null;
        }
        while (connection.myself.Wait(id))
            yield return null;

        string errortext = connection.myself.Error(id);
        string wwwtext = connection.myself.Result(id);
        registercode_return = 1;
        if (errortext != "")
        {
        }
        else
        {
#if DEBUG_APICALLS
            Debug.Log("[APICALLS] RegisterWithPasscode:" + wwwtext);
#endif
            if (ErrorTestRegister(wwwtext))
            {
            }
            else
            {
                // All OK
                registercode_return = 2;
            }
        }
    }

    public static void UnRegisterEndTime()
    {
        PlayerPrefs.DeleteKey(LAST_REGISTERED_END_TIME + machineid);
        PlayerPrefs.DeleteKey(LAST_REGISTERED_END_TIME_VALUE + machineid);
    }

    public static void RegisterEndTime()
    {
        DateTime dt = DateTime.Now;
        PlayerPrefs.SetString(LAST_REGISTERED_END_TIME + machineid, CreateDate(dt));
        string val = dt.ToBinary().ToString();
        PlayerPrefs.SetString(LAST_REGISTERED_END_TIME_VALUE + machineid, val);
        PlayerPrefs.SetFloat(LAST_REGISTERED_END_TIME_PAUSE + machineid, 0.0f);
        PlayerPrefs.DeleteKey(LAST_REGISTERED_PAUSE + machineid);
        PlayerPrefs.Save();
    }

    public static void SetPause()
    {
#if DEBUG_APICALLS
        Debug.Log("[APICALLS] SetPause");
#endif
        DateTime dt = DateTime.Now;
        PlayerPrefs.SetString(LAST_REGISTERED_PAUSE + machineid, dt.ToOADate().ToString());
        PlayerPrefs.Save();
    }

    public static void ResumePause()
    {
#if DEBUG_APICALLS
        Debug.Log("[APICALLS] ResumePause");
#endif
        UpdateRegisteredEndTimePause(true);
    }

    public static void UpdatePause()
    {
#if DEBUG_APICALLS
        Debug.Log("[APICALLS] UpdatePause");
#endif
        UpdateRegisteredEndTimePause(false);
    }

    private static void UpdateRegisteredEndTimePause(bool resume)
	{
        DateTime dt = DateTime.Now;
        string str = PlayerPrefs.GetString(LAST_REGISTERED_PAUSE + machineid);
        double v2 = 0;
        double.TryParse(str, out v2);
        DateTime dt2 = DateTime.FromOADate(v2);
        TimeSpan ts = dt - dt2;
        float totalpause = (float)(ts.TotalSeconds) + PlayerPrefs.GetFloat(LAST_REGISTERED_END_TIME_PAUSE + machineid);
        Debug.Log("[PAUSE] totalpause " + totalpause);
        PlayerPrefs.SetFloat(LAST_REGISTERED_END_TIME_PAUSE + machineid, totalpause);
        if (resume)
            PlayerPrefs.DeleteKey(LAST_REGISTERED_PAUSE + machineid);
        else
            PlayerPrefs.SetString(LAST_REGISTERED_PAUSE + machineid, dt.ToOADate().ToString());
        PlayerPrefs.Save();
    }

    public static double RegisterTimeDifference()
    {
        if (PlayerPrefs.HasKey(LAST_REGISTERED_PAUSE + machineid))
        {
            UpdatePause();
        }
        DateTime dt = DateTime.Now;
        string str = PlayerPrefs.GetString(LAST_REGISTERED_END_TIME_VALUE + machineid);
        long v2 = 0;
        long.TryParse(str, out v2);
        DateTime dt2 = DateTime.FromBinary(v2);
        TimeSpan ts = dt - dt2;
        double ret = ts.TotalSeconds;
        Debug.Log("[PAUSE] RegisterTimeDifference time base " + ret);
        if (PlayerPrefs.HasKey(LAST_REGISTERED_END_TIME_PAUSE + machineid))
        {
            double pause = (double)PlayerPrefs.GetFloat(LAST_REGISTERED_END_TIME_PAUSE + machineid);
            Debug.Log("[PAUSE] RegisterTimeDifference pause " + pause);
            ret -= pause;
            if (ret < 0)
                ret = 0;
        }
        Debug.Log("[PAUSE] RegisterTimeDifference time result " + ret);
        return (ret);
    }

    public static string GetUniqueSession()
	{
        DateTime dt = DateTime.Today;
        TimeSpan ts = DateTime.Now - dt;
        return dt.Year.ToString() + dt.Month.ToString("00") + dt.Day.ToString("00") + ((int)(ts.TotalMilliseconds)).ToString("0000");
    }

    IEnumerator GameStart()
    {
        while (GameEndIsBusy)
            yield return null;

        DateTime dt = DateTime.Now;
        string mydate = CreateDate(dt);
        RegisterEndTime();

        if (multiplayerlobby.sessionid == "0000")
            multiplayerlobby.sessionid = GetUniqueSession();

        string json = "{" +
            "\"id_machine\":\"" + machineid + "\"," +
            "\"session_id\":\"" + multiplayerlobby.sessionid + "\"," +
            "\"game_id\":\"game" + multiplayerlobby.levelname + "\"," +
            "\"date\":\"" + mydate + "\"," +
            "\"product\":\"" + multiplayerlobby.product + "\"" +
            "}";
        
        if (PlayerPrefs.HasKey("HasCrashed"))
        {
            if (PlayerPrefs.HasKey("CrashLastRegisteredEndTime"))
            {
                string crashdate = PlayerPrefs.GetString("CrashLastRegisteredEndTime");
                json = "{" +
                    "\"id_machine\":\"" + machineid + "\"," +
                    "\"date\":\"" + mydate + "\"," +
                    "\"crashed\":\"" + crashdate + "\"," +
                    "\"product\":\"" + multiplayerlobby.product + "\"" +
                    "}";
            }
        }
#if DEBUG_APICALLS
        Debug.Log("[APICALLS] >>>" + json);
#endif
        WWWForm form = new WWWForm();
        form.AddField("data", json);

        double id = 0;
        while (id == 0)
        {
            id = connection.myself.Post(server_url + "gamestart", form);
            yield return null;
        }
        while (connection.myself.Wait(id))
            yield return null;

        string errortext = connection.myself.Error(id);
        string wwwtext = connection.myself.Result(id);
        if (errortext != "")
        {
        }
        else
        {
#if DEBUG_APICALLS
            Debug.Log("[APICALLS] GameStart:" + wwwtext);
#endif
            if (ErrorTestRegister(wwwtext))
            {

            }
            else
            {
                // All OK
                _isGameStarted = true;
                // First save after 3s
                yield return new WaitForSeconds(3f);
                UpdateGameCounter();
            }
        }
    }

    

    IEnumerator GameEnd(string jsonreplace="")
    {
        bool isReplacingJson = !string.IsNullOrEmpty(jsonreplace);
        if (!_isGameStarted && !isReplacingJson)
            yield break;

        while (GameEndIsBusy)
            yield return null;
        
        GameEndIsBusy = true;
        multiplayerlobby ml = GameObject.FindObjectOfType<multiplayerlobby>();

        _isGameStarted = false;

        string json = "";
        if (isReplacingJson)
        {
            json = jsonreplace;
        }
        else
        {
            if (string.IsNullOrEmpty(savejsonstring))
			{
                UI_EndRaceResult.ScoreValueTypeCoef scoreCoef = gamesettings_general.myself.scoreValueTypeCoef;
                UI_EndRaceResult.EndPlayerData data = new UI_EndRaceResult.EndPlayerData(GameflowBase.myTeam, GameflowBase.myId, scoreCoef, 0f);
                PrepareJson(GameflowBase.myId, data, false);
            }

            DateTime dt = DateTime.Now;
            string mydate = CreateDate(dt);
            string myduration = ((long)RegisterTimeDifference()).ToString();

            json = "{" +
                "\"id_machine\":\"" + machineid + "\"," +
                "\"session_id\":\"" + multiplayerlobby.sessionid + "\"," +
                "\"game_id\":\"game" + multiplayerlobby.levelname + "\"," +
                "\"game_data\":" + savejsonstring + "," +
                "\"date\":\"" + mydate + "\"," +
                "\"duration\":" + myduration + "," +
                "\"product\":\"" + multiplayerlobby.product + "\"" +
                "}";
        }

        if (!isReplacingJson)
        {
            PlayerPrefs.SetString("trytosavegame", json);
            PlayerPrefs.Save();
        }

#if DEBUG_APICALLS
        if (isReplacingJson)
            Debug.Log("[APICALLS] PREVIOUS GAME:" + json);
        else
            Debug.Log("[APICALLS] END GAME:" + json);
#endif

        WWWForm form = new WWWForm();
        form.AddField("data", json);

        double id = 0;
        while (id == 0)
        {
            id = connection.myself.Post(server_url + "gameend", form);
            yield return null;
        }
        while (connection.myself.Wait(id))
            yield return null;

        string errortext = connection.myself.Error(id);
        string wwwtext = connection.myself.Result(id);
        if (errortext != "")
        {
            Debug.LogError(errortext);
        }
        else
        {
#if DEBUG_APICALLS
            Debug.Log("[APICALLS] GameEnd:" + wwwtext);
#endif
            if (ErrorTestRegister(wwwtext))
            {

            }
            else
            {
                PlayerPrefs.DeleteKey("trytosavegame");
                PlayerPrefs.Save();
                // All OK
                if (!isReplacingJson)
                {
                    int tmp = 0;
                    int.TryParse(multiplayerlobby.levelname, out tmp);
                    tmp++;
                    multiplayerlobby.levelname = tmp.ToString();
                }
            }
        }
        if (!isReplacingJson)
            UnRegisterEndTime();
        GameEndIsBusy = false;
    }

    IEnumerator GameEndGameStart()
    {
        StartCoroutine(GameEnd());
        while (GameEndIsBusy)
            yield return null;
        StartCoroutine(GameStart());
    }

    public void StartGameCounter()
    {
#if DEBUG_APICALLS
        Debug.Log("[APICALLS] !!! StartGameCounter Called");
#endif
#if USE_SYNTHESIS && IS_MASTER
        Debug.Log("[SYNTHESIS] ResetBillingSession: " + SynthesisArcadeObject.Instance.synthesisGameId);
        ArcadeInterface svrInterface = ArcadeFactory.Synthesis();
        svrInterface.ResetBillingSession(SynthesisArcadeObject.Instance.synthesisGameId);
#endif
        StartCoroutine(GameStart());
        lastdt = DateTime.Now;
    }
    public void StopGameCounter()
    {
#if DEBUG_APICALLS
        Debug.Log("[APICALLS] !!! StopGameCounter Called");
#endif
        StartCoroutine(GameEnd());
    }
    public void UpdateGameCounter()
    {
#if DEBUG_APICALLS
        Debug.Log("[APICALLS] UpdateGameCounter Called");
#endif
        StartCoroutine(UpdateGameEnd());
    }
    public void StopAndStartGameCounter()
    {
#if DEBUG_APICALLS
        Debug.Log("[APICALLS] StopAndStartGameCounter Called");
#endif
        StartCoroutine(GameEndGameStart());
    }

    public void SendLeaderboard()
	{
#if USE_SYNTHESIS && IS_MASTER
        if (!string.IsNullOrEmpty(savejsonstring))
        {
            Debug.Log("[SYNTHESIS] AddToLeaderboard:\n" + savejsonstring);
            ArcadeInterface svrInterface = ArcadeFactory.Synthesis();
            var success = svrInterface.AddToLeaderboard(savejsonstring);
        }
#endif
    }

    public void PrepareJson(long id, UI_EndRaceResult.EndPlayerData ep, bool win)
    {
        if (id == PhotonNetworkController.GetPlayerId())
        {
            savejsonstring = ComputeJson(id, ep, win);
        }
    }

    private string ComputeJson(long id, UI_EndRaceResult.EndPlayerData ep, bool win)
	{
        int wave = 0;
        float gameTime = 0f;
        int totalscore = 0;
        if (TowerDefManager.myself != null)
		{
            wave = TowerDefManager.myself.currentWave;
            gameTime = TowerDefManager.myself.gameTime;
        }
        if (RaceManager.myself != null)
		{
            totalscore = RaceManager.myself.totalGold;
        }

        string jsonstring = "{";
        jsonstring += "\"name\":\"" + ep.sName + "\",";
        jsonstring += "\"playerId\":" + id + ",";
        jsonstring += "\"title\":\"" + ep.sTitle + "\",";
        jsonstring += "\"team\":" + ep.team + ",";
        jsonstring += "\"treasures\":" + ep.treasures + ",";
        jsonstring += "\"kills\":" + ep.kills + ",";
        jsonstring += "\"deaths\":" + ep.deaths + ",";
        jsonstring += "\"monsters\":" + ep.monsters + ",";
        jsonstring += "\"skinId\":" + ep.skinId + ",";
        jsonstring += "\"score\":" + ep.score + ",";
        jsonstring += "\"malus\":" + ep.malus + ",";
        jsonstring += "\"theme\":\"" + multiplayerlobby.theme.ToString() + "\",";
#if USE_KDK
        jsonstring += "\"drones\":" + ep.drone + ",";
        jsonstring += "\"mines\":" + ep.mine + ",";
        jsonstring += "\"superDrones\":" + ep.superDrone + ",";
        jsonstring += "\"megaDroids\":" + ep.megaDroid + ",";
        jsonstring += "\"plasmaBombs\":" + ep.plasmaBomb + ",";
        jsonstring += "\"bombers\":" + ep.bomber + ",";
        jsonstring += "\"conveyors\":" + ep.conveyor + ",";
        jsonstring += "\"droneUltras\":" + ep.droneUltra + ",";
        jsonstring += "\"state\":\"" + GameflowKDK.myGameState + "\",";
        jsonstring += "\"wave\":" + wave + ",";
        jsonstring += "\"gameTime\":" + gameTime.ToString("F3") + ",";
        jsonstring += "\"levelname\":\"" + GameflowKDK.currentLevelName + "\",";
        jsonstring += "\"totalscore\":" + Player.myplayer.teamGold + ",";
        jsonstring += "\"progress\":\"" + Player.myplayer.progress + "\",";
        jsonstring += "\"product\":\"KDK\",";
#elif USE_BOD
        jsonstring += "\"state\":\"" + GameflowBOD.myGameState + "\",";
        jsonstring += "\"wave\":" + wave + ",";
        jsonstring += "\"gameTime\":" + gameTime.ToString("F3") + ",";
        jsonstring += "\"levelname\":\"" + GameflowBOD.currentLevelName + "\",";
        jsonstring += "\"totalscore\":" + Player.myplayer.teamGold + ",";
        jsonstring += "\"progress\":\"" + Player.myplayer.progress + "\",";
        jsonstring += "\"archives\":" + ep.archives  + ",";
        jsonstring += "\"scientists\":" + ep.scientists + ",";
        jsonstring += "\"completion\":" + GameflowBOD.completion + ",";
        jsonstring += "\"product\":\"BOD\",";
#else
        jsonstring += "\"levelname\":\"" + gameflowmultiplayer.currentLevelName + "\",";
        jsonstring += "\"totalscore\":" + totalscore + ",";
        jsonstring += "\"state\":\"" + gameflowmultiplayer.myGameState + "\",";
        jsonstring += "\"distance\":" + ep.distance.ToString("F3") + ",";
        jsonstring += "\"tourret\":" + ep.tourret + ",";
        jsonstring += "\"boatsKills\":" + ep.boatsKills + ",";
        jsonstring += "\"mermaids\":" + ep.mermaids + ",";
        jsonstring += "\"skulls\":" + ep.skulls + ",";
        jsonstring += "\"obstacles\":" + ep.obstacles + ",";
        jsonstring += "\"product\":\"TOG\",";
#endif
        jsonstring += "\"win\":" + (win ? "1" : "0") + ",";
        jsonstring += "\"launcher\":\"" + (InitGameData.instance != null ? "advanced" : "classic") + "\",";
        jsonstring += "\"version\":\"" + AllMyScripts.Common.Version.Version.GetVersionNumber() + "\"}";
        return jsonstring;
    }

    public static void GetSessions(string sess)
    {
        myself.StartCoroutine(myself.CreateAllHOF(sess));
    }

    List<IDictionary> [] receivedgames;

    public IEnumerator CreateAllHOF(string sess, bool debug = false)
    {
        yield return new WaitForSeconds(0.2f);

        yield return GetSessionData(sess, debug);
        // now setup for each game

        string productName = "LaSuiteStudioGame";
        switch (multiplayerlobby.product)
		{
            case multiplayerlobby.Product.TOG:
                productName = "Trails Of Gold";
                GameflowBase.nrteam = 2;
                GameflowBase.nrplayersperteam = 10;
                break;
            case multiplayerlobby.Product.KDK:
                productName = "Kaireyhs";
                GameflowBase.nrteam = 1;
                GameflowBase.nrplayersperteam = 16;
                break;
            case multiplayerlobby.Product.BOD:
                productName = "RavageK1S1";
                GameflowBase.nrteam = 1;
                GameflowBase.nrplayersperteam = 8;
                break;
        }
        GameflowBase.nrplayersmax = GameflowBase.nrplayersperteam * GameflowBase.nrteam;

        GameflowBase.piratenames = new string[GameflowBase.nrplayersmax];
        GameflowBase.pirateskins = new string[GameflowBase.nrplayersmax];
        GameflowBase.piratesCustomHats = new string[GameflowBase.nrplayersmax];
        GameflowBase.pirateStatsNames = new string[GameflowBase.nrplayersmax];
        GameflowBase.piratedeaths = new bool[GameflowBase.nrplayersmax];
        GameflowBase.piratehealths = new float[GameflowBase.nrplayersmax];
        GameflowBase.teamlistA = new long[GameflowBase.nrplayersperteam];
        GameflowBase.teamlistB = new long[GameflowBase.nrplayersperteam];

    // generate structure
    string rootpath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/" + productName;
        if (!System.IO.Directory.Exists(rootpath)) System.IO.Directory.CreateDirectory(rootpath);
        // sessions day
        rootpath += "/" + DateTime.Now.ToString("yyyy_MM_dd");
        if (!System.IO.Directory.Exists(rootpath)) System.IO.Directory.CreateDirectory(rootpath);
        // session itself
        rootpath += "/" + DateTime.Now.ToString("T").Replace(":","_");
        if (!System.IO.Directory.Exists(rootpath)) System.IO.Directory.CreateDirectory(rootpath);

        gameflowmultiplayer.boatDistances = new float[GameflowBase.nrteam];

        for (int index = 0;index < receivedgames.Length;index++)
        {
            if (receivedgames[index] != null)
            {
//                Debug.Log("================Receivedgames " + index);
                string levelname = "";
                int completion = 100;
                bool[] team = new bool[GameflowBase.nrteam];
                long[] total_treasures = new long[GameflowBase.nrteam];
                long[] total_kills = new long[GameflowBase.nrteam];
                long[] total_deaths = new long[GameflowBase.nrteam];
                long[] total_obstacles = new long[GameflowBase.nrteam];
                long[] total_tourret = new long[GameflowBase.nrteam];
                long[] total_boatsKills = new long[GameflowBase.nrteam];
                long[] total_mermaids = new long[GameflowBase.nrteam];
                long[] total_monsters = new long[GameflowBase.nrteam];
                long[] total_drones = new long[GameflowBase.nrteam];
                long[] total_mines = new long[GameflowBase.nrteam];
                long[] total_superDrones = new long[GameflowBase.nrteam];
                long[] total_megaDroids = new long[GameflowBase.nrteam];
                long[] total_plasmaBombs = new long[GameflowBase.nrteam];
                long[] total_bombers = new long[GameflowBase.nrteam];
                long[] total_conveyors = new long[GameflowBase.nrteam];
                long[] total_droneUltras = new long[GameflowBase.nrteam];
                long[] total_score = new long[GameflowBase.nrteam];
                long totalscore = 0;
                long numWave = 0;
                float gameTime = 0;
                bool[] win = new bool[GameflowBase.nrteam];

                for (int i = 0; i < GameflowBase.nrteam; i++)
                {
                    team[i] = false;
                    total_treasures[i] = 0;
                    total_kills[i] = 0;
                    total_deaths[i] = 0;
                    total_obstacles[i] = 0;
                    total_tourret[i] = 0;
                    total_boatsKills[i] = 0;
                    total_mermaids[i] = 0;
                    total_monsters[i] = 0;
                    total_drones[i] = 0;
                    total_mines[i] = 0;
                    total_superDrones[i] = 0;
                    total_megaDroids[i] = 0;
                    total_plasmaBombs[i] = 0;
                    total_bombers[i] = 0;
                    total_conveyors[i] = 0;
                    total_droneUltras[i] = 0;
                    total_score[i] = 0;
                    win[i] = false;
                    gameflowmultiplayer.boatDistances[i] = 0f;
                    gameflowmultiplayer.finishTeams[i] = -1;
                }
                foreach (IDictionary dic in receivedgames[index])
                {
                    int t = (int)(long)dic["team"];
                    team[t] = true;
                    if (levelname == "") levelname = (string)dic["levelname"];
                    if (dic.Contains("completion"))
                        completion = (int)((long)dic["completion"]);

                    bool isTog = true;
                    bool isKdk = false;
                    bool isBod = false;

                    if (dic.Contains("product"))
                    {
                        string product = (string)dic["product"];
                        isTog = product == "TOG";
                        isKdk = product == "KDK";
                        isBod = product == "BOD";
                    }

                    total_treasures[t] += (long)dic["treasures"];
                    total_kills[t] += (long)dic["kills"];
                    total_deaths[t] += (long)dic["deaths"];
                    total_monsters[t] += (long)dic["monsters"];
                    if (isTog)
                    {
                        total_obstacles[t] += (long)dic["obstacles"];
                        total_tourret[t] += (long)dic["tourret"];
                        total_boatsKills[t] += (long)dic["boatsKills"];
                        total_mermaids[t] += (long)dic["mermaids"];
                    }
                    if (isKdk)
                    {
                        total_drones[t] = (long)dic["drones"];
                        total_mines[t] = (long)dic["mines"];
                        total_superDrones[t] = (long)dic["superDrones"];
                        total_megaDroids[t] = (long)dic["megaDroids"];
                        total_plasmaBombs[t] = (long)dic["plasmaBombs"];
                        total_bombers[t] = (long)dic["bombers"];
                        total_conveyors[t] = (long)dic["conveyors"];
                        total_droneUltras[t] = (long)dic["droneUltras"];
                        numWave = (long)dic["wave"];
                    }
                    if (isBod)
					{
                        numWave = (long)dic["wave"];
                        gameTime = (float)Helper.GetDictionaryValue(dic, "gameTime");
                    }
                    total_score[t] += (long)dic["score"];
                    if (dic.Contains("totalscore"))
                    {
                        totalscore = (long)dic["totalscore"];
                    }
                    else
                    {
                        totalscore += total_treasures[t];
                    }
                    win[t] = (long)dic["win"] > 0;
                    if (dic.Contains("distance") && GameflowBase.nrteam > 1)
                    {
                        gameflowmultiplayer.boatDistances[t] = (float)Helper.GetDictionaryValue(dic, "distance");
                        // Add minimum distance to other team if distance is set to this team
                        if (gameflowmultiplayer.boatDistances[1 - t] == 0 && gameflowmultiplayer.boatDistances[t] > 0f)
                            gameflowmultiplayer.boatDistances[1 - t] = gameflowmultiplayer.boatDistances[t] + (win[t] ? -1f : 1f);
                    }
                }
                UI_EndRaceResult endRaceScreenPrefab = endRaceTOG_2;
                if (multiplayerlobby.product == multiplayerlobby.Product.TOG)
                {
                    // define display type
                    if ((!team[0]) || (!team[1]))
                    {
                        endRaceScreenPrefab = endRaceTOG_1;
                        GameflowBase.myTeam = team[0] ? 0 : 1;
                    }
                    gameflowmultiplayer.finishTeams[0] = win[0] ? 0 : (win[1] ? 1 : -1);
                }
                else if (multiplayerlobby.product == multiplayerlobby.Product.KDK)
				{
                    endRaceScreenPrefab = endRaceKDK_1;
                    GameflowBase.myTeam = 0;
                }
                else if (multiplayerlobby.product == multiplayerlobby.Product.BOD)
                {
                    endRaceScreenPrefab = endRaceBOD_1;
                    GameflowBase.myTeam = 0;
                }

                if (GameflowBase.teamlistA == null)
                    GameflowBase.teamlistA = new long[GameflowBase.nrplayersperteam];
                if (GameflowBase.teamlistB == null)
                    GameflowBase.teamlistB = new long[GameflowBase.nrplayersperteam];

                for (int i = 0; i < GameflowBase.teamlistA.Length; ++i)
                    GameflowBase.SetInTeamA(i, -1);
                for (int i = 0; i < GameflowBase.teamlistB.Length; ++i)
                    GameflowBase.SetInTeamB(i, -1);

                int teamA = 0;
                int teamB = 0;
                int playerId = 0;
                // fill in each player
                foreach (IDictionary dic in receivedgames[index])
                {
                    int t = (int)(long)dic["team"];
                    if (levelname == "")
                        levelname = (string)dic["levelname"];
                    int skinId = (int)(long)dic["skinId"];
                    string name = (string)dic["name"];
					string title = (string)dic["title"];

                    gamesettings_player gsp = null;
                    switch (multiplayerlobby.product)
					{
                        case multiplayerlobby.Product.TOG:
                            gsp = gsPlayerTOG;
                            break;
                        case multiplayerlobby.Product.KDK:
                            gsp = gsPlayerKDK;
                            break;
                        case multiplayerlobby.Product.BOD:
                            gsp = gsPlayerBOD;
                            break;
                    }

                    GameflowBase.piratenames[playerId] = name;
                    GameflowBase.pirateskins[playerId] = gsp != null ? gsp.GetSkinName(skinId) : "default";
                    if (t == 0)
                        GameflowBase.SetInTeamA(teamA++, playerId++);
                    else
                        GameflowBase.SetInTeamB(teamB++, playerId++);
                }

                UI_EndRaceResult endRaceResultScreen = GameObject.Instantiate<UI_EndRaceResult>(endRaceScreenPrefab);
                endRaceResultScreen.InitData(true, levelname, (int)numWave, (int)totalscore);
                endRaceResultScreen.SetMissionData(completion, gameTime);

                playerId = 0;
                // fill in each player
                foreach (IDictionary dic in receivedgames[index])
                {
                    UI_EndRaceResult.EndPlayerData data = endRaceResultScreen.GetPlayerData(playerId);
                    if (data != null)
					{
                        data.SetupFromDic(dic as Dictionary<string, object>);
                    }
                    playerId++;
                }

                endRaceResultScreen.InitScreen(true, (int)totalscore);
                yield return new WaitForSeconds(0.2f);
                endRaceResultScreen.TakeScreenShootAtPath(rootpath, index);
                yield return null;
				GameObject.Destroy(endRaceResultScreen.gameObject);
				yield return null;
            }
            yield return null;
        }

    }


    public IEnumerator GetSessionData(string sess, bool debug)
    {
#if DEBUG_APICALLS
        Debug.Log("[APICALLS] Session:" + sess+ " ");
#endif

        double id = 0;
        while (id == 0)
        {
            if (debug)
                id = connection.myself.Get(server_url + "lastgames?debug_session_id=" + sess + "&codeaccess=" + debugCodeAccess);
            else
                id = connection.myself.Get(server_url + "lastgames?session_id=" + sess);
            yield return null;
        }
        while (connection.myself.Wait(id))
            yield return null;

        string errortext = connection.myself.Error(id);
        string wwwtext = connection.myself.Result(id);
        if (errortext != "")
        {
        }
        else
        {
            Debug.Log("GetSessionData:" + wwwtext);
            IDictionary res = (IDictionary)Json.Deserialize(wwwtext);
            List<object> games = null;
            try
            {
               games = (List<object>)res["result"];
            }
            catch
            {
                games = null;
            }
            receivedgames = new List<IDictionary>[32];
            for (int i = 0; i < 32; i++) receivedgames[i] = null;

            if (games != null)
            {
                foreach (IDictionary game in games)
                {
                    string tmpstr = ((string)game["game_id"]).Replace("game", "");
                    int index = 0;
                    int.TryParse(tmpstr, out index);

                    IDictionary gdata = null;
                    try
                    {
                        gdata = (IDictionary)game["game_data"];
                    }
                    catch
                    {

                    }
                    if (gdata != null)
                    {
                        if (receivedgames[index] == null)
                            receivedgames[index] = new List<IDictionary>();
                        receivedgames[index].Add(gdata);
//                        Debug.Log(Json.Serialize(gdata));
                    }
                }                
            }
        }
    }



    IEnumerator UpdateGameEnd()
    {
        if (!_isGameStarted)
            yield break;

        while (GameEndIsBusy) 
            yield return null;

        GameEndIsBusy = true;
        multiplayerlobby ml = GameObject.FindObjectOfType<multiplayerlobby>();

        DateTime dt = DateTime.Now;
        string mydate = CreateDate(dt);
        string myduration = ((long)RegisterTimeDifference()).ToString();

        UI_EndRaceResult.ScoreValueTypeCoef scoreCoef = gamesettings_general.myself.scoreValueTypeCoef;
        UI_EndRaceResult.EndPlayerData data = new UI_EndRaceResult.EndPlayerData(GameflowBase.myTeam, GameflowBase.myId, scoreCoef, 0);
        string gameData = ComputeJson(GameflowBase.myId, data, false);

        string json = "{" +
            "\"id_machine\":\"" + machineid + "\"," +
            "\"session_id\":\"" + multiplayerlobby.sessionid + "\"," +
            "\"game_id\":\"game" + multiplayerlobby.levelname + "\"," +
            "\"game_data\":" + gameData + "," +
            "\"date\":\"" + mydate + "\"," +
            "\"duration\":" + myduration + "," +
            "\"product\":\"" + multiplayerlobby.product + "\"" +
            "}";
        WWWForm form = new WWWForm();
        form.AddField("data", json);

        double id = 0;
        while (id == 0)
        {
            id = connection.myself.Post(server_url + "gameend", form);
            yield return null;
        }
        while (connection.myself.Wait(id))
            yield return null;

        string errortext = connection.myself.Error(id);
        string wwwtext = connection.myself.Result(id);
        if (errortext != "")
        {
            Debug.LogError(errortext);
        }
        else
        {
#if DEBUG_APICALLS
            Debug.Log("[APICALLS] GameEnd:" + wwwtext);
#endif
            if (ErrorTestRegister(wwwtext))
            {

            }
            else
            {

            }
        }
        GameEndIsBusy = false;
    }

    ///////////////////////////////////
    /// CRASH PROTECTION
    ///////////////////////////////////

    static DateTime lastdt;

    public static void LoadCrash()
    {
        lastdt = DateTime.Now;
        PlayerPrefs.DeleteKey("crashroom");
        PlayerPrefs.DeleteKey("CrashLastRegisteredEndTime");
        //PlayerPrefs.SetInt("HasCrashed", 1);
        // HOTFIX -> Don't take care of crashes (not the good detection)
        PlayerPrefs.DeleteKey("HasCrashed");
        PlayerPrefs.Save();
    }


    public void PingCrashPrevention()
    {
        DateTime dt = DateTime.Now;
        TimeSpan ts = dt - lastdt;
        if (ts.TotalSeconds > 60)
        {
            PlayerPrefs.SetString("CrashLastRegisteredEndTime", CreateDate(dt));
            UpdateGameCounter();
            lastdt = dt;
        }
    }

    private void OnApplicationQuit()
    {
        if (!multiplayerlobby.AmILauncher())
        {
            StopGameCounter();     // quit, force save existing game
        }
        PlayerPrefs.DeleteKey("HasCrashed");
        PlayerPrefs.Save();
    }

    private void Update()
    {
        if (_isGameStarted && !multiplayerlobby.AmILauncher())
        {
            PingCrashPrevention();
        }

    }

}
