using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProjectileCannon_Custom : MonoBehaviour
{

    #region Enums

    public enum Target
    {
        Boat01_Red,
        Boat02_Blue
    }

    #endregion

    [SerializeField] private bool cannonActive = true;
    [SerializeField] private GameObject projectile = null;
    [SerializeField] private Vector2 minMaxAngle = default;
    [SerializeField] private Transform cannonTurnBase = null;
    [SerializeField] private Transform cannonBarrel = null;
    [SerializeField] private ParticleSystem cannonSmoke = null;
    [SerializeField] private float fireVelocity = 10;
    [SerializeField] private bool arcFire = false;
    [SerializeField] private Target[] _targetsSequenceData = null;
    [SerializeField] private float _damageMultiplicator = 1f;

    public Transform[] targetsSequence;
    int targetsSequenceindex = 0;
    public float FirePauseSeconds = 2.0f;
    float FireWaitAfterSeconds = 1.0f;
    bool movecanon = true;

    public int curvesteps = 1;
    public AnimationCurve curvevelocity;

    float velostep = 1.0f;
    float velocurvepos = 0.0f;
    public float veloshow = 0.0f;

    GameObject[] targetsSequenceObj;
    public bool[] validtargets;

    public enum AimType
    {
        usevelocity,
        randomdistance,
        patterndistance
    }
    public AimType aimtype = AimType.usevelocity;
    public Vector3 currentError = new Vector3(0, 0, 0);
    public float errorDistance = 0.0f;

    private bool _debugDisableCannon = false;
    private Health _health = null;

    private Transform Nexttarget = null;

    private void OnEnable()
    {
        Init();
    }

    private void Init()
    {
        if (_targetsSequenceData == null || _targetsSequenceData.Length == 0)
        {
            _targetsSequenceData = new Target[2];
            _targetsSequenceData[0] = Target.Boat01_Red;
            _targetsSequenceData[1] = Target.Boat02_Blue;
            Debug.LogWarning("ProjectileCannon_Custom - Start - missing targetSequenceData in " + gameObject.name);
        }

        if (RaceManager.myself == null)
            Debug.LogWarning("ProjectileCannon_Custom - RaceManager is not instanciated!");

        if (_targetsSequenceData != null && _targetsSequenceData.Length > 0 && RaceManager.myself != null)
        {
            targetsSequence = new Transform[_targetsSequenceData.Length];
            for (int i = 0; i < _targetsSequenceData.Length; ++i)
            {
                switch (_targetsSequenceData[i])
                {
                    case Target.Boat02_Blue:
                        targetsSequence[i] = RaceManager.myself.boats[boat_followdummy.TeamColor.Blue]?.transform;
                        break;
                    case Target.Boat01_Red:
                        targetsSequence[i] = RaceManager.myself.boats[boat_followdummy.TeamColor.Red]?.transform;
                        break;
                }
            }
        }


        targetsSequenceObj = new GameObject[targetsSequence.Length];
        validtargets = new bool[targetsSequence.Length];
        for (int i = 0; i < targetsSequence.Length; i++)
        {
            validtargets[i] = false;
            if (targetsSequenceObj.Length - 1 >= i && targetsSequence.Length - 1 >= i)
            {
                if (targetsSequence[i] != null)
                    targetsSequenceObj[i] = targetsSequence[i]?.gameObject;
            }
        }
        velostep = 1.0f / (float)(curvesteps - 1);
        StartCoroutine("FireLoop");
    }



    [ContextMenu("Fill targets sequence data")]
    private void FillTargetsSequenceData()
    {
        if (targetsSequence != null && targetsSequence.Length > 0)
        {
            _targetsSequenceData = new Target[targetsSequence.Length];
            for (int i = 0; i < _targetsSequenceData.Length; ++i)
            {
                if (targetsSequence[i] != null)
                {
                    boat_followdummy boat = targetsSequence[i].GetComponent<boat_followdummy>();
                    switch (boat.teamColor)
                    {
                        case boat_followdummy.TeamColor.Blue:
                            _targetsSequenceData[i] = Target.Boat02_Blue;
                            break;
                        case boat_followdummy.TeamColor.Red:
                            _targetsSequenceData[i] = Target.Boat01_Red;
                            break;
                    }
                }

            }
        }
    }

    float GetCurveValue()
    {
        return (curvevelocity.Evaluate(velocurvepos));
    }

    float GetVelocity()
    {
        float velo = fireVelocity;
        if (aimtype == AimType.usevelocity)
        {
            if (curvesteps > 1) velo = fireVelocity + (fireVelocity * GetCurveValue());
        }
        else
            velo = fireVelocity;
        veloshow = velo;
        return (velo);
    }

    // Update is called once per frame
    void Update()
    {
        if (_health == null)
        {
            _health = gameObject.GetComponentInParent<Health>();
        }
        else if (_health.dead)
        {
            if (_health.respawnTime <= 0f)
                Destroy(this);
            return;
        }

        Debug.DrawRay(cannonBarrel.position, cannonBarrel.forward * GetVelocity(), Color.white);
        Vector3 xComponenet = cannonBarrel.forward * GetVelocity();
        Vector3 yComponenet = cannonBarrel.forward * GetVelocity();
        xComponenet.y = 0;
        yComponenet.x = 0;
        yComponenet.z = 0;

        Debug.DrawRay(cannonBarrel.position, xComponenet, Color.red);
        Debug.DrawRay(cannonBarrel.position, yComponenet, Color.green);

        if (cannonActive && movecanon)
        {
            //Nexttarget = GetNextTarget();
            TurnCannonBase();
            TurnBarrel();
        }

    }

    public void TurnCannonBase()
    {
        if (Nexttarget == null) return;
        Vector3 baseTurnUp = cannonTurnBase.up;
        if (Mathf.Approximately(baseTurnUp.y, 1f))
        {
            Vector3 baseTurnDir = GetTargetPosition() - cannonTurnBase.position;
            baseTurnDir.y = 0;
            cannonTurnBase.forward = Vector3.Lerp(cannonTurnBase.forward, baseTurnDir, Time.deltaTime * 0.1f);
        }
        else
        {
            Vector3 baseTurnDir = cannonTurnBase.InverseTransformPoint(GetTargetPosition());
            baseTurnDir.y = 0;
            baseTurnDir = cannonTurnBase.TransformPoint(baseTurnDir);
            cannonTurnBase.LookAt(baseTurnDir, baseTurnUp);
        }
    }

    public void ChangeFireMode()
    {
        arcFire = !arcFire;
    }

    private float z;
    public void TurnBarrel()
    {
        if (Nexttarget == null) return;
        Vector3 H_levelPos = GetTargetPosition();
        H_levelPos.y = cannonBarrel.position.y;

        float Rx = Vector3.Distance(H_levelPos, cannonBarrel.position);

        float k = 0.5f * Physics.gravity.y * Rx * Rx * (1 / (GetVelocity() * GetVelocity()));

        float h = GetTargetPosition().y - cannonBarrel.position.y;

        float j = (Rx * Rx) - (4 * (k * (k - h)));

        if (j >= 0)
        {
            if (arcFire) z = (-Rx - Mathf.Sqrt(j)) / (2 * k);

            else z = (-Rx + Mathf.Sqrt(j)) / (2 * k);
        }
        cannonBarrel.localEulerAngles = new Vector3(Mathf.LerpAngle(cannonBarrel.localEulerAngles.x, Mathf.Clamp((Mathf.Atan(z) * 57.2958f), minMaxAngle.x, minMaxAngle.y) * -1, Time.deltaTime * 10), 0, 0);
    }

    IEnumerator FireLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(FirePauseSeconds);
            if (_health != null && _health.reactionanimations != null)
            {
                _health.reactionanimations.SetTrigger("Shoot");
                yield return new WaitForSeconds(0.1f);
            }
            Nexttarget = GetNextTarget();
            while (Nexttarget == null)
            {
                yield return new WaitForSeconds(FireWaitAfterSeconds);
                Nexttarget = GetNextTarget();
            }
            FireCannon();
            RandomError();
            movecanon = false;
            yield return new WaitForSeconds(FireWaitAfterSeconds);
            movecanon = true;
        }
    }

    bool IncreaseTargetSequence(long startindex)
    {
        targetsSequenceindex++;
        if (startindex == targetsSequenceindex) return (false);
        if (targetsSequenceindex >= targetsSequence.Length)
        {
            targetsSequenceindex = 0;
            velocurvepos += velostep;
            while (velocurvepos >= velostep * (float)(curvesteps - 1)) velocurvepos = 0.0f;
        }
        return (true);
    }

    public void FireCannon()
    {
        if (_health != null && _health.dead)
            return;
        if (gameflowmultiplayer.gameplayEndRace)
            return;
        if (_debugDisableCannon)
            return;
        if (poolhelper.myself == null)
            return;

        if ((targetsSequence[targetsSequenceindex] != null) && (!Helper.IsDestroyed(targetsSequenceObj[targetsSequenceindex])) && (validtargets[targetsSequenceindex]))
        {
            cannonSmoke.Play();
            GameObject fireObj = Instantiate(projectile, cannonBarrel.position, cannonBarrel.rotation, poolhelper.myself.transform);
            Projectile proj = fireObj.GetComponentInChildren<Projectile>();
            proj.projectile_creator = gameObject;
            proj.damage *= _damageMultiplicator;
            Rigidbody rb = fireObj.GetComponent<Rigidbody>();
            rb.velocity = cannonBarrel.forward * GetVelocity();
            Destroy(fireObj, 10);
        }
        long strt = targetsSequenceindex;
        IncreaseTargetSequence(strt);
        while ((targetsSequence[targetsSequenceindex] != null) &&
            ((Helper.IsDestroyed(targetsSequenceObj[targetsSequenceindex])) || (!validtargets[targetsSequenceindex])))
        {
            if (!IncreaseTargetSequence(strt)) break;
        }
    }


    Transform GetNextTarget()
    {
        if (targetsSequenceObj.Length == 0) return (null);
        Transform ret = null;
        int i = targetsSequenceindex;
        for (int n = 0; n < targetsSequenceObj.Length; n++)
        {
            bool ignorethisone = false;
            if ((targetsSequenceObj[i] != null) && (validtargets[i]))
            {
                if (Helper.IsDestroyed(targetsSequenceObj[i]))
                    ignorethisone = true;
                else
                {
                    boat_followdummy bfd = targetsSequenceObj[i].GetComponent<boat_followdummy>();
                    if (bfd)
                    {
                        if (bfd.isSinking || bfd.shield_time > 0.0f)
                        {
                            ignorethisone = true;
                        }
                        else
                        {
                            ret = targetsSequence[i];
                            break;
                        }
                    }
                    else
                    {
                        ret = targetsSequence[i];
                        break;
                    }
                }
            }
            if (!ignorethisone)
            {
                if (validtargets[i])
                {
                    ret = targetsSequence[i];
                    break;
                }
            }
            i++;
            if (i >= targetsSequenceObj.Length)
                i = 0;
        }
        return (ret);
    }


    public void ZoneEntered(GameObject obj)
    {
        if (targetsSequenceObj != null)
        {
            for (int i = 0; i < targetsSequenceObj.Length; i++)
            {
                if (obj == targetsSequenceObj[i])
                {
                    validtargets[i] = true;
                    break;
                }
            }
        }
    }

    public void ZoneLeft(GameObject obj)
    {
        if (targetsSequenceObj != null)
        {
            for (int i = 0; i < targetsSequenceObj.Length; i++)
            {
                if (obj == targetsSequenceObj[i])
                {
                    validtargets[i] = false;
                    break;
                }
            }
        }
    }


    Vector3 GetTargetPosition()
    {
        if (Nexttarget == null) return (Vector3.zero);

        if (aimtype == AimType.usevelocity)
            return (Nexttarget.position);
        return (Nexttarget.position + currentError);
    }

    void RandomError()
    {
        if (aimtype == AimType.randomdistance)
        {
            currentError.x = Random.Range(0.0f, errorDistance * 2.0f) - errorDistance;
            currentError.z = Random.Range(0.0f, errorDistance * 2.0f) - errorDistance;
        }
        if (aimtype == AimType.patterndistance)
        {
            if (GetCurveValue() > 0.5f)
            {
                currentError.x = 0;
                currentError.z = 0;
            }
            else
            {
                // circle around 0,0
                float angle = Random.Range(0.0f, Mathf.PI * 2.0f);
                currentError.x = Mathf.Sin(angle) * errorDistance;
                currentError.z = Mathf.Cos(angle) * errorDistance;
            }
        }
    }

}
