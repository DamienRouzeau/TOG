using UnityEngine;
using UnityEngine.Events;

public class TargetRecovery : MonoBehaviour
{
    [SerializeField]
    private Health _healthTarget = null;
    [SerializeField]
    private float _recoveryBySecond = 100f;
    [SerializeField]
    private UnityEvent _actionOnStart = null;
    [SerializeField]
    private UnityEvent _actionOnEnd = null;
    [SerializeField]
    private UnityEvent _actionOnFull = null;

    public bool isInRecovery => _isInRecoveryByPlayer || _isInRecoveryByState;
    private bool _isInRecoveryByState = false;
    private bool _isInRecoveryByPlayer = false;
    private float _initialRecoveryBySecond = 0f; 

    public void SetRecoveryBySecond(float recovery)
	{
        _recoveryBySecond = recovery;
	}

    public void ResetRecoveryBySecond()
	{
        _recoveryBySecond = _initialRecoveryBySecond;
    }

    public void SetRecoveryByPlayer(bool enableRecoveryByPlayer)
	{
        UpdateRecovery(enableRecoveryByPlayer, true);
    }

    private void OnRecoveryStart()
	{
        if (!_healthTarget.dead && _healthTarget.currentHealth > 0f)
            _actionOnStart?.Invoke();
    }

    private void OnRecoveryEnd()
    {
        if (!_healthTarget.dead && _healthTarget.currentHealth > 0f)
            _actionOnEnd?.Invoke();
    }

    private void OnRecoveryUpdate()
	{
        if (isInRecovery && _healthTarget != null && PhotonNetworkController.IsMaster())
		{
            if (!_healthTarget.dead && _healthTarget.currentHealth > 0f && _healthTarget.currentHealth < _healthTarget.maxHealth)
            {
                float givenLife = _recoveryBySecond * Time.deltaTime;
                if (_healthTarget.currentHealth + givenLife >= _healthTarget.maxHealth)
				{
                    givenLife = _healthTarget.maxHealth - _healthTarget.currentHealth;
                    _actionOnFull?.Invoke();
                }
                if (_isInRecoveryByState)
                {
                    _healthTarget.ForceCurrentHealth(_healthTarget.currentHealth + givenLife);
                }
                else
                {
                    if (TowerDefManager.myself.regeneratorLife > 0)
                    {
                        TowerDefManager.myself.UseRegeneratorLife(givenLife);
                        _healthTarget.ForceCurrentHealth(_healthTarget.currentHealth + givenLife);
                    }
                }
            }
		}
	}

    private void Awake()
    {
        TowerDefManager.onTowerDefState += OnTowerDefState;
        _initialRecoveryBySecond = _recoveryBySecond;
    }

    // Update is called once per frame
    private void OnDestroy()
    {
        TowerDefManager.onTowerDefState -= OnTowerDefState;
    }

	private void Update()
	{
        OnRecoveryUpdate();
	}

	private void OnTowerDefState(TowerDefManager.TowerDefState state, int numWave, int relativeWave, int level)
    {
        UpdateRecovery(state == TowerDefManager.TowerDefState.RECOVERY);
    }

    private void UpdateRecovery(bool recov, bool byPlayer = false)
	{
        bool wasInRecovery = isInRecovery;
        
        if (byPlayer)
            _isInRecoveryByPlayer = recov;
        else
            _isInRecoveryByState = recov;

        if (isInRecovery && _healthTarget.currentHealth < _healthTarget.maxHealth)
        {
            if (!wasInRecovery)
            {
                OnRecoveryStart();
            }
        }
        else
        {
            if (wasInRecovery)
            {
                OnRecoveryEnd();
            }
        }
    }
}
