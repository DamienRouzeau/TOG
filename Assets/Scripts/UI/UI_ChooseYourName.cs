using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VRKB;

public class UI_ChooseYourName : MonoBehaviour
{
    public enum NameType
	{
        Player,
        Room
	}


    public static UI_ChooseYourName myself = null;

    [SerializeField]
    private TMP_InputField _inputName = null;
    [SerializeField]
    private UI_KeyboardManagerTMPro _keyboardManager = null;
    [SerializeField]
    private KeyboardBehaviour _keyboardBehaviour = null;
    [SerializeField]
    private List<TextMeshProUGUI> _textListToUpdate = null;
    [SerializeField]
    private bool _setDefaultNameAtStart = true;

    public System.Action<string> onNameValidatedAction = null;
    public System.Action<string> onNameChangedAction = null;

    public string playerName => _inputName?.text ?? "";

    public bool isNameConfirmed => _isNameConfirmed;
    private bool _isNameConfirmed = false;

    private string _defaultName = null;
    private string _currentName = null;

    public NameType nameType => _nameType;
    private NameType _nameType = NameType.Player;

	private void Awake()
	{
        myself = this;
    }

	private void Start()
    {
        if (_keyboardManager != null)
        {
            _keyboardManager.onValueChangedCbk += SetName;
            if (_inputName != null)
                _inputName.interactable = false;
        }
        else
        {
            _inputName.onValueChanged.AddListener(SetName);
        }
        if (_keyboardBehaviour != null)
            _keyboardBehaviour.OnConfirm.AddListener(SetNameConfirmed);
    }

	private void OnDestroy()
    {
        if (myself == this)
            myself = null;

        if (_keyboardManager != null)
            _keyboardManager.onValueChangedCbk -= SetName;
        else 
            _inputName?.onValueChanged?.RemoveListener(SetName);

        if (_keyboardBehaviour != null)
            _keyboardBehaviour.OnConfirm.RemoveListener(SetNameConfirmed);

        _inputName = null;
        _keyboardManager = null;
        _keyboardBehaviour = null;
    }

    public void Init(NameType nameType, string defaultName = null, bool setDefaultName = true)
	{
        _nameType = nameType;
        switch (_nameType)
        {
            case NameType.Player:
                {
                    if (setDefaultName)
                        SetNameAsDefault(defaultName);
                    Debug.Log($"UI_ChooseYourName Start! name {GameflowBase.myPirateName}");
                    SetName(GameflowBase.myPirateName, _setDefaultNameAtStart);
                }
                break;
            case NameType.Room:
            {
                if (setDefaultName)
                    SetNameAsDefault(defaultName);
                SetName(defaultName);
            }
            break;
        }

    }

    [ContextMenu("OnNameConfirmed")]
    public void OnNameConfirmed()
	{
        string name = _currentName;
        if (_keyboardBehaviour != null)
            _keyboardBehaviour.OnConfirm.Invoke(name);
        if (_keyboardManager != null)
            SetNameConfirmed(name);
        onNameValidatedAction?.Invoke(name);
    }

    public void SetNameConfirmed(string name)
	{
        Debug.Log("SetNameConfirmed " + name);
        _isNameConfirmed = true;
        SetName(name);
	}

    public void SetNameAsDefault(string name)
    {
        SetName(name, true);
    }

    public void SetName(string name)
    {
        SetName(name, false);
    }

    private void SetName(string name, bool isPlaceHolder)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            if (name.Length > 16)
                name = name.Substring(0, 16);
        }
        else
		{   
            name = _defaultName;
		}

        if (name == _defaultName)
            isPlaceHolder = true;
        
        if (_inputName != null)
            _inputName.text = isPlaceHolder ? "" : name;
        
        if (isPlaceHolder)
        {
            _defaultName = name;
            if (_keyboardBehaviour != null)
                _keyboardBehaviour.PlaceholderText = name;
            else if (_inputName.placeholder != null)
                (_inputName.placeholder as TextMeshProUGUI).text = name;
        }
        
        if (_nameType == NameType.Player)
        {
            if (_textListToUpdate != null)
            {
                foreach (var tmp in _textListToUpdate)
                {
                    tmp.text = name;
                }
            }
            GameflowBase.myPirateName = name;
            Player.myplayer.UpdateName();
        }

        _currentName = name;

        onNameChangedAction?.Invoke(name);
    }
}
