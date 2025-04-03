using UnityEngine;
using TMPro;
using System.Collections;

public class UI_LifeRegenStatus : MonoBehaviour
{
    public enum LifeRegenStep
    {
        STEP_75,
        STEP_50,
        STEP_25,
        STEP_0,
    }

    [SerializeField]
    private CanvasGroup _canvasGroup = null;
    [SerializeField]
    private TextMeshProUGUI _percentValue = null;
    [SerializeField]
    private GameObject[] _voiceOvers = null;
    [SerializeField]
    private float _animationDuration = 0.2f;
    [SerializeField]
    private float _showDuration = 2f;

    public void ShowLifeRegen(LifeRegenStep step)
	{
        float value = 100f;
        int voIndex = 0;
        switch (step)
		{
            case LifeRegenStep.STEP_0:
                value = 0f;
                voIndex = 3;
                break;
            case LifeRegenStep.STEP_25:
                value = 25f;
                voIndex = 2;
                break;
            case LifeRegenStep.STEP_50:
                value = 50f;
                voIndex = 1;
                break;
            case LifeRegenStep.STEP_75:
                value = 75f;
                voIndex = 0;
                break;
        }

        _percentValue.text = value.ToString() + "%";
        StartCoroutine(ShowLifeRegenEnum(_voiceOvers[voIndex]));
	}

    private void OnEnable()
	{
        _canvasGroup.alpha = 0f;
	}

    private IEnumerator ShowLifeRegenEnum(GameObject voiceOver)
	{
        if (_animationDuration > 0)
        {
            float time = Time.time;
            while (Time.time - time < _animationDuration)
            {
                float alpha = (Time.time - time) / _animationDuration;
                _canvasGroup.alpha = alpha;
                yield return null;
            }
        }

        _canvasGroup.alpha = 1f;

        voiceOver.SetActive(true);

        yield return new WaitForSeconds(_showDuration);

        if (_animationDuration > 0)
        {
            float time = Time.time;
            while (Time.time - time < _animationDuration)
            {
                float alpha = 1f - (Time.time - time) / _animationDuration;
                _canvasGroup.alpha = alpha;
                yield return null;
            }
        }

        _canvasGroup.alpha = 0f;

        yield return new WaitForSeconds(2f);

        voiceOver.SetActive(false);
    }
}
