namespace Photon.Pun
{
	using UnityEngine;
	using Valve.VR.InteractionSystem;

	[AddComponentMenu("Photon Networking/Photon Hand Over View")]
    [HelpURL("https://doc.photonengine.com/en-us/pun/v2/gameplay/synchronization-and-state")]
    [RequireComponent(typeof(PhotonView))]
    public class PhotonHandOverView : MonoBehaviour
    {
        private PhotonView _photonView = null;
		private HoverButton _hoverButton = null;

        public void Awake()
        {
            _photonView = GetComponent<PhotonView>();
			_hoverButton = GetComponent<HoverButton>();

		}

		public void PressButton()
		{
			_photonView?.RPC("RpcPressButton", RpcTarget.Others);
		}

		[PunRPC]
		private void RpcPressButton()
		{
			_hoverButton?.PressButton();
		}

		//public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		//{
		//	if (stream.IsWriting)
		//	{
		//		stream.SendNext(_circularDrice.outAngle);
		//	}
		//	else
		//	{
		//		_circularDrice.ForceOutAngle((float)stream.ReceiveNext());
		//	}
		//}


	}
}