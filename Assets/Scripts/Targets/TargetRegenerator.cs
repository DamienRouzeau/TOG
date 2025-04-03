using UnityEngine;
using UnityEngine.Events;

public class TargetRegenerator : MonoBehaviour
{
    public enum State
	{
        Working,
        Full,
        Disabled
	}

    [SerializeField]
    private Transform _center = null;
    [SerializeField]
    private float _radius = 1f;
    [SerializeField]
    private GameObject _activatedLight = null;
    [SerializeField]
    private GameObject _activatedLightDisabled = null;
    [SerializeField]
    private UnityEvent _workingEvent = null;
    [SerializeField]
    private UnityEvent _fullEvent = null;
    [SerializeField]
    private UnityEvent _disabledEvent = null;

    public bool activated => _isActivated;
    public bool isEnabled => _isEnabled;

    public bool isLightVisible => _isActivated && _isEnabled;

    private bool _isActivated = false;
    private bool _isEnabled = false;
    private int _numPlayer = -1;
    private State _currentState = State.Working;

    public bool IsInCenter(Vector3 pos)
	{
        float centerX = _center.position.x;
        float centerZ = _center.position.z;
        Vector2 diff = new Vector2(centerX - pos.x, centerZ - pos.z);
        return diff.sqrMagnitude < _radius * _radius;
	}

    public bool SetLightActivation(bool activate, int numPlayer)
	{
        if (_isActivated == activate)
            return false;
        if (activate)
            _numPlayer = numPlayer;
        else if (_numPlayer != numPlayer)
            return false;
        _isActivated = activate;
        UpdateLight();
        return true;
    }

    public void SetEnable(bool isEnabled)
	{
        _isEnabled = isEnabled;
        UpdateLight();
    }

    private void UpdateLight()
	{
        _activatedLight.SetActive(_isActivated && _isEnabled && _currentState == State.Working);
        _activatedLightDisabled.SetActive(_isActivated && _currentState == State.Disabled);
    }

    public void SetState(State state, bool force = false)
	{
        if (force || _currentState != state)
        {
            _currentState = state;
            switch (state)
            {
                case State.Working:
                    _workingEvent?.Invoke();
                    break;
                case State.Full:
                    _fullEvent?.Invoke();
                    break;
                case State.Disabled:
                    _disabledEvent?.Invoke();
                    break;
            }
        }
	}
}
