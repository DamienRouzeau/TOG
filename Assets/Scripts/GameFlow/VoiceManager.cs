using UnityEngine;
using Dissonance;

public class VoiceManager : MonoBehaviour
{

    public static VoiceManager myself => _myself;
    private static VoiceManager _myself = null;

    [SerializeField]
    private DissonanceComms _dissonanceComms = null;
    [SerializeField]
    private VoiceBroadcastTrigger _voiceInBlue = null;
    [SerializeField]
    private VoiceBroadcastTrigger _voiceInRed = null;
    [SerializeField]
    private VoiceReceiptTrigger _voiceOutBlue = null;
    [SerializeField]
    private VoiceReceiptTrigger _voiceOutRed = null;

    private string _roomName = null;
    private string _roomNameRed = null;

    // Start is called before the first frame update
    void Awake()
    {
        _myself = this;
        DontDestroyOnLoad(gameObject);
        Logs.SetLogLevel(LogCategory.Core, LogLevel.Error);
        Logs.SetLogLevel(LogCategory.Network, LogLevel.Error);
        Logs.SetLogLevel(LogCategory.Playback, LogLevel.Error);
        Logs.SetLogLevel(LogCategory.Recording, LogLevel.Error);
    }

    private void ClearTokens()
	{
        if (_roomName != null)
		{
            if (_dissonanceComms.ContainsToken(_roomName))
                _dissonanceComms.RemoveToken(_roomName);
            if (_dissonanceComms.ContainsToken(_roomNameRed))
                _dissonanceComms.RemoveToken(_roomNameRed);
            if (_voiceInBlue.ContainsToken(_roomName))
                _voiceInBlue.RemoveToken(_roomName);
            if (_voiceOutBlue.ContainsToken(_roomName))
                _voiceOutBlue.RemoveToken(_roomName);
            if (_voiceInRed.ContainsToken(_roomNameRed))
                _voiceInRed.RemoveToken(_roomNameRed);
            if (_voiceOutRed.ContainsToken(_roomNameRed))
                _voiceOutRed.RemoveToken(_roomNameRed);
        }
    }

    public void InitWithPhotonRoom(string roomName)
	{
        Debug.Log("InitWithPhotonRoom " + roomName);
        ClearTokens();
        _roomName = roomName;
        _roomNameRed = _roomName + "Red";
        _dissonanceComms.AddToken(_roomName);
        _voiceInBlue.AddToken(_roomName);
        _voiceInRed.AddToken(_roomNameRed);
        _voiceOutBlue.AddToken(_roomName);
        _voiceOutRed.AddToken(_roomNameRed);
    }

    public void GoToRedVoiceRoom()
	{
        if (_roomName != null)
        {
            _dissonanceComms.RemoveToken(_roomName);
            _dissonanceComms.AddToken(_roomNameRed);
        }
    }

    public void ReturnToGlobalVoiceRoom()
    {
        if (_roomName != null)
        {
            _dissonanceComms.RemoveToken(_roomNameRed);
            _dissonanceComms.AddToken(_roomName);
        }
    }
}
