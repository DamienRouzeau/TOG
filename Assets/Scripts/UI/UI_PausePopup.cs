using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RRLib;

public class UI_PausePopup : MonoBehaviour
{
    public static UI_PausePopup myself = null;

    public enum ButtonType
	{
        Cabin,
        Lobby,
        Quit,
        Resume
	}

    [SerializeField]
    private Button _cabinButton = null;
    [SerializeField]
    private Button _lobbyButton = null;
    [SerializeField]
    private Button _resumeButton = null;
    [SerializeField]
    private Button _quitButton = null;
    [SerializeField]
    private GameObject _validationRoot = null;
    [SerializeField]
    private Button _validButton = null;
    [SerializeField]
    private Button _cancelButton = null;
    [SerializeField]
    private TextMeshProUGUI _validationTitle = null;
    [SerializeField]
    private RRLocalizedTextMP _validationDesc = null;
    [SerializeField]
    private RRLocalizedTextMP _resumeText = null;

    private bool _isFromRightHand = false;
    private bool _addedWeapon = false;
    private ButtonType _buttonType = ButtonType.Resume;

    private void Start()
	{
        myself = this;

        //gamesettings_screen.myself.FadeOut(0.01f);

        _cabinButton.onClick.AddListener(OnCabinButtonClick);
        _lobbyButton.onClick.AddListener(OnLobbyButtonClick);
        _resumeButton.onClick.AddListener(OnResumeButtonClick);
        _quitButton.onClick.AddListener(OnQuitButtonClick);
        _validButton.onClick.AddListener(OnValidButtonClick);
        _cancelButton.onClick.AddListener(OnCancelButtonClick);

        _validationRoot.SetActive(false);

        _resumeButton.gameObject.SetActive(true);
        if (StarterHub.myself == null)
		{
            _lobbyButton.gameObject.SetActive(gameflowmultiplayer.levelIndex > 0);
            _cabinButton.gameObject.SetActive(true);
            _quitButton.gameObject.SetActive(false);
            _resumeText.SetTextId("str_popupbtnresume");
        }
        else
		{
            bool inLobby = StarterHub.myself != null && StarterHub.myself.hubType == StarterHub.HubType.BeforeRace;
            _lobbyButton.gameObject.SetActive(false);
            _cabinButton.gameObject.SetActive(inLobby);
            _quitButton.gameObject.SetActive(!inLobby);
            _resumeText.SetTextId("str_popupbtnresumegame");
        }

        Player.myplayer?.OnPausePopupVisible(true);
        CurvedUIInputModule.Instance.EventCamera = GetComponent<Camera>();
    }

	private void OnDestroy()
	{
        _cabinButton.onClick.RemoveAllListeners();
        _lobbyButton.onClick.RemoveAllListeners();
        _resumeButton.onClick.RemoveAllListeners();

        if (Player.myplayer != null)
        {
            if (_addedWeapon)
            {
                Player.WeaponPlace place = _isFromRightHand ? Player.WeaponPlace.RightHand : Player.WeaponPlace.LeftHand;
                Player.myplayer.SetWeaponInPlace(Player.WeaponType.None, place);
            }
            Player.myplayer.OnPausePopupVisible(false);
            CurvedUIInputModule.Instance.EventCamera = Player.myplayer.cam;
        }

        //gamesettings_screen.myself.FadeIn();

        myself = null;
    }

    public void SetFromRightHand(bool right)
	{
        _isFromRightHand = right;
        Player.WeaponPlace place = right ? Player.WeaponPlace.RightHand : Player.WeaponPlace.LeftHand;
        if (Player.myplayer.GetWeaponAtPlace(place) == Player.WeaponType.None)
        {
            Player.myplayer.SetWeaponInPlace(Player.WeaponType.Musket, place);
            _addedWeapon = true;
        }
    }

    private void OnCabinButtonClick()
	{
        _buttonType = ButtonType.Cabin;
        _validationTitle.text = _cabinButton.GetComponentInChildren<TextMeshProUGUI>().text;
        _validationDesc.SetTextId("str_popupdesctxt");
        _validationRoot.SetActive(true);
    }

    private void OnLobbyButtonClick()
    {
        _buttonType = ButtonType.Lobby;
        _validationTitle.text = _lobbyButton.GetComponentInChildren<TextMeshProUGUI>().text;
        _validationDesc.SetTextId("str_popupdesctxt");
        _validationRoot.SetActive(true);
    }

    private void OnResumeButtonClick()
    {
        GameObject.Destroy(gameObject);
    }

    private void OnQuitButtonClick()
    {
        _buttonType = ButtonType.Quit;
        _validationTitle.text = _quitButton.GetComponentInChildren<TextMeshProUGUI>().text;
        _validationDesc.SetTextId("str_popupquittxt");
        _validationRoot.SetActive(true);
    }

    private void OnValidButtonClick()
	{
        switch (_buttonType)
        {
            case ButtonType.Cabin:
                gameflowmultiplayer.myself?.QuitRace();
                break;
            case ButtonType.Lobby:
                gameflowmultiplayer.myself?.ReplayRace();
                break;
            case ButtonType.Quit:
#if !UNITY_EDITOR
                Application.Quit();
                return;
#else
                break;
#endif
        }
        GameObject.Destroy(gameObject);
    }

    private void OnCancelButtonClick()
    {
        _validationRoot.SetActive(false);
    }
}
