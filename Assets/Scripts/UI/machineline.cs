using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AllMyScripts.Common.Version;
using System.Collections.Generic;
using RRLib;

public class machineline : MonoBehaviour
{
    public int playerTeam => _playerTeam;
    public string playerName => string.IsNullOrEmpty(_playerName) ? "" : _playerName;

    public string reference = "";
    public string machineId = "";
    public string machineName = "";
    public string productName = "";
    public string version = "";

    private multiplayerlobby _ml = null;
    private masterrooms _mr = null;
    
    public GameObject myselection = null;
    [SerializeField]
    private TextMeshProUGUI _version = null;
    [SerializeField]
    private Button _addBtn = null;
    [SerializeField]
    private GameObject _goOldVersion = null;
    [SerializeField]
    private string _minimumAvailableVersion = null;
    [SerializeField]
    private TMP_InputField _input = null;

    private int _playerTeam = 0;
    private string _playerName = null;
    private int _playerId = 0;

    public void Init(multiplayerlobby ml, masterrooms mr) 
	{
        _ml = ml;
        _mr = mr;
    }

    public void SetPlayerTeam(int team)
	{
        _playerTeam = team;
	}

    public void SetPlayerName(string name)
	{
        _playerName = name;
	}

    public void SetPlayerId(int id)
    {
        _playerId = id;
    }

    public void SetMachineName(string name)
	{
        _input.text = name;
    }

    public string GetMachineName()
    {
        return _input.text;
    }

    public string GetFinalPlayerName()
	{
        return string.IsNullOrEmpty(_playerName) ? GetMachineName() : _playerName;
    }

    public void RemoveFromSession()
	{
        _mr.RemoveFromSelectedSession(this);
	}

    public void ChangeTeam()
	{
        if (_playerTeam == 0 && _ml.SelectedGame.playerCountTeamB >= _ml.SelectedGame.playerCountMaxByTeam)
            return;
        if (_playerTeam == 1 && _ml.SelectedGame.playerCountTeamA >= _ml.SelectedGame.playerCountMaxByTeam)
            return;
        _playerTeam = 1 - _playerTeam;
        _mr.UpdateMe();
    }

    public IEnumerator Start()
    {
        while (_ml.setup == null) 
            yield return null;
          
        if (_ml.setup[machineId] == null)
        {
            _ml.setup[machineId] = GetNewName();
            _ml.SaveSetup();
        }
        _input.text = _ml.GetMyName(machineId);
        _version.text = "v " + version;
        bool availableVersion = Version.CompareVersions(_minimumAvailableVersion, version) >= 0;
        _addBtn.interactable = availableVersion;
        _addBtn.gameObject.SetActive(availableVersion);
        _goOldVersion.SetActive(!availableVersion);
        _input.onEndEdit.AddListener(OnNameChanged);
    }

	private void OnDestroy()
	{
        _input.onEndEdit.RemoveListener(OnNameChanged);
    }

	private string GetNewName()
	{
        string newName;
        if (!string.IsNullOrEmpty(machineName) && machineName != "$")
        {
            newName = machineName;
        }
        else
        {
            int machinecount = 1;
            if (PlayerPrefs.HasKey("machinecount"))
            {
                machinecount = PlayerPrefs.GetInt("machinecount");
            }
            newName = apicalls.id_salle + "_PC" + machinecount;
            machinecount++;
            PlayerPrefs.SetInt("machinecount", machinecount);
            PlayerPrefs.Save();
        }
        return newName;
    }

    private void OnNameChanged(string newName)
	{
        if (string.IsNullOrEmpty(newName))
        {
            newName = GetNewName();
            _input.text = newName;
        }
        Debug.Log("Rename " + newName);
        _ml.setup[machineId] = newName;
        _ml.SaveSetup();
    }

    // Called by Add Button
    public void AddToRoom()
    {
        Debug.Log("ADD TO ROOM " + reference);
        if (apicalls.isDemoGame)
        {
            Debug.Log("DEMO ROOM");
            if (!_mr.VerifyMaxPlayers(4))
            {
                _mr.ShowError(RRLanguageManager.instance.GetString("str_launcher_demogame"));
                return;
            }
        }
        if (_mr.selectedsession == -1)
        {
            _mr.ShowError(RRLanguageManager.instance.GetString("str_launcher_gameselectfirst"));
            return;
        }
        if (_ml.SelectedGame.currentTeam == 0 && _ml.SelectedGame.playerCountTeamA >= _ml.SelectedGame.playerCountMaxByTeam)
		{
            return;
        }
        if (_ml.SelectedGame.currentTeam == 1 && _ml.SelectedGame.playerCountTeamB >= _ml.SelectedGame.playerCountMaxByTeam)
        {
            return;
        }

        SetPlayerTeam(_ml.SelectedGame.currentTeam);

        _mr.AddToSelectedSession(this);
        _mr.CreateRoomLineFromMachine(_ml.SelectedGame, this);
    }

    public void DeleteFromRoom()
    {
        Debug.Log("[MACHINELINE] DeleteFromRoom " + reference);
        if (myselection != null)
        {
            selectedroomline srl = myselection.GetComponent<selectedroomline>();
            srl.Remove();
        }
    }

    public Dictionary<string, object> GetPlayerData()
	{
        Dictionary<string, object> dic = new Dictionary<string, object>();
        dic["id"] = _playerId;
        dic["name"] = GetFinalPlayerName();
        dic["machine"] = GetMachineName();
        dic["team"] = _playerTeam;
        return dic;
    }
}
