using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using System.Globalization;

public class UI_ProfileManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _profileRoot = null;
    [SerializeField]
    private GameObject _audioRoot = null;
    [SerializeField]
    private GameObject _languageRoot = null;
    [SerializeField]
    private GameObject _customRoot = null;
    [SerializeField]
    private UI_ProfileManager_Multi _multiRoot = null;
    [SerializeField]
    private Animator _animator = null;
    [SerializeField]
    private GameObject _playerDataRoot = null;
    [SerializeField]
    private TextMeshProUGUI _playerName = null;
    [SerializeField]
    private GameObject[] _playerAvatar = null;
    [SerializeField]
    private GameObject _playerCoinRoot = null;
    [SerializeField]
    private GameObject _playerSkullRoot = null;
    [SerializeField]
    private TextMeshProUGUI _playerCoinCount = null;
    [SerializeField]
    private TextMeshProUGUI _playerSkullCount = null;
    [SerializeField]
    private GameObject _bottomButtonsRoot = null;
    [SerializeField]
    private GameObject _prevButtonRoot = null;
    [SerializeField]
    private GameObject _nextButtonRoot = null;
    [SerializeField]
    private GameObject _keyboard = null;
    [SerializeField]
    private Button _deleteBtn = null;
    [SerializeField]
    private Button _editBtn = null;
    [SerializeField]
    private Button _loadBtn = null;
    [SerializeField]
    private Button _newBtn = null;
    [SerializeField]
    private Button _prevBtn = null;
    [SerializeField]
    private Button _nextBtn = null;
    [SerializeField]
    private Button _validLanguageBtn = null;
    [SerializeField]
    private Button _musicBtn = null;
    [SerializeField]
    private Button _voicesBtn = null;
    [SerializeField]
    private Button _sfxBtn = null;
    [SerializeField]
    private Button _validProfileBtn = null;
    [SerializeField]
    private Button _cancelProfileBtn = null;
    [SerializeField]
    private UI_ChooseYourName _chooseYourName = null;
    [SerializeField]
    private GameObject _chooseAvatars = null;
    [SerializeField]
    private GameObject[] _avatarChecks = null;
    [SerializeField]
    private Button[] _chooseAvatarsButtons = null;
    [SerializeField]
    private GameObject[] _musicStates = null;
    [SerializeField]
    private GameObject[] _voicesStates = null;
    [SerializeField]
    private GameObject[] _sfxStates = null;
    [SerializeField]
    private VoiceOver _voicesTestAudio = null;
    [SerializeField]
    private AudioSource _sfxTestAudio = null;
    [SerializeField]
    private GameObject _magicMirror = null;
    [Header("Delete popup")]
    [SerializeField]
    private GameObject _popupDeleteProfile = null;
    [SerializeField]
    private Button _popupDeleteProfileAcceptButton = null;
    [SerializeField]
    private Button _popupDeleteProfileCancelButton = null;
    [Header("Load popup")]
    [SerializeField]
    private GameObject _popupLoadProfile = null;
    [SerializeField]
    private Button _popupLoadProfileAcceptButton = null;
    [SerializeField]
    private Button _popupLoadProfileCancelButton = null;
    [Header("Voice Overs")]
    [SerializeField]
    private GameObject _voiceNoProfile = null;
    [SerializeField]
    private GameObject _voiceReadyToPlay = null;
    [SerializeField]
    private VoiceOver _voiceShootOnFrame = null;
    [Header("Levels")]
    [SerializeField]
    private GameObject[] _levels = null;
    [Header("Menu buttons")]
    [SerializeField]
    private GameObject _menuRoot = null;
    [SerializeField]
    private Button _profileBtn = null;
    [SerializeField]
    private Button _playBtn = null;
    [SerializeField]
    private Button _multiBtn = null;
    [SerializeField]
    private Button _optionsBtn = null;
    [SerializeField]
    private Button _customBtn = null;
    [SerializeField]
    private Color _selectedMenuIconsColor = Color.green;
    [SerializeField]
    private Color _idleMenuIconsColor = Color.white;
    [SerializeField]
    private Image _profileIcon = null;
    [SerializeField]
    private Image _playIcon = null;
    [SerializeField]
    private Image _optionsIcon = null;
    [SerializeField]
    private Image _customIcon = null;
    [SerializeField]
    private Image _multiIcon = null;
    [SerializeField]
    private GameObject _profileButtonFX = null;
    [SerializeField]
    private GameObject _playButtonFX = null;
    [SerializeField]
    private GameObject _optionsButtonFX = null;
    [SerializeField]
    private GameObject _customButtonFX = null;
    [SerializeField]
    private GameObject _multiButtonFX = null;
    [SerializeField]
    private GameObject _multiChooseLevel = null;
    [SerializeField]
    private GameObject _multiWaitLevelChoosen = null;
    [Header("Customization")]
    [SerializeField]
    private CustomPackSettings _customPackSettings = null;
    [SerializeField]
    private TextMeshProUGUI _customPlayerName = null;
    [SerializeField]
    private GameObject[] _customPlayerAvatar = null;
    [SerializeField]
    private TextMeshProUGUI _customPlayerCoinCount = null;
    [SerializeField]
    private TextMeshProUGUI _customPlayerSkullCount = null;
    [SerializeField]
    private TextMeshProUGUI _customFeatureCost = null;
    [SerializeField]
    private TextMeshProUGUI _customPackCost = null;
    [SerializeField]
    private Button _customUnlockFeatureBtn = null;
    [SerializeField]
    private Button _customUnlockPackBtn = null;
    [SerializeField]
    private GameObject _featureCustomizationPanel = null;
    [SerializeField]
    private GameObject[] _packCustomizationPanels = null;
    [SerializeField]
    private GameObject _packCustomizationUnlockedTilde = null;
    [SerializeField]
    private GameObject _mirrorCutomization = null;
    [SerializeField]
    private Button _packCustomizationPrevButton = null;
    [SerializeField]
    private Button _packCustomizationNextButton = null;
    [SerializeField]
    private UI_CustomItem[] _customItems = null;
    [Header("CustomUnlock feature popup")]
    [SerializeField]
    private GameObject _popupCustomUnlockFeature = null;
    [SerializeField]
    private Button _popupCustomUnlockFeatureAcceptButton = null;
    [SerializeField]
    private Button _popupCustomUnlockFeatureCancelButton = null;
    [Header("CustomUnlock pack popup")]
    [SerializeField]
    private GameObject _popupCustomUnlockPack = null;
    [SerializeField]
    private Button _popupCustomUnlockPackAcceptButton = null;
    [SerializeField]
    private Button _popupCustomUnlockPackCancelButton = null;

    private int _currentProfileIdx = 0;
    private int _lastProfileIdx = 0;
    private string _currentName = "Player 1";
    private bool _createFirstProfile = false;
    private bool _createNewProfile = false;
    private int _currentFeature = 0;
    private int _currentPack = 0;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        _animator.enabled = false;
        _popupDeleteProfile.SetActive(false);
        _popupLoadProfile.SetActive(false);
        _magicMirror.SetActive(false);
        
        while (!GameLoader.myself.isInit)
            yield return null;

        _currentProfileIdx = SaveManager.myself.profileIdx;

        bool existProfile = SaveManager.myself.profileCount > 0;
        _profileRoot.SetActive(false);
        _audioRoot.SetActive(false);
        _customRoot.SetActive(false);
        _multiRoot.gameObject.SetActive(false);
        _languageRoot.SetActive(!existProfile);
        _menuRoot.SetActive(existProfile);
        ShowLevels(existProfile);

        if (existProfile)
        {
            ShowCurrentProfile(true);
            UpdateLevels();
            UpdateAudio();
            OnMenuPlayButton();
        }

        _deleteBtn.onClick.AddListener(OnDeleteButton);
        _editBtn.onClick.AddListener(OnEditButton);
        _loadBtn.onClick.AddListener(OnLoadButton);
        _newBtn.onClick.AddListener(OnNewButton);
        _prevBtn.onClick.AddListener(OnPrevButton);
        _nextBtn.onClick.AddListener(OnNextButton);
        _validLanguageBtn.onClick.AddListener(OnValidLanguageButton);
        _musicBtn.onClick.AddListener(OnMusicButton);
        _voicesBtn.onClick.AddListener(OnVoicesButton);
        _sfxBtn.onClick.AddListener(OnSFXButton);
        _popupDeleteProfileAcceptButton.onClick.AddListener(OnAcceptDeleteProfileButton);
        _popupDeleteProfileCancelButton.onClick.AddListener(OnCancelDeleteProfileButton);
        _popupLoadProfileAcceptButton.onClick.AddListener(OnAcceptLoadProfileButton);
        _popupLoadProfileCancelButton.onClick.AddListener(OnCancelLoadProfileButton);
        _profileBtn.onClick.AddListener(OnMenuProfileButton);
        _playBtn.onClick.AddListener(OnMenuPlayButton);
        _multiBtn.onClick.AddListener(OnMenuMultiButton);
        _optionsBtn.onClick.AddListener(OnMenuOptionsButton);
        _customBtn.onClick.AddListener(OnMenuCustomButton);
        _validProfileBtn.onClick.AddListener(OnValidProfileButton);
        _cancelProfileBtn.onClick.AddListener(OnCancelProfileButton);
        _customUnlockFeatureBtn.onClick.AddListener(OnUnlockFeatureButton);
        _customUnlockPackBtn.onClick.AddListener(OnUnlockPackButton);
        _packCustomizationPrevButton.onClick.AddListener(OnPrevPackButton);
        _packCustomizationNextButton.onClick.AddListener(OnNextPackButton);
        _popupCustomUnlockFeatureAcceptButton.onClick.AddListener(OnAcceptCustomUnlockFeatureButton);
        _popupCustomUnlockFeatureCancelButton.onClick.AddListener(OnCancelCustomUnlockFeatureButton);
        _popupCustomUnlockPackAcceptButton.onClick.AddListener(OnAcceptCustomUnlockPackButton);
        _popupCustomUnlockPackCancelButton.onClick.AddListener(OnCancelCustomUnlockPackButton);
        _chooseYourName.onNameValidatedAction += OnNameValidated;
        _chooseYourName.onNameChangedAction += OnNameChanged;
        for (int i = 0; i < _chooseAvatarsButtons.Length; ++i)
        {
            int avatar = i;
            _chooseAvatarsButtons[i].onClick.AddListener(() => OnAvatarChoosen(avatar));
        }
        for (int i = 0; i < _customItems.Length; ++i)
		{
            UI_CustomItem customItem = _customItems[i];
            customItem.itemButton.onClick.AddListener(() => OnCustomItemChoosen(customItem));
        }
        _multiRoot.onPlayStartAction += OnMultiStart;
        _multiRoot.onJoinOrCreateRoomAction += OnJoinOrCreateRoom;
        _multiRoot.onCancelMulti += OnCancelMulti;
        gameflowmultiplayer.onPlayerEventDelegate += OnPlayerEvent;
    }

	private void OnDestroy()
	{
        _multiRoot.onPlayStartAction -= OnMultiStart;
        _multiRoot.onJoinOrCreateRoomAction -= OnJoinOrCreateRoom;
        _multiRoot.onCancelMulti -= OnCancelMulti;
        gameflowmultiplayer.onPlayerEventDelegate -= OnPlayerEvent;
        _deleteBtn.onClick.RemoveAllListeners();
        _editBtn.onClick.RemoveAllListeners();
        _loadBtn.onClick.RemoveAllListeners();
        _newBtn.onClick.RemoveAllListeners();
        _prevBtn.onClick.RemoveAllListeners();
        _nextBtn.onClick.RemoveAllListeners();
        _validLanguageBtn.onClick.RemoveAllListeners();
        _musicBtn.onClick.RemoveAllListeners();
        _voicesBtn.onClick.RemoveAllListeners();
        _sfxBtn.onClick.RemoveAllListeners();
        _popupDeleteProfileAcceptButton.onClick.RemoveAllListeners();
        _popupDeleteProfileCancelButton.onClick.RemoveAllListeners();
        _popupLoadProfileAcceptButton.onClick.RemoveAllListeners();
        _popupLoadProfileCancelButton.onClick.RemoveAllListeners();
        _profileBtn.onClick.RemoveAllListeners();
        _playBtn.onClick.RemoveAllListeners();
        _multiBtn.onClick.RemoveAllListeners();
        _optionsBtn.onClick.RemoveAllListeners();
        _customBtn.onClick.RemoveAllListeners();
        _validProfileBtn.onClick.RemoveAllListeners();
        _cancelProfileBtn.onClick.RemoveAllListeners();
        _customUnlockFeatureBtn.onClick.RemoveAllListeners();
        _customUnlockPackBtn.onClick.RemoveAllListeners();
        _packCustomizationPrevButton.onClick.RemoveAllListeners();
        _packCustomizationNextButton.onClick.RemoveAllListeners();
        _popupCustomUnlockFeatureAcceptButton.onClick.RemoveAllListeners();
        _popupCustomUnlockFeatureCancelButton.onClick.RemoveAllListeners();
        _popupCustomUnlockPackAcceptButton.onClick.RemoveAllListeners();
        _popupCustomUnlockPackCancelButton.onClick.RemoveAllListeners();
        _chooseYourName.onNameValidatedAction -= OnNameValidated;
        _chooseYourName.onNameChangedAction -= OnNameChanged;
        for (int i = 0; i < _chooseAvatarsButtons.Length; ++i)
        {
            _chooseAvatarsButtons[i].onClick.RemoveAllListeners();
        }
        for (int i = 0; i < _customItems.Length; ++i)
        {
            _customItems[i].itemButton.onClick.RemoveAllListeners();
        }
    }

    private void ShowLevels(bool show)
    {
        if (_levels != null)
        {
            for (int i = 0; i < _levels.Length; ++i)
            {
                _levels[i].SetActive(show);
            }
        }
    }

    private void ShowKeyboard(bool show)
	{
        _keyboard.SetActive(show);
        Debug.Log("ShowKeyboard " + show);
    }

	private void ShowCurrentProfile(bool forceSkin = false)
	{
        if (SaveManager.myself.profileCount == 0)
        {
            CreateProfile();
            return;
        }
        SaveManager.ProfileData data = SaveManager.myself.GetProfile(_currentProfileIdx);
        Debug.Assert(data != null, $"Profile {_currentProfileIdx} not found!");
        ShowKeyboard(false);
        _chooseAvatars.SetActive(false);
        _playerDataRoot.SetActive(true);
        _bottomButtonsRoot.SetActive(true);
        _prevButtonRoot.SetActive(_currentProfileIdx > 0);
        _nextButtonRoot.SetActive(_currentProfileIdx < SaveManager.myself.profileCount - 1);
        bool isOnCurrentProfile = _currentProfileIdx == SaveManager.myself.profileIdx;
        _deleteBtn.gameObject.SetActive(isOnCurrentProfile);
        _editBtn.gameObject.SetActive(isOnCurrentProfile);
        _newBtn.gameObject.SetActive(isOnCurrentProfile);
        _loadBtn.gameObject.SetActive(!isOnCurrentProfile);
        int unlockedSkulls = data.progression.GetUnlockedSkulls();
        string profileName = data.name;
        string profileCoints = data.coins.ToString();
        string profileSkulls = unlockedSkulls.ToString();
        GameflowBase.myPirateName = profileName;
        _playerName.text = profileName;
        _customPlayerName.text = profileName;
        _playerCoinCount.text = profileCoints;
        _customPlayerCoinCount.text = profileCoints;
        _playerSkullCount.text = profileSkulls;
        _customPlayerSkullCount.text = profileSkulls;
        SetCurrentAvatar(data.avatar);
        _customBtn.interactable = unlockedSkulls > 0;
        _multiBtn.interactable = unlockedSkulls > 0;
        if (forceSkin)
        {
            Player.myplayer.ForceSkin(data.avatar, true);
            Player.myplayer.SetCustomHat(data.custom.hat, _customPackSettings);
        }
    }

    private void CreateProfile()
	{
        _customBtn.interactable = false;
        _profileRoot.SetActive(true);
        _audioRoot.SetActive(false);
        _customRoot.SetActive(false);
        _multiRoot.gameObject.SetActive(false);
        ShowKeyboard(false);
        _chooseAvatars.SetActive(false);
        _prevButtonRoot.SetActive(false);
        _nextButtonRoot.SetActive(false);
        _playerDataRoot.SetActive(true);
        SaveManager.myself.NewProfile();
        _currentProfileIdx = 0;
        GameflowBase.myPirateName = GetDefaultNameForProfileIdx(_currentProfileIdx);
        SetEditionMode(true, true);
        OnAvatarChoosen(0);
        _profileButtonFX.SetActive(true);
    }

    private void SetMusicVolume(float volume)
	{
        int state = GetVolumeState(_musicStates.Length, volume);
        for (int i = 0; i < _musicStates.Length; ++i)
		{
            _musicStates[i].SetActive(i == state);
        }
        gamesettings_general.myself.SetMusicVolume(volume);
    }

    private void SetVoicesVolume(float volume)
	{
        int state = GetVolumeState(_voicesStates.Length, volume);
        for (int i = 0; i < _voicesStates.Length; ++i)
        {
            _voicesStates[i].SetActive(i == state);
        }
        gamesettings_general.myself.SetVoicesVolume(volume);
    }

    private void SetSFXVolume(float volume)
    {
        int state = GetVolumeState(_sfxStates.Length, volume);
        for (int i = 0; i < _sfxStates.Length; ++i)
        {
            _sfxStates[i].SetActive(i == state);
        }
        gamesettings_general.myself.SetSFXVolume(volume);
    }

    private float GetVolumeFromState(int state, int stateCount)
	{
        return (float)state / (float)(stateCount - 1);

    }

    private int GetVolumeState(int stateCount, float volume)
	{
        return Mathf.RoundToInt(volume * (stateCount - 1));
    }

    private void SetCurrentAvatar(string avatar)
	{
        int num = gamesettings_player.myself.GetSkinIndexFromName(avatar);
        if (num < 0)
            num = 0;
        for (int i = 0; i < _playerAvatar.Length; ++i)
            _playerAvatar[i].SetActive(num == i);
        for (int i = 0; i < _playerAvatar.Length; ++i)
            _customPlayerAvatar[i].SetActive(num == i);
    }

    private void SetEditionMode(bool edit, bool firstProfile = false, bool newProfile=false)
	{
        if (edit)
        {
            _createFirstProfile = firstProfile;
            _createNewProfile = newProfile;
            if (_createFirstProfile || _createNewProfile)
			{
                _playerCoinRoot.SetActive(false);
                _playerSkullRoot.SetActive(false);
            }
            string name = GameflowBase.myPirateName;
            string defaultName = GetDefaultNameForProfileIdx(_currentProfileIdx);
            _chooseYourName.Init(UI_ChooseYourName.NameType.Player);
            _chooseYourName.SetNameAsDefault(defaultName);
            if (name != defaultName)
                _chooseYourName.SetName(name);
            _prevButtonRoot.SetActive(false);
            _nextButtonRoot.SetActive(false);
            _voiceReadyToPlay.SetActive(false);
            CheckAvatarFromName(GameflowBase.GetMySkinName());
        }
        else
		{
            _playerCoinRoot.SetActive(true);
            _playerSkullRoot.SetActive(true);
        }
        _bottomButtonsRoot.SetActive(!edit);
        ShowKeyboard(edit);
        _chooseAvatars.SetActive(edit);
    }

    private string GetDefaultNameForProfileIdx(int idx)
	{
        return "Player " + (idx + 1);
    }

    private void UpdateLevels()
    {
        UI_CabinLevelSelection[] items = FindObjectsOfType<UI_CabinLevelSelection>();
        foreach (UI_CabinLevelSelection item in items)
        {
            bool show = PhotonNetworkController.soloMode || item.GetLevelIndex() > 0;
            item.gameObject.SetActive(show);
            if (show)
                item.Init();
        }
    }

    private string ConvertBoolArrayToString(bool[] boolArray)
    {
        string result = "";
        for (int i=0; i < boolArray.Length; ++i)
		{
            if (i > 0)
                result += "-";
            if (boolArray[i])
                result += "1";
            else
                result += "0";
        }
        return result;
    }

    private bool[] ConvertStringAsBoolArray(string data)
	{
        bool[] boolArray = null;
        if (!string.IsNullOrEmpty(data))
        {
            string[] split = data.Split('-');
            if (split != null && split.Length > 0)
            {
                int levelCount = split.Length;
                boolArray = new bool[levelCount];
                for (int i = 0; i < levelCount; ++i)
                {
                    boolArray[i] = (split[i] == "1");
                }
            }
        }
        return boolArray;
    }

    private bool[] GetAvailableLevels()
    {
        int levelCount = _levels.Length;
        bool[] result = new bool[levelCount];
        for (int i = 0; i < _levels.Length; ++i)
        {
            UI_CabinLevelSelection item = _levels[i].GetComponent<UI_CabinLevelSelection>();
            result[i] = item.IsAvailable();
        }
        return result;
    }

    private void HideInfoOnLevels(bool[] availableLevels, bool isMaster)
    {
        for (int i = 0; i < _levels.Length; ++i)
		{
            bool available = i < availableLevels.Length && availableLevels[i];
            UI_CabinLevelSelection item = _levels[i].GetComponent<UI_CabinLevelSelection>();
            item.HideInfoAndButton(available, isMaster);
		}
    }

    private void UpdateAudio()
    {
        if (SaveManager.myself != null)
        {
            SaveManager.ProfileData profile = SaveManager.myself.profile;
            if (profile != null)
            {
                SetMusicVolume(profile.musicVolume);
                SetVoicesVolume(profile.voicesVolume);
                SetSFXVolume(profile.sfxVolume);
            }
        }
    }

    private void OnDeleteButton()
	{
        _lastProfileIdx = _currentProfileIdx;
        _popupDeleteProfile.SetActive(true);        
    }
    private void OnEditButton()
	{
        SetEditionMode(true);
    }

    private void OnLoadButton()
	{
        _popupLoadProfile.SetActive(true);
    }

    private void OnNewButton()
	{
        SaveManager.myself.NewProfile();
        _lastProfileIdx = _currentProfileIdx;
        _currentProfileIdx = SaveManager.myself.profileCount - 1;
        OnAcceptLoadProfileButton();
        GameflowBase.myPirateName = GetDefaultNameForProfileIdx(_currentProfileIdx);
        SetEditionMode(true, false, true);
    }

    private void OnPrevButton()
    {
        _currentProfileIdx--;
        ShowCurrentProfile();
    }

    private void OnNextButton()
    {
        _currentProfileIdx++;
        ShowCurrentProfile();
    }

    private void OnValidLanguageButton()
	{
        _profileRoot.SetActive(true);
        _audioRoot.SetActive(true);
        _languageRoot.SetActive(false);
        _customRoot.SetActive(false);
        _multiRoot.gameObject.SetActive(false);
        ShowCurrentProfile();
        UpdateLevels();
        UpdateAudio();
        _voiceNoProfile.SetActive(true);
        OnMenuProfileButton();
        ShowKeyboard(true);
        _menuRoot.SetActive(true);
        _profileButtonFX.SetActive(true);
        _playBtn.interactable = false;
        _multiBtn.interactable = false;
        _optionsBtn.interactable = false;
        _customBtn.interactable = false;
    }

    private void OnMusicButton()
	{
        int state = GetVolumeState(_musicStates.Length, SaveManager.myself.profile.musicVolume);
        state = (state + 1) % _musicStates.Length;
        float volume = GetVolumeFromState(state, _musicStates.Length);
        if (SaveManager.myself != null)
        {
            SaveManager.myself.profile.musicVolume = volume;
            SaveManager.myself.Save();
        }
        SetMusicVolume(volume);
    }

    private void OnVoicesButton()
	{
        int state = GetVolumeState(_voicesStates.Length, SaveManager.myself.profile.voicesVolume);
        state = (state + 1) % _voicesStates.Length;
        float volume = GetVolumeFromState(state, _voicesStates.Length);
        if (SaveManager.myself != null)
        {
            SaveManager.myself.profile.voicesVolume = volume;
            SaveManager.myself.Save();
        }
        SetVoicesVolume(volume);
        _voicesTestAudio.PlayVoiceOver();
    }

    private void OnSFXButton()
	{
        int state = GetVolumeState(_sfxStates.Length, SaveManager.myself.profile.sfxVolume);
        state = (state + 1) % _sfxStates.Length;
        float volume = GetVolumeFromState(state, _sfxStates.Length);
        SaveManager.myself.profile.sfxVolume = volume;
        SaveManager.myself.Save();
        SetSFXVolume(volume);
        _sfxTestAudio.Play();
    }

    private void OnAcceptDeleteProfileButton()
	{
        _popupDeleteProfile.SetActive(false);
        SaveManager.myself.DeleteProfile(_currentProfileIdx);
        int profileCount = SaveManager.myself.profileCount;
        if (profileCount > 0)
        {
            _currentProfileIdx = _lastProfileIdx;
            if (_currentProfileIdx == profileCount)
                _currentProfileIdx--;
            OnAcceptLoadProfileButton();
        }
        else
        {
            StopAllCoroutines();
            _voiceNoProfile.SetActive(false);
            _voiceReadyToPlay.SetActive(false);
            _voiceShootOnFrame.gameObject.SetActive(false);
            _profileRoot.SetActive(false);
            _audioRoot.SetActive(false);
            _customRoot.SetActive(false);
            _multiRoot.gameObject.SetActive(false);
            _languageRoot.SetActive(true);
            _menuRoot.SetActive(false);
            ShowLevels(false);
        }
    }

    private void OnCancelDeleteProfileButton()
	{
        _popupDeleteProfile.SetActive(false);
    }

    private void OnAcceptLoadProfileButton()
    {
        _popupLoadProfile.SetActive(false);
        SaveManager.myself.SetCurrentProfileIndex(_currentProfileIdx);
        SetCurrentAvatar(SaveManager.myself.profile.avatar);
        ShowCurrentProfile(true);
        UpdateLevels();
        UpdateAudio();
    }

    private void OnCancelLoadProfileButton()
    {
        _popupLoadProfile.SetActive(false);
    }

    private void OnMenuProfileButton()
	{
        RestoreSoloMode();
        if (_multiChooseLevel != null)
            _multiChooseLevel.SetActive(false);
        if (_multiWaitLevelChoosen != null)
            _multiWaitLevelChoosen.SetActive(false);
        _profileRoot.SetActive(true);
        _audioRoot.SetActive(false);
        _customRoot.SetActive(false);
        _multiRoot.gameObject.SetActive(false);
        ShowKeyboard(false);
        ShowLevels(false);
        _magicMirror.SetActive(true);
        _profileIcon.color = _selectedMenuIconsColor;
        _playIcon.color = _idleMenuIconsColor;
        _optionsIcon.color = _idleMenuIconsColor;
        _customIcon.color = _idleMenuIconsColor;
        _multiIcon.color = _idleMenuIconsColor;
        StopAllCoroutines();
        _voiceShootOnFrame.gameObject.SetActive(false);
        SetEditionMode(false);
        ShowCurrentProfile(true);
    }

    private void OnMenuPlayButton()
	{
        RestoreSoloMode();
        if (_multiChooseLevel != null)
            _multiChooseLevel.SetActive(false);
        if (_multiWaitLevelChoosen != null)
            _multiWaitLevelChoosen.SetActive(false);
        _profileRoot.SetActive(false);
        _audioRoot.SetActive(false);
        _customRoot.SetActive(false);
        _multiRoot.gameObject.SetActive(false);
        ShowKeyboard(false);
        ShowLevels(true);
        _magicMirror.SetActive(false);
        _profileIcon.color = _idleMenuIconsColor;
        _playIcon.color = _selectedMenuIconsColor;
        _optionsIcon.color = _idleMenuIconsColor;
        _customIcon.color = _idleMenuIconsColor;
        _multiIcon.color = _idleMenuIconsColor;
        _playButtonFX.SetActive(_createFirstProfile);
        _optionsButtonFX.SetActive(false);
        _customButtonFX.SetActive(false);
        _multiButtonFX.SetActive(false);
        UpdateLevels();
        SetEditionMode(false);
        ShowCurrentProfile(true);
        if (_createFirstProfile)
        {
            StartCoroutine(WaitToPlayVoiceOver(_voiceShootOnFrame, 1f));
            _createFirstProfile = false;
        }
        if (_createNewProfile)
		{
            OnAcceptDeleteProfileButton();
            _createNewProfile = false;
        }
        StartCoroutine(PlayVoiceOverAtRegularTime(_voiceShootOnFrame, 30f, 60f));
    }

    private void OnMenuMultiButton()
	{
        if (_multiChooseLevel != null)
            _multiChooseLevel.SetActive(false);
        if (_multiWaitLevelChoosen != null)
            _multiWaitLevelChoosen.SetActive(false);
        PhotonNetworkController.myself?.StartMultiWithLobby();
        _profileRoot.SetActive(false);
        _audioRoot.SetActive(false);
        _customRoot.SetActive(false);
        _multiRoot.gameObject.SetActive(true);
        _multiRoot.ShowStartMenu();
        ShowKeyboard(false);
        ShowLevels(false);
        _magicMirror.SetActive(false);
        _profileIcon.color = _idleMenuIconsColor;
        _playIcon.color = _idleMenuIconsColor;
        _optionsIcon.color = _idleMenuIconsColor;
        _customIcon.color = _idleMenuIconsColor;
        _multiIcon.color = _selectedMenuIconsColor;
        StopAllCoroutines();
        _voiceShootOnFrame.gameObject.SetActive(false);
        SetEditionMode(false);
        ShowCurrentProfile(true);
        _profileBtn.interactable = true;
        _playBtn.interactable = true;
        _multiBtn.interactable = true;
        _optionsBtn.interactable = true;
        _customBtn.interactable = true;
        if (_createNewProfile)
        {
            OnAcceptDeleteProfileButton();
            _createNewProfile = false;
        }
    }

    private void OnMenuOptionsButton()
	{
        RestoreSoloMode();
        if (_multiChooseLevel != null)
            _multiChooseLevel.SetActive(false);
        if (_multiWaitLevelChoosen != null)
            _multiWaitLevelChoosen.SetActive(false);
        _profileRoot.SetActive(false);
        _audioRoot.SetActive(true);
        _customRoot.SetActive(false);
        _multiRoot.gameObject.SetActive(false);
        ShowKeyboard(false);
        ShowLevels(false);
        _magicMirror.SetActive(false);
        _profileIcon.color = _idleMenuIconsColor;
        _playIcon.color = _idleMenuIconsColor;
        _optionsIcon.color = _selectedMenuIconsColor;
        _customIcon.color = _idleMenuIconsColor;
        _multiIcon.color = _idleMenuIconsColor;
        StopAllCoroutines();
        _voiceShootOnFrame.gameObject.SetActive(false);
        SetEditionMode(false);
        ShowCurrentProfile(true);
        if (_createNewProfile)
        {
            OnAcceptDeleteProfileButton();
            _createNewProfile = false;
        }
    }

    private void OnMenuCustomButton()
	{
        RestoreSoloMode();
        if (_multiChooseLevel != null)
            _multiChooseLevel.SetActive(false);
        if (_multiWaitLevelChoosen != null)
            _multiWaitLevelChoosen.SetActive(false);
        _profileRoot.SetActive(false);
        _audioRoot.SetActive(false);
        _customRoot.SetActive(true);
        _multiRoot.gameObject.SetActive(false);
        _magicMirror.SetActive(true);
        ShowKeyboard(false);
        ShowLevels(false);
        _profileIcon.color = _idleMenuIconsColor;
        _playIcon.color = _idleMenuIconsColor;
        _optionsIcon.color = _idleMenuIconsColor;
        _customIcon.color = _selectedMenuIconsColor;
        _multiIcon.color = _idleMenuIconsColor;
        StopAllCoroutines();
        UpdateCustomPanel();
        UpdateCustomItems();
        SetEditionMode(false);
        ShowCurrentProfile(true);
        if (_createNewProfile)
        {
            OnAcceptDeleteProfileButton();
            _createNewProfile = false;
        }
    }

    private void RestoreSoloMode()
	{
        bool wasInMulti = !PhotonNetworkController.soloMode;
        PhotonNetworkController.myself?.StartSolo();
        if (wasInMulti)
            ResetGamePools();
        GameLoader.myself.DestroyVoiceManager();
    }

    private void OnValidProfileButton()
	{
        _chooseYourName.OnNameConfirmed();
        if (_chooseYourName.nameType == UI_ChooseYourName.NameType.Room)
            return;
        if (SaveManager.myself != null && SaveManager.myself.profile != null)
        {
            SaveManager.myself.profile.avatar = GameflowBase.GetMySkinName();
            SaveManager.myself.profile.name = _currentName;
            SaveManager.myself.Save();
        }
        SetEditionMode(false);
        ShowCurrentProfile();
        UpdateAudio();
        if (_createFirstProfile)
        {
            _profileButtonFX.SetActive(false);
            _playBtn.interactable = true;
            _optionsBtn.interactable = true;
            _voiceReadyToPlay.SetActive(true);
            _playButtonFX.SetActive(true);
            OnMenuPlayButton();
        }
        _createNewProfile = false;
    }

    private void OnCancelProfileButton()
    {
        SetEditionMode(false);
        ShowCurrentProfile(true);
        if (_chooseYourName.nameType == UI_ChooseYourName.NameType.Room)
        {
            _multiRoot.ShowStartMenu();
            return;
        }
        if (_createFirstProfile || _createNewProfile)
        {
            OnAcceptDeleteProfileButton();
            _createFirstProfile = false;
            _createNewProfile = false;
        }
    }

    private void OnUnlockFeatureButton()
	{
        _popupCustomUnlockFeature.SetActive(true);
    }

    private void OnUnlockPackButton()
	{
        _popupCustomUnlockPack.SetActive(true);
    }

    private void OnPrevPackButton()
	{
        _currentPack--;
        UpdateCustomPanel();
    }

    private void OnNextPackButton()
	{
        _currentPack++;
        UpdateCustomPanel();
    }

    private void OnAcceptCustomUnlockFeatureButton()
    {
        _popupCustomUnlockFeature.SetActive(false);
        CustomPackSettings.CustomFeature feature = _customPackSettings.GetFeature(_currentFeature);
        if (SaveManager.myself != null)
        {
            SaveManager.myself.profile.UnlockFeature(feature.name, feature.cost);
            SaveManager.myself.Save();
        }
        ShowCurrentProfile();
        UpdateCustomPanel();
        UpdateCustomItems();
    }

    private void OnCancelCustomUnlockFeatureButton()
    {
        _popupCustomUnlockFeature.SetActive(false);
    }

    private void OnAcceptCustomUnlockPackButton()
    {
        _popupCustomUnlockPack.SetActive(false);
        CustomPackSettings.CustomPack pack = _customPackSettings.GetPack(_currentPack);
        if (SaveManager.myself != null)
        {
            SaveManager.myself.profile.UnlockPack(pack.name, pack.cost);
            SaveManager.myself.Save();
        }
        ShowCurrentProfile();
        UpdateCustomPanel();
        UpdateCustomItems();
    }

    private void OnCancelCustomUnlockPackButton()
    {
        _popupCustomUnlockPack.SetActive(false);
    }
    
    private void UpdateCustomPanel()
	{
        CustomPackSettings.CustomFeature feature =_customPackSettings.GetFeature(_currentFeature);
        Debug.Assert(feature != null, "No feature for index " + _currentFeature);
        int featureCost = feature.cost;
        bool featureUnlocked = SaveManager.myself.profile.IsFeatureUnlocked(feature.name);
        _customFeatureCost.text = featureCost.ToString(CultureInfo.GetCultureInfo(RRLib.RRLanguageManager.instance.currentLanguage.m_sLanguageCulture));
        _customUnlockFeatureBtn.interactable = SaveManager.myself.profile.coins >= featureCost;
        int packCount = feature.packNames.Count;
        _packCustomizationPrevButton.gameObject.SetActive(featureUnlocked && _currentPack > 0);
        _packCustomizationNextButton.gameObject.SetActive(featureUnlocked && _currentPack < packCount - 1);
        for (int i = 0; i < packCount; ++i)
            _packCustomizationPanels[i].SetActive(_currentPack == i && featureUnlocked);
        _mirrorCutomization.SetActive(featureUnlocked);
        _featureCustomizationPanel.SetActive(!featureUnlocked);
        string packName = feature.packNames[_currentPack];
        CustomPackSettings.CustomPack pack = _customPackSettings.GetPack(packName);
        Debug.Assert(pack != null);
        int packCost = pack.cost;
        bool packUnlocked = SaveManager.myself.profile.IsPackUnlocked(pack.name);
        _customPackCost.text = packCost.ToString(CultureInfo.GetCultureInfo(RRLib.RRLanguageManager.instance.currentLanguage.m_sLanguageCulture));
        _customUnlockPackBtn.interactable = SaveManager.myself.profile.coins >= packCost;
        _packCustomizationUnlockedTilde.SetActive(packUnlocked && featureUnlocked);
        _customUnlockPackBtn.gameObject.SetActive(!packUnlocked && featureUnlocked);
    }

    private void UpdateCustomItems()
	{
        foreach (UI_CustomItem item in _customItems)
		{
            string itemName = item.itemName;
            bool locked = true;

            if (itemName == "Default")
                continue;

            if (locked)
            {
                CustomPackSettings.CustomFeature feature = _customPackSettings.GetFeature(_currentFeature);
                if (feature != null)
                {
                    string featureName = feature.name;
                    if (SaveManager.myself.profile.custom.IsFeatureUnlocked(featureName))
                    {
                        foreach (string name in feature.itemNames)
						{
                            if (itemName == name)
                            {
                                locked = false;
                                break;
                            }
						}
                        if (locked)
						{
                            int packCount = feature.packNames.Count;
                            for (int i = 0; i < packCount; ++i)
                            {
                                string packName = feature.packNames[i];
                                CustomPackSettings.CustomPack pack = _customPackSettings.GetPack(packName);
                                if (pack != null)
                                {
                                    if (SaveManager.myself.profile.custom.IsPackUnlocked(packName))
                                    {
                                        foreach (string name in pack.itemNames)
                                        {
                                            if (itemName == name)
                                            {
                                                locked = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (!locked)
                                    break;
                            }
                        }
                    }
                }
            }
            item.SetLock(locked);
		}
	}

    private void OnMultiStart()
	{
        _profileRoot.SetActive(false);
        _audioRoot.SetActive(false);
        _customRoot.SetActive(false);
        _multiRoot.gameObject.SetActive(false);
        ShowKeyboard(false);
        ShowLevels(true);
        _magicMirror.SetActive(false);
        _profileIcon.color = _idleMenuIconsColor;
        _playIcon.color = _idleMenuIconsColor;
        _optionsIcon.color = _idleMenuIconsColor;
        _customIcon.color = _idleMenuIconsColor;
        _multiIcon.color = _idleMenuIconsColor;
        _playButtonFX.SetActive(false);
        _optionsButtonFX.SetActive(false);
        _customButtonFX.SetActive(false);
        _multiButtonFX.SetActive(false);
        _profileBtn.interactable = false;
        _playBtn.interactable = false;
        _multiBtn.interactable = false;
        _optionsBtn.interactable = false;
        _customBtn.interactable = false;
        if (_multiChooseLevel != null)
            _multiChooseLevel.SetActive(true);
        if (_multiWaitLevelChoosen != null)
            _multiWaitLevelChoosen.SetActive(false);
        UpdateLevels();
        bool[] availableLevels = GetAvailableLevels();
        HideInfoOnLevels(availableLevels, true);
        gameflowmultiplayer.TriggerPlayerEvent(gameflowmultiplayer.PlayerEvent.MasterMustChooseLevel, ConvertBoolArrayToString(availableLevels));
    }

    private void OnMasterMustChooseLevel(bool[] availableLevels)
    {
        _profileRoot.SetActive(false);
        _audioRoot.SetActive(false);
        _customRoot.SetActive(false);
        _multiRoot.SetRoomLocked();
        _multiRoot.gameObject.SetActive(false);
        ShowKeyboard(false);
        ShowLevels(true);
        _magicMirror.SetActive(false);
        _profileIcon.color = _idleMenuIconsColor;
        _playIcon.color = _idleMenuIconsColor;
        _optionsIcon.color = _idleMenuIconsColor;
        _customIcon.color = _idleMenuIconsColor;
        _multiIcon.color = _idleMenuIconsColor;
        _playButtonFX.SetActive(false);
        _optionsButtonFX.SetActive(false);
        _customButtonFX.SetActive(false);
        _multiButtonFX.SetActive(false);
        _profileBtn.interactable = false;
        _playBtn.interactable = false;
        _multiBtn.interactable = false;
        _optionsBtn.interactable = false;
        _customBtn.interactable = false;
        if (_multiChooseLevel != null)
            _multiChooseLevel.SetActive(false);
        if (_multiWaitLevelChoosen != null)
            _multiWaitLevelChoosen.SetActive(true);
        UpdateLevels();
        HideInfoOnLevels(availableLevels, false);
    }

    private void ResetGamePools()
	{
        if (poolhelper.myself != null && GameLoader.myself != null)
        {
            GameObject.Destroy(poolhelper.myself.gameObject);
            GameObject.Instantiate(GameLoader.myself.poolHelperPrefab);
        }
    }

    private void OnJoinOrCreateRoom(string roomName)
    {
        ResetGamePools();
        GameLoader.myself.InitVoiceManager();
        VoiceManager.myself?.InitWithPhotonRoom(roomName);
    }

    private void OnCancelMulti()
	{
        OnMenuMultiButton();
    }

    private void OnPlayerEvent(gameflowmultiplayer.PlayerEvent pEvent, string data)
	{
        switch (pEvent)
		{
            case gameflowmultiplayer.PlayerEvent.MasterMustChooseLevel:
                OnMasterMustChooseLevel(ConvertStringAsBoolArray(data));
                break;
		}
	}

    private void OnNameValidated(string name)
	{
        switch (_chooseYourName.nameType)
        {
            case UI_ChooseYourName.NameType.Player:
                _currentName = name;
                break;
            case UI_ChooseYourName.NameType.Room:
                break;
        }
    }

    private void OnNameChanged(string name)
	{
        switch (_chooseYourName.nameType)
        {
            case UI_ChooseYourName.NameType.Player:
                break;
            case UI_ChooseYourName.NameType.Room:
                break;
        }
    }

    private IEnumerator WaitToPlayVoiceOver(VoiceOver voice, float delay)
	{
        while (VoiceOver.isPlayingVoice)
            yield return null;
        yield return new WaitForSeconds(delay);
        voice.gameObject.SetActive(true);
        while (VoiceOver.isPlayingVoice)
            yield return null;
        voice.gameObject.SetActive(false);
    }

    private IEnumerator PlayVoiceOverAtRegularTime(VoiceOver voice, float delayMin, float delayMax)
	{
        while (SaveManager.myself.profileCount > 0)
		{
            float delay = Random.Range(delayMin, delayMax);
            yield return WaitToPlayVoiceOver(voice, delay);
        }
	}

    private void OnAvatarChoosen(int numAvatar)
	{
        string avatarName = gamesettings_player.myself.GetSkinName(numAvatar);
        Player.myplayer.ForceSkin(avatarName);
        SetCurrentAvatar(avatarName);
        CheckAvatarFromIndex(numAvatar);
    }

    private void CheckAvatarFromName(string avatarName)
    {
        CheckAvatarFromIndex(gamesettings_player.myself.GetSkinIndexFromName(avatarName));
    }

    private void CheckAvatarFromIndex(int numAvatar)
	{
        for (int i = 0; i < _avatarChecks.Length; ++i)
        {
            _avatarChecks[i].SetActive(numAvatar == i);
        }
    }

    private void OnCustomItemChoosen(UI_CustomItem customItem)
	{
        string itemName = customItem.itemName;
        if (itemName == "Default")
		{
            // Default
            multiplayerlobby.theme = multiplayerlobby.SkinTheme.Normal;
            Player.myplayer.UpdateCustomTheme();
            SaveManager.myself.profile.custom.hat = null;
            SaveManager.myself.Save();
        }
        else
		{
            multiplayerlobby.theme = multiplayerlobby.SkinTheme.Custom;
            CustomPackSettings.CustomItem item = _customPackSettings.GetItem(itemName);
            Player.myplayer.UpdateCustomTheme(item.prefab);
            SaveManager.myself.profile.custom.hat = itemName;
            SaveManager.myself.Save();
        }
	}
}
