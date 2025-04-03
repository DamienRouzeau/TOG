//#define DEBUG_CANON_ORIENTATION

using UnityEngine;
using Valve.VR.InteractionSystem;

public class boat_canon : MonoBehaviour
{
	public float orientationH => _linearMappingH?.value ?? 0.5f;
	public float orientationV => _linearMappingV?.value ?? 0.5f;
	public Vector2 orientation => new Vector2(orientationH, orientationV);

	public string id => _avatarCanon.canonid + "_" + _avatarCanon.team;

	[SerializeField]
	private LinearMapping _linearMappingH = null;
	[SerializeField]
	private LinearMapping _linearMappingV = null;
	[SerializeField]
	private float _defaultVerticalValue = 0.25f;
	[SerializeField]
	private float _smoothSpeed = 5f;
	[SerializeField]
	private float _targetThreshold = 0.01f;

	private avatarcanon _avatarCanon = null;
	private float _ratioTargetH = 0f;
	private float _ratioTargetV = 0f;
	private bool _useTargets = false;

	private void Awake()
	{
		if (_linearMappingH == null || _linearMappingV == null)
		{
			LinearMapping[] lmArray = GetComponentsInChildren<LinearMapping>();
			if (lmArray != null)
			{
				if (lmArray.Length > 0)
					_linearMappingH = lmArray[0];
				if (lmArray.Length > 1)
					_linearMappingV = lmArray[1];
			}
		}

		_avatarCanon = GetComponentInChildren<avatarcanon>(true);
	}

	private void Update()
	{
		if (_useTargets)
		{
			if (_linearMappingH != null && _linearMappingV != null)
			{
				float smooth = Time.deltaTime * _smoothSpeed;
				_linearMappingH.value = Mathf.Lerp(_linearMappingH.value, _ratioTargetH, smooth);
				_linearMappingV.value = Mathf.Lerp(_linearMappingV.value, _ratioTargetV, smooth);
				Vector2 linear = new Vector2(_linearMappingH.value, _linearMappingV.value);
				Vector2 target = new Vector2(_ratioTargetH, _ratioTargetV);
#if DEBUG_CANON_ORIENTATION
				Debug.Log($"[CANON_ORIENTATION] Update {linear}");
#endif
				if ((linear - target).sqrMagnitude < _targetThreshold)
				{
					_linearMappingH.value = _ratioTargetH;
					_linearMappingV.value = _ratioTargetV;
					_useTargets = false;
#if DEBUG_CANON_ORIENTATION
					Debug.Log($"[CANON_ORIENTATION] Update done");
#endif
				}
			}
		}
	}

	public void SetOrientationWithTargetRatio(float ratioH, float ratioV)
	{
#if DEBUG_CANON_ORIENTATION
		Debug.Log($"[CANON_ORIENTATION] SetOrientationWithTargetRatio {ratioH} {ratioV} ");
#endif
		_useTargets = true;
		_ratioTargetH = ratioH;
		_ratioTargetV = ratioV;
	}

	public void SetOrientationWithRatio(float ratioH, float ratioV)
	{
		//Debug.Log($"Raph - SetOrientationWithRatio {_avatarCanon.canonid} {ratioH} {ratioV}");
		if (_linearMappingH != null)
			_linearMappingH.value = ratioH;
		if (_linearMappingV != null)
			_linearMappingV.value = ratioV;
	}

	public bool CanTargetPosition(Vector3 pos)
	{
		Vector3 diff = pos - transform.position;
		diff.y = 0;
		if (diff.sqrMagnitude < 22500f)
		{
			Vector3 dir = diff.normalized;
			float dot = Vector3.Dot(-transform.right, dir);
			return dot > 0.5f;
		}
		return false;
	}

	public void SetOrientationWithPosition(Vector3 pos, bool keepDefaultVerticalValue = true, float precision=1f)
	{
		pos += Random.insideUnitSphere * precision;
		Vector3 diff = pos - transform.position;
		Vector3 diffXZ = diff - Vector3.up * diff.y;
		Vector3 dir = diffXZ.normalized;
		float dot = Vector3.Dot(-transform.right, dir);
		if (dot > 0.5f)
		{
			float orientation = Vector3.Dot(transform.forward, -dir);
			float ratio = Mathf.InverseLerp(-0.5f, 0.5f, orientation);
			if (keepDefaultVerticalValue)
			{
				SetOrientationWithRatio(ratio, _defaultVerticalValue);
			}
			else
			{
				float ratioY = Mathf.InverseLerp(-0.05f, 0.6f, diff.normalized.y);
				SetOrientationWithRatio(ratio, ratioY);
			}
		}
		else
		{
			SetOrientationWithRatio(0.5f, _defaultVerticalValue);
		}
	}	
}
