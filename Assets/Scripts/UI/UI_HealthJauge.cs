using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UI_HealthJauge : MonoBehaviour
{
    [SerializeField]
    private Image m_fillImage = null;
    [SerializeField]
    private TextMeshProUGUI _levelText = null;
    [SerializeField]
    private GameObject _levelUpFX = null;
    [SerializeField]
    private TextMeshProUGUI _name = null; 

    private bool m_bIsActive = true;

    public void Enable()
    {
        m_bIsActive = true;
        gameObject.SetActive(true);
    }

    public void Disable()
    {
        m_bIsActive = false;
        gameObject.SetActive(false);
    }

    public void UpdateValue( float f )
    {
        Debug.Assert(f >= 0 && f <= 1);
        if (m_fillImage != null)
        {   
            if (!m_bIsActive)
            {
                Enable();
            }
            m_fillImage.fillAmount = f;
        }
    }

    public void SetLevelText(string text)
	{
        if (_levelText != null)
            _levelText.text = text;
	}

    public void SetName(string name)
    {
        if (_name != null)
        {
            _name.text = name;
            _name.gameObject.SetActive(true);
        }
    }

    public void LevelUp()
	{
        StartCoroutine(LevelUpEnum(10f));
	}

    private IEnumerator LevelUpEnum(float waitEndFxDuration)
	{
        if (_levelUpFX != null)
		{
            yield return new WaitForSeconds(1f);
            _levelUpFX.SetActive(true);
            yield return new WaitForSeconds(waitEndFxDuration);
            _levelUpFX.SetActive(false);
        }
	}
}
