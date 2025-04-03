using UnityEngine;
using RootMotion.FinalIK;
using System.Collections;

public class player_vrik_corrector : MonoBehaviour
{
    private VRIK _vrik = null;
    private GameObject _vrik_root = null;
    private bool _initLocalPos = false;
    private Transform _parent = null;    
    private Player_avatar _avatar = null;
    private bool _isAvatarTeleporting = false;
    private float _speed = 0f;
    private Vector3 _vrikDeltaPos = Vector3.zero;
    private Vector3 _lastLocalPos = Vector3.zero;
    private Vector3 _localPos = Vector3.zero;
    private Vector3 _localDeltaPos = Vector3.zero;
    private Vector3 _lastWorldPos = Vector3.zero;
    private Vector3 _worldPos = Vector3.zero;
    private Vector3 _worldDeltaPos = Vector3.zero;

    private void Awake()
    {
        _vrik_root = gameObject.GetComponentInChildren<vrik_tag>().gameObject;
        Debug.Assert(_vrik_root != null);
        _lastWorldPos = transform.position;
    }

	private void OnDisable()
	{
        _isAvatarTeleporting = false;
    }

	private void OnEnable()
	{
        _isAvatarTeleporting = false;
        if (_avatar != null)
            _avatar.TeleportShowOn();
    }

	private void OnDestroy()
    {
        _vrik = null;
        _vrik_root = null;
        _parent = null;
    }

    public void SetVRIK(VRIK vrik, Player_avatar avatar = null)
    {
        _vrik = vrik;
        _initLocalPos = false;
        _avatar = avatar;
    }

	private void LateUpdate()
    {
        if (_vrik == null)
            _vrik = gameObject.GetComponentInChildren<VRIK>();

        if (_vrik == null)
            return;

        if (_parent != transform.parent)
        {
            _initLocalPos = false;
            _parent = transform.parent;
        }

        _vrikDeltaPos = Vector3.zero;
        _localDeltaPos = Vector3.zero;

        _worldPos = transform.position;
        _worldDeltaPos = _worldPos - _lastWorldPos;
        _lastWorldPos = _worldPos;
        _speed = _worldDeltaPos.magnitude / Time.deltaTime;

        if (_parent != null)
        {
            _localPos = transform.localPosition; // _parent.InverseTransformPoint(_vrik_root.transform.position);

            if (_initLocalPos)
            {
                _localDeltaPos = _localPos - _lastLocalPos;
                _speed = _localDeltaPos.magnitude / Time.deltaTime;
            }
            else
            {
                _initLocalPos = true;
                _speed = 0f;
            }

            _lastLocalPos = _localPos;

            _vrikDeltaPos = _worldDeltaPos;

			if (_localDeltaPos.sqrMagnitude < 1f && !_isAvatarTeleporting)
				_vrikDeltaPos -= _localDeltaPos;
		}
		else
		{
            if (_worldDeltaPos.sqrMagnitude > 1f || _isAvatarTeleporting)
			{
                _vrikDeltaPos = _worldDeltaPos;
            }
		}

        if (_vrikDeltaPos.sqrMagnitude > 0f)
        {
            _vrik.solver.AddPlatformMotion(_vrikDeltaPos, Quaternion.identity, Vector3.zero);
        }

		if (_avatar != null)
		{
			if (!_isAvatarTeleporting && _speed > 20f && _speed < 100f)
			{
                //Debug.Log("[VRIK] Teleporting speed " + _speed);
				_isAvatarTeleporting = true;
				StartCoroutine(TeleportAvatarEnum());
			}
		}
	}

	private IEnumerator TeleportAvatarEnum()
	{
        _avatar.TeleportShowOff(_lastWorldPos);
        while (_speed > 0.1f)
            yield return null;
        _avatar.TeleportShowOn();
        yield return new WaitForSeconds(1f);
        _isAvatarTeleporting = false;
    }
}
