//#define DEBUG_AVATAR

using Photon.Pun;
using RootMotion.FinalIK;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
#if STEAM_PRESENT
using Valve.VR;
#endif

public class Player_avatar : MonoBehaviour, Player.IPlayer
{
    public int team => _team;
    private int _team = 0;

    public List<attachobject> left_hand_objects = null;
    public List<attachobject> right_hand_objects = null;
    public List<attachobject> belt_left_objects = null;
    public List<attachobject> belt_right_objects = null;
    public List<attachobject> belt_front_objects = null;
    public List<attachobject> belt_back_objects = null;
    public int actornumber = -100;
    public GameObject _fxTeleport = null;
    [SerializeField]
    private TextMeshProUGUI _avatarName = null;
    [SerializeField]
    private Player_icon _icon = null;
    [SerializeField]
    private GameObject _playerInfos = null;
    [SerializeField]
    private UI_HealthJauge _healthGauge = null;

    public VRIK vrik => _vrik;
    private VRIK _vrik = null;

    public SkinPlayer skinPlayer => _skinPlayer;
    private SkinPlayer _skinPlayer = null;

    private avatar_vrik_corrector _vrikCorrector = null;
    private player_vrik_corrector _playerVrikCorrector = null;

    public bool isInPause => _isInPause;
    private bool _isInPause = false;

    public int id => actornumber;
    public GameObject goRoot => gameObject;

    private string _skinName = null;

    public bool isVisible => _isVisible;
    private bool _isVisible = false;

    public bool isInTeleportation => _isInTeleportation;
    private bool _isInTeleportation = false;

    public bool isDead { private set; get; }

    // Start is called before the first frame update
    void Awake()
    {
        OtherPlayerInitialization();
        _vrikCorrector = GetComponent<avatar_vrik_corrector>();
        _playerVrikCorrector = GetComponent<player_vrik_corrector>();
#if DEBUG_AVATAR
        Debug.Log($"[AVATAR] {actornumber} _vrikCorrector " + _vrikCorrector);
#endif
    }

    public void PoolAvatar()
    {
        actornumber = -100;
        _vrik = null;
        _skinPlayer = null;
        if (gamesettings_player.myself != null)
            transform.SetParent(gamesettings_player.myself.transform);
        name = "NotUsedAvatar";
    }

    public void InitTeam(int team)
    {
        _team = team;
        VRIK vrik = gameObject.GetComponentInChildren<VRIK>();
        if (vrik != null)
            InitVRIK(vrik);
        else if (!string.IsNullOrEmpty(_skinName))
            SetSkinName(_skinName, true);
        SetMyAttachedObjects();
        UI_ChangeColor[] changeColors = gameObject.GetComponentsInChildren<UI_ChangeColor>(true);
        foreach (UI_ChangeColor changeColor in changeColors)
            changeColor.InitColor((boat_followdummy.TeamColor)team);
    }

    public void InitVRIK(VRIK vrik)
    {
#if DEBUG_AVATAR
        Debug.Log($"[AVATAR] {actornumber} InitVRIK " + vrik);
#endif
        _vrik = vrik;
        _skinPlayer = vrik?.GetComponent<SkinPlayer>();
        if (_vrik != null && _vrikCorrector != null)
            _vrikCorrector.SetVRIK(vrik, this);
        if (_vrik != null && _playerVrikCorrector != null)
            _playerVrikCorrector.SetVRIK(vrik, this);
    }

    public void SetName(string name)
	{
        if (_avatarName != null)
            _avatarName.text = name;
    }

    public void SetInPause(bool pause)
	{
        _isInPause = pause;
        if (_isInPause)
		{
            TeleportShowOff(transform.position);
            ShowIcon(true);
            SetName(GameflowBase.piratenames[actornumber] + "\n" + RRLib.RRLanguageManager.instance.GetString("str_playerpaused"));
        }
        else
		{
            TeleportShowOn();
            ShowIcon(false);
            SetName(GameflowBase.piratenames[actornumber]);
        }
    }

    public void ShowIcon(bool show)
	{
        if (_icon != null)
        {
            _icon.gameObject.SetActive(show);
            if (show)
            {
                _icon.SetPlayerIcon(gamesettings_player.myself.GetSkinIndexFromName(_skinName));
            }
        }
	}

    public void SetMyAttachedObjects()
    {
#if DEBUG_AVATAR
        Debug.Log("[AVATAR] SetMyAttachedObjects for " + actornumber);
#endif

        left_hand_objects = new List<attachobject>();
        right_hand_objects = new List<attachobject>();
        belt_left_objects = new List<attachobject>();
        belt_right_objects = new List<attachobject>();
        belt_front_objects = new List<attachobject>();
        belt_back_objects = new List<attachobject>();

        VRIK[] allvrik = gameObject.GetComponentsInChildren<VRIK>(true);
        foreach (VRIK vrik in allvrik)
        {
            GameObject sk = vrik.gameObject;
            if (sk.name == _skinName)
            {
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
            }
        }
#if DEBUG_AVATAR
        Debug.Log("[AVATAR] SetMyAttachedObjects for " + actornumber + " left_hand_objects count " + left_hand_objects.Count);
#endif
    }

