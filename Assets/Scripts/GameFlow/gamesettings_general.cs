using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class gamesettings_general : MonoBehaviour
{
    public static gamesettings_general myself = null;

    [Tooltip("en-US for english, fr-FR for french, you can see other code in RRCountryCode")]
    public string languageCode = "fr-FR";
    public string languageAudioCode = "fr-FR";
    public AudioMixer mixer;

    [Header("Guardian Voice Over")]
    public GameObject guardian_vo_male = null;
    public GameObject guardian_vo_female = null;

    [Header("Levels")]
    public LevelSettings levelSettings;
    public string levelStart => _sceneNames[0];
    private List<string> _sceneNames = null;

    [Header("CustomTheme")]
    public CustomPackSettings customPackSetting = null;

    [Header("EndScreen - Score")]
    public UI_EndRaceResult.ScoreValueTypeCoef scoreValueTypeCoef = null;
    public float delayToStopGameAfterButtonAppearance = 3f;

    [Header("GameSpeed")]
    [Range(0.0f, 10.0f)]
    public float GameTimeScale = 1.0f;
    float lastGameTimeScale = 0.0f;

    [Header("CheatBoat")]
    public bool CheatBoatBlue = false;
    public bool CheatBoatRed = false;

    [Header("Cabin")]
    public float timerMaxDurationInCabin = 60f;

    [Header("Lobby")]
    public float timerMaxDurationInLobby = 60f;

    [Header("Audio volumes")]
    public float musicVolume = 0.33f;
    public float voicesVolume = 1f;
    public float sfxVolume = 0.66f;

    public float currentMusicVolume => _currentMusicVolume;
    public float currentVoicesVolume => _currentVoicesVolume;
    public float currentSFXVolume => _currentSFXVolume;

    private float _currentMusicVolume = 0.5f;
    private float _lastMusicVolume = 0f;
    private float _currentVoicesVolume = 0.5f;
    private float _lastVoicesVolume = 0f;
    private float _currentSFXVolume = 0.5f;
    private float _fadeInMusicDelay = 0f;

    private void Update()
    {
        if (GameTimeScale != lastGameTimeScale)
        {
            Time.timeScale = GameTimeScale;
            lastGameTimeScale = GameTimeScale;
        }
        if (CheatBoatBlue)
        {
            SetGodModeToBoat(boat_followdummy.TeamColor.Blue);
            CheatBoatBlue = false;
        }
        if (CheatBoatRed)
        {
            SetGodModeToBoat(boat_followdummy.TeamColor.Red);
            CheatBoatRed = false;
        }
    }

    private void SetGodModeToBoat(boat_followdummy.TeamColor team)
    {
        // Give health to boats
        foreach (boat_followdummy boat in FindObjectsOfType<boat_followdummy>())
        {
            if (boat.teamColor == team)
            {
                boat.SetStartingLife(100000);
            }
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        myself = this;
        DontDestroyOnLoad(gameObject);

        languageCode = GameLoader.LoadWrittenLanguage(languageCode);
        LanguageChanged(languageCode, selectflag.LanguageType.Written);
        // Set default audio to written language
        languageAudioCode = GameLoader.LoadAudioLanguage(languageAudioCode);
        if (string.IsNullOrEmpty(languageAudioCode))
            languageAudioCode = languageCode;
        LanguageChanged(languageAudioCode, selectflag.LanguageType.Audio);

        SetVoicesVolume(voicesVolume);
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);

        // Get scenes
        _sceneNames = new List<string>();
        Debug.Log($"[SCENE] scene count: {SceneManager.sceneCountInBuildSettings}");
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; ++i)
		{
            Scene scene = SceneManager.GetSceneByBuildIndex(i);
            if (!string.IsNullOrEmpty(scene.name))
            {
                _sceneNames.Add(scene.name);
                Debug.Log($"[SCENE] {scene.name}");
            }
        }
    }

    private float GetAttenuationFromVolume(float volume)
	{
        float attenuation = Mathf.Log10(Mathf.Lerp(0.0001f, 1f, volume * volume)) * 20f;
        //Debug.Log($"[ATTENUATION] volume {volume} -> attenuation {attenuation}");
        return attenuation;
    }

    public void SetMusicVolume(float volume)
    {
        _currentMusicVolume = volume;
        float attenuation = GetAttenuationFromVolume(volume);
        mixer.SetFloat("musicvol", attenuation);
    }

    public void SetVoicesVolume(float volume)
    {
        _currentVoicesVolume = volume;
        float attenuation = GetAttenuationFromVolume(volume);
        mixer.SetFloat("voicesvol", attenuation);
        mixer.SetFloat("voicesintvol", attenuation);
        mixer.SetFloat("teaservol", attenuation);
    }

    public void SetSFXVolume(float volume)
    {
        _currentSFXVolume = volume;
        float attenuation = GetAttenuationFromVolume(volume);
        mixer.SetFloat("weaponsvol", attenuation);
        mixer.SetFloat("sfxvol", attenuation);
    }

    public void FadeOutMusicVolume(float delay)
    {
        _lastMusicVolume = _currentMusicVolume;
        _lastVoicesVolume = _currentVoicesVolume;
        if (_currentMusicVolume > 0f)
            StartCoroutine(FadeOutMusicVolumeEnum(delay));
    }

    private IEnumerator FadeOutMusicVolumeEnum(float delay)
	{
        _fadeInMusicDelay = delay;
        float ftime = Time.time;
        while (Time.time - ftime < delay)
		{
            float ratio = (Time.time - ftime) / delay;
            float volume = Mathf.Lerp(_lastMusicVolume, 0f, ratio);
            SetMusicVolume(volume);
            volume = Mathf.Lerp(_lastVoicesVolume, 0f, ratio);
            SetVoicesVolume(volume);
            yield return null;
		}
        SetMusicVolume(0f);
        SetVoicesVolume(0f);
    }

    public void FadeInMusicVolume()
	{
        if (_lastMusicVolume > 0f)
            StartCoroutine(FadeInMusicVolumeEnum());
    }

    private IEnumerator FadeInMusicVolumeEnum()
	{
        float ftime = Time.time;
        while (Time.time - ftime < _fadeInMusicDelay)
        {
            float ratio = (Time.time - ftime) / _fadeInMusicDelay;
            float volume = Mathf.Lerp(0f, _lastMusicVolume, ratio);
            SetMusicVolume(volume);
            volume = Mathf.Lerp(0f, _lastVoicesVolume, ratio);
            SetVoicesVolume(volume);
            yield return null;
        }
        SetMusicVolume(_lastMusicVolume);
        SetVoicesVolume(_lastVoicesVolume);
    }

    public static void LanguageChanged(string newcode, selectflag.LanguageType languageType)
    {
        Debug.Log("LanguageChanged: " + newcode + " - " + languageType);

        if (languageType == selectflag.LanguageType.Written || languageType == selectflag.LanguageType.Both)
        {
            RRLib.RRLanguageManager languageManager = RRLib.RRLanguageManager.instance;
            if (string.IsNullOrEmpty(newcode))
            {
                languageManager.SetDefaultLanguage();
                newcode = languageManager.currentLanguage.m_sLanguageCulture;
            }

            languageManager.SetLanguage(newcode);
            for (int nLanguageIndex = 0; nLanguageIndex < languageManager.nLanguageCount; ++nLanguageIndex)
            {
                string sLangCulture = languageManager[nLanguageIndex].m_sLanguageCulture;
                languageManager.AddTextFile(sLangCulture, sLangCulture, "Texts");
            }

            RRLib.RRLanguageManager.instance.ReloadTexts();

            if (myself != null)
                myself.languageCode = newcode;
            
            GameLoader.SaveWrittenLanguage(newcode);
        }
        if (languageType == selectflag.LanguageType.Audio || languageType == selectflag.LanguageType.Both)
		{
            if (myself != null)
                myself.languageAudioCode = newcode;

            GameLoader.SaveAudioLanguage(newcode);
        }
    }
}
