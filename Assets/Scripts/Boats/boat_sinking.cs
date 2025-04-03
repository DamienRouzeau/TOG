using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using RootMotion.FinalIK;

public class boat_sinking : MonoBehaviour
{
    public boat_followdummy father;
    public float sinkto = 3.52f;
    public float emptyboatto = 3.1f;
    public float sinkspeed = 0.03f;
    public float sinkto_20pc = 2.0f;
    public GameObject[] othersinkingobjects = null;
    public Material matLifeGaugeNormal = null;
    public Material matLifeGaugeSunken = null;

    Vector3[] othersinkingobjects_pos;

    Health health;

    GameObject Pump_Rear_Left;
    GameObject Pump_Rear_Right;

    Animator leftpump;
    Animator rightpump;
    GameObject CylinderLeft;
    GameObject CylinderRight;

    public bool sinkme = false;
    public bool sinkme_half = false;

    private Coroutine intermediateRoutine = null;

    public bool isSinking => _isSinking;
    private bool _isSinking = false;

    public bool isSunken => _isSunken;
    private bool _isSunken = false;

    private Quaternion _lastRotation;
    private Vector3 _lastPosition;

    public Vector3 lastMove => _lastMove;
    private Vector3 _lastMove;

    IEnumerator Spit(GameObject obj)
    {
        obj = obj.FindInChildren("FX_Water_Splash");
        if (obj == null) yield break;
        AudioSource asrc = obj.GetComponent<AudioSource>();
        asrc.Stop();
        asrc.Play();
        ParticleSystem ps = obj.GetComponentInChildren<ParticleSystem>(true);
        ps.gameObject.SetActive(true);
        obj.SetActive(true);
        ps.Stop();
//        ps.Clear();
        ps.Play();
        while (ps.isPlaying)
            yield return null;
        ps.Stop();
//        ps.Clear();
        obj.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        othersinkingobjects_pos = new Vector3[othersinkingobjects.Length];
        for(int i=0;i< othersinkingobjects.Length;i++)
            othersinkingobjects_pos[i] = othersinkingobjects[i].transform.localPosition;

        father = gameObject.GetComponentInParent<boat_followdummy>();
        health = father.health;
        Pump_Rear_Left = father.gameObject.FindInChildren("Pump_Rear_Left");
        Pump_Rear_Right = father.gameObject.FindInChildren("Pump_Rear_Right");

        leftpump = Pump_Rear_Left.FindInChildren("Boat_Water_Pump").GetComponent<Animator>();
        rightpump = Pump_Rear_Right.FindInChildren("Boat_Water_Pump").GetComponent<Animator>();
        CylinderLeft = leftpump.gameObject.FindInChildren("Handle");
        CylinderRight = rightpump.gameObject.FindInChildren("Handle");
    }
    // 

