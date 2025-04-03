//#define DEBUG_BAIT

using UnityEngine;

public class boat_canon_bait : MonoBehaviour
{
	public GameObject lastColliderGO => _lastColliderGO;
	private GameObject _lastColliderGO = null;

	[SerializeField]
	private GameObject _goBaitOn = null;
	[SerializeField]
	private GameObject _goBaitOff = null;
	[SerializeField]
	private ProjectileCannon _canon = null;
	[SerializeField]
	private boat_canon_handle _canonHandle = null;

	private float _lastFireTime = 0f;
	private bool _isTrigger = false;
	private float _fireDelay = 1f;
	private bool _showBait = false;
	private bool _canFire = false;
	private Transform _tr = null;
	private float _reloadSqrMagnitude = 0f;
	private bool _init = false;
	
#if DEBUG_BAIT
	avatarcanon _avatarCanon = null;
#endif

	private void Start()
	{
		UpdateShowBait(false);
		_canFire = false;
		_tr = transform;
		CheckInit();
#if DEBUG_BAIT
		_avatarCanon = gameObject.GetComponentInParent<avatarcanon>();
#endif
	}

	private void CheckInit()
	{
		if (_init)
			return;
		if (gamesettings_boat.myself != null)
		{
			UpdateShowBait(true);
			_canFire = true;
			_fireDelay = gamesettings_boat.myself.boat_fire_cannon_reload_delay;
			float reloadDistance = gamesettings_boat.myself.boat_fire_cannon_reload_distance;
			_reloadSqrMagnitude = Mathf.Max(reloadDistance * reloadDistance, 0.002f);
			if (_canonHandle == null)
				_canonHandle = transform.parent.parent.GetComponentInChildren<boat_canon_handle>(true);
			_init = true;
		}
	}

	private void OnDestroy()
	{
		_canonHandle = null;
		_tr = null;
	}

	private void Update()
	{
		CheckInit();
		UpdateTrigger();
		if (_canFire)
		{
			if (_isTrigger)
			{
				_canFire = false;
				_lastFireTime = Time.time;
#if DEBUG_BAIT
				if (_avatarCanon == null)
					_avatarCanon = gameObject.GetComponentInParent<avatarcanon>();
				if (_avatarCanon != null)
					Debug.Log($"[BAIT] FireCannon {_canon} {_avatarCanon.canonid}");
#endif
				if (_canon != null)
					_canon.FireCannon();
			}
		}
		else
		{
			if (_lastFireTime == 0f || Time.time - _lastFireTime > _fireDelay)
			{
				if (!_isTrigger)
				{
					_canFire = true;
#if DEBUG_BAIT
					if (_avatarCanon == null)
						_avatarCanon = gameObject.GetComponentInParent<avatarcanon>();
					if (_avatarCanon != null)
						Debug.Log($"[BAIT] FireCannon _isTrigger {_isTrigger} _canFire {_canFire} {_avatarCanon.canonid}");
#endif
				}
			}
		}

		UpdateShowBait(_canFire);
	}

	private void UpdateTrigger()
	{
		_isTrigger = false;
		if (_canonHandle != null)
		{
			if (_canonHandle.currentTorch != null)
			{
				Transform torchTr = _canonHandle.currentTorch.transform;
				Vector3 v3Diff = _tr.position - (torchTr.position + torchTr.forward * 0.4f);
				_isTrigger = v3Diff.sqrMagnitude < _reloadSqrMagnitude;
			}
		}
	}

	private void UpdateShowBait(bool show)
	{
		if (_showBait != show)
		{
#if DEBUG_BAIT
			if (_avatarCanon == null)
				_avatarCanon = gameObject.GetComponentInParent<avatarcanon>();
			if (_avatarCanon != null)
				Debug.Log($"[BAIT] UpdateShowBait {show} {_avatarCanon.canonid}");
#endif
			_showBait = show;
			_goBaitOn.SetActive(show);
			_goBaitOff.SetActive(!show);
			//_tr.localScale = Vector3.one * (show ? 1f : 2f);
		}
	}
	/*
	private void OnTriggerStay(Collider other)
	{
		_lastColliderGO = other.gameObject;
		if (_isTrigger)
			return;
		_isTrigger = true;
#if DEBUG_BAIT
		if (_avatarCanon == null)
			_avatarCanon = gameObject.GetComponentInParent<avatarcanon>();
		if (_avatarCanon != null)
			Debug.Log($"[BAIT] OnTriggerStay {_lastColliderGO} _isTrigger {_isTrigger} _canFire {_canFire} {_avatarCanon.canonid}");
#endif
		
	}
	*/
}
