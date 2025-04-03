namespace Photon.Pun
{
	using UnityEngine;
    using Valve.VR.InteractionSystem;

    [AddComponentMenu("Photon Networking/Photon Circular Drive View")]
    [HelpURL("https://doc.photonengine.com/en-us/pun/v2/gameplay/synchronization-and-state")]
    [RequireComponent(typeof(PhotonView))]
    public class PhotonCircularDriveView : MonoBehaviour
    {
        private CircularDrive _circularDrice = null;
		private PhotonView _photonView = null;
		private bool _isDriving = false;

        public void Awake()
        {
            _circularDrice = GetComponent<CircularDrive>();
			_photonView = GetComponent<PhotonView>();
		}

		private void OnEnable()
		{
			_circularDrice.onDrivingEvent += OnDrivingEvents;
		}

		private void OnDisable()
		{
			_circularDrice.onDrivingEvent -= OnDrivingEvents;
		}

		public void StartDriving()
		{
			_photonView.RPC("RpcStartDriving", RpcTarget.Others);
		}

		public void StopDriving()
		{
			_photonView.RPC("RpcStopDriving", RpcTarget.Others);
		}

		public void SetOutAngle(float angle)
		{
			_photonView.RPC("RpcSetOutAngle", RpcTarget.Others, angle);
		}

		private void OnDrivingEvents(bool drive, float angle)
		{
			if (_isDriving)
			{
				if (!drive)
					StopDriving();
				else
					SetOutAngle(angle);
			}
			else
			{
				if (drive)
					StartDriving();
			}
			_isDriving = drive;
		}

		[PunRPC]
		private void RpcStartDriving()
		{
			_circularDrice.ForceDriving(true);
		}

		[PunRPC]
		private void RpcStopDriving()
		{
			_circularDrice.ForceDriving(false);
		}

		[PunRPC]
		private void RpcSetOutAngle(float angle)
		{
			_circularDrice.ForceOutAngle(angle);
		}
	}
}