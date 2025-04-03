// ----------------------------------------------------------------------------
// <copyright file="PhotonTransformView.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   Component to synchronize Transforms via PUN PhotonView.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------
//#define DEBUG_BULLET

namespace Photon.Pun
{
	using RootMotion.FinalIK;
	using UnityEngine;

    [AddComponentMenu("Photon Networking/Photon Transform View")]
    [HelpURL("https://doc.photonengine.com/en-us/pun/v2/gameplay/synchronization-and-state")]
    [RequireComponent(typeof(PhotonView))]
    public class PhotonTransformBulletView : MonoBehaviour, IPunObservable
    {
        public enum State
        {
            None,
            Start,
            Alive,
            StartHit,
            AliveHit,
            End
        }

        public bool isPhotonViewMine => PhotonNetworkController.soloMode || _photonView.IsMine;
        public bool needToRaycast => (GameflowBase.amIAlone || isPhotonViewMine) && _state != State.None && _rigidbody != null;

        private PhotonView _photonView = null;

        private Vector3 _networkPosition;
        private Vector3 _lastNetworkPosition;
#if !USE_KDK && !USE_BOD
        private Vector3 _previousLastNetworkPosition;
#endif

        private Quaternion _networkRotation;

        private Projectile _projectile = null;
        private GameObject _goChild = null;
        private Vector3 _startPosition = Vector3.zero;
        private Quaternion _startRotation = Quaternion.identity;
        private int _projectileId = -1;
        private State _state = State.None;
        private bool _areDataSent = false;
        private Vector3 _hitPosition = Vector3.zero;
        private Rigidbody _rigidbody = null;

