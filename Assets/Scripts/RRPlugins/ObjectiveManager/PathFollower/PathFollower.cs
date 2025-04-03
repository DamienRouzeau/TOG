using UnityEngine;
using Photon.Pun;

public class PathFollower : MonoBehaviour
{
	#region Enums

    public enum PathEvent
    {
        PathEnded
    }

	#endregion

	#region Delegates

	public delegate void OnPathEventCbk(PathFollower path, PathEvent pathEvent, object data=null);

    #endregion

    public PathHall path => m_hallPath;

    public float pathCurrentValue => m_fPathCurrentValue + _currentSegment;

    [SerializeField]
    private PathHall m_hallPath = null;
    [SerializeField]
    private int m_preferredPath = -1;
    [SerializeField]
    private float m_fSpeedMin = 10f;
    [SerializeField]
    private float m_fSpeedMax = 100f;
    [SerializeField]
    private float m_fSpeedIncrement = 100f;
    [SerializeField]
    private float m_fLateralVarianceIncrement = 0.01f;
    [SerializeField]
    private bool _useSplineOrientation = true;

    public int preferredPath { get { return m_preferredPath; } }
    public bool hasSpeedModifier => _speedmodifier != null;
    public bool hasSpeedModifierFromCord => _speedmodifierCord != null;

    private float m_fPathCurrentValue = 0f;
    private float m_fLateralVariance = 1f;
    private float m_fCurrentSpeed = 0f;
    private int m_nPlayerId = -1;
    private bool m_bIsLoopPath = false;

    private AnimationCurve _speedmodifier;
    private float _speedModifierDuration;
    private float _fStartSpeedModifierTime;

    private AnimationCurve _speedmodifierCord;
    private float _speedModifierDurationCord;
    private float _fStartSpeedModifierTimeCord;
    private bool _isCordSpeedModifierAdditive;

    private float _speedAlterationCoef = 1f;

    private float _currentIncrement = 0f;

    public bool raceended = false;
    public bool gameplayrunning = false;

    public float showModifiedSpeed = 0f;
    public float showSpeedModifierTime = 0f;
    public float showRealSpeed = 0f;
    public float showSpeedAlteration = 0f;

    boat_followdummy _bfd = null;

    // Endless mode part
    private Vector3 _splineStart;
    private Vector3 _splineEnd;
    private OnPathEventCbk _onPathEventCbk = null;
    private int _currentSegment = 0;

    public bool isInPause => _isInPause;
    private bool _isInPause = false;

    private void OnDestroy()
    {
        _bfd = null;
        _onPathEventCbk = null;
    }

    #region controller 
    public void SpeedUp()
    {
        m_fCurrentSpeed = Mathf.Min(m_fSpeedMax, m_fCurrentSpeed + m_fSpeedIncrement);
    }

    public void SetSpeed(float multiplier)
    {
        m_fCurrentSpeed = Mathf.Lerp(m_fSpeedMin, m_fSpeedMax, multiplier);
    }

    public void Brake()
    {
        m_fCurrentSpeed = Mathf.Max(m_fSpeedMin, m_fCurrentSpeed - m_fSpeedIncrement);
    }

    public void DriftLeft()
    {
        m_fLateralVariance = Mathf.Max(0f, m_fLateralVariance - m_fLateralVarianceIncrement);
    }

    public void DriftRight()
    {
        m_fLateralVariance = Mathf.Min(1f, m_fLateralVariance + m_fLateralVarianceIncrement);
    }


    #endregion

    public void Setup( int playerId, PathHall pathHall, float fLateralVariance )
    {
        m_hallPath = pathHall;
        m_bIsLoopPath = m_hallPath.IsLoopPath();
        m_fLateralVariance = fLateralVariance;
        m_nPlayerId = playerId;
        Init();
        _splineStart = m_hallPath.GetPositionAtTime(0f, 0f);
        _splineEnd = m_hallPath.GetPositionAtTime(1f, 0f);
    }

    public void SetupNewPathHall(PathHall pathHall)
    {
        m_hallPath = pathHall;
        m_bIsLoopPath = m_hallPath.IsLoopPath();
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

    public void SetModifierFromCord(AnimationCurve curve, float fDuration, bool additive)
    {
        _fStartSpeedModifierTimeCord = Time.time;
        _speedModifierDurationCord = fDuration;
        _speedmodifierCord = curve;
        _isCordSpeedModifierAdditive = additive;
    }

    public void SetSpeedAlterationCoef(float coef)
    {
        _speedAlterationCoef = coef;
        showSpeedAlteration = _speedAlterationCoef;
    }

    #region unity callback
    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (_bfd == null)
        {
            boat_followdummy[] allbfd = gameflowmultiplayer.allBoats;
            if (allbfd != null)
            {
                foreach (boat_followdummy bf in allbfd)
                {
                    if (bf.dummy == gameObject)
                        _bfd = bf;
                }
            }
        }
        else
        {
            if (m_hallPath != null)
            {
                float currentspeed = GetModifiedCurrentSpeed();
                if ((gameplayrunning) && (!raceended) && (!_bfd.isSinking) && !_isInPause)
                {
                    _currentIncrement = m_hallPath.ComputeIncrementFromWorlUnit(currentspeed);
                    m_fPathCurrentValue = (m_fPathCurrentValue + _currentIncrement * Time.deltaTime);
                    UpdatePosAndOrientation();
                    RRObjective.RRObjectiveManager.instance.UpdateObjective("Path", m_nPlayerId, new RRObjective.ObjectivePath.PathObjData(new Vector2(m_fPathCurrentValue, m_fLateralVariance)));
                }
            }
        }
    }

