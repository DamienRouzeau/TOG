using UnityEngine;
using UnityEngine.Events;

public class TeleportSnapPoint : MonoBehaviour
{
    public float snapDistance => _snapDistance;

    [SerializeField]
    private float _snapDistance = 0.5f;
	[SerializeField]
	private bool _addSphereCollider = true;
	[SerializeField]
	private bool _teleportToAnotherTarget = false;
	[SerializeField]
	private float _teleportToAnotherTargetResetTime = 0.2f;
	[SerializeField]
	private Transform[] _otherTargets = null;
	[SerializeField]
	private Collider _colliderToDetect = null;
	[SerializeField]
	private UnityEvent _onEnter = null;
	[SerializeField]
	private UnityEvent _onExit = null;

	private GameObject _teleportTarget = null;
	private SphereCollider _collider = null;
	private float _resetTime = 0f;
	private bool _isTeleportSet = false;

	private void Start()
	{
		if (_addSphereCollider)
		{
			Vector3 scale = transform.lossyScale;
			float maxScale = Mathf.Max(scale.x, scale.y, scale.z);
			if (maxScale > 0f)
			{
				_collider = gameObject.GetAddComponent<SphereCollider>();
				_collider.gameObject.layer = LayerMask.NameToLayer("Floor");
				_collider.radius = (_snapDistance / maxScale) * 0.8f;
			}
		}
	}

	public void Update()
	{
		if (_teleportTarget == null)
			_teleportTarget = pointfromhand.teleporttarget;

		if (_colliderToDetect != null)
		{
			if (Player.pointright.teleportCollider == _colliderToDetect ||
				Player.pointleft.teleportCollider == _colliderToDetect)
			{
				SetTeleportPos();
			}
		}
		else if (IsTeleporterInsideRadius())
		{
			SetTeleportPos();
		}

		if (_resetTime > 0f)
		{
			_resetTime -= Time.deltaTime;
			if (_resetTime < 0f)
			{
				ResetTeleportOverride();
			}
		}
	}

	private void SetTeleportPos()
	{
		if (pointfromhand.teleportOverride != null)
			_resetTime = _teleportToAnotherTargetResetTime;

		if (_isTeleportSet)
			return;

		_teleportTarget.transform.position = transform.position;
		if (_teleportToAnotherTarget && _otherTargets != null && _otherTargets.Length > 0)
		{
			int idx = Random.Range(0, _otherTargets.Length);
			pointfromhand.teleportOverride = _otherTargets[idx];
			_resetTime = _teleportToAnotherTargetResetTime;
		}

		_onEnter?.Invoke();
		_isTeleportSet = true;
	}

	private void ResetTeleportOverride()
	{
		if (_isTeleportSet)
		{
			pointfromhand.teleportOverride = null;
			_resetTime = 0f;

			_onExit?.Invoke();
			_isTeleportSet = false;
		}
	}

	public bool IsTeleporterInsideRadius()
	{
		if (_teleportTarget != null && _teleportTarget.activeInHierarchy)
		{
			float sqrDist = Vector3.SqrMagnitude(_teleportTarget.transform.position - transform.position);
			return sqrDist < _snapDistance * _snapDistance;
		}
		return false;
	}

	private void OnDestroy()
	{
		_teleportTarget = null;
		_collider = null;
	}
}