        public void Awake()
        {
            _photonView = GetComponent<PhotonView>();
            _networkPosition = Vector3.zero;
            _networkRotation = Quaternion.identity;
            _projectile = gameObject.GetComponent<Projectile>();
            Debug.Assert(_projectile != null, $"No projectile found in {gameObject.name}!" );
            _goChild = gameObject.FindInChildren("object");
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void Update()
        {
            // OTHER BULLET
            if (!isPhotonViewMine)
            {
                if (_state != State.None)
                {
                    if (!_goChild.activeSelf && _state != State.End)
                    {
                        ActivateBullet();
                    }

                    if (_rigidbody != null)
                    {
                        float speedTime = Time.deltaTime * 10f;
                        transform.position = Vector3.Lerp(transform.position, this._networkPosition, speedTime);
                        transform.rotation = Quaternion.Lerp(transform.rotation, this._networkRotation, speedTime);
                    }
                }
                else
                {
                    if (_goChild.activeSelf)
                    {
                        DeactivateBullet();
                    }
                }
            }
        }

        public void LateUpdate()
        {
            if (GameflowBase.instance == null)
                return;

            // MY BULLET
            if (isPhotonViewMine)
            {
                if (_areDataSent || (GameflowBase.allFlows != null && GameflowBase.allFlows.Length == 1))
                {
                    _areDataSent = false;
                    if (_state == State.Start || _state == State.StartHit || _state == State.AliveHit)
                        _state = State.Alive;
                    if (_state == State.End)
                        _state = State.None;
                }

                if (_goChild.activeSelf)
                {
                    if (_state == State.None && (GameflowBase.instance.canPlayerShoot || GameflowBase.amIAlone))
                    {
                        _state = State.Start;
                        _startPosition = transform.position;
                        _startRotation = transform.rotation;
                    }
                }
                else
                {
                    if (_state == State.Alive)
                    {
                        _state = State.End;
                    }
                }
            }
        }

        private void ActivateBullet()
        {
            _goChild.SetActive(true);
            LogBullet($"myprojectile.tailfocus {_projectile.tailfocus}");
            if (_projectile.tailfocus != null)
            {
                _projectile.CheckAvatarCreator();
                LogBullet($"myprojectile.projectile_creator {_projectile.projectile_creator}");
                if (_projectile.projectile_creator != null)
                {
                    Player_avatar avatar = _projectile.projectile_creator.GetComponent<Player_avatar>();
                    LogBullet($"avatar {avatar}");
                    if (avatar != null)
                    {
                        _projectile.tailfocus.transform.SetParent(avatar.gameObject.transform);
                        _projectile.tailfocus.transform.position = _startPosition;
                        SkinPlayer skin = avatar.GetComponentInChildren<SkinPlayer>();
                        if (skin != null)
                        {
                            VRIK vrik = skin.GetComponent<VRIK>();
                            if (vrik != null)
                            {
                                ProjectileCannon projectileCannon = vrik.references.rightHand.GetComponentInChildren<ProjectileCannon>();
                                if (projectileCannon == null)
                                    projectileCannon = vrik.references.leftHand.GetComponentInChildren<ProjectileCannon>();
                                if (projectileCannon != null)
                                    _projectile.tailfocus.transform.position = projectileCannon.lookAt.position;
                            }
                        }
                    }
                }
            }
        }

        private void DeactivateBullet()
        {
            _goChild.SetActive(false);
            if (_projectile.tailfocus != null)
            {
                _projectile.tailfocus.transform.SetParent(_goChild.transform);
                _projectile.tailfocus.transform.localPosition = Vector3.zero;
            }
        }

        public void TriggerHit(GameObject obj, Vector3 hitPos, float dammage, int projectileId)
        {
            //Debug.Log($"DEBUGHIT TriggerHit {obj} dammage {dammage} projectileId {projectileId}");
            if (isPhotonViewMine)
            {
                if (_state == State.Start)
                    _state = State.StartHit;
                else
                    _state = State.AliveHit;
                _hitPosition = hitPos;
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (multiplayerlobby.mas != null) return;
            if (stream.IsWriting)
            {
                // MY BULLET
                stream.SendNext(_state);

                if (_state != State.None)
                {
                    if (_state == State.Start || _state == State.StartHit)
                    {
                        stream.SendNext(_startPosition);
                        stream.SendNext(_startRotation);
                        stream.SendNext(_projectile.id);

                        LogBullet($"send {_state} startPosition {_startPosition} startRotation {_startRotation}");
                    }

                    if (_rigidbody != null)
                    {
                        if (_state == State.StartHit || _state == State.AliveHit)
                        {
                            stream.SendNext(_hitPosition);
                            LogBullet($"send {_state} hitpos {_hitPosition} startPosition {_startPosition}");
                        }
                        else
                        {
                            stream.SendNext(transform.position);
                            LogBullet($"send {_state} currentpos {transform.position} startPosition {_startPosition}");
                        }
                        stream.SendNext(transform.rotation);
                    }

                    _areDataSent = true;
                }
            }
            else
            {
                // OTHER BULLET
                _state = (State)stream.ReceiveNext();
                if (_state != State.None)
                {
                    if (_state == State.Start || _state == State.StartHit)
                    {
                        _startPosition = (Vector3)stream.ReceiveNext();
                        _startRotation = (Quaternion)stream.ReceiveNext();
                        _projectileId = (int)stream.ReceiveNext();
                        if (_rigidbody != null)
                        {
                            _rigidbody.isKinematic = true;
                            _rigidbody.velocity = Vector3.zero;
                        }
                        _projectile.SetOldCoord(_startPosition);
                        _projectile.transform.position = _startPosition;
                        _projectile.transform.rotation = _startRotation;
                        _projectile.id = _projectileId;
                        _projectile.Initialize();
                        _lastNetworkPosition = _startPosition;
#if !USE_KDK && !USE_BOD
                        _previousLastNetworkPosition = _startPosition;
#endif
                        ActivateBullet();
                        LogBullet($"receive {_state} {_startPosition} {_startRotation}");
                    }

                    if (_rigidbody != null)
                    {
                        _networkPosition = (Vector3)stream.ReceiveNext();
                        _networkRotation = (Quaternion)stream.ReceiveNext();

                        LogBullet($"receive {_state} pos {_networkPosition} lastpos {_lastNetworkPosition}");

#if !USE_KDK && !USE_BOD
                        if (_state == State.StartHit || _state == State.AliveHit)
						{
							// Check collision with a raycast between 2 last received positions
							_projectile.SetOldCoord(_lastNetworkPosition);
							transform.position = _networkPosition;
							transform.rotation = _networkRotation;
							bool result = _projectile.RayCastOldToNew(_projectile.id, true);
							LogBullet($"First raycast result {result} from {_lastNetworkPosition} to {_networkPosition}");
							if (!result)
							{
								LogBullet($"Retry raycast from previous {_previousLastNetworkPosition}");
								// Retry raycast
								_projectile.SetOldCoord(_previousLastNetworkPosition);
								result = _projectile.RayCastOldToNew(_projectile.id, true);
								LogBullet($"Retry raycast result {result} from {_previousLastNetworkPosition} to {_networkPosition}");
								if (!result)
								{
									LogBullet($"Retry raycast from start {_startPosition}");
									_projectile.SetOldCoord(_startPosition);
									result = _projectile.RayCastOldToNew(_projectile.id, true);
									LogBullet($"Retry raycast result {result} from {_startPosition} to {_networkPosition}");
								}
							}
							if (result)
								DeactivateBullet();
						}
                        _previousLastNetworkPosition = _lastNetworkPosition;
#endif
                        _lastNetworkPosition = _networkPosition;
                    }
                }
            }
        }

        [System.Diagnostics.Conditional("DEBUG_BULLET")]
        private void LogBullet(string msg)
        {
			if (_projectile != null && _projectile.id >= 0)
			{
				Debug.Log($"[BULLET] PhotonView {_photonView.ViewID} ProjectileId {_projectile.id} {msg}");
			}
		}
    }
}