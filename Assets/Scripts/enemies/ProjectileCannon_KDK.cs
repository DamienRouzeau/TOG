using System.Collections;
using UnityEngine;

public class ProjectileCannon_KDK : MonoBehaviour
{
	[SerializeField] private bool cannonActive = true;
	[SerializeField] private GameObject projectile = null;
    [SerializeField] private Vector2 minMaxAngle = default;
    [SerializeField] private Transform cannonTurnBase = null;
    [SerializeField] private Transform cannonBarrel = null;
    [SerializeField] private ParticleSystem cannonSmoke = null;
    [SerializeField] private float fireVelocity = 10;
    [SerializeField] private bool fireVelocityUseDistance = false;
    [SerializeField] private float fireVelocityDistanceMax = 100;
    [SerializeField] private AnimationCurve fireVelocityIntensity;
    [SerializeField] private bool needTurnBase = true;
    [SerializeField] private bool needTurnBarrel = true;
    [SerializeField] private bool arcFire = false;
    [SerializeField] private Transform[] targetsSequence = null;
    [SerializeField] private float _damageMultiplicator = 1f;
    [SerializeField] private bool giveTargetToProjectile = false;
    [SerializeField] private bool _dontFireThroughWalls = false;
    public float FirePauseSeconds = 2.0f;
    public float FireWaitAfterSeconds = 1.0f;
    public int curvesteps = 1;
    public AnimationCurve curvevelocity;
    
	int targetsSequenceindex = 0;
    bool movecanon = true;   
    float velostep = 1.0f;
    float velocurvepos = 0.0f;
    public float veloshow = 0.0f;

    GameObject [] targetsSequenceObj;
    public bool[] validtargets;
    private int _fireCounter = 0;
    private Transform _barrel = null;

    public enum AimType
    {
        usevelocity,
        randomdistance,
        patterndistance
    }
    public AimType aimtype = AimType.usevelocity;
    public Vector3 currentError = new Vector3(0, 0, 0);
    public float errorDistance = 0.0f;
    public float destroyProjectileAfterTime = 0f;

    private bool _debugDisableCannon = false;
    private Health _health = null;

    private Transform Nexttarget = null;
    private CheckConditionsEvent _fireConditions = null;
    private float _targetDistance = 50f;

    private void Start()
	{
        _barrel = cannonBarrel;
        if (_barrel == null)
            _barrel = transform;
    }

	private void OnEnable()
    {
        if (targetsSequence != null && targetsSequence.Length > 0 && targetsSequence[0] != null)
            Init(targetsSequence, validtargets[0]);
    }

    public void Init(Transform[] targets, bool defaultValidTargets = false, CheckConditionsEvent fireConditions = null, bool dontFireThroughWalls = false)
    {
        targetsSequence = targets;
        targetsSequenceObj = new GameObject[targetsSequence.Length];
        validtargets = new bool[targetsSequence.Length];
        for (int i = 0; i < targetsSequence.Length; i++)
        {
            validtargets[i] = defaultValidTargets;
            targetsSequenceObj[i] = targetsSequence[i]?.gameObject;
        }
        _fireConditions = fireConditions;
        _dontFireThroughWalls = dontFireThroughWalls;
        velostep = 1.0f / (float)(curvesteps - 1);
        if (gameObject.activeInHierarchy)
            StartCoroutine(FireLoop());
    }

    float GetCurveValue()
    {
        return(curvevelocity.Evaluate(velocurvepos));
    }

    float GetVelocity()
    {
        float velo = fireVelocity;
        if (aimtype == AimType.usevelocity)
        {
            if (curvesteps > 1) velo = fireVelocity + (fireVelocity * GetCurveValue());
        }
        if (fireVelocityUseDistance && fireVelocityIntensity != null && fireVelocityDistanceMax > 0f && _targetDistance > 0f)
		{
            velo *= fireVelocityIntensity.Evaluate(_targetDistance / fireVelocityDistanceMax);
            //Debug.Log("GetVelocity " + fireVelocity + " " + _targetDistance + " " + fireVelocityDistanceMax + " " + velo);
        }
        veloshow = velo;
        return (velo);
    }

    // Update is called once per frame
    void Update ()
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
#if UNITY_EDITOR
        Debug.DrawRay(_barrel.position, _barrel.forward * GetVelocity(), Color.white);
        Vector3 xComponenet = _barrel.forward * GetVelocity();
        Vector3 yComponenet = _barrel.forward * GetVelocity();
        xComponenet.y = 0;
        yComponenet.x = 0;
        yComponenet.z = 0;

        Debug.DrawRay(_barrel.position, xComponenet, Color.red);
        Debug.DrawRay(_barrel.position, yComponenet, Color.green);
#endif

