// ----------------------------------------------------------------------------
// <copyright file="PhotonTransformView.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   Component to synchronize Transforms via PUN PhotonView.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------


namespace Photon.Pun
{
    using UnityEngine;

    [AddComponentMenu("Photon Networking/Photon Transform Game View")]
    [HelpURL("https://doc.photonengine.com/en-us/pun/v2/gameplay/synchronization-and-state")]
    [RequireComponent(typeof(PhotonView))]
    public class PhotonTransformGameView : MonoBehaviour, IPunObservable
    {
        public delegate string OnSendingData();
        public delegate void OnReceivingData(string data, bool forceReceive = false);

        private PhotonView m_PhotonView;

        public string m_Data = "";

        public string sending = "";
        public string receiving = "";

        public OnSendingData onSendingDataCbk = null;
        public OnReceivingData onReceivingDataCbk = null;

        public void Awake()
        {            
            m_PhotonView = GetComponent<PhotonView>();
            m_PhotonView.ObservedComponents[0] = (gameObject.GetComponent<PhotonTransformGameView>());
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                if (onSendingDataCbk != null)
                    this.m_Data = onSendingDataCbk();
                stream.SendNext(this.m_Data);
                sending = this.m_Data;
            }
            else
            {
                this.m_Data = (string)stream.ReceiveNext();
                if (onReceivingDataCbk != null)
                    onReceivingDataCbk(this.m_Data);
                receiving = this.m_Data;
            }
        }
    }
}