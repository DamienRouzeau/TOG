using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ThemePrefabArray : RREnumArray<multiplayerlobby.SkinTheme, GameObject> { }

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
        public bool arePlayersTargets = false;
        public bool defaultValidTargets = false;
        public bool dontFireThroughWalls = false;
        public bool addEnemyTarget = true;

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
                    {
                        _isAvailable = true;
                        return;
                    }
                }
			}
            if (arePlayersTargets)
			{
                _isAvailable = false;
                if (IsTargetAvailable(Player.myplayer.transform))
                {
                    _isAvailable = true;
                    return;
                }
                foreach (Player_avatar avatar in Player.myplayer.avatars)
				{
                    if (avatar.actornumber >= 0 && IsTargetAvailable(avatar.transform))
                    {
                        _isAvailable = true;
                        return;
                    }
                }
            }
        }

        private bool IsTargetAvailable(Transform tr)
		{
            if (tr == null)
                return false;
            Health health = tr.GetComponent<Health>();
            if (health == null)
                health = tr.GetComponent<TargetHealth>()?.targetHealth;
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
    private ThemePrefabArray _themePrefabs = null;
    [SerializeField]
    private SplineConfig[] _splineConfigs = null;
    [Header("Spawn Count")]
    [SerializeField]
    private float _spawnFirstDelay = 0f;
    [SerializeField]
    private int _spawnCount = 10;
    [SerializeField]
    private float _spawnDelay = 1f;
    [SerializeField]
    private bool _needToDestroy = false;
    [SerializeField]
    private bool _needToFinishWave = true;
    [SerializeField]
    private bool _speedUpAtFinishWave = true;
    [Header("Increment First Delay")]
    [SerializeField]
    private float _firstDelayIncTime = 0;
    [SerializeField]
    private int _firstDelayIncStartWave = 0;
    [SerializeField]
    private int _firstDelayIncCyclicWave = 1;
    [Header("Increment Spawn Count")]
    [SerializeField]
    private int _spawnIncCount = 1;
    [SerializeField]
    private int _spawnIncStartWave = 2;
    [SerializeField]
    private int _spawnIncCyclicWave = 1;
    [Header("Available waves")]
    [SerializeField]
    private List<int> _availableWaves = null;
    [SerializeField]
    private int _cyclicWaves = 0;
    [SerializeField]
    private int _numWaveMin = 0;
    [SerializeField]
    private int _numWaveMax = 0;
    [Header("PlayerCount")]
    [SerializeField]
    private int _playerCountMin = 0;
    [SerializeField]
    private int _playerCountMax = 0;
    [Header("Repeat Wave")]
    [SerializeField]
    private bool _repeatWave = false;
    [SerializeField]
    private float _repeatDelay = 0f;
    [Header("Check Conditions")]
    [SerializeField]
    private CheckConditionsEvent _checkConditionsToFire = null;
    [SerializeField]
    private CheckConditionsEvent _checkConditionsToStartWave = null;
    [SerializeField]
    private CheckConditionsEvent _checkConditionsToDelayWave = null;
    [SerializeField]
    private CheckConditionsEvent _checkConditionsToSpawnEnemy = null;

    public bool waveStarted => _waveStarted;

    private GameObject[] _spawnedObjects = null;
    private Health[] _spawnedHealths = null;
    private uint _seed = 0;
    private bool _waveStarted = false;

	public void StartWave(int numWave)
	{
        _spawnedObjects = null;
        _spawnedHealths = null;
        _waveStarted = false;
        bool canLaunchWave = (_numWaveMin == 0 || numWave >= _numWaveMin - 1) && (_numWaveMax == 0 || numWave <= _numWaveMax - 1);
        if (canLaunchWave && _availableWaves != null && _availableWaves.Count > 0)
		{
            if (_cyclicWaves > 0)
                numWave %= _cyclicWaves;
            canLaunchWave = _availableWaves.Contains(numWave + 1);
        }
        if (canLaunchWave)
		{
            int playerCount = GameflowBase.playerCount;
            if ((_playerCountMin > 0 && playerCount < _playerCountMin) ||
                (_playerCountMax > 0 && playerCount > _playerCountMax))
            {
                canLaunchWave = false;
            }
        }
        if (canLaunchWave && _checkConditionsToStartWave != null)
		{
            if (!_checkConditionsToStartWave.AreConditionsValid())
                canLaunchWave = false;
		}
        if (canLaunchWave)
        {
            int spawnCount = _spawnCount;
            // Increment spawn count
            if (_spawnIncCyclicWave > 0)
			{
                int relativeWave = numWave - _spawnIncStartWave + 1 + _spawnIncCyclicWave;
                spawnCount += (relativeWave / _spawnIncCyclicWave) * _spawnIncCount;
            }
            float firstDelay = _spawnFirstDelay;
            // Increment first delay
            if (_firstDelayIncCyclicWave > 0)
			{
                int relativeWave = numWave - _firstDelayIncStartWave + 1 + _firstDelayIncCyclicWave;
                firstDelay += (relativeWave / _firstDelayIncCyclicWave) * _firstDelayIncTime;
            }
            _waveStarted = true;
            StartCoroutine(SpawnEnum(spawnCount, _spawnDelay, firstDelay, numWave));
        }
	}

    public void FinishWave()
	{
        if (_spawnedObjects != null)
        {
            for (int i = 0; i < _spawnedObjects.Length; ++i)
            {
                GameObject obj = _spawnedObjects[i];
                if (obj != null)
                {
                    if (_speedUpAtFinishWave)
                    {
                        SplineFollower follow = obj.GetComponent<SplineFollower>();
                        if (follow != null)
                        {
                            follow.SetLoop(false);
                            follow.SetSpeed(1f);
                        }
                    }
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

    public void KillAllEnemies()
	{
        if (_spawnedObjects != null && _needToDestroy)
        {
            for (int i = 0; i < _spawnedObjects.Length; ++i)
            {
                if (!IsSpawnedObjectDead(_spawnedObjects[i], _spawnedHealths[i]))
                {
                    _spawnedHealths[i].Die();
                }
            }
        }
    }

    public int ComputeEnemies()
    {
        if (_spawnedObjects != null)
        {
            return _spawnedObjects.Length;
        }
        return 0;
    }

    public void DestroyWave(bool stopCoroutines = true)
	{
        if (stopCoroutines)
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

    public bool AreAllDead(bool checkNeedToFinishWave = true)
	{
        if (checkNeedToFinishWave && !_needToFinishWave)
            return true;
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
        {
            //Debug.Log("IsSpawnedObjectDead go at null or !activeSelf");
            return true;
        }
        if (h == null)
        {
            //Debug.Log("IsSpawnedObjectDead h at null");
            return true;
        }
        if (h.isDying)
        {
            //Debug.Log("IsSpawnedObjectDead isDying");
            return false;
        }
        if (h.dead || h.currentHealth <= 0f || !h.enabled)
        {
            //Debug.Log("IsSpawnedObjectDead h dead or currentHealth<0 or disabled");
            return true;
        }
        return false;
	}

    private IEnumerator SpawnEnum(int count, float delay, float firstDelay, int numwWave)
	{
        //Debug.Log($"[WAVE_SPAWNER] SpawnEnum {gameObject.name} {count} {delay} {firstDelay} {numwWave}");

        if(_splineConfigs == null || _splineConfigs.Length == 0)
            yield break;

        if (firstDelay > 0f)
            yield return new WaitForSeconds(firstDelay);

        if (_checkConditionsToDelayWave != null)
		{
            while (!_checkConditionsToDelayWave.AreConditionsValid())
                yield return new WaitForSeconds(0.5f);
        }

        while (true)
        {
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

                if (_checkConditionsToSpawnEnemy != null)
                {
                    if (!_checkConditionsToSpawnEnemy.AreConditionsValid())
                    {
                        yield return new WaitForSeconds(delay);
                        continue;
                    }
                }

                GameObject prefab = _prefabToSpawn;
                if (multiplayerlobby.theme != multiplayerlobby.SkinTheme.Normal)
                {
                    GameObject goTheme = _themePrefabs[multiplayerlobby.theme];
                    if (goTheme != null)
                        prefab = goTheme;
                }

                //GameObject obj = PhotonNetworkController.InstantiateSoloOrMulti(true, prefab.name, Vector3.zero, Quaternion.identity);
                GameObject obj = GameObject.Instantiate<GameObject>(prefab);
                obj.transform.SetParent(transform);
                Health[] healths = obj.GetComponentsInChildren<Health>(true);
                if (healths.Length == 0)
                {
                    Debug.LogError("No health on spawned enemy!");
                    yield break;
                }
                Health health = healths[0];
                if (healths.Length > 1)
				{
                    foreach (Health h in healths)
                    {
                        if (h.mainHealth)
                            health = h;
                    }
				}
                health.onDieCallback += OnEnemyDie;
                _seed = (uint)((numwWave + 1) * 100000000 + _instanceId * 1000000 + i * 1000);
                for (int h = 0; h < healths.Length; ++h)
                {
                    healths[h].SetInstanceId((int)_seed + h * 100);
                }

                if (config.addEnemyTarget)
                    Player.myplayer.AddTarget(obj.transform);

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

                List<Transform> targets = new List<Transform>();
                if (config.targets != null && config.targets.Length > 0)
                {
                    targets.AddRange(config.targets);
                }
                if (config.arePlayersTargets)
                {
                    List<Player.IPlayer> players = new List<Player.IPlayer>();
                    players.Add(Player.myplayer);
                    foreach (Player_avatar avatar in Player.myplayer.avatars)
                    {
                        if (avatar.actornumber >= 0)
                        {
                            players.Add(avatar);
                        }
                    }

                    players.Sort((a, b) => a.id.CompareTo(b.id));

                    foreach (var player in players)
                        AddTargetOnPlayer(targets, player.goRoot.GetComponentInChildren<PlayerBody>());
                }

                if (targets.Count > 0)
                {
                    ProjectileCannon_KDK[] canons = obj.GetComponentsInChildren<ProjectileCannon_KDK>(true);
                    foreach (ProjectileCannon_KDK canon in canons)
                    {
                        canon.Init(targets.ToArray(), config.defaultValidTargets, _checkConditionsToFire, config.dontFireThroughWalls);
                    }
                }

                _spawnedObjects[i] = obj;
                _spawnedHealths[i] = health;

                yield return new WaitForSeconds(delay);
            }
            if (!_repeatWave)
                break;

            yield return new WaitForSeconds(_repeatDelay);
            while (!AreAllDead(false))
                yield return new WaitForSeconds(_repeatDelay);
            DestroyWave(false);
        }
    }

    private void AddTargetOnPlayer(List<Transform> targets, PlayerBody playerBody)
	{
        if (playerBody != null)
        {
            targets.Add(playerBody.transform);
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
