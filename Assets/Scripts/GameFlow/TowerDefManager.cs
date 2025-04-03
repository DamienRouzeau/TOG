//#define SHOW_ACCURACY

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RRLib;
using UnityEngine.Events;

public class TowerDefManager : MonoBehaviour
{
	#region Classes

    [System.Serializable]
    public class WaveCheckConditions
    {
        public int numWave;
        public CheckConditionsEvent conditions;
    }

    [System.Serializable]
    public class StartWaveEvent
    {
        public int numWave;
        public UnityEvent actions;
        public List<GameObject> goListToActivate;
        public Vector3 playerPos;
    }

    [System.Serializable]
    public class TimerDisplayInfo
    {
        public enum TimerType
		{
            TIME_FROM_START,
            TIME_FROM_FIRST_WAVE,
            TIME_FROM_WAVE,
            COUNTDOWN
        }

        public int numWave;
        public TimerType timerType;
        public float duration;
    }

    #endregion

    #region Enums

    public enum TowerDefState
	{
        NONE,
        INTRO,
        WAVE,
        RECOVERY,
        OUTRO,
        GAMEOVER,
        END,
        FALLBACK,
        CONTINUE,
        TIMEOVER
    }

    #endregion

    #region Delegates
    public delegate void OnTowerDefState(TowerDefState state, int numWave, int relativeWave, int level);
    public delegate void OnLifeRegenState(float oldVal, float newVal, float maxVam);
    #endregion

    #region Properties

    [System.Serializable]
    public class BriefingThemePrefabArray : RREnumArray<multiplayerlobby.SkinTheme, GameObject> { }

    public static TowerDefManager myself = null;
    public static OnTowerDefState onTowerDefState = null;
    public static OnLifeRegenState onLifeRegenState = null;
    public static bool isInvincible = false;
    public static bool keepGoldScoreAndGuns = false;
    public static bool keepLastWaveOnContinue = true;
    public static int maxGunsUpgradeOnIntro = 3;

    public AudioSource gamemusic => _gamemusic;
    public GameObject worldRoot => _worldRoot;
    public UpgradeKDKSettings upgradeSettings => _upgradeSettings;
    public int currentWave => _currentWave;
    public int relativeWave => _relativeWave;
    public TowerDefState state => _state;
    public float regeneratorLife => _regeneratorLife;
    public int nextRaceMinWaveToFinish => _nextRaceMinWaveToFinish;
    public int nextRaceMaxWaveToFinish => _nextRaceMaxWaveToFinish;
    public float nextRaceMaxGameTime => _nextRaceMaxGameTime;
    public float nextRaceMinRemainingTime => _nextRaceMinRemainingTime;

    public float gameTime => _finishTime > 0 ? _finishTime - _startTime : Time.time - _startTime;

    public bool canShoot => _state != TowerDefState.END && _state != TowerDefState.GAMEOVER && _state != TowerDefState.OUTRO && _state != TowerDefState.CONTINUE;
    public bool canShowMiniMap => _state == TowerDefState.INTRO || _state == TowerDefState.WAVE || _state == TowerDefState.FALLBACK || _state == TowerDefState.RECOVERY;

    public bool isShortVersion => _shortVersion;

    [SerializeField] private AudioSource _gamemusic = null;
    [SerializeField] private GameObject _worldRoot = null;
    [SerializeField] private float _gameDuration = 720f;
    [SerializeField] private float _introDuration = 30f;
    [SerializeField] private float _waveDuration = 120f;
    [SerializeField] private float _waveIncDuration = 5f;
    [SerializeField] private float _fallbackMinDuration = 1f;
    [SerializeField] private float _fallbackMaxDuration = 2f;
    [SerializeField] private float _recoveryDuration = 30f;
    [SerializeField] private float _outroDuration = 30f;
    [SerializeField] private float _gameOverDuration = 10f;
    [SerializeField] private GameObject _waveSpawnersRoot = null;
    [SerializeField] private Health[] _healthsToDefend = null;
    [SerializeField] private Transform _endScreenResultRoot = null;
    [SerializeField] private UpgradeKDKSettings _upgradeSettings = null;
    [SerializeField] private SpawnpointReferences _startSpawnpoints = null;
    [SerializeField] private SpawnpointReferences _endScreenSpawnpoints = null;
    [SerializeField] private TargetRegenerator[] _regenerators = null;
    [Header("Wave conditions")]
    [SerializeField] private List<WaveCheckConditions> _waveConditions = null;
    [Header("Next Race")]
    [SerializeField] private int _nextRaceMinWaveToFinish = 11;
    [SerializeField] private int _nextRaceMaxWaveToFinish = 17;
    [SerializeField] private float _nextRaceMaxGameTime = 900f;
    [SerializeField] private float _nextRaceMinRemainingTime = 90f;
    [Header("Briefing")]
    [SerializeField] private GameObject _briefingPrefab = null;
    [SerializeField]
    private BriefingThemePrefabArray _briefingThemePrefabs = null;
    [Header("StarShip position")]
    [SerializeField] private SplineFollower _starShipSplineFollower = null;
    [SerializeField] private int _starShipMaxPosAtWave = 8;
    [SerializeField] private float _starShipMaxPosAtRatio = 0.9f;
    [Header("Regenerator")]
    [SerializeField] private float _regeneratorLifeMax = 10000f;
    [SerializeField] private GaugeRegenerator _regenerator = null;
    [SerializeField] private GameObject _regeneratorSystem = null;
    [SerializeField] private int _regeneratorStartWave = 6;
    [Header("Timer Display")]
    [SerializeField] private List<TimerDisplayInfo> _timerDisplayList;
    [Header("Events")]
    [SerializeField] private UnityEvent _onTimeOverEvent;
    [SerializeField] private UnityEvent _onOutroEvent;
    [SerializeField] private UnityEvent _onEndEvent;
    [Header("Options TEST")]
    [SerializeField] private int _startWave = 0;
    [SerializeField] private bool _testSameWave = false;
    [SerializeField] private List<StartWaveEvent> _startWaveEvents = null;
    [Header("End Screen TEST")]
    [SerializeField] private bool _testEndScreen = false;
    [SerializeField] private int _testPlayerCount = 8;

