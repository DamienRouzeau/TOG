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


    [AddComponentMenu("Photon Networking/Photon Avatar View")]
    [HelpURL("https://doc.photonengine.com/en-us/pun/v2/gameplay/synchronization-and-state")]
    [RequireComponent(typeof(PhotonView))]
    public class PhotonAvatarView : MonoBehaviour, IPunObservable
    {
        private float m_Distance;
        private float m_Angle;

        private PhotonView m_PhotonView;

        private Vector3 m_Direction;
        private Vector3 m_NetworkPosition;
        private Vector3 m_StoredPosition;

        private Quaternion m_NetworkRotation;

        public bool m_SynchronizePosition = true;
        public bool m_SynchronizeRotation = true;
        public bool m_SynchronizeScale = false;

        bool m_firstTake = false;

        public string _datatype = "global";

        public void Awake()
        {
            m_PhotonView = GetComponent<PhotonView>();
            switch (_datatype)
            {
                case "global":
                    m_StoredPosition = transform.position;
                    break;
                case "local":
                    m_StoredPosition = transform.localPosition;
                    break;
            }
            m_NetworkPosition = Vector3.zero;

            m_NetworkRotation = Quaternion.identity;
        }

        void OnEnable()
        {
            m_firstTake = true;
        }

        public void Update()
        {
            if (!this.m_PhotonView.IsMine)
            {
                switch (_datatype)
                {
                    case "global":
                        transform.position = this.m_NetworkPosition;
                        break;
                    case "local":
                        transform.localPosition = this.m_NetworkPosition;
                        break;
                }
                transform.rotation = this.m_NetworkRotation;
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                if (this.m_SynchronizePosition)
                {
                    switch (_datatype)
                    {
                        case "global":
                            this.m_Direction = transform.position - this.m_StoredPosition;
                            this.m_StoredPosition = transform.position;
                            stream.SendNext(transform.position);
                            break;
                        case "local":
                            this.m_Direction = transform.localPosition - this.m_StoredPosition;
                            this.m_StoredPosition = transform.localPosition;
                            stream.SendNext(transform.localPosition);
                            break;
                    }
                    stream.SendNext(this.m_Direction);
                }

                if (this.m_SynchronizeRotation)
                {
                    stream.SendNext(transform.rotation);
                }

                if (this.m_SynchronizeScale)
                {
                    stream.SendNext(transform.localScale);
                }
            }
            else
            {


                if (this.m_SynchronizePosition)
                {
                    this.m_NetworkPosition = (Vector3)stream.ReceiveNext();
                    this.m_Direction = (Vector3)stream.ReceiveNext();

                    if (m_firstTake)
                    {
                        switch (_datatype)
                        {
                            case "global":
                                transform.position = this.m_NetworkPosition;
                                break;
                            case "local":
                                transform.localPosition = this.m_NetworkPosition;
                                break;
                        }
                        this.m_Distance = 0f;
                    }
                    else
                    {
                        float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                        this.m_NetworkPosition += this.m_Direction * lag;
                        switch (_datatype)
                        {
                            case "global":
                                this.m_Distance = Vector3.Distance(transform.position, this.m_NetworkPosition);
                                break;
                            case "local":
                                this.m_Distance = Vector3.Distance(transform.localPosition, this.m_NetworkPosition);
                                break;
                        }
                    }
                }

                if (this.m_SynchronizeRotation)
                {
                    this.m_NetworkRotation = (Quaternion)stream.ReceiveNext();

                    if (m_firstTake)
                    {
                        this.m_Angle = 0f;
                        transform.rotation = this.m_NetworkRotation;
                    }
                    else
                    {
                        this.m_Angle = Quaternion.Angle(transform.rotation, this.m_NetworkRotation);
                    }
                }

                if (this.m_SynchronizeScale)
                {
                    transform.localScale = (Vector3)stream.ReceiveNext();
                }

                if (m_firstTake)
                {
                    m_firstTake = false;
                }
            }
        }
    }
}