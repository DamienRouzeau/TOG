using UnityEngine;
using UnityEngine.UI;

public class GameSetup : MonoBehaviour
{
	[System.Serializable]
	public class CheckSaved
	{
		public string name;
		public GameObject goCheck;
		public GameObject goCross;

		public void SetSaved(bool saved)
		{
			goCheck.SetActive(saved);
			goCross.SetActive(!saved);
		}
	}


	public InputField _machineNameTxt = null;
	public InputField _roomIdTxt = null;
	public ChoicesInputField _regionIdTxt = null;
	public ChoicesInputField _langIdTxt = null;
	public ChoicesInputField _langAudioIdTxt = null;
	public InputField _passcodeTxt = null;
	public InputField _machineIdTxt = null;
	public string _path = "Trails Of Gold_Data";
	[SerializeField]
	private ChoicesPopup _choicesPopupPrefab = null;
	[SerializeField]
	private TextPopup _textPopupPrefab = null;
	[SerializeField]
	private CheckSaved[] _checkSavedArray = null;

	// Start is called before the first frame update
	void Start()
    {
		LoadFromFileToText("mp_machineName", _machineNameTxt);
		LoadFromFileToText("mp_roomId", _roomIdTxt);
		LoadFromFileToText("mp_machineId", _machineIdTxt);
		LoadFromFileToText("mp_region", _regionIdTxt);
		LoadFromFileToText("mp_lang", _langIdTxt);
		LoadFromFileToText("mp_langAudio", _langAudioIdTxt);
		LoadFromFileToText("mp_arcadeCode", _passcodeTxt, true);

		_regionIdTxt.Init(OnRegionClick);
		_langIdTxt.Init(OnLangClick);
		_langAudioIdTxt.Init(OnLangAudioClick);
		_passcodeTxt.onValueChanged.AddListener(OnPassCodeChanged);
		_roomIdTxt.onValueChanged.AddListener(OnRoomIdChanged);

		CheckTextLength(_passcodeTxt);
		CheckTextLength(_roomIdTxt);
	}

    public void Save()
    {
		if (_passcodeTxt.text.Trim().Length < 12)
			_passcodeTxt.text = "";
		if (_roomIdTxt.text.Trim().Length < 12)
			_roomIdTxt.text = "";

		SaveFileFromText("mp_machineName", _machineNameTxt);
		SaveFileFromText("mp_roomId", _roomIdTxt);
		SaveFileFromText("mp_machineId", _machineIdTxt); 
		SaveFileFromText("mp_region", _regionIdTxt);
		SaveFileFromText("mp_lang", _langIdTxt);
		SaveFileFromText("mp_langAudio", _langAudioIdTxt);
		SaveFileFromText("mp_arcadeCode", _passcodeTxt, true);

		TextPopup popup = GameObject.Instantiate<TextPopup>(_textPopupPrefab, transform);
		popup.transform.localPosition = Vector3.zero;
		popup.transform.localScale = Vector3.one;
		popup.Init("SAVED", "Game is Ready!");
	}

	public void Quit()
	{
		Application.Quit();
	}

	private void OnRegionClick()
	{
		ChoicesPopup popup = GameObject.Instantiate<ChoicesPopup>(_choicesPopupPrefab, transform);
		popup.transform.localPosition = Vector3.zero;
		popup.transform.localScale = Vector3.one;
		string[] texts = new string[] { "Asia", "Australia", "Canada, East", "Chinese", "Europe", "India", "Japan", "Russia", "Russia, East", "South Africa", "South America", "South Korea", "USA, East", "USA, West" };
		string[] values = new string[] { "asia", "au", "cae", "cn", "eu", "in", "jp", "ru", "rue", "za", "sa", "kr", "us", "usw" };
		popup.SetTitle("Server Region");
		popup.InitChoices(texts, values, (string result) => _regionIdTxt.text = result);
	}

