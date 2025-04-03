using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class selectedroomline : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField]
    private Button _removeBtn = null;
    [SerializeField]
    private Button _changeTeamBtn = null;
    [Header("Machine")]
    [SerializeField]
    private TextMeshProUGUI _machineName = null;
    [SerializeField]
    private Text _name = null;
    [Header("Player")]
    [SerializeField]
    private TMP_InputField _playerName = null;
    [Header("Team")]
    [SerializeField]
    private Image _teamImage = null;
    [SerializeField]
    private Sprite _blueSprite = null;
    [SerializeField]
    private Sprite _redSprite = null;
    [Header("Master")]
    [SerializeField]
    private TextMeshProUGUI _roleName = null;
    [SerializeField]
    private GameObject _masterIcon = null;

    public machineline machine { get; set; } = null;
    public string reference { get; set; }

	private void Start()
	{
        if (_removeBtn != null)
            _removeBtn.onClick.AddListener(OnRemoveClick);
        if (_changeTeamBtn != null)
            _changeTeamBtn.onClick.AddListener(OnChangeTeamClick);
        if (_playerName != null)
            _playerName.onValueChanged.AddListener(OnPlayerNameChanged);
    }

	private void OnDestroy()
	{
        if (_removeBtn != null)
            _removeBtn.onClick.RemoveListener(OnRemoveClick);
        if (_changeTeamBtn != null)
            _changeTeamBtn.onClick.RemoveListener(OnChangeTeamClick);
        if (_playerName != null)
            _playerName.onValueChanged.RemoveListener(OnPlayerNameChanged);
    }

    public void SetChangeTeamAvailable(bool available)
	{
        if (_changeTeamBtn != null)
            _changeTeamBtn.gameObject.SetActive(available);
    }

    public void SetMachineName(string name)
	{
        if (_machineName != null)
            _machineName.text = name;
        if (_name != null)
            _name.text = name;
    }

    public void SetPlayerName(string name)
    {
        if (_playerName != null)
            _playerName.text = name;
    }

    public void SetPlayerTeam(int team)
	{
        if (_teamImage != null)
            _teamImage.sprite = team == 0 ? _redSprite : _blueSprite;
    }

    public void SetMaster(bool master)
	{
        if (_roleName != null)
            _roleName.text = master ? "Master PC" : "Client PC";
        if (_masterIcon != null)
            _masterIcon.SetActive(master);
    }

    public void Remove()
	{
        if (machine != null)
        {
            machine.gameObject.SetActive(true);
            machine.RemoveFromSession();
        }
        Destroy(gameObject);
    }

	private void OnRemoveClick()
    {
        Remove();
    }

    private void OnChangeTeamClick()
	{
        if (machine != null)
            machine.ChangeTeam();
    }

    private void OnPlayerNameChanged(string name)
	{
        if (machine != null)
            machine.SetPlayerName(name);
	}

}
