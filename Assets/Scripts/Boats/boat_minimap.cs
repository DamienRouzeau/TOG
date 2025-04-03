using UnityEngine;
using UnityEngine.UI;

public class boat_minimap : MonoBehaviour
{
    [SerializeField]
    private Slider _sliderBoatBlue = null;
    [SerializeField]
    private Slider _sliderBoatRed = null;
    [SerializeField]
    private Image _drawing = null;

    public void SetSliders(float valBlue, float valRed)
	{
        if (_sliderBoatBlue != null)
            _sliderBoatBlue.value = valBlue;
        if (_sliderBoatRed != null)
            _sliderBoatRed.value = valRed;
    }

    public void SetDrawing(Sprite sprite)
	{
        if (_drawing != null)
            _drawing.sprite = sprite;
	}
}