        if (cannonActive && movecanon)
        {
            //Nexttarget = GetNextTarget();
            if (needTurnBase)
			    TurnCannonBase();
            if (needTurnBarrel)
                TurnBarrel();
        }
    }

    public void TurnCannonBase()
    {
        if (cannonTurnBase == null) return;
        if (Nexttarget == null) return;
        Vector3 baseTurnUp = cannonTurnBase.up;
        if (baseTurnUp.y > 0.99f)
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
            cannonTurnBase.forward = Vector3.Lerp(cannonTurnBase.forward, baseTurnDir - transform.position, Time.deltaTime * 0.1f);
        }
    }

    public void ChangeFireMode()
    {
        arcFire = !arcFire;
    }

    private float z;
    public void TurnBarrel()
    {
        if (cannonBarrel == null) return;
        if (Nexttarget == null) return;
        Vector3 H_levelPos = GetTargetPosition();
        H_levelPos.y = cannonBarrel.position.y;

        float Rx = Vector3.Distance(H_levelPos, cannonBarrel.position);
        _targetDistance = Rx;

        float velocity = GetVelocity();

        float k = 0.5f * Physics.gravity.y * Rx * Rx * (1 / (velocity * velocity));

        float h = H_levelPos.y - cannonBarrel.position.y;

        float j = (Rx * Rx) - (4 * (k * (k - h)));

        if(j >= 0)
        {
            if(arcFire) z = (-Rx - Mathf.Sqrt(j)) / (2 * k);

            else z = (-Rx + Mathf.Sqrt(j)) / (2 * k);
        } 
        cannonBarrel.localEulerAngles = new Vector3(Mathf.LerpAngle(cannonBarrel.localEulerAngles.x,  Mathf.Clamp((Mathf.Atan(z) * 57.2958f), minMaxAngle.x, minMaxAngle.y) * -1, Time.deltaTime * 10), 0, 0);
    }

    IEnumerator FireLoop()
    {
        while(true)
        {
            yield return new WaitForSeconds(FirePauseSeconds);
            
            Nexttarget = GetNextTarget();
            while (Nexttarget == null)
            {
                yield return new WaitForSeconds(1f);
                Nexttarget = GetNextTarget();
            }
            if (cannonActive)
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
        if (startindex == targetsSequenceindex)            return (false);
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
        if (_fireConditions != null)
		{
            if (!_fireConditions.AreConditionsValid())
                return;
		}

        if ((targetsSequence[targetsSequenceindex] != null) && (!Helper.IsDestroyed(targetsSequenceObj[targetsSequenceindex])) && (validtargets[targetsSequenceindex]))
        {
            if (cannonSmoke != null)
                cannonSmoke.Play();
            GameObject fireObj = Instantiate(projectile, _barrel.position, _barrel.rotation, poolhelper.myself.transform);
            Projectile proj = fireObj.GetComponentInChildren<Projectile>(true);
            proj.projectile_creator = gameObject;
            proj.damage *= _damageMultiplicator;
            if (giveTargetToProjectile)
                proj.SetTarget(Nexttarget);
            Rigidbody rb = fireObj.GetComponentInChildren<Rigidbody>();
            rb.velocity = _barrel.forward * GetVelocity();
            Health h = fireObj.GetComponentInChildren<Health>(true);
            if (h != null)
			{
                h.SetInstanceId(_health.instanceId + (_fireCounter % 99) + 1);
                _fireCounter++;
#if USE_KDK
                if (h.healtObjectType != Health.HealtObjectType.projectile)
                    Player.myplayer.AddTarget(fireObj.transform);
#endif
            }
            if (_health != null && _health.reactionanimations != null)
                _health.reactionanimations.SetTrigger("Shoot");
            if (destroyProjectileAfterTime > 0f)
                Destroy(fireObj, destroyProjectileAfterTime);
        }
        long strt = targetsSequenceindex;
        IncreaseTargetSequence(strt);
        while ((targetsSequence[targetsSequenceindex] != null) &&
            (( Helper.IsDestroyed(targetsSequenceObj[targetsSequenceindex]) ) || (!validtargets[targetsSequenceindex])))
        {
            if (!IncreaseTargetSequence(strt)) break;
        }
    }

    Transform GetNextTarget()
    {
        int count = targetsSequenceObj.Length;
        if (count == 0)
            return null;
        int i = targetsSequenceindex;
        for (int n=0;n<targetsSequenceObj.Length;n++)
        {
            i = (targetsSequenceindex + n) % count;
            GameObject go = targetsSequenceObj[i];
            if (go != null && validtargets[i])
            {
                if (Helper.IsDestroyed(go))
                {
                    continue;
                }
                else
                {
                    if (_dontFireThroughWalls)
                    {
                        Vector3 diff = go.transform.position - _barrel.position;
                        float maxDistance = diff.magnitude;
                        Ray ray = new Ray(_barrel.position, diff / maxDistance);
                        if (Physics.Raycast(ray, maxDistance, LayerMask.GetMask("Walls")))
                        {
                            continue;
                        }
                    }
                    Health health = go.GetComponent<TargetHealth>()?.targetHealth;
                    if (health != null)
                    {
                        if (health.dead || health.currentHealth <= 0f)
                            continue;
                        else
                            return targetsSequence[i];
                    }
					else
					{
                        Player.IPlayer player = go.GetComponent<Player.IPlayer>();
                        if (player != null)
                        {
                            if (!GameflowBase.piratedeaths[player.id] && GameflowBase.piratehealths[player.id] > 0f)
                                return targetsSequence[i];
                        }
                        return targetsSequence[i];
                    }
                }
            }
        }
        return null;
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
