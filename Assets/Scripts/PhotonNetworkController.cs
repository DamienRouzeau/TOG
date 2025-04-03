#define DEBUG_PHOTON_NET_CTRL

using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonNetworkController : MonoBehaviourPunCallbacks
{
    public enum PhotonEvent
    {
        JoinedLobby,
        JoinedRoom,
        JoinRoomFailedFull,
        JoinRoomFailed,
        CreateRoomFailed,
        PlayerEnterRoom,
        PlayerLeaveRoom
    }

    public string roomname = "PierreTestRoom";
    private byte _roomSize = (byte)GameflowBase.nrplayersmax; //Manual set the number of player in the room at one time.

    public bool ready = false;
    public static bool soloMode = false;
    public static PhotonNetworkController myself = null;
    public static GameObject myobj;

    public bool UseLauncher = false;
    private bool _started = false;
    private bool _useLobby = false;

    public delegate void OnPlayerJoinDelegate(Photon.Realtime.Player p);
    public delegate void OnPlayerLeaveDelegate(Photon.Realtime.Player p);
    public delegate void OnPhotonEventCallback(PhotonEvent photonEvent, string msg);

    public OnPlayerJoinDelegate onPlayerJoinDelegate = null;
    public OnPlayerLeaveDelegate onPlayerLeaveDelegate = null;
    public static OnPhotonEventCallback onPhotonEventCallback = null;

    private Dictionary<string, bool> _roomListName = null;

    private void Awake()
    {
        if (myself != null)
        {
            GameObject.DestroyImmediate(gameObject);
        }
        else
        {
            myobj = gameObject;
            myself = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
#if USE_STANDALONE
        StartSolo(false);
#else
        StartMulti(false);
#endif
    }

	private void OnDestroy()
	{
        myself = null;
	}

	private void Reset()
    {
        if (GameflowBase.instance != null)
        {
            Debug.Log("Reset - Destroy flow!");
            DestroySoloOrMulti(GameflowBase.instance.gameObject);
        }
        if (!soloMode)
        {
            LeaveLobby();
            if (PhotonNetwork.CurrentRoom != null)
                PhotonNetwork.LeaveRoom();
            PhotonNetwork.Disconnect();
        }
    }

    public void StartSolo(bool reset = true)
    {
        if (soloMode && GameflowBase.instance != null)
            return;
        Debug.Log("=============== StartSolo " + reset);
        if (reset && _started)
            Reset();
		soloMode = true;
		_started = false;
        StartGame();
    }

    public void StartMultiWithLobby(bool reset = true)
    {
        Debug.Log("=============== StartMulti " + reset);
        _useLobby = true;
        StartMulti(reset);
    }

    public void StartMulti(byte roomSize, string roomName, bool reset = true)
    {
        Debug.Log("=============== StartMulti " + reset);
        _roomSize = roomSize;
        roomname = roomName;
        _useLobby = false;
        StartMulti(reset);
    }

    private void StartMulti(bool reset = true)
    {
        if (reset && _started)
            Reset();
        soloMode = false;
        _started = false;
        if (!UseLauncher)
        {
            StartCoroutine(ConnectToPhoton());
        }
    }

    private IEnumerator ConnectToPhoton()
	{
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();

        while (PhotonNetwork.NetworkClientState != ClientState.Disconnected)
            yield return null;

        PhotonNetwork.ConnectUsingSettings();
    }

    public bool CheckRoomExist(string room, bool needOpen = false)
	{
        if (!string.IsNullOrEmpty(room) && _roomListName != null)
        {
            if (needOpen)
                return _roomListName.ContainsKey(room) && _roomListName[room];
            else
                return _roomListName.ContainsKey(room);
        }
        return false;
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            string name = info.Name;
            if (_roomListName == null)
                _roomListName = new Dictionary<string, bool>();
            if (info.IsVisible && !info.RemovedFromList)
            {
                if (!_roomListName.ContainsKey(name))
                    _roomListName.Add(name, info.IsOpen);
                else
                    _roomListName[name] = info.IsOpen;
            }
            else
            {
                _roomListName.Remove(name);
            }
        }
        if (_roomListName != null)
        {
            Debug.Log("List of rooms:");
            foreach (var elem in _roomListName)
                Debug.Log($" - {elem.Key} : {elem.Value}");
        }
    }

    public int NumberOfPlayers()
    {
        if (soloMode)
        {
            return 1;
        }
        else
        {
            if (PhotonNetwork.CurrentRoom == null)
                return 1;
            int ret = PhotonNetwork.CurrentRoom.PlayerCount;
            //        Debug.Log("NUMBER OF PLAYERS:" + ret);
            return (ret);
        }
    }

    public void JoinLobby()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public void LeaveLobby()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
    }

    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnConnectedToMaster() //Callback function for when the first connection is established successfully.
    {
        if (_useLobby)
        {
            Debug.Log("OnConnectedToMaster -> JoinLobby");
            JoinLobby();
        }
        else
        {
            Debug.Log($"OnConnectedToMaster -> JoinOrCreateRoom {roomname} with size {_roomSize}");
            JoinOrCreateRoom(roomname, _roomSize);
        }
    }

    public void JoinOrCreateRoom(string roomName, byte size)
	{
        if (!UseLauncher)
        {
#if DEBUG_PHOTON_NET_CTRL
            Debug.Log($"[PHOTON_NET_CTRL] JoinOrCreateRoom {roomName} size {size} region {PhotonNetwork.CloudRegion}");
#endif
            PhotonNetwork.AutomaticallySyncScene = false; //Makes it so whatever scene the master client has loaded is the scene all other clients will load

            RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = size };
            PhotonNetwork.JoinOrCreateRoom(roomName, roomOps, null);
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (!UseLauncher)
        {
            string msg = "Failed to join a room (" + returnCode + ") : " + message;
            Debug.Log(msg);
            if (returnCode == ErrorCode.GameFull)
                onPhotonEventCallback?.Invoke(PhotonEvent.JoinRoomFailedFull, msg);
            else
                onPhotonEventCallback?.Invoke(PhotonEvent.JoinRoomFailed, msg);
        }

    }

    public override void OnCreateRoomFailed(short returnCode, string message) //callback function for if we fail to create a room. Most likely fail because room name was taken.
    {
        if (!UseLauncher)
        {
            string msg = "Failed to create room... trying again (" + returnCode + ") : " + message;
            Debug.Log(msg);
            onPhotonEventCallback?.Invoke(PhotonEvent.CreateRoomFailed, msg);
        }
    }

	public override void OnJoinedLobby()
	{
        if (!UseLauncher)
        {
#if DEBUG_PHOTON_NET_CTRL
            Debug.Log("[PHOTON_NET_CTRL] Joined Lobby");
#endif
            onPhotonEventCallback?.Invoke(PhotonEvent.JoinedLobby, null);
        }
    }

	public override void OnJoinedRoom() //Callback function for when we successfully create or join a room.
    {
        if (!UseLauncher)
        {
#if DEBUG_PHOTON_NET_CTRL
            Debug.Log("[PHOTON_NET_CTRL] Joined Room");
#endif
            onPhotonEventCallback?.Invoke(PhotonEvent.JoinedRoom, null);
            StartGame();
        }
    }

    public bool predefinedavatar = false;

    public void StartGame() //Function for loading into the multiplayer scene.
    {
        StartCoroutine(StartPlayerInMultiEnum());
    }

    public static GameObject InstantiateSoloOrMulti(string assetName, Vector3 pos, Quaternion rot, byte group = 0, object[] data = null)
    {
        return InstantiateSoloOrMulti(soloMode, assetName, pos, rot, group, data);
    }

    public static GameObject InstantiateSoloOrMulti(bool solo, string assetName, Vector3 pos, Quaternion rot, byte group = 0, object[] data = null)
    {
        if (solo)
            return GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(assetName), pos, rot);
        else
            return PhotonNetwork.Instantiate(assetName, pos, rot, group, data);
    }

    public static void DestroySoloOrMulti(GameObject go)
    {
        DestroySoloOrMulti(go, soloMode);
    }

    public static void DestroySoloOrMulti(GameObject go, bool solo)
    {
        if (solo)
            GameObject.Destroy(go);
        else
            PhotonNetwork.Destroy(go);
    }

    public static bool IsMaster()
    {
        if (soloMode)
            return true;
        else if (PhotonNetwork.IsConnected)
            return PhotonNetwork.IsMasterClient;
        else
            return true;
    }

    public static int GetPlayerId()
    {
        if (soloMode)
            return 0;
        else if (PhotonNetwork.IsConnected)
            return PhotonNetwork.LocalPlayer.ActorNumber - 1;
        else
            return 0;
    }

    private IEnumerator StartPlayerInMultiEnum()
    {
#if DEBUG_PHOTON_NET_CTRL
        Debug.Log("[PHOTON_NET_CTRL] StartPlayerInMultiEnum");
#endif
        gamesettings_screen gsScreen = gamesettings_screen.myself;

        //yield return new WaitForSeconds(200f);
#if USE_KDK
        string gameflowName = "GameflowKDK";
#elif USE_BOD
        string gameflowName = "GameflowBOD";
#else
        string gameflowName = "gameflowmultiplayer";
#endif
        GameObject obj = InstantiateSoloOrMulti(gameflowName, Vector3.zero, Quaternion.identity);
        GameflowBase.instance = obj.GetComponent<GameflowBase>();
        GameflowBase.instance.ownedobject = true;
        ready = true;

        GameflowBase.isBotScene = GameLoader.myself.isBotScene;
        GameflowBase.simulateLocalBots = GameLoader.myself.simulateLocalBots;
        GameflowBase.localBotCount = GameLoader.myself.localBotCount;
        GameflowBase.localBotsOnSameBoat = GameLoader.myself.localBotsOnSameBoat;

        if (GameflowBase.isBotScene)
        {
            AIBot bot = GameObject.FindObjectOfType<AIBot>();
            GameflowBase.instance.SetAIBot(bot, GameflowBase.myPirateName, 0, false);
            bot.SetGameFlow(GameflowKDK.myself);
        }

        if (GameflowBase.simulateLocalBots)
        {
            int count = Mathf.Clamp(GameflowBase.localBotCount, 0, GameflowBase.nrplayersmax);
            for (int i = 0; i < count; ++i)
            {
                obj = InstantiateSoloOrMulti(gameflowName, Vector3.zero, Quaternion.identity);
                GameflowBase gameflow = obj.GetComponent<GameflowBase>();
                AIBot bot = obj.AddComponent<AIBot>();
                gameflow.SetAIBot(bot, "Bot_" + (i + 1).ToString("00"), i + 1, true);
                bot.SetGameFlow(gameflow);
            }
        }

        while (Player.myplayer == null)
            yield return null;

        while (gsScreen.faderunning)
            yield return null;

        if (!soloMode)
            Player.myplayer.StartMultiPlayer();

#if !USE_STANDALONE
        if (GameLoader.myself.multiplayerUseLauncher)
        {
            while (!(multiplayerlobby.myself.icreated || multiplayerlobby.myself.insideroom))
                yield return null;

            if (multiplayerlobby.myself.icreated)
                VoiceManager.myself?.InitWithPhotonRoom(multiplayerlobby.myself.lastCreatedRoomName);
            else
                VoiceManager.myself?.InitWithPhotonRoom(multiplayerlobby.myself.lastJoinedRoomName);
        }
        else
        {
            VoiceManager.myself?.InitWithPhotonRoom(roomname);
        }
#endif

        _started = true;
    }

