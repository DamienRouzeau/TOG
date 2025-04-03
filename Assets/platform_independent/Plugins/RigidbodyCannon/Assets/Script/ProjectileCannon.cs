//#define DEBUG_COOLDOWN
//#define DEBUG_SFX

using System.Collections;
using UnityEngine;

public class ProjectileCannon : MonoBehaviour
{
    [System.Serializable]
    public class OtherCannonData
    {
        public bool isVisible;
        public Transform barrel;
        public GameObject[] parts;
        public ParticleSystem smoke;
        public ParticleSystem cantShoot;
    }


    [SerializeField] private bool cannonActive = true;
    [SerializeField] private GameObject projectile = null;
    [SerializeField] private Vector2 minMaxAngle = Vector2.zero;
    [SerializeField] private Transform cannonTurnBase = null;
    [SerializeField] private Transform cannonBarrel = null;
    [SerializeField] private ParticleSystem cannonSmoke = null;
    [SerializeField] private ParticleSystem cannonCantShoot = null;
    [SerializeField] private OtherCannonData[] otherCannons = null;
    [SerializeField] private float fireVelocity = 10;
    [SerializeField] private bool arcFire = false;
    [SerializeField] private AudioClip fireSound = null;
    [SerializeField] private AudioClip[] otherFireSounds = null;
    [SerializeField] private TOG_BatterieBehaviour batterie;

    [SerializeField] private Transform targetObject = null;
    [SerializeField] private string usethispool = "cannon_pool";

    [Header("Rate Shoot")]
    [SerializeField] private float _rateShoot = 0f;
    [SerializeField] private Animator _noShootAnimator = null;
    [SerializeField] private string _noShootParameter = null;
    [SerializeField] private float _noShootAnimationDuration = 2f;
    [SerializeField] private float _noShootAnimationSpeed = 2f;

    [Header("SFX")]
    [SerializeField] private float _sfxShootVolume = 1f;
    [SerializeField] private float _sfxCantShootVolume = 0.2f;

    [Header("Automatic")]
    [SerializeField] private bool _isAutomatic = false;
    [SerializeField] private float _automaticStartDelay = 0.2f;
    [SerializeField] private float _automaticRateShoot = 5;
    [SerializeField] private bool _automaticDispatchCannons = true;

    [Header("Laser")]
    [SerializeField] private KDK_LaserControl _laser = null;

    [Header("Reset")]
    [SerializeField] private UnityEngine.Events.UnityEvent _onResetGunEvents = null;

    private bool _isInCooldownGun = false;
    private float _startCooldownGunTime = 0f;
    private float _lastFireTime = 0f;
    private float _automaticDelayShoot = 1f;
    private int _automaticDispatchedNumCannon = 0;

    public float lastFireTime => _lastFireTime;

    public string poolname => _initialpool;

    public string projectileName => projectile?.name;

    public bullet_pool_creator._bullettype bulletType => bullet_pool_creator.GetBulletTypeFromName(projectileName);

    public Transform lookAt => cannonBarrel;

    boat_followdummy _bfd = null;

    private string _initialpool = "cannon_pool";

    private AudioSource _audioSource = null;

    private float _automaticStartTime = 0f;
    private float _automaticBeginTime = 0f;
    private int _currentCannonCount = 1;

    private void Awake()
    {
        _initialpool = usethispool;
    }

