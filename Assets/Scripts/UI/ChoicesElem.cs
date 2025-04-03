using UnityEngine;
using UnityEngine.UI;

public class ChoicesElem : MonoBehaviour
{
	[SerializeField]
	private Text _text;
	[SerializeField]
	private Button _button;

	private System.Action<string> _clickCbk = null;
	private string _value;

	public void Init(string text, string value, System.Action<string> clickCbk = null)
	{
		_text.text = text;
		_clickCbk = clickCbk;
		_value = value;
		_button.onClick.AddListener(OnClick);
	}

	private void OnClick()
	{
		_clickCbk?.Invoke(_value);
	}

	private void OnDestroy()
	{
		_clickCbk = null;
	}
}
