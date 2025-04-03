using UnityEngine;
using UnityEngine.UI;

public class UI_Volumes : MonoBehaviour
{
    [SerializeField]
    private Button _musicBtn = null;
    [SerializeField]
    private Button _voicesBtn = null;
    [SerializeField]
    private Button _sfxBtn = null;
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
    private bool _canSetOff = false;

    private void Start()
	{
        _musicBtn.onClick.AddListener(OnMusicButton);
        _voicesBtn.onClick.AddListener(OnVoicesButton);
        _sfxBtn.onClick.AddListener(OnSFXButton);
    }

	private void OnDestroy()
	{
        _musicBtn.onClick.RemoveAllListeners();
        _voicesBtn.onClick.RemoveAllListeners();
        _sfxBtn.onClick.RemoveAllListeners();
    }

    private void OnMusicButton()
    {
        float volume = gamesettings_general.myself.currentMusicVolume;
        if (SaveManager.myself?.profile != null)
            volume = SaveManager.myself.profile.musicVolume;
        int state = GetVolumeState(_musicStates.Length, volume);
        state = (state + 1) % _musicStates.Length;
        if (!_canSetOff && state == 0)
            state++;
        volume = GetVolumeFromState(state, _musicStates.Length);
        if (SaveManager.myself?.profile != null)
        {
            SaveManager.myself.profile.musicVolume = volume;
            SaveManager.myself.Save();
        }
        SetMusicVolume(volume);
    }

    private void OnVoicesButton()
    {
        float volume = gamesettings_general.myself.currentVoicesVolume;
        if (SaveManager.myself?.profile != null)
            volume = SaveManager.myself.profile.voicesVolume;
        int state = GetVolumeState(_voicesStates.Length, volume);
        state = (state + 1) % _voicesStates.Length;
        if (!_canSetOff && state == 0)
            state++;
        volume = GetVolumeFromState(state, _voicesStates.Length);
        if (SaveManager.myself?.profile != null)
        {
            SaveManager.myself.profile.voicesVolume = volume;
            SaveManager.myself.Save();
        }
        SetVoicesVolume(volume);
        _voicesTestAudio.PlayVoiceOver();
    }

    private void OnSFXButton()
    {
        float volume = gamesettings_general.myself.currentSFXVolume;
        if (SaveManager.myself?.profile != null)
            volume = SaveManager.myself.profile.sfxVolume;
        int state = GetVolumeState(_sfxStates.Length, volume);
        state = (state + 1) % _sfxStates.Length;
        if (!_canSetOff && state == 0)
            state++;
        volume = GetVolumeFromState(state, _sfxStates.Length);
        if (SaveManager.myself?.profile != null)
        {
            SaveManager.myself.profile.sfxVolume = volume;
            SaveManager.myself.Save();
        }
        SetSFXVolume(volume);
        _sfxTestAudio.Play();
    }

    public void SetMusicVolume(float volume)
    {
        int state = GetVolumeState(_musicStates.Length, volume);
        for (int i = 0; i < _musicStates.Length; ++i)
        {
            _musicStates[i].SetActive(i == state);
        }
        gamesettings_general.myself.SetMusicVolume(volume);
    }

    public void SetVoicesVolume(float volume)
    {
        int state = GetVolumeState(_voicesStates.Length, volume);
        for (int i = 0; i < _voicesStates.Length; ++i)
        {
            _voicesStates[i].SetActive(i == state);
        }
        gamesettings_general.myself.SetVoicesVolume(volume);
    }

    public void SetSFXVolume(float volume)
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
}