    bool left_handle_state = false;     // up
    bool right_handle_state = false;     // up
    // Update is called once per frame
    void Update()
    {
        gamesettings_boat gdBoat = gamesettings_boat.myself;

        if (gdBoat == null)
            return;

        if (sinkme)
        {
            sinkme = false;
            health.ChangeHealth(-3000);
        }
        if (sinkme_half)
        {
            float targetvalue = (health.maxHealth * gdBoat.sinktreshold) / 100.0f;

            sinkme_half = false;

            if (health.currentHealth > targetvalue)
                health.ChangeHealth(targetvalue - health.currentHealth);
            else
                health.ChangeHealth(-targetvalue / 5);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            PlayerBucket();
        }

        // define boats potitions
        Vector3 pos = gameObject.transform.localPosition;
        float newPosY = Mathf.Lerp( pos.y, father.sinkposition, Time.deltaTime * 5f);
        pos.y = newPosY;
        gameObject.transform.localPosition = pos;
        for (int i = 0;i < othersinkingobjects.Length;i++)
            othersinkingobjects[i].transform.localPosition = new Vector3(othersinkingobjects_pos[i].x, newPosY, othersinkingobjects_pos[i].z);

        if (_isSinking)
        {
            father.shield_time = gdBoat.boat_shield_time;

            float l_ypos = CylinderLeft.transform.localPosition.y;
            if (!left_handle_state)     // was up wait for down
            {
                if (l_ypos < gamesettings_boat.myself.pump_low_limit)
                {
                    left_handle_state = true;
                    StartCoroutine(Spit(Pump_Rear_Left));
                    PlayerBucket();
                }
            }
            else
            {
                if (l_ypos > gdBoat.pump_high_limit)
                    left_handle_state = false;
            }

            float r_ypos = CylinderRight.transform.localPosition.y;
            if (!right_handle_state)     // was up wait for down
            {
                if (r_ypos < gdBoat.pump_low_limit)
                {
                    right_handle_state = true;
                    StartCoroutine(Spit(Pump_Rear_Right));
                    PlayerBucket();
                }
            }
            else
            {
                if (r_ypos > gdBoat.pump_high_limit)
                    right_handle_state = false;
            }
        }
        else
        {
            if (father.shield_time > 0.0f)
            {
                father.shield_time -= Time.deltaTime;
                if (father.shield_time < 0.0f)
                    father.shield_time = 0.0f;
            }
        }
    }

    void OnEnable()
    {
        _lastPosition = transform.position;
        _lastRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        if (Player.myplayer.team >= 0)
        {
            _lastMove = transform.position - _lastPosition;
            Quaternion diffRot = transform.rotation * Quaternion.Inverse(_lastRotation);
            Vector3 pivot = transform.position;
            if (Player.myplayer.team == father.team)
            {
                AddPlatformMotionOnVRIK(Player.myplayer.vrik, _lastMove, diffRot, pivot, "MyPlayer " + gameflowmultiplayer.myId + " on team " + father.team);
            }
            foreach (Player_avatar avatar in Player.myplayer.avatars)
            {
                if (avatar.actornumber >= 0 && avatar.team == father.team)
                {
                    AddPlatformMotionOnVRIK(avatar.vrik, _lastMove, diffRot, pivot, "Avatar " + avatar.actornumber + " on team " + father.team);
                }
            }
        }
        _lastPosition = transform.position;
        _lastRotation = transform.rotation;
    }

    private void AddPlatformMotionOnVRIK(VRIK vrik, Vector3 diffPos, Quaternion diffRot, Vector3 pivot, string context = "")
    {
        if (vrik != null)
        {
            //Debug.Log($"AddPlatformMotionOnVRIK {context} {diffPos} {diffRot} {pivot}");
            vrik.solver.AddPlatformMotion(diffPos, diffRot, pivot);
        }
        else
        {
            Debug.LogError($"AddPlatformMotionOnVRIK vrik is null for " + context);
        }
    }

    public void SinkMe()
    {
        StopAllCoroutines();
        StartCoroutine(Sinking());
    }

    public void IntermediateSinking()
    {
        if (intermediateRoutine != null)
            StopCoroutine(intermediateRoutine);
        intermediateRoutine = StartCoroutine(Intermediate());
    }

    public void Touched(GameObject obj)
    {
        if (!_isSinking)
           SinkMe();
    }

