using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RRLib;
using System.Collections;
using Photon.Pun;

public class UI_ProfileManager_Multi : MonoBehaviour
{
	public System.Action onPlayStartAction = null;
	public System.Action<string> onJoinOrCreateRoomAction = null;
	public System.Action onCancelMulti = null;

	[Header("Menus")]
	[SerializeField]
    private GameObject _startMenu = null;
    [SerializeField]
    private GameObject _createMenu = null;
    [SerializeField]
    private GameObject _joinMenu = null;
    [SerializeField]
    private GameObject _keyboard = null;
	[SerializeField]
	private UI_ChooseYourName _chooseRoomName = null;
	[Header("Buttons")]
	[SerializeField]
	private Button _createRoomButton = null;
	[SerializeField]
	private Button _joinRoomButton = null;
	[SerializeField]
	private Button _playRoomButton = null;
	[SerializeField]
	private Button _cancelRoomButton = null;
	[SerializeField]
	private Button _validRoomButton = null;
	[Header("Creation")]
	[SerializeField]
	private TextMeshProUGUI _createRoomText = null;
	[Header("Join")]
	[SerializeField]
	private TextMeshProUGUI _jointRoomText = null;
	[Header("Player List")]
	[SerializeField]
	private GameObject _playerListRoot = null;
	[SerializeField]
	private GameObject _playerListButtons = null;
	[SerializeField]
	private GameObject _playerListWaiting = null;
	[SerializeField]
	private TextMeshProUGUI _playerListMyName = null;
	[SerializeField]
	private GameObject[] _playerListOtherLines = null;
	[SerializeField]
	private TextMeshProUGUI[] _playerListOtherNames = null;
	[SerializeField]
	private Button[] _playerListOtherRemoveButtons = null;
	[Header("Popup")]
	[SerializeField]
	private GameObject _popupRoot = null;
	[SerializeField]
	private RRLocalizedTextMP _popupMessage = null;
	[SerializeField]
	private Button _popupOkButton = null;

	private string _currentRoomName = null;
	private string _currentPhotonRoomName = null;
	private bool _isCreatingRoom = true;
	private bool _isRoomLocked = false;

	private void Start()
	{
		_createRoomButton.onClick.AddListener(OnCreateRoomClicked);
		_joinRoomButton.onClick.AddListener(OnJoinRoomClicked);
		_playRoomButton.onClick.AddListener(OnPlayRoomClicked);
		_cancelRoomButton.onClick.AddListener(OnCancelRoomClicked);
		PhotonNetworkController.onPhotonEventCallback += OnPhotonEvents;
	}

	private void OnDestroy()
	{
		PhotonNetworkController.onPhotonEventCallback -= OnPhotonEvents;		
		_createRoomButton.onClick.RemoveAllListeners();
		_joinRoomButton.onClick.RemoveAllListeners();
		_playRoomButton.onClick.RemoveAllListeners();
		_cancelRoomButton.onClick.RemoveAllListeners();
	}

	private void OnEnable()
	{
		_chooseRoomName.onNameChangedAction += OnRoomNameChanged;
		_chooseRoomName.onNameValidatedAction += OnRoomNameValidated;
		_validRoomButton.interactable = false;
	}

	private void OnDisable()
	{
		_chooseRoomName.onNameChangedAction -= OnRoomNameChanged;
		_chooseRoomName.onNameValidatedAction -= OnRoomNameValidated;
		_validRoomButton.interactable = true;
	}

	public void ShowStartMenu()
	{
		Debug.Log($"ShowStartMenu");
		_startMenu.SetActive(true);
		_createMenu.SetActive(false);
		_joinMenu.SetActive(false);
		_keyboard.SetActive(false);
		_playerListRoot.SetActive(false);
		_playerListButtons.SetActive(false);
		_playerListWaiting.SetActive(false);
		string defaultRoomName = _currentRoomName;
		if (string.IsNullOrEmpty(defaultRoomName))
			defaultRoomName = "MyRoom";
		UpdateRoomName(defaultRoomName);
		CheckRoomName(_currentRoomName);
		_chooseRoomName.Init(UI_ChooseYourName.NameType.Room, defaultRoomName);
		_isRoomLocked = false;
	}

	private void ShowCreateMenu()
	{
		Debug.Log($"ShowCreateMenu");
		_startMenu.SetActive(false);
		_createMenu.SetActive(true);
		_joinMenu.SetActive(false);
		_keyboard.SetActive(true);
		_playerListRoot.SetActive(false);
		_playerListButtons.SetActive(false);
		_playerListWaiting.SetActive(false);
		_playRoomButton.gameObject.SetActive(true);
		_playRoomButton.interactable = false;
		_isCreatingRoom = true;
		CheckRoomName(_currentRoomName);
		_isRoomLocked = false;
	}

	private void ShowJoinMenu()
	{
		Debug.Log($"ShowJoinMenu");
		_startMenu.SetActive(false);
		_createMenu.SetActive(false);
		_joinMenu.SetActive(true);
		_keyboard.SetActive(true);
		_playerListRoot.SetActive(false);
		_playerListButtons.SetActive(false);
		_playerListWaiting.SetActive(false);
		_playRoomButton.gameObject.SetActive(false);
		_isCreatingRoom = false;
		CheckRoomName(_currentRoomName);
		_isRoomLocked = false;
	}

	private void ShowListPlayers()
	{
		Debug.Log($"ShowListPlayers");
		_playerListRoot.SetActive(true);
		_playerListButtons.SetActive(true);
		_playerListWaiting.SetActive(true);
		_playerListMyName.text = SaveManager.myself.profile.name;
		if (_playerListOtherLines != null)
		{
			for (int i = 0; i < _playerListOtherLines.Length; ++i)
			{
				_playerListOtherLines[i].SetActive(false);
			}
		}
		StartCoroutine(RefreshPlayerCountEnum(1f));
	}

