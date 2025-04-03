using AllMyScripts.Common.Tools;
using RRLib;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class roomline : MonoBehaviour
{
    private const string TOG_ENDLESS_DURATION_KEY = "TOG_ENDLESS_DURATION_KEY";
    private const string KDK_ENDLESS_DURATION_KEY = "KDK_ENDLESS_DURATION_KEY";
    private const string BOD_ENDLESS_DURATION_KEY = "BOD_ENDLESS_DURATION_KEY";

    [System.Serializable]
    public class LevelSprite
	{
        public string level;
        public Sprite sprite;
	}

    public enum BodVersion
	{
        Short,
        Long
	}


    public Image backgroundimage => _backgroundimage;
    public string roomName => gameObject.name;
    public string unique_session => _unique_session;
    public GameObject goCreatePdf => _goCreatePdf;
    public int currentTeam => _currentPanel;
    public int playerCountMaxByTeam => _playerCountMaxByTeam;
    public int playerCountTeamA => _playerCountTeamA;
    public int playerCountTeamB => _playerCountTeamB;

    public bool isActivated { get; set; }

    [Header("Interface")]
    [SerializeField]
    private GameObject _classicInterface = null;
    [SerializeField]
    private GameObject _advancedInterface = null;
    [SerializeField]
    private GameObject _otherTeamInterface = null;
    [SerializeField]
    private Button _otherTeamButton = null;
    [SerializeField]
    private Image _greyOptionsImage = null;
    [SerializeField]
    private Image _advancedGreyOptionsImage = null;
    [SerializeField]
    private Image _backgroundimage = null;
    [SerializeField]
    private Button _addMachineBtn = null;
    [SerializeField]
    private Button _addAdvancedMachineBtn = null;
    [SerializeField]
    private Button _addOtherMachineBtn = null;
    [SerializeField]
    private Button _startSessionBtn = null;
    [SerializeField]
    private Button _stopSessionBtn = null;
    [SerializeField]
    private Button _advancedStartSessionBtn = null;
    [SerializeField]
    private Button _advancedStopSessionBtn = null;
    [SerializeField]
    private Button _advancedStopSessionAndKillBtn = null;
    [SerializeField]
    private float _bkgImageAlpha = 0.6f;
    [SerializeField]
    private GameObject _goCreatePdf = null;
    [SerializeField]
    private Image _startSessionImage = null;
    [SerializeField]
    private Transform _trContent = null;
    [SerializeField]
    private Transform _trOtherContent = null;
    [SerializeField]
    private Transform _trClassicContent = null;
    [SerializeField]
    private TextMeshProUGUI _timecount = null;
    [SerializeField]
    private TextMeshProUGUI _advancedTimecount = null;
    [SerializeField]
    private TextMeshProUGUI _playerCountText = null;
    [SerializeField]
    private TextMeshProUGUI _otherPlayerCountText = null;
    [SerializeField]
    private Image _playerCountImage = null;
    [SerializeField]
    private Sprite _blueTeamSprite = null;
    [SerializeField]
    private Sprite _neutralTeamSprite = null;
    [Header("Endless")]
    [SerializeField]
    private Toggle _endlessToggle = null;
    [SerializeField]
    private Slider _endlessSlider = null;
    [SerializeField]
    private TextMeshProUGUI _endlessSliderText = null;
    [SerializeField]
    private GameObject _endlessModeRoot = null;
    [SerializeField]
    private GameObject _endlessTitleNoToggle = null;
    [SerializeField]
    private Toggle _advancedEndlessToggle = null;
    [SerializeField]
    private Slider _advancedEndlessSlider = null;
    [SerializeField]
    private TextMeshProUGUI _advancedEndlessSliderText = null;
    [SerializeField]
    private GameObject _advancedEndlessModeRoot = null;
    [SerializeField]
    private GameObject _advancedEndlessTitleNoToggle = null;
    [Header("Next Race")]
    [SerializeField]
    private Toggle _canPlayNextRaceToggle = null;
    [SerializeField]
    private GameObject _canPlayNextRaceRoot = null;
    [SerializeField]
    private Toggle _advancedCanPlayNextRaceToggle = null;
    [SerializeField]
    private GameObject _advancedCanPlayNextRaceRoot = null;
    [Header("Level")]
    [SerializeField]
    private GameObject _levelRoot = null;
    [SerializeField]
    private Button _levelButton = null;
    [SerializeField]
    private TextMeshProUGUI _levelText = null;
    [Header("Mode")]
    [SerializeField]
    private GameObject _gameModeRoot = null;
    [SerializeField]
    private Button _gameModeButton = null;
    [SerializeField]
    private TextMeshProUGUI _gameModeText = null;
    [SerializeField]
    private GameObject _advancedGameModeRoot = null;
    [SerializeField]
    private Button _advancedGameModeButton = null;
    [SerializeField]
    private TextMeshProUGUI _advancedGameModeText = null;
    [Header("Version")]
    [SerializeField]
    private GameObject _versionRoot = null;
    [SerializeField]
    private Button _versionButton = null;
    [SerializeField]
    private TextMeshProUGUI _versionText = null;
    [SerializeField]
    private GameObject _advancedVersionRoot = null;
    [SerializeField]
    private Button _advancedVersionButton = null;
    [SerializeField]
    private TextMeshProUGUI _advancedVersionText = null;
    [Header("Theme")]
    [SerializeField]
    private GameObject _themeRoot = null;
    [SerializeField]
    private Button _themeButton = null;
    [SerializeField]
    private TextMeshProUGUI _themeText = null;
    [SerializeField]
    private GameObject _advancedThemeRoot = null;
    [SerializeField]
    private Button _advancedThemeButton = null;
    [SerializeField]
    private TextMeshProUGUI _advancedThemeText = null;
    [Header("Popup")]
    [SerializeField]
    private ChoicesPopup _choicesPopupPrefab = null;
    [Header("Start Wave")]
    [SerializeField]
    private GameObject _startWaveRoot = null;
    [SerializeField]
    private Slider _startWaveSlider = null;
    [SerializeField]
    private TextMeshProUGUI _startWaveSliderText = null;
    [SerializeField]
    private GameObject _advancedStartWaveRoot = null;
    [SerializeField]
    private Slider _advancedStartWaveSlider = null;
    [SerializeField]
    private TextMeshProUGUI _advancedStartWaveSliderText = null;
    [Header("Durations")]
    [SerializeField]
    private float _togDefaultDuration = 20f;
    [SerializeField]
    private float _kdkDefaultDuration = 30f;
    [SerializeField]
    private float _bodDefaultDuration = 53f;
    [SerializeField]
    private int _bodShortDuration = 28;
    [Header("Levels")]
    [SerializeField]
    private LevelSprite[] _levelSprites = null;
    [SerializeField]
    private LevelSettings _togLevelSettings = null;
    [SerializeField]
    private LevelSettings _kdkLevelSettings = null;
    [SerializeField]
    private LevelSettings _bodLevelSettings = null;
    [SerializeField]
    private Button _prevLevelBtn = null;
    [SerializeField]
    private Button _nextLevelBtn = null;
    [SerializeField]
    private Image _prevLevelImage = null;
    [SerializeField]
    private Image _nextLevelImage = null;
    [SerializeField]
    private Image _currentLevelImage = null;
    [SerializeField]
    private TextMeshProUGUI _currentLevelName = null;
    [Header("Context")]
    public string currentgamename = "";

    private int _endlessGameDuration = 0;
    private int _startWave = 0;
    private bool _canPlayNextRace = true;
    private bool _bodShortVersion = true;
    private multiplayerlobby.GameMode _gameMode = multiplayerlobby.GameMode.Normal;
    private multiplayerlobby.SkinTheme _theme = multiplayerlobby.SkinTheme.Normal;
    private int _level = 0;
    private int _defaultEndlessDuration = 5;
    private multiplayerlobby _ml;
    private masterrooms _mr;
    private string _unique_session = "";
    private int _currentPanel = 0;
    private LevelSettings _levelSettings = null;
    private string _levelListId = null;
    private bool _isInAdvancedInterface = false;
    private int _playerCountMaxByTeam = 0;
    private int _playerCountTeamA = 0;
    private int _playerCountTeamB = 0;

    public void Init(multiplayerlobby ml, masterrooms mr)
	{
        _ml = ml;
        _mr = mr;
    }

	private void Start()
    {
        _timecount.gameObject.SetActive(false);
        _advancedTimecount.gameObject.SetActive(false);
        _backgroundimage.color = new Color(0.8f, 0.8f, 0.8f, _bkgImageAlpha);
        Activate();
        _endlessToggle.onValueChanged.AddListener(OnEndlessToggleChanged);
        _advancedEndlessToggle.onValueChanged.AddListener(OnEndlessToggleChanged);
        _endlessSlider.onValueChanged.AddListener(OnEndlessSliderChanged);
        _advancedEndlessSlider.onValueChanged.AddListener(OnEndlessSliderChanged);
        _endlessToggle.SetValue(false);
        _advancedEndlessToggle.SetValue(false);
        OnEndlessToggleChanged(false);
        _canPlayNextRaceToggle.onValueChanged.AddListener(OnPlayNextRaceToggleChanged);
        _advancedCanPlayNextRaceToggle.onValueChanged.AddListener(OnPlayNextRaceToggleChanged);
        _canPlayNextRaceToggle.SetValue(true);
        _advancedCanPlayNextRaceToggle.SetValue(true);
        OnPlayNextRaceToggleChanged(true);
        _startWaveSlider.onValueChanged.AddListener(OnStartWaveSliderChanged);
        _advancedStartWaveSlider.onValueChanged.AddListener(OnStartWaveSliderChanged);
        OnStartWaveSliderChanged(1);

        _addMachineBtn.onClick.AddListener(AddMachine);
        _addAdvancedMachineBtn.onClick.AddListener(AddMachine);
        _addOtherMachineBtn.onClick.AddListener(AddMachine);
        _startSessionBtn.onClick.AddListener(StartSession);
        _advancedStartSessionBtn.onClick.AddListener(StartSession);
        _stopSessionBtn.onClick.AddListener(StopSession);
        _advancedStopSessionBtn.onClick.AddListener(StopSession);
        _advancedStopSessionAndKillBtn.onClick.AddListener(StopSessionAndKill);

        _prevLevelBtn.onClick.AddListener(OnPrevLevelClick);
        _nextLevelBtn.onClick.AddListener(OnNextLevelClick);

        _endlessModeRoot.SetActive(true);
        _advancedEndlessModeRoot.SetActive(true);

        _canPlayNextRaceRoot.SetActive(false);
        _advancedCanPlayNextRaceRoot.SetActive(false);

        _greyOptionsImage.gameObject.SetActive(false);
        _advancedGreyOptionsImage.gameObject.SetActive(false);

        _gameModeButton.onClick.AddListener(OnClickGameMode);
        _advancedGameModeButton.onClick.AddListener(OnClickGameMode);
        _versionButton.onClick.AddListener(OnClickVersion);
        _advancedVersionButton.onClick.AddListener(OnClickVersion);
        _themeButton.onClick.AddListener(OnClickTheme);
        _advancedThemeButton.onClick.AddListener(OnClickTheme);
        _levelButton.onClick.AddListener(OnClickLevel);

        UpdateVersion(BodVersion.Short);
        OnLevelResult(RRLanguageManager.instance.GetString("str_launcher_leveltxt") + " 1");
        UpdateProduct();
        InitLevelSettings();
        SetCurrentPanel(0);
    }

    private void OnDestroy()
    {
        _ml = null;
        _mr = null;
        _endlessToggle.onValueChanged.RemoveAllListeners();
        _advancedEndlessToggle.onValueChanged.RemoveAllListeners();
        _endlessSlider.onValueChanged.RemoveAllListeners();
        _advancedEndlessSlider.onValueChanged.RemoveAllListeners();
        _canPlayNextRaceToggle.onValueChanged.RemoveAllListeners();
        _advancedCanPlayNextRaceToggle.onValueChanged.RemoveAllListeners();
        _startWaveSlider.onValueChanged.RemoveAllListeners();
        _advancedStartWaveSlider.onValueChanged.RemoveAllListeners();
        _gameModeButton.onClick.RemoveAllListeners();
        _advancedGameModeButton.onClick.RemoveAllListeners();
        _versionButton.onClick.RemoveAllListeners();
        _advancedVersionButton.onClick.RemoveAllListeners();
        _themeButton.onClick.RemoveAllListeners();
        _advancedThemeButton.onClick.RemoveAllListeners();
        _levelButton.onClick.RemoveAllListeners();

        _addMachineBtn.onClick.RemoveListener(AddMachine);
        _addAdvancedMachineBtn.onClick.RemoveListener(AddMachine);
        _addOtherMachineBtn.onClick.RemoveListener(AddMachine);
        _startSessionBtn.onClick.RemoveListener(StartSession);
        _advancedStartSessionBtn.onClick.RemoveListener(StartSession);
        _stopSessionBtn.onClick.RemoveListener(StopSession);
        _advancedStopSessionBtn.onClick.RemoveListener(StopSession);
        _advancedStopSessionAndKillBtn.onClick.RemoveListener(StopSessionAndKill);

        _prevLevelBtn.onClick.RemoveListener(OnPrevLevelClick);
        _nextLevelBtn.onClick.RemoveListener(OnNextLevelClick);
    }

    private void Update()
    {
        _startSessionImage.color = apicalls.isDemoOver ? Color.black : Color.white;
    }

    public void SetAdvancedInterface(bool advanced)
	{
        _isInAdvancedInterface = advanced;
        _classicInterface.SetActive(!advanced);
        _advancedInterface.SetActive(advanced);
    }

    public Transform GetContentFromTeam(int team)
	{
        if (_isInAdvancedInterface)
        {
            switch (team)
            {
                default:
                case 0:
                    return _trContent;
                case 1:
                    return _trOtherContent;
            }
        }
        else
		{
            return _trClassicContent;
        }
    }

    public void UpdateProduct()
    {
        bool togProduct = multiplayerlobby.product == multiplayerlobby.Product.TOG;
        bool kdkProduct = multiplayerlobby.product == multiplayerlobby.Product.KDK;
        bool bodProduct = multiplayerlobby.product == multiplayerlobby.Product.BOD;
        
        _otherTeamInterface.SetActive(togProduct);
        _otherTeamButton.gameObject.SetActive(togProduct);
        _playerCountImage.sprite = togProduct ? _blueTeamSprite : _neutralTeamSprite;
        _gameModeRoot.SetActive(togProduct);
        _advancedGameModeRoot.SetActive(togProduct);
        _versionRoot.SetActive(bodProduct);
        _advancedVersionRoot.SetActive(bodProduct);
        _levelRoot.SetActive(kdkProduct);
        _themeRoot.SetActive(togProduct || kdkProduct);
        _advancedThemeRoot.SetActive(togProduct || kdkProduct);
        _startWaveRoot.SetActive(kdkProduct);
        _advancedStartWaveRoot.SetActive(kdkProduct);

        _canPlayNextRaceRoot.SetActive(togProduct);
        _advancedCanPlayNextRaceRoot.SetActive(togProduct);

        if (kdkProduct || bodProduct)
        {
            _gameMode = multiplayerlobby.GameMode.Endless;
            _canPlayNextRace = true;
        }

        bool isEndlessMode = _gameMode == multiplayerlobby.GameMode.Endless;
        bool showEndlessModeRoot = isEndlessMode;
        int playerCountMax = 0;
        int playerCount0 = _mr.GetPlayerCountAtTeam(0);
        int playerCount1 = _mr.GetPlayerCountAtTeam(1);

        switch (multiplayerlobby.product)
        {
            case multiplayerlobby.Product.TOG:
                if (PlayerPrefs.HasKey(TOG_ENDLESS_DURATION_KEY))
                {
                    _defaultEndlessDuration = Mathf.RoundToInt(PlayerPrefs.GetFloat(TOG_ENDLESS_DURATION_KEY));
                }
                else
                {
                    _defaultEndlessDuration = Mathf.RoundToInt(_togDefaultDuration);
                    PlayerPrefs.SetFloat(TOG_ENDLESS_DURATION_KEY, _togDefaultDuration);
                }
                _endlessSlider.minValue = 5;
                _endlessSlider.maxValue = 60;
                _advancedEndlessSlider.minValue = 5;
                _advancedEndlessSlider.maxValue = 60;
                playerCountMax = playerCount0 == 0 || playerCount1 == 0 ? 8 : 10;
                break;
            case multiplayerlobby.Product.KDK:
                if (PlayerPrefs.HasKey(KDK_ENDLESS_DURATION_KEY))
                {
                    _defaultEndlessDuration = Mathf.RoundToInt(PlayerPrefs.GetFloat(KDK_ENDLESS_DURATION_KEY));
                }
                else
                {
                    _defaultEndlessDuration = Mathf.RoundToInt(_kdkDefaultDuration);
                    PlayerPrefs.SetFloat(KDK_ENDLESS_DURATION_KEY, _kdkDefaultDuration);
                }
                _endlessSlider.minValue = 5;
                _endlessSlider.maxValue = 60;
                _advancedEndlessSlider.minValue = 5;
                _advancedEndlessSlider.maxValue = 60;
                playerCountMax = 16;
                break;
            case multiplayerlobby.Product.BOD:
                if (PlayerPrefs.HasKey(BOD_ENDLESS_DURATION_KEY))
                {
                    _defaultEndlessDuration = Mathf.RoundToInt(PlayerPrefs.GetFloat(BOD_ENDLESS_DURATION_KEY));
                }
                else
                {
                    _defaultEndlessDuration = Mathf.RoundToInt(_bodDefaultDuration);
                    PlayerPrefs.SetFloat(BOD_ENDLESS_DURATION_KEY, _bodDefaultDuration);
                }
                _endlessSlider.minValue = 45;
                _endlessSlider.maxValue = 60;
                _advancedEndlessSlider.minValue = 45;
                _advancedEndlessSlider.maxValue = 60;
                if (_bodShortVersion)
                {
                    _defaultEndlessDuration = _bodShortDuration;
                    showEndlessModeRoot = false;
                }
                playerCountMax = 8;
                break;
        }

        _endlessGameDuration = isEndlessMode ? Mathf.RoundToInt(_defaultEndlessDuration * 60f) : 0;
        _endlessModeRoot.SetActive(showEndlessModeRoot);
        _advancedEndlessModeRoot.SetActive(showEndlessModeRoot);
        _endlessTitleNoToggle.SetActive(isEndlessMode);
        _advancedEndlessTitleNoToggle.SetActive(isEndlessMode);
        _endlessToggle.SetValue(isEndlessMode);
        _advancedEndlessToggle.SetValue(isEndlessMode);
        OnEndlessToggleChanged(isEndlessMode);
        OnEndlessSliderChanged(_defaultEndlessDuration);
        if (showEndlessModeRoot)
        {
            _endlessSlider.value = _defaultEndlessDuration;
            _advancedEndlessSlider.value = _defaultEndlessDuration;
        }
        
        _playerCountText.text = $"{playerCount0} / {playerCountMax}";
        _otherPlayerCountText.text = $"{playerCount1} / {playerCountMax}";
        _playerCountMaxByTeam = playerCountMax;
        _playerCountTeamA = playerCount0;
        _playerCountTeamB = playerCount1;
    }

    public string GetGameData()
	{
        string d = multiplayerlobby.DELIMITER;
        // See FillGameData to split these data
        if (multiplayerlobby.product == multiplayerlobby.Product.TOG)
            return $"{_gameMode}{d}{_endlessGameDuration}{d}{(_canPlayNextRace ? 1 : 0)}{d}{_theme}";
        else
            return $"{_gameMode}{d}{_endlessGameDuration}{d}{(_canPlayNextRace ? 1 : 0)}{d}{_theme}{d}{_level}{d}{_startWave}";
    }

    private void OnEndlessToggleChanged(bool on)
    {
        if (on)
        {
            _endlessSlider.gameObject.SetActive(true);
            _advancedEndlessSlider.gameObject.SetActive(true);
            _endlessSliderText.gameObject.SetActive(true);
            _advancedEndlessSliderText.gameObject.SetActive(true);
            OnEndlessSliderChanged(_defaultEndlessDuration);
        }
        else
        {
            _endlessSlider.gameObject.SetActive(false);
            _advancedEndlessSlider.gameObject.SetActive(false);
            _endlessSliderText.gameObject.SetActive(false);
            _advancedEndlessSliderText.gameObject.SetActive(false);
            _endlessGameDuration = 0;
        }
    }

    private void OnPlayNextRaceToggleChanged(bool on)
	{
        _canPlayNextRace = on;
    }

    private void OnClickGameMode()
	{
        ChoicesPopup popup = GameObject.Instantiate<ChoicesPopup>(_choicesPopupPrefab, transform);
        popup.transform.localPosition = Vector3.zero;
        popup.transform.localScale = Vector3.one;
        int modeCount = (int)multiplayerlobby.GameMode.Count;
        string[] texts = new string[modeCount];
        string[] values = new string[modeCount];
        for (int i = 0; i < modeCount; ++i)
        {
            multiplayerlobby.GameMode mode = (multiplayerlobby.GameMode)i;
            texts[i] = RRLanguageManager.instance.GetString("str_launcher_gamemode_" + mode.ToString().ToLower());
            values[i] = mode.ToString();
        }
        popup.SetTitle(RRLanguageManager.instance.GetString("str_launcher_gamemode_btntxt"));
        popup.InitChoices(texts, values, OnGameModeResult);
    }

    private void OnGameModeResult(string result)
	{
        string text = RRLanguageManager.instance.GetString("str_launcher_gamemode_" + result.ToLower());
        _gameModeText.text = text;
        _advancedGameModeText.text = text;
        Enum.TryParse(result, out _gameMode);
        bool showDuration = _gameMode == multiplayerlobby.GameMode.Endless;
        if (showDuration)
        {
            OnEndlessSliderChanged(_defaultEndlessDuration);
            _endlessSlider.value = _defaultEndlessDuration;
            _advancedEndlessSlider.value = _defaultEndlessDuration;
        }
        else
        {
            _endlessGameDuration = 0;
        }
        _endlessSlider.gameObject.SetActive(showDuration);
        _advancedEndlessSlider.gameObject.SetActive(showDuration);
        _endlessSliderText.gameObject.SetActive(showDuration);
        _advancedEndlessSliderText.gameObject.SetActive(showDuration);
        UpdateProduct();

        if (multiplayerlobby.product == multiplayerlobby.Product.TOG)
        {
            switch (_gameMode)
            {
                case multiplayerlobby.GameMode.Normal:
                    _levelListId = "Arcade";
                    break;
                case multiplayerlobby.GameMode.Endless:
                    _levelListId = "Endless";
                    break;
                case multiplayerlobby.GameMode.Kid:
                    _levelListId = "Kid_Races";
                    break;
            }
            _level = 0;
            UpdateLevels();
        }
    }

    private void OnClickVersion()
	{
        ChoicesPopup popup = GameObject.Instantiate<ChoicesPopup>(_choicesPopupPrefab, transform);
        popup.transform.localPosition = Vector3.zero;
        popup.transform.localScale = Vector3.one;
        int versionCount = 2;
        string[] texts = new string[versionCount];
        string[] values = new string[versionCount];
        texts[0] = RRLanguageManager.instance.GetString("str_launcher_ravageshort");
        texts[1] = RRLanguageManager.instance.GetString("str_launcher_ravagelong");
        values[0] = BodVersion.Short.ToString();
        values[1] = BodVersion.Long.ToString();
        popup.SetTitle("Version");
        popup.InitChoices(texts, values, OnVersionResult);

    }

    private void OnVersionResult(string result)
    {
        if (Enum.TryParse(result, out BodVersion version))
        {
            UpdateVersion(version);
        }
    }

    private void UpdateVersion(BodVersion version)
    {
        string text = null;
        switch (version)
        {
            case BodVersion.Short:
                text = RRLanguageManager.instance.GetString("str_launcher_ravageshort");
                _bodShortVersion = true;
                break;
            case BodVersion.Long:
                text = RRLanguageManager.instance.GetString("str_launcher_ravagelong");
                _bodShortVersion = false;
                break;
        }
        _versionText.text = text;
        _advancedVersionText.text = text;
        UpdateProduct();
    }

    private void OnClickTheme()
	{
        ChoicesPopup popup = GameObject.Instantiate<ChoicesPopup>(_choicesPopupPrefab, transform);
        popup.transform.localPosition = Vector3.zero;
        popup.transform.localScale = Vector3.one;
        int themeCount = (int)multiplayerlobby.SkinTheme.Count;
        if (multiplayerlobby.product == multiplayerlobby.Product.KDK)
            themeCount = 2;
        string[] texts = new string[themeCount];
        string[] values = new string[themeCount];
        for (int i = 0; i < themeCount; ++i)
        {
            multiplayerlobby.SkinTheme theme = (multiplayerlobby.SkinTheme)i;
            texts[i] = RRLanguageManager.instance.GetString("str_launcher_theme_" + theme.ToString().ToLower());
            values[i] = theme.ToString();
        }
        popup.SetTitle(RRLanguageManager.instance.GetString("str_launcher_theme_btntxt"));
        popup.InitChoices(texts, values, OnThemeResult);
    }

    private void OnThemeResult(string result)
    {
        string text = RRLanguageManager.instance.GetString("str_launcher_theme_" + result.ToLower());
        _themeText.text = text;
        _advancedThemeText.text = text;
        Enum.TryParse(result, out _theme);
    }

    private void OnClickLevel()
	{
        ChoicesPopup popup = GameObject.Instantiate<ChoicesPopup>(_choicesPopupPrefab, transform);
        popup.transform.localPosition = Vector3.zero;
        popup.transform.localScale = Vector3.one;
        int levelCount = 1;
        if (multiplayerlobby.product == multiplayerlobby.Product.KDK)
            levelCount = 2;
        string[] texts = new string[levelCount];
        for (int i = 0; i < levelCount; ++i)
            texts[i] = RRLanguageManager.instance.GetString("str_launcher_leveltxt") + " " + (i + 1);
        popup.SetTitle(RRLanguageManager.instance.GetString("str_launcher_lvlselect"));
        popup.InitChoices(texts, texts, OnLevelResult);
    }

    private void OnLevelResult(string result)
    {
        _levelText.text = result;
        string[] split = result.Split(' ');
        if (split != null && split.Length == 2)
        {
            int.TryParse(split[1], out int level);
            _level = level - 1;
        }
    }

    private void OnPrevLevelClick()
    {
        LevelSettings.LevelList list = _levelSettings.GetLevelListFromId(_levelListId);
        if (_level > 0)
            _level--;
        else
            _level = list.levelIds.Count - 1;
        UpdateLevels();
    }

    private void OnNextLevelClick()
    {
        LevelSettings.LevelList list = _levelSettings.GetLevelListFromId(_levelListId);
        if (_level < list.levelIds.Count - 1)
            _level++;
        else
            _level = 0;
        UpdateLevels();
    }

    private void InitLevelSettings()
	{
        switch (multiplayerlobby.product)
        {
            case multiplayerlobby.Product.TOG:
                _levelSettings = _togLevelSettings;
                _levelListId = "Arcade";
                break;
            case multiplayerlobby.Product.KDK:
                _levelSettings = _kdkLevelSettings;
                _levelListId = "TowerDef_Endless";
                break;
            case multiplayerlobby.Product.BOD:
                _levelSettings = _bodLevelSettings;
                _levelListId = "BOD_Levels";
                break;
        }
        UpdateLevels();
    }

    private void UpdateLevels()
	{
        LevelSettings.LevelList list = _levelSettings.GetLevelListFromId(_levelListId);
        LevelSettings.Level level = _levelSettings.GetLevelFromListIndex(_levelListId, _level);
        Sprite sprite = GetSpriteFromLevelName(level.id);
        _currentLevelImage.sprite = sprite;
        _currentLevelName.gameObject.GetComponent<RRLocalizedTextMP>().SetTextId(level.textId);
        // Prev image 
        int prevLevel = _level - 1;
        if (prevLevel < 0)
            prevLevel = list.levelIds.Count - 1;
        level = _levelSettings.GetLevelFromListIndex(_levelListId, prevLevel);
        sprite = GetSpriteFromLevelName(level.id);
        _prevLevelImage.sprite = sprite;
        // Next image
        int nextLevel = _level + 1;
        if (nextLevel == list.levelIds.Count)
            nextLevel = 0;
        level = _levelSettings.GetLevelFromListIndex(_levelListId, nextLevel);
        sprite = GetSpriteFromLevelName(level.id);
        _nextLevelImage.sprite = sprite;
    }

    public Sprite GetSpriteFromLevelName(string level)
	{
        int count = _levelSprites.Length;
        for (int i = 0; i < count; ++i)
		{
            if (_levelSprites[i].level == level)
                return _levelSprites[i].sprite;
        }
        return null;
	}

    private void OnEndlessSliderChanged(float value)
    {
        _endlessSliderText.text = $"{value} min";
        _advancedEndlessSliderText.text = $"{value} min";
        _endlessGameDuration = Mathf.RoundToInt(value * 60f);
        SaveEndlessSliderValue();
    }

    private void SaveEndlessSliderValue()
	{
        Slider slider = _isInAdvancedInterface ? _advancedEndlessSlider : _endlessSlider;
        if (slider != null)
        {
            switch (multiplayerlobby.product)
            {
                case multiplayerlobby.Product.TOG:
                    PlayerPrefs.SetFloat(TOG_ENDLESS_DURATION_KEY, slider.value);
                    break;
                case multiplayerlobby.Product.KDK:
                    PlayerPrefs.SetFloat(KDK_ENDLESS_DURATION_KEY, slider.value);
                    break;
                case multiplayerlobby.Product.BOD:
                    PlayerPrefs.SetFloat(BOD_ENDLESS_DURATION_KEY, slider.value);
                    break;
            }
        }
    }

    private void OnStartWaveSliderChanged(float value)
    {
        _startWaveSliderText.text = $"{value}";
        _advancedStartWaveSliderText.text = $"{value}";
        _startWave = Mathf.RoundToInt(value) - 1;
    }

    public void Delete()
    {
        _mr.Remove(gameObject.name);
        Destroy(gameObject);
    }

    public void SelectRoom()
    {
        roomline[] alllines = _mr.GetComponentsInChildren<roomline>();
        foreach(roomline rl in alllines)
            rl.backgroundimage.color = new Color(0.8f, 0.8f, 0.8f, _bkgImageAlpha);
        _backgroundimage.color = new Color(1,1,1, _bkgImageAlpha);
        _ml.SetSelectedGame(this);

        _mr.selectedtitle.text = gameObject.name;
        _mr.Select(gameObject.name);
        _mr.UpdateMe();
        selectedline_tag[] allplayers = gameObject.GetComponentsInChildren<selectedline_tag>();
        foreach (selectedline_tag line in allplayers)
        {
            line.gameObject.transform.localScale = Vector3.one;
        }
    }

    public void SetCurrentPanel(int num)
	{
        _currentPanel = num;
        _addMachineBtn.interactable = num == 0;
        _addAdvancedMachineBtn.interactable = num == 0;
        _addOtherMachineBtn.interactable = num == 1;
    }

    public void AddMachine()
    {
        SelectRoom();
        _ml.ShowMachinesPopup();
    }

    public void StartSession()
    {
        if (apicalls.isDemoOver)
            return;

        _unique_session = apicalls.GetUniqueSession();

        if (_mr.StartSession(this))
            Grey();

        SaveEndlessSliderValue();
    }

    public void StopSession()
    {
        EndSession(true);
    }

    public void StopSessionAndKill()
    {
        EndSession(true, true);
    }

    private void EndSession(bool callJoinToStopSession, bool killProcess = false)
    {
        if (!_ml.gametreatment)
        {
            _addMachineBtn.gameObject.SetActive(true);
            _addAdvancedMachineBtn.gameObject.SetActive(true);
            _addOtherMachineBtn.gameObject.SetActive(true);
            _timecount.gameObject.SetActive(false);
            _advancedTimecount.gameObject.SetActive(false);
            StopAllCoroutines();

            if (callJoinToStopSession)
            {
                _mr.StartCoroutine(_mr.JoinToStopSession(currentgamename, killProcess));
            }
            else
			{
                _mr.EndSession();
            }

            _stopSessionBtn.gameObject.SetActive(false);
            _advancedStopSessionBtn.gameObject.SetActive(false);
            _advancedStopSessionAndKillBtn.gameObject.SetActive(false);
            _startSessionBtn.gameObject.SetActive(true);
            _advancedStartSessionBtn.gameObject.SetActive(true);

            if (_endlessSlider != null)
                _endlessSlider.enabled = true;
            if (_advancedEndlessSlider != null)
                _advancedEndlessSlider.enabled = true;
        }
        // here stop all
    }

    public void CleanPlayers()
    {
        playerline_tag[] allplayerline = gameObject.GetComponentsInChildren<playerline_tag>();
        foreach (playerline_tag ptag in allplayerline)
        {
            Destroy(ptag.gameObject);
        }
    }
    public void Activate()
    {
        _timecount.gameObject.SetActive(false);
        _advancedTimecount.gameObject.SetActive(false);
        _addMachineBtn.gameObject.SetActive(true);
        _addAdvancedMachineBtn.gameObject.SetActive(true);
        _addOtherMachineBtn.gameObject.SetActive(true);
        _greyOptionsImage.gameObject.SetActive(false);
        _advancedGreyOptionsImage.gameObject.SetActive(false);
        isActivated = true;
    }

    void Grey()
    {
        CleanPlayers();

        selectedline_tag[] alllines = gameObject.GetComponentsInChildren<selectedline_tag>();
        foreach (selectedline_tag line in alllines)
        {
            selectedroomline srl = line.gameObject.GetComponent<selectedroomline>();

            playerline_tag tag = _mr.CreatePlayerLineFromMachine(this, srl.machine);

            tag.transform.SetParent(GetContentFromTeam(srl.machine.playerTeam));
            tag.transform.localScale = Vector3.one;
            tag.transform.localRotation = Quaternion.identity;
        }

        _timecount.gameObject.SetActive(!_isInAdvancedInterface);
        _advancedTimecount.gameObject.SetActive(_isInAdvancedInterface);
        StartCoroutine(LoopTimer(_isInAdvancedInterface ? _advancedTimecount : _timecount));
        _addMachineBtn.gameObject.SetActive(false);
        _addAdvancedMachineBtn.gameObject.SetActive(false);
        _addOtherMachineBtn.gameObject.SetActive(false);
        if (_endlessSlider != null)
            _endlessSlider.enabled = false;
        if (_advancedEndlessSlider != null)
            _advancedEndlessSlider.enabled = false;
        _startSessionBtn.gameObject.SetActive(false);
        _advancedStartSessionBtn.gameObject.SetActive(false);
        _stopSessionBtn.gameObject.SetActive(true);
        _advancedStopSessionBtn.gameObject.SetActive(true);
        _advancedStopSessionAndKillBtn.gameObject.SetActive(true);
        _greyOptionsImage.gameObject.SetActive(true);
        _advancedGreyOptionsImage.gameObject.SetActive(true);
    }

    IEnumerator LoopTimer(TextMeshProUGUI timer)
    {
        DateTime dt = DateTime.Now;
        while (timer.gameObject.activeInHierarchy)
        {
            DateTime dt2 = DateTime.Now;
            TimeSpan ts = dt2 - dt;
            double miliseconds = ts.TotalMilliseconds;
            double tmp = miliseconds / 1000.0;
            int topnmbr = (int)tmp;
            tmp = miliseconds - (topnmbr*1000.0);
            int _milisec = (int)tmp;
            int sec = topnmbr % 60;
            topnmbr = topnmbr / 60;
            int min = topnmbr % 60;
            topnmbr = topnmbr / 60;
            timer.text = topnmbr.ToString("d2") + ":" + min.ToString("d2") + ":" + sec.ToString("d2") + ":" + _milisec.ToString("d3");
            yield return null;
        }
    }

    public void PressHallOfFame()
    {
        Debug.Log("Name:" + roomName);
        EndSession(false);
        masterrooms.ReActiveRoom(roomName);
        apicalls.GetSessions(_unique_session);
    }

    public string GetInitData(List<machineline> machines)
	{
        if (machines == null)
            return null;

        Dictionary<string, object> dic = new Dictionary<string, object>();
        
        dic["product"] = multiplayerlobby.product.ToString();
        dic["mode"] = _gameMode.ToString();
        dic["duration"] = _endlessGameDuration;
        dic["next"] = _canPlayNextRace;
        dic["theme"] = _theme.ToString();
        dic["level"] = _level;
        dic["wave"] = _startWave;

        List<object> players = new List<object>();
        foreach (machineline machine in machines)
		{
            players.Add(machine.GetPlayerData());
        }
        dic["players"] = players;

        string data = JSON.Serialize(dic);
        Debug.Log("[DATA] " + data);

        return data;
	}
}