    IEnumerator Sinking()
    {
        _isSinking = true;
        _isSunken = false;

        Vector3 v = CylinderLeft.transform.localEulerAngles;
        v.y = 0;
        CylinderLeft.transform.localEulerAngles = v;
        v = CylinderRight.transform.localEulerAngles;
        v.y = 0;
        CylinderRight.transform.localEulerAngles = v;

        while (father.sinkposition > (-sinkto))
        {
            if (PhotonNetworkController.IsMaster())
            {
                father.sinkposition -= sinkspeed * Time.deltaTime * 50f;
                if (father.sinkposition < (-sinkto))
                    father.sinkposition = (-sinkto);
            }
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        _isSunken = true;

        if (GameflowBase.myTeam == father.team)
            father.PlayCaptainVoice("Vo_Water_Pump");

        //        father
//        Pump_Rear_Left.SetActive(true);
//        Pump_Rear_Right.SetActive(true);
        leftpump.SetTrigger("Reveal");
        rightpump.SetTrigger("Reveal");

        gameObject.FindInChildren("ICO_Bucket").SetActive(true);

        if (UI_Tutorial.myself != null)
            UI_Tutorial.myself.OnTriggerCondition(UI_Tutorial.TutoCondition.BoatSunken);

        // wait to remove water
        while (health.GetCurrentHealth() < health.maxHealth)
        {
            yield return null;
        }

        health.dead = false;

        _isSinking = false;

        leftpump.SetTrigger("Hide");
        rightpump.SetTrigger("Hide");

        if (UI_Tutorial.myself != null)
            UI_Tutorial.myself.OnTriggerCondition(UI_Tutorial.TutoCondition.BoatFullPumped);

        if (PhotonNetworkController.IsMaster())
        {
            while (father.sinkposition < 0.0f)
            {
                father.sinkposition += sinkspeed * Time.deltaTime * 50f;
                yield return null;
            }
            father.sinkposition = 0f;
        }

        gameObject.FindInChildren("ICO_Bucket").SetActive(false);

        health.reactionanimations.SetTrigger("IsAlive");

        _isSunken = false;
    }


    public void OneBucket()
    {
        if (father != null)
        {
            if (father.sinkposition < 0.0f)
            {
                int playerCountInBoat = gameflowmultiplayer.GetActorCountInTeam(father.team);
                float snk = (sinkto - emptyboatto) / gamesettings_boat.myself.GetPumpCounter(playerCountInBoat);
                father.sinkposition += snk;
                SetLifeFromSinkPosition();
            }
        }
    }

    public void SetSinkPositionFromLife(float life)
	{
        if (father != null)
        {
            if (father.sinkposition < 0.0f && _isSunken)
            {
                float ratio = life / father.health.maxHealth;
                father.sinkposition = Mathf.Lerp(-sinkto, -emptyboatto, ratio);
                
            }
        }
        health.SetCurrentHealth(life);
    }

    public void SetLifeFromSinkPosition()
    {
        if (father != null)
        {
            float value = -(father.sinkposition + emptyboatto);
            value = (value * 100.0f) / (sinkto - emptyboatto);
            if (value > 100)
                value = 100;
            if (value < 0)
                value = 0;
            value = 100 - value;
            value = (value * health.maxHealth) / 100;
            health.SetCurrentHealth(value);
        }
    }

    public void PlayerBucket(bool fromAI=false)
    {
        if (father.team == GameflowBase.myTeam)
        {
            if (PhotonNetworkController.IsMaster())
                OneBucket();
            gameflowmultiplayer.bucketlifted++;
        }
        else if (fromAI)
		{
            if (PhotonNetworkController.IsMaster())
                OneBucket();
        }
    }

    IEnumerator Intermediate()
    {
        while (true)
        {
            float targetvalue = (health.maxHealth * gamesettings_boat.myself.sinktreshold) / 100.0f;

            targetvalue = health.currentHealth * 100.0f / targetvalue;
            if (targetvalue > 100.0f)
            {
                // back up to the surface
                while (father.sinkposition < 0.0f)
                {
                    father.sinkposition += Time.deltaTime;
                    yield return null;
                }
                yield break;
            }
            float sinktarget = sinkto_20pc - ((sinkto_20pc * targetvalue) / 100.0f);
            if (sinktarget < 0) sinktarget = 0;
            if (sinktarget > sinkto_20pc) sinktarget = sinkto_20pc;


            if (father.sinkposition > (-sinktarget))
                father.sinkposition -= sinkspeed;
            else
            {
                father.sinkposition = (-sinktarget);
            }
            yield return null;
        }        
    }
 }
