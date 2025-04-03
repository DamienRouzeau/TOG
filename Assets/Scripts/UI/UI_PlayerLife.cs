using UnityEngine;
using TMPro;

public class UI_PlayerLife : MonoBehaviour
{
    [SerializeField]
    private Animator _anim = null;
    [SerializeField]
    private TextMeshProUGUI _lifeText = null;
    [SerializeField]
    private string _hitTrigger = null;
    [SerializeField]
    private string _deadTrigger = null;
    [SerializeField]
    private string _regenTrigger = null;

    private int _oldPercent = 100;

    public void SetPercentLife(int percent, bool playAnim = true)
	{
        if (playAnim)
		{   
            if (percent < _oldPercent)
                _anim.SetTrigger(_hitTrigger);
            else
                _anim.SetTrigger(_regenTrigger);
            if (percent == 0)
                _anim.SetTrigger(_deadTrigger);
        }
        
        _lifeText.text = percent.ToString() + "%";
        _oldPercent = percent;
    }
}
