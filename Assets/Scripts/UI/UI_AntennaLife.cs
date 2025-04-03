using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_AntennaLife : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _life = null;
    [SerializeField]
    private TextMeshProUGUI _antennaId = null;
    [SerializeField]
    private Image _bkg = null;
    [SerializeField]
    private Gradient _bkgGradient = null;
    [SerializeField]
    private GameObject _dead = null;
    [SerializeField]
    private GameObject _warning = null;
    [SerializeField]
    private Color _deadBkgColor = Color.white;

    public void SetLifePercent(int percent)
	{
        bool dead = percent == 0;
        bool warning = percent <= 25;
        _life.text = percent + "%";
        _life.gameObject.SetActive(!dead);
        _bkg.color = dead ? _deadBkgColor : _bkgGradient.Evaluate(percent / 100f);
        _dead.SetActive(dead);
        _warning.SetActive(!dead && warning);
    }

    public void SetAntennaId(int id)
	{
        _antennaId.text = id.ToString();
	}
}
