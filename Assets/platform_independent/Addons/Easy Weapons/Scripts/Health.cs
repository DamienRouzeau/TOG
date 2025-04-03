//#define DEBUG_HEALTH
//#define DEBUG_BOAT_VALUES
//#define DEBUG_INSTANCE_ID

/// <summary>
/// Health.cs
/// Author: MutantGopher
/// This is a sample health script.  If you use a different script for health,
/// make sure that it is called "Health".  If it is not, you may need to edit code
/// referencing the Health component from other scripts
/// </summary>

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public delegate void OnDieDelegate(Health h);

    [System.Serializable]
    public class ReactionAnimationStep
	{
        public string triggerName = null;
        public float percentLife = 0f;
	}

    public class ObjectPosData
	{
        public Vector3 pos;
        public Quaternion rot;
        public Vector3 scale;

        public ObjectPosData(Transform tr)
        {
            if (tr != null)
            {
                pos = tr.localPosition;
                rot = tr.localRotation;
                scale = tr.localScale;
            }
        }

        public void ApplyOnTransform(Transform tr)
		{
            if (tr != null)
            {
                tr.localPosition = pos;
                tr.localRotation = rot;
                tr.localScale = scale;
                Rigidbody rb = tr.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
		}
    }

    public class HealthById
    {
        public long healthId;
        public int playerId;
        public float value;
    }

    public enum HealtObjectType
    { 
        none, 
        chest, 
        turret, 
        lava, 
        boat, 
        bird, 
        pirate, 
        coins, 
        cannonball, 
        beholder, 
        heart,
        shield, 
        splineswitch, 
        mermaid, 
        monster,
        skull,
        drone,
        mine,
        superDrone,
        megaDroid,
        plasmaBomb,
        bomber,
        conveyor,
        projectile,
        droneUltra
    };

    public OnDieDelegate onDieCallback = null;

    public bool canDie = true;                  // Whether or not this health can die

    public float startingHealth = 100.0f;       // The amount of health to start with
    public float maxHealth = 100.0f;            // The maximum amount of health

    public bool replaceWhenDead = false;        // Whether or not a dead replacement should be instantiated.  (Useful for breaking/shattering/exploding effects)
    public Animator reactionanimations = null;  // use these to get hit and die and stuff
    public string reactionWhenGotHitBase = "Turret_GotHit";
    public ReactionAnimationStep[] reactionAnimSteps = null;

    public GameObject deadReplacement;          // The prefab to instantiate when this GameObject dies
    public bool makeExplosion = false;          // Whether or not an explosion prefab should be instantiated
    public GameObject explosion;                // The explosion prefab to be instantiated

    public bool isPlayer = false;               // Whether or not this health is the player
    public GameObject deathCam;                 // The camera to activate when the player dies

    public bool dead = false;					// Used to make sure the Die() function isn't called twice
    
    public bool animationBeforeDeath = true;    // Check true to enable animation before death, and before destruction of the gameObject
    private Animator animatorDeath;             // Animator of the GameObject
    public AnimationClip animationDeath;        // Put the desired animation clip of the animator set on the gameObject here

    public GameObject activateWhenDead = null;                 // Activate this object before the gameObject dies

    public float respawnTime = 0f;              // Delay to wait to respawn automatically
    public bool keepChildrenPosAtRespawn = false; // keep position of all children to reset at respawn

    [Header("Main Health if multiple healths")]
    public bool mainHealth = false;

    [Header("Die")]
    public float autoDieAfterDelay = 0f;        // Delay to die automatically
    public float dyingTime = 0f;
    public bool destroyAfterDying = false;
    
    public bool isDying => _isDying;
    private bool _isDying = false;

    [Header("ObjectInfos")]
    public HealtObjectType healtObjectType = HealtObjectType.none;
    public int deathGain = 0;
    public int touchGain = 0;
    public float shieldtimer = 1.0f;
    public float collectableparam = 0;

    [Header("HealthJauge")]
    public bool hasHealthJauge = true;
    public float fJaugeOffsetY = 20f;
    public Transform jaugeParent = null;
    public bool autoAssignId = true;

    [Header("Collectable")]
    public string collectable_pool = "";
    public float collectable_cost = 0;
    float calc_collectable_cost;

    [Header("Health")]
    public UI_HealthJauge jaugeSpecificPrefab = null;
    private UI_HealthJauge healthJauge = null;
    public float currentHealth;                // The current ammount of health
    public float givenHealth; // Give some health to the player that kill this item
    public GameObject lasthitted = null;

    [Header("Events")]
    public UnityEvent onDieActions = null;

    private boat_followdummy _boat = null;
    public boat_followdummy boat => _boat;

    public bool isTreasureType =>
        healtObjectType == HealtObjectType.chest ||
        healtObjectType == HealtObjectType.coins ||
        healtObjectType == HealtObjectType.turret ||
        healtObjectType == HealtObjectType.mermaid ||
        healtObjectType == HealtObjectType.monster ||        
        healtObjectType == HealtObjectType.drone ||
        healtObjectType == HealtObjectType.mine ||
        healtObjectType == HealtObjectType.superDrone ||
        healtObjectType == HealtObjectType.megaDroid ||
        healtObjectType == HealtObjectType.plasmaBomb ||
        healtObjectType == HealtObjectType.bomber ||
        healtObjectType == HealtObjectType.conveyor ||
        healtObjectType == HealtObjectType.droneUltra;

    public bool isBonusType =>
        healtObjectType == HealtObjectType.heart ||
        healtObjectType == HealtObjectType.shield;

    public bool couldBeRespawned => isTreasureType || isBonusType ||
        healtObjectType == HealtObjectType.lava || healtObjectType == HealtObjectType.projectile;

    public bool isProtectedByShield => boat != null && boat.isShieldActivated;

    private float _stolenGoldForBoat = 0f;

    private float _lastDiedTime = 0f;

    private Dictionary<Transform, ObjectPosData> _childrenPosData = null;

    private float _startTime = 0f;

    private static int _instanceCount = 0;
    private int _instanceId = -1;
    public int instanceId => _instanceId;

    private ProgressionItem _progression = null;

    private static Dictionary<long, Health> _healthDic = new Dictionary<long, Health>();
    //private static string _debugHealthDic = "";

    public static Transform fxParent => _fxParent;
    private static Transform _fxParent = null;

    public static Health GetHealthFromId(long healthId)
	{
        if (_healthDic.TryGetValue(healthId, out Health health))
            return health;
        return null;
	}

    private void Awake()
    {
        calc_collectable_cost = collectable_cost;
        if (activateWhenDead) activateWhenDead.SetActive(false);
        if (autoAssignId)
        {
            _instanceId = _instanceCount++;
            _healthDic[instanceId] = this;
        }
        //_debugHealthDic += $"Healt awake - instanceId {instanceId} {gameObject.name} {healtObjectType} {gameObject.GetInstanceID()} \n";
    }

    // Use this for initialization
    void Start()
    {
        // Initialize Animator set on the GameObject
        animatorDeath = this.GetComponent<Animator>();

        // Initialize the currentHealth variable to the value specified by the user in startingHealth
        currentHealth = startingHealth;

        _boat = gameObject.GetComponent<boat_followdummy>();
#if DEBUG_BOAT_VALUES
        if ((_boat) && (_boat.DebugBoatValues))
        {
            StartCoroutine("WaitForLink");
        }
#endif

        if (gamesettings.myself != null)
        {
            bool needToSetRespawnTime = gamesettings.myself.respawnAllItemsAtTime > 0f;
            if (needToSetRespawnTime && couldBeRespawned && respawnTime == 0f)
                respawnTime = gamesettings.myself.respawnAllItemsAtTime;
        }

        if (respawnTime > 0f)
		{
            _childrenPosData = new Dictionary<Transform, ObjectPosData>();
            Transform[] trArray = GetComponentsInChildren<Transform>(true);
            foreach(Transform tr in trArray)
			{
                _childrenPosData.Add(tr, new ObjectPosData(tr));
			}
        }

        _startTime = Time.time;
    }

    private void OnDestroy()
    {
        _isDying = false;
        _boat = null;
        _progression = null;
        onDieCallback = null;
        if (instanceId >= 0 && _healthDic.ContainsKey(instanceId))
            _healthDic.Remove(instanceId);
    }

    public static void ResetInstanceCount()
	{
        _instanceCount = 0;
        _healthDic.Clear();
        //Debug.Log("_debugHealthDic " + _debugHealthDic);
        //_debugHealthDic = "";
    }

    public void SetInstanceId(int id)
	{
        _instanceId = id;
        _healthDic[instanceId] = this;
    }

#if DEBUG_BOAT_VALUES
    IEnumerator WaitForLink()
    {
        boat_followdummy bfd = _boat;
        while (bfd.MyBoatLife_Value == null)
            yield return null;
        bfd.MyBoatLife_Value.text = currentHealth + "/" + maxHealth;
        bfd.Damage04_Value.text = "";
        bfd.Damage03_Value.text = "";
        bfd.Damage02_Value.text = "";
        bfd.Damage01_Value.text = "";
    }
#endif

    public void SetProgressionItem(ProgressionItem progression)
	{
        _progression = progression;
    }

    public float GetCurrentHealth()
    {
        return (currentHealth);
    }

    public void ForceCurrentHealth(float h, GameObject hitter = null)
    {
        lasthitted = hitter;
        float val = h - currentHealth;
        if (val != 0)
            ChangeHealth(val, -1, true);
    }

    public void SetCurrentHealth(float h)
    {
        currentHealth = Mathf.Clamp(h, 0f, maxHealth);
    }

    public void AddStolenGold(float gold, int projectileId = -1, bool fromMaster = false)
    {
        boat_followdummy bfd = gameObject.GetComponent<boat_followdummy>();
        if (bfd != null)
        {
            if (PhotonNetworkController.IsMaster())
            {
                if (currentHealth <= 0)
                    return;
                if (bfd.isSinking || bfd.isSunken)
                    return;    // no damage when sinking
                if (bfd.hasReachedFinishLine)
                {
                    //Debug.Log("Raph - Boat is invincible - finish line is reached!");
                    return;
                }
            }
            else if (!fromMaster)
            {
                return;
            }
            if (lasthitted != null)
            {
                _stolenGoldForBoat += gold;
                bfd.SetStolenGold((int)_stolenGoldForBoat);
            }
        }
    }

    public void ChangeHealth(float amount, int projectileId = -1, bool fromMaster=false)
    {
        if (!enabled || !gameObject.activeInHierarchy)
            return;

        if (currentHealth > maxHealth)            
            currentHealth = maxHealth;

        boat_followdummy bfd = null;
#if USE_TOG
        bfd = gameObject.GetComponent<boat_followdummy>();
        if (bfd != null)
        {
            if (PhotonNetworkController.IsMaster())
            {
                if (currentHealth <= 0)
                    return;
                if (bfd.isSinking)
                    return;    // no damage when sinking
                // No damage when finish line is reached
                if (bfd.hasReachedFinishLine)
                {
                    //Debug.Log("Raph - Boat is invincible - finish line is reached!");
                    return;
                }
            }
            else if (!fromMaster)
            {
                return;
            }
        }
#endif

        bool bIsMeTheShooter = false;
        int teamOfTheShooter = -1;
        if( lasthitted != null )
        {
            Player player = lasthitted.GetComponent<Player>();
            if (player != null)
            {
                bIsMeTheShooter = true;
                teamOfTheShooter = GameflowBase.myTeam;
                GameflowBase.IncrementPlayerStat(gamesettings.STAT_TOUCH, 1);
            }
            else
            {
#if USE_KDK || USE_BOD
                Player.IPlayer iPlayer = lasthitted.GetComponentInParent<Player.IPlayer>();
                if (iPlayer != null)
                {
                    teamOfTheShooter = 0;
                }
#else
                boat_followdummy boat = lasthitted.GetComponentInParent<boat_followdummy>();
                if (boat == null)
				{
                    Player.IPlayer iPlayer = lasthitted.GetComponentInParent<Player.IPlayer>();
                    if (iPlayer != null)
                    {
                        boat = iPlayer.GetBoat();
                    }
                }
                if (boat != null )
				{
                    teamOfTheShooter = boat.team;
                }
#endif
            }
            if (touchGain > 0)
                IncrementStat(gamesettings.STAT_POINTS, touchGain, teamOfTheShooter, bIsMeTheShooter);
        }

        if (collectable_cost != 0)
        {
            calc_collectable_cost += amount;
            while (calc_collectable_cost <= 0.0f)
            {
                GameObject collectable = poolhelper.myself.GetNextPoolItem(collectable_pool, gameObject.transform.position, gameObject.transform.rotation, gameObject);
                Projectile pro = collectable.GetComponent<Projectile>();
                if (pro != null)
                {
                    if ((pro.enemyList == null) || (pro.enemyList.Length < 1))
                    {
                        pro.enemyList = new GameObject[1];
                        pro.enemyList[0] = lasthitted;
                    }
                    else
                        pro.enemyList[0] = lasthitted;
                }
                calc_collectable_cost += collectable_cost;
            }
        }

        if (bfd != null)
        {
            if (GameflowBase.myTeam == bfd.team)
            {
                /*/
                float clc = (float)maxHealth * 0.7f;
                if ((currentHealth >= clc) && ((currentHealth + amount) < clc))
                    bfd.PlayCaptainVoice("Vo_LifeJauge30");
                clc = (float)maxHealth * 0.3f;
                if ((currentHealth >= clc) && ((currentHealth + amount) < clc))
                    bfd.PlayCaptainVoice("Vo_LifeJauge70");
                /*/
                // Only one threshold at 50% 
                float clc = (float)maxHealth * 0.5f;
                if ((currentHealth >= clc) && ((currentHealth + amount) < clc))
                {
                    if (Random.Range(0,2) == 0)
                        bfd.PlayCaptainVoice("Vo_LifeJauge30");
                    else
                        bfd.PlayCaptainVoice("Vo_LifeJauge70");
                }
                //*/
            }
        }

        // Change the health by the amount specified in the amount variable
        float oldcurrentHealth = currentHealth;
        currentHealth += amount;
        if (currentHealth > maxHealth) 
            currentHealth = maxHealth;

        if (bIsMeTheShooter && couldBeRespawned)
            GameflowBase.TriggerHealthEvent(instanceId, GameflowBase.myId, currentHealth);

#if DEBUG_HEALTH
#if USE_KDK || USE_BOD
        if (_instanceId < 10)
            Debug.Log($"[HEALTH] {gameObject.name} Touched - amount {amount} - currentHealth {currentHealth} - maxHealth {maxHealth}  (old {oldcurrentHealth}) lasthitted {lasthitted}");
#else
        if (bfd != null)
            Debug.Log($"[HEALTH] Boat {bfd.teamColor} Touched! amount {amount} - currentHealth {currentHealth} - maxHealth {maxHealth}  (old {oldcurrentHealth}) lasthitted {lasthitted}");
        else
            Debug.Log($"[HEALTH] {gameObject.name} Touched - amount {amount} - currentHealth {currentHealth} - maxHealth {maxHealth}  (old {oldcurrentHealth}) lasthitted {lasthitted}");
#endif        
#endif

        if (bfd != null)
        {
#if DEBUG_BOAT_VALUES
            if (bfd.DebugBoatValues)
            {
                bfd.MyBoatLife_Value.text = currentHealth + "/" + maxHealth;
                bfd.Damage04_Value.text = bfd.Damage03_Value.text;
                bfd.Damage03_Value.text = bfd.Damage02_Value.text;
                bfd.Damage02_Value.text = bfd.Damage01_Value.text;
                bfd.Damage01_Value.text = amount.ToString();
            }
#endif

            //Debug.Log($"[HEALTH] boat team {bfd.team} life {currentHealth} dam {amount} by {lasthitted} ");
            if (PhotonNetworkController.IsMaster())
            {
                float targetvalue = (maxHealth * gamesettings_boat.myself.sinktreshold) / 100.0f;
                boat_sinking bs = bfd.boatSinking;
                if ((currentHealth < targetvalue) && (oldcurrentHealth >= targetvalue))
                {
                    bs.IntermediateSinking();
                }
            }
            if (currentHealth <= 0)
            {
                currentHealth = 0;

				bfd.boatSinking.Touched(null);
				if (reactionanimations != null)
					reactionanimations.SetTrigger("IsDead");
			}
        }

        // if we are dead, we are dead
        if ( dead )
        {
            return;
        }
        // If the health runs out, then Die.
        else
        {
            if (currentHealth <= 0 && canDie)
            {
                // update stat
                if (teamOfTheShooter >= 0)
                    UpdateStats(teamOfTheShooter, bIsMeTheShooter);

                // Give health to the team of player who fired
                if (givenHealth > 0f && lasthitted != null)
                {
                    Player.IPlayer player = lasthitted.GetComponent<Player.IPlayer>();
                    if (player != null)
                    {
                        boat_followdummy boat = player.GetBoat();
                        if (boat != null)
                        {
                            Health boatHealth = boat.GetComponentInChildren<Health>();
                            if (boatHealth != null)
                            {
                                boatHealth.ChangeHealth(givenHealth);
                            }
                            boat_lifeFX boatLifeFX = boat.GetComponentInChildren<boat_lifeFX>();
                            if (boatLifeFX != null)
							{
                                boatLifeFX.TriggerGivenLife(givenHealth);
							}
                        }
                    }
                }

                Die();
                if (healthJauge != null)
                {
                    GameObject.Destroy(healthJauge.gameObject);
                }
            }
            // Make sure that the health never exceeds the maximum health
            else
            {
                if (reactionanimations != null)
				{
                    if (amount < 0f && !string.IsNullOrEmpty(reactionWhenGotHitBase))
                        reactionanimations.SetTrigger(reactionWhenGotHitBase);

                    if (reactionAnimSteps != null && reactionAnimSteps.Length > 0)
					{
                        float oldPercent = oldcurrentHealth / maxHealth * 100f;
                        float newPercent = currentHealth / maxHealth * 100f;

                        for (int i = 0; i < reactionAnimSteps.Length; ++i)
						{
                            ReactionAnimationStep step = reactionAnimSteps[i];
                            if (oldPercent > step.percentLife && newPercent <= step.percentLife)
							{
                                if (!string.IsNullOrEmpty(step.triggerName))
                                    reactionanimations.SetTrigger(step.triggerName);
                            }
                        }
					}
                }   

                if (currentHealth > maxHealth)
                {
                    currentHealth = maxHealth;
                    if (healthJauge != null)
                    {
                        healthJauge.Disable();
                    }
                }
                // just update
                else
                {
                    UpdateJauge();
                }
            }
        }
    }

    public void UpdateJauge()
	{
        if (hasHealthJauge)
        {
            if (healthJauge == null)
            {
                UI_HealthJauge prefab = jaugeSpecificPrefab ?? gamesettings.myself.healthJaugePrefab;
                if (jaugeParent != null)
                {
                    healthJauge = GameObject.Instantiate<UI_HealthJauge>(prefab, jaugeParent);
                    healthJauge.transform.localPosition = new Vector3(0f, 0f, 0f);
                }
                else
                {
                    healthJauge = GameObject.Instantiate<UI_HealthJauge>(prefab, transform);
                    healthJauge.transform.localPosition = new Vector3(0f, fJaugeOffsetY, 0f);
                }
#if DEBUG_INSTANCE_ID
                healthJauge.SetName(_instanceId.ToString());
#endif
            }
            healthJauge.UpdateValue(currentHealth / maxHealth);
        }
    }

    private void UpdateStats(int teamOfTheShooter, bool bIsMeTheShooter)
    {
        if (deathGain > 0 && isTreasureType)
        {
            IncrementStat(gamesettings.STAT_POINTS, deathGain, teamOfTheShooter, bIsMeTheShooter);
            if (UI_Tutorial.myself != null && healtObjectType == HealtObjectType.coins)
                UI_Tutorial.myself.OnTriggerCondition(UI_Tutorial.TutoCondition.Gold);
        }
        switch (healtObjectType)
        {
            case HealtObjectType.turret:
                IncrementStat(gamesettings.STAT_TURRETS, 1, teamOfTheShooter, bIsMeTheShooter);
                if (UI_Tutorial.myself != null)
                    UI_Tutorial.myself.OnTriggerCondition(UI_Tutorial.TutoCondition.Turret);
                break;
            case HealtObjectType.bird:
                IncrementStat(gamesettings.STAT_BIRDS, 1, teamOfTheShooter, bIsMeTheShooter);
                break;
            case HealtObjectType.boat:
                IncrementStat(gamesettings.STAT_BOATSKILLER, 1, teamOfTheShooter, bIsMeTheShooter);
                break;
            case HealtObjectType.chest:
                IncrementStat(gamesettings.STAT_CHESTS, 1, teamOfTheShooter, bIsMeTheShooter);
                if (UI_Tutorial.myself != null)
                    UI_Tutorial.myself.OnTriggerCondition(UI_Tutorial.TutoCondition.Chest);
                break;
            case HealtObjectType.lava:
                IncrementStat(gamesettings.STAT_LAVA, 1, teamOfTheShooter, bIsMeTheShooter);
                break;
            case HealtObjectType.pirate:
                IncrementStat(gamesettings.STAT_KILLS, 1, teamOfTheShooter, bIsMeTheShooter);
                break;
            case HealtObjectType.heart:
                IncrementStat(gamesettings.STAT_HEARTS, 1, teamOfTheShooter, bIsMeTheShooter);
                break;
            case HealtObjectType.mermaid:
                IncrementStat(gamesettings.STAT_MERMAIDS, 1, teamOfTheShooter, bIsMeTheShooter);
                break;
            case HealtObjectType.monster:
                IncrementStat(gamesettings.STAT_MONSTERS, 1, teamOfTheShooter, bIsMeTheShooter);
                break;
            case HealtObjectType.skull:
                IncrementStat(gamesettings.STAT_SKULLS, 1, teamOfTheShooter, bIsMeTheShooter);
                if (UI_Tutorial.myself != null)
                    UI_Tutorial.myself.OnTriggerCondition(UI_Tutorial.TutoCondition.Skull);
                break;
            case HealtObjectType.splineswitch:
                gameflowmultiplayer.myself.pathchanger = (int)collectableparam;
                break;
            case HealtObjectType.shield:
            {
                if (lasthitted != null)
                {
                    Player.IPlayer player = lasthitted.GetComponent<Player.IPlayer>();
                    if (player != null)
                    {
                        boat_followdummy boat = player.GetBoat();
                        if (boat != null && boat.shield != null)
                        {
                            boat.shield.ActivateShield(shieldtimer);
                            IncrementStat(gamesettings.STAT_SHIELDS, 1, teamOfTheShooter, bIsMeTheShooter);
                        }
                    }
                }
            }
            break;
            case HealtObjectType.drone:
            case HealtObjectType.mine:
            case HealtObjectType.superDrone:
            case HealtObjectType.megaDroid:
            case HealtObjectType.plasmaBomb:
            case HealtObjectType.bomber:
            case HealtObjectType.conveyor:
            case HealtObjectType.droneUltra:
                IncrementStat(healtObjectType.ToString(), 1, teamOfTheShooter, bIsMeTheShooter);
                break;
        }
    }

    private void IncrementStat(string statId, double increment, int teamOfTheShooter, bool bIsMeTheShooter)
    {
        GameflowBase.TriggerRaceEvent(teamOfTheShooter, bIsMeTheShooter, statId, increment);
        if (bIsMeTheShooter)
            GameflowBase.IncrementPlayerStat(statId, increment);
    }

    public bool tryhit = false;
    public bool tryshoot = false;
    public bool trydie = false;
    public bool tryalive = false;

    private void Update()
    {
#if UNITY_EDITOR
        if (reactionanimations != null)
        {
            if (tryhit)
            {
                if (!string.IsNullOrEmpty(reactionWhenGotHitBase))
                    reactionanimations.SetTrigger(reactionWhenGotHitBase);
                tryhit = false;
            }
            if (tryshoot)
            {
                reactionanimations.SetTrigger("Shoot");
                tryshoot = false;
            }            
            if (tryalive)
            {
                reactionanimations.SetTrigger("IsAlive");
                tryalive = false;
            }
        }
        if (trydie)
        {
            UpdateStats(0, true);
            Die();
            trydie = false;
        }
#endif

        if (animationBeforeDeath) // if the variable is enabled, the gameObject is destroyed at the end of the animation
        {
            if (animatorDeath != null && animationDeath != null)
                if (animatorDeath.GetCurrentAnimatorStateInfo(0).IsName(animationDeath.name) &&
                    (animatorDeath.GetCurrentAnimatorStateInfo(0).normalizedTime > animatorDeath.GetCurrentAnimatorStateInfo(0).length))
                    Destroy(gameObject);
        }

        // Respawn Part
        if (dead && respawnTime > 0f)
		{
            // Respawn test
            if (Time.time - _lastDiedTime > respawnTime)
            {
                // Delay to respawn is over
                reactionanimations.Rebind();
                currentHealth = startingHealth;
                dead = false;
                _startTime = Time.time;
                // Children pos
                if (_childrenPosData != null)
                {
                    foreach (var keyval in _childrenPosData)
                    {
                        keyval.Value.ApplyOnTransform(keyval.Key);
                    }
                }
            }
        }

        if (_isDying && dyingTime > 0f)
		{
            if (Time.time - _lastDiedTime > dyingTime)
			{
                _isDying = false;
                if (destroyAfterDying)
                    Destroy(gameObject);
            }
		}

        // Auto die
        if (autoDieAfterDelay > 0f && !dead)
		{
            if (Time.time - _startTime > autoDieAfterDelay)
			{
                Die();
			}
		}
    }

    public void Die()
    {
        // This GameObject is officially dead.  This is used to make sure the Die() function isn't called again
        dead = true;
        _isDying = true;

        onDieCallback?.Invoke(this);

        onDieActions?.Invoke();

        if (_progression != null)
            _progression.CollectItem();

		if (reactionanimations != null)
			reactionanimations.SetTrigger("IsDead");

		_lastDiedTime = Time.time;

        // Play DeathAnimation if enabled, and animator exists, and animation clip exists
        if (animationBeforeDeath && animatorDeath != null && animationDeath != null)
            animatorDeath.Play(animationDeath.name);

        // Make death effects
        if (activateWhenDead) activateWhenDead.SetActive(true);
        if (replaceWhenDead)
        {
            CheckFxParent();
            Instantiate(deadReplacement, transform.position, transform.rotation, _fxParent);
        }
        if (makeExplosion)
        {
            CheckFxParent();
            Instantiate(explosion, transform.position, transform.rotation, _fxParent);
        }

        if (isPlayer && deathCam != null)
            deathCam.SetActive(true);


        // Remove this GameObject from the scene if there is no animation before death
        if ((reactionanimations == null) && (!animationBeforeDeath))
            Destroy(gameObject);
    }

    public static void CheckFxParent()
	{
        if (_fxParent == null)
		{
            GameObject go = new GameObject("FX");
            _fxParent = go.transform;
		}
    }

    public static void ChangeHealthNow(GameObject target, float amount, int projectileId=-1)
    {
        Health h = target.GetComponentInParent<Health>();
        if (h != null && !h.isProtectedByShield)
            h.ChangeHealth(amount, projectileId);
    }

    public static void StealGoldNow(GameObject target, float gold, int projectileId = -1)
    {
        Health h = target.GetComponentInParent<Health>();
        if (h != null && !h.isProtectedByShield)
            h.AddStolenGold(gold, projectileId);
    }
}