    private void OnEnable()
    {
        UpdateBoat();
        _isInCooldownGun = false;
        _automaticStartTime = 0f;
        _automaticBeginTime = 0f;
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if (cannonBarrel != null)
        {
            Debug.DrawRay(cannonBarrel.position, cannonBarrel.forward * fireVelocity, Color.white);
            Vector3 xComponenet = cannonBarrel.forward * fireVelocity;
            Vector3 yComponenet = cannonBarrel.forward * fireVelocity;
            xComponenet.y = 0;
            yComponenet.x = 0;
            yComponenet.z = 0;

            Debug.DrawRay(cannonBarrel.position, xComponenet, Color.red);
            Debug.DrawRay(cannonBarrel.position, yComponenet, Color.green);
        }
#endif
        if (cannonActive)
        {
            TurnCannonBase();
            TurnBarrel();
        }

        /*if( Input.GetKeyDown(KeyCode.V))
        {
            FireCannon();
        }*/

        if (_isAutomatic && _automaticStartTime > 0f)
        {
            if (_automaticBeginTime == 0f && Time.time - _automaticStartTime > _automaticStartDelay)
            {
                _automaticDelayShoot = 1f / _automaticRateShoot;
                if (_automaticDispatchCannons)
                {
                    _currentCannonCount = GetAvailableCannonCount();
                    _automaticDelayShoot /= _currentCannonCount;
                    _automaticDispatchedNumCannon = -1;
                }
                _automaticBeginTime = Time.time;
            }
            if (_automaticBeginTime > 0f && Time.time - _automaticBeginTime > _automaticDelayShoot)
            {
                _automaticBeginTime = Time.time;
                if (_automaticDispatchCannons)
                {
                    _automaticDispatchedNumCannon++;
                    int numCannon = _automaticDispatchedNumCannon % _currentCannonCount;
                    FireCannon(numCannon);
                }
                else
                {
                    FireCannon();
                }
            }
        }
    }

    public void UpdateBoat()
    {
        _bfd = gameObject.GetComponentInParent<boat_followdummy>();
    }

    public void TurnCannonBase()
    {
        if (targetObject == null) return;
        Vector3 baseTurnDir = targetObject.position - cannonTurnBase.position;
        baseTurnDir.y = 0;
        cannonTurnBase.forward = Vector3.Lerp(cannonTurnBase.forward, baseTurnDir, Time.deltaTime);
    }

    public void ChangeFireMode()
    {
        arcFire = !arcFire;
    }

    private float z;
    public void TurnBarrel()
    {
        if (targetObject == null) return;

        Vector3 H_levelPos = targetObject.position;
        H_levelPos.y = cannonBarrel.position.y;

        float Rx = Vector3.Distance(H_levelPos, cannonBarrel.position);

        float k = 0.5f * Physics.gravity.y * Rx * Rx * (1 / (fireVelocity * fireVelocity));

        float h = targetObject.position.y - cannonBarrel.position.y;

        float j = (Rx * Rx) - (4 * (k * (k - h)));

        if (j >= 0)
        {
            if (arcFire) z = (-Rx - Mathf.Sqrt(j)) / (2 * k);

            else z = (-Rx + Mathf.Sqrt(j)) / (2 * k);
        }
        cannonBarrel.localEulerAngles = new Vector3(Mathf.LerpAngle(cannonBarrel.localEulerAngles.x, Mathf.Clamp((Mathf.Atan(z) * 57.2958f), minMaxAngle.x, minMaxAngle.y) * -1, Time.deltaTime * 10), 0, 0);
    }

    private void SetNoShootAnimatorParameter(bool isInCooldown, float speed = 1f)
    {
        if (_noShootAnimator != null)
        {
            _noShootAnimator.speed = speed;
            _noShootAnimator.SetBool(_noShootParameter, !isInCooldown);
        }
    }

    private IEnumerator StartGunCooldown(float duration)
    {
#if DEBUG_COOLDOWN
        Debug.Log("[COOLDOWN] - StartGunCooldown enter");
#endif
        _isInCooldownGun = true;
        yield return new WaitForSeconds(duration);
        SetNoShootAnimatorParameter(false);
        _isInCooldownGun = false;
#if DEBUG_COOLDOWN
        Debug.Log("[COOLDOWN] - StartGunCooldown exit");
#endif
    }

