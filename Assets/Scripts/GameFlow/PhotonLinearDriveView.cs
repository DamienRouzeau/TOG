namespace Photon.Pun
{
	using UnityEngine;
    using Valve.VR.InteractionSystem;

    [AddComponentMenu("Photon Networking/Photon Linear Drive View")]
    [HelpURL("https://doc.photonengine.com/en-us/pun/v2/gameplay/synchronization-and-state")]
    [RequireComponent(typeof(PhotonView))]
    public class PhotonLinearDriveView : MonoBehaviour
    {
        private LinearDrive _linearDrice = null;
		private PhotonView _photonView = null;
		private bool _isDriving = false;

        public void Awake()
        {
            _linearDrice = GetComponent<LinearDrive>();
			_photonView = GetComponent<PhotonView>();
		}
		 
		private void OnEnable()
		{
			_linearDrice.onDrivingEvent += OnDrivingEvents;
		}

		private void OnDisable()
		{
			_linearDrice.onDrivingEvent -= OnDrivingEvents;
		}

		public void SetValue(float value)
		{
			_photonView.RPC("RpcSetValue", RpcTarget.Others, value);
		}

		private void OnDrivingEvents(bool drive, float value)
		{
			if (_isDriving && drive)
				SetValue(value);
			_isDriving = drive;
		}

		[PunRPC]
		private void RpcSetValue(float value)
		{
			_linearDrice.SetValue(value);
		}
	}
}