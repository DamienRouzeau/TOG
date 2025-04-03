using UnityEngine;
using TMPro;

public class UI_SetLabel : MonoBehaviour
{
    public enum LabelType
	{
        PlayerName
	}

    [SerializeField]
    private TextMeshProUGUI _textToUpdate = null;
    [SerializeField]
    private LabelType _labelType = LabelType.PlayerName;

    // Start is called before the first frame update
    void Start()
    {
        if (_textToUpdate == null)
            _textToUpdate = gameObject.GetComponent<TextMeshProUGUI>();
        if (_textToUpdate != null)
		{
            switch (_labelType)
            {
                case LabelType.PlayerName:
                    _textToUpdate.text = GameflowBase.myPirateName;
                    break;
            }
		}
    }
}