    private WaveSpawner[] _waveSpawners = null;
    private float _startTime = 0f;
    private float _stateTime = 0f;
    private float _waveTime = 0f;
    private float _firstWaveTime = 0f;
    private float _finishTime = 0f;
    private float _countdownTime = 0f;
    private TowerDefState _state = TowerDefState.NONE;
    private int _currentWave = 0;
    private int _relativeWave = 0;
    private float _checkHealthsTime = 0f;
    private float _checkEndWaveTime = 0f;
    private float _fallbackDuration = 0f;
    private float _recoveryExtraDuration = 0f;
    private int _currentUpgradeByGold = 0;
    private float _maxLife = 0;
    private UI_EndRaceResult _endResultScreen = null;
    private bool _endOfTimeReached = false;
    private bool _noUpgradeFirstTime = false;
    private float _startWaveDuration = 0f;
    private float _regeneratorLife = 0f;
    private TargetRecovery[] targetRecoveries = null;
    private bool _updateTimer = false;
    private TimerDisplayInfo.TimerType _currentTimerType;
    private float _currentTimerDuration = 0f;
    private bool _shortVersion = false;

    #endregion

    private void Awake()
    {
        myself = this;
        GameflowBase.CheckRandomSeed();
        Health.ResetInstanceCount();
        Random.InitState(GameflowBase.randomSeed);
        _startWaveDuration = _waveDuration;
        int count = _healthsToDefend != null ? _healthsToDefend.Length : 0;
        targetRecoveries = new TargetRecovery[count];
        for (int i = 0; i < count; ++i)
		{
            targetRecoveries[i] = _healthsToDefend[i].GetComponent<TargetRecovery>();
        }
    }

    private IEnumerator Start()
    {
        while (!GameLoader.myself.isInit)
            yield return null;

        while (!Player.myplayer.isInit)
            yield return null;

        while (GameflowBase.instance == null)
            yield return null;

        yield return new WaitForSeconds(5f);

        while (!GameflowBase.areAllRacesStarted)
            yield return null;

        GameflowBase.instance.SetTowerDefReady();

        while (!GameflowBase.instance.AreAllTowerDefReady())
            yield return null;

        _startTime = Time.time;

        Player.myplayer.CreateMinimap();

#if UNITY_EDITOR
        if (multiplayerlobby.startWave > 0)
        {
            _currentWave = multiplayerlobby.startWave;
            _startWave = _currentWave;
            _relativeWave = 0;
            CallStartWaveEvent(true);
        }
        else if (_startWave > 0)
        {
            _currentWave = _startWave - 1;
            _startWave = _currentWave;
            _relativeWave = 0;
            CallStartWaveEvent(true);
        }
        if (_testEndScreen)
		{
            SetState(TowerDefState.OUTRO);
            _outroDuration = 5f;
            yield break;
		}
#else
        _startWave = 0;
        if (multiplayerlobby.startWave > 0)
		{
            _currentWave = multiplayerlobby.startWave;
            _startWave = _currentWave;
            _relativeWave = 0;
            CallStartWaveEvent();
        }
#endif

        GameObject briefingPrefab = _briefingPrefab;
        if (multiplayerlobby.theme != multiplayerlobby.SkinTheme.Normal)
		{
            GameObject goBriefing = _briefingThemePrefabs[multiplayerlobby.theme];
            if (goBriefing != null)
                briefingPrefab = goBriefing;
        }
        if (briefingPrefab != null)
		{
            GameObject briefing = Instantiate<GameObject>(briefingPrefab);
            briefing.transform.SetParent(Player.myplayer.uiCanvas.transform);
            briefing.transform.localPosition = Vector3.zero;
            briefing.transform.localRotation = Quaternion.identity;
            briefing.transform.localScale = Vector3.one;
        }

        if (_recoveryDuration < 1f)
            _recoveryDuration = 1f;

        SetState(TowerDefState.INTRO);
    }

    private void OnDestroy()
    {
        Health.ResetInstanceCount();
        myself = null;
    }

    public void SetGameDuration(float duration)
	{
        _gameDuration = duration;
        multiplayerlobby.endlessDuration = (int)_gameDuration;
    }