	private IEnumerator RefreshPlayerCountEnum(float delay)
	{
		while (true)
		{
			yield return new WaitForSeconds(delay);
			UpdatePlayerList();
		}
	}

	private void OnRoomNameChanged(string name)
	{
		UpdateRoomName(name);
		CheckRoomName(name);
	}

	private void OnRoomNameValidated(string name)
	{
		string roomName = "TOG_Standalone_Multi_Room_" + name.Trim();
		bool exist = PhotonNetworkController.myself.CheckRoomExist(roomName, !_isCreatingRoom);
		Debug.Log($"OnRoomNameValidated exist {exist} roomName {roomName}");
		if (_isCreatingRoom && exist)
		{
			Debug.Log($"Created Room {name} already exist!");
			ShowRoomPopupError("str_multipmenu13", true);
			return;
		}
		if (!_isCreatingRoom && !exist)
		{
			Debug.Log($"Joined Room {name} don't exist!");
			ShowRoomPopupError("str_multipmenu16", true);
			return;
		}
		_keyboard.SetActive(false);
		_currentRoomName = name;
		_currentPhotonRoomName = roomName;
		PhotonNetworkController.myself.JoinOrCreateRoom(roomName, 4);
		
	}

	private void ClosePopup(bool showKeyboard = false)
	{
		_popupOkButton.onClick.RemoveAllListeners();
		_popupRoot.SetActive(false);
		if (showKeyboard)
			_keyboard.SetActive(true);
	}

	private void UpdateRoomName(string name)
	{
		string title = RRLanguageManager.instance.GetString("str_multipmenu05") + " " + name;
		_createRoomText.text = title;
		_jointRoomText.text = title;
	}

	private void CheckRoomName(string name)
	{
		if (!string.IsNullOrEmpty(name) && PhotonNetwork.InLobby)
			_validRoomButton.interactable = name.Trim().Length > 5;
		else
			_validRoomButton.interactable = false;
	}

	private void OnCreateRoomClicked()
	{
		ShowCreateMenu();
	}

	private void OnJoinRoomClicked()
	{
		ShowJoinMenu();
	}

	private void OnPlayRoomClicked()
	{
		onPlayStartAction?.Invoke();
		PhotonNetwork.CurrentRoom.IsOpen = false;
		SetRoomLocked();
	}

	public void SetRoomLocked()
	{
		_isRoomLocked = true;
	}

	private void OnCancelRoomClicked()
	{
		StopAllCoroutines();
		PhotonNetworkController.myself.LeaveRoom();
		if (_isCreatingRoom)
			ShowCreateMenu();
		else
			ShowJoinMenu();
	}

	private void UpdatePlayerList()
	{
		bool canPlay = false;
		if (_playerListOtherLines != null)
		{
			GameflowBase.SearchAllFlows();
			int playerCount = GameflowBase.allFlows != null ? GameflowBase.allFlows.Length : 0;
			canPlay = playerCount > 1;
			int numLine = 0;
			for (int i = 0; i < playerCount; ++i)
			{
				int actorNum = GameflowBase.allFlows[i].actorNum;
				if (actorNum != GameflowBase.myId)
				{
					_playerListOtherLines[numLine].SetActive(true);
					_playerListOtherNames[numLine].text = GameflowBase.piratenames[actorNum];
					numLine++;
				}
			}
			for (int i = numLine; i < _playerListOtherLines.Length; ++i)
			{
				_playerListOtherLines[i].SetActive(false);
			}
		}
		_playRoomButton.interactable = canPlay;
		if (!PhotonNetwork.InRoom)
			OnCancelRoomClicked();
	}

	private void ShowRoomPopupError(string textId, bool showKeyboard=false)
	{
		StopAllCoroutines();
		_keyboard.SetActive(false);
		_popupRoot.SetActive(true);
		_popupMessage.SetTextId(textId);
		_popupOkButton.onClick.AddListener(() => ClosePopup(showKeyboard));
	}

	private void OnPhotonEvents(PhotonNetworkController.PhotonEvent photonEvent, string msg)
	{
		Debug.Log($"OnPhotonEvents {photonEvent} - {msg}");
		switch (photonEvent)
		{
			case PhotonNetworkController.PhotonEvent.JoinedLobby:				
				CheckRoomName(_currentRoomName);
				break;
			case PhotonNetworkController.PhotonEvent.JoinedRoom:
				Debug.Log("JoinedRoom " + _currentRoomName);
				UpdateRoomName(_currentRoomName);
				_keyboard.SetActive(false);
				ShowListPlayers();
				onJoinOrCreateRoomAction?.Invoke(_currentPhotonRoomName);
				break;
			case PhotonNetworkController.PhotonEvent.JoinRoomFailed:
				ShowRoomPopupError("str_multierror", true);
				break;
			case PhotonNetworkController.PhotonEvent.JoinRoomFailedFull:
				ShowRoomPopupError("str_multiroomfull", true);
				break;
			case PhotonNetworkController.PhotonEvent.PlayerEnterRoom:
				if (int.TryParse(msg, out int actorNum))
				{
					if (actorNum > GameflowBase.nrplayersmax)
					{
						ShowRoomPopupError("str_multierror");
						onCancelMulti?.Invoke();
					}
				}
				break;
			case PhotonNetworkController.PhotonEvent.PlayerLeaveRoom:
				if (_isRoomLocked)
				{
					ShowRoomPopupError("str_multierror");
					onCancelMulti?.Invoke();
				}
				break;
		}	
	}
}
