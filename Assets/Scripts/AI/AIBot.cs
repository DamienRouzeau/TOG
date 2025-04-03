#define DEBUG_BOT

using UnityEngine;
using TMPro;
using Photon.Pun;
using System.Collections;

public class AIBot : MonoBehaviour
{
    public enum BotState
	{
        None,
        Init,
        ChooseName,
        NameChoosen,
        ChooseLevel,
        LevelChoosen,
        GoToLobby,
        ValidateTeam,
        LoadRace,
        StartRace,
        EndGame,
        WaitLoadScene,
        Reset
	}

    [SerializeField]
    private TextMeshProUGUI _stateUI = null;
    [SerializeField]
    private TextMeshProUGUI _gameFlowStateUI = null;
    [SerializeField]
    private TextMeshProUGUI _botNameUI = null;
    [SerializeField]
    private TextMeshProUGUI _masterUI = null;

    public BotState botState => _botState;
    private BotState _botState = BotState.None;

    public int team => _team;

    private gameflowmultiplayer.GameState _flowState = gameflowmultiplayer.GameState.Cabin;
    private float _startStateTime = 0f;
    private bool _isInPause = false;
    private gameflowmultiplayer _gameFlow = null;
    private GameflowKDK _gameFlowKDK = null;
    private GameflowBOD _gameFlowBOD = null;
    private int _team;

    // Start is called before the first frame update
    void Awake()
    {
        PlayerPrefs.SetInt("PhotonNoSceneLoading", 1);
        PlayerPrefs.Save();
        SetState(BotState.Init);
    }

	private void Start()
	{
        if (_gameFlow == null)
            _gameFlow = gameflowmultiplayer.myself;
    }

	// Update is called once per frame
	void Update()
    {
        if (_startStateTime > 0f)
        {
            if (Time.time - _startStateTime < 3f)
            {
                return;
            }
            _startStateTime = 0f;
        }

        if (_botState == BotState.Reset)
        {
            GameLoader.myself.LoadAllPrefabs();
            SetState(BotState.Init);
            return;
        }

        if (_botState == BotState.Init)
		{
            if (_gameFlow != null)
                SetState(BotState.ChooseName);
            return;
        }

        if (_gameFlow == null)
            return;

        if (_flowState != _gameFlow.gameState)
            UpdateTexts();

        switch (_botState)
        {
            case BotState.Init:
                if (_gameFlow != null)
                    SetState(BotState.ChooseName);
                break;
            case BotState.ChooseName:
                if (GameflowBase.isBotScene)
                {
                    GameflowBase.myPirateName = "Bot_" + (PhotonNetworkController.GetPlayerId() + 1).ToString("000");
                    Player.myplayer.UpdateName();
                }
                SetState(BotState.NameChoosen);
                break;
            case BotState.NameChoosen:
                if (_gameFlow.gameState == gameflowmultiplayer.GameState.ChooseLevel)
                    SetState(BotState.ChooseLevel);
                break;
            case BotState.ChooseLevel:
                if (gameflowmultiplayer.isBotScene && PhotonNetworkController.IsMaster())
                    gameflowmultiplayer.levelToLaunch = "StarterHub_Poseidon";
                if (_gameFlow.gameState == gameflowmultiplayer.GameState.LevelValidated)
                    SetState(BotState.LevelChoosen);
                break;
            case BotState.LevelChoosen:
                if (_gameFlow.gameState >= gameflowmultiplayer.GameState.GoToLobby &&
                    _gameFlow.gameState <= gameflowmultiplayer.GameState.TeamValidated)
                {
                    if (gameflowmultiplayer.isBotScene)
                        _gameFlow.OnSceneGoToLobby();
                    SetState(BotState.GoToLobby);
                }
                break;
            case BotState.GoToLobby:
                if (_gameFlow.gameState == gameflowmultiplayer.GameState.TeamValidated)
                    SetState(BotState.ValidateTeam);
                break;
            case BotState.ValidateTeam:
                if (_gameFlow.gameState == gameflowmultiplayer.GameState.ShowRaceWorld)
                    SetState(BotState.LoadRace);
                break;
            case BotState.LoadRace:
                if (_gameFlow.gameState == gameflowmultiplayer.GameState.RaceStarted)
                    SetState(BotState.StartRace);
                break;
            case BotState.StartRace:
                if (_gameFlow.gameState == gameflowmultiplayer.GameState.EndGame)
                    SetState(BotState.EndGame);
                CheckEndGame();
                break;
            case BotState.EndGame:
                CheckEndGame();
                break;
            case BotState.WaitLoadScene:
                CheckNewScene();
                break;
        }        
    }

    public void SetGameFlow(GameflowBase gameFlow)
	{
        if (gameFlow is gameflowmultiplayer)
            _gameFlow = gameFlow as gameflowmultiplayer;
        else if (gameFlow is GameflowKDK)
            _gameFlowKDK = gameFlow as GameflowKDK;
        else if (gameFlow is GameflowBOD)
            _gameFlowBOD = gameFlow as GameflowBOD;
    }

    public void SetTeam(int team)
	{
        _team = team;
	}

    public void SetState(BotState state)
	{
        _botState = state;
        _startStateTime = Time.time;
#if DEBUG_BOT
        Debug.Log($"[BOT] SetState {state}");
#endif
        UpdateTexts();
    }

    public void SetInPause(bool pause)
	{
        _isInPause = pause;
        UpdateTexts();
	}

    public void CheckQuitGame()
    {
        if (_gameFlow.gameState == gameflowmultiplayer.GameState.QuitRace)
            SetState(BotState.EndGame);
    }
    public void CheckEndGame()
    {
        if (_gameFlow.gameState == gameflowmultiplayer.GameState.QuitRace)
        {
            SetState(BotState.WaitLoadScene);
            _gameFlow.gameState = gameflowmultiplayer.GameState.WaitLoadScene;
        }
    }

    public void CheckNewScene()
	{   
        if (_gameFlow.gameState == gameflowmultiplayer.GameState.GoToLobby)
        {
            apicalls.myself?.StopGameCounter();
            _gameFlow.OnSceneGoToLobby();
            SetState(BotState.GoToLobby);
        }
        if (_gameFlow.gameState == gameflowmultiplayer.GameState.Cabin)
        {
            apicalls.myself?.StopGameCounter();
            _gameFlow.OnSceneReturnToCabin();
            SetState(BotState.Reset);
        }
    }

    private void UpdateTexts()
	{
        if (_stateUI != null)
            _stateUI.text = _botState.ToString();
        if (_gameFlow != null)
        {
            _flowState = _gameFlow.gameState;
            if (_gameFlowStateUI != null)
                _gameFlowStateUI.text = _flowState.ToString();
            if (_botNameUI != null)
                _botNameUI.text = GameflowBase.myPirateName + (_isInPause ? " - Paused" : "");
            if (_masterUI != null)
                _masterUI.text = (GameflowBase.isBotScene && PhotonNetworkController.IsMaster() ? "Yes" : "No") + "\n" + apicalls.machineid;
        }
    }
}