    private void OnIntro()
    {
        Debug.Log("[TOWERDEF] OnIntro");
        _waveSpawners = _waveSpawnersRoot.GetComponentsInChildren<WaveSpawner>();
        // Compute max life
        _maxLife = 0f;
        if (_healthsToDefend != null && _healthsToDefend.Length > 0)
        {
            for (int i = 0; i < _healthsToDefend.Length; ++i)
            {
                Health h = _healthsToDefend[i];
                if (h != null)
                {
                    _maxLife += h.startingHealth;
                    Player.myplayer.antennaLifes[i].SetAntennaId(i + 1);
                    Player.myplayer.antennaLifes[i].SetLifePercent(100);
                }
            }
        }


        if (keepGoldScoreAndGuns)
		{
            if (Player.myplayer.currentLeftGunUpgrade > maxGunsUpgradeOnIntro)
			{
                Player.myplayer.ResetGun(true);
                for (int i = 0; i < maxGunsUpgradeOnIntro; ++i)
                    Player.myplayer.UpgradeGun(true, i, 0);
            }

            if (Player.myplayer.currentRightGunUpgrade > maxGunsUpgradeOnIntro)
            {
                Player.myplayer.ResetGun(false);
                for (int i = 0; i < maxGunsUpgradeOnIntro; ++i)
                    Player.myplayer.UpgradeGun(false, i, 0);
            }
        }
        else
        {
            Player.myplayer.ResetGoldAndScore();
            Player.myplayer.ResetGuns();
            _currentUpgradeByGold = 0;
        }
        
        SetAntennasLevel(1, false, true);
        if (multiplayerlobby.gameMode == multiplayerlobby.GameMode.Endless && multiplayerlobby.endlessDuration > 0)
        {
            _gameDuration = multiplayerlobby.endlessDuration;
            Debug.Log("[TOWERDEF] Set GameDuration by mode " + _gameDuration);
        }
        else
		{
            _gameDuration = 3600f;
            multiplayerlobby.endlessDuration = (int)_gameDuration;
            Debug.Log("[TOWERDEF] Set GameDuration by default " + _gameDuration);
        }
        _noUpgradeFirstTime = true;

#if USE_BOD
        _shortVersion = _gameDuration < 3600f;
        if (_shortVersion)
        {
            Debug.Log("[TOWERDEF] Short Version Detected!");
            _currentWave = 2;
            _startWave = _currentWave;
            _relativeWave = 0;
            CallStartWaveEvent();
        }
#endif

        InitAtWave(_currentWave);
    }

    private void InitAtWave(int wave)
	{
        float antennasLifeRatio = wave == 0 ? 1f : 0.75f;
        float antennasRecoveryBySecond = 0f;
        int antennasLevel = 1;

        List<UpgradeKDKSettings.UpgradeDataByWave> upWaves = _upgradeSettings.GetUpgradesUntilWave(wave);
        if (upWaves != null)
        {
            foreach (UpgradeKDKSettings.UpgradeDataByWave upWave in upWaves)
            {
                if (upWave.upType == UpgradeKDKSettings.UpgradeType.AntennaRecovery)
                {
                    antennasLevel = upWave.level;
                    antennasRecoveryBySecond = upWave.value;
                }
                else
				{
                    if (upWave.upType == UpgradeKDKSettings.UpgradeType.LeftGun)
                        Player.myplayer.UpgradeGun(true, upWave.level, 0);
                    else if (upWave.upType == UpgradeKDKSettings.UpgradeType.RightGun)
                        Player.myplayer.UpgradeGun(false, upWave.level, 0);
                }
            }
        }

        for (int i = 0; i < _healthsToDefend.Length; ++i)
        {
            Health h = _healthsToDefend[i];
            if (h != null)
            {
                h.GetComponent<Animator>().Rebind();
                h.dead = false;
                h.ForceCurrentHealth(h.maxHealth * antennasLifeRatio);
                TargetRecovery targetRecov = h.GetComponent<TargetRecovery>();
                if (targetRecov != null)
                {
                    if (antennasRecoveryBySecond > 0f)
                        targetRecov.SetRecoveryBySecond(antennasRecoveryBySecond);
                    else
                        targetRecov.ResetRecoveryBySecond();
                }
                Player.myplayer.antennaLifes[i].SetLifePercent(Mathf.RoundToInt(antennasLifeRatio * 100f));
            }
        }

        SetAntennasLevel(antennasLevel);

        if (!keepGoldScoreAndGuns)
        {
            UpgradeKDKSettings.UpgradeDataByGold[] upGolds = _upgradeSettings.GetUpgradesByGoldUntilWave(wave);
            if (upGolds != null)
            {
                _currentUpgradeByGold = 0;
                foreach (UpgradeKDKSettings.UpgradeDataByGold upGold in upGolds)
                {
                    if (upGold.upType == UpgradeKDKSettings.UpgradeType.LeftGun)
                        Player.myplayer.UpgradeGun(true, (int)upGold.value, 0);
                    else if (upGold.upType == UpgradeKDKSettings.UpgradeType.RightGun)
                        Player.myplayer.UpgradeGun(false, (int)upGold.value, 0);
                    _currentUpgradeByGold++;
                }
            }
        }

        if (_starShipSplineFollower != null)
		{
            _starShipSplineFollower.startValue = Mathf.Clamp01((float)_currentWave / (float)_starShipMaxPosAtWave) * _starShipMaxPosAtRatio;
            _starShipSplineFollower.Setup(false);
        }

#if USE_KDK
        SetRegeneratorLifeToMax();
        _regeneratorSystem.SetActive(wave >= _regeneratorStartWave - 1);
#else
        _regeneratorSystem.SetActive(false);
#endif

        GameflowBase.instance?.ResetStats();
    }

