using System;
using System.Collections;
using UnityEngine;

public class ColliderSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SplineConfig
    {
        public SplineController spline = null;
        public AnimationCurve curve = null;
        public bool isLooping = false;
        public Transform anchor = null;
        public WaveSpawner followFirstInstance = null;
        public float weight = 1f;
        public Transform[] targets = null;
        public bool defaultValidTargets = false;

        public bool isAvailable => _isAvailable && weight > 0f;
        private bool _isAvailable = true;
        public void CheckTargets()
        {
            _isAvailable = true;
            if (targets != null && targets.Length > 0)
            {
                _isAvailable = false;
                foreach (Transform tr in targets)
                {
                    if (IsTargetAvailable(tr))
                        _isAvailable = true;
                }
            }
        }

        private bool IsTargetAvailable(Transform tr)
        {
            if (tr == null)
                return false;
            Health health = tr.GetComponent<TargetHealth>()?.targetHealth;
            if (health != null)
            {
                if (health.dead || health.currentHealth <= 0f)
                    return false;
            }
            return true;
        }
    }

    [Header("ID")]
    [SerializeField]
    private int _instanceId = 0;
    [SerializeField]
    private GameObject _prefabToSpawn = null;
    [SerializeField]
    private SplineConfig[] _splineConfigs = null;
    [Header("Spawn Minimum Delay")]
    [SerializeField]
    private float _spawnMinimumDelay = 10f;
    [Header("Spawn Count")]
    [SerializeField]
    private float _spawnFirstDelay = 0f;
    [SerializeField]
    private int _spawnCount = 10;
    [SerializeField]
    private float _spawnDelay = 1f;
    [SerializeField]
    private bool _needToDestroy = false;
    [Header("Increment First Delay")]
    [SerializeField]
    private float _firstDelayIncTime = 0;
    [SerializeField]
    private int _firstDelayIncStartCounter = 0;
    [SerializeField]
    private int _firstDelayIncCyclicCounter = 1;
    [Header("Increment Spawn Count")]
    [SerializeField]
    private int _spawnIncCount = 1;
    [SerializeField]
    private int _spawnIncStartCounter = 2;
    [SerializeField]
    private int _spawnIncCyclicCounter = 1;
    [Header("PlayerCount")]
    [SerializeField]
    private int _playerCountMin = 0;
    [SerializeField]
    private int _playerCountMax = 0;

    private GameObject[] _spawnedObjects = null;
    private Health[] _spawnedHealths = null;
    private uint _seed = 0;
    private int _counter = 0;
    private float _lastSpawnTime = 0f;

	public void TriggerSpawn()
    {
        _spawnedObjects = null;
        _spawnedHealths = null;
        bool canLaunchWave = true;
        int playerCount = GameflowBase.playerCount;
        if ((_playerCountMin > 0 && playerCount < _playerCountMin) ||
            (_playerCountMax > 0 && playerCount > _playerCountMax))
        {
            canLaunchWave = false;
        }
        if (canLaunchWave && _spawnMinimumDelay > 0f && _lastSpawnTime > 0f)
		{
            if (Time.time - _lastSpawnTime < _spawnMinimumDelay)
                canLaunchWave = false;
        }
        if (canLaunchWave)
        {
            int spawnCount = _spawnCount;
            // Increment spawn count
            if (_spawnIncCyclicCounter > 0)
            {
                int relativeWave = _counter - _spawnIncStartCounter + 1 + _spawnIncCyclicCounter;
                spawnCount += (relativeWave / _spawnIncCyclicCounter) * _spawnIncCount;
            }
            float firstDelay = _spawnFirstDelay;
            // Increment first delay
            if (_firstDelayIncCyclicCounter > 0)
            {
                int relativeWave = _counter - _firstDelayIncStartCounter + 1 + _firstDelayIncCyclicCounter;
                firstDelay += (relativeWave / _firstDelayIncCyclicCounter) * _firstDelayIncTime;
            }
            _lastSpawnTime = Time.time;
            StartCoroutine(SpawnEnum(spawnCount, _spawnDelay, firstDelay, _counter));
            _counter++;
        }
    }

    public void FinishSpawn()
    {
        if (_spawnedObjects != null)
        {
            for (int i = 0; i < _spawnedObjects.Length; ++i)
            {
                GameObject obj = _spawnedObjects[i];
                if (obj != null)
                {
                    ProjectileCannon_KDK[] canons = obj.GetComponentsInChildren<ProjectileCannon_KDK>(true);
                    foreach (ProjectileCannon_KDK canon in canons)
                    {
                        GameObject.Destroy(canon);
                    }
                    ProjectileSpawn[] projectileSpawns = obj.GetComponentsInChildren<ProjectileSpawn>(true);
                    foreach (ProjectileSpawn proj in projectileSpawns)
                    {
                        GameObject.Destroy(proj);
                    }
                }
            }
        }
    }

    public void DestroySpawn()
    {
        StopAllCoroutines();
        if (_spawnedObjects != null)
        {
            for (int i = 0; i < _spawnedObjects.Length; ++i)
            {
                GameObject obj = _spawnedObjects[i];
                if (obj != null)
                {
                    Player.myplayer.RemoveTarget(obj.transform);
                    PhotonNetworkController.DestroySoloOrMulti(obj, true);
                }
            }
        }
        _spawnedObjects = null;
        _spawnedHealths = null;
    }

    public bool AreAllDead()
    {
        if (_spawnedObjects != null)
        {
            for (int i = 0; i < _spawnedObjects.Length; ++i)
            {
                if (!IsSpawnedObjectDead(_spawnedObjects[i], _spawnedHealths[i]))
                {
                    return false;
                }
            }
            return true;
        }
        return false;
    }

    public bool NeedAllDead()
    {
        if (_spawnedObjects != null && _needToDestroy)
        {
            for (int i = 0; i < _spawnedObjects.Length; ++i)
            {
                if (!IsSpawnedObjectDead(_spawnedObjects[i], _spawnedHealths[i]))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public Vector3 GetPosFirstInstance()
    {
        if (_spawnedObjects != null && _spawnedObjects.Length > 0)
        {
            if (_spawnedObjects[0] != null)
            {
                return _spawnedObjects[0].transform.position;
            }
        }
        return Vector3.zero;
    }

    public static bool IsSpawnedObjectDead(GameObject go, Health h)
    {
        if (go == null || !go.activeSelf)
            return true;
        if (h == null || h.dead || h.currentHealth <= 0f || !h.enabled)
            return true;
        return false;
    }

    private IEnumerator SpawnEnum(int count, float delay, float firstDelay, int numwWave)
    {
        //Debug.Log($"[WAVE_SPAWNER] SpawnEnum {gameObject.name} {count} {delay} {firstDelay} {numwWave}");

        if (_splineConfigs == null || _splineConfigs.Length == 0)
            yield break;

        if (firstDelay > 0f)
            yield return new WaitForSeconds(firstDelay);

        _spawnedObjects = new GameObject[count];
        _spawnedHealths = new Health[count];

        for (int i = 0; i < count; ++i)
        {
            if (_spawnedObjects == null)
                yield break;

            float totalWeight = 0f;
            foreach (SplineConfig conf in _splineConfigs)
            {
                conf.CheckTargets();
                if (conf.isAvailable)
                    totalWeight += conf.weight;
            }

            SplineConfig config = null;
            if (totalWeight > 0f)
            {
                float random = Rnd();
                float weight = 0f;
                foreach (SplineConfig conf in _splineConfigs)
                {
                    if (conf.isAvailable)
                    {
                        weight += conf.weight;
                        if (weight / totalWeight >= random)
                        {
                            config = conf;
                            break;
                        }
                    }
                }
            }

            if (config == null)
                yield break;

            //GameObject obj = PhotonNetworkController.InstantiateSoloOrMulti(true, _prefabToSpawn.name, Vector3.zero, Quaternion.identity);
            GameObject obj = GameObject.Instantiate<GameObject>(_prefabToSpawn);
            obj.transform.SetParent(transform);
            Health[] healths = obj.GetComponentsInChildren<Health>(true);
            if (healths.Length == 0)
            {
                Debug.LogError("No health on spawned enemy!");
                yield break;
            }
            Health health = healths[0];
            health.onDieCallback += OnEnemyDie;
            _seed = (uint)((numwWave + 1) * 100000000 + _instanceId * 1000000 + i * 1000);
            for (int h = 0; h < healths.Length; ++h)
            {
                healths[h].SetInstanceId((int)_seed + h * 100);
            }

#if USE_KDK
            Player.myplayer.AddTarget(obj.transform);
#endif

            if (config.spline != null)
            {
                SplineFollower follow = obj.GetAddComponent<SplineFollower>();
                follow.SetupNewSpline(config.spline, config.isLooping, true, config.curve);
                if (!config.isLooping)
                {
                    follow.AddOnPathEvent(OnPathEventCbk);
                }
            }
            else if (config.anchor != null)
            {
                obj.transform.position = config.anchor.position;
                obj.transform.rotation = config.anchor.rotation;
                obj.transform.localScale = Vector3.one;
            }
            else if (config.followFirstInstance != null)
            {
                obj.transform.position = config.followFirstInstance.GetPosFirstInstance();
                obj.transform.rotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;
            }

            if (config.targets != null && config.targets.Length > 0)
            {
                ProjectileCannon_KDK[] canons = obj.GetComponentsInChildren<ProjectileCannon_KDK>(true);
                foreach (ProjectileCannon_KDK canon in canons)
                {
                    canon.Init(config.targets, config.defaultValidTargets);
                }
            }

            _spawnedObjects[i] = obj;
            _spawnedHealths[i] = health;

            yield return new WaitForSeconds(delay);
        }
    }

    private void OnPathEventCbk(SplineFollower path, SplineFollower.PathEvent pathEvent, object data)
    {
        if (pathEvent == SplineFollower.PathEvent.PathEnded)
        {
            path.RemoveOnPathEvent(OnPathEventCbk);
            Health h = path.gameObject.GetComponentInChildren<Health>(true);
            if (h != null)
                h.Die();
        }
    }

    private void OnEnemyDie(Health h)
    {
        for (int i = 0; i < _spawnedHealths.Length; ++i)
        {
            if (_spawnedHealths[i] == h)
            {
                h.onDieCallback = null;
                _spawnedHealths[i] = null;
                GameObject obj = _spawnedObjects[i];
                if (obj != null)
                {
                    Player.myplayer.RemoveTarget(obj.transform);
                }
                return;
            }
        }
    }

    public float Rnd()
    {
        _seed = (uint)(SetRnd() * int.MaxValue);
        return SetRnd();
    }

    private float SetRnd()
    {
        _seed = (uint)(((double)_seed * 16807) % int.MaxValue);
        return (float)(((double)_seed / (double)0x7FFFFFFF) + 0.000000000233);
    }
}
