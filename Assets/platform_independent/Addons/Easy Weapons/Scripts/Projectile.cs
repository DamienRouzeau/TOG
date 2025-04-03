/// <summary>
/// Projectile.cs
/// Author: MutantGopher
/// Attach this script to your projectile prefabs.  This includes rockets, missiles,
/// mortars, grenade launchers, and a number of other weapons.  This script handles
/// features like seeking missiles and the instantiation of explosions on impact.
/// </summary>

//#define DEBUG_HIT
//#define DEBUG_IGNORE

using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;

public enum ProjectileType
{
    Standard,
    Seeker,
    ClusterBomb
}
public enum DamageType
{
    Direct,
    Explosion,
    Manual
}

public class Projectile : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    public ProjectileType projectileType = ProjectileType.Standard;     // The type of projectile - Standard is a straight forward moving projectile, Seeker type seeks GameObjects with a specified tag
    public DamageType damageType = DamageType.Direct;                   // The damage type - Direct applys damage directly from the projectile, Explosion lets an instantiated explosion handle damage
    public float damage = 100.0f;                                       // The amount of damage to be applied (only for Direct damage type)
    public int stolenGold = 0;                                          // The amount of gold to be stolen
    public float speed = 10.0f;                                         // The speed at which this projectile will move
    public float initialForce = 1000.0f;                                // The force to be applied to the projectile initially
    public float lifetime = 30.0f;                                      // The maximum time (in seconds) before the projectile is destroyed

    public float seekRate = 1.0f;                                       // The rate at which the projectile will turn to seek enemies
    public string seekTag = "Enemy";                                    // The projectile will seek gameobjects with this tag
    public float seekSpeed = 1f;                                        // The projectile will seek gameobjects with this speed
    public GameObject explosion;                                        // The explosion to be instantiated when this projectile hits something
    public float targetListUpdateRate = 1.0f;                           // The rate at which the projectile will update its list of all enemies to target

    public GameObject clusterBomb;                                      // The array of bombs to be instantiated on explode if this projectile is of clusterbomb type
    public int clusterBombNum = 6;                                      // The number of cluster bombs to instantiate

    public int weaponType = 0;                                          // Bloody Mess support variable

    private float lifeTimer = 0.0f;                                     // The timer to keep track of how long this projectile has been in existence
    private float targetListUpdateTimer = 0.0f;                         // The timer to keep track of how long it's been since the enemy list was last updated
    public GameObject[] enemyList;                                     // An array to hold possible targets

    public GameObject projectile_creator = null;
    public poolhelper.ProjectileSize projectilesize = poolhelper.ProjectileSize.small;
    public string customSize = null;

    public static int idCounter = 0;
    public int id { get; set; } = -1;

    public GameObject tailfocus = null;
    public GameObject tail = null;

    public float damageperiod = 0.0f;

    public bool invisiblealive = false;
    public bool useNetIdToFindCreator = false;
    public bool useScaleForTail = false;

    public float seekStartTime = 0f;
    public float seekEndTime = 0f;

    public bool isStoppedByWalls = true;
    public LayerMask wallLayers = 65536;
    
    public LayerMask ignoreLayers = 0;

    public float destroytimer = 0.0f;

    public bool isActivated => multiplayerbullet != null && multiplayerbullet.activeInHierarchy;
    public bool isDeactivated => multiplayerbullet != null && !multiplayerbullet.activeInHierarchy;

    private GameObject _lastPlayerHit = null;
    private float startlifetime = -1.0f;
    private GameObject multiplayerbullet = null;
    private Rigidbody _rb;

    private float damagetimer = -1.0f;

    private Vector3 oldcoord = Vector3.zero;
    private int raycastwait = 0;
    private Vector3 displayposition = Vector3.zero;
    private PhotonTransformBulletView _photonBullet = null;

    private Transform _enemyTarget = null;
    private Collider _collider = null;
    private int _colLayerMask = 0;
    private bool _forceTarget = false;

    void Start()
    {
        if (damageType == DamageType.Manual)
            return;

        bullet_tag bulletTag = gameObject.GetComponent<bullet_tag>();
        if (bulletTag != null)
            multiplayerbullet = bulletTag.myObject;

        invisiblealive = false;
              /*
              initialForce = 50;
              lifetime = 500;
*/      
        // Initialize the enemy list
        UpdateEnemyList();
        ChooseTarget();
        _rb = GetComponent<Rigidbody>();
        if (_rb)
            _rb.AddRelativeForce(0, 0, initialForce);

        _photonBullet = GetComponent<PhotonTransformBulletView>();
        _collider = GetComponent<Collider>();

        ActivateBullet(false);

        _colLayerMask = physicslayermask.MaskForLayer(gameObject.layer);
    }

    public void CheckAvatarCreator()
    {
        if (projectile_creator == null && useNetIdToFindCreator)
        {
            PhotonView pv = GetComponent<PhotonView>();
            if (pv != null)
            {
                foreach (Player_avatar avatar in Player.myplayer.avatars)
                {
                    if (avatar.actornumber == pv.OwnerActorNr - 1)
                    {
                        projectile_creator = avatar.gameObject;
#if DEBUG_HIT
                        Debug.Log($"[HEALTH] CheckAvatarCreator creator {projectile_creator} num {avatar.actornumber}");
#endif
                        break;
                    }
                }
            }
        }
    }

    

    // Update is called once per frame
    void Update()
    {
        if (damageType == DamageType.Manual)
            return;

        if (!invisiblealive && isDeactivated && _rb != null && _rb.velocity != Vector3.zero)
            _rb.velocity = Vector3.zero;

        DestroyTime();

        if (isDeactivated)
            return;

        if (_rb == null || !_rb.isKinematic)
        {
            // Update the timer
            lifeTimer += Time.deltaTime;

            // Destroy the projectile if the time is up
            if (lifeTimer >= lifetime)
                DestroyMyself();

            // Make the projectile move
            if (_rb != null && initialForce == 0)      // Only if initial force is not being used to propel this projectile
            {
                _rb.velocity = transform.forward * speed;
            }
            // Make the projectile seek nearby targets if the projectile type is set to seeker
            if (projectileType == ProjectileType.Seeker)
            {
                // Keep the timer updating
                targetListUpdateTimer += Time.deltaTime;

                // If the targetListUpdateTimer has reached the targetListUpdateRate, update the enemy list and restart the timer
                if (targetListUpdateTimer >= targetListUpdateRate)
                {
                    UpdateEnemyList();
                    ChooseTarget();
                    targetListUpdateTimer = 0.0f;
                }

                if (_enemyTarget != null &&
                    (seekStartTime == 0f || lifeTimer >= seekStartTime) &&
                    (seekEndTime == 0f || lifeTimer < seekEndTime))
                {
                    // Rotate the projectile to look at the target
                    Quaternion targetRotation = Quaternion.LookRotation(_enemyTarget.position - transform.position);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * seekRate);
                }
            }
        }
        displayposition = transform.position;
    }

    private void LateUpdate()
	{
        if (damageType == DamageType.Manual)
            return;

        if (useScaleForTail && tail != null && tailfocus != null && isActivated)
        {
            float distanceFromCanon = gamesettings_player.myself.trailMinDistanceFromCannon;
            float distance = Vector3.Distance(transform.position, tailfocus.transform.position) - distanceFromCanon;
            float globalScale = transform.lossyScale.x;
            float minDistance = gamesettings_player.myself.trailMinDistanceToShow;
            float maxDistance = gamesettings_player.myself.trailMaxLength;
            if (distance > minDistance)
            {
                float scl = Mathf.Min(distance, maxDistance) / globalScale;
                Vector3 tailScale = tail.transform.parent.localScale;
                tail.transform.parent.localScale = new Vector3(tailScale.x, tailScale.y, scl);
                tail.transform.parent.LookAt(tailfocus.transform);
                tail.gameObject.SetActive(true);
            }
            else
			{
                tail.gameObject.SetActive(false);
            }
        }
    }

	public void SetOldCoord(Vector3 pos)
    {
        oldcoord = pos;
    }

    public bool RayCastOldToNew(int projectileId, bool showRaycastLog = false)
    {
        bool hasHit = false;

        Vector3 objpos = transform.position;

		//if (showRaycastLog)
		//Debug.Log($"RAYCAST enter RayCastOldToNew oldcoord {oldcoord} objpos {objpos} projectileId {projectileId}");

		if (raycastwait > 0)
        {
            if (tailfocus != null)
                oldcoord = tailfocus.transform.position;
            raycastwait--;
        }
        //        Debug.Log("oldcoord:" + oldcoord.x + ";" + oldcoord.y + ";" + oldcoord.z + "    " + objpos.x + ";" + objpos.y + ";" + objpos.z);
        if (oldcoord != Vector3.zero && oldcoord != objpos)
        {
#if DEBUG_HIT
            Debug.Log($"RaycastHit RayCastOldToNew {oldcoord} to {objpos}");
#endif
            //if (showRaycastLog)
            //Debug.Log($"RAYCAST it s possible oldcoord {oldcoord} objpos {objpos} projectileId {projectileId}");
            // raytrace between 2 Vectors and see if a collision was made
            float distance = Vector3.Distance(oldcoord, objpos);
            Vector3 direction = (objpos - oldcoord) / distance;
            int testlayer = _colLayerMask;
            //Debug.Log($"BULLET Raycast {oldcoord} {direction} {distance} {oldcoord + direction * distance}");
            // Add 1m to be sure to pass through
            RaycastHit[] hits = Physics.RaycastAll(oldcoord, direction, distance + 1f, testlayer);
            if (hits.Length > 0)
            {
                if (hits.Length > 1)
                {
                    List<RaycastHit> hitList = new List<RaycastHit>();
                    hitList.AddRange(hits);
                    hits = hitList.OrderBy(h => h.distance).ToArray();
                }
                //if (showRaycastLog)
                CheckAvatarCreator();
                //Debug.Log($"RAYCAST find hits {hits.Length} oldcoord {oldcoord} objpos {objpos} projectileId {projectileId} creator {projectile_creator}");
                foreach (RaycastHit hit in hits)
                {
                    if (gameObject != hit.collider.gameObject)
                    {
						displayposition = hit.point;
                        //Debug.Log($"BULLET Raycast - hit at {displayposition}");
                        //if (showRaycastLog)
                        //Debug.Log($"RAYCAST hit {hit} displayposition {displayposition} dist {(oldcoord- displayposition).magnitude}");
                        if (hit.collider.gameObject.tag == "PlayerBody")
                        {
                            if (IsShooterFromSameTeam(hit.collider.gameObject))
                                return false;
                            if (IgnoreTesting(hit.collider.gameObject)) 
                                return false;
    						_lastPlayerHit = hit.collider.gameObject;
							PlayerHit();
                            Player.IPlayer player = _lastPlayerHit.GetComponentInParent<Player.IPlayer>();
                            if (player != null)
                                GameflowBase.instance.SendDamageOnOtherPlayer(player.id, damage); 
                            hasHit = true;
                        }
                        else
                        {
                            hasHit = HitCollider(hit.collider.gameObject, displayposition, true);
                        }
						if (hasHit)
						{
                            //if (showRaycastLog)
                            //Debug.Log($"RAYCAST hit {hit.collider.gameObject} displayposition {displayposition} dist {(oldcoord - displayposition).magnitude}");
                            break;
                        }	
					}
                }
            }
            else
			{
				//if (showRaycastLog)
				//Debug.Log($"RAYCAST no hit for oldcoord {oldcoord} objpos {objpos} projectileId {projectileId}");
			}
        }
        oldcoord = transform.position;
        return hasHit;
    }

    void FixedUpdate()
    {
        if (damageType == DamageType.Manual)
            return;
        if (_photonBullet != null && _photonBullet.needToRaycast)
			RayCastOldToNew(id);
		UpdateTargetDirection();
    }

    public void ActivateBullet(bool activate)
	{
        if (multiplayerbullet != null)
        {
            multiplayerbullet.SetActive(activate);
            if (_collider != null)
                _collider.enabled = activate;
        }
	}

    private void UpdateTargetDirection()
	{
        if (projectileType == ProjectileType.Seeker && _enemyTarget != null)
        {
            if ((seekStartTime == 0f || lifeTimer >= seekStartTime) &&
                (seekEndTime == 0f || lifeTimer < seekEndTime))
            {
                if (_rb != null && _rb.velocity != Vector3.zero)
                {
                    float magnitude = _rb.velocity.magnitude;
                    Vector3 dir = _enemyTarget.position - transform.position;
                    if (dir != Vector3.zero)
                        _rb.velocity = Vector3.ClampMagnitude(_rb.velocity + dir.normalized * (magnitude * Time.fixedDeltaTime * seekSpeed), magnitude);
                }
            }
        }
    }

    void UpdateEnemyList()
    {
        if (!string.IsNullOrEmpty(seekTag) && !_forceTarget)
        {
            enemyList = GameObject.FindGameObjectsWithTag(seekTag);
#if USE_KDK || USE_BOD
            List<GameObject> list = new List<GameObject>(enemyList);
            foreach (GameObject go in enemyList)
            {
                TargetHealth targetH = go.GetComponent<TargetHealth>();
                if (targetH != null)
                {
                    if (targetH.targetHealth == null || targetH.targetHealth.dead || targetH.targetHealth.currentHealth <= 0f)
                        list.Remove(go);
                }
                else
                {
                    list.Remove(go);
                }
            }
            enemyList = list.ToArray();
#endif
        }
    }

    private void ChooseTarget()
    { 
        if (enemyList != null && enemyList.Length > 0 && !_forceTarget)
        {
            // Choose a target to "seek" or rotate toward
            float greatestDotSoFar = -1.0f;
            _enemyTarget = null;
            foreach (GameObject enemy in enemyList)
            {
                if (enemy != null)
                {
                    Vector3 direction = enemy.transform.position - transform.position;
                    float dot = Vector3.Dot(direction.normalized, transform.forward);
                    if (dot > greatestDotSoFar)
                    {
                        greatestDotSoFar = dot;
                        _enemyTarget = enemy.transform;
                    }
                }
            }
        }
    }

    public void SetTarget(Transform tr)
	{
        _enemyTarget = tr;
        _forceTarget = true;
        enemyList = new GameObject[] { tr.gameObject };
    }

    bool IgnoreTesting(GameObject obj)
    {
#if DEBUG_IGNORE
        Debug.Log($"[IGNORE] IgnoreTesting({obj})");
#endif
        if (invisiblealive)
            return true;
        if (isDeactivated)
            return true;
        if (obj == projectile_creator)
            return true;
        if (ignoreLayers > 0 && ((1 << obj.layer) & ignoreLayers) > 0)
            return true;
        if (GameflowBase.instance != null && GameflowBase.instance.endRaceScreen != null)
            return true;
        if (obj.GetComponent<activationdetection>())
            return true;
#if USE_TOG
        // Don't ignore enemy on boat i want to shoot
        if (obj.GetComponent<EnemyOnBoat>())
            return false;
        if (obj.GetComponent<Projectile>())
            return (true);
#endif

        if (projectile_creator != null)
        {
            // Ignore myself
            // ignore children of touched object
            ignoreprojectilesfromchilds ipfc = obj.GetComponentInParent<ignoreprojectilesfromchilds>();
            if (ipfc != null)
            {
                if (obj == projectile_creator)
                {
#if DEBUG_IGNORE
                    Debug.Log($"[IGNORE] Hit the creator !!! {projectile_creator}");
#endif
                    return (true);
                }

                if (ipfc.father != null)
                {
                    foreach (Transform t in ipfc.father.GetComponentsInChildren<Transform>(true))
                    {
                        if (t.gameObject == projectile_creator)
                        {
                            // Don't ignore enemy on boat that damage my boat
                            if (projectile_creator.GetComponent<EnemyOnBoat>())
                                return false;
#if DEBUG_IGNORE
                            Debug.Log($"[IGNORE] Hit a child of {ipfc.father} !!! {projectile_creator}");
#endif
                            return (true);
                        }
                    }
                }
            }

            if (gamesettings_player.myself.canKill)
			{
                Player_avatar avatar = obj.GetComponentInParent<Player_avatar>();
                if (avatar != null)
				{
                    if (!avatar.isVisible)
					{
#if DEBUG_IGNORE
                        Debug.Log($"[IGNORE] Avatar is invisible {avatar} !!! {projectile_creator}");
#endif
                        return true;
                    }
				}
            }
            else
            {
                Player player = obj.GetComponentInParent<Player>();
                if (player != null)
                {
#if DEBUG_IGNORE
                    Debug.Log($"[IGNORE] Shoot on your player {player} !!! {projectile_creator}");
#endif
                    return true;
                }

                PlayerBody[] bodies = projectile_creator.GetComponentsInChildren<PlayerBody>();
				foreach (PlayerBody body in bodies)
				{
					if (obj == body.gameObject)
						return (true);
				}
				foreach (Transform tm in projectile_creator.GetComponentsInChildren<Transform>())
				{
					if (tm.gameObject.tag == "Player")
					{
						if (obj == tm.gameObject)
							return (true);
					}
				}
			}
#if USE_TOG
            // Don't ignore enemy on boat that damage my boat
            if (projectile_creator.GetComponent<EnemyOnBoat>())
                return false;

            // ignore boat??
            boat_followdummy bfd1 = obj.GetComponentInParent<boat_followdummy>();
            if (bfd1 != null)
            {
                boat_followdummy bfd2 = projectile_creator.GetComponentInParent<boat_followdummy>();
                if (bfd1 == bfd2)
                {
#if DEBUG_IGNORE
                    Debug.Log("[IGNORE] creator & obj on the same boat !!! " + obj);
#endif
                    return true;
                }
            }
#endif
        }
        return (false);
    }
    // damageperiod
    void OnCollisionStay(Collision col)
	{
		if (damageperiod == 0.0f)
			return;
		OnCollisionEnter(col);
	}

	void OnCollisionEnter(Collision col)
	{
		//Projectile pr = col.gameObject.GetComponent<Projectile>();
		//if (pr) return;
		Debug.Log("OnCollisionEnter " + gameObject.name + " HIT " + col.gameObject.name);

		Hit(col);
	}
    
	private void OnTriggerStay(Collider other)
	{
		if (damageperiod == 0.0f)
			return;
		OnTriggerEnter(other);
	}
   
	private void OnTriggerEnter(Collider other)
	{
		// if projectile is from a player, raycasts are done every frame
		if (id >= 0 && !gamesettings_player.myself.canKill)
			return;
#if DEBUG_HIT
	        Debug.Log($"DEBUGHIT OnTriggerEnter {transform.position}");
#endif
		if (other.gameObject.tag == "PlayerBody")
		{
            if (IsShooterFromSameTeam(other.gameObject))
                return;
			if (IgnoreTesting(other.gameObject)) 
                return;
			if (_lastPlayerHit == null)
				_lastPlayerHit = other.gameObject;
			PlayerHit();
		}
		else if (id < 0)
		{
#if DEBUG_HIT
	            Debug.Log($"DEBUGHIT OnTriggerEnter HitCollider other {other} id {id}");
#endif
			HitCollider(other.gameObject, other.bounds.center, false);
		}
	}

    private bool IsShooterFromSameTeam(GameObject goPlayer)
	{
        if (projectile_creator != null)
        {
            Player.IPlayer playerCreator = projectile_creator.GetComponentInParent<Player.IPlayer>();
            if (playerCreator != null)
            {
                Player.IPlayer player = goPlayer.gameObject.GetComponentInParent<Player.IPlayer>();
                if (playerCreator.GetTeam() == player.GetTeam())
                    return true;
            }
        }
        return false;
    }

    private void PlayerHit()
    {
        if (TowerDefManager.myself != null)
        {
            if (!TowerDefManager.myself.canShoot)
                return;
        }

        if (_lastPlayerHit != null)
        {
            PlayerBody pb = _lastPlayerHit.GetComponent<PlayerBody>();
            if (pb != null)
            {
                pb.AddDamage(damage);
            }
        }
#if USE_TOG
        Health.CheckFxParent();
        GameObject go = PhotonNetworkController.InstantiateSoloOrMulti("Sparks", transform.position, Quaternion.identity);
        go.transform.SetParent(Health.fxParent);
#endif
        DestroyMyself();
    }

    public bool TriggerHitCollider(GameObject creator, GameObject obj, Vector3 hitPos, bool fixedDeltaTime = true)
	{
        projectile_creator = creator;
        return HitCollider(obj, hitPos, false, fixedDeltaTime);

    }

    bool HitCollider(GameObject obj, Vector3 hitPos, bool fromOther, bool fixedDeltaTime = true)
    {
        if (GameflowBase.instance == null)
            return false;

#if DEBUG_HIT
        Debug.Log($"HitCollider obj {obj} fromOther {fromOther}");
        
        if (gameObject.transform.parent != null)
            Debug.Log("HITCOLLIDER ("+gameObject.name+"/"+gameObject.transform.parent.name+") HIT "+obj.name);
        else
            Debug.Log("HITCOLLIDER (" + gameObject.name+ ") HIT " + obj.name);
#endif

        CheckAvatarCreator();

        if (projectile_creator == null)
        {
#if DEBUG_HIT
            Debug.Log($"HitCollider obj {obj} fromOther {fromOther} but no projectile_creator!");
#endif
            return false;
        }

#if DEBUG_IGNORE
        Debug.Log($"[IGNORE] Hit obj {obj} creator {projectile_creator} fromOther {fromOther}");
#endif

        if (IgnoreTesting(obj))
        {
#if DEBUG_IGNORE
            Debug.Log($"[IGNORE] IgnoreTesting obj {obj} creator {projectile_creator} fromOther {fromOther}");
#endif
#if DEBUG_HIT
            Debug.Log($"HitCollider IgnoreTesting obj {obj}");
#endif
            return false;
        }

        bool destroy = true;
        // Apply damage to the hit object if damageType is set to Direct
        if (damageType == DamageType.Direct || damageType == DamageType.Manual)
        {
#if USE_KDK || USE_BOD
            if (isStoppedByWalls && ((1 << obj.layer) & wallLayers) > 0)
			{
#if DEBUG_HIT
                Debug.Log($"HitCollider isStoppedByWalls obj {obj} at {hitPos}"); 
#endif
                poolhelper.myself?.CreateImpact(gameObject, hitPos, obj, projectilesize, customSize);
                DestroyMyself();
                return true;
            }
#endif
            Health h = obj.GetComponentInParent<Health>();
            if (h != null)
            {
                obj = h.gameObject;
                // Don't apply damage if boat have a shield
                if (h.isProtectedByShield)
                {
#if DEBUG_IGNORE
                    Debug.Log($"[IGNORE] isProtectedByShield obj {obj} creator {projectile_creator} fromOther {fromOther}");
#endif
                    poolhelper.myself?.CreateImpact(gameObject, displayposition, obj, projectilesize, customSize);
                    DestroyMyself();
                    return true;
                }

                h.lasthitted = projectile_creator;
            }

            Player.IPlayer iplayer = projectile_creator.GetComponent<Player.IPlayer>();

            if (damageperiod == 0.0f)
            {
                float dam = -damage;

                float multiplier = 1f;
                
                if (iplayer != null)
                {
                    int team = iplayer.GetTeam();
                    int playerCountInBoat = GameflowBase.GetActorCountInTeam(team);
                    if (projectilesize == poolhelper.ProjectileSize.small)
                        multiplier = gamesettings_difficulty.myself.GetBulletMultiplier(playerCountInBoat);
                    else
                        multiplier = gamesettings_difficulty.myself.GetCannonMultiplier(playerCountInBoat);
                }                
#if DEBUG_HIT
                Debug.Log($"HitCollider dam {dam} - multiplier {multiplier} - obj {obj.name}");
#endif
                dam *= multiplier;

                poolhelper.myself?.CreateImpact(gameObject, displayposition, obj, projectilesize, customSize);
#if DEBUG_HIT
                Debug.Log($"DEBUGHIT HitCollider obj {obj} ChangeHealthNow {dam} {projectile_creator} {boat_followdummy.GetPath(transform)}");
#endif
                if (_photonBullet != null)
                    _photonBullet.TriggerHit(obj, hitPos, dam, id);
                if (iplayer == null || iplayer == (Player.IPlayer)Player.myplayer)
                    Health.ChangeHealthNow(obj, dam, id);
                if (stolenGold > 0)
                    Health.StealGoldNow(obj, stolenGold, id);
            }
            else
            {
                destroy = false;
                float dam = -damage * (fixedDeltaTime ? Time.fixedDeltaTime : Time.deltaTime);
                if (damageperiod != -1.0f)      // endless
                {
                    if (damagetimer == -1.0f)                        damagetimer = 0.0f;
                    damagetimer += Time.fixedDeltaTime;
                    if (damagetimer >= damageperiod)                    destroy = true;
                }

                if (iplayer == null || iplayer == (Player.IPlayer)Player.myplayer)
                    Health.ChangeHealthNow(obj, dam, id);

                if (stolenGold > 0)
                    Health.StealGoldNow(obj, stolenGold * Time.fixedDeltaTime, id);
            }

            //call the ApplyDamage() function on the enenmy CharacterSetup script
            if (obj.layer == LayerMask.NameToLayer("Limb"))
            {
                Vector3 directionShot = obj.transform.position - transform.position;

                // Un-comment the following section for Bloody Mess support
                /*
				if (c.gameObject.GetComponent<Limb>())
				{
					GameObject parent = c.gameObject.GetComponent<Limb>().parent;
					CharacterSetup character = parent.GetComponent<CharacterSetup>();
					character.ApplyDamage(damage, c.gameObject, weaponType, directionShot, Camera.main.transform.position);
				}
				*/
            }
        }
        if (destroy)        
            DestroyMyself();
        return true;
    }

    void Hit(Collision col)
    {
#if DEBUG_HIT
        Debug.Log($"DEBUGHIT HitCollider Hit collision {col} id {id}");
#endif
        HitCollider(col.gameObject, col.GetContact(0).point, false);
    }

    // Modify the damage that this projectile can cause
    public void MultiplyDamage(float amount)
    {
        damage *= amount;
    }

    // Modify the inital force
    public void MultiplyInitialForce(float amount)
    {
        initialForce *= amount;
    }
    
    void DestroyTime()
    {
        if (destroytimer > 0.0f)
        {
            destroytimer -= Time.deltaTime;
            if (destroytimer <= 0.0f)
            {
                if (multiplayerbullet != null)
                {
                    ActivateBullet(false);
                    if (_rb != null)
                        _rb.velocity = Vector3.zero;
                    if (tailfocus != null)
                        tailfocus.transform.SetParent(multiplayerbullet.transform);                    
                    
                }
                else
				{
                    DestroyMyself();
                }
                destroytimer = 0.0f;
            }
        }
        else
        {
            //            if ((multiplayerbullet) && (multiplayerbullet.activeInHierarchy)) invisiblealive = false;
            if (multiplayerbullet != null)
                invisiblealive = false;
        }
    }

    public void DestroyMyself()
    {
        if (damageType == DamageType.Manual)
            return;

        if (tailfocus != null)
            tailfocus.transform.SetParent(gameObject.transform);

        PhotonView pv = GetComponent<PhotonView>();
        if (pv)
        {
            bool isMine = PhotonNetworkController.soloMode || pv.IsMine;
            if (isMine)
            {
                if (multiplayerbullet != null)
                {
                    if (isActivated)
                    {
                        //if (gameObject.name.Contains("bulletforpool"))
                        //    Debug.Log("BULLET DISTROYED");
                        ActivateBullet(false);
                        invisiblealive = true;
                        destroytimer = 1.0f;
                    }
                }
                else
                {
                    PhotonNetworkController.DestroySoloOrMulti(this.gameObject);
                }
            }
        }
        else
        {
            Health h = gameObject.GetComponent<Health>();
            if (h != null)
                h.Die();
            else
			    PhotonNetworkController.DestroySoloOrMulti(this.gameObject, true);
        }
    }

    public void Initialize()
    {
        if (startlifetime == -1.0f)
            startlifetime = lifetime;

        lifetime = startlifetime;
        displayposition = Vector3.zero;
        lifeTimer = 0;
        if (_rb != null)
            _rb.isKinematic = true;

    }


    public void Restart()
    {
        oldcoord = gameObject.transform.position;
        raycastwait = 3;
        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.AddRelativeForce(0, 0, initialForce);
        }

        AudioSource auso = gameObject.GetComponent<AudioSource>();
        if (auso)
        {
            auso.Stop();
            auso.Play();
        }
        
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        if (multiplayerlobby.mas != null)
            return;

        string name1 = $"PoolForActor_{info.Sender.ActorNumber - 1}";
        string name2 = gameObject.name.Replace("forpool(Clone)","") + "_pool";
        Transform actorParent = poolhelper.myself.transform.Find(name1);
        if (actorParent == null)
        {
            GameObject go = new GameObject(name1);
            go.transform.SetParent(poolhelper.myself.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            actorParent = go.transform;
        }
        Transform parent = actorParent.Find(name2);
        if (parent == null)
        {
            GameObject goType = new GameObject(name2);
            goType.transform.SetParent(actorParent);
            goType.transform.localPosition = Vector3.zero;
            goType.transform.localRotation = Quaternion.identity;
            parent = goType.transform;
        }
        transform.SetParent(parent);
    }

}

