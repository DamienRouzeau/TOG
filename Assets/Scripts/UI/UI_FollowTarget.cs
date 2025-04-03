using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_FollowTarget : MonoBehaviour
{
	public Transform target => _target;

	[Header("Images")]
	[SerializeField]
	private Image _image = null;
	[Header("Distances")]
	[SerializeField]
	private float _distanceMax = 1000f;
	[SerializeField]
	private float _distanceMin = 20f;
	[SerializeField]
	private float _posMin = 2.5f;
	[SerializeField]
	private float _posMax = 7.5f;
	[SerializeField]
	private float _scaleMin = 3f;
	[SerializeField]
	private float _scaleMax = 5f;
	[SerializeField]
	private AnimationCurve _positionCurve = null;
	[SerializeField]
	private AnimationCurve _scaleCurve = null;
	[SerializeField]
	private AnimationCurve _alphaCurve = null;
	[Header("Time")]
	[SerializeField]
	private float _loopTime = 0f;
	[SerializeField]
	private AnimationCurve _scaleOverTimeCurve = null;
	[SerializeField]
	private AnimationCurve _alphaOverTimeCurve = null;
	[Header("Debug")]
	[SerializeField]
	private bool _showDebug = false;
	[SerializeField]
	private TextMeshProUGUI _debugText = null;

	private Camera _cam = null;
	private Transform _target = null;
	private RectTransform _rt = null;
	private float _time = 0f;

	public void Init(Camera cam, Transform tr)
	{
		_cam = cam;
		_target = tr;
		_rt = transform.GetComponent<RectTransform>();
	}

	private void LateUpdate()
	{
		if (_cam != null && _target != null)
		{
			Vector3 camPos = _cam.transform.position;
			Vector3 diff = _target.position - _cam.transform.position;
			float distance = diff.magnitude;
			Vector3 dir = diff / distance;
			float distanceCoef = Mathf.InverseLerp(_distanceMin, _distanceMax, distance);
			float alphaCoef = _alphaCurve != null ? _alphaCurve.Evaluate(distanceCoef) : distanceCoef;
			float posCoef = _positionCurve != null ? _positionCurve.Evaluate(distanceCoef) : distanceCoef;
			float scaleCoef = _scaleCurve != null ? _scaleCurve.Evaluate(distanceCoef) : distanceCoef;
			float scale = Mathf.Lerp(_scaleMin, _scaleMax, scaleCoef);
			if (_loopTime > 0f)
			{
				_time += Time.deltaTime;
				float ratioTime = (_time % _loopTime) / _loopTime;
				float scaleTime = _scaleOverTimeCurve != null ? _scaleOverTimeCurve.Evaluate(ratioTime) : ratioTime;
				float alphaTime = _alphaOverTimeCurve != null ? _alphaOverTimeCurve.Evaluate(ratioTime) : ratioTime;
				scale *= scaleTime;
				alphaCoef *= alphaTime;
			}
			Color color = _image.color;
			color.a = alphaCoef;
			_image.color = color;
			_rt.position = camPos + dir * Mathf.Lerp(_posMin, _posMax, posCoef);
			_rt.localScale = Vector3.one * scale;
			_rt.LookAt(camPos);
			if (_debugText != null)
			{
				if (_showDebug)
				{
					Health h = _target.GetComponentInChildren<Health>();
					if (h != null)
						_debugText.text = h.instanceId.ToString() + "\n" + h.currentHealth.ToString();
				}
				_debugText.gameObject.SetActive(_showDebug);
			}
		}
		else if (_target == null)
		{
			Player.myplayer.RemoveTarget(null);
		}
	}
}
