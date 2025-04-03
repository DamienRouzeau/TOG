//#define DEBUG_TELEPORT
//#define DEBUG_WEAPON
//#define PLAYER_CAN_TELEPORT_OUTSIDE_BOAT

using DynamicFogAndMist;
using RootMotion.FinalIK;
using RootMotion.Demos;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.XR;

public class Player : MonoBehaviour, Player.IPlayer
{
	#region Interfaces

    public interface IPlayer
    {
        int id { get; }
        boat_followdummy GetBoat();
        bool isInPause { get; }
        int GetTeam();
        bool isDead { get; }

        GameObject goRoot { get; }
    }

	#endregion

	#region Enums
	public enum WeaponType
    {
        None,
        Musket,
        TOG_Biggun,
        Canon_Torche,
        Dagger,
#if USE_KDK || USE_BOD
        KDK_Hands,
        KDK_MusketElectic,
#endif
        COUNT
    }

    public enum WeaponPlace
    {
        None,
        LeftHand,
        RightHand,
        LeftBelt,
        RightBelt,
        FrontBelt,
        BackBelt
    }

    public enum PlayerEvent
	{
        Teleport
	}

	#endregion

	#region Structs

    public struct WeaponData
	{
        public WeaponType weaponType;
        public WeaponPlace weaponPlace;

        public WeaponData(WeaponType wtype, WeaponPlace wplace)
		{
            weaponType = wtype;
            weaponPlace = wplace;
        }
    }

    #endregion

    #region Delegates

    public delegate void OnPlayerEvent(PlayerEvent evt, object data);

	#endregion

    // SYNCED DATAS
    public float respawnTimeMax = 3f;
    public float health = 100;
    public int kills = 0;
    public float healthRegeneration = 1f;
    public float secondsBeforeHealthRegeneration = 1f;
    public bool theRamboVariable = false; // False == 1 gun in the hand at the start, and can't be thrown. Can't have more gun if he has 2 guns in inventory + 1 gun in gun /// true == Rambo mode, no gun in hand at the beginning, but unlimited slot storage, can have much guns ah he can hold.
    private bool respawn = false;
    public bool isDead { private set; get; }

    [Header("UI")]
    public GameObject deathDisplay;
    public Text respawnTimeText;
    public Text killsText;
    public Image healthSlider;
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI lifeText;
    public TextMeshProUGUI accuracyText;
    public TextMeshProUGUI upgradeGoldText;
    public TextMeshProUGUI upgradeCostText;
    public TextMeshProUGUI upgradeNameText;
    public TextMeshProUGUI upgradeTitleText;
    public TextMeshProUGUI timeText;
    public GameObject upgradeChecked;
    public GameObject upgradeNoCash;
    public Image upgradeImage;
    public GameObject upgradeGoldRoot;
    public GameObject upgradeFrame;
    public Animator upgradeAnimator;
    public string upgradeRecoveryTextId = "str_kdk_recovery01";
    public UI_AntennaLife[] antennaLifes = null;
    public UI_LifeRegenStatus lifeRegenStatus = null;

    [Header("Player composants")]
    public GameObject[] skins;
    public PlayerBody playerBody;
    public GameObject playerBodyObject;
    public Camera cam;
    public GameObject playerHead;
    public GameObject hat;
    public GameObject body;
    public Transform camOffset;
    public GameObject bodyTuto => _currentSkinPlayer != null ? _currentSkinPlayer.skinPlayerBodyTuto : null;
    //    public List<GameObject> objectsSlots;
    public AmplifyColorEffect fadeEffect = null;
    public GameObject impactEffect;
    public GameObject weaponByDefault;

    [Header("Gameplay loop")]
    private float actualRespawnTime;


    [Header("Controllers")]
    public GameObject leftController;
    public GameObject rightController;
    public Controllers _leftController;
    public Controllers _rightController;

    public bool isPlayerSpawned => _startPoint != null;

    private Transform _startPoint;

    public int team => _team;
    private int _team = -1;

    public int id => _id;
    private int _id = -1;
    public GameObject goRoot => gameObject;

    private GameObject _deadPirate;
    private int _oldKills;
    private float _lastHitTime = 0f;

    public static Player myplayer = null;
    public static pointfromhand pointleft = null;
    public static pointfromhand pointright = null;
    public static bool fixedPosition = false;
    public static bool magnet = false;

    [Header("Hand and Belt Inventory")]
    public List<attachobject> left_hand_objects = null;
    public List<attachobject> right_hand_objects = null;
    public List<attachobject> belt_left_objects = null;
    public List<attachobject> belt_right_objects = null;
    public List<attachobject> belt_front_objects = null;
    public List<attachobject> belt_back_objects = null;

    public GameObject leftMallet = null;
    public GameObject rightMallet = null;

    public string[] deathprefabnames;
    public GameObject guardian_appearance = null;

    public List<GameObject> NonPlayersPool;
    public GameObject currentskin;
    public float starthealth;

    public Player_avatar[] avatars => _avatars;
    private Player_avatar[] _avatars = null;

    public VRIK vrik => _vrik;
    private VRIK _vrik = null;

    public bool isInPause => _isInPause;
    private bool _isInPause = false;

    public bool isTeleporting => _isTeleporting;
    public bool canTeleport => _canTeleport && !_isTeleporting;
    public Transform forcedTeleportTarget => _forcedTeleportTarget;

    public bool hitmyself = false;

    [Header("All UIs")]
    [SerializeField]
    private Canvas _uiCanvas = null;

    public Canvas uiCanvas => _uiCanvas;

    [Header("UI Targets")]
    [SerializeField]
    private UI_FollowTarget _targetPrefab = null;
    [SerializeField]
    private Transform _targetRoot = null;
    [SerializeField]
    private UI_FollowTarget _allyTargePrefab = null;
    [SerializeField]
    private UI_FollowTarget _goalsTargetPrefab = null;
    [SerializeField]
    private UI_FollowTarget _archivesTargetPrefab = null;
    [SerializeField]
    private UI_FollowTarget _scientistTargetPrefab = null;
    [Header("Minimap")]
    [SerializeField]
    private UI_PlayerMapPosition _miniMapPrefab = null;
    [SerializeField]
    private GameObject _miniMapGoalPrefab = null;

    private List<UI_FollowTarget> _followTargets = null;

    [Header("UI Life")]
    [SerializeField]
    private UI_PlayerLife _uiLife = null;


    [Header("Goals")]
    [SerializeField]
    private UI_PlayerGoals _uiGoals = null;

    bool _isTeleporting = false;
    bool _canTeleport = true;

    private Transform _forcedTeleportTarget = null;

    Vector3 playerstartposition;

    GameObject head_src = null;

    GameObject body_dst;
    GameObject head_dst;
    GameObject left_dst;
    GameObject right_dst;

    private UI_PlayerMapPosition _miniMap = null;
    private float _miniMapScale = 1f;

    public ProjectileCannon gun => _gun;

    private SkinPlayer _currentSkinPlayer = null;
    private GameObject _uiFeedback = null;
    private Transform _uiFeedbackAnchor = null;
    private ProjectileCannon _gun = null;
    private float _lastUiFeedbackDistance = 0f;

    public int currentLeftGunUpgrade => _currentLeftGunUpgrade;
    private int _currentLeftGunUpgrade = 0;
    public int currentRightGunUpgrade => _currentRightGunUpgrade;
    private int _currentRightGunUpgrade = 0;

    public float heightOfEyes => _heightOfEyes;
    private float _heightOfEyes = 1.6f;
    private int _averageCounterForHeightOfEyes = 1;

    // Weapons
    private List<WeaponData> _weaponDataList = null;
    private Dictionary<WeaponPlace, List<attachobject>> _attachObjectDic = null;
    private WeaponType _lastWeaponLeft = WeaponType.None;
    private WeaponType _lastWeaponRight = WeaponType.None;

    // Events
    public OnPlayerEvent onPlayerEvent = null;

    [Header("Shaders")]
    // Body alpha shader
    public Shader bodyAlphaShader = null;
    private Material _oldBodyMaterial = null;
    private SkinnedMeshRenderer _skinBody = null;

    [Header("Events")]
    [SerializeField]
    private PlayEventCommand _eventCommand = null;

    [Header("Long Teleport")]
    [SerializeField]
    private GameObject _longTeleportFX = null;
    [SerializeField]
    private float _longTeleportDuration = 1f;
    [SerializeField]
    private float _longTeleportFadeDuration = 0.07f;

    public string customHat => _customHat;
    private string _customHat = null;

    public bool isInit => _isInit;
    private bool _isInit = false;

    public int teamGold => _teamGoldCollected - _teamGoldSpent;
    public int collectedTeamGold => _teamGoldCollected;
    private int _teamGoldCollected = 0;
    private int _teamGoldSpent = 0;

