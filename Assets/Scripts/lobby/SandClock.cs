using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

[ExecuteAlways]
public class SandClock : MonoBehaviour
{
    public delegate void OnSandClockOver();

    [System.Serializable]
    public class CountdownStep
	{
        public int stepTime;
        public Color textColor = Color.white;
        public VoiceOver voiceOver = null;
	}

    [SerializeField]
    private Material _topMat = null;
    [SerializeField]
    private Material _bottomMat = null;
    [SerializeField]
    private GameObject _sandTop = null;
    [SerializeField]
    private GameObject _sandBottom = null;
    [SerializeField]
    private GameObject _sandFX = null;
    [SerializeField]
    private GameObject _sandFxQuad = null;
    [SerializeField]
    private GameObject _topMinPos = null;
    [SerializeField]
    private GameObject _topMaxPos = null;
    [SerializeField]
    private GameObject _bottomMinPos = null;
    [SerializeField]
    private float _bottomMinPosY = 0f;
    [SerializeField]
    private float _bottomMaxPosY = 0.25f;
    [SerializeField]
    private float _fxQuadMinPosY = -0.5f;
    [SerializeField]
    private float _fxQuadMaxPosY = -0.1f;
    [SerializeField]
    private TextMeshProUGUI _countdownText = null;
    [SerializeField]
    public List<CountdownStep> _steps = null;

    [Range(0f, 1f)]
    public float timeSlider = 0f;
    public bool resetInit = false;

    public float currentTimeRatio => _currentRatio;
    public OnSandClockOver onSandClockOverCbk = null;

    private Material _sandTopMat = null;
	private Material _sandBottomMat = null;
    private MeshRenderer _sandTopMeshRd = null;
    private MeshRenderer _sandBottomMeshRd = null;
    private Vector4 _topPlanePos = Vector4.zero;
    private Vector4 _bottomPlanePos = Vector4.zero;
    private Vector3 _bottomPos = Vector3.zero;
    private Vector3 _fxQuadPos = Vector3.zero;
    private float _matTopMinPosY = 0f;
    private float _matTopMaxPosY = 0f;
    private float _matBottomMinPosY = 0f;
    private float _currentRatio = 0f;
    private float _timeDuration = 010f;
    private float _startTime = 0f;
    private int _countdownShowed = 0;

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

	private void OnDestroy()
	{
        onSandClockOverCbk = null;
    }

	private void Init()
	{
        _sandTopMeshRd = _sandTop.GetComponent<MeshRenderer>();
        _sandTopMat = new Material(_topMat);
        _sandTopMeshRd.material = _sandTopMat;
        _sandBottomMeshRd = _sandBottom.GetComponent<MeshRenderer>();
        _sandBottomMat = new Material(_bottomMat);
        _sandBottomMeshRd.material = _sandBottomMat;
        _matTopMinPosY = _topMinPos.transform.position.y;
        _matTopMaxPosY = _topMaxPos.transform.position.y;
        _matBottomMinPosY = _bottomMinPos.transform.position.y;
        _bottomPlanePos.y = _matBottomMinPosY;
        _sandBottomMat.SetVector("_PlanePosition", _bottomPlanePos);
    }

    public void StartTime(float duration)
	{
        _timeDuration = duration;
        _startTime = Time.time;
        _countdownShowed = Mathf.RoundToInt(duration);
        UpdateCountdown();
    }

    public void SetTimeRatio(float ratio)
	{
        _currentRatio = ratio;
        _topPlanePos.y = Mathf.Lerp(_matTopMaxPosY, _matTopMinPosY, ratio);
        _sandTopMat.SetVector("_PlanePosition", _topPlanePos);
        _bottomPos.y = Mathf.Lerp(_bottomMinPosY, _bottomMaxPosY, ratio);
        _sandBottom.transform.localPosition = _bottomPos;
        _fxQuadPos.y = Mathf.Lerp(_fxQuadMinPosY, _fxQuadMaxPosY, ratio);
        _sandFxQuad.transform.localPosition = _fxQuadPos;
    }

    public void ShowFX(bool show)
	{
        _sandFX.SetActive(show);
    }

	private void Update()
	{
        if (Application.isPlaying)
		{
            if (_startTime > 0f && _timeDuration > 0f)
			{
                float elapsedTime = Time.time - _startTime;

                if (elapsedTime < _timeDuration)
				{
                    int timeRemaing = Mathf.CeilToInt(_timeDuration - elapsedTime);
                    if (timeRemaing != _countdownShowed)
					{
                        _countdownShowed = timeRemaing;
                        UpdateCountdown();
                        
                    }
                    SetTimeRatio(elapsedTime / _timeDuration);
                    ShowFX(true);
                }
                else
				{
                    ShowFX(false);
                    _startTime = 0f;
                    _countdownShowed = 0;
                    UpdateCountdown();
                    if (onSandClockOverCbk != null)
                        onSandClockOverCbk();
                }
			}
		}
        else
        {
            if (resetInit)
			{
                Init();
                resetInit = false;
            }
            SetTimeRatio(timeSlider);
        }
    }

    private void UpdateCountdown()
	{
        _countdownText.text = _countdownShowed.ToString();
        UpdateSteps();
    }

    private void UpdateSteps()
	{
        if (_steps != null)
		{
            foreach (var step in _steps)
			{
                if (step.stepTime == _countdownShowed)
				{
                    _countdownText.color = step.textColor;
                    if (step.voiceOver != null)
					{
                        VoiceOver voice = GameObject.Instantiate<VoiceOver>(step.voiceOver);
                        if (voice != null)
						{
                            voice.gameObject.SetActive(true);
                            StartCoroutine(DeleteMeAfterDelayEnum(voice.gameObject, 10f));
						}
					}
				}
			}
		}
	}

    private IEnumerator DeleteMeAfterDelayEnum(GameObject go, float delay)
	{
        if (delay > 0f)
            yield return new WaitForSeconds(delay);
        GameObject.Destroy(go);
	}
}
