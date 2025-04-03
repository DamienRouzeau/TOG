using UnityEngine;
using UnityEngine.UI;

public class UI_CustomItem : MonoBehaviour
{
    public string itemName => _name;
    public Button itemButton => _button;

    [SerializeField]
    private Button _button = null;
    [SerializeField]
    private Image _image = null;
    [SerializeField]
    private Sprite _unlockedItem = null;
    [SerializeField]
    private Sprite _lockedItem = null;
    [SerializeField]
    private string _name = null;

    public void SetLock(bool locked)
	{
        _button.interactable = !locked;
        _image.sprite = locked ? _lockedItem : _unlockedItem;
	}
}
