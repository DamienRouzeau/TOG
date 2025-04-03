using UnityEngine;

public class PlayerFollowTarget : MonoBehaviour
{
    public enum FollowTargetType
    {
        Enemy,
        Ally,
        Goal,
        Archive,
        Scientist
    }

    [SerializeField]
    private FollowTargetType _followTargetType = FollowTargetType.Enemy;

    private bool _followTargetAdded = false;

    private void CheckAddTarget()
	{
        if (!_followTargetAdded)
        {
            if (Player.myplayer != null)
            {
                Player.myplayer.AddTarget(transform, _followTargetType);
                _followTargetAdded = true;
            }
        }
    }

    private void CheckRemoveTarget()
    {
        if (_followTargetAdded)
        {
            if (Player.myplayer != null)
            {
                Player.myplayer.RemoveTarget(transform);
            }
            _followTargetAdded = false;
        }
    }

    private void Update()
	{
        CheckAddTarget();
    }

	void OnEnable()
    {
        CheckAddTarget();
    }

    // Update is called once per frame
    void OnDisable()
    {
        CheckRemoveTarget();
    }

	private void OnDestroy()
	{
        CheckRemoveTarget();
    }
}
