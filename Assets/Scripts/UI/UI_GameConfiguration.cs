using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_GameConfiguration : MonoBehaviour
{
    [SerializeField]
    private UI_PlayerLine[] _playerLines = null;
    [SerializeField]
    private Button _startBtn = null;
    [SerializeField]
    private GameObject[] _onlyMasterObjects = null;
    [SerializeField]
    private Sprite[] _skinSprites = null;
    [SerializeField]
    private Sprite _defaultSkinSprite = null;
    [SerializeField]
    private TextMeshProUGUI _playerCounter = null;
    [SerializeField]
    private GridLayoutGroup _grid = null;

    private float _checkPlayerCountTime = 0f;
    private System.Action _onStartCallback = null;

    public void SetStartCallback(System.Action cbk)
	{
        _onStartCallback = cbk;
	}

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetworkController.IsMaster())
        {
            _startBtn.gameObject.SetActive(true);
            _startBtn.onClick.AddListener(OnStartClicked);
            _startBtn.interactable = false;
        }
        else
		{
            _startBtn.gameObject.SetActive(false);
        }
        UpdateOnlyMasterObjects(false);
        UpdatePlayers();
    }

    void OnDestroy()
    {
        _onStartCallback = null;
        _startBtn.onClick.RemoveListener(OnStartClicked);
    }

	private void Update()
	{
        _checkPlayerCountTime += Time.deltaTime;
        if (_checkPlayerCountTime > 0.5f)
		{
            UpdatePlayers();
            _checkPlayerCountTime = 0f;
        }
    }

    private void UpdateOnlyMasterObjects(bool isActive)
	{
        if (_onlyMasterObjects != null)
		{
            foreach (GameObject go in _onlyMasterObjects)
			{
                go.SetActive(isActive);
			}
		}
	}

    private void UpdatePlayers()
	{
        int playerCount = GameflowBase.playerCount;
        int lineCount = _playerLines.Length;
        int playerReady = 0;

        for (int i = 0; i < playerCount; ++i)
		{
			GameflowBase flow = GameflowBase.allFlows[i];
            int lineIdx = flow.actorNum;
            if (lineIdx < lineCount)
            {
                UI_PlayerLine line = _playerLines[lineIdx];
                line.gameObject.SetActive(true);
                if (flow.nameValidated)
                {
                    line.SetName(flow.localName);
                    string skin = GameflowBase.pirateskins[flow.actorNum];
                    int skinIdx = gamesettings_player.myself.GetSkinIndexFromName(skin);
                    line.SetSkin(_skinSprites[skinIdx]);
                    playerReady++;
                }
                else
                {
                    line.SetName(RRLib.RRLanguageManager.instance.GetString("str_kdk_wait4players"));
                    line.SetSkin(_defaultSkinSprite);
                }
            }
            else
			{
                Debug.LogError($"Not enough lines {lineCount} for players {playerCount}");
			}
        }

        for (int i = playerCount; i < lineCount; ++i)
        {
            _playerLines[i].gameObject.SetActive(false);
        }
        
        bool interactable = playerCount == playerReady;
        _startBtn.interactable = interactable;
        UpdateOnlyMasterObjects(PhotonNetworkController.IsMaster() && interactable);

        int maxPlayers = Mathf.Min(lineCount, GameflowBase.nrplayersmax);

        if (_playerCounter != null)
            _playerCounter.text = $"{playerCount} / {maxPlayers}";

        if (_grid != null)
		{
            if (playerCount <= lineCount / 2)
			{
                _grid.padding.left = 500;
            }
            else
			{
                _grid.padding.left = 0;
            }
		}
    }

    private void OnStartClicked()
	{
        _onStartCallback?.Invoke();
    }
}
