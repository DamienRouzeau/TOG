using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextPopup : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _title;
    [SerializeField]
    private TextMeshProUGUI _content;
    [SerializeField]
    private Button _okButton;

    private System.Action _okCbk =  null;

    public void Init(string title, string content, System.Action cbk = null)
	{
        _title.text = title;
        _content.text = content;
        _okCbk = cbk;
        _okButton.onClick.AddListener(OnClickOk);
	}

    private void OnClickOk()
	{
        _okCbk?.Invoke();
        Close();
	}

    public void Close()
	{
        GameObject.Destroy(gameObject);
	}

}