    public void InitSpeed(float min, float max, float current)
    {
        m_fSpeedMin = min;
        m_fSpeedMax = max;
        m_fCurrentSpeed = current;
    }

    #endregion

    #region private functions
    private void Init()
    {
        m_fPathCurrentValue = 0f;

        if (RaceManager.myself?.startAtSimulation ?? false)
            m_fPathCurrentValue = RaceManager.myself.simulatePosition;

        m_fCurrentSpeed = m_fSpeedMin;

        if ( m_hallPath!=null )
        {
            m_hallPath.Init();
            UpdatePosAndOrientation();
        }
    }

    private void UpdatePosAndOrientation()
    {
        // DEBUG BOAT SPEED
        Vector3 oldPos = transform.position;
        // DEBUG BOAT SPEED

        // Endless mode : update current pos & rotation with segment path
        if (gameflowmultiplayer.gameMode == gameflowmultiplayer.GameMode.Endless)
        {
            if (m_fPathCurrentValue >= 1f)
            {
                if (_onPathEventCbk != null)
                    _onPathEventCbk(this, PathEvent.PathEnded);
                m_fPathCurrentValue -= 1f;
                _currentSegment++;
            }
            if (PhotonNetworkController.IsMaster())
            {
                transform.position = m_hallPath.GetPositionAtTime(m_fPathCurrentValue, m_fLateralVariance);
                transform.rotation = m_hallPath.GetRotationAtTime(m_fPathCurrentValue, m_fLateralVariance) * Quaternion.Euler(0,-90f,0);
            }
        }
        else
        {
            // Endless mode with defined duration
            if (multiplayerlobby.IsInEndlessRace)
            {
                if (m_fPathCurrentValue >= 1f)
				{
                    m_fPathCurrentValue = m_hallPath.GetLoopTime(m_fLateralVariance) + m_fPathCurrentValue - 1f;
                    _currentSegment++;
                }
            }

            float currentValue = Mathf.Min(m_fPathCurrentValue, 1f);
            Vector3 vPos = m_hallPath.GetPositionAtTime(currentValue, m_fLateralVariance);

            if (PhotonNetworkController.IsMaster())
                transform.position = vPos;

            // TODO : only master compute finish line
            float fNext = (m_fPathCurrentValue + _currentIncrement) /*% 1f*/;
            if (fNext >= gamesettings.myself.pathgamesize)
            {
                boat_followdummy.EndOfSplineReached(this);
            }

            if (_useSplineOrientation)
            {
                if (PhotonNetworkController.IsMaster())
                    transform.rotation = m_hallPath.GetRotationAtTime(currentValue, m_fLateralVariance) * Quaternion.Euler(0, -90f, 0);
            }
            else
            {
                if (m_bIsLoopPath)
                {
                    fNext = fNext % 1f;
                }
                Vector3 vNext = m_hallPath.GetPositionAtTime(fNext, m_fLateralVariance);

                Vector3 vDir = (vNext - vPos).normalized;
                float fAngle = (Mathf.Acos(vDir.x) * Mathf.Rad2Deg) % 360;
                if ((fAngle > 180f) && vDir.z > 0)
                {
                    fAngle = 180f - fAngle;
                }
                else if (fAngle < 180f && vDir.z < 0)
                {
                    fAngle = -fAngle;
                }
                if (PhotonNetworkController.IsMaster())
                    transform.rotation = Quaternion.Euler(0f, -fAngle, 0f);
            }
        }
        // DEBUG BOAT SPEED
        Vector3 difPos = transform.position - oldPos;
        //Debug.Log("difPos " + difPos.magnitude);
        showRealSpeed = difPos.magnitude;
        // DEBUG BOAT SPEED
    }

    private float GetModifiedCurrentSpeed()
    {
        float fModifiedTime;
        float fCoef;
        float fSceneMultiplier = 1f;
        float fCordMultiplier = 1f;
        float fCordAddition = 0f;

        // Speed modifiers from scene
        if (_speedmodifier!=null )
        {
            fModifiedTime = Time.time - _fStartSpeedModifierTime;
            fCoef = fModifiedTime / _speedModifierDuration;
            showSpeedModifierTime = fCoef;
            fSceneMultiplier = _speedmodifier.Evaluate(fCoef);
            if (fCoef >= 1f) // reset
            {
                _speedmodifier = null;
                showSpeedModifierTime = 0f;
            }
        }

        // Speed modifier from cord
        if (_speedmodifierCord != null)
        {
            fModifiedTime = Time.time - _fStartSpeedModifierTimeCord;
            fCoef = fModifiedTime / _speedModifierDurationCord;
            float eval = _speedmodifierCord.Evaluate(fCoef);
            if (_isCordSpeedModifierAdditive)
			{
                fCordAddition = eval;
            }
            else
			{
                fCordMultiplier = eval;
            }
            if (fCoef >= 1f) // reset
                _speedmodifierCord = null;
        }

        showModifiedSpeed = Mathf.Clamp(fSceneMultiplier * fCordMultiplier * _speedAlterationCoef * m_fCurrentSpeed, m_fSpeedMin, m_fSpeedMax);

        if (gamesettings_boat.myself.boat_speed_with_health != null)
		{
            float healthCoef = _bfd.health.currentHealth / _bfd.health.maxHealth;
            showModifiedSpeed *= gamesettings_boat.myself.boat_speed_with_health.Evaluate(healthCoef);
        }

        showModifiedSpeed += fCordAddition;

        return showModifiedSpeed;
    }


    #endregion
}
