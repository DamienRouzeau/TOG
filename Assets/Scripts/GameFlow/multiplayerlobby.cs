#define DEBUG_MULTI

using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AllMyScripts.Common.Version;
using ExitGames.Client.Photon;
using AllMyScripts.Common.Tools;

#if STEAM_PRESENT
#endif
using MiniJSON;

public class multiplayerlobby : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public enum Product
	{
        TOG,
        KDK,
        BOD
	}

    public enum GameMode
    {
        Normal,
        Endless,
        Kid,
        Count
    }

    public enum SkinTheme
    {
        Normal,
        Halloween,
		Christmas,
        Count,
        Custom
    }

    public enum PhotonEvent
	{
        INIT_DATA = 1
	}

    public const string DELIMITER = "|";
    public const char DELIMITER_CHAR = '|';

    public string clientprefix = "";
    private string _machineRef = "";

    public static GameObject mas = null;
    public GameObject Rooms;
    public GameObject Machines;
    public roomline SelectedGame { get; private set; }

#if DEBUG_MULTI
    Text debugtext = null;
    string fulltext = "";
#endif
    string gameroomtobecreated = "";
    string gameroomtobejoined = "";
    public static multiplayerlobby myself = null;

    public bool gametreatment = false;
    masterrooms _ms = null;
    public int minnrplayers = 1;
    public bool acceptallrooms = false;
    public static string levelname = "0";
    public static string sessionid = "0000";
    public static string machineName = "";
    public static GameMode gameMode = GameMode.Normal;
    public static SkinTheme theme = SkinTheme.Normal;
    public static Product product = Product.TOG;
    public static int endlessDuration = 0;
    public static bool canPlayNextRace = true;
    public static bool useVoip = true;
    public static int startLevel = 0;
    public static int startWave = 0;
    public static bool IsInEndlessRace => endlessDuration > 0 || gameMode == GameMode.Endless;
    public static bool IsInKidMode => gameMode == GameMode.Kid;
    public static bool IsInNormalMode => gameMode == GameMode.Normal;

    [Tooltip("en-US for english, fr-FR for french, you can see other code in RRCountryCode")]
    public string languageCode = "fr-FR";
    public string languageAudioCode = "fr-FR";

    public bool icreated = false;

    public bool isInAdvancedInterface => _isInAdvancedInterface;

    public string lastCreatedRoomName => _lastcreated;
    string _lastcreated = "";

    public bool insideroom = false;
    public bool connected = false;
    public bool waitToConnectToMaster = false;

    public string lastJoinedRoomName => _lastAttemptedJoinRoomName;
    string _lastAttemptedJoinRoomName = "";

    private int _tryToJoinRoomCounter = 5;

    GameObject DemoMode;
    TextMeshProUGUI Demo_Counter;

    public static GameObject globalwaiter = null;

    [SerializeField]
    private Text _versionText;
    [SerializeField]
    private Text _titleText;
    [SerializeField]
    private GameObject _locker = null;
    [SerializeField]
    private registerpage _register = null;
    [SerializeField]
    private Button _backBtn = null;
    [Header("Game Selection")]
    [SerializeField]
    private GameObject _chooseProductRoot = null;
    [SerializeField]
    private Button _togProductBtn = null;
    [SerializeField]
    private Button _kdkProductBtn = null;
    [SerializeField]
    private Button _bodProductBtn = null;
    [Header("Advanced Interface Selection")]
    [SerializeField]
    private Button _classicInterfaceBtn = null;
    [SerializeField]
    private Button _advancedInterfaceBtn = null;
    [SerializeField]
    private GameObject _classicInterfaceCheck = null;
    [SerializeField]
    private GameObject _advancedInterfaceCheck = null;
    [Header("Background")]
    [SerializeField]
    private Image _bkgImage = null;
    [SerializeField]
    private Sprite _togBkgSprite = null;
    [SerializeField]
    private Sprite _kdkBkgSprite = null;
    [SerializeField]
    private Sprite _bodBkgSprite = null;
    [Header("Popups")]
    [SerializeField]
    private GameObject _machinesPopup = null;
    [Header("Flags")]
    [SerializeField]
    private Button _flagButtonFR = null;
    [SerializeField]
    private Button _flagButtonEN = null;

    private List<string> _roomListName = new List<string>();
    private bool _needToCleanListName = false;

    private bool _waitInLobbyStarted = false;
    private bool _isInAdvancedInterface = false;
    private string _gameName = null;

    public static bool AmILauncher()
    {
        if (mas == null)
            return false;
        return true;
    }

    private void Awake()
    {
        if (_versionText != null)
            _versionText.text = $"v. {Version.GetVersionNumber()}";

        acceptallrooms = false;
        DemoMode = GameObject.Find("DemoMode");
        if (DemoMode != null)
        {
            Demo_Counter = DemoMode.FindInChildren("Demo_Counter").GetComponent<TextMeshProUGUI>();
            DemoMode.SetActive(false);
        }
        if (myself != null)
            return;
        DontDestroyOnLoad(this);
        myself = this;
        PhotonNetwork.AutomaticallySyncScene = false;
        mas = GameObject.Find("MasterCanvas");

        GameLoader.LoadRegion();

        if (mas == null)    // client machine
        {
            if (_register != null)
            {
                _register.gameObject.SetActive(true);
                _register.Connect(null);
            }

            apicalls.LoadCrash();

            SetProductWithGlobalDefines();

#if DEBUG_MULTI
            if (GameObject.Find("debugtext") != null)
                debugtext = GameObject.Find("debugtext").GetComponent<Text>();
#endif
            StartCoroutine(WaitInLobby());
        }
        else
        {
            _titleText.gameObject.SetActive(true);

            _backBtn.onClick.AddListener(OnClickBack);

            _chooseProductRoot.SetActive(true);
            _togProductBtn.onClick.AddListener(OnClickTOG);
            _kdkProductBtn.onClick.AddListener(OnClickKDK);
            _bodProductBtn.onClick.AddListener(OnClickBOD);

            _classicInterfaceBtn.onClick.AddListener(OnClickClassicInterface);
            _advancedInterfaceBtn.onClick.AddListener(OnClickAdvancedInterface);

            _flagButtonFR.onClick.AddListener(OnClickFlagFR);
            _flagButtonEN.onClick.AddListener(OnClickFlagEN);

            globalwaiter = _locker; // GameObject.Find("globalwaiter");
            globalwaiter.SetActive(false);
            languageCode = GameLoader.LoadWrittenLanguage(languageCode);
            gamesettings_general.LanguageChanged(languageCode, selectflag.LanguageType.Written);
            // Set default audio code from written code
            languageAudioCode = languageCode;
            languageAudioCode = GameLoader.LoadAudioLanguage(languageAudioCode);
            gamesettings_general.LanguageChanged(languageAudioCode, selectflag.LanguageType.Audio);

            _ms = GameObject.FindObjectOfType<masterrooms>();
            LoadSetup();
            Rooms = mas.FindInChildren("Rooms");
            Machines = mas.FindInChildren("Machines");
            StartCoroutine(WaitInLobby());

            OnClickAdvancedInterface();
            _machinesPopup.SetActive(false);
        }

        /*
        machineline mmm = AddMachineLineToList("TEST 1");
        mmm.reference = "one";
        mmm = AddMachineLineToList("TEST 2");
        mmm.reference = "two";
        mmm = AddMachineLineToList("TEST 3");
        mmm.reference = "three";
        */

        // DEBUG
        //for (int i = 0; i < 20; ++i)
        //	AddMachineLineToList("TEST_" + i);
        // DEBUG
    }

	private void OnDestroy()
	{
        if (_backBtn != null)
            _backBtn.onClick.RemoveListener(OnClickBack);
        if (_togProductBtn != null)
            _togProductBtn.onClick.RemoveListener(OnClickTOG);
        if (_kdkProductBtn != null)
            _kdkProductBtn.onClick.RemoveListener(OnClickKDK);
        if (_bodProductBtn != null)
            _bodProductBtn.onClick.RemoveListener(OnClickBOD);
        if (_classicInterfaceBtn != null)
            _classicInterfaceBtn.onClick.RemoveListener(OnClickClassicInterface);
        if (_advancedInterfaceBtn != null)
            _advancedInterfaceBtn.onClick.RemoveListener(OnClickAdvancedInterface);
        if (_flagButtonFR != null)
            _flagButtonFR.onClick.RemoveListener(OnClickFlagFR);
        if (_flagButtonEN != null)
            _flagButtonEN.onClick.RemoveListener(OnClickFlagEN);
        myself = null;
    }

	private void Update()
    {
#if DEBUG_MULTI
        if (debugtext)
        {
            debugtext.text = fulltext;
        }
#endif  
    }

    public static void SetProductWithGlobalDefines()
	{
#if USE_KDK
        product = Product.KDK;
#elif USE_BOD
        product = Product.BOD;
#else
        product = Product.TOG;
#endif
    }

    public void SetSelectedGame(roomline game)
	{
        SelectedGame = game;
	}

    public void ShowMachinesPopup()
    {
        _machinesPopup.SetActive(true);
    }

    public void QuitMachinesPopup()
    {
        _machinesPopup.SetActive(false);
    }

    public void UpdateDemoMode()
	{
        if (DemoMode != null)
        {
            bool showDemoMode = false;
            if (apicalls.isDemoGame)
            {
                if (apicalls.isExpiratedDemo)
                    Demo_Counter.text = "Expired";
                else
                    Demo_Counter.text = Mathf.Min(apicalls.demoGameSessions, apicalls.maxdemosessions) + "/" + apicalls.maxdemosessions;
                showDemoMode = true;
            }
            DemoMode.SetActive(showDemoMode);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    IEnumerator TryLogin()
    {
        DebugText("========== TryLogin mas " + mas);
        bool invalidloop = true;
        while (invalidloop)
        {
            if (mas == null)
            {
                //                if (crash["crashroom"] != null)
                {
                    //                    if ((string)crash["crashroom"] != "")
                    {
                        if (PlayerPrefs.HasKey("crashroom"))
                        {
                            SetNickname(PlayerPrefs.GetString("crashroom"));
                        }
                        invalidloop = false;
                        try
                        {
                            PhotonNetwork.ConnectUsingSettings();
                        }
                        catch
                        {
                            invalidloop = true;
                        }
                    }
                }
            }
            else
            {
                DebugText("I am : " + _machineRef);
                SetNickname();
                invalidloop = false;
                try
                {
                    PhotonNetwork.ConnectUsingSettings();
                }
                catch
                {
                    invalidloop = true;
                }
            }
            yield return new WaitForSeconds(1.0f);
        }
    }

    private void OnClickBack()
	{
        QuitMachinesPopup();
        _chooseProductRoot.SetActive(true);
        _ms.Remove(_gameName, true);
        _titleText?.gameObject.SetActive(false);
    }

    private void OnClickTOG()
	{
        _chooseProductRoot.SetActive(false);
        product = Product.TOG;
        AddGame();
    }

    private void OnClickKDK()
	{
        _chooseProductRoot.SetActive(false);
        product = Product.KDK;
        AddGame();
    }

    private void OnClickBOD()
    {
        _chooseProductRoot.SetActive(false);
        product = Product.BOD;
        AddGame();
    }

    private void OnClickClassicInterface()
	{
        _isInAdvancedInterface = false;
        _classicInterfaceCheck.SetActive(true);
        _advancedInterfaceCheck.SetActive(false);
    }

    private void OnClickAdvancedInterface()
	{
        _isInAdvancedInterface = true;
        _classicInterfaceCheck.SetActive(false);
        _advancedInterfaceCheck.SetActive(true);
    }

    private void OnClickFlagFR()
	{
        gamesettings_general.LanguageChanged("fr-FR", selectflag.LanguageType.Written);
	}

    private void OnClickFlagEN()
	{
        gamesettings_general.LanguageChanged("en-US", selectflag.LanguageType.Written); 
    }

    private void AddGame()
	{
        apicalls.UpdateServerURL(false, apicalls.myself._debugInLiveServer);
        if (_register != null)
        {
			_register.gameObject.SetActive(true);
            _gameName = "Game" + product + "_" + Mathf.RoundToInt(Time.realtimeSinceStartup * 1000f);
            _register.Connect(() => { _ms.ListRoomAdd(_gameName); });
        }
        
        machineline[] machines = Machines.GetComponentsInChildren<machineline>(true);
        foreach (machineline machine in machines)
		{
            machine.gameObject.SetActive(machine.productName == product.ToString());
        }
        switch (product)
		{
            case Product.TOG:
                _bkgImage.sprite = _togBkgSprite;
                _titleText.text = "Trails Of Gold";
                break;
            case Product.KDK:
                _bkgImage.sprite = _kdkBkgSprite;
                _titleText.text = "Kaireyhs";
                break;
            case Product.BOD:
                _bkgImage.sprite = _bodBkgSprite;
                _titleText.text = "Ravage K1S1";
                break;
        }
        _titleText.gameObject.SetActive(true);
    }

    public void SetNickname(string name = "")
    {
        if (name == "")
            name = _machineRef;
        PhotonNetwork.LocalPlayer.NickName = name;
    }

    public override void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
        DebugText("OnCustomAuthenticationResponse");
    }

    public override void OnCustomAuthenticationFailed(string debugMessage)
    {
        DebugText("OnCustomAuthenticationFailed:" + debugMessage);
    }


    public override void OnConnectedToMaster()
    {
        DebugText("OnConnectedToMaster");
        waitToConnectToMaster = true;
        if (connected)
            return;
        connected = true;
        if (gameroomtobejoined != "")
        {
            JoinRoom(gameroomtobejoined);
            gameroomtobejoined = "";
        }
        else
        {
            if (gameroomtobecreated != "")
            {
                CreateRoom(gameroomtobecreated);
                gameroomtobecreated = "";
            }
            else
            {
                DebugText("Connected");
                if (!mas)
                {
                    DebugText("Create request");
                    CreateRoom();
                }
                else
                {
                    DebugText("Join request");
                    JoinLobby();
                }
            }
        }
    }

    public static string GetMachineName()
	{
        if (string.IsNullOrEmpty(machineName))
            return "$";
        if (machineName.Contains(DELIMITER))
            machineName = machineName.Replace(DELIMITER, "");
        return machineName;
    }

    public void CreateRoom(string roomName = "")
    {
        icreated = true;
        
        if (roomName == "")
            roomName = _machineRef + DELIMITER + GetMachineName() + DELIMITER + product + DELIMITER + Version.GetVersionNumber();
        _lastcreated = roomName;
        PhotonNetworkController.myself.roomname = roomName;
        DebugText("RoomCreated: " + roomName);
        byte maxPlayers = (byte)(GameflowBase.nrplayersmax + 1);
        RoomOptions options = new RoomOptions { MaxPlayers = maxPlayers };
        PhotonNetwork.CreateRoom(roomName, options, null);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        globalwaiter.SetActive(true);
        CreateRoom(_lastcreated);       // Must try again !!!
    }


    /// <summary>
    /// 
    /// </summary>
    public void JoinLobby()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
    }
    public override void OnJoinedLobby()
    {
        DebugText("Joined Lobby");
        _needToCleanListName = true;
        //        PhotonNetwork.GetCustomRoomList();
    }

    machineline AddMachineLineToList(string roomName, string machineId, string machineName, string productName, string version)
    {
        GameObject clone = Instantiate(Resources.Load("machineline") as GameObject);
        machineline myml = clone.GetComponent<machineline>();
        myml.reference = roomName;
        myml.machineId = machineId;
        myml.machineName = machineName;
        myml.productName = productName;
        myml.version = version;
        myml.Init(this, _ms);
        clone.name = "machineline_" + product + DELIMITER + machineName;
        clone.transform.SetParent(Machines.FindInChildren("Content").transform);
        clone.transform.localScale = Vector3.one;
        clone.gameObject.SetActive(productName == product.ToString());
        return (myml);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (mas != null && !string.IsNullOrEmpty(clientprefix))
        {
            List<string> names = new List<string>();
            foreach (RoomInfo info in roomList)
            {
                if (!info.Name.Contains(DELIMITER))
                    continue;

                bool acceptme = acceptallrooms;

                if (!acceptme)
                    acceptme = info.Name.StartsWith(clientprefix);

                if (!acceptme)
                    continue;

                string name = info.Name;
                names.Add(name);
                if (_roomListName.Contains(name))
                {
                    //Debug.Log($"Raph - {name} updated");
                    if (!info.IsVisible || info.RemovedFromList)
                    {
                        _roomListName.Remove(name);
                        //Debug.Log($"Raph - {name} removed");
                        OnRoomListNameRemoved(name);
                    }
                }
                else
                {
                    if (!info.IsVisible || info.RemovedFromList)
                        continue;

                    _roomListName.Add(name);
                    //Debug.Log($"Raph - {name} added");
                    OnRoomListNameAdded(name);
                }
            }

            if (_needToCleanListName)
            {
                _needToCleanListName = false;
                for (int i = _roomListName.Count - 1; i >= 0; i--)
                {
                    string name = _roomListName[i];
                    if (!names.Contains(name))
                    {
                        _roomListName.RemoveAt(i);
                        //Debug.Log($"Raph - {name} removed");
                        OnRoomListNameRemoved(name);
                    }
                }
            }
        }
    }

    private void OnRoomListNameAdded(string name)
    {
        if (!name.Contains("<game>"))
        {
            string[] splitName = name.Split(DELIMITER_CHAR);
            if (splitName.Length < 4)
                return;
            string machineId = splitName[1];
            string machineName = splitName[2];
            string product = splitName[3];
            string version = null;
            if (splitName.Length > 4)
                version = splitName[4];

            if (!_ms.IsThisMachineGaming(machineId))
            {
                DebugText($"OnRoomListNameAdded {name} not in game");

                bool valid = true;
                machineline_tag[] mlts = mas.GetComponentsInChildren<machineline_tag>(true);
                foreach (machineline_tag mlt in mlts)
                {
                    machineline myml = mlt.gameObject.GetComponent<machineline>();
                    if (myml.reference == name)
                    {
                        DebugText("Double lines detected p, :" + mlt.gameObject.name);
                        valid = false;
                        break;
                    }
                }
                if (valid)
                {
                    DebugText("Add lines:" + name);
                    AddMachineLineToList(name, machineId, machineName, product, version);
                }
                else
                {
                    DebugText("Double lines:" + name);
                }
            }
            else
            {
                DebugText($"OnRoomListNameAdded {name} is gaming");
            }
        }
    }

    private void OnRoomListNameRemoved(string name)
    {
        DebugText("REMOVED FROM LIST " + name);

        if (name.Contains("<game>"))
        {
            DebugText("GAME SEEN:" + name);
            
            string[] gamename = name.Split(DELIMITER_CHAR);
            if (gamename.Length > 2)
            {
                name = gamename[2];
                DebugText("FinalizeRoom " + name);
                masterrooms.FinalizeRoom(name);
                return;
            }
        }

        selectedline_tag[] all = gameObject.GetComponentsInChildren<selectedline_tag>(true);
        foreach (selectedline_tag tg in all)
        {
            selectedroomline ssrl = tg.gameObject.GetComponent<selectedroomline>();
            if (ssrl.reference == name)
            {
                ssrl.Remove(); 
                break;
            }
        }
        machineline_tag[] mlts = mas.GetComponentsInChildren<machineline_tag>(true);
        foreach (machineline_tag mlt in mlts)
        {
            machineline myml = mlt.gameObject.GetComponent<machineline>();
            if (myml.reference.Contains(name))
            {
                Destroy(mlt.gameObject);
                break;
            }
        }

        masterrooms.myself.ShowAllMachinesState();
    }

    public bool JoinRoom(string rmname)
    {
        _tryToJoinRoomCounter = 5;
        _lastAttemptedJoinRoomName = rmname;
        DebugText("========== JoinRoom " + rmname);
        return PhotonNetwork.JoinRoom(rmname);
    }

    public void JoinWithRoomInfo(RoomInfo info)
    {
        JoinRoom(info.Name);
    }

    public void LeaveRoom()
    {
        try
        {
            PhotonNetwork.LeaveRoom();
        }
        catch
        {

        }
    }


    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        globalwaiter.SetActive(false);
        DebugText("OnJoinRoomFailed " + message);
        _tryToJoinRoomCounter--;
        if (_tryToJoinRoomCounter > 0)
        {
            // Try join again
            PhotonNetwork.JoinRoom(_lastAttemptedJoinRoomName);
        }
		else
		{
            JoinLobby();
		}
    }

    public override void OnJoinedRoom()
    {
        DebugText("Joined Room");
        insideroom = true;
    }

    public override void OnLeftRoom()
    {
        icreated = false;
        PlayerPrefs.DeleteKey("crashroom");
        PlayerPrefs.Save();
        DebugText("Left Room");
        insideroom = false;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        DebugText("OnDisconnected(" + cause + ")");
        connected = false;
    }


    public IDictionary setup = null;

    void LoadSetup()
    {
        string path = Application.streamingAssetsPath + "/mp_settings.txt";
        if (System.IO.File.Exists(path))
        {
            string json = System.IO.File.ReadAllText(path);
            DebugText("Read:" + json);
            setup = (IDictionary)Json.Deserialize(json);
        }
        else
        {
            string json = "{}";
            setup = (IDictionary)Json.Deserialize(json);
            SaveSetup();
        }
    }

    public void SaveSetup()
    {
        string path = Application.streamingAssetsPath + "/mp_settings.txt";
        string json = (string)Json.Serialize(setup);
        DebugText("Saving:" + json);
        System.IO.File.WriteAllText(path, json);
    }

    public string GetMyName(string inname)
    {
        string reference = inname;
        if (!string.IsNullOrEmpty(clientprefix))
            inname.Replace(clientprefix, "");
        if (setup[reference] != null)
            return ((string)setup[reference]);
        return (inname);
    }

    void DebugText(string txt)
    {
#if DEBUG_MULTI
        Debug.Log(txt);
        fulltext += txt + "\n";
#endif
    }

    IEnumerator RepeatLeaving()
    {
        while (true)
        {
            yield return new WaitForSeconds(5.0f);
            if (!insideroom)
                yield break;
            LeaveRoom();
        }
    }

    private void FillGameData(string lvl)
    {
        if (lvl.Contains(DELIMITER))
        {
            string[] names = lvl.Split(DELIMITER_CHAR);
            // Level
            levelname = names[0];
            // Session
            sessionid = names[1];
            // GameMode
            if (names.Length > 3)
            {
                if (!System.Enum.TryParse(names[3], out gameMode))
                    gameMode = GameMode.Normal;
            }
            // Duration
            if (names.Length > 4)
            {
                if (gameMode == GameMode.Endless)
                    int.TryParse(names[4], out endlessDuration);
                else
                    endlessDuration = 0;
            }
            // Next Race
            canPlayNextRace = true;
            if (names.Length > 5)
            {
                if (int.TryParse(names[5], out int playNextRace))
                    canPlayNextRace = playNextRace != 0;
            }
            // Theme
            if (names.Length > 6)
			{
                if (!System.Enum.TryParse(names[6], out theme))
                    theme = SkinTheme.Normal;
            }
            // Level
            if (names.Length > 7)
            {
                if (!int.TryParse(names[7], out startLevel))
                    startLevel = 0;
            }
            // Wave
            if (names.Length > 8)
            {
                if (!int.TryParse(names[8], out startWave))
                    startWave = 0;
            }
        }
        DebugText($"FillGameData Received lvl {levelname} session {sessionid} mode {gameMode} duration {endlessDuration} canPlayNextRace {canPlayNextRace} theme {theme} startWave {startWave}");
    }

    IEnumerator WaitInLobby()
    {
        while (string.IsNullOrEmpty(apicalls.id_salle))
            yield return null;
        clientprefix = apicalls.id_salle;
        _machineRef = clientprefix + DELIMITER + apicalls.machineid;
        StartCoroutine("TryLogin");
        DebugText("WaitInLobby Ready machineref " + _machineRef);

        _waitInLobbyStarted = true;
    }

    private IEnumerator CreateGame(string name)
	{
        DebugText("CREATE RECEIVED " + name);
        string lvl = name.Replace("create_","");
        FillGameData(lvl);

        // wait for all players to leave
        while (PhotonNetwork.PlayerList.Length > 1)
            yield return null;
        DebugText("Everyone left");
        LeaveRoom();        // leave myself
        StartCoroutine("RepeatLeaving");
        while (insideroom)
            yield return null;
        DebugText("I left");

        PhotonNetwork.Disconnect();
        while (connected)
            yield return null;
        DebugText("I disconnected");
        StopCoroutine("RepeatLeaving");

        string rmname = clientprefix + "<game>" + lvl;
        gameroomtobecreated = rmname;
		PhotonNetwork.ConnectUsingSettings();

		while (!insideroom)
            yield return null;
        PlayerPrefs.SetString("crashroom", rmname);
        PlayerPrefs.Save();

        DebugText("I created " + rmname);
        foreach (Photon.Realtime.Player ppp in PhotonNetwork.PlayerList)
        {
            if ((ppp != null) && (ppp.NickName != null))
                DebugText(ppp.NickName);
        }
        while (!PhotonNetwork.IsConnected)
            yield return null;
        PhotonNetworkController.myself.StartGame();
    }

    private IEnumerator JoinGame(string name)
    {
        DebugText("JOIN RECEIVED " + name);
        string lvl = name.Replace("join_", "");
        FillGameData(lvl);
        // wait for all players to leave
        while (PhotonNetwork.PlayerList.Length > 1)
            yield return null;
        DebugText("Everyone left");
        LeaveRoom();        // leave myself
        StartCoroutine("RepeatLeaving");
        while (insideroom)
            yield return null;
        DebugText("I left");

        PhotonNetwork.Disconnect();
        while (connected)
            yield return null;
        DebugText("I disconnected");
        StopCoroutine("RepeatLeaving");

        string rmname = clientprefix + "<game>" + lvl;
        gameroomtobejoined = rmname;
        PhotonNetwork.ConnectUsingSettings();

        while (!insideroom)
            yield return null;
        PlayerPrefs.SetString("crashroom", rmname);
        PlayerPrefs.Save();
        DebugText("I joined " + rmname);
        foreach (Photon.Realtime.Player ppp in PhotonNetwork.PlayerList)
        {
            if ((ppp != null) && (ppp.NickName != null))
                DebugText(ppp.NickName);
        }

        while (!PhotonNetwork.IsConnected)
            yield return null;
        PhotonNetworkController.myself.StartGame();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player p)
    {
        if (!_waitInLobbyStarted)
            return;

        if ((p != null) && (p.NickName != null))
        {
            if (p.NickName.StartsWith("create_"))
            {
                StartCoroutine(CreateGame(p.NickName));
            }

            if (p.NickName.StartsWith("join_"))
            {
                StartCoroutine(JoinGame(p.NickName));
            }
        }
    }

    public void SendInitDataPhotonEvent(string data)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent((byte)PhotonEvent.INIT_DATA, data, raiseEventOptions, SendOptions.SendReliable);
    }

    void IOnEventCallback.OnEvent(EventData photonEvent)
	{
        byte eventCode = photonEvent.Code;
        if (eventCode == (byte)PhotonEvent.INIT_DATA)
        {
            string data = (string)photonEvent.CustomData;
            Debug.Log("[INIT_DATA] " + data);
            Dictionary<string, object>  dic = JSON.Deserialize(data) as Dictionary<string, object>;
            InitGameData.Create(dic);
        }
    }
}