using DynamicFogAndMist;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class endlesscord : MonoBehaviour
{
    public Valve.VR.InteractionSystem.LinearMapping linear = null;
    public float cordEnd = 4.25f;
    public float releasespeed_persecond = 0.25f;
    public float releasespeedAtStart_persecond = 0.1f;
    public float timeBeforeReleasingAll = 1f;
    public float timeToKeepAtCordEnd = 1f;
    public float timeToResetCord = 1f;
    public AnimationCurve distanceCurveAnim = null;
    public float showedCordDistance = 0f;
    public GameObject[] cordCylinders = null;
    public Material cordCylinderMaterialOff = null;
    public Animator pipeAnimator = null;
    public Transform[] ghostHandsTrArray = null;
    public float ghostHandMinY = 0f;
    public float ghostHandMaxY = 1f;
    public Slider[] slidersToUpdate = null;

    pointfromhand registeredhand = null;
    //GameObject registerhelphand = null;
    float registerhandpos;

    TextMesh showpos;
    float startz;
    GameObject myendlesscord;
    int myteam = 0;
    private bool _isCordAtEnd = false;
    private bool _isCordReleasing = false;
    private float _lastReleaseTime = 0f;
    private float _startCordAtEndTime = 0f;
    private float _smoothedCordDistance = -1f;
    private MeshRenderer[] cordCylinderRenderers = null;
    private Material[] initialCordCylinderMaterials = null;
    private boat_followdummy _boat = null;
    private float _resetCordTime = 0f;

    void Awake()
    {
        myendlesscord = gameObject.FindInChildren("endlesscord");
        showpos = gameObject.FindInChildren("text").GetComponent<TextMesh>();
    }

    // Start is called before the first frame update
    private void Start()
    {
        GameObject obj = gameObject;
        _boat = obj.GetComponentInParent<boat_followdummy>();
        if (_boat != null)
            myteam = _boat.team;
        if (cordCylinders != null && cordCylinders.Length > 0)
        {
            cordCylinderRenderers = new MeshRenderer[cordCylinders.Length];
            initialCordCylinderMaterials = new Material[cordCylinders.Length];
            for (int i = 0; i < cordCylinders.Length; ++i)
            {
                MeshRenderer mr = cordCylinders[i].GetComponent<MeshRenderer>();
                cordCylinderRenderers[i] = mr;
                initialCordCylinderMaterials[i] = mr.material;
            }
        }
        if (timeToKeepAtCordEnd > 0 && gamesettings_boat.myself != null)
        {
            float cordLiftDuration = cordEnd / releasespeed_persecond;
            //Debug.Log("Raph - cordLiftDuration " + cordLiftDuration);
            timeToKeepAtCordEnd = Mathf.Max(timeToKeepAtCordEnd, gamesettings_boat.myself.boat_speed_modifier_duration - cordLiftDuration);
        }
    }

    private void OnDestroy()
    {
        registeredhand = null;
        _boat = null;
        //registerhelphand = null;
    }


    private float GetCordDistance()
	{
        return Mathf.Max(gameflowmultiplayer.GetCordDistanceRatio(myteam), 0f) * cordEnd;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos;
        float cordDistance = GetCordDistance();
        bool canUseCord = CanUseCord();

        if (cordCylinderRenderers != null && cordCylinderRenderers.Length > 0)
        {
            for (int i = 0; i < cordCylinderRenderers.Length; ++i)
            {
                cordCylinderRenderers[i].material = canUseCord ? initialCordCylinderMaterials[i] : cordCylinderMaterialOff;
            }
        }

        if (ghostHandsTrArray != null)
		{
            foreach (Transform tr in ghostHandsTrArray)
			{
                float posY = tr.localPosition.y + cordDistance;
                tr.gameObject.SetActive(canUseCord && posY >= ghostHandMinY && posY <= ghostHandMaxY);
			}
		}

        if (canUseCord)
        {
            if ((GameflowBase.isInNoVR && Input.GetKey(KeyCode.C) && _boat.aiBoat == null)
                || (registeredhand != null && registeredhand.triggerstate)
                || (_boat.aiBoat != null && _boat.aiBoat.needToUseSail))
            {
                if (!gameflowmultiplayer.IsPullingCord(myteam))
                {
                    if (registeredhand != null)
                    {
                        registerhandpos = registeredhand.transform.position.y - transform.position.y;
                        startz = cordDistance;
                    }
                    gameflowmultiplayer.SetPullingCord(true, myteam);
                    gameflowmultiplayer.SetCordPlayerId(gameflowmultiplayer.myId, myteam);
                    _resetCordTime = 0f;
                    _smoothedCordDistance = 0f;
                }
                if (GameflowBase.isInNoVR || _boat.aiBoat != null)
                {
                    cordDistance += Time.deltaTime;
                }
                else
				{
                    float diffDistance = registeredhand.transform.position.y - transform.position.y - registerhandpos;
                    cordDistance = Mathf.Max(startz - diffDistance, 0f);
                }

                if (cordDistance > cordEnd)
                {
                    if (timeToKeepAtCordEnd > 0)
                    {
                        _isCordAtEnd = true;
                        SetPipeAnimationState(true);
                    }
                    else
                    {
                        ReleaseCord(true);
                    }
                    cordDistance = cordEnd;
                    if (UI_Tutorial.myself != null && myteam == Player.myplayer.team)
                        UI_Tutorial.myself.OnTriggerCondition(UI_Tutorial.TutoCondition.Rope);
                }
                gameflowmultiplayer.SetCordDistanceRatio(cordDistance / cordEnd, myteam);
                _lastReleaseTime = 0f;
            }
            else
            {
                ReleaseCord();
            }
        }
        else if (gameflowmultiplayer.IsCordPlayerIdMyId(myteam))
        {
            ReleaseCord(_boat.isSinking);
        }

        if (_resetCordTime > 0f)
		{
            UpdateCordResetting();
        }
        else if (gameflowmultiplayer.IsCordPlayerIdMyId(myteam) || _isCordReleasing)
        {
            UpdateCordReleasing();

            if (_isCordReleasing || (!gameflowmultiplayer.IsPullingCord(myteam) && !_isCordAtEnd))
            {
                float speed = _isCordReleasing ? releasespeed_persecond : releasespeedAtStart_persecond;
                float diff = -speed * Time.deltaTime;
                pos = myendlesscord.transform.localPosition;
                cordDistance += diff;
                if (cordDistance < 0.0f)
                {
                    diff -= cordDistance;
                    cordDistance = 0f;
                    _isCordReleasing = false;
                    gameflowmultiplayer.SetCordDistanceRatio(-1f, myteam);
                    if (timeToResetCord > 0f)
                        _resetCordTime = Time.time;
                }
                else
                {
                    gameflowmultiplayer.SetCordDistanceRatio(cordDistance / cordEnd, myteam);
                }
                pos.y += diff;
                myendlesscord.transform.localPosition = pos;
            }
            else
            {
                pos = myendlesscord.transform.localPosition;
                pos.y = cordDistance;
                myendlesscord.transform.localPosition = pos;
            }
            showedCordDistance = cordDistance;
        }
        else
        {
            _isCordReleasing = false;
            if (_smoothedCordDistance < 0f)
                _smoothedCordDistance = cordDistance;
            _smoothedCordDistance = Mathf.Lerp(_smoothedCordDistance, cordDistance, Time.deltaTime * 4f);
            pos = myendlesscord.transform.localPosition;
            pos.y = _smoothedCordDistance;
            myendlesscord.transform.localPosition = pos;
            showedCordDistance = _smoothedCordDistance;
            cordDistance = _smoothedCordDistance;
        }

        if (linear != null)
        {
            float ratio = cordDistance / cordEnd;
            linear.value = distanceCurveAnim != null ? distanceCurveAnim.Evaluate(ratio) : ratio;
            if (slidersToUpdate != null)
			{
                foreach (Slider slider in slidersToUpdate)
				{
                    slider.value = Mathf.Lerp(slider.minValue, slider.maxValue, ratio);
				}
			}
        }

        if (_isCordAtEnd)
        {
            if (_startCordAtEndTime == 0f)
            {
                _startCordAtEndTime = Time.time;
            }
            else if (Time.time - _startCordAtEndTime > timeToKeepAtCordEnd)
            {
                ReleaseCord(true);
                _isCordAtEnd = false;
                _startCordAtEndTime = 0f;
                SetPipeAnimationState(false);
            }
        }

#if UNITY_EDITOR
        showpos.text = cordDistance.ToString("F2");
#endif
    }

    //private void OnTriggerExit(Collider other)
    //{
    //    Debug.Log("Raph - OnTriggerExit CanUseCord()" + CanUseCord() + " registeredhand" + registeredhand);
    //    if (!CanUseCord())
    //        return;

    //    pointfromhand hand = other.gameObject.GetComponent<pointfromhand>();
    //    if (registeredhand == hand)
    //    {
    //        ReleaseCord();
    //        registeredhand = null;
    //        registerhelphand = null;
    //    }
    //}

    private void OnTriggerStay(Collider other)
    {
        //Debug.Log("Raph - OnTriggerStay CanUseCord()" + CanUseCord() + " registeredhand" + registeredhand);

        if (!CanUseCord())
        {
            registeredhand = null;
            return;
        }

        pointfromhand hand = other.gameObject.GetComponent<pointfromhand>();
        if (hand != null && hand.triggerstate && pointfromhand.lastTriggerStateFromRightHand == hand.righthand)
        {
            if (registeredhand == null)
			{
                registeredhand = hand;
            }
            else if (hand != registeredhand)
            {
                ReleaseCord();
            }
        }
    }

    private bool CanUseCord()
    {
        return (myteam == GameflowBase.myTeam || (_boat.aiBoat != null && PhotonNetworkController.IsMaster()))
            && !_isCordReleasing && !_isCordAtEnd && gameflowmultiplayer.CanUseCord(myteam) && !_boat.isSinking;
    }

    private void ReleaseCord(bool releasingTotally=false)
    {
        registeredhand = null;
        if (gameflowmultiplayer.IsPullingCord(myteam))
            gameflowmultiplayer.SetPullingCord(false, myteam);
        if (_lastReleaseTime == 0f && (releasingTotally || timeBeforeReleasingAll > 0f))
        {
            if (releasingTotally)
                _isCordReleasing = true;
            else
                _lastReleaseTime = Time.time;
        }
    }

    private void UpdateCordReleasing()
    {
        if (timeBeforeReleasingAll > 0f && _lastReleaseTime > 0f && Time.time - _lastReleaseTime > timeBeforeReleasingAll)
        {
            _isCordReleasing = true;
            _lastReleaseTime = 0f;
        }
    }

    private void UpdateCordResetting()
	{
        if (Time.time - _resetCordTime > timeToResetCord)
		{
            if (gameflowmultiplayer.IsCordPlayerIdMyId(myteam))
                gameflowmultiplayer.SetCordPlayerId(-1, myteam);
            _resetCordTime = 0f;
        }
    }

    private void SetPipeAnimationState(bool areSailsOpened)
	{
        if (pipeAnimator != null)
		{
            pipeAnimator.SetBool("SailsOpened", areSailsOpened);
		}
	}
}
