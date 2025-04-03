using UnityEngine;
using RootMotion.FinalIK;

public class avatar_vrik_corrector : MonoBehaviour
{
    public float speedMin = 2f;
    public float speedMax = 5f;
    public float freezeDuration = 0.2f;
    public float teleportDistanceMin = 0.5f;
    public float teleportFreezeDuration = 0.5f;
    public float changeParentFreezeDuration = 1f;

    private Vector3 _lastLocalPos = Vector3.zero;
    private VRIK _vrik = null;
    private GameObject _vrik_root = null;
    private float _freezeStartTime = 0f;
    private float _freezeDuration = 0f;
    private bool _initLocalPos = false;
    private Transform _parent = null;
    private Vector3 _lastWorldPos = Vector3.zero;
    private boat_sinking _boatSinking = null;
    private Player_avatar _avatar = null;
    private int _actorNumber = -1;
    private bool _useTeleportation = false;

    private void Awake()
    {
        _vrik_root = gameObject.GetComponentInChildren<vrik_tag>().gameObject;
        _avatar = GetComponent<Player_avatar>();
        Debug.Assert(_vrik_root != null);
        _lastWorldPos = transform.position;
        if (_avatar != null)
            _actorNumber = _avatar.actornumber;
    }

    private void OnDestroy()
    {
        _vrik = null;
        _vrik_root = null;
        _parent = null;
        _boatSinking = null;
    }

    public void SetVRIK(VRIK vrik, Player_avatar avatar=null)
    {
        //Debug.Log("AVATAR SetVRIK " + vrik + " avatar " + avatar);
        _vrik = vrik;
        _initLocalPos = false;
        _avatar = avatar;
        if (_avatar != null)
            _actorNumber = _avatar.actornumber;
    }

    private void UpdateFreezeDuration(float duration, bool useTeleport)
	{
        if (useTeleport)
            _useTeleportation = true;

        if (_freezeStartTime == 0f)
            _freezeDuration = duration;
        else
            _freezeDuration = Mathf.Max(_freezeStartTime + _freezeDuration - Time.time, duration);

        _freezeStartTime = Time.time;
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
            _boatSinking = gameObject.GetComponentInParent<boat_sinking>();
            UpdateFreezeDuration(changeParentFreezeDuration, false);
            //Debug.Log($"AVATAR {_actorNumber} change parent {_parent} freeze {_freezeDuration} _boatSinking { _boatSinking}");
        }

		bool applyCorrection = false;
        Vector3 deltaPos = Vector3.zero;

        if (transform.position != _lastWorldPos)
        {
            Vector3 diffWorldPos = transform.position - _lastWorldPos;
            if (_boatSinking != null)
                diffWorldPos -= _boatSinking.lastMove;
            deltaPos = diffWorldPos;
            float diffWorldDistance = diffWorldPos.magnitude;
            if (diffWorldDistance > teleportDistanceMin)
            {
                if (_avatar != null && !_useTeleportation)
                    _avatar.TeleportShowOff(_lastWorldPos);
                UpdateFreezeDuration(teleportFreezeDuration, true);
                //Debug.Log($"AVATAR {_actorNumber} start teleportation " + _freezeDuration);
            }
            _lastWorldPos = transform.position;
        }


        Vector3 localPos = Vector3.zero;

        if (_parent != null)
        {
            localPos = _parent.InverseTransformPoint(_vrik_root.transform.position);
        }

        if (_freezeDuration > 0f && _freezeStartTime > 0f)
        {
            float freezeTime = Time.time - _freezeStartTime;
            applyCorrection = freezeTime < _freezeDuration;
            //Debug.Log($"AVATAR {_actorNumber} freeze " + freezeTime + " " + applyCorrection + " " + _useTeleportation + " " + _avatar + " " + _boatSinking);
            if ( !applyCorrection)
            {
                _freezeDuration = 0f;
                _freezeStartTime = 0f;
                if (_useTeleportation)
                {
                    if (_avatar != null)
                        _avatar.TeleportShowOn();
                    _useTeleportation = false;
                }
            }
        }
        else if (_initLocalPos)
        {
            float deltaTime = Time.deltaTime;

            if (localPos != _lastLocalPos && deltaTime > 0f && !_useTeleportation)
            {
                Vector3 localDeltaPos = localPos - _lastLocalPos;

                if (speedMax > 0f)
                {
                    float distance = deltaPos.magnitude;
                    float speed = distance / deltaTime;

                    if (speed > speedMin)
                    {
                        applyCorrection = true;
                        deltaPos = localDeltaPos;
                        if (speed > speedMax)
                        {
                            UpdateFreezeDuration(freezeDuration, false);
                            //Debug.Log($"AVATAR {_actorNumber} start freeze local " + _freezeDuration);
                        }
                        else if (speed > speedMin && _freezeStartTime == 0f)
                        {
                            float ratio = Mathf.InverseLerp(speedMin, speedMax, speed);
                            deltaPos *= ratio;
                        }
                    }
                }
                else
				{
                    deltaPos -= localDeltaPos;
                }
            }
        }

        if (applyCorrection)
        {
            _vrik.solver.AddPlatformMotion(deltaPos, Quaternion.identity, Vector3.zero);
        }

		if (_parent != null)
        {
            _lastLocalPos = localPos;
            _initLocalPos = true;
        }
    }
}