    private void OnWave()
	{
#if USE_KDK
        _regeneratorSystem.SetActive(_currentWave >= _regeneratorStartWave - 1);
#endif
        _waveDuration = _startWaveDuration + _waveIncDuration * _currentWave;
        Debug.Log("[TOWERDEF] OnWave num " + _currentWave + " duration " + _waveDuration);
        if (_waveSpawners != null)
        {
            foreach (WaveSpawner spawner in _waveSpawners)
			{
                spawner.DestroyWave();
                spawner.StartWave(_currentWave);
			}
        }

        _waveTime = Time.time;
        if (_startWave == _currentWave)
            _firstWaveTime = _waveTime;
        _updateTimer = false;
        if (_timerDisplayList != null)
		{
            foreach (TimerDisplayInfo timer in _timerDisplayList)
			{
                if (timer.numWave - 1 == _currentWave || timer.numWave - 1 <= _startWave)
				{
                    _updateTimer = true;
                    _currentTimerType = timer.timerType;
                    _currentTimerDuration = timer.duration;
                    switch (_currentTimerType)
					{
                        case TimerDisplayInfo.TimerType.COUNTDOWN:
                            _countdownTime = _waveTime;
                            break;
                    }
                }
			}
		}
        _fallbackDuration = Random.Range(_fallbackMinDuration, _fallbackMaxDuration); 
    }

    private void OnFallback()
    {
        Debug.Log("[TOWERDEF] OnFallback wave " + _currentWave);
        if (_waveSpawners != null)
        {
            foreach (WaveSpawner spawner in _waveSpawners)
            {
                spawner.FinishWave();
            }
        }
    }

    private void OnRecovery()
	{
        Debug.Log("[TOWERDEF] OnRecovery wave " + _currentWave);
        StartCoroutine(UpdateUpgrades());
        if (_testSameWave)
            return;
        _currentWave++;
    }

    private void OnOutro()
	{
        Debug.Log("[TOWERDEF] OnOutro");
        _onOutroEvent?.Invoke();

        if (_waveSpawners != null)
        {
            foreach (WaveSpawner spawner in _waveSpawners)
            {
                spawner.FinishWave();
            }
        }
        keepGoldScoreAndGuns = true;
        maxGunsUpgradeOnIntro = 1;
        _finishTime = Time.time;
    }

    private void OnGameOver()
	{
        Debug.Log("[TOWERDEF] OnGameOver");
        if (_waveSpawners != null)
        {
            foreach (WaveSpawner spawner in _waveSpawners)
            {
                spawner.FinishWave();
            }
        }
        _finishTime = Time.time;
    }

    private void OnEnd()
	{
        Debug.Log("[TOWERDEF] OnEnd");
        _onEndEvent?.Invoke();

        if (_waveSpawners != null)
        {
            foreach (WaveSpawner spawner in _waveSpawners)
            {
                spawner.DestroyWave();
            }
        }
#if UNITY_EDITOR
        if (_testEndScreen)
		{
            for (int i = 1; i < _testPlayerCount; ++i)
                GameflowBase.teamlistA[i] = GameflowBase.myId;
		}
#endif
        UI_EndRaceResult endRacePrefab = gamesettings.myself.endRaceResultPrefabOneTeam;
        _endResultScreen = GameObject.Instantiate<UI_EndRaceResult>(endRacePrefab, _endScreenResultRoot);
        _endResultScreen.transform.localPosition = Vector3.zero;
        _endResultScreen.transform.localScale = Vector3.one;
        _endResultScreen.transform.localRotation = Quaternion.identity;
        _endResultScreen.SetResult(_endOfTimeReached);
        if (GameflowKDK.myself != null)
        {
            GameflowKDK.myself.gameState = GameflowKDK.GameState.EndGame;
            GameflowKDK.levelToLaunch = null;
            GameflowKDK.gameplayEndRace = true;
        }
        if (GameflowBOD.myself != null)
        {
            GameflowBOD.myself.gameState = GameflowBOD.GameState.EndGame;
            GameflowBOD.levelToLaunch = null;
            GameflowBOD.gameplayEndRace = true;
        }
        multiplayerlobby.endlessDuration -= (int)(Time.time - _startTime);

        if (_endScreenSpawnpoints != null)
            _endScreenSpawnpoints.TeleportPlayerWithFadeIn();
    }

    private void OnContinue()
	{
        if (_endResultScreen != null)
        {
            GameObject.Destroy(_endResultScreen.gameObject);
        }
        Random.InitState(GameflowBase.randomSeed);
        // Reset Gun upgrades
        if (!keepGoldScoreAndGuns)
        {
            Player.myplayer.ResetGoldAndScore();
            Player.myplayer.ResetGuns();
        }
        _currentUpgradeByGold = 0;
        if (GameflowKDK.myself != null)
        {
            GameflowKDK.myself.gameState = GameflowKDK.GameState.TowerDef;
            GameflowKDK.levelToLaunch = null;
            GameflowKDK.gameplayEndRace = false;
        }
        if (GameflowBOD.myself != null)
        {
            GameflowBOD.myself.gameState = GameflowBOD.GameState.TowerDef;
            GameflowBOD.levelToLaunch = null;
            GameflowBOD.gameplayEndRace = false;
        }
        apicalls.myself?.StartGameCounter();
        _gameDuration = multiplayerlobby.endlessDuration;
        Debug.Log($"[TOWERDEF] CONTINUE _gameDuration {_gameDuration}");
        _startWave = _currentWave;
        _startTime = Time.time;
        InitAtWave(_currentWave);
        if (_startSpawnpoints != null)
            _startSpawnpoints.TeleportPlayerWithFadeIn();
        _noUpgradeFirstTime = true;
    }

