#define CAN_ROTATE_PLAYER_WITH_STICK
#define ROTATE_PLAYER_BY_STEP
#define CAN_TELEPORT_WITH_STICK
#define CAN_TELEPORT_WITH_TOUCHPAD

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if STEAM_PRESENT
using Valve.VR;
#else
using Oculus.Avatar;
#endif

public class pointfromhand : MonoBehaviour
{
    public static Vector3 leftHandPos = Vector3.zero;
    public static Vector3 rightHandPos = Vector3.zero;

    public bool righthand = false;
#if STEAM_PRESENT
    public SteamVR_Input_Sources isource = SteamVR_Input_Sources.Any;
    public SteamVR_Action_Boolean grabpinch;
    public SteamVR_Action_Boolean triggerpinch;
    public SteamVR_Action_Vector2 stick;
    public SteamVR_Action_Boolean touchpadbutton;
    public SteamVR_Action_Boolean pauseButton;
#else
    public OVRInput.Controller controllerInput;
#endif

    public bool pinchstate = false;
    public bool triggerstate = false;
    public bool pauseState = false;
    public int teleportstate = 0;
    public Vector2 stickpos;
    public static GameObject teleporttarget = null;
    public static Transform teleportOverride = null;
    GameObject arc;
    GameObject camerarig = null;
    public Player player = null;

    public bool handclosetoground = false;

    public bool pinchstate_trigger = false;
    public bool triggerstate_trigger = false;
    public bool _pinchstate_trigger = false;
    public bool _triggerstate_trigger = false;

    public List<attachobject> my_hand_objects;

    public bool validteleport = false;
    public Vector3 validteleportpos;
    public Collider teleportCollider = null;

    AudioSource hand_as = null;
    public bool belttouched = false;
    static int arcstate = -1;
    private bool _needToTeleport = false;
    private float _attachLimit = -0.5f;

    public static bool isUsingArc => arcstate != -1;

    [SerializeField]
    private UI_PausePopup _pausePopupPrefab = null;

    private static UI_PausePopup _pausePopup = null;
    public static bool isPausePopupVisible => _pausePopup != null;

    private static bool _isTurning = false;
    private static bool _isTurningFromRightHand = false;
    private static float _startTurningTime = 0f;

    public static bool lastTriggerStateFromRightHand = false;

    private float _handsDistance = 1f;

    private void Awake()
    {
        hand_as = gameObject.GetComponent<AudioSource>();
        player = gameObject.GetComponentInParent<Player>();
    }

    void Start()
    {
        if (righthand) 
            my_hand_objects = player.right_hand_objects;
        else
            my_hand_objects = player.left_hand_objects;

        arc = gameObject.FindInChildren("arc");
        arc.SetActive(false);

        camerarig = player.gameObject;

        if (gamesettings_player.myself != null)
		{
            _attachLimit = -Mathf.Sin(gamesettings_player.myself.attachItemBottomAngleMax * Mathf.PI / 180f);
        }
    }

    public void InitTeleporterTarget(avatar_root avatarRoot)
	{
        if (avatarRoot != null)
        {
            teleporttarget = avatarRoot.teleporterTarget;
            if (teleporttarget != null)
            {
                teleporttarget.SetActive(false);
            }
            else
            {
                Debug.LogError("No teleporter found!");
            }
        }
        else
        {
            Debug.LogError("No avatar_root found!");
        }
    }

