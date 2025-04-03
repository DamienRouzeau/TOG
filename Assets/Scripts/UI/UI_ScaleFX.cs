using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ScaleFX : MonoBehaviour
{
    public enum ScaleFX
	{
        LOOP,
        PING_PONG
	}

    [SerializeField]
    private Vector3 _startScale = Vector3.one;
    [SerializeField]
    private Vector3 _endScale = Vector3.one;
    [SerializeField]
    private Image _image = null;
    [SerializeField]
    private float _startAlpha = 1f;
    [SerializeField]
    private float _endAlpha = 1f;
    [SerializeField]
    private float _duration = 1f;
    [SerializeField]
    private ScaleFX _fx = ScaleFX.LOOP;

    // Update is called once per frame
    void Update()
    {
        float time = Time.time % _duration;
        float ratio = 1f;
        switch (_fx)
		{
            case ScaleFX.LOOP:
                ratio = time / _duration;
                break;
            case ScaleFX.PING_PONG:
                ratio = 1f - Mathf.Abs(1f - time * 2f / _duration);
                break;
        }
        transform.localScale = Vector3.Lerp(_startScale, _endScale, ratio);
        if (_image != null)
		{
            Color color = _image.color;
            color.a = Mathf.Lerp(_startAlpha, _endAlpha, ratio);
            _image.color = color;
        }
    }
}