	private void OnLangClick()
	{
		ChoicesPopup popup = GameObject.Instantiate<ChoicesPopup>(_choicesPopupPrefab, transform);
		popup.transform.localPosition = Vector3.zero;
		popup.transform.localScale = Vector3.one;
		string[] texts = new string[] { "Français", "English", "Spanish", "German" };
		string[] values = new string[] { "fr-FR", "en-EN", "es-ES", "de-DE" };
		popup.SetTitle("Default Language");
		popup.InitChoices(texts, values, (string result) => _langIdTxt.text = result);
	}

	private void OnLangAudioClick()
	{
		ChoicesPopup popup = GameObject.Instantiate<ChoicesPopup>(_choicesPopupPrefab, transform);
		popup.transform.localPosition = Vector3.zero;
		popup.transform.localScale = Vector3.one;
		string[] texts = new string[] { "Français", "English" };
		string[] values = new string[] { "fr-FR", "en-EN" };
		popup.SetTitle("Default Language");
		popup.InitChoices(texts, values, (string result) => _langAudioIdTxt.text = result);
	}

	private void OnPassCodeChanged(string text)
	{
		CheckTextLength(_passcodeTxt);
	}

	private void OnRoomIdChanged(string text)
	{
		CheckTextLength(_roomIdTxt);
	}

	private void CheckTextLength(InputField field)
	{
		string result = field.text.Trim();
		if (result.Length < 12)
		{
			field.textComponent.color = Color.red * 0.75f;
		}
		else
		{
			field.textComponent.color = Color.white;
		}
	}

	public void LoadFromFileToText(string file, InputField dest, bool decrypt = false)
	{
		Debug.Log($"[LoadFromFileToText] {file} decrypt {decrypt}");
		string text = null;
		string path = GetPath(file);
		CheckSaved check = GetCheckSavedFromName(file);
		if (System.IO.File.Exists(path))
		{
			text = System.IO.File.ReadAllText(path);
			if (decrypt && !string.IsNullOrEmpty(text))
				text = UtilsString.Decrypt(text);
			if (check != null)
				check.SetSaved(true);
		}
		else
		{
			if (PlayerPrefs.HasKey("Setup_" + file))
			{
				text = PlayerPrefs.GetString("Setup_" + file);
				Debug.Log($"[PLAYERPREFS] {file} -> {text}");
				if (decrypt && !string.IsNullOrEmpty(text))
				{
					text = UtilsString.Decrypt(text);
					Debug.Log($"[DECRYPT] {file} -> {text}");
				}
			}
			if (check != null)
				check.SetSaved(false);
		}
		if (!string.IsNullOrEmpty(text))
		{
			Debug.Log($"[READ] {file} -> {text}");
			dest.text = text;
		}
	}

	public void SaveFileFromText(string file, InputField src, bool encrypt = false)
	{
		string text = src.text;
		if (encrypt)
			text = UtilsString.Encrypt(text);
		CheckSaved check = GetCheckSavedFromName(file);
		string path = GetPath(file);
		if (!string.IsNullOrEmpty(text) && System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(path)))
		{
			Debug.Log($"[SAVE] {file} -> {text}");
			System.IO.File.WriteAllText(path, text);
			PlayerPrefs.SetString("Setup_" + file, text);
			if (check != null)
				check.SetSaved(true);
		}
		else if (System.IO.File.Exists(path))
		{
			Debug.Log($"[DELETE] {file}");
			System.IO.File.Delete(path);
			if (check != null)
				check.SetSaved(false);
		}
	}

	private string GetPath(string file)
	{
		return $"{Application.dataPath}/../{_path}/StreamingAssets/{file}.txt";
	}

	private CheckSaved GetCheckSavedFromName(string name)
	{
		if (_checkSavedArray != null)
		{
			foreach (CheckSaved check in _checkSavedArray)
			{
				if (check.name == name)
					return check;
			}
		}
		return null;
	}
}