    private void OnTimeOver()
	{
        Debug.Log("[TOWERDEF] OnTimeOver");
        _onTimeOverEvent?.Invoke();
    }

    public void SetRegeneratorLifeToMax()
    {
        UpdateGeneratorLife(_regeneratorLifeMax);
    }

    public void UseRegeneratorLife(float life)
	{
        UpdateGeneratorLife(_regeneratorLife - life);
    }

    public void UpdateGeneratorLife(float life)
	{
        onLifeRegenState?.Invoke(_regeneratorLife, life, _regeneratorLifeMax);
        _regeneratorLife = life;
        if (_regenerator != null)
		{
            if (life <= 0f)
            {
                _regenerator.SetEmptyAnim();
                _regeneratorLife = 0f;
            }
            else
			{
                float ratio = life / _regeneratorLifeMax;
                if (ratio == 1f)
                    _regenerator.SetFullAnim();
                else
                    _regenerator.SetRatio(ratio);
			}
        }
	}

    public void SetStateEvent(TowerDefState state, int numWave, int relativeWave)
	{
        Debug.Log($"[TOWERDEF] SetStateEvent {state} {numWave} {relativeWave} {GameflowBase.myId}");
        _currentWave = numWave;
        _relativeWave = relativeWave;
        _startWave = _currentWave - relativeWave;
        SetState(state, true);
	}

    private void SetState(TowerDefState state, bool force = false)
    {
        if (force || PhotonNetworkController.IsMaster())
        {
            _state = state;
            _relativeWave = _currentWave - _startWave;
            onTowerDefState?.Invoke(_state, _currentWave, _relativeWave, multiplayerlobby.startLevel);
            Debug.Log($"[TOWERDEF] SetState {state} {_currentWave} {_relativeWave} {GameflowBase.myId}");
            _stateTime = Time.time;
            CheckHealths();
#if SHOW_ACCURACY
            UpdateAccuracy();
#endif
            switch (_state)
            {
                case TowerDefState.INTRO:
                    OnIntro();
                    break;
                case TowerDefState.WAVE:
                    OnWave();
                    break;
                case TowerDefState.FALLBACK:
                    OnFallback();
                    break;
                case TowerDefState.RECOVERY:
                    OnRecovery();
                    break;
                case TowerDefState.OUTRO:
                    OnOutro();
                    break;
                case TowerDefState.GAMEOVER:
                    OnGameOver();
                    break;
                case TowerDefState.END:
                    OnEnd();
                    break;
                case TowerDefState.CONTINUE:
                    OnContinue();
                    break;
                case TowerDefState.TIMEOVER:
                    OnTimeOver();
                    break;
            }
        }
    }

    private void Update()
	{
        float time = Time.time;
        if (PhotonNetworkController.IsMaster())
        {   
            switch (_state)
            {
                case TowerDefState.INTRO:
                    if (time - _stateTime > _introDuration)
                    {
                        SetState(TowerDefState.WAVE);
                    }
                    break;
                case TowerDefState.WAVE:
                    if (CheckEndWave(time))
                    {
                        SetState(TowerDefState.FALLBACK);
                    }
                    else
                    {
                        CheckEndGame(time);
                    }
                    break;
                case TowerDefState.FALLBACK:
                    if (time - _stateTime > _fallbackDuration)
                    {
                        SetState(TowerDefState.RECOVERY);
                    }
                    else
                    {
                        CheckEndGame(time, true);
                    }
                    break;
                case TowerDefState.RECOVERY:
                    if (time - _stateTime > _recoveryDuration + _recoveryExtraDuration)
                    {
                        SetState(TowerDefState.WAVE);
                    }
                    else
                    {
                        CheckEndGame(time);
                    }
                    break;
                case TowerDefState.OUTRO:
                    if (time - _stateTime > _outroDuration)
                    {
                        SetState(TowerDefState.END);
                    }
                    break;
                case TowerDefState.GAMEOVER:
                    if (time - _stateTime > _gameOverDuration)
                    {
                        SetState(TowerDefState.END);
                    }
                    break;
                case TowerDefState.CONTINUE:
                    if (time - _stateTime > 3f)
                    {
                        SetState(TowerDefState.WAVE);
                    }
                    break;
                case TowerDefState.TIMEOVER:
                    if (time - _stateTime > 1f)
                    {
                        SetState(TowerDefState.OUTRO);
                    }
                    break;
            }
        }
		else
		{
            if (time - _checkHealthsTime > 0.5f)
            {
                _checkHealthsTime = time;
                CheckHealths();
            }
        }
#if SHOW_ACCURACY
        UpdateAccuracy();
#endif
#if USE_KDK
        UpdateRegenerators();
#endif
        if (_updateTimer && Player.myplayer != null)
		{
            switch (_currentTimerType)
			{
                case TimerDisplayInfo.TimerType.TIME_FROM_START:
                    Player.myplayer.UpdateDisplayTime(time - _startTime);
                    break;
                case TimerDisplayInfo.TimerType.TIME_FROM_WAVE:
                    Player.myplayer.UpdateDisplayTime(time - _waveTime);
                    break;
                case TimerDisplayInfo.TimerType.TIME_FROM_FIRST_WAVE:
                    Player.myplayer.UpdateDisplayTime(time - _firstWaveTime);
                    break;
                case TimerDisplayInfo.TimerType.COUNTDOWN:
                    Player.myplayer.UpdateDisplayTime(_currentTimerDuration - time + _countdownTime);
                    break;
            }
            
        }
    }

