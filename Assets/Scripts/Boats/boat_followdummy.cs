//#define DEBUG_BOAT_VALUES

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class boat_followdummy : MonoBehaviour
{
    #region Delegates

    public delegate void OnGoldUpdated(int gold);

    #endregion

    #region Enums

    public enum TeamColor
    {
        Blue,
        Red
    }

    #endregion

    public TeamColor teamColor => (TeamColor)team;
    public PathFollower pathFollower
    {
        get
        {
            if (_pathFollower != null)
                return _pathFollower;
            if (dummy != null)
                _pathFollower = dummy.GetComponent<PathFollower>();
            return _pathFollower;
        }   
    }

    public PathHall path
    {
        get
        {
            return pathFollower?.path;
        }
        set
        {
            if (pathFollower != null)
                pathFollower.SetupNewPathHall(value);
        }
    }

    public Health health => _health;
    private Health _health = null;

    public bool isSinking => _boatSinking.isSinking;
    public bool isSunken => _boatSinking.isSunken;
    public boat_sinking boatSinking => _boatSinking;
    public boat_minimap boatMinimap => _boatMinimap;
    public boat_endlessUI boatEndlessUI => _boatEndlessUI;
    public boat_finishFeedback boatFinishFeedback => _boatFinishFeedback;
    public boat_resultScreen boatResultScreen => _boatResultScreen;

    public string dummy_identifier_name = "";
    public int team = 0;
    public GameObject dummy = null;
    public float sinkposition = 0.0f;
    
    public float shield_time = 0.0f;

    public string boatmeshname = "SM_Veh_Boat_Medium_01_Hull_Pirate_Attachments";
    /*
    public float min_brightness = 0.0f;
    public float max_brightness = 1.0f;
    public float min_saturation = 0.0f;
    public float max_saturation = 1.0f;
    */
    Material boatmeshmaterial = null;
    public string captaingender = "Man";

    public bool hasReachedFinishLine = false;

#if DEBUG_BOAT_VALUES
    public bool DebugBoatValues = true;
    public Text BoatLife_Value = null;
    public Text MyBoatLife_Value = null;
    public Text Damage01_Value = null;
    public Text Damage02_Value = null;
    public Text Damage03_Value = null;
    public Text Damage04_Value = null;
#endif

    [SerializeField]
    private GameObject visualBoat = null;
    [SerializeField]
    private GameObject ghostBoatPrefab = null;

    private boat_shield _shield = null;
    public boat_shield shield => _shield;
    public bool isShieldActivated => _shield.gameObject.activeInHierarchy;

    private Slider sliderleft = null;
    private Slider sliderright = null;
    private Renderer _lifeGaugeRender = null;
    private Material _matLifeGaugeNormal = null;
    private Material _matLifeGaugeSunken = null;
    private TextMeshProUGUI Water_Bucket_Value_Txt1;
    private TextMeshProUGUI Water_Bucket_Value_Txt2;

    float lasthealth = 0.0f;
    private PathFollower _pathFollower = null;
    private boat_sinking _boatSinking = null;
    private boat_minimap _boatMinimap = null;
    private boat_endlessUI _boatEndlessUI = null;
    private boat_finishFeedback _boatFinishFeedback = null;
    private boat_resultScreen _boatResultScreen = null;

    public AIBoat aiBoat => _aiBoat;
    private AIBoat _aiBoat = null;

    public int goldOnBoat => Mathf.Max( _wonGold - _stolenGold, 0);

    public int wonGold => _wonGold;
    private int _wonGold = 0;

    public int stolenGold => _stolenGold;
    private int _stolenGold = 0;
    
    public bool canRegenerateHealth => _canRegenerateHealth;
    private bool _canRegenerateHealth = true;

    public OnGoldUpdated onGoldUpdatedCallback = null;

    private float speedBoost;

	private void Awake()
	{
        _health = gameObject.GetComponentInChildren<Health>(true);
        _boatSinking = gameObject.GetComponentInChildren<boat_sinking>(true);
        _shield = gameObject.GetComponentInChildren<boat_shield>(true);
        _boatFinishFeedback = gameObject.GetComponentInChildren<boat_finishFeedback>(true);
        _boatResultScreen = gameObject.GetComponentInChildren<boat_resultScreen>(true);
        _boatMinimap = gameObject.GetComponentInChildren<boat_minimap>(true);
        _boatEndlessUI = gameObject.GetComponentInChildren<boat_endlessUI>(true);
        _boatEndlessUI.gameObject.SetActive(false);
    }

	IEnumerator Start()
    {
#if DEBUG_BOAT_VALUES
        BoatLife_Value = gameObject.FindInChildren("BoatLife_Value").GetComponent<Text>();
        MyBoatLife_Value = gameObject.FindInChildren("MyBoatLife_Value").GetComponent<Text>();
        Damage01_Value = gameObject.FindInChildren("Damage01_Value").GetComponent<Text>();
        Damage02_Value = gameObject.FindInChildren("Damage02_Value").GetComponent<Text>();
        Damage03_Value = gameObject.FindInChildren("Damage03_Value").GetComponent<Text>();
        Damage04_Value = gameObject.FindInChildren("Damage04_Value").GetComponent<Text>();
#endif

        while (!PhotonNetworkController.myself.ready)
            yield return null;

        if (string.IsNullOrEmpty(dummy_identifier_name))
            yield break;

        if (PhotonNetworkController.IsMaster())
        {
            GameObject obj = PhotonNetworkController.InstantiateSoloOrMulti(dummy_identifier_name, Vector3.zero, Quaternion.identity);
            obj.name = dummy_identifier_name;
            obj.SetActive(true);
        }
        while (dummy == null)
        {
            dummy = GameObject.Find(dummy_identifier_name);
            if (dummy == null)
                dummy = GameObject.Find(dummy_identifier_name + "(Clone)");
            yield return null;
        }
        if (PhotonNetworkController.IsMaster())
        {
            gamesettings_boat gb = gamesettings_boat.myself;

            if (pathFollower != null && gb.override_boat_speeds)
            {
                switch (teamColor)
                {
                    case TeamColor.Red:
                        pathFollower.InitSpeed(gb.boat01_speed_min, gb.boat01_speed_max, gb.boat01_speed_start);
                        break;
                    case TeamColor.Blue:
                        pathFollower.InitSpeed(gb.boat02_speed_min, gb.boat02_speed_max, gb.boat02_speed_start);
                        break;
                }
                
            }
        }
        else
        {
            RRObjective.RRObjectivePlayer delrr = dummy.GetComponent<RRObjective.RRObjectivePlayer>();
            if (delrr != null)
                Destroy(delrr);
            PathFollowerController delpfc = dummy.GetComponent<PathFollowerController>();
            if (delpfc != null)
                Destroy(delpfc);
            PathFollower delpf = pathFollower;
            if (delpf != null)
                Destroy(delpf);
        }

        // Minimap drawing
        if (_boatMinimap != null)
		{
            _boatMinimap.SetDrawing(gamesettings.myself.drawingMinimap);
		}

        // Init gold counter
        AddGold(0);

        while (GameflowBase.areAllRacesStarted)
            yield return null;

        yield return new WaitForSeconds(10f);

        if ((gameflowmultiplayer.magneticBoatBlue && teamColor == TeamColor.Blue) ||
            (gameflowmultiplayer.magneticBoatRed && teamColor == TeamColor.Red))
        {
            while (true)
            {
                foreach (Health h in RaceManager.myself.GetComponentsInChildren<Health>(true))
                {
                    if (h.currentHealth > 0f && (h.isTreasureType || h.isBonusType))
                    {
						if (Vector3.Distance(h.transform.position, transform.position) < 100f)
						{
                            h.lasthitted = Player.myplayer.gameObject;
                            h.ChangeHealth(-10000f);
                            //Debug.Log($"TotalGold kill {h.deathGain} in {GetPath(h.transform)} of type {h.healtObjectType} -> {_goldOnBoat}");
                        }
                    }
                }
                yield return new WaitForSeconds(2f);
            }
        }
    }

    public static string GetPath(Transform current)
    {
        if (current.parent == null)
            return "/" + current.name;
        return GetPath(current.parent) + "/" + current.name;
    }

    private void OnDestroy()
    {
        _shield = null;
        _health = null;
        _pathFollower = null;
        _boatSinking = null;
        _boatFinishFeedback = null;
        _boatResultScreen = null;
        _boatMinimap = null;
        _aiBoat = null;
        _boatEndlessUI = null;
    }

    void Update()
    {
        speedBoost -= Time.deltaTime;

        if (!gameflowmultiplayer.areAllRacesLoaded)
            return;

        // TO DEBUG VOICEOVER START
        //if (team == gameflowmultiplayer.myteam && Input.GetKeyDown(KeyCode.KeypadPlus))
        //    PlayCaptainVoice("Vo_LifeJauge30");
        // TO DEBUG VOICEOVER END

        if (dummy != null)
        {
            gamesettings_boat gsBoat = gamesettings_boat.myself;

            if (sliderleft == null)
            {
                GameObject obj = gameObject.FindInChildren("UI_Boat_Infos_Left");
                if (obj)                sliderleft = obj.GetComponentInChildren<UnityEngine.UI.Slider>();
                obj = gameObject.FindInChildren("UI_Boat_Infos_Right");
                if (obj) sliderright = obj.GetComponentInChildren<UnityEngine.UI.Slider>();

                obj = gameObject.FindInChildren("Pump_Rear_Right").FindInChildren("Water_Bucket_Value_Txt");
                if (obj)
                {
                    Water_Bucket_Value_Txt1 = obj.GetComponent<TextMeshProUGUI>();
                    Water_Bucket_Value_Txt1.text = "";
                }
                obj = gameObject.FindInChildren("Pump_Rear_Left").FindInChildren("Water_Bucket_Value_Txt");
                if (obj)
                {
                    Water_Bucket_Value_Txt2 = obj.GetComponent<TextMeshProUGUI>();
                    Water_Bucket_Value_Txt2.text = "";
                }

                obj = gameObject.FindInChildren("ICO_Bucket");
                if (obj) obj.SetActive(false);

                obj = gameObject.FindInChildren("UI_LifeJauge");
                if (obj)
                {
                    _lifeGaugeRender = obj.GetComponent<Renderer>();
                    if (_lifeGaugeRender != null)
                    {
                        _matLifeGaugeNormal = new Material(boatSinking.matLifeGaugeNormal);
                        _matLifeGaugeSunken = new Material(boatSinking.matLifeGaugeSunken);
                        _matLifeGaugeNormal.SetFloat("_Height", 100.0f);
                        _matLifeGaugeNormal.SetFloat("_Speed", 0.0f);
                        _matLifeGaugeSunken.SetFloat("_Height", 100.0f);
                        _matLifeGaugeSunken.SetFloat("_Speed", 0.0f);
                        _lifeGaugeRender.material = _matLifeGaugeNormal;
                    }
                }
                obj = gameObject.FindInChildren(boatmeshname);
                if (obj)
                {
                    Renderer rnd = obj.GetComponent<Renderer>();
                    boatmeshmaterial = rnd.material;
                }
            }
            else
            {
                float h = _health.GetCurrentHealth();
                if (h != lasthealth)
                {
                    float scale = (sliderleft.maxValue - sliderleft.minValue);
                    float max = _health.maxHealth;
                    float percentage = ((h * scale) / max)+ sliderleft.minValue;
                    if (percentage < 0.0f) percentage = 0;
                    if (sliderleft)                    sliderleft.value = percentage;
                    if (sliderright)                    sliderright.value = percentage;
                    if (_lifeGaugeRender != null)
                    {
                        _lifeGaugeRender.material = ((isSinking || h == 0f) && h != _health.maxHealth) ? _matLifeGaugeSunken : _matLifeGaugeNormal;
                        if (isSinking)
                        {
                            float inversed = (100.0f - percentage);
                            int playerCountInBoat = gameflowmultiplayer.GetActorCountInTeam(team);
                            float pumpCountMax = gsBoat.GetPumpCounter(playerCountInBoat);
                            float tmp = (inversed * pumpCountMax)/100.0f;
                            int itmp = (int)Mathf.Min(tmp + 1, pumpCountMax);

                            //if (itmp == 0) _health.SetCurrentHealth(_health.maxHealth);
                            Water_Bucket_Value_Txt1.text = itmp.ToString();
                            Water_Bucket_Value_Txt2.text = itmp.ToString();
                        }
                        
                        float fsize = gsBoat.jauge_high_limit - gsBoat.jauge_low_limit;
                        fsize = fsize * percentage / 100.0f;
                        float fullval = gsBoat.jauge_high_limit - fsize;
                        _lifeGaugeRender.material.SetFloat("_DissolveAmount", fullval);
                        
                        if (boatmeshmaterial != null)
                        {
                            float ratio = percentage / 100.0f;
                            boatmeshmaterial.SetFloat("_DissolveAmount", ratio);
                                /*
                            tmp = ((max_brightness- min_brightness) * percentage / 100.0f) +min_brightness;
                            boatmeshmaterial.SetFloat("_Brightness", tmp);
                            tmp = ((max_saturation - min_saturation) * percentage / 100.0f) + min_saturation;
                            boatmeshmaterial.SetFloat("_Saturation", tmp);
                            */
                        }
                    }
                }
                lasthealth = h;
            }

            transform.position = Vector3.Lerp(transform.position, dummy.transform.position, Time.deltaTime * (5f + speedBoost));
            transform.rotation = Quaternion.Lerp(transform.rotation, dummy.transform.rotation, Time.deltaTime * (5f + speedBoost));
        }
    }

    public void SetSpeedBoost(float newSpeedBoost)
    {
        speedBoost += newSpeedBoost;
    }

    public void SetStartingLife(float life)
	{
        health.maxHealth = life;
        health.startingHealth = life;
        health.currentHealth = life;
    }

    public void SetRegenerateHealth(bool canRegenerate)
	{
        _canRegenerateHealth = canRegenerate;
    }

    public void SetAiBoat()
	{
        _aiBoat = gameObject.AddComponent<AIBoat>();
		visualBoat.SetActive(false);
		if (ghostBoatPrefab == null)
			ghostBoatPrefab = Resources.Load<GameObject>("GhostBoat_Pirate Variant");
		GameObject ghostBoat = Instantiate(ghostBoatPrefab);
		ghostBoat.transform.SetParent(visualBoat.transform.parent);
		ghostBoat.transform.localPosition = visualBoat.transform.localPosition;
		ghostBoat.transform.localRotation = visualBoat.transform.localRotation;
		ghostBoat.transform.localScale = visualBoat.transform.localScale;
		_health.reactionanimations = ghostBoat.GetComponent<Animator>();
		GameObject.DestroyImmediate(visualBoat);
		_boatSinking = gameObject.GetComponentInChildren<boat_sinking>(true);
        _boatFinishFeedback = null;
        _boatResultScreen = null;
        _boatMinimap = gameObject.GetComponentInChildren<boat_minimap>(true);
        _boatEndlessUI = gameObject.GetComponentInChildren<boat_endlessUI>(true);
        _boatEndlessUI.gameObject.SetActive(false);
        gamesettings_boat gsBoat = gamesettings_boat.myself;
        if (gsBoat != null)
        {
            GameObject goCaptainMale = ghostBoat.FindInChildren(gsBoat.boat01objecttoremove);
            if (goCaptainMale != null)
                Destroy(goCaptainMale);
            GameObject goCaptainFemale = ghostBoat.FindInChildren(gsBoat.boat02objecttoremove);
            if (goCaptainFemale != null)
                Destroy(goCaptainFemale);
        }
        foreach (ProjectileCannon cannon in ghostBoat.GetComponentsInChildren<ProjectileCannon>(true))
			cannon.UpdateBoat();
	}

    public void AddGold(int gold)
	{
        _wonGold += gold;
        if (onGoldUpdatedCallback != null)
            onGoldUpdatedCallback(goldOnBoat);
    }

    public void SetStolenGold(int gold)
    {
        _stolenGold = gold;
        if (onGoldUpdatedCallback != null)
            onGoldUpdatedCallback(goldOnBoat);
    }

    public void ForceWonGold(int gold)
	{
        if (_wonGold != gold)
        {
            _wonGold = gold;
            if (onGoldUpdatedCallback != null)
                onGoldUpdatedCallback(goldOnBoat);
        }
    }

    public void ForceStolenGold(int gold)
    {
        if (_stolenGold != gold)
        {
            _stolenGold = gold;
            if (onGoldUpdatedCallback != null)
                onGoldUpdatedCallback(goldOnBoat);
        }
    }

    public void OneBucket()
    {
        if (_boatSinking != null)
            _boatSinking.OneBucket();
    }

    public static void EndOfSplineReached(PathFollower obj)
    {
        // Endless mode : don't end game on end of spline (looping)
        if (gameflowmultiplayer.gameMode == gameflowmultiplayer.GameMode.Endless)
            return;

        // Endless mode with defined duration
        if (multiplayerlobby.IsInEndlessRace)
            return;

        boat_followdummy[] alldummies = GameObject.FindObjectsOfType<boat_followdummy>();
        foreach (boat_followdummy bfd in alldummies)
        {
            if (bfd.dummy == obj.gameObject)
            {
                // set end race
                PathFollower pf = bfd.pathFollower;
                if (pf != null)
                {
                    pf.raceended = true;
                }
                /*
                GameObject Treasure = bfd.gameObject.FindInChildren("Treasure_Display");
                Treasure.SetActive(true);

                // show result screen
                GameObject screenRoot = Treasure.FindInChildren("End_Race_ResultRoot");
                if (screenRoot != null)
                {
                    UI_EndRaceResult resultScreen = GameObject.Instantiate<UI_EndRaceResult>(gamesettings._myself.endRaceResultPrefab, screenRoot.transform);
                    resultScreen.transform.localPosition = Vector3.zero;
                    resultScreen.transform.localRotation = Quaternion.Euler(Vector3.zero);
                }
                */
            }
        }
    }

    public void PlayCaptainVoice(string objectname)
    {
        objectname = objectname + "_" + captaingender;
        //Debug.Log("PlayCaptainVoice:"+ objectname);

        GameObject obj = gameObject.FindInChildren(objectname);
        if (obj)
        {
            
            VoiceOver voiceOver = obj.GetComponent<VoiceOver>();
            if (voiceOver != null)
            {
                voiceOver.playVoiceOverWhenEnable = false;
                obj.SetActive(true);
                // If voice over -> play next clip automatically
                if (obj.activeInHierarchy)
                    voiceOver.PlayVoiceOver();
            }
            else
            {
                obj.SetActive(true);
                // No voice over -> play current audio clip
                AudioSource auso = obj.GetComponent<AudioSource>();
                if (auso)
                {
                    auso.Stop();
                    auso.Play();
                }
            }
        }
    }
}