    public void FireCannon(int numCannon = -1)
    {
        if (cannonBarrel == null)
            return;

        if (poolhelper.myself == null)
            return;
        if (batterie != null)
            if (!batterie.UseEnergy())
                return;

        usethispool = _initialpool;

        _lastFireTime = Time.time;

        bool canShoot = true;

        if (_rateShoot > 0f)
        {
            float cooldownDelay = 1f / _rateShoot;
            float time = _lastFireTime - _startCooldownGunTime;
#if DEBUG_COOLDOWN
            Debug.Log("[COOLDOWN] - Fire _isInCooldownGun " + _isInCooldownGun + " " + time + "/" + cooldownDelay);
#endif
            if (_isInCooldownGun)
            {
                usethispool = poolhelper.myself.cantShootBulletPoolName;
                canShoot = false;
            }
            else
            {
                _startCooldownGunTime = _lastFireTime;
                SetNoShootAnimatorParameter(true, _noShootAnimationSpeed * _noShootAnimationDuration / cooldownDelay);
                StartCoroutine(StartGunCooldown(cooldownDelay));
            }
#if DEBUG_COOLDOWN
            Debug.Log("[COOLDOWN] - canShoot " + canShoot);
#endif
        }

        if (gameflowmultiplayer.myself != null)
        {
            if (gameflowmultiplayer.gameplayEndRace)
            {
                usethispool = gameflowmultiplayer.amWinnerTeam ? poolhelper.myself.endRaceWonBulletPoolName : poolhelper.myself.endRaceLooseBulletPoolName;
            }
            if ((gameflowmultiplayer.myself.myboat != null) && (gameflowmultiplayer.myself.myboat.isSinking))
            {
                usethispool = poolhelper.myself.sinkingBulletPoolName;
                canShoot = false;
            }
        }

        if (TowerDefManager.myself != null)
        {
            canShoot = TowerDefManager.myself.canShoot;
            if (!canShoot)
                usethispool = null;
        }

        bool isPlayerBulletPool = poolhelper.myself.IsPlayerBulletPool(_initialpool);

        if (isPlayerBulletPool)
        {
            if (Player.myplayer.isDead || Player.myplayer.isTeleporting)
                return;
        }

        if (fireSound != null)
        {
            AudioClip clip = fireSound;
            if (otherFireSounds != null && otherFireSounds.Length > 0)
            {
                int rnd = Random.Range(0, otherFireSounds.Length + 1);
#if DEBUG_SFX
                Debug.Log($"[SFX] ProjectileCannon rnd {rnd} otherFireSounds.Length {otherFireSounds.Length} {Random.value} {Random.seed}");
#endif
                if (rnd < otherFireSounds.Length)
                    clip = otherFireSounds[rnd];
            }
#if DEBUG_SFX
            Debug.Log($"[SFX] ProjectileCannon PlayOneShot {clip.name}");
#endif
            if (_audioSource == null)
                _audioSource = GetComponent<AudioSource>();
            if (_audioSource != null)
            {
                _audioSource.volume = canShoot ? _sfxShootVolume : _sfxCantShootVolume;
                if (_audioSource.isPlaying)
                    _audioSource.Stop();
                _audioSource.PlayOneShot(clip);
            }
        }

        if (isPlayerBulletPool)
        {
            if (OnHover.lastButtonHover != null && OnHover.needPlayerBullet && Player.myplayer.gun == this)
            {
                OnHover.lastButtonHover.onClick.Invoke();
            }
        }

        if (poolhelper.myself.IsBoatBulletPool(_initialpool))
        {
            boat_canon_bait bait = transform.parent.GetComponentInChildren<boat_canon_bait>();
            if (bait != null && bait.lastColliderGO != null)
            {
                Player.IPlayer player = bait.lastColliderGO.GetComponentInParent<Player.IPlayer>();
                if (player != null && player is Player_avatar)
                {
                    Debug.Log($"[CANON] FireCanon by avatar {player}! -> don't shoot");
                    return;
                }
            }
        }

        ParticleSystem fx = (canShoot || cannonCantShoot == null) ? cannonSmoke : cannonCantShoot;

        if (string.IsNullOrEmpty(usethispool))
            return;

        if (numCannon < 1)
            Shoot(usethispool, cannonBarrel, isPlayerBulletPool, fx);

        if (otherCannons != null && otherCannons.Length > 0 && numCannon != 0)
        {
            int i = 1;
            foreach (OtherCannonData data in otherCannons)
            {
                if (data.isVisible && data.barrel != null)
                {
                    if (numCannon < 0 || numCannon == i)
                    {
                        fx = (canShoot || data.cantShoot == null) ? data.smoke : data.cantShoot;
                        Shoot(usethispool, data.barrel, isPlayerBulletPool, fx);
                    }
                    i++;
                }
            }
        }
    }

