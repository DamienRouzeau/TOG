using Photon.Pun;
using RRLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class masterrooms : MonoBehaviour
{
    public static masterrooms myself;
    public static int maxplayer = 20;
    private static int nrsessions = 1;

    public int selectedsession => _selectedsession;

    [SerializeField]
    private roomline _roomPrefab = null;
    [SerializeField]
    private selectedroomline _roomLineAdvancedPrefab = null;
    [SerializeField]
    private selectedroomline _roomLinePrefab = null;
    [SerializeField]
    private playerline_tag _playerLineAdvancedPrefab = null;
    [SerializeField]
    private playerline_tag _playerLinePrefab = null;

    public InputField roomname;
    public Text selectedtitle;
    public Text errormessage;
    public Text screentitle;

    private multiplayerlobby _ml;
    private roomline[] sessions = new roomline[nrsessions];
    private machineline[,] allmachines = new machineline[nrsessions, maxplayer];
    private int _selectedsession = -1;

    private void Awake()
    {
        myself = this;
        _ml = gameObject.GetComponentInParent<multiplayerlobby>();
        for (int i = 0; i < nrsessions; i++)
        {
            sessions[i] = null;
            for (int j=0;j< maxplayer;j++)
                allmachines[i,j] = null;
        }
    }

    //bool fillonce = false;
    private void Update()
    {
        if (apicalls.id_salle != "")
        {
            string tit = "MASTER CONSOLE : " + apicalls.id_salle;
            if (tit != screentitle.text)
            {
                screentitle.text = tit;
            }
        }
    }

    public void ListRoomAdd(string name)
    {
        int i = 0;
        for (; i < nrsessions; i++)
        {
            if (sessions[i] != null)
            {
                if (sessions[i].name == name) return;
            }
        }
        i = 0;
        for (; i < nrsessions; i++)
        {
            if (!sessions[i])
                break;
        }
        if (i == nrsessions) return;

        roomline clone = GameObject.Instantiate<roomline>(_roomPrefab);
        clone.Init(_ml, this);
        clone.SetAdvancedInterface(_ml.isInAdvancedInterface);
        clone.goCreatePdf.SetActive(false);
        sessions[i] = clone;

        clone.transform.SetParent(_ml.Rooms.FindInChildren("Content").transform);
        clone.transform.localScale = Vector3.one;
        clone.gameObject.name = name;
        clone.SelectRoom();
    }

    public void AddRoom()
    {
        if (roomname.text != "")
            ListRoomAdd(roomname.text);
        roomname.text = "";
    }

    public void Select(string sessionname)
    {
        for (int i = 0; i < nrsessions; i++)
        {
            if (sessions[i] != null)
            {
                Debug.Log("NAME:"+ sessions[i].name);
                if (sessions[i].name == sessionname)
                {
                    _selectedsession = i;
                    return;
                }
            }
        }        
    }

    public void Remove(string nm, bool delete = false)
    {
        for (int i = 0; i < nrsessions; i++)
        {
            if (sessions[i] != null)
            {
                if (sessions[i].name == nm)
                {
                    if (i == _selectedsession)
                    {
                        selectedtitle.text = "...";
                        _selectedsession = -1;
                    }
                    for (int j = 0; j < maxplayer; j++)
                    {
                        if (allmachines[i, j] != null)
                        {
                            allmachines[i, j].DeleteFromRoom();
                            allmachines[i, j] = null;
                        }
                    }
                    if (delete)
                        GameObject.Destroy(sessions[i].gameObject);
                    sessions[i] = null;
                    break;
                }
            }
        }
    }

    public int GetPlayerIndex(machineline machine)
    {
        if (_selectedsession == -1) return 0;
        for (int j = 0; j < maxplayer; j++)
        {
            if (allmachines[_selectedsession, j] == machine)
                return j;
        }
        return -1;
    }

    public int GetPlayerCountAtTeam(int team)
    {
        if (_selectedsession == -1) return 0;
        int j = 0;
        int n = 0;
        for (; j < maxplayer; j++)
        {
            machineline machine = allmachines[_selectedsession, j];
            if ( machine!= null)
            {
                if (machine.playerTeam == team)
                    n++;
            }
        }
        return n;
    }

    public List<machineline> GetAvailableMachines()
    {
        List<machineline> machines = new List<machineline>();
        if (_selectedsession == -1) return null;
        int j = 0;
        int n = 0;
        for (; j < maxplayer; j++)
        {
            machineline machine = allmachines[_selectedsession, j];
            if (machine != null)
            {
                machines.Add(machine);
            }
        }
        return machines;
    }

    public bool VerifyMaxPlayers(int max)
    {
        if (_selectedsession == -1) return true;
        int j = 0;
        int n = 0;
        for (; j < maxplayer; j++)
        {
            if (allmachines[_selectedsession, j] != null)
                n++;
        }
        Debug.Log("COUNTED:"+n+" reqmax:"+max);
        if (n >= max) return false;
        return true;
    }

    public void AddToSelectedSession(machineline ml)
    {
        if (_selectedsession == -1) return;
        if (ml == null) return;

        for (int j=0; j < maxplayer; j++)
        {
            if (allmachines[_selectedsession, j] == null)
            {
                ml.SetPlayerId(j);
                allmachines[_selectedsession, j] = ml;
                break;
            }
        }

        roomline session = sessions[_selectedsession];
        session.UpdateProduct();
        
        ShowAllMachinesState();
    }

    public void RemoveFromSelectedSession(machineline ml)
    {
        if (_selectedsession == -1) return;
        for (int j = 0; j < maxplayer; j++)
        {
            if (allmachines[_selectedsession, j] == ml)
            {   
                allmachines[_selectedsession, j] = null;
                for (int i = j + 1; i < maxplayer; ++i)
				{
                    machineline machine = allmachines[_selectedsession, i];
                    allmachines[_selectedsession, i - 1] = machine;
                    if (machine != null)
                        machine.SetPlayerId(i - 1);
                }
                break;
            }
        }

        roomline session = sessions[_selectedsession];
        session.UpdateProduct();
        UpdateMe();
        ShowAllMachinesState();
    }

    public void ShowAllMachinesState()
	{
        if (_selectedsession == -1)
        {
            Debug.Log("====== STATE: No Session =====");
            return;
        }
        Debug.Log("====== STATE =====");
        for (int j = 0; j < maxplayer; j++)
        {
            if (allmachines[_selectedsession, j] != null)
            {
                Debug.Log($">>> {j}: {allmachines[_selectedsession, j].reference}");
            }
        }
        Debug.Log("==================");
    }

    public selectedroomline CreateRoomLineFromMachine(roomline session, machineline machine)
	{
        selectedroomline prefab = _ml.isInAdvancedInterface ? _roomLineAdvancedPrefab : _roomLinePrefab;
        selectedroomline srl = GameObject.Instantiate<selectedroomline>(prefab);
        srl.machine = machine;
        srl.reference = machine.reference;
        Transform parent = session.GetContentFromTeam(machine.playerTeam);
        srl.transform.SetParent(parent);
        srl.transform.localScale = Vector3.one;
        srl.SetMachineName(machine.GetMachineName());
        srl.SetPlayerName(machine.playerName);
        srl.SetChangeTeamAvailable(multiplayerlobby.product == multiplayerlobby.Product.TOG); 
        srl.SetPlayerTeam(machine.playerTeam);
        srl.SetMaster(GetPlayerIndex(machine) == 0);
        machine.gameObject.SetActive(false);
        return srl;
    }

    public playerline_tag CreatePlayerLineFromMachine(roomline session, machineline machine)
    {
        playerline_tag prefab = _ml.isInAdvancedInterface ? _playerLineAdvancedPrefab : _playerLinePrefab;
        playerline_tag line = GameObject.Instantiate<playerline_tag>(prefab);
        line.Init(_ml, session, machine.reference);
        line.SetMachineName(machine.GetMachineName());
        line.SetPlayerName(machine.GetFinalPlayerName());
        line.SetMaster(GetPlayerIndex(machine) == 0);
        return line;
    }

    public void UpdateMe()
    {
        if (_selectedsession == -1) return;
        selectedroomline[] allselection = _ml.SelectedGame.GetComponentsInChildren<selectedroomline>();
        foreach (selectedroomline sl in allselection)
            Destroy(sl.gameObject);
        machineline[] allmachine = _ml.Machines.GetComponentsInChildren<machineline>();
        foreach (machineline m in allmachine)
        {
            m.gameObject.SetActive(true);
            m.myselection = null;       // break the link
        }

        roomline session = sessions[_selectedsession]; 

        if (session.isActivated)
        {
            int num = 0;
            for (int j = 0; j < maxplayer; j++)
            {
                machineline machine = allmachines[_selectedsession, j];
                if (machine != null)
                {
                    machine.SetPlayerId(num++);
                    CreateRoomLineFromMachine(session, machine);
                }
            }
            session.UpdateProduct();
        }
    }

    int creationid = -1;
    roomline currentroomline = null;
    public bool StartSession(roomline myroomline)
    {
        if (currentroomline != null) return false;
        Debug.Log("SELECTED:"+_selectedsession);
        if (_selectedsession == -1)
        {
            ShowError(RRLanguageManager.instance.GetString("str_launcher_gameselectfirst"));
            return false;
        }
        int count = 0;
        for (int j = 0; j < maxplayer; j++)
        {
            if (allmachines[_selectedsession, j] != null)
                count++;
        }
        Debug.Log("NR PLAYERS:" + count);

        if (count < _ml.minnrplayers)
        {
            ShowError(RRLanguageManager.instance.GetString("str_launcher_numberofplayers"));
            return false;
        }
        currentroomline = myroomline;
        if (sessions[_selectedsession].isActivated)
        {
            creationid = _selectedsession;
            Debug.Log("Room Selected "+myroomline.currentgamename);
            StartCoroutine(JoinAllSelectedMachinesAndLinkThem(myroomline));
            sessions[_selectedsession].isActivated = false;
            UpdateMe();
        }
        else
        {
// Join this game
        }
        return true;
    }

    IEnumerator RepeatLeaving()
    {
        while (true)
        {
            yield return new WaitForSeconds(5.0f);
            if (!_ml.insideroom) yield break;
            _ml.LeaveRoom();
        }
    }

    private string GetGameName(roomline myroomline)
	{
        string d = multiplayerlobby.DELIMITER;
        return $"{currentroomline.unique_session}{d}{sessions[creationid].name}{d}{myroomline.GetGameData()}";
    }

    IEnumerator JoinAllSelectedMachinesAndLinkThem(roomline myroomline)
    {
        bool first = true;
        yield return null;
        _ml.gametreatment = true;
        Debug.Log("JoinAllSelectedMachinesAndLinkThem true");
        PlayerPrefs.SetInt("PhotonNoSceneLoading",1);
        PlayerPrefs.Save();
        multiplayerlobby.globalwaiter.SetActive(true);

        string d = multiplayerlobby.DELIMITER;

        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();

        while (!PhotonNetwork.IsConnected)
            yield return null;

        string initData = _ml.isInAdvancedInterface ? myroomline.GetInitData(GetAvailableMachines()) : null;

        for (int j = 0; j < maxplayer; j++)
        {
            machineline machine = allmachines[creationid, j];
            if (machine != null)
            {
                if (first)
                {
                    Debug.Log("Creationid:"+creationid);
                    // set nick to command _ roomname
                    Debug.Log("Trying to join master " + machine.reference);
                    //                    ml.SetNickname("create_"+ currentroomline.levelselect.selected_value + "_" + currentroomline.unique_session + "_" + sessions[creationid].name);
                    _ml.SetNickname($"create_0{d}" + GetGameName(myroomline));
                    // join room
                    myroomline.currentgamename = $"{apicalls.id_salle}<game>0{d}{GetGameName(myroomline)}";

                    Debug.Log("Temp game name:"+ myroomline.currentgamename);
                    string roomToJoin = machine.reference;
                    if (!_ml.JoinRoom(roomToJoin))
					{
                        Debug.LogError("Cant join room " + roomToJoin);
                        multiplayerlobby.globalwaiter.SetActive(false);
                        yield break;
                    }
                    while (PhotonNetwork.CurrentRoom == null)
                        yield return null;
                    Debug.Log("Requested join room");
                    while (!_ml.insideroom)                        
                        yield return null;
//                    while (!ml.connected) yield return null;
                    Debug.Log("Inside room now");
                    if (initData != null)
                        _ml.SendInitDataPhotonEvent(initData);
                    yield return new WaitForSeconds(0.5f);
                    _ml.LeaveRoom();
                    while (PhotonNetwork.CurrentRoom != null)
                        yield return null;
                    if (_ml.insideroom)
                        StartCoroutine("RepeatLeaving");
                    Debug.Log("Requested to leave room");
                    while (_ml.insideroom)                        
                        yield return null;
                    Debug.Log("Outside room now");
                    yield return new WaitForSeconds(0.1f);
                    _ml.waitToConnectToMaster = false;
                    while (!_ml.waitToConnectToMaster)
                        yield return null;
                    first = false;
                }
                else
                {
                    _ml.SetNickname($"join_0{d}" + GetGameName(myroomline));
                    Debug.Log("Trying to join client " + machine.reference);
                    // join room
                    Debug.Log("==Requested join room");
                    string roomToJoin = machine.reference;
                    if (!_ml.JoinRoom(roomToJoin))
					{
                        Debug.LogError("Cant join room " + roomToJoin);
                        multiplayerlobby.globalwaiter.SetActive(false);
                        yield break;
                    }
                    while (PhotonNetwork.CurrentRoom == null)
                        yield return null;
                    while (!_ml.insideroom) 
                        yield return null;
                    //                    while (!ml.connected) yield return null;
                    Debug.Log("==Inside room now");
                    if (initData != null)
                        _ml.SendInitDataPhotonEvent(initData);
                    yield return new WaitForSeconds(0.5f);
                    _ml.LeaveRoom();
                    while (PhotonNetwork.CurrentRoom != null)
                        yield return null;
                    if (_ml.insideroom)
                        StartCoroutine("RepeatLeaving");
                    Debug.Log("==Requested to leave room");
                    while (_ml.insideroom) 
                        yield return null;
                    Debug.Log("Outside room now");
                    yield return new WaitForSeconds(0.1f);
                    _ml.waitToConnectToMaster = false;
                    while (!_ml.waitToConnectToMaster)
                        yield return null;
                }
            }
            /*
            PhotonNetwork.Disconnect();
            while (multiplayerlobby.myself.connected) yield return null;
            */
        }
        PlayerPrefs.DeleteKey("PhotonNoSceneLoading");
        PlayerPrefs.Save();
        _ml.SetNickname();
        Debug.Log("JoinAllSelectedMachinesAndLinkThem false");
        _ml.gametreatment = false;
        currentroomline = null;
        multiplayerlobby.globalwaiter.SetActive(false);

        multiplayerlobby.myself.JoinLobby();
    }

    public bool IsThisMachineGaming(string name)
    {
        for (int i = 0; i < nrsessions; i++)
        {
            for (int j = 0; j < maxplayer; j++)
            {
                if (allmachines[i, j] != null)
                {
                    Debug.Log("IsThisMachineGaming " + allmachines[i, j].reference +" "+i+" "+j);
                    if (allmachines[i, j].reference == name)
                        return true;
                }
            }
        }
        return false;
    }

    public void ShowError(string error)
    {
        StartCoroutine(_ShowError(error));
    }
    public IEnumerator _ShowError(string error)
    {
        errormessage.text = error;
        yield return new WaitForSeconds(5.0f);
        errormessage.text = "";
    }

    public static void FinalizeRoom(string name)
    {
        foreach (roomline rl in myself.sessions)
        {
            if (rl.roomName == name)
            {
                Debug.Log("[FINALIZE_ROOM] " + name);
                rl.StopCoroutine("LoopTimer");
                rl.goCreatePdf.SetActive(true);
            }
        }
        myself.ShowAllMachinesState();
    }

    public static void ReActiveRoom(string name)
    {
        foreach(roomline rl in myself.sessions)
        {
            if (rl.roomName == name)
            {
                rl.goCreatePdf.SetActive(false);
                rl.CleanPlayers();
                rl.Activate();
            }
        }
    }

    public IEnumerator JoinToStopSession(string gametostop, bool killProcess = false)
    {
        multiplayerlobby.globalwaiter.SetActive(true);
        //bool first = true;
        yield return null;
        Debug.Log("JoinToStopSession true");

        _ml.gametreatment = true;
        PlayerPrefs.SetInt("PhotonNoSceneLoading", 1);
        PlayerPrefs.Save();

        Debug.Log("Trying to join " + gametostop);
        if (killProcess)
            _ml.SetNickname("_QUITKILL_");
        else
            _ml.SetNickname("_QUIT_");
        // join room
        if (!_ml.JoinRoom(gametostop))
		{
            Debug.LogError("Cant join room " + gametostop);
            multiplayerlobby.globalwaiter.SetActive(false);
            yield break;
        }
        Debug.Log("Requested join room");
        while (PhotonNetwork.CurrentRoom == null)
            yield return null;
        while (!_ml.insideroom)
            yield return null;
        Debug.Log("Inside room now");
        yield return new WaitForSeconds(1f);
        _ml.LeaveRoom();
        while (PhotonNetwork.CurrentRoom != null)
            yield return null;
        if (_ml.insideroom)
            StartCoroutine("RepeatLeaving");
        Debug.Log("Requested to leave room");
        while (_ml.insideroom)
            yield return null;
        Debug.Log("Outside room now");
        yield return new WaitForSeconds(1.0f);

        multiplayerlobby.myself.JoinLobby();

        PlayerPrefs.DeleteKey("PhotonNoSceneLoading");
        PlayerPrefs.Save();

        _ml.gametreatment = false;        
        Debug.Log("JoinToStopSession false");
        EndSession();
        multiplayerlobby.globalwaiter.SetActive(false);

    }

    public void EndSession()
	{
        _ml.SetNickname();
        currentroomline = null;

        if (!sessions[_selectedsession].isActivated)
        {
            sessions[_selectedsession].isActivated = true;
            UpdateMe();
        }
    }
}