    private void OtherPlayerInitialization()
    {
        DestroyDummyAvatarStructure();
/*
        if (playerHead != null)
            playerHead.SetActive(true);
        if (hat != null)
            hat.SetActive(true);
            */
    }

    void DestroyDummyAvatarStructure()
    {
        foreach(attachobject aobj in gameObject.GetComponentsInChildren<attachobject>(true))
        {
            ProjectileCannon pc = aobj.gameObject.GetComponent<ProjectileCannon>();
            if (pc) Destroy(pc);
            InfiniGun ig = aobj.gameObject.GetComponent<InfiniGun>();
            if (ig) Destroy(ig);
            BoxCollider bc = aobj.gameObject.GetComponent<BoxCollider>();
            if (bc) Destroy(bc);
            Rigidbody rb = aobj.gameObject.GetComponent<Rigidbody>();
            if (rb) Destroy(rb);
        }
        // need to rethink collissions
        //        if (gameObject.FindInChildren("PlayerBodyCollider") != null) Destroy(gameObject.FindInChildren("PlayerBodyCollider"));

        foreach (BeautifyEffect.Beautify beauty in gameObject.GetComponentsInChildren<BeautifyEffect.Beautify>())
            Destroy(beauty);
        foreach (StylizedWaterShader.EnableDepthBuffer depth in gameObject.GetComponentsInChildren<StylizedWaterShader.EnableDepthBuffer>())
            Destroy(depth);
        foreach (AmplifyColorEffect coleff in gameObject.GetComponentsInChildren<AmplifyColorEffect>())
            Destroy(coleff);
        foreach (MK.Glow.MKGlow glow in gameObject.GetComponentsInChildren<MK.Glow.MKGlow>())
            Destroy(glow);

        foreach (BoxCollider boxcol in gameObject.GetComponentsInChildren<BoxCollider>())
            Destroy(boxcol);
        foreach (SphereCollider spherecol in gameObject.GetComponentsInChildren<SphereCollider>())
            Destroy(spherecol);
        foreach (Camera cams in gameObject.GetComponentsInChildren<Camera>())
            Destroy(cams);
        foreach (pointfromhand pnt in gameObject.GetComponentsInChildren<pointfromhand>())
            DestroyImmediate(pnt.gameObject);
        foreach (Controllers cnt in gameObject.GetComponentsInChildren<Controllers>())
            Destroy(cnt);
        foreach (AudioListener listen in gameObject.GetComponentsInChildren<AudioListener>())
            Destroy(listen);

#if STEAM_PRESENT
        foreach (SteamVR_Behaviour_Pose poses in gameObject.GetComponentsInChildren<SteamVR_Behaviour_Pose>())
            Destroy(poses);
#else
            foreach (OVRHeadsetEmulator headset in gameObject.GetComponentsInChildren<OVRHeadsetEmulator>())
                Destroy(headset);
            foreach (OVRManager manager in gameObject.GetComponentsInChildren<OVRManager>())
                Destroy(manager);
            foreach (OVRCameraRig rig in gameObject.GetComponentsInChildren<OVRCameraRig>())
                Destroy(rig);
#endif
    }


    public static void SetSkin(int id,string skinname)
    {
        foreach(Player_avatar pa in Player.myplayer.avatars)
        {
            if (pa.actornumber == id)
            {
                pa.SetSkinName(skinname);
                break;
            }
        }
    }

    public static void SetCustomHat(int id, string customHat, CustomPackSettings customPackSettings)
	{
        foreach (Player_avatar pa in Player.myplayer.avatars)
        {
            if (pa.actornumber == id)
            {
                pa.SetCustomHat(customHat, customPackSettings);
                break;
            }
        }
    }

    public void SetSkinName(string skinname, bool force = false)
	{
        if (!force && skinname == _skinName)
            return;

#if DEBUG_AVATAR
        Debug.Log("[AVATAR] SetSkinName for " + actornumber + " - " + skinname);
#endif

        VRIK[] allvrik = gameObject.GetComponentsInChildren<VRIK>(true);
        foreach (VRIK vrik in allvrik)
        {
            GameObject sk = vrik.gameObject;
            if (sk.name == skinname)
            {
                _isVisible = GameflowBase.instance.AreAvatarsVisibles() && GameflowBase.IsNumActorVisible(id);
                if (_isVisible && !isInPause)
                {
                    if (!sk.activeInHierarchy)
                    {
                        ShowEffect(sk.FindInChildren("FX_Skin_Swap"));
                    }
                    sk.SetActive(true);
                }
                InitVRIK(vrik);
                _skinName = skinname;
            }
            else
            {
                sk.SetActive(false);
            }
        }
        SetMyAttachedObjects();
        UpdateThemeMaterial();
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

    public void SetCustomHat(string customHat, CustomPackSettings customPackSettings)
	{
        if (!string.IsNullOrEmpty(customHat))
        {
            multiplayerlobby.theme = multiplayerlobby.SkinTheme.Custom;
            CustomPackSettings.CustomItem item = customPackSettings.GetItem(customHat);
            UpdateCustomTheme(item.prefab);
        }
        else
        {
            multiplayerlobby.theme = multiplayerlobby.SkinTheme.Normal;
            UpdateCustomTheme();
        }
    }

    public void UpdateCustomTheme(GameObject prefab = null)
    {
        ThemeReplaceObjects[] replaceObjs = gameObject.GetComponentsInChildren<ThemeReplaceObjects>(true);
        if (replaceObjs != null && replaceObjs.Length > 0)
        {
            int layerToSet = LayerMask.NameToLayer("Player");
            foreach (ThemeReplaceObjects replaceObj in replaceObjs)
            {
                replaceObj.SetPrefabForTheme(multiplayerlobby.SkinTheme.Custom, prefab);
                replaceObj.ReplaceObjects(force: true, layer: layerToSet);
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

    public void SetHealth(float health)
	{
        float ratio = health / Player.myplayer.starthealth;
        _healthGauge.UpdateValue(ratio);
        isDead = health <= 0f;
    }

    public void TeleportShowOff(Vector3 lastPos)
    {
#if DEBUG_AVATAR
        Debug.Log($"[AVATAR] {actornumber} TeleportShowOff " + _vrik);
#endif
        _isInTeleportation = true;
        if (_vrik == null)
            _vrik = gameObject.GetComponentInChildren<VRIK>(true);
        if (_vrik != null)
        {
            _skinPlayer?.ShowPlayer(false);
            _isVisible = GameflowBase.instance.AreAvatarsVisibles() && GameflowBase.IsNumActorVisible(id);
            _vrik.gameObject.SetActive(_isVisible);
            gameObject.SetActive(_isVisible);
            _playerInfos.SetActive(_isVisible);
            if (_fxTeleport != null)
            {
                if (_isVisible)
                {
                    _fxTeleport.transform.SetParent(transform.parent, true);
                    _fxTeleport.transform.position = lastPos;
                    ShowEffect(_fxTeleport);
                }
                else
				{
                    _fxTeleport.gameObject.SetActive(false);
                }
            }
        }
    }

    public void TeleportShowOn()
    {
#if DEBUG_AVATAR
        Debug.Log($"[AVATAR] {actornumber} TeleportShowOn " + _vrik);
#endif
        _isInTeleportation = false;
        if (_vrik == null)
            _vrik = gameObject.GetComponentInChildren<VRIK>(true);
        if (_vrik != null)
        {
            _skinPlayer?.ShowPlayer(true);
            _isVisible = GameflowBase.instance.AreAvatarsVisibles() && GameflowBase.IsNumActorVisible(id);
            gameObject.SetActive(_isVisible);
            _vrik.gameObject.SetActive(_isVisible);
            _playerInfos.SetActive(_isVisible);
            if (_fxTeleport != null)
            {
                if (_isVisible)
                {
                    ShowEffect(_fxTeleport);
                    _fxTeleport.transform.SetParent(transform);
                    _fxTeleport.transform.localPosition = Vector3.zero;
                    _fxTeleport.transform.localScale = Vector3.one;
                    _fxTeleport.transform.localRotation = Quaternion.identity;
                }
                else
				{
                    _fxTeleport.gameObject.SetActive(false);
                }
            }
        }
    }

    public static void ShowEffect(GameObject go)
    {
        if (go != null)
        {
            go.SetActive(true);
            Animation eff = go.GetComponentInChildren<Animation>(true); 
            if (eff != null)
            {
                eff.gameObject.SetActive(true);
                eff.Stop();
                eff.Rewind();
                eff.Play();
            }
        }
    }

    boat_followdummy Player.IPlayer.GetBoat()
    {
#if USE_KDK || USE_BOD
        return null;
#else
        return gameObject.GetComponentInParent<boat_followdummy>();
#endif
    }

	int Player.IPlayer.GetTeam()
	{
#if USE_KDK || USE_BOD
        return team;
#else
        boat_followdummy boat = ((Player.IPlayer)this).GetBoat();
        if (boat != null)
            return boat.team;
        return 0;
#endif
    }
}