    public void ContinueGame()
	{
        if (!keepLastWaveOnContinue)
        {
            int wave = Mathf.Max((_currentWave + 1) / 2 - 1, 0);
            _currentWave = wave;
        }
        SetState(TowerDefState.CONTINUE);
    }

    public void SetNoLife()
	{
        if (_healthsToDefend != null && _healthsToDefend.Length > 0)
        {
            foreach (Health h in _healthsToDefend)
                h.ForceCurrentHealth(10f);
        }
    }

    public void KillAllEnemies()
	{
        if (_waveSpawners != null)
        {
            foreach (WaveSpawner spawner in _waveSpawners)
            {
                spawner.KillAllEnemies();
            }
        }
    }

    private bool CheckEndWave(float time)
	{
        if (time - _checkEndWaveTime > 1f)
        {
            _checkEndWaveTime = time;

            CheckConditionsEvent conditions = GetConditionsOnWave(_currentWave);
            if (conditions != null)
			{
                if (!conditions.AreConditionsValid())
                    return false;
            }

            if (AreAllDead())
            {
                Debug.Log("[TOWERDEF] End Wave - All Dead!");
                return true;
            }
            if (!NeedAllDead())
            {
                if (time - _stateTime > _waveDuration)
                {
                    Debug.Log("[TOWERDEF] End Wave - Time Over!");
                    return true;
                }
            }
        }
        return false;
    }

    private CheckConditionsEvent GetConditionsOnWave(int wave)
	{
        if (_waveConditions != null)
        {
            foreach (WaveCheckConditions checkConditions in _waveConditions)
            {
                if (checkConditions.numWave == wave + 1)
                {
                    return checkConditions.conditions;
                }
            }
        }
        return null;
    }

    public int ComputeEnemies()
	{
        int count = 0;
        if (_waveSpawners != null)
        {
            foreach (WaveSpawner spawner in _waveSpawners)
            {
                count += spawner.ComputeEnemies();
            }
        }
        return count;
    }

    private bool AreAllDead()
    {
        if (_waveSpawners != null)
        {
            foreach (WaveSpawner spawner in _waveSpawners)
            {
                if (spawner.waveStarted && !spawner.AreAllDead())
                    return false;
            }
        }
        return true;
    }

    private bool NeedAllDead()
    {
        if (_waveSpawners != null)
        {
            foreach (WaveSpawner spawner in _waveSpawners)
            {
                if (spawner.NeedAllDead())
                    return true;
            }
        }
        return false;
    }

    private void CheckEndGame(float time, bool checkLastWave = false)
    {
        if (time - _checkHealthsTime > 0.5f)
        {
            _checkHealthsTime = time;
            if (!CheckHealths())
            {
                _endOfTimeReached = false;
                SetState(TowerDefState.GAMEOVER);
            }
        }
        else if (time - _startTime > _gameDuration - _introDuration - _outroDuration)
        {
            _endOfTimeReached = true;
            SetState(TowerDefState.TIMEOVER);
        }
        else if (checkLastWave)
		{
            if (_currentWave >= _nextRaceMaxWaveToFinish - 1)
			{
                _endOfTimeReached = true;
                _currentWave++;
                SetState(TowerDefState.OUTRO);
            }
		}
    }

    private void UpdateAccuracy()
	{
        Player.myplayer.SetAccuracyPercent(GameflowBase.GetPlayerAccuracy(gamesettings.STAT_TOUCH, gamesettings.STAT_FIRE) * 100f);
    }

#if USE_KDK
    private void UpdateRegenerators()
	{
        if (_regenerators != null)
        {
            Vector3 pos = Player.myplayer.transform.position;
            for (int i = 0; i < _regenerators.Length; ++i)
			{
                TargetRegenerator regenerator = _regenerators[i];
                if (regenerator != null)
				{
                    Health healthToDefend = _healthsToDefend[i];
                    TargetRecovery recovery = targetRecoveries[i];
                    if (regenerator.IsInCenter(pos))
                    {
                        if (!regenerator.activated)
                        {
                            if (regenerator.SetLightActivation(true, GameflowBase.myId))
                            {
                                GameflowKDK.TriggerPlayerEvent(GameflowKDK.PlayerEvent.RecoverAntenna, "ON-" + i);
                            }
                        }
                    }
                    else
                    {
                        if (regenerator.activated)
                        {
                            if (regenerator.SetLightActivation(false, GameflowBase.myId))
                            {
                                GameflowKDK.TriggerPlayerEvent(GameflowKDK.PlayerEvent.RecoverAntenna, "OFF-" + i);
                            }
                        }
                    }
                    bool needLife = !healthToDefend.dead && healthToDefend.currentHealth > 0f && healthToDefend.currentHealth < healthToDefend.maxHealth;
                    bool haveLife = _regeneratorLife > 0f;
                    bool gameStarted = _state == TowerDefState.WAVE || _state == TowerDefState.FALLBACK || _state == TowerDefState.RECOVERY;
                    if (!gameStarted || healthToDefend.dead || healthToDefend.currentHealth == 0f || !haveLife)
                        regenerator.SetState(TargetRegenerator.State.Disabled);
                    else if (healthToDefend.currentHealth >= healthToDefend.maxHealth)
                        regenerator.SetState(TargetRegenerator.State.Full);
                    else
                        regenerator.SetState(TargetRegenerator.State.Working);
                    regenerator.SetEnable(needLife && haveLife && gameStarted);
                    recovery.SetRecoveryByPlayer(regenerator.isLightVisible);
                }
            }
        }
    }
#endif