    public void SetOtherCannonVisible(int numCannon, bool visible)
    {
        if (otherCannons != null)
        {
            if (numCannon >= 0 && numCannon < otherCannons.Length)
            {
                OtherCannonData data = otherCannons[numCannon];
                data.isVisible = visible;
                if (data.parts != null)
                {
                    foreach (GameObject go in data.parts)
                        go.SetActive(visible);
                    data.barrel.gameObject.SetActive(visible);
                    data.smoke.gameObject.SetActive(visible);
                }
            }
        }
    }

    public int GetAvailableCannonCount()
    {
        int count = 1;
        if (otherCannons != null)
        {
            foreach (OtherCannonData data in otherCannons)
            {
                if (data.isVisible && data.barrel != null)
                {
                    count++;
                }
            }
        }
        return count;
    }

    private void Shoot(string pool, Transform tr, bool isPlayerBulletPool, ParticleSystem fx)
    {
        GameObject fireObj = poolhelper.myself.GetNextPoolItem(pool, tr.position, tr.rotation, gameObject);

        if (fireObj == null && isPlayerBulletPool)
        {
            usethispool = poolhelper.myself.fallbackPlayerBulletPoolName;
            poolhelper.myself.GetNextPoolItem(pool, tr.position, tr.rotation, gameObject);
            return;
        }

        if (fireObj != null)
        {
            //            Debug.Log("Shooting Bullet found");
            Projectile proj = fireObj.GetComponent<Projectile>();
            if (proj != null)
            {
                if (_bfd != null)
                    proj.projectile_creator = _bfd.gameObject;

                bool myBullet = poolhelper.myself.IsPlayerBulletPool(usethispool);
                bool myCannon = poolhelper.myself.IsBoatBulletPool(usethispool) && _bfd != null && _bfd.team == GameflowBase.myTeam;

                if (myBullet || myCannon)
                {
                    proj.projectile_creator = Player.myplayer.gameObject;
                    proj.id = Projectile.idCounter++;
                    if (GameflowBase.instance != null)
                        GameflowBase.IncrementPlayerStat(gamesettings.STAT_FIRE, 1);
                    // Tutorial Event
                    if (UI_Tutorial.myself != null)
                    {
                        if (gameObject.name == Player.WeaponType.Musket.ToString())
                            UI_Tutorial.myself.OnTriggerCondition(UI_Tutorial.TutoCondition.Musket);
                        if (gameObject.name == Player.WeaponType.TOG_Biggun.ToString())
                            UI_Tutorial.myself.OnTriggerCondition(UI_Tutorial.TutoCondition.Musket);
                        if (myCannon)
                            UI_Tutorial.myself.OnTriggerCondition(UI_Tutorial.TutoCondition.Canon);
                    }
                    //Debug.Log($"DEBUGHIT Fire {proj.id}");
                }

                proj.ActivateBullet(true);
                //Debug.Log($"DEBUGHIT Fire {usethispool} {initialpool} {proj.id} myBullet {myBullet} myCannon {myCannon} creator {proj.projectile_creator}");
            }
            Rigidbody rb = fireObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = cannonBarrel.forward * fireVelocity;
            }
            if (fx != null)
            {
                fx.Play();
            }
        }
    }

    public void Triggered()
    {
        _automaticStartTime = Time.time;
        _automaticBeginTime = 0f;
        FireCannon();
        if (_laser != null)
        {
            bool canShoot = true;
            if (TowerDefManager.myself != null)
                canShoot = TowerDefManager.myself.canShoot;
            _laser.Activate = canShoot;
        }
    }

    public void Released()
    {
        _automaticStartTime = 0f;
        _automaticBeginTime = 0f;
        if (_laser != null)
            _laser.Activate = false;
    }

    public void ResetGuns()
    {
        for (int i = 0; i < otherCannons.Length; ++i)
            SetOtherCannonVisible(i, false);
        _onResetGunEvents?.Invoke();
    }

}
