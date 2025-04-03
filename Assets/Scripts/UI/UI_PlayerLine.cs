using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_PlayerLine : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _name = null;
    [SerializeField]
    private Image _skin = null;


    public void SetName(string name)
	{
        _name.text = name;
	}

    public void SetSkin(Sprite sprite)
	{
        _skin.sprite = sprite;
	}
}
