using UnityEngine;
using Photon.Pun;

public class SplineFollower : MonoBehaviour
{
	#region Enums

    public enum PathEvent
    {
        PathEnded
    }

	#endregion

	#region Delegates

	public delegate void OnPathEventCbk(SplineFollower path, PathEvent pathEvent, object data=null);

    #endregion

    public float pathCurrentValue => _pathCurrentValue + _currentSegment;

    [SerializeField]
    private SplineController _splineToFollow = null;
    [SerializeField]
    private float _startSpeed = 50f;
    [SerializeField]
    private float _speedMin = 10f;
    [SerializeField]
    private float _speedMax = 100f;
    [SerializeField]
    private AnimationCurve _speedCurve = null;

    public bool isRunning = false;
    public bool useLoop = false;
    public float startValue = 0f;

    public bool hasSpeedModifier => _speedmodifier != null;

    private float _pathCurrentValue = 0f;
    private float _currentSpeed = 0f;
    private float _targetSpeed = 0f;
    private float _changeSpeedVelocity = 1f;
    private bool _isLoopPath = false;

    private AnimationCurve _speedmodifier;
    private float _speedModifierDuration;
    private float _fStartSpeedModifierTime;

    private float _speedAlterationCoef = 1f;

    private float _currentIncrement = 0f;

    private OnPathEventCbk _onPathEventCbk = null;
    private int _currentSegment = 0;

    public bool isInPause => _isInPause;
    private bool _isInPause = false;

    private bool _init = false;

    private void OnDestroy()
    {
        _onPathEventCbk = null;
    }

    #region controller 
    public void SetSpeed(float multiplier, float velocity = 1f)
    {
        _targetSpeed = Mathf.Lerp(_speedMin, _speedMax, multiplier);
        _speedCurve = null;
        _changeSpeedVelocity = velocity;
    }

    #endregion

    public void Setup(bool loop)
    {
        SetLoop(loop);
        Init();
    }

    public void SetupNewSpline(SplineController spline, bool loop = false, bool run = false, AnimationCurve curve = null)
    {
        _splineToFollow = spline;
        SetLoop(loop);
        isRunning = run;
        _speedCurve = curve;
        Init();
    }

    public void SetLoop(bool loop)
	{
        _isLoopPath = loop;
        if (!loop)
		{
            _pathCurrentValue %= 1f;
		}
    }

    public void AddOnPathEvent(OnPathEventCbk cbk)
    {
        _onPathEventCbk += cbk;
    }

    public void RemoveOnPathEvent(OnPathEventCbk cbk)
    {
        _onPathEventCbk -= cbk;
    }

    public void SetInPause(bool inPause)
	{
        _isInPause = inPause;
    }

    public void SetModifier(AnimationCurve curve, float fDuration)
    {
        _fStartSpeedModifierTime = Time.time;
        _speedModifierDuration = fDuration;
        _speedmodifier = curve;
    }

    public void SetSpeedAlterationCoef(float coef)
    {
        _speedAlterationCoef = coef;
    }

    #region unity callback
    // Start is called before the first frame update
    void Start()
    {
        if (!_init)
            Setup(useLoop);
    }

    // Update is called once per frame
    void Update()
    {
        if (_splineToFollow != null && _splineToFollow.SplineMagnitude > 0f)
        {
            float currentspeed = GetModifiedCurrentSpeed();
            if (isRunning && !_isInPause)
            {
                _currentIncrement = currentspeed * 1f / _splineToFollow.SplineMagnitude;
                _pathCurrentValue = (_pathCurrentValue + _currentIncrement * Time.deltaTime);
                UpdatePosAndOrientation();
            }
        }
    }

    public void InitSpeed(float min, float max, float current)
    {
        _speedMin = min;
        _speedMax = max;
        _currentSpeed = current;
        _targetSpeed = current;
    }

    #endregion

    #region private functions
    private void Init()
    {
        _pathCurrentValue = startValue;

        _currentSpeed = _startSpeed;
        _targetSpeed = _startSpeed;

        if ( _splineToFollow!=null )
        {
            _splineToFollow.Init();
            UpdatePosAndOrientation();
        }

        _init = true;
    }

    private void UpdatePosAndOrientation()
    {
        float currentValue = pathCurrentValue;
        if (currentValue >= 1f)
        {
            _onPathEventCbk?.Invoke(this, PathEvent.PathEnded);
            if (_isLoopPath)
                currentValue %= 1f;
            else
                currentValue = 1f;
        }
        transform.position = _splineToFollow.GetPositionAtTime(currentValue);
        transform.rotation = _splineToFollow.GetRotationAtTime(currentValue);
    }

    private float GetModifiedCurrentSpeed()
    {
        float fModifiedTime;
        float fCoef;
        float fSceneMultiplier = 1f;

        // Speed modifiers from scene
        if (_speedmodifier!=null )
        {
            fModifiedTime = Time.time - _fStartSpeedModifierTime;
            fCoef = fModifiedTime / _speedModifierDuration;
            fSceneMultiplier = _speedmodifier.Evaluate(fCoef);
            if (fCoef >= 1f) // reset
            {
                _speedmodifier = null;
            }
        }
        if (_speedCurve != null)
		{
            _currentSpeed = Mathf.Lerp(_speedMin, _speedMax, _speedCurve.Evaluate(pathCurrentValue % 1f));
        }
        else if (Mathf.Abs(_currentSpeed - _targetSpeed) > 0.01f)
		{
            _currentSpeed = Mathf.Lerp(_currentSpeed, _targetSpeed, Time.deltaTime * _changeSpeedVelocity);
        }
        float speed = Mathf.Clamp(fSceneMultiplier * _speedAlterationCoef * _currentSpeed, _speedMin, _speedMax);
        return speed;
    }


    #endregion
}
