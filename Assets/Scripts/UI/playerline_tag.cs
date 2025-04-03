using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class playerline_tag : MonoBehaviour
{
    [SerializeField]
    private Button _respawnBtn = null;
    [SerializeField]
    private Button _pauseBtn = null;
    [SerializeField]
    private Button _unPauseBtn = null;
    [SerializeField]
    private Text _name = null;
    [SerializeField]
    private TextMeshProUGUI _playerName = null;
    [SerializeField]
    private TextMeshProUGUI _machineName = null;
    [Header("Master")]
    [SerializeField]
    private TextMeshProUGUI _roleName = null;
    [SerializeField]
    private GameObject _masterIcon = null;

    private roomline _rl = null;
    private multiplayerlobby _ml;
    private string _reference;

    public void Init(multiplayerlobby ml, roomline rl, string reference)
	{
        _ml = ml;
        _rl = rl;
        _reference = reference;
    }

    public void SetMachineName(string name)
    {
        if (_name != null)
            _name.text = name;
        if (_machineName != null)
            _machineName.text = name;
    }

    public void SetPlayerName(string name)
	{
        if (_playerName != null)
            _playerName.text = name;
    }

    public void SetMaster(bool master)
    {
        if (_roleName != null)
            _roleName.text = master ? "Master PC" : "Client PC";
        if (_masterIcon != null)
            _masterIcon.SetActive(master);
    }

    private void Start()
	{
        _pauseBtn.onClick.AddListener(ClickPause);
        _unPauseBtn.onClick.AddListener(ClickUnPause);
        _respawnBtn.onClick.AddListener(ClickRespawn);

        _respawnBtn.gameObject.SetActive(multiplayerlobby.product != multiplayerlobby.Product.TOG);
    }

	private void OnDestroy()
	{
        _pauseBtn.onClick.RemoveListener(ClickPause);
        _unPauseBtn.onClick.RemoveListener(ClickUnPause);
        _respawnBtn.onClick.RemoveListener(ClickRespawn);
        _ml = null;
        _rl = null;
    }

    private void ClickPause()
    {
        if (!_ml.gametreatment)
        {
            StartCoroutine(SendInfoToSession("_PAUSE_" + _reference));
            _pauseBtn.gameObject.SetActive(false);
            _unPauseBtn.gameObject.SetActive(true);
        }
    }

    private void ClickUnPause()
    {
        if (!_ml.gametreatment)
        {
            StartCoroutine(SendInfoToSession("_UNPAUSE_" + _reference));
            _pauseBtn.gameObject.SetActive(true);
            _unPauseBtn.gameObject.SetActive(false);
        }
    }

    private void ClickRespawn()
    {
        if (!_ml.gametreatment)
        {
            StartCoroutine(SendInfoToSession("_RESPAWN_" + _reference));
        }
    }

    private IEnumerator SendInfoToSession(string info)
    {
        multiplayerlobby.globalwaiter.SetActive(true);
        //bool first = true;
        yield return null;
        _ml.gametreatment = true;
        Debug.Log("SendInfoToSession true");

        PlayerPrefs.SetInt("PhotonNoSceneLoading", 1);
        PlayerPrefs.Save();
       
        Debug.Log("Trying to join " + _rl.currentgamename + " for "+info);
        _ml.SetNickname(info);
        // join room
        if (!_ml.JoinRoom(_rl.currentgamename))
		{
            Debug.LogError("Cant join room " + _rl.currentgamename);
            multiplayerlobby.globalwaiter.SetActive(false);
            yield break;
        }
        Debug.Log("Requested join room");
        while (!_ml.insideroom) yield return null;
        Debug.Log("Inside room now");
        yield return new WaitForSeconds(0.7f);
        _ml.LeaveRoom();

        Debug.Log("Requested to leave room");

        while (_ml.insideroom) 
            yield return null;

        Debug.Log("Outside room now");

        PlayerPrefs.DeleteKey("PhotonNoSceneLoading");
        PlayerPrefs.Save();

        _ml.SetNickname();
        Debug.Log("SendInfoToSession false");
        _ml.gametreatment = false;

        yield return new WaitForSeconds(0.1f);
        _ml.waitToConnectToMaster = false;
        while (!_ml.waitToConnectToMaster)
            yield return null;

        multiplayerlobby.myself.JoinLobby();
        multiplayerlobby.globalwaiter.SetActive(false);
    }
}