    public void UpdateRecoveryAntennaByPlayer(int numAntenna, int playerId, bool activate)
    {
        TargetRegenerator regenerator = _regenerators[numAntenna];
        if (regenerator != null)
            regenerator.SetLightActivation(activate, playerId);
    }

    private bool CheckHealths()
	{
        if (_healthsToDefend != null && _healthsToDefend.Length > 0)
		{
            float currentLife = 0f;
            for (int i = 0; i < _healthsToDefend.Length; ++i)
            {
                Health h = _healthsToDefend[i];
                if (h != null)
                {
                    if (isInvincible)
                        h.SetCurrentHealth(h.maxHealth);
                    if (!h.dead && h.currentHealth > 0f)
                        currentLife += h.currentHealth;
                    int percent = Mathf.Clamp(Mathf.CeilToInt(h.currentHealth / h.maxHealth * 100f), 0, 100);
                    Player.myplayer.antennaLifes[i].SetLifePercent(percent);
                }
            }
            if (currentLife > 0f)
			{
                if (_maxLife > 0f)
                    Player.myplayer.SetLifePercent(currentLife * 100f / _maxLife);
                return true;
            }
		}
        return false;
	}

    private IEnumerator UpdateUpgrades()
	{
        Player.myplayer.upgradeGoldRoot.SetActive(false);

#if USE_KDK
        while (!Player.myplayer.upgradeAnimator.gameObject.activeInHierarchy)
            yield return null;
#endif

        Player.myplayer.upgradeGoldText.text = Player.myplayer.teamGold.ToString();

        _recoveryExtraDuration = 0f;
        bool areAntennasUpgraded = false;

        List<UpgradeKDKSettings.UpgradeDataByWave> upWaves = _upgradeSettings.GetUpgradesByWave(_currentWave);
        foreach (UpgradeKDKSettings.UpgradeDataByWave upWave in upWaves)
        {
            if (upWave != null)
            {
                if (upWave.upType == UpgradeKDKSettings.UpgradeType.AntennaRecovery)
                {
                    Player.myplayer.upgradeAnimator.SetTrigger("CurrentUpgrade");
                    foreach (Health h in _healthsToDefend)
                    {
                        TargetRecovery targetRecov = h.GetComponent<TargetRecovery>();
                        if (targetRecov != null)
                            targetRecov.SetRecoveryBySecond(upWave.value);
                    }
                    SetAntennasLevel(upWave.level, true);
                    Player.myplayer.upgradeImage.sprite = upWave.sprite;
                    Player.myplayer.upgradeNoCash.SetActive(false);
                    Player.myplayer.upgradeChecked.SetActive(false);
                    Player.myplayer.upgradeNameText.text = RRLanguageManager.instance.GetString(upWave.textId);
                    Player.myplayer.upgradeCostText.text = "";
                    Player.myplayer.upgradeTitleText.text = RRLanguageManager.instance.GetString("str_kdk_recovery03");
                    areAntennasUpgraded = true;
                }
                else
                {
                    if (upWave.upType == UpgradeKDKSettings.UpgradeType.LeftGun)
                    {
                        Player.myplayer.UpgradeGun(true, upWave.level, 0);
                    }
                    else if (upWave.upType == UpgradeKDKSettings.UpgradeType.RightGun)
                    {
                        Player.myplayer.UpgradeGun(false, upWave.level, 0);
                    }
                }
            }
        }

        if (areAntennasUpgraded)
        {
            _recoveryExtraDuration += 8f;
            yield return new WaitForSeconds(8f);
            Player.myplayer.upgradeGoldRoot.SetActive(false);
            Player.myplayer.upgradeImage.GetComponentInParent<Animator>().Rebind();
            while (!Player.myplayer.upgradeAnimator.gameObject.activeInHierarchy)
                yield return null;
        }

        UpgradeKDKSettings.UpgradeDataByGold upWaveGold = _upgradeSettings.GetUpgradeByGold(_currentUpgradeByGold);
        if (upWaveGold != null)
        {
            Player.myplayer.upgradeAnimator.SetTrigger("CurrentUpgrade");
            Player.myplayer.upgradeImage.gameObject.SetActive(true);
            Player.myplayer.upgradeGoldRoot.SetActive(true);
            Player.myplayer.upgradeFrame.SetActive(true);
            if (Player.myplayer.teamGold >= upWaveGold.gold)
            {
                Player.myplayer.upgradeImage.sprite = upWaveGold.sprite;
                Player.myplayer.upgradeNameText.text = RRLanguageManager.instance.GetString(upWaveGold.textId);
                Player.myplayer.upgradeCostText.text = upWaveGold.gold.ToString();
                if (upWaveGold.upType == UpgradeKDKSettings.UpgradeType.LeftGun)
                    Player.myplayer.UpgradeGun(true, (int)upWaveGold.value, upWaveGold.gold);
                else if (upWaveGold.upType == UpgradeKDKSettings.UpgradeType.RightGun)
                    Player.myplayer.UpgradeGun(false, (int)upWaveGold.value, upWaveGold.gold);
                _currentUpgradeByGold++;

                Player.myplayer.upgradeNoCash.SetActive(false);
                Player.myplayer.upgradeChecked.SetActive(true);
                Player.myplayer.upgradeTitleText.text = RRLanguageManager.instance.GetString("str_kdk_recovery04");
            }
            else
			{
                Player.myplayer.upgradeImage.sprite = upWaveGold.sprite;
                Player.myplayer.upgradeNameText.text = RRLanguageManager.instance.GetString(upWaveGold.textId);
                Player.myplayer.upgradeCostText.text = upWaveGold.gold.ToString();
                Player.myplayer.upgradeNoCash.SetActive(false); // Not Used now
                Player.myplayer.upgradeChecked.SetActive(false);
                Player.myplayer.upgradeTitleText.text = RRLanguageManager.instance.GetString("str_kdk_recovery05");
            }
        }
        else
		{
            Player.myplayer.upgradeAnimator.SetTrigger("NoUpgrade");
            Player.myplayer.upgradeImage.gameObject.SetActive(false);
            Player.myplayer.upgradeFrame.SetActive(false);
            Player.myplayer.upgradeNoCash.SetActive(true);
            Player.myplayer.upgradeChecked.SetActive(false);
            Player.myplayer.upgradeNameText.text = RRLanguageManager.instance.GetString("str_kdk_orempty");
            Player.myplayer.upgradeCostText.text = "";
            if (_noUpgradeFirstTime)
            {
                Player.myplayer.upgradeTitleText.text = RRLanguageManager.instance.GetString("str_kdk_recovery06");
                _noUpgradeFirstTime = false;
            }
            else
            {
                Player.myplayer.upgradeTitleText.text = RRLanguageManager.instance.GetString(Player.myplayer.upgradeRecoveryTextId);
            }
        }

        
    }