	private void Update()
	{
        if (righthand)
            rightHandPos = transform.position;
        else
            leftHandPos = transform.position;
#if USE_STANDALONE
        //if (RaceManager.IsInstanceValid())
        {
            if (gameflowmultiplayer.isInNoVR)
            {
                pauseState = Input.GetKeyDown(KeyCode.Escape) && righthand;
            }
            else
            {
                pauseState = (Input.GetKeyDown(KeyCode.Escape) && righthand) || pauseButton.GetStateDown(isource);
            }
            if (pauseState)
            {
                if (_pausePopup == null)
                {
                    _pausePopup = GameObject.Instantiate<UI_PausePopup>(_pausePopupPrefab);
                    _pausePopup.transform.SetParent(player.camOffset);
                    _pausePopup.transform.localPosition = Vector3.zero;
                    _pausePopup.transform.localRotation = Quaternion.identity;
                    _pausePopup.transform.localScale = Vector3.one;
                    //_pausePopup.SetFromRightHand(righthand);
                }
                else
                {
                    GameObject.Destroy(_pausePopup.gameObject);
                }
            }
        }
#endif
#if USE_BOD
        if (Player.myplayer != null && TowerDefManager.myself != null && TowerDefManager.myself.canShowMiniMap)
        {
            if (righthand)
            {
                if (pauseButton.GetState(isource))
                {
                    float dist = (rightHandPos - leftHandPos).magnitude;
                    if (pauseButton.GetStateDown(isource))
                    {
                        _handsDistance = dist;
                    }
                    else
                    {
                        float move = dist - _handsDistance;
                        Player.myplayer.SetMiniMapHandMove(move);
                        _handsDistance = dist;
                    }
                }
            }
            else
            {
                bool minimapPressed = false;
                if (GameflowBase.isInNoVR)
                {
                    minimapPressed = Input.GetKeyDown(KeyCode.Escape);
                }
                else
                {
                    minimapPressed = Input.GetKeyDown(KeyCode.Escape) || pauseButton.GetStateDown(isource);
                }
                if (minimapPressed)
                {
                    Player.myplayer.ShowHideMiniMap();
                }
            }
        }
#endif
    }

	private void LateUpdate()
	{
		if (_needToTeleport)
		{
            if (teleportCollider != null && teleporttarget != null)
            {
                boat_followdummy boat = null;
#if USE_TOG
                boat = teleportCollider.gameObject.GetComponentInParent<boat_followdummy>();
#endif
                bool telOver = teleportOverride != null;
                Vector3 position = telOver ? teleportOverride.position : teleporttarget.transform.position;
                if (telOver)
				{
                    Player.myplayer.AddProgress($"TP({position.x:F2}|{position.y:F2}|{position.z:F2})");
                }
                Teleport(position, boat, telOver);
                teleportOverride = null;
                teleporttarget.SetActive(false);
            }
            _needToTeleport = false;
        }
	}

	void FixedUpdate()
    {
/*
        if (righthand)
        {
            if (noputback > 0.0f)
            {
                noputback -= Time.fixedDeltaTime;
                if (noputback < 0) noputback = 0;
            }
        }
        */
        if (camerarig != null)
        {
            float diff = Mathf.Abs(transform.position.y - camerarig.transform.position.y);
            if (diff < 0.2f)
                handclosetoground = true;
            else
                handclosetoground = false;
        }

#if STEAM_PRESENT
        if (grabpinch.GetState(isource))
#else
        if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, controllerInput))
#endif
            pinchstate = true;
        else
            pinchstate = false;
#if STEAM_PRESENT
        if (triggerpinch.GetState(isource))
#else
        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, controllerInput))
#endif
            triggerstate = true;
        else
            triggerstate = false;
#if STEAM_PRESENT
#if CAN_TELEPORT_WITH_TOUCHPAD
        if (gamesettings_player.myself.teleportWithTouchEnable)
        {
            if (touchpadbutton.GetState(isource))
                teleportstate = 10;
            else
            {
                if (teleportstate > 0) teleportstate--;
            }
        }
#endif
        if (GameflowBase.isInNoVR && righthand)
		{
            if (Input.GetKey(KeyCode.LeftShift))
			{
                teleportstate = 10;
            }
            else
			{
                if (teleportstate > 0)
                    teleportstate--;
            }
            triggerstate = Input.GetKey(KeyCode.Space);
        }

        stickpos = stick.GetAxis(isource);

        if (GameflowBase.isInNoVR && righthand)
		{
            if (Input.GetKey(KeyCode.RightArrow))
                stickpos.x = 1f;
            if (Input.GetKey(KeyCode.LeftArrow))
                stickpos.x = -1f;
        }

