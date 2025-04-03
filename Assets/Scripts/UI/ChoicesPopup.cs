using UnityEngine;
using TMPro;

public class ChoicesPopup : MonoBehaviour
{
    [SerializeField]
    private ChoicesElem _elemPrefab = null;
    [SerializeField]
    private TextMeshProUGUI _title = null;
    [SerializeField]
    private Transform _content = null;

    private System.Action<string> _onChoiceClick = null;

	private void OnDestroy()
	{
        _onChoiceClick = null;
	}

	public void SetTitle(string title)
	{
        _title.text = title;
	}

    public void InitChoices(string[] textArray, string[] valueArray, System.Action<string> cbk)
	{
        _onChoiceClick = cbk;
        for (int i = 0; i < textArray.Length; ++i)
		{
            ChoicesElem elem = GameObject.Instantiate<ChoicesElem>(_elemPrefab, _content);
            elem.Init(textArray[i], valueArray[i], OnChoiceClick);
		}
    }

    private void OnChoiceClick(string choice)
	{
        _onChoiceClick?.Invoke(choice);
        Close();
    }

    public void Close()
	{
        GameObject.Destroy(gameObject);
    }
}
