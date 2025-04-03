using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CabinPlayerLine : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _text;
    [SerializeField]
    private Image _checkInBkgImage;
    [SerializeField]
    private Image _checkInOnImage;
    [SerializeField]
    private Image _checkInOffImage;

    public void SetPlayerName(string name)
	{
        _text.text = name;
    }

    public void SetReady(bool ready)
	{
        Color nameColor = Color.black;
        nameColor.a = ready ? 1f : 0.7f;
        _checkInBkgImage.color = nameColor;
        _text.color = _checkInBkgImage.color;
        _checkInOnImage.gameObject.SetActive(ready);
        _checkInOffImage.gameObject.SetActive(!ready);
    }
}