    public int score => _score;
    private int _score = 0;

    public string currentSkinName => _currentSkinName;
    private string _currentSkinName = null;

    public float accuracy => _accuracy;
    private float _accuracy = 0f;

    public string progress => _progress;
    private string _progress = "";

    private Vector3 _oldLocalPos = Vector3.zero;
    private float _magnetTime = 0f;
    private Vector3 _magnetDir = Vector3.zero;
    private bool _areGunsHidden = false;

    public Transform virtualWorldOrigin; // Point de référence dans l'environnement virtuel
    public Transform xrRig; // L'objet représentant le joueur (XR Rig)

    private Transform initialHeadsetPosition;


    void Awake()
    {
        myplayer = this;
        starthealth = health;
        NonPlayersPool = new List<GameObject>();
        _avatars = new Player_avatar[GameflowBase.nrplayersmax-1];
        GameObject avatarPrefab = gamesettings_player.myself.avatarPrefab;
        for (int i = 0; i < _avatars.Length; i++)
        {
            GameObject avatar = Instantiate(avatarPrefab, gamesettings_player.myself.transform);
            if (avatar != null)
            {
                VRIK[] allvrik = avatar.GetComponentsInChildren<VRIK>();
                foreach (VRIK vrik in allvrik)
                {
                    vrik.gameObject.SetActive(false);
                }
                avatar.SetActive(false);
                NonPlayersPool.Add(avatar);
                avatar.name = "NotUsedAvatar";
                _avatars[i] = avatar.GetComponent<Player_avatar>();
            }
        }
    }
    
    void Start()
    {
        SkinSetting();
        playerstartposition = gameObject.transform.position;
        ActualPlayerInitialization();
        GameflowBase.onRaceEventDelegate += OnRaceEvent;
#if USE_KDK
        TowerDefManager.onLifeRegenState += OnLifeRegenState;
#endif
        if (goldText != null)
            goldText.text = "0";
        if (scoreText != null)
            scoreText.text = "0";
        if (lifeText != null)
            lifeText.text = "100%";
        if (accuracyText != null)
            accuracyText.text = "100%";
        if (timeText != null)
            timeText.text = "00:00:00";

        //AdjustPlayerSpawn();
    }

    void AdjustPlayerSpawn()
    {
        // Calcul du décalage entre le monde réel et virtuel
        Vector3 offset = initialHeadsetPosition.position - virtualWorldOrigin.position;

        // Applique cet offset au joueur dans le jeu
        xrRig.position = virtualWorldOrigin.position + offset;
    }

    void OnDestroy()
	{
        GameflowBase.onRaceEventDelegate -= OnRaceEvent;
#if USE_KDK
        TowerDefManager.onLifeRegenState -= OnLifeRegenState;
#endif
        Debug.Log("Player is killed and was in " + transform.parent);
        if (GetComponentInParent<StarterHub>() != null)
		{
            Debug.LogError("Player is killed and was in avatar_root type " + GetComponentInParent<StarterHub>().hubType);
        }
        myplayer = null;
        _currentSkinPlayer = null;
	}

#if USE_KDK
    private void OnLifeRegenState(float oldVal, float newVal, float maxVal)
    {
        //Debug.Log($"[REGENERATOR] OnLifeRegenState {oldVal} {newVal} {maxVal}");
        if (lifeRegenStatus != null && lifeRegenStatus.gameObject.activeInHierarchy)
        {
            if (oldVal / maxVal > 0.75f && newVal / maxVal < 0.75f)
            {
                lifeRegenStatus.ShowLifeRegen(UI_LifeRegenStatus.LifeRegenStep.STEP_75);
            }
            else if (oldVal / maxVal > 0.5f && newVal / maxVal < 0.5f)
            {
                lifeRegenStatus.ShowLifeRegen(UI_LifeRegenStatus.LifeRegenStep.STEP_50);
            }
            else if (oldVal / maxVal > 0.25f && newVal / maxVal < 0.25f)
            {
                lifeRegenStatus.ShowLifeRegen(UI_LifeRegenStatus.LifeRegenStep.STEP_25);
            }
            else if (oldVal > 0f && newVal <= 0f)
            {
                lifeRegenStatus.ShowLifeRegen(UI_LifeRegenStatus.LifeRegenStep.STEP_0);
            }
        }
    }
#endif

    public void ResetGoldAndScore()
	{
        _teamGoldCollected = 0;
        _teamGoldSpent = 0;
        _score = 0;
        UpdateGoldAndScoreTexts();
    }

    public void SetCollectedTeamGold(int gold)
	{
        if (_teamGoldCollected != gold)
        {
            _teamGoldCollected = gold;
            UpdateGoldAndScoreTexts();
        }
	}

    private void UpdateGoldAndScoreTexts()
	{
        if (goldText != null)
            goldText.text = teamGold.ToString();
        if (scoreText != null)
            scoreText.text = _score.ToString();
    }

	public void Reset()
	{
        right_dst = null;
        left_dst = null;
        body_dst = null;
        head_src = null;
        head_dst = null;
        _vrik = null;
        ResetGoldAndScore();
        ResetGuns();
        SkinSetting();
        ActivateMallets();
        PrepareToLobby();
        ResetHUD();
    }

    public void PrepareToLobby()
	{
        // Activate body tuto
        bodyTuto.SetActive(true);
        // Reset Fog
        DynamicFog fog = cam.GetComponent<DynamicFog>();
        if (fog != null)
            fog.profile.Load(fog);
    }