#if USE_STANDALONE
 //   public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
	//{
 //       Debug.Log($"OnMasterClientSwitched newMasterClient {newMasterClient.ActorNumber}");
 //       PhotonNetwork.LeaveRoom();
	//}
#endif

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player p)
    {
        if (!_started)
            return;

        onPhotonEventCallback?.Invoke(PhotonEvent.PlayerEnterRoom, p.ActorNumber.ToString());
        onPlayerJoinDelegate?.Invoke(p);

        if ((p != null) && (p.NickName != null))
        {
            if (p.NickName.StartsWith("_QUIT_"))
            {
#if DEBUG_PHOTON_NET_CTRL
                Debug.Log("[PHOTON_NET_CTRL] QUIT RECEIVED");
#endif
                GameflowBase.instance.QuitRace();
            }
            else if (p.NickName.StartsWith("_QUITKILL_"))
            {
#if DEBUG_PHOTON_NET_CTRL
                Debug.Log("[PHOTON_NET_CTRL] QUIT & KILL RECEIVED");
#endif
                GameflowBase.instance.QuitRace(true);
            }
            else if (p.NickName.StartsWith("_PAUSE_"))
            {
#if DEBUG_PHOTON_NET_CTRL
                Debug.Log("[PHOTON_NET_CTRL] PAUSE RECEIVED");
#endif
                if (!GameflowBase.instance.pausedplayer)
                {
                    string ttmp = p.NickName.Replace("_PAUSE_", "");
                    if (ttmp.Contains(apicalls.machineid))
                    {
                        GameflowBase.instance.pausedplayer = true;
                        apicalls.SetPause();
                    }
                }
            }
            else if (p.NickName.StartsWith("_UNPAUSE_"))
            {
#if DEBUG_PHOTON_NET_CTRL
                Debug.Log("[PHOTON_NET_CTRL] RESUME RECEIVED");
#endif
                if (GameflowBase.instance.pausedplayer)
                {
                    string ttmp = p.NickName.Replace("_UNPAUSE_", "");
                    if (ttmp.Contains(apicalls.machineid))
                    {
                        GameflowBase.instance.pausedplayer = false;
                        apicalls.ResumePause();
                    }
                }
            }
            else if (p.NickName.StartsWith("_RESPAWN_"))
            {
#if DEBUG_PHOTON_NET_CTRL
                Debug.Log("[PHOTON_NET_CTRL] RESPAWN RECEIVED");
#endif
                string ttmp = p.NickName.Replace("_RESPAWN_", "");
                if (ttmp.Contains(apicalls.machineid))
                {
                    Player.myplayer.playerBody.AddDamage(10000f);
                }
            }
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player p)
    {
        if (!_started)
            return;

        onPlayerLeaveDelegate?.Invoke(p);
        onPhotonEventCallback?.Invoke(PhotonEvent.PlayerLeaveRoom, p.ActorNumber.ToString());
    }
}
