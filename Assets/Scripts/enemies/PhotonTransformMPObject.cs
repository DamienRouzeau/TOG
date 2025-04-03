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

    [AddComponentMenu("Photon Networking/Photon Transform MP View")]
    [HelpURL("https://doc.photonengine.com/en-us/pun/v2/gameplay/synchronization-and-state")]
    [RequireComponent(typeof(PhotonView))]
    public class PhotonTransformMPObject : MonoBehaviour, IPunObservable
    {
        private float m_Distance;
        private float m_Angle;

        private PhotonView m_PhotonView;
        
        private Vector3 m_Direction;
        private Vector3 m_NetworkPosition;
        private Vector3 m_StoredPosition;

        private Quaternion m_NetworkRotation;

        bool m_firstTake = false;

        public bool dataready = false;
        public GameObject[] rotationstoapply = new GameObject[0];
        public float[] m_Angles;
        public Quaternion[] m_NetworkRotations;

        public void Awake()
        {
            m_PhotonView = GetComponent<PhotonView>();
            m_PhotonView.ObservedComponents[0] = (gameObject.GetComponent<PhotonTransformMPObject>());
            m_PhotonView = GetComponent<PhotonView>();

            m_StoredPosition = transform.position;
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
                transform.position = Vector3.MoveTowards(transform.position, this.m_NetworkPosition, this.m_Distance * (1.0f / PhotonNetwork.SerializationRate));
                transform.rotation = Quaternion.RotateTowards(transform.rotation, this.m_NetworkRotation, this.m_Angle * (1.0f / PhotonNetwork.SerializationRate));

                for (int i = 0; i < rotationstoapply.Length; i++)
                {
                    rotationstoapply[i].transform.rotation = Quaternion.RotateTowards(rotationstoapply[i].transform.rotation, m_NetworkRotations[i], m_Angles[i] * (1.0f / PhotonNetwork.SerializationRate));
                }
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (!dataready) return;
            if (stream.IsWriting)
            {
                this.m_Direction = transform.position - this.m_StoredPosition;
                this.m_StoredPosition = transform.position;

                stream.SendNext(transform.position);
                stream.SendNext(this.m_Direction);
                stream.SendNext(transform.rotation);

                foreach (GameObject obj in rotationstoapply)
                    stream.SendNext(obj.transform.rotation);
            }
            else
            {
                this.m_NetworkPosition = (Vector3)stream.ReceiveNext();
                this.m_Direction = (Vector3)stream.ReceiveNext();
                this.m_NetworkRotation = (Quaternion)stream.ReceiveNext();
                for (int i = 0; i < rotationstoapply.Length; i++)
                    m_NetworkRotations[i] = (Quaternion)stream.ReceiveNext();

                if (m_firstTake)
                {
                    transform.position = this.m_NetworkPosition;
                    this.m_Distance = 0f;
                    this.m_Angle = 0f;
                    transform.rotation = this.m_NetworkRotation;
                    for (int i = 0; i < rotationstoapply.Length; i++)
                    {
                        rotationstoapply[i].transform.rotation = m_NetworkRotations[i];
                        m_Angles[i] = 0;
                    }
                    m_firstTake = false;
                }
                else
                {
                    float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                    this.m_NetworkPosition += this.m_Direction * lag;
                    this.m_Distance = Vector3.Distance(transform.position, this.m_NetworkPosition);
                    this.m_Angle = Quaternion.Angle(transform.rotation, this.m_NetworkRotation);
                    for (int i = 0; i < rotationstoapply.Length; i++)
                        m_Angles[i] = Quaternion.Angle(rotationstoapply[i].transform.rotation, m_NetworkRotations[i]);
                }
            }
        }
    }
}