#if CAN_ROTATE_PLAYER_WITH_STICK
#if ROTATE_PLAYER_BY_STEP
        if (gamesettings_player.myself.rotationWithStickEnable)
        {
            if (!triggerstate && teleportstate == 0 && !_isTurning && (_startTurningTime == 0f || _isTurningFromRightHand == righthand))
            {
                if (Mathf.Abs(stickpos.x) > gamesettings_player.myself.rotationWithStickThreshold)
                {
                    _isTurningFromRightHand = righthand;
                    if (_startTurningTime == 0f)
                        _startTurningTime = Time.time;
                    if (Time.time - _startTurningTime > gamesettings_player.myself.rotationWithStickDelay)
                    {
                        _isTurning = true;
                        StartCoroutine(TurnPlayerEnum(90f * Mathf.Sign(stickpos.x)));
                        _startTurningTime = 0f;
                    }
                }
                else
                {
                    if (_startTurningTime > 0f)
                        _startTurningTime = 0f;
                }
            }
        }
#else
        float threshold = 0.2f;
        float speed = 0.5f;
        if (Mathf.Abs(stickpos.x) > threshold)
        {
            if (!_isTurning)
            {
                _isTurning = true;
                _isTurningFromRightHand = righthand;
            }

            if (_isTurningFromRightHand == righthand)
            {
                float angle = (stickpos.x - Mathf.Sign(stickpos.x) * threshold) / (1f - threshold);
                float angleSpeed = angle * speed;
                Player.myplayer.transform.Rotate(Player.myplayer.transform.up, angleSpeed);
                float invAngle = 1f - Mathf.Abs(angle);
                float brightness = Mathf.Lerp( 0.05f, 1f, invAngle * invAngle);
                gamesettings_screen.myself.SetBrightness(brightness);
            }
        }
        else
        {
            if (_isTurning && _isTurningFromRightHand == righthand)
            {
                gamesettings_screen.myself.SetBrightness(1f);
                _isTurning = false;
            }
        }
#endif
#endif


#if CAN_TELEPORT_WITH_STICK
        if (gamesettings_player.myself.teleportWithStickEnable)
        {
            if (stickpos.y > 0.9f)
            {
                if (teleportstate == 0)
                    validteleport = false;
                teleportstate = 10;
            }
            else
            {
                if (teleportstate > 0)
                    teleportstate--;
            }
        }
#endif
#else
        Vector2 vec = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, controllerInput);
        if (vec.y > 0.5f)
        {
            if (teleportstate == 0)
                validteleport = false;
            teleportstate = 10;
        }
        else
        {
            if (teleportstate > 0) teleportstate--;
        }
#endif
        if (teleporttarget != null && gamesettings_ui.myself.controltype == gamesettings_ui._controltype.teleport)
        {
            if ((arcstate == -1) || (righthand && (arcstate == 1)) || ((!righthand) && (arcstate == 2)))
            {
                if (teleportstate > 0 && !player.isInPause && player.canTeleport)
                {
                    if (righthand) arcstate = 1;
                    else
                    {
                        arcstate = 2;
                    }
                    arc.SetActive(true);

                    if (validteleport)
                    {
                        teleporttarget.transform.position = validteleportpos + camerarig.transform.position;
                        teleporttarget.SetActive(true);
                    }
                    else
                    {
                        teleporttarget.SetActive(false);
                    }
                }
                else
                {
                    if (validteleport && !player.isInPause && !gamesettings_screen.myself.faderunning)
                    {
                        if (teleporttarget.activeInHierarchy)
                            _needToTeleport = true;
                        validteleport = false;
                    }
                    arcstate = -1;
                    arc.SetActive(false);
                    if (!_needToTeleport)
                        teleporttarget.SetActive(false);
                }
            }
        }


        if (pinchstate)
        {
            if (!_pinchstate_trigger)
            {
                pinchstate_trigger = true;
                //                foreach (attachobject handatt in my_hand_objects)
                //                    handatt.gameObject.SendMessage("Grabbed",SendMessageOptions.DontRequireReceiver);
            }
            else
                pinchstate_trigger = false;
            _pinchstate_trigger = true;
        }
        else
            _pinchstate_trigger = false;
        if (triggerstate)
        {
            if (!_triggerstate_trigger)
            {
                if (OnHover.lastButtonHover != null && !OnHover.needPlayerBullet)
                {
                    OnHover.lastButtonHover.onClick.Invoke();
                }

                triggerstate_trigger = true;
                lastTriggerStateFromRightHand = righthand;
                foreach (attachobject handatt in my_hand_objects)
                {
                    if (handatt.gameObject.activeInHierarchy)
                    {
                        if (!CanAttach())
                            handatt.gameObject.SendMessage("Triggered", SendMessageOptions.DontRequireReceiver);
                    }
                }
                _triggerstate_trigger = true;
            }
            else
            {
                triggerstate_trigger = false;
            }
        }
        else
        {
            if (_triggerstate_trigger)
            {
                foreach (attachobject handatt in my_hand_objects)
                {
                    if (handatt.gameObject.activeInHierarchy)
                    {
                        if (!CanAttach())
                            handatt.gameObject.SendMessage("Released", SendMessageOptions.DontRequireReceiver);
                    }
                }
                _triggerstate_trigger = false;
            }
        }
    }

    private IEnumerator TurnPlayerEnum(float angle)
	{
        gamesettings_screen.myself.FadeOut(0.2f);
        while (gamesettings_screen.myself.faderunning)
            yield return null;
        Vector3 pos = Player.myplayer.GetFootPos();
        Player.myplayer.transform.Rotate(Vector3.up, angle, Space.Self);
        Player.myplayer.Teleport(pos);
        gamesettings_screen.myself.FadeIn();
        yield return new WaitForSeconds(0.5f);
        _isTurning = false;
    }

    public void Teleport(Vector3 targetplace, boat_followdummy boat = null, bool longTeleport = false)
    {
//        Debug.Log("TELEPORT CALLED");
        Player.myplayer.TelePorter(camerarig, targetplace, boat, longTeleport);
    }

    public static float noputback = 0.0f;
    public static GameObject noputbackobject = null;

    void HandEmpty_MalletsBack()
    {
        Debug.Log("Release Object");
        ActivateMallets();
    }

    void PrepareValidity()
    {
        DeactivateMallets();
    }

    public void DeactivateMallets()
    {
        if (player != null)
        {
            player.DeactivateMallets();
        }
    }
    public void ActivateMallets()
    {
        if (player != null)
        {
            player.ActivateMallets();
        }
    }

    public bool CanAttach()
	{
        GameObject controller = righthand ? player.rightController : player.leftController;
        if (Player.myplayer.GetWeaponAtPlace(righthand ? Player.WeaponPlace.RightHand : Player.WeaponPlace.LeftHand) == Player.WeaponType.Canon_Torche)
            return false;
        if (controller.transform.forward.y < _attachLimit)
            return belttouched;
        return false;
    }

    public void HandTouched(GameObject obj)
    {
        if (!triggerstate_trigger) 
            return;
        triggerstate_trigger = false;

        // Don't take avatar items
        Player_avatar avatar = obj.GetComponentInParent<Player_avatar>();
        if (avatar != null)
            return;

        bool emptyhand = true;
        foreach (attachobject handatt in my_hand_objects)
        {
            if (handatt.gameObject.activeInHierarchy)
            {
                if (!emptyhand)
                    handatt.gameObject.SetActive(false);
                emptyhand = false;       // no pick up .. hand already full
            }
        }

        InventorySlot invslot = obj.GetComponent<InventorySlot>();
        if (invslot != null)
        {
            List<attachobject> belt_objects = player.belt_right_objects;
            
            switch (invslot.slotPosition)
            {
                case InventorySlot.SlotPosition.Left:
                    belt_objects = player.belt_left_objects;
                    break;
                case InventorySlot.SlotPosition.Front:
                    belt_objects = player.belt_front_objects;
                    break;
                case InventorySlot.SlotPosition.Back:
                    belt_objects = player.belt_back_objects;
                    break;
            }

            bool noobjects = true;
            foreach (attachobject handatt in belt_objects)
            {
                if (handatt.gameObject.activeInHierarchy)
                {
                    if (!noobjects)
                        handatt.gameObject.SetActive(false);
                    noobjects = false;
                }
            }
            
            if (emptyhand)
            {
                if (noobjects) return;
                foreach (attachobject beltatt in belt_objects)
                {
                    if (beltatt.gameObject.activeInHierarchy)
                    {
                        foreach (attachobject handatt in my_hand_objects)
                        {
                            if (handatt.gameObject.name == beltatt.gameObject.name)
                            {
//                                touchtime += Time.deltaTime;
//                                if (touchtime > 0.5f)
                                {
                                    handatt.gameObject.SetActive(true);
                                    beltatt.gameObject.SetActive(false);
                                    Player.myplayer.UpdateWeaponsData();
                                    UI_Tutorial.myself?.OnTriggerCondition(UI_Tutorial.TutoCondition.WeaponOnHand);
                                    if (hand_as)
                                    {
                                        if (gamesettings.myself)
                                            hand_as.PlayOneShot(gamesettings.myself.pickupobject);
                                    }
                                    PrepareValidity();
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (!noobjects) return;     // already an object
                // Attach to belt

                if (!CanAttach())
                    return;
                    
                foreach (attachobject handatt in my_hand_objects)
                {
                    if (handatt.gameObject.activeInHierarchy)
                    {
                        foreach (attachobject beltatt in belt_objects)
                        {
                            if (handatt.gameObject.name == beltatt.gameObject.name)
                            {
//                                touchtime += Time.deltaTime;
//                                if (touchtime > 0.5f)
                                {
                                    Animator anim = player.currentskin.GetComponent<Animator>();
                                    if (righthand)
                                    {
                                        anim.ResetTrigger("Weapon_R");
                                        anim.ResetTrigger("Handle_R");
                                        anim.ResetTrigger("Hand_Interaction_R");
                                        anim.SetTrigger("Idle_R");
                                    }
                                    else
                                    {
                                        anim.ResetTrigger("Weapon");
                                        anim.ResetTrigger("Handle");
                                        anim.ResetTrigger("Hand_Interaction");
                                        anim.SetTrigger("Idle");
                                    }
                                    handatt.gameObject.SetActive(false);
                                    beltatt.gameObject.SetActive(true);
                                    Player.myplayer.UpdateWeaponsData();

                                    HandEmpty_MalletsBack();
                                    if (hand_as != null && gamesettings.myself)
                                    {
                                        hand_as.PlayOneShot(gamesettings.myself.attachobject);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return;
        }

        if (!emptyhand) return;
//        if (!pinchstate_trigger) return;
//        if (!triggerstate_trigger) return;

        attachobject att = obj.GetComponent<attachobject>();
        foreach (attachobject handatt in my_hand_objects)
        {
            if (handatt.gameObject.name == att.show_in_hand)
            {
                noputback = 0.5f;
                noputbackobject = handatt.gameObject;

                obj.SetActive(false);
                handatt.gameObject.SetActive(true);
                hand_as.PlayOneShot(gamesettings.myself.pickupobject);
                PrepareValidity();
                Player.myplayer.UpdateWeaponsData();
                break;
            }
        }
    }

    
}
