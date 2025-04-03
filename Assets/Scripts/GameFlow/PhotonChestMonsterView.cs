namespace Photon.Pun
{
    using UnityEngine;


    [AddComponentMenu("Photon Networking/Photon ChestMonster View")]
    [HelpURL("https://doc.photonengine.com/en-us/pun/v2/gameplay/synchronization-and-state")]
    [RequireComponent(typeof(PhotonView))]
    public class PhotonChestMonsterView : MonoBehaviour, IPunObservable
    {
        public enum TentacleState
		{
            Hidden,
            Show,
            Full,
            Hide
		}

        [SerializeField]
        private Animator _animator = null;
        [SerializeField]
        private Transform _centerTr = null;
        [SerializeField]
        private float _distanceToDetectPlayer = 8f;
        [SerializeField]
        private float _distanceToRageOn = 5f;
        [SerializeField]
        private float _rotationSpeed = 1f;
        [SerializeField]
        private float _tentacleSpeed = 1f;
        [SerializeField]
        private float _magnetSpeed = 1f;
        [SerializeField]
        private GameObject _tentacle = null;
        [SerializeField]
        private AudioSource _tentacleSFX = null;

        private PhotonView _photonView = null;
        private Vector3 _tentacleTarget = Vector3.zero;
        private Vector3 _tentacleDir = Vector3.zero;
        private float _angleTarget = 0f;
        private float _tentacleDistanceTarget = 0f;
        private float _tentacleDistance = 0f;
        private float _tentacleDistanceMax = 0f;
        private bool _needUpdateRotation = false;
        private bool _isOnRage = false;
        private TentacleState _tentacleState = TentacleState.Hidden;
        private float _tentacleStateTime = 0f;
        private Health _health = null;
        private int _targetId = -1;

        public void SetOnRage()
		{
            _animator.SetTrigger("CM_RageOn");
            _isOnRage = true;
        }

        public void SetIdle()
        {
            _animator.SetTrigger("CM_Idle");
            _isOnRage = false;
        }

        private void Awake()
        {
            _photonView = GetComponent<PhotonView>();
            _health = GetComponentInChildren<Health>();
            _health.onDieCallback += OnDie;
        }

		private void OnDestroy()
		{
            if (_health != null)
            {
                _health.onDieCallback -= OnDie;
                _health = null;
            }
            _photonView = null;
        }

		private void OnEnable()
        {
            _angleTarget = transform.rotation.y;
        }

		private void Update()
		{
            if (_health == null || _health.dead)
                return;
            if (_targetId == Player.myplayer.id && _tentacleState == TentacleState.Full && _tentacleDistance > 3f)
			{
                Player.myplayer.transform.localPosition -= _tentacleDir * Time.deltaTime * _magnetSpeed * 0.1f;
            }
		}

		private void LateUpdate()
		{
            if (_health == null || _health.dead)
                return;

            if (_photonView.IsMine)
            {
                Vector3 pos = _centerTr.position;
                Vector3 dir = Vector3.forward;
                float sqrDist = 0f;
                bool detect = false;
                int targetId = -1;
                Vector3 target = Vector3.zero;

                if (_targetId < 0 || _targetId == Player.myplayer.id)
                {
                    target = Player.myplayer.cam.transform.position - Vector3.up * 0.3f;
                    dir = (target - pos);
                    sqrDist = dir.sqrMagnitude;
                    detect = sqrDist < _distanceToDetectPlayer * _distanceToDetectPlayer && CheckWall(pos, dir);
                    targetId = Player.myplayer.id;
                }

                foreach (Player_avatar avatar in Player.myplayer.avatars)
                {
                    if (avatar.actornumber >= 0)
                    {
                        if (_targetId < 0 || _targetId == avatar.actornumber)
                        {
                            Vector3 targetAvatar = avatar.transform.position + Vector3.up;
                            Vector3 dirAvatar = (targetAvatar - pos);
                            float sqrDistAvatar = dirAvatar.sqrMagnitude;
                            if (!detect || sqrDistAvatar < sqrDist)
                            {
                                target = targetAvatar;
                                sqrDist = sqrDistAvatar;
                                dir = dirAvatar;
                                detect = sqrDist < _distanceToDetectPlayer * _distanceToDetectPlayer && CheckWall(pos, dir);
                                targetId = avatar.actornumber;
                            }
                        }
                    }
                }
                
                if (detect)
                {
                    _targetId = targetId;
                    dir.y = 0f;
                    _tentacleDir = dir;
                    float angle = Quaternion.LookRotation(dir, Vector3.up).eulerAngles.y;
                    if (Mathf.Abs(Mathf.DeltaAngle(angle, _angleTarget)) >= 1f)
                    {
                        //Debug.Log("[ANGLE] Set target angle " + _targetAngle);
                        _angleTarget = angle;
                        _needUpdateRotation = true;
                    }
                    if (!_isOnRage && sqrDist < _distanceToRageOn * _distanceToRageOn)
                    {
                        SetOnRage();
                        _photonView.RPC("SetOnRageRpc", RpcTarget.Others);
                    }
                    _tentacleTarget = target;
                    _tentacleDistanceTarget = Mathf.Sqrt(sqrDist) * 2f - 0.3f;
                }
                else
                {
                    if (_isOnRage)
					{
                        SetIdle();
                        _photonView.RPC("SetIdleRpc", RpcTarget.Others);
                    }
                    _targetId = -1;
                    _tentacleDistanceTarget = 0f;
                }
            }
            else if (_targetId == Player.myplayer.id)
			{
                Vector3 pos = _centerTr.position;
                Vector3 target = Player.myplayer.cam.transform.position - Vector3.up * 0.3f;
                Vector3 dir = (target - pos);
                dir.y = 0f;
                _tentacleDir = dir;
                _angleTarget = Quaternion.LookRotation(dir, Vector3.up).eulerAngles.y;
                _tentacleTarget = target;
            }

            if (_needUpdateRotation)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, _angleTarget, 0f), Time.deltaTime * _rotationSpeed);
                float deltaAngle = Mathf.Abs(Mathf.DeltaAngle(transform.rotation.eulerAngles.y, _angleTarget));
                //Debug.Log("[ANGLE] Update deltaAngle " + deltaAngle);
                if (deltaAngle < 1f)
                    _needUpdateRotation = false;
            }

            switch (_tentacleState)
			{
                case TentacleState.Hidden:
                    if (_tentacleDistanceTarget > 0)
					{
                        _tentacleStateTime = Time.time;
                        _tentacle.SetActive(true);
                        _tentacleState = TentacleState.Show;
                        _tentacleDistanceMax = _tentacleDistanceTarget;
                        _tentacleSFX.Play();
                    }
                    break;
                case TentacleState.Show:
                    if (_tentacleDistanceTarget > 0f)
                        _tentacleDistanceMax = _tentacleDistanceTarget;
                    float ratioShow = Mathf.Clamp01((Time.time - _tentacleStateTime) * _tentacleSpeed);
                    _tentacleDistance = _tentacleDistanceMax * ratioShow;
                    if (ratioShow == 1f)
                    {
                        _tentacleState = TentacleState.Full;
                    }
                    break;
                case TentacleState.Full:
                    if (_tentacleDistanceTarget > 0f)
                    {
                        _tentacleDistanceMax = _tentacleDistanceTarget;
                        _tentacleDistance = _tentacleDistanceMax;
                        if (_targetId == Player.myplayer.id)
                            Player.myplayer.EnableTeleport(false);
                    }
                    else
                    {
                        _tentacleStateTime = Time.time;
                        _tentacleState = TentacleState.Hide;
                        _tentacleSFX.Play();
                        if (_targetId == Player.myplayer.id)
                            Player.myplayer.EnableTeleport(true);
                    }
                    break;
                case TentacleState.Hide:
                    float ratioHide = 1f - Mathf.Clamp01((Time.time - _tentacleStateTime) * _tentacleSpeed);
                    _tentacleDistance = _tentacleDistanceMax * ratioHide;
                    if (ratioHide == 0f)
                    {
                        _tentacleState = TentacleState.Hidden;
                        _tentacle.SetActive(false);
                    }
                    break;
			}
            
            if (_tentacleState != TentacleState.Hidden)
            {
                _tentacle.transform.LookAt(_tentacleTarget);
                _tentacle.transform.localScale = new Vector3(5f, 5f, _tentacleDistance);
            }
        }

		private bool CheckWall(Vector3 pos, Vector3 dir)
		{
            dir.y = 0f;
            if (Physics.Raycast(pos, dir.normalized, _distanceToDetectPlayer, LayerMask.GetMask("Walls")))
                return false;
            return true;
		}

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_targetId);
                stream.SendNext(_angleTarget);
                stream.SendNext(_tentacleDistanceTarget);
                if (_tentacleDistanceTarget > 0f)
                    stream.SendNext(_tentacleTarget);
            }
            else
            {
                _targetId =  (int)stream.ReceiveNext();
                _angleTarget = (float)stream.ReceiveNext();
                _needUpdateRotation = true;
                _tentacleDistanceTarget = (float)stream.ReceiveNext();
                if (_tentacleDistanceTarget > 0f)
                {
                    _tentacleTarget = (Vector3)stream.ReceiveNext();
                }
            }
        }

        [PunRPC]
        private void SetOnRageRpc()
		{
            SetOnRage();
        }

        [PunRPC]
        private void SetIdleRpc()
        {
            SetIdle();
        }

        private void OnDie(Health h)
		{
            _tentacle.SetActive(false);
            _tentacleState = TentacleState.Hidden;
            if (_targetId == Player.myplayer.id)
                Player.myplayer.EnableTeleport(true);
        }
    }
}