    private void SetAntennasLevel(int level, bool levelUp = false, bool updateGauge = false)
	{
        if (multiplayerlobby.product != multiplayerlobby.Product.KDK)
            return;

        for (int i = 0; i < _healthsToDefend.Length; ++i)
        {
            Health h = _healthsToDefend[i];
            if (updateGauge)
            {
                h.UpdateJauge();
            }
            TargetRecovery targetRecov = h.GetComponent<TargetRecovery>();
            if (targetRecov != null)
            {   
                UI_HealthJauge gauge = targetRecov.GetComponentInChildren<UI_HealthJauge>();
                if (gauge != null)
                {
                    gauge.SetLevelText("LVL" + level);
                    gauge.SetName(RRLanguageManager.instance.GetString("str_kdk_antena_id") + " " + (i + 1).ToString("00"));
                    if (levelUp)
                        gauge.LevelUp();
                }
            }
        }
    }

    public List<float> GetHealthToDefendValues()
	{
        List<float> values = new List<float>();
        foreach (Health h in _healthsToDefend)
		{
            values.Add(h.currentHealth);
		}
        return values;
	}

    public void SetHealthToDefendValues(List<float> values)
    {
        for (int i = 0; i < values.Count; ++i)
        {
            _healthsToDefend[i].ForceCurrentHealth(values[i]);
        }
    }

    public void TeleportPlayerToStartSpawnpoint()
	{
        if (_startSpawnpoints != null)
            _startSpawnpoints.TeleportPlayer();
	}

    public bool CanShowNextRace()
	{
#if USE_KDK
        if (multiplayerlobby.startLevel > 0)
            return false;
#endif
        if (_currentWave >= _nextRaceMaxWaveToFinish)
            return true;
        if (multiplayerlobby.endlessDuration >= _nextRaceMinRemainingTime)
        {
            if (_currentWave >= _nextRaceMinWaveToFinish)
                return true;
            if (Time.time - _startTime > _nextRaceMaxGameTime)
                return true;
        }
        return false;
	}

    public void ForceWave(int wave)
	{
        SetStateEvent(TowerDefState.WAVE, wave, 0);
        ApplyWave(wave);
    }

    private void CallStartWaveEvent(bool teleport = false)
	{
#if UNITY_EDITOR && USE_BOD
        _introDuration = 5f;
#endif
        ApplyWave(_startWave, teleport);
	}

    private void ApplyWave(int wave, bool teleport = false)
	{
        foreach (StartWaveEvent swe in _startWaveEvents)
        {
            if (swe.numWave == wave + 1)
            {
                swe.actions?.Invoke();
                foreach (GameObject go in swe.goListToActivate)
                    go.SetActive(true);
                if (teleport)
                    Player.myplayer.transform.position = swe.playerPos;
            }
        }
    }
}