    public static int GetPlayerIdxInRoom()
	{
        if (PhotonNetworkController.soloMode)
            return 0;
        int idx = 0;
        foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerList)
        {
            if (p.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                return idx;
            }
            idx++;
		}
        return idx;
	}

    public void SetStartSpawnPoint(Transform sp)
	{
        _startPoint = sp;
        if (sp != null)
            playerstartposition = sp.position;
	}

    public void InitTeam(int team)
    {
        _team = team;
        if (!_isInPause)
            _vrik = gameObject.GetComponentInChildren<VRIK>();
    }

    public void RemoveDagger()
	{
        List<attachobject> attachobjects = new List<attachobject>();
        attachobjects.AddRange(left_hand_objects);
        attachobjects.AddRange(right_hand_objects);
        foreach (attachobject attach in attachobjects)
		{
            if (attach.gameObject.activeInHierarchy && attach.gameObject.name == WeaponType.Dagger.ToString())
                attach.gameObject.SetActive(false);
        }
    }

    public void SetInPause(bool pause)
	{
        _isInPause = pause;
        _vrik.gameObject.SetActive(!pause);
        if (pause)
            gamesettings_screen.myself.FadeOut(0.5f);
        else
            gamesettings_screen.myself.FadeIn();

        if (GameflowBase.instance?.aiBot != null)
            GameflowBase.instance.aiBot.SetInPause(pause);
    }

    public void CreateMinimap()
	{
        if (_miniMap == null && _miniMapPrefab != null)
        {
            Transform parent = _vrik.references.leftHand;
            _miniMap = GameObject.Instantiate<UI_PlayerMapPosition>(_miniMapPrefab, parent);
            _miniMap.SetScale(_miniMapScale);
            _miniMap.gameObject.SetActive(false);
        }
    }

    public void ShowHideMiniMap()
	{
        if (_miniMap != null)
            _miniMap.gameObject.SetActive(!_miniMap.gameObject.activeSelf);
	}

    public void SetMiniMapHandMove(float move)
    {
        _miniMapScale = Mathf.Clamp(_miniMapScale + move * 2f, 0.5f, 1.5f);
        if (_miniMap != null)
            _miniMap.SetScale(_miniMapScale);
    }

    public void ArchiveA2()
    {
        if (_miniMap != null)
            _miniMap.GetComponent<ChangeObjectiveMiniMapBOD>().ArchiveA2();
    }
    public void ArchiveB1()
    {
        if (_miniMap != null)
            _miniMap.GetComponent<ChangeObjectiveMiniMapBOD>().ArchiveB1();
    }
    public void ArchiveB2()
    {
        if (_miniMap != null)
            _miniMap.GetComponent<ChangeObjectiveMiniMapBOD>().ArchiveB2();
    }
    public void ArchiveC()
    {
        if (_miniMap != null)
            _miniMap.GetComponent<ChangeObjectiveMiniMapBOD>().ArchiveC();
    }
    public void ArchiveAnomalie()
    {
        if (_miniMap != null)
            _miniMap.GetComponent<ChangeObjectiveMiniMapBOD>().ArchiveAnomalie();
    }



    private void ActualPlayerInitialization()
    {
        Debug.Log("ActualPlayerInitialization");
        actualRespawnTime = respawnTimeMax;
        killsText.enabled = false;
        cam.enabled = true;
        
        pointfromhand[] hands = gameObject.GetComponentsInChildren<pointfromhand>();
        if (hands[0].righthand)
        {
            pointright = hands[0];
            pointleft = hands[1];
        }
        else
        {
            pointright = hands[1];
            pointleft = hands[0];
        }
        if (pointleft != null)
        {
            pointleft.my_hand_objects = left_hand_objects;
            pointleft.InitTeleporterTarget(avatar_root.myself);
        }
        if (pointright != null)
        {
            pointright.my_hand_objects = right_hand_objects;
            pointright.InitTeleporterTarget(avatar_root.myself);
        }
        _isInit = true;
    }

    public bool HasDaggerInRightHand()
	{
        foreach (var attach in right_hand_objects)
        {
            if (attach.gameObject.name == WeaponType.Dagger.ToString() && attach.gameObject.activeInHierarchy)
                return true;
        }
        return false;
    }

    public bool HasDaggerInLeftHand()
    {
        foreach (var attach in left_hand_objects)
        {
            if (attach.gameObject.name == WeaponType.Dagger.ToString() && attach.gameObject.activeInHierarchy)
                return true;
        }
        return false;
    }

    public void InitTeleporterTarget(avatar_root avatarRoot)
    {
        if (pointleft != null)
        {
            pointleft.InitTeleporterTarget(avatarRoot);
        }
        if (pointright != null)
        {
            pointright.InitTeleporterTarget(avatarRoot);
        }
    }

    public void TeleportOnSpawnPoint()
	{
        if (StarterHub.myself != null)
        {
#if DEBUG_TELEPORT
            Debug.Log("[TELEPORT] TeleportOnSpawnPoint type : " + StarterHub.myself.hubType);
#endif

            if (PhotonNetworkController.soloMode && StarterHub.myself.hubType == StarterHub.HubType.BeforeRace)
            {
                // Dont search spawnpoint, use default
                Teleport(_startPoint.transform.position, _startPoint.transform.rotation);
                return;
            }

            spawnpoint[] _startPoints = StarterHub.myself.GetComponentsInChildren<spawnpoint>(true);

            if (PhotonNetworkController.soloMode && StarterHub.myself.hubType == StarterHub.HubType.Cabin && _startPoints.Length > 1)
            {
                if (SaveManager.myself.profileCount > 0)
                    _startPoint = _startPoints[0].transform;
                else
                    _startPoint = _startPoints[1].transform;
                Teleport(_startPoint.transform.position, _startPoint.transform.rotation);
                return;
            }

            int spawnIdx = 0;
			if (PhotonNetwork.IsConnectedAndReady)
			{
				//spawnIdx = Player.GetPlayerIdxInRoom();
			}
#if DEBUG_TELEPORT
            Debug.Log("[TELEPORT] TeleportOnSpawnPoint : " + spawnIdx);
#endif
			foreach (spawnpoint sp in _startPoints)
            {
                if (sp.id == spawnIdx)
                {
#if DEBUG_TELEPORT
                    Debug.Log("[TELEPORT] TELEPORT TO SPAWN");
#endif
                    Teleport(sp.transform.position, sp.transform.rotation);
                    _startPoint = sp.transform;
                    break;
                }
            }
        }
    }

    public void StartMultiPlayer()
    {   
        //        multiplayer
        right_dst = AddMultiPlayerItem(gameObject.name + "_r_", rightController, true); 
        left_dst = AddMultiPlayerItem(gameObject.name + "_l_", leftController, true);
        body_dst = AddMultiPlayerItem(gameObject.name + "_b_", gameObject);
        head_src = cam.gameObject;
        head_dst = AddMultiPlayerItem(gameObject.name + "_h_", head_src, true);
        SetName();
    }

    public bool UnlinkPlayers()
    {
        Debug.Log("UnlinkPlayers was in " + transform.parent);
        // Our player 
        if (transform.parent != null)
            transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        // TeleportTarget
        if (pointfromhand.teleporttarget != null)
        {
            pointfromhand.teleporttarget.transform.SetParent(null);
            DontDestroyOnLoad(pointfromhand.teleporttarget);
        }
        // Other players
        foreach (Player_avatar avatar in _avatars)
        {
            if (avatar.actornumber >= 0)
            {
                if (avatar.transform.parent != null)
                    avatar.transform.SetParent(null);
                avatar.gameObject.SetActive(false);
                DontDestroyOnLoad(avatar.gameObject);
            }
        }
        return true;
    }

    GameObject AddMultiPlayerItem(string name, GameObject obj, bool smooth=false)
    {
        int playerNum = PhotonNetworkController.GetPlayerId();
        string goName = name + playerNum;
        GameObject dummyobject = PhotonNetworkController.myobj.transform.Find(goName)?.gameObject;
        if (dummyobject != null)
            return dummyobject;
        object[] instanceData = new object[1];
        instanceData[0] = "{ \"nr\":" + playerNum + ", \"name\":\"" + name + "\"}";
        string prefabName = smooth ? "mp_dum_smooth" : "mp_dum";
        dummyobject = PhotonNetworkController.InstantiateSoloOrMulti(prefabName, obj.transform.position, obj.transform.rotation, 0, instanceData);
        Debug.Log(">>>" + instanceData[0] + " " + PhotonNetworkController.myobj.name);
        dummyobject.name = goName;
        dummyobject.transform.SetParent(PhotonNetworkController.myobj.transform);
        return (dummyobject);
    }

    public void ActivateMallets()
    {
        //if (leftMallet != null && !leftMallet.activeSelf)
        //    leftMallet.SetActive(true);
        //if (rightMallet != null && !rightMallet.activeSelf)
        //    rightMallet.SetActive(true);
        DeactivateMallets(); // Don't use mallet anymore
    }

    public void DeactivateMallets()
    {
        if (leftMallet != null && leftMallet.activeSelf)
            leftMallet.SetActive(false);
        if (rightMallet != null && rightMallet.activeSelf)
            rightMallet.SetActive(false);
    }

    public GameObject ForceSkin(string name, bool force=false)
    {
        Debug.Log($"ForceSkin:[{name}] force {force}");

        _currentSkinName = name;

        GameObject skin = null;
        GameObject active = null;

        foreach (GameObject sk in skins)
        {
            if (sk.activeInHierarchy)
            {
                active = sk;
            }
        }

        if (!force && GameflowBase.GetMySkinName() == name)
            return active;

        GameflowBase.SetMySkin(name);

        foreach (GameObject sk in skins)
        {
            if (sk.name.Contains(name))
            {
                skin = sk;
                sk.SetActive(true);

                _vrik = sk.GetComponentInChildren<VRIK>();
                _vrik.solver.Reset();
                AdaptScaleToCurrentSkin();
                
                Debug.Log("SKIN VRIK " + _vrik.name); 
                
                avatar_vrik_corrector vrikCorrector = GetComponent<avatar_vrik_corrector>();
                if (vrikCorrector != null)
                    vrikCorrector.SetVRIK(_vrik);

                player_vrik_corrector playerVrikCorrector = GetComponent<player_vrik_corrector>();
                if (playerVrikCorrector != null)
                    playerVrikCorrector.SetVRIK(_vrik);

                left_hand_objects = new List<attachobject>();
                right_hand_objects = new List<attachobject>();
                belt_left_objects = new List<attachobject>();
                belt_right_objects = new List<attachobject>();
                belt_front_objects = new List<attachobject>();
                belt_back_objects = new List<attachobject>();

                if (_attachObjectDic == null)
                    _attachObjectDic = new Dictionary<WeaponPlace, List<attachobject>>();
                else
                    _attachObjectDic.Clear();
                _attachObjectDic.Add(WeaponPlace.LeftHand, left_hand_objects);
                _attachObjectDic.Add(WeaponPlace.RightHand, right_hand_objects);
                _attachObjectDic.Add(WeaponPlace.LeftBelt, belt_left_objects);
                _attachObjectDic.Add(WeaponPlace.RightBelt, belt_right_objects);
                _attachObjectDic.Add(WeaponPlace.FrontBelt, belt_front_objects);
                _attachObjectDic.Add(WeaponPlace.BackBelt, belt_back_objects);

                foreach (attachobject att in sk.GetComponentsInChildren<attachobject>(true))
                {
                    if (att.gameObject.transform.parent.gameObject.name.Contains(" L "))
                        left_hand_objects.Add(att);
                    else
                    {
                        if (att.gameObject.transform.parent.gameObject.name.Contains(" R "))
                            right_hand_objects.Add(att);
                        else
                        {
                            if (att.gameObject.transform.parent.gameObject.name.Contains("Left"))
                                belt_left_objects.Add(att);
                            else
                            {
                                if (att.gameObject.transform.parent.gameObject.name.Contains("Right"))
                                    belt_right_objects.Add(att);
                                else
                                {
                                    if (att.gameObject.transform.parent.gameObject.name.Contains("Front"))
                                        belt_front_objects.Add(att);
                                    else
                                        belt_back_objects.Add(att);
                                }
                            }
                        }
                    }
					att.gameObject.SetActive(att.displayfromstart);
				}

                if (pointleft) pointleft.my_hand_objects = left_hand_objects;
                if (pointright) pointright.my_hand_objects = right_hand_objects;

                if (active)
                {
                    attachobject[] initialattach = active.GetComponentsInChildren<attachobject>(true);
                    attachobject[] todoattach = sk.GetComponentsInChildren<attachobject>(true);
                    foreach (attachobject att in initialattach)
                    {
                        string cont = "";
                        if (att.gameObject.transform.parent.gameObject.name.Contains(" L ")) cont = " L ";
                        else
                        {
                            if (att.gameObject.transform.parent.gameObject.name.Contains(" R ")) cont = " R ";
                            else
                            {
                                if (att.gameObject.transform.parent.gameObject.name.Contains("Left")) cont = "Left";
                                else
                                {
                                    if (att.gameObject.transform.parent.gameObject.name.Contains("Right")) cont = "Right";
                                    else
                                    {
                                        if (att.gameObject.transform.parent.gameObject.name.Contains("Front")) cont = "Front";
                                        else cont = "Back";
                                    }
                                }
                            }
                        }
                        foreach (attachobject todo in todoattach)
                        {
                            if (todo.gameObject.name == att.gameObject.name)
                            {
                                string comp = "";
                                if (todo.gameObject.transform.parent.gameObject.name.Contains(" L ")) comp = " L ";
                                else
                                {
                                    if (todo.gameObject.transform.parent.gameObject.name.Contains(" R ")) comp = " R ";
                                    else
                                    {
                                        if (todo.gameObject.transform.parent.gameObject.name.Contains("Left")) comp = "Left";
                                        else
                                        {
                                            if (todo.gameObject.transform.parent.gameObject.name.Contains("Right")) comp = "Right";
                                            else
                                            {
                                                if (todo.gameObject.transform.parent.gameObject.name.Contains("Front")) comp = "Front";
                                                else comp = "Back";
                                            }
                                        }
                                    }
                                }
                                if (comp == cont)
                                {
                                    todo.gameObject.SetActive(att.gameObject.activeInHierarchy);
                                    break;
                                }
                            }
                        }
                    }
                }
                break;
            }
        }

        UpdateWeaponsData();

        foreach (GameObject sk in skins)
        {
            if (sk.name.Contains(name))
            {
            }
            else
            {
                sk.SetActive(false);
                //                Destroy(sk);
            }
        }

        // Revert shader
        UpdateBodyShaderAlpha(false, true);

        SkinPlayer sP = skin.GetComponent<SkinPlayer>();
        currentskin = skin;
        playerBody = sP.skinPlayerBodyScript;
        playerBodyObject = sP.skinPlayerBodyObject;
        playerHead = sP.skinPlayerHead;
        hat = sP.skinPlayerHat;
        body = sP.skinPlayerBody;
        _currentSkinPlayer = sP;
        // Activate body tuto
        bodyTuto.SetActive(true);

        UpdateThemeMaterial();

        return (skin);
    }

    public void SetId(int id)
	{
        _id = id;
	}

    public void UpdateThemeMaterial()
	{
        PlayerChangeMaterial[] changeMats = gameObject.GetComponentsInChildren<PlayerChangeMaterial>();
        if (changeMats != null && changeMats.Length > 0)
        {
            foreach (PlayerChangeMaterial changeMat in changeMats)
                changeMat.UpdateMaterial();
        }
    }

    public void UpdateCustomTheme(GameObject prefab = null)
    {
        ThemeReplaceObjects[] replaceObjs = gameObject.GetComponentsInChildren<ThemeReplaceObjects>(true);
        if (replaceObjs != null && replaceObjs.Length > 0)
        {
            int layerToSet = LayerMask.NameToLayer("TransparentFX");
            foreach (ThemeReplaceObjects replaceObj in replaceObjs)
            {
                replaceObj.SetPrefabForTheme(multiplayerlobby.SkinTheme.Custom, prefab);
                replaceObj.ReplaceObjects(force:true, layer: layerToSet);
                replaceObj.autoStart = false;
            }
        }
        ThemeEnableObjects[] enableObjs = gameObject.GetComponentsInChildren<ThemeEnableObjects>(true);
        if (enableObjs != null && enableObjs.Length > 0)
		{
            foreach (ThemeEnableObjects enableObj in enableObjs)
            {
                enableObj.SetTheme(multiplayerlobby.theme);
            }
        }
    }

    private void SkinSetting()
    {
        gamesettings_player gsPlayer = gamesettings_player.myself;
        deathprefabnames = new string[gsPlayer.deathprefabnames.Length];
        for (int ii = 0; ii < gsPlayer.deathprefabnames.Length; ii++)
            deathprefabnames[ii] = gsPlayer.deathprefabnames[ii];
        string name = gsPlayer.GetSkinName(0);
        ForceSkin(name, true);
    }

    private bool IsWeaponAttached(attachobject attach, WeaponType weaponType)
    {
        return attach.gameObject.name == weaponType.ToString();
    }

    public bool IsWeaponActiveInList(List<attachobject> attachList, WeaponType weaponType)
    {
        foreach (var attach in attachList)
        {
            if (attach.gameObject.activeSelf)
            {
                if (IsWeaponAttached(attach, weaponType))
                    return true;
            }
        }
        return false;
    }

    public void SetWeaponActiveInList(List<attachobject> attachList, WeaponType weaponType, bool active)
    {
        if (weaponType == WeaponType.None || weaponType == WeaponType.COUNT)
            return;
        foreach (var attach in attachList)
        {
            if (IsWeaponAttached(attach, weaponType))
            {
                attach.gameObject.SetActive(active);
            }
        }
        UpdateWeaponsData();
    }

    public void UpdateWeaponsData()
	{
#if DEBUG_WEAPON
        Debug.Log("[WEAPON] UpdateWeaponsData");
#endif
        if (_weaponDataList != null)
            _weaponDataList.Clear();
        else
            _weaponDataList = new List<WeaponData>();

        foreach (var keyval in _attachObjectDic)
		{
            for (int i = 0; i < (int)WeaponType.COUNT; ++i)
			{
                WeaponType wType = (WeaponType)i;
                if (IsWeaponActiveInList(keyval.Value, wType))
				{
                    _weaponDataList.Add(new WeaponData(wType, keyval.Key));
#if DEBUG_WEAPON
                    Debug.Log($"[WEAPON] _weaponDataList add {wType} at {keyval.Key}");
#endif
                }
            }
        }
	}

    public void SetWeaponInPlace(WeaponType wType, WeaponPlace wPlace)
	{
#if DEBUG_WEAPON
        Debug.Log($"[WEAPON] SetWeaponInPlace {wType} at {wPlace}");
#endif
        List<attachobject> objects = _attachObjectDic[wPlace];
        for (int i = 0; i < (int)WeaponType.COUNT; ++i)
        {
            WeaponType localWeaponType = (WeaponType)i;
            SetWeaponActiveInList(objects, localWeaponType, localWeaponType == wType);
        }
	}

    public WeaponType GetWeaponAtPlace(WeaponPlace wPlace)
    {
        foreach (WeaponData data in _weaponDataList)
		{
            if (data.weaponPlace == wPlace)
                return data.weaponType;
		}
        return WeaponType.None;
    }

    public void ChangePlaceOfWeaponData(int numWeaponData, WeaponPlace place)
	{
        if (numWeaponData >= 0 && numWeaponData < _weaponDataList.Count)
		{
            if (_weaponDataList[numWeaponData].weaponPlace != place)
			{
#if DEBUG_WEAPON
                Debug.Log($"[WEAPON] ChangePlaceOfWeaponData {numWeaponData} at {place}");
#endif
                WeaponType weapon = _weaponDataList[numWeaponData].weaponType;
                SetWeaponInPlace(WeaponType.None, _weaponDataList[numWeaponData].weaponPlace);
                SetWeaponInPlace(weapon, place);
                _weaponDataList[numWeaponData] = new WeaponData(weapon, place);
            }
        }
	}

    public void ChangeTypeOfWeaponData(int numWeaponData, WeaponType wType)
    {
        if (_weaponDataList == null)
            return;
        if (HasDaggerInLeftHand() || HasDaggerInRightHand())
            return;
        if (numWeaponData >= 0 && numWeaponData < _weaponDataList.Count)
        {
            if (_weaponDataList[numWeaponData].weaponType != wType)
            {
#if DEBUG_WEAPON
                Debug.Log($"[WEAPON] ChangeTypeOfWeaponData {numWeaponData} at {wType}");
#endif
                SetWeaponInPlace(wType, _weaponDataList[numWeaponData].weaponPlace);
                _weaponDataList[numWeaponData] = new WeaponData(wType, _weaponDataList[numWeaponData].weaponPlace);
            }
        }
    }

    public attachobject GetAttachObjectOfPlace(WeaponPlace place)
	{
        foreach (attachobject attach in _attachObjectDic[place])
		{
            if (attach.gameObject.activeSelf)
			{
                return attach;
			}
		}
        return null;
    }

    public WeaponType GetTypeOfWeaponData(int numWeaponData)
    {
        if (numWeaponData >= 0 && numWeaponData < _weaponDataList.Count)
        {
            return _weaponDataList[numWeaponData].weaponType;
        }
        return WeaponType.None;
    }

    public WeaponPlace GetPlaceOfWeaponData(int numWeaponData)
    {
        if (numWeaponData >= 0 && numWeaponData < _weaponDataList.Count)
        {
            return _weaponDataList[numWeaponData].weaponPlace;
        }
        return WeaponPlace.None;
    }

    public void UpdateName()
    {
        SetName();
    }

    private void SetName()
    {
        if (playerName != null)
            playerName.text = GameflowBase.myPirateName;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)// We own this player: send the others our data
        {
            stream.SendNext(respawnTimeMax);
            stream.SendNext(health);
            stream.SendNext(kills);
            stream.SendNext(respawn);
            stream.SendNext(isDead);
            stream.SendNext(rightController.transform.position);
            stream.SendNext(rightController.transform.rotation);
            stream.SendNext(leftController.transform.position);
            stream.SendNext(leftController.transform.rotation);
        }
        else // Network player, receive data
        {
            respawnTimeMax = (float)stream.ReceiveNext();
            health = (float)stream.ReceiveNext();
            kills = (int)stream.ReceiveNext();
            respawn = (bool)stream.ReceiveNext();
            isDead = (bool)stream.ReceiveNext();
            healthSlider.fillAmount = health / starthealth;
            rightController.transform.position = (Vector3)stream.ReceiveNext();
            rightController.transform.rotation = (Quaternion)stream.ReceiveNext();
            leftController.transform.position = (Vector3)stream.ReceiveNext();
            leftController.transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }

    public bool IsOneAvatarInTeleportation()
	{
        foreach (Player_avatar avatar in _avatars)
		{
            if (avatar.actornumber >= 0)
			{
                if (avatar.isInTeleportation)
                    return true;
			}
		}
        return false;
    }

    public void ShowAllAvatars()
    {
        foreach (Player_avatar avatar in _avatars)
        {
            if (avatar.actornumber >= 0)
            {
                avatar.gameObject.SetActive(true);
                if (avatar.isInTeleportation)
                    avatar.TeleportShowOn();
            }
        }
    }

    public Player_avatar GetAvatar(int id)
	{
        foreach (Player_avatar avatar in _avatars)
        {
            if (avatar.actornumber == id)
            {
                return avatar;
            }
        }
        return null;
    }

    void Update()
    {
        PlayerLifeCycle();

        if (hitmyself)
        {
            hitmyself = false;
            playerBody.AddDamage(1000);
        }

        if (gameflowmultiplayer.areAllRacesLoaded)
            playerstartposition = gameObject.transform.position;

        UpdateUiFeedback();
        LateUpdateUiFeedback();
        UpdateUiLife();

        if (gameflowmultiplayer.myself != null && (gameflowmultiplayer.myself.isInCabin || _averageCounterForHeightOfEyes < 100))
		{
            float headsetHeight = GetHeadsetHeight();
            if (headsetHeight > 1f)
            {
                _averageCounterForHeightOfEyes = Mathf.Min(_averageCounterForHeightOfEyes, 100);
                _heightOfEyes = _heightOfEyes * _averageCounterForHeightOfEyes + headsetHeight;
                _averageCounterForHeightOfEyes++;
                _heightOfEyes /= _averageCounterForHeightOfEyes;
                AdaptScaleToCurrentSkin();
            }
        }

        float dotCamForward = Vector3.Dot(cam.transform.forward, Vector3.down);
        UpdateBodyShaderAlpha(dotCamForward > 0.75f);
    }

    private void UpdateBodyShaderAlpha(bool useAlpha, bool resetSkin = false)
	{
        if (body == null)
            return;

        if (_skinBody == null)
            _skinBody = body.GetComponent<SkinnedMeshRenderer>();

        if (useAlpha)
		{
            if (_oldBodyMaterial == null)
            {
                _oldBodyMaterial = _skinBody.material;
                _skinBody.material = new Material(_oldBodyMaterial);
                _skinBody.material.shader = bodyAlphaShader;
            }
        }
        else
		{
            if (_oldBodyMaterial != null)
            {
                _skinBody.material = _oldBodyMaterial;
                _oldBodyMaterial = null;
            }        
        }

        if (resetSkin)
            _skinBody = null;
    }

    private void AdaptScaleToCurrentSkin()
    {
        // Adapt scale of skin in function of height of eyes computed in cabin
        if (_vrik != null)
        {
            float scale = gamesettings_player.myself.GetScaleFromHeightOfEyesForSkin(_vrik.gameObject.name, GetEyesHeight());
            _vrik.transform.localScale = new Vector3(scale, scale, scale);
			float offset = gamesettings_player.myself.GetOffsetFromHeightOfEyesForSkin(_vrik.gameObject.name, _heightOfEyes);
			camOffset.localPosition = Vector3.up * offset;
		}
    }

    private void LateUpdate()
    {
        UpdatePlayerPositionOnAvatar();

        if (fixedPosition)
        {
            Vector3 pos = cam.transform.localPosition;
            if (_oldLocalPos != Vector3.zero)
            {
                Vector3 diffPos = pos - _oldLocalPos;
                diffPos.y = 0f;
                transform.localPosition -= transform.TransformVector(diffPos);
            }
            _oldLocalPos = pos;
        }
        if (magnet)
		{
            if (_magnetTime == 0f)
            {
                if (pointleft._triggerstate_trigger || pointright._triggerstate_trigger)
                {
                    _magnetTime = Time.time;
                    _magnetDir = cam.transform.forward;
                    _magnetDir.y = 0f;
                }
            }
            else
            {
                if (Time.time - _magnetTime < 2.5f)
                    transform.localPosition += _magnetDir * Time.deltaTime;
                else
                    _magnetTime = 0f;
            }
        }
    }

    public void UpdatePlayerPositionOnAvatar()
    {
        if (head_dst != null && head_src != null)
        {
            body_dst.transform.localPosition = gameObject.transform.localPosition;
            body_dst.transform.localRotation = gameObject.transform.localRotation;

            head_dst.transform.localPosition = head_src.transform.localPosition;
            head_dst.transform.localRotation = head_src.transform.localRotation;

            left_dst.transform.localPosition = leftController.transform.localPosition;
            left_dst.transform.localRotation = leftController.transform.localRotation;

            right_dst.transform.localPosition = rightController.transform.localPosition;
            right_dst.transform.localRotation = rightController.transform.localRotation;
        }
    }

    private void UpdateUiFeedback()
	{
#if USE_STANDALONE
        bool needUiFeedback = true;
#else
        bool needUiFeedback = false;
#endif
        if (pointfromhand.isUsingArc)
		{
            needUiFeedback = false;
        }
        else
        {
            _gun = null;
            ProjectileCannon[] guns = _currentSkinPlayer.GetComponentsInChildren<ProjectileCannon>();
            if (guns != null && guns.Length > 0)
			{
                _gun = guns[0];
                if (guns.Length > 1 && guns[1].lastFireTime > guns[0].lastFireTime)
                    _gun = guns[1];
            }

            if (_gun != null)
            {
                CurvedUIInputModule.Instance.ControllerTransformOverride = _gun.lookAt;
            }
            else
			{
                CurvedUIInputModule.Instance.ControllerTransformOverride = pointright.transform;
			}
#if USE_KDK
            if (GameflowKDK.myself != null)
            {
                switch (GameflowKDK.myself.gameState)
                {
                    case GameflowKDK.GameState.Cabin:
                    case GameflowKDK.GameState.NameValidated:
                    case GameflowKDK.GameState.ChooseLevel:
                    case GameflowKDK.GameState.EndGame:
                        needUiFeedback = true;
                        break;
                    default:
                        needUiFeedback = false;
                        break;
                }
            }
            else
			{
                needUiFeedback = true;
            }
#elif USE_BOD
            if (GameflowBOD.myself != null)
            {
                switch (GameflowBOD.myself.gameState)
                {
                    case GameflowBOD.GameState.Cabin:
                    case GameflowBOD.GameState.NameValidated:
                    case GameflowBOD.GameState.ChooseLevel:
                    case GameflowBOD.GameState.EndGame:
                        needUiFeedback = true;
                        break;
                    default:
                        needUiFeedback = false;
                        break;
                }
            }
            else
            {
                needUiFeedback = true;
            }
#else
            if (gameflowmultiplayer.myself != null)
            {
                switch (gameflowmultiplayer.myself.gameState)
                {
                    case gameflowmultiplayer.GameState.Cabin:
                        needUiFeedback = true;
                        break;
                    case gameflowmultiplayer.GameState.NameValidated:
                    case gameflowmultiplayer.GameState.LevelValidated:
                        needUiFeedback = false;
                        break;
                    case gameflowmultiplayer.GameState.ChooseLevel:
                    case gameflowmultiplayer.GameState.EndGame:
#if USE_STANDALONE
                        needUiFeedback = true;
#else
                        needUiFeedback = PhotonNetworkController.IsMaster();
#endif
                        break;
                    case gameflowmultiplayer.GameState.LobbyLoaded:
                    case gameflowmultiplayer.GameState.TeamSelected:
                        needUiFeedback = InitGameData.instance == null;
                        break;
                    case gameflowmultiplayer.GameState.TeamValidated:
                    case gameflowmultiplayer.GameState.ShowRaceWorld:
                    case gameflowmultiplayer.GameState.PlayersToBoat:
                    case gameflowmultiplayer.GameState.RaceLoaded:
                    case gameflowmultiplayer.GameState.OpenDoors:
                    case gameflowmultiplayer.GameState.RaceStarted:
                        needUiFeedback = false;
                        break;
                }
            }
#endif
        }

        if (pointfromhand.isPausePopupVisible)
        {
            needUiFeedback = true;
        }

        if (needUiFeedback)
        {
            if (_uiFeedback == null)
            {
                _uiFeedback = GameObject.Instantiate(gamesettings_player.myself.uiFeedbackPrefab);
                _uiFeedback.transform.SetParent(transform);
                _uiFeedbackAnchor = _uiFeedback.transform.GetChild(0);
                OnPausePopupVisible(pointfromhand.isPausePopupVisible);
            }
        }
        else
        {
            if (_uiFeedback != null)
            {
                GameObject.DestroyImmediate(_uiFeedback);
                _uiFeedback = null;
            }
        }
    }

    public void OnPausePopupVisible(bool visible)
	{
        if (_uiFeedback != null)
        {
            if (visible)
            {
                _uiFeedback.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Tools"));
            }
            else
            {
                _uiFeedback.gameObject.SetLayerRecursively(LayerMask.NameToLayer("UI"));
            }
        }
    }

    private void LateUpdateUiFeedback()
    {
        if (_uiFeedback != null)
        {
            if (CurvedUIInputModule.Instance.ControllerTransformOverride != null)
            {
                Vector3 feedbackPos = CurvedUIInputModule.Instance.ControllerTransformOverride.position;
                Ray ray = new Ray(CurvedUIInputModule.Instance.ControllerPointingOrigin, CurvedUIInputModule.Instance.ControllerPointingDirection);
                RaycastHit hit;
                Vector3 pointStart = feedbackPos;
                Vector3 pointEnd = pointStart + CurvedUIInputModule.Instance.ControllerPointingDirection * _lastUiFeedbackDistance;
                string layerName = UI_PausePopup.myself != null ? "Tools" : "UI";
                if (Physics.Raycast(ray, out hit, 1000f, 1 << LayerMask.NameToLayer(layerName)))
                {
                    pointEnd = hit.point;
                    _lastUiFeedbackDistance = Vector3.Distance(pointStart, pointEnd);
                }
                else
				{
                    _lastUiFeedbackDistance = 10f;
                }

                _uiFeedback.transform.position = feedbackPos;
                _uiFeedback.transform.localScale = Vector3.one;
                _uiFeedback.transform.LookAt(pointEnd);

                float scale = 1f;
                if (OnHover.lastButtonHover != null)
                {
                    _uiFeedbackAnchor.gameObject.SetActive(true);
                    _uiFeedbackAnchor.forward = OnHover.lastButtonHover.transform.forward;

                    BoxCollider box = OnHover.lastButtonHover.GetComponent<BoxCollider>();
                    if (box != null)
                        scale = Mathf.Min(box.transform.lossyScale.x * box.size.x, box.transform.lossyScale.y * box.size.y) * 2f;

                    _uiFeedbackAnchor.localScale = Vector3.one * scale;

                    _lastUiFeedbackDistance -= 0.01f * scale;
                }
                else
				{
                    _uiFeedbackAnchor.gameObject.SetActive(false);
                }

                _uiFeedbackAnchor.localPosition = Vector3.forward * _lastUiFeedbackDistance;
            }
        }
    }

    private void PlayerLifeCycle()
    {
        if (_oldKills != kills)
            StartCoroutine(DisplayKillCount());
        _oldKills = kills;
        if (healthSlider != null)
            healthSlider.fillAmount = health / starthealth;
        PlayerLifeCycleDeathStateCheck();
        if (isDead == false)
            PlayerLifeCycleAliveState();
    }


    private void PlayerLifeCycleAliveState()
    {
        if (Time.time - _lastHitTime > secondsBeforeHealthRegeneration)
            HealthRegeneration();
        
        if (_leftController.wantsToShot == true)
            FireLeft();
        if (_rightController.wantsToShot == true)
            FireRight();
        
		///// RECEPTION DE DEGATS
		if (gamesettings_player.myself.canKill && playerBody.hasBeenHit == true)
		{
			StartCoroutine(ImpactEffect());
            _lastHitTime = Time.time;
			health -= playerBody.lastHitDamage;
            if (health < 0f)
                health = 0f;
            playerBody.lastHitDamage = 0f;
            playerBody.hasBeenHit = false;
            UpdateUiLife();
            GameflowBase.instance?.SendPlayerHealth();
        }
	}

	private void PlayerLifeCycleDeathStateCheck()
    {
        //// DEATH
        if (health <= 0 && isDead == false)
        {
            deathDisplay.SetActive(true);
            isDead = true;
            actualRespawnTime = respawnTimeMax;
            GameflowBase.instance?.SendPlayerIsDead();
            GameflowBase.IncrementPlayerStat(gamesettings.STAT_DEATH, 1);
            Vector3 pos = transform.position;
            AddProgress($"DEAD({pos.x:F2}|{pos.y:F2}|{pos.z:F2})");
            Teleport(Vector3.up * 1000f);
        }
        if (isDead)
        {
            actualRespawnTime -= Time.deltaTime;
            respawnTimeText.text = $"{RRLib.RRLanguageManager.instance.GetString("str_kdk_playerregenerates")} {Mathf.CeilToInt(actualRespawnTime)}s";
            if (actualRespawnTime <= 0)
            {
                respawn = true;
            }
        }
        if (respawn == true)
        {
            StartCoroutine(Revive());
            respawn = false;
        }
    }

    private void UpdateUiLife()
	{
        //_uiLife.SetPercentLife(Mathf.CeilToInt(health * 100f / starthealth));
    }


    private IEnumerator ImpactEffect()
    {
        if (impactEffect.activeSelf == false)
        {
            impactEffect.SetActive(true);
            SpriteRenderer renderer = impactEffect.GetComponent<SpriteRenderer>();
            while (renderer.color.a <= 1)
            {
                Color col = renderer.color;
                col.a += 0.05f;
                renderer.color = col;
                yield return null;
            }
            while (renderer.color.a >= 0)
            {
                Color col = renderer.color;
                col.a -= 0.05f;
                renderer.color = col;
                yield return null;
            }
            impactEffect.SetActive(false);
        }
        yield return null;
    }

    private IEnumerator DisplayKillCount()
    {
        killsText.enabled = true;
        killsText.text = kills + " kills";
        yield return new WaitForSeconds(3);
        killsText.enabled = false;
        yield return null;
    }

    public void HealthRegeneration()
    {
        if (health < starthealth)
        {
            health += healthRegeneration * Time.deltaTime;
            if (health > starthealth)
                health = starthealth;
            UpdateUiLife();
            GameflowBase.instance?.SendPlayerHealth();
        }
    }

    IEnumerator Revive()
    {
        gamesettings_screen.myself.FadeOut(0.05f);
        while (gamesettings_screen.myself.faderunning)
            yield return null;

        deathDisplay.SetActive(false);
        RespawnAtSpawnPoint();
        gamesettings_screen.myself.FadeIn();
        isDead = false;
        health = starthealth;
        _canTeleport = true;
        UpdateUiLife();
		ShowGuns();
	}

    public Vector3 GetFootPos()
	{
        return cam.transform.position - transform.up * GetEyesHeight();
    }

    public float GetCamRotationY()
    {
        return cam.transform.localRotation.eulerAngles.y;
    }

    public float GetEyesHeight()
	{
        return cam.transform.localPosition.y + camOffset.localPosition.y;

    }

    public float GetHeadsetHeight()
    {
        return cam.transform.localPosition.y;

    }

    public void RespawnAtSpawnPoint()
    {
        if (_startPoint != null)
            Teleport(_startPoint.transform.position);
        else
            Teleport(playerstartposition);
    }

    public void Teleport(Vector3 targetplace)
    {
#if DEBUG_TELEPORT
        Debug.Log("[TELEPORT] Teleport to targetplace " + targetplace + " _vrik " + _vrik);
#endif
        Vector3 footPos = GetFootPos();
        Vector3 dpos = footPos - transform.position;
        if (dpos.sqrMagnitude > 10f)
            dpos = Vector3.zero;

#if DEBUG_TELEPORT
        Debug.Log("[TELEPORT] Teleport dpos " + dpos + " dpos.sqrMagnitude " + dpos.sqrMagnitude);
#endif

        transform.position = targetplace - dpos;
#if DEBUG_TELEPORT
        Debug.Log("[TELEPORT] Teleport final position " + transform.position);
#endif
        if (onPlayerEvent != null)
            onPlayerEvent(PlayerEvent.Teleport, transform.position);
    }

    public void Teleport(Vector3 targetplace, Quaternion targetRotation, Transform parent = null)
    {
#if DEBUG_TELEPORT
        Debug.Log($"[TELEPORT] direct {targetplace} {targetRotation} ");
#endif
        if (parent != null)
            transform.SetParent(parent);
        transform.rotation = targetRotation * Quaternion.Euler(0f, -GetCamRotationY(), 0f);
        transform.position += targetplace - GetFootPos();
        if (onPlayerEvent != null)
            onPlayerEvent(PlayerEvent.Teleport, transform.position);
    }

    public void TeleportWithFadeIn(Transform trPos, Transform parent, float fadeDuration = 0.5f)
	{
        StartCoroutine(TeleportWithFadeInEnum(trPos, parent, fadeDuration));
	}

    private IEnumerator TeleportWithFadeInEnum(Transform trPos, Transform parent, float fadeDuration)
    {
        EnableTeleport(false);

        gamesettings_screen.myself.FadeOut(fadeDuration);
        while (gamesettings_screen.myself.faderunning)
            yield return null;

        Vector3 pos = trPos.position;
        Quaternion rot = trPos.rotation;

#if DEBUG_TELEPORT
        Debug.Log($"[TELEPORT] with fadein {pos} {rot} {fadeDuration} parent {parent}");
#endif
        Teleport(pos, rot, parent);

        yield return new WaitForSeconds(fadeDuration);

        gamesettings_screen.myself.FadeIn();
        while (gamesettings_screen.myself.faderunning)
            yield return null;

        EnableTeleport(true);
    }

    public void AddProgress(string data)
	{
        _progress += $"${data}";
        Debug.Log("[PROGRESS] " + _progress);
    }

    public void AddPercent()
    {
        GameflowBOD.completion += 12.5f;
    }
    private GameObject GetFreeSlot()
    {
        return null;
    }

    private int CheckNumberFreeSlot()
    {
        return 0;
    }

    void FireRight()
    {
        if (_rightController.isAWeapon == false)
            return;
#if STEAM_PRESENT
        _rightController.weapon.fireThatWeaponVR = true;
#endif
        _rightController.wantsToShot = false;
    }

    void FireLeft()
    {
        // Check si arme ou objet
        if (_leftController.isAWeapon == false)
            return;
#if STEAM_PRESENT
        _leftController.weapon.fireThatWeaponVR = true;
#endif
        _leftController.wantsToShot = false;
    }

    public void EnableTeleport(bool enabled)
	{
        _canTeleport = enabled;
    }

    public void SetForcedTeleportTarget(Transform tr)
	{
        _forcedTeleportTarget = tr;
    }
    
    public void TelePorter(GameObject camerarig, Vector3 pos, boat_followdummy boat = null, bool longTeleport = false)
	{
        if (_isTeleporting || _isInPause)
            return;

#if PLAYER_CAN_TELEPORT_OUTSIDE_BOAT
        if (gameObject.GetComponentInParent<boat_followdummy>() != boat)
		{
            Transform parent = boat?.boatSinking?.transform;
            transform.SetParent(parent);
            pointfromhand.teleporttarget.transform.SetParent(parent);
		}
#endif

        StartCoroutine(TelePorterEnum(camerarig, pos, longTeleport));
	}

    private IEnumerator TelePorterEnum(GameObject camerarig, Vector3 pos, bool longTeleport = false)
    {
        if (_isTeleporting) yield break;
        _isTeleporting = true;
        //Vector3 moveOld = pos - camerarig.transform.position;

        gamesettings_screen gsScreen = gamesettings_screen.myself;

        float fadeDuration = longTeleport ? _longTeleportFadeDuration : 0.05f;

        while (gsScreen.faderunning)
            yield return null;
        
        Vector3 move = camerarig.transform.InverseTransformPoint(pos);
        gsScreen.FadeOut(fadeDuration);
        while (gsScreen.faderunning)
            yield return null;

        //pos = move + camerarig.transform.position;
        pos = camerarig.transform.TransformPoint(move);
        //Vector3 old = moveOld + camerarig.transform.position;
        //if (pos != old)
        //    Debug.Log($"[TELEPORT] pos {pos.x} {pos.y} {pos.z} old {old.x} {old.y} {old.z}");

        if (longTeleport && _longTeleportFX != null)
        {
            Teleport(Vector3.up * 1000f);

            Vector3 playerPos = cam.transform.position;
            Vector3 playerDir = _currentSkinPlayer.skinPelvis.transform.forward;
            playerDir.y = 0f;
            _longTeleportFX.transform.position = cam.transform.position + playerDir.normalized * 0.1f;
            _longTeleportFX.transform.LookAt(playerPos);
            _longTeleportFX.SetActive(true);

            gsScreen.FadeIn();
            while (gsScreen.faderunning)
                yield return null;

            yield return new WaitForSeconds(_longTeleportDuration);

            gsScreen.FadeOut(fadeDuration);
            while (gsScreen.faderunning)
                yield return null;

            _longTeleportFX.SetActive(false);
        }

        Teleport(pos);
        // Tutorial Event
        if (UI_Tutorial.myself != null)
            UI_Tutorial.myself.OnTriggerCondition(UI_Tutorial.TutoCondition.Teleport);

		ShowGuns();

		if (!isInPause)
        {
            gsScreen.FadeIn();
            while (gsScreen.faderunning)
                yield return null;
        }

        _isTeleporting = false;
    }

    boat_followdummy IPlayer.GetBoat()
    {
#if USE_KDK || USE_BOD
        return null;
#else
        return gameObject.GetComponentInParent<boat_followdummy>();
#endif
    }

    int IPlayer.GetTeam()
    {
#if USE_KDK || USE_BOD
        return team;
#else
        boat_followdummy boat = ((IPlayer)this).GetBoat();
        if (boat != null)
            return boat.team;
        return 0;
#endif
    }

    public void SetCustomHat(string customHat, CustomPackSettings customPackSettings)
	{
        _customHat = customHat;
        GameflowBase.SetMyCustomHat(_customHat);
        if (!string.IsNullOrEmpty(_customHat))
        {
            multiplayerlobby.theme = multiplayerlobby.SkinTheme.Custom;
            CustomPackSettings.CustomItem item = customPackSettings.GetItem(_customHat);
            UpdateCustomTheme(item.prefab);
        }
        else
        {
            multiplayerlobby.theme = multiplayerlobby.SkinTheme.Normal;
            UpdateCustomTheme();
        }
    }

    private void OnRaceEvent(int team, bool isMe, string statId, object param)
    {
        //Debug.Log($"OnRaceEvent team {team} - {statId} - param {param}");
        if (team < 0)
        {
            Debug.LogWarning($"OnRaceEvent team {team} is not valid for {statId} & param {param}");
            return;
        }

        switch (statId)
        {
            case gamesettings.STAT_POINTS:
                {
                    int gold = (int)((double)param);
					_teamGoldCollected += gold;
					if (isMe)
                        _score += gold;
                    UpdateGoldAndScoreTexts();
                }
                break;
            default:
                if (System.Enum.TryParse(statId, out Health.HealtObjectType result))
				{
                    if (gamesettings_general.myself != null && isMe)
					{
                        UI_EndRacePlayer.ValueType valueType = UI_EndRacePlayer.ValueType.Treasures;
                        switch (result)
						{
                            case Health.HealtObjectType.drone:
                                valueType = UI_EndRacePlayer.ValueType.Drone;
                                break;
                            case Health.HealtObjectType.mine:
                                valueType = UI_EndRacePlayer.ValueType.Mine;
                                break;
                            case Health.HealtObjectType.superDrone:
                                valueType = UI_EndRacePlayer.ValueType.SuperDrone;
                                break;
                            case Health.HealtObjectType.megaDroid:
                                valueType = UI_EndRacePlayer.ValueType.MegaDroid;
                                break;
                            case Health.HealtObjectType.plasmaBomb:
                                valueType = UI_EndRacePlayer.ValueType.PlasmaBomb;
                                break;
                            case Health.HealtObjectType.bomber:
                                valueType = UI_EndRacePlayer.ValueType.Bomber;
                                break;
                            case Health.HealtObjectType.conveyor:
                                valueType = UI_EndRacePlayer.ValueType.Conveyor;
                                break;
                            case Health.HealtObjectType.droneUltra:
                                valueType = UI_EndRacePlayer.ValueType.DroneUltra;
                                break;

                        }
                        _score += (int)gamesettings_general.myself.scoreValueTypeCoef[valueType];
                        UpdateGoldAndScoreTexts();
                    }
				}
                break;
        }
    }

    public void SetLifePercent(float life)
	{
        if (lifeText != null)
            lifeText.text = $"{Mathf.RoundToInt(life)}%";
    }

    public void SetAccuracyPercent(float accuracyPercent)
    {
        _accuracy = accuracyPercent * 0.01f;
        if (accuracyText != null)
            accuracyText.text = $"{Mathf.RoundToInt(accuracyPercent)}%";
    }

    public void UpdateDisplayTime(float timeInSecond)
	{
        int minutes = Mathf.FloorToInt(timeInSecond) / 60;
        int seconds = Mathf.FloorToInt(timeInSecond) % 60;
        int ms = Mathf.FloorToInt(timeInSecond * 100f) % 100;
        if (timeText != null)
            timeText.text = $"{minutes.ToString("00")}:{seconds.ToString("00")}:{ms.ToString("00")}";

    }

    public void UpgradeGun(bool left, int numCannon, int goldCost)
	{
        if (teamGold >= goldCost)
        {
            _teamGoldSpent += goldCost;
            UpdateGoldAndScoreTexts();
            attachobject attach = left ? GetAttachObjectOfPlace(WeaponPlace.LeftHand) : GetAttachObjectOfPlace(WeaponPlace.RightHand);
            ProjectileCannon gun = attach.GetComponentInChildren<ProjectileCannon>();
            if (gun != null)
			{
                gun.SetOtherCannonVisible(numCannon, true);
                if (left)
                    _currentLeftGunUpgrade = numCannon + 1;
                else
                    _currentRightGunUpgrade = numCannon + 1;
            }
        }
    }

    public void ResetGuns()
	{
        ResetGun(true);
        ResetGun(false);
    }

    public void ResetGun(bool left)
    {
        attachobject attach = left ? GetAttachObjectOfPlace(WeaponPlace.LeftHand) : GetAttachObjectOfPlace(WeaponPlace.RightHand);
        if (attach != null)
		{
            ProjectileCannon gun = attach.GetComponentInChildren<ProjectileCannon>();
            if (gun != null)
            {
                gun.ResetGuns();
                if (left)
                    _currentLeftGunUpgrade = 0;
                else
                    _currentRightGunUpgrade = 0;
            }
            else
            {
                Debug.Log("ResetGun - no ProjectileCannon in gun attached");
            }
        }
        else
		{
            Debug.Log("ResetGun - no gun attached");
		}
    }

    [ContextMenu("Hide Guns")]
    public void HideGuns()
	{
        _lastWeaponLeft = GetWeaponAtPlace(WeaponPlace.LeftHand);
        _lastWeaponRight = GetWeaponAtPlace(WeaponPlace.RightHand);
#if USE_KDK || USE_BOD
        SetWeaponInPlace(WeaponType.KDK_Hands, WeaponPlace.LeftHand);
        SetWeaponInPlace(WeaponType.KDK_Hands, WeaponPlace.RightHand);
#else
        SetWeaponInPlace(WeaponType.None, WeaponPlace.LeftHand);
        SetWeaponInPlace(WeaponType.None, WeaponPlace.RightHand);
#endif
        _areGunsHidden = true;
    }

    [ContextMenu("Show Guns")]
    public void ShowGuns()
    {
        if (_areGunsHidden)
        {
            _areGunsHidden = false;
            SetWeaponInPlace(_lastWeaponLeft, WeaponPlace.LeftHand);
            SetWeaponInPlace(_lastWeaponRight, WeaponPlace.RightHand);
        }
    }

    public void AddTarget(Transform tr, PlayerFollowTarget.FollowTargetType targetType = PlayerFollowTarget.FollowTargetType.Enemy)
	{
        UI_FollowTarget prefab = null;
        GameObject miniMapPrefab = null;
        switch (targetType)
		{
            case PlayerFollowTarget.FollowTargetType.Ally:
                prefab = _allyTargePrefab;
                break;
            case PlayerFollowTarget.FollowTargetType.Enemy:
                prefab = _targetPrefab;
                break;
            case PlayerFollowTarget.FollowTargetType.Archive:
                prefab = _archivesTargetPrefab;
                miniMapPrefab = _miniMapGoalPrefab;
                break;
            case PlayerFollowTarget.FollowTargetType.Goal:
                prefab = _goalsTargetPrefab;
                miniMapPrefab = _miniMapGoalPrefab;
                break;
            case PlayerFollowTarget.FollowTargetType.Scientist:
                prefab = _scientistTargetPrefab;
                miniMapPrefab = _miniMapGoalPrefab;
                break;
        }
        if (prefab != null)
            AddTargetWithPrefab(tr, prefab, miniMapPrefab);
	}

    public void AddTargetWithPrefab(Transform tr, UI_FollowTarget prefab, GameObject miniMapPrefab = null)
    {
        UI_FollowTarget target = GameObject.Instantiate<UI_FollowTarget>(prefab, _targetRoot);
        if (target != null)
        {
            if (_followTargets == null)
                _followTargets = new List<UI_FollowTarget>();
            target.Init(cam, tr);
            _followTargets.Add(target);
        }
        if (_miniMap != null && miniMapPrefab != null)
		{
            _miniMap.AddTarget(tr, miniMapPrefab);
        }
    }

    public void RemoveTarget(Transform tr, bool destroyTarget = true)
    {
        if (_followTargets != null)
		{
            for (int i = _followTargets.Count - 1; i >= 0; --i)
			{
                if (_followTargets[i].target == tr)
				{
                    if (destroyTarget)
                    {
                        if (_followTargets[i] != null && _followTargets[i].gameObject != null)
                            GameObject.Destroy(_followTargets[i].gameObject);
                    }
                    _followTargets.RemoveAt(i);
				}
			}
		}
        if (_miniMap != null)
        {
            _miniMap.RemoveTarget(tr);
        }
    }

    public void ResetHUD()
	{
        if (_uiCanvas != null)
		{
            foreach (Transform tr in _uiCanvas.transform)
                tr.gameObject.SetActive(false);
        }
	}

    public void ReceiveCommand(string name)
	{
        if (_eventCommand != null)
            _eventCommand.PlayCommand(name);
    }

    public void SetGoalFromName(string name)
	{
        if (_uiGoals != null)
		{
            _uiGoals.SetGoalFromName(name);
		}
	}

    public void UpdateGoalCounter()
    {
        if (_uiGoals != null)
        {
            _uiGoals.UpdateGoalCounter();
        }
    }

    public void IncrementGoalCounterMax()
    {
        if (_uiGoals != null)
        {
            _uiGoals.IncrementGoalCounterMax();
        }
    }

    public void AddVRIKPlatform(GameObject parent)
	{
        VRIKPlatform vrikPlatform = parent.GetComponent<VRIKPlatform>();
        if (vrikPlatform == null)
            vrikPlatform = parent.AddComponent<VRIKPlatform>();
        vrikPlatform.ik = _vrik;
    }

    public void RemoveVRIKPlatform(GameObject parent)
    {
        VRIKPlatform vrikPlatform = parent.GetComponent<VRIKPlatform>();
        if (vrikPlatform != null)
            GameObject.Destroy(vrikPlatform);
    }

    public void AddLife (int Gift)
    {
        health += Gift;
        UpdateUiLife();
    }
}
