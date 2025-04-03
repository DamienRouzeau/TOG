// Created by Ronis Vision. All rights reserved
// 27.10.2019.

using RVModules.RVUtilities.Extensions;
using UnityEngine;
using UnityEngine.AI;

namespace RVModules.RVSmartAI.Content
{
    /// <summary>
    /// IMovement implementation using Unity's NavMeshAgent
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class UnityNavMeshMovement : MonoBehaviour, IMovement
    {
        #region Fields

        [SerializeField]
        private bool reserveDestinationPosition = true;

        [SerializeField]
        [HideInInspector]
        private NavMeshAgent agent;

        private GameObject destPosBlocker;

        [SerializeField]
        private int destinationBlockLayer = 9;

        // serialized for debugging only
        [SerializeField]
        private Vector3 destination;

        // cached trasform access
        private new Transform transform;
        
        [SerializeField]
        private bool randomAvoidancePriority = true;

        [Tooltip("If closer to destination than this AtDestination will return true")]
        [SerializeField]
        private float atDestinationDistance = .2f;

        public Transform Transform => transform;

        #endregion

        #region Properties

        public Vector3 Velocity => agent.velocity;

        public float MovementSpeed
        {
            get => agent.speed;
            set => agent.speed = value;
        }

        public bool UpdatePosition
        {
            get => agent.updatePosition;
            set => agent.updatePosition = value;
        }

        public bool UpdateRotation
        {
            get => agent.updateRotation;
            set
            {
                if (agent == null) return;
                agent.updateRotation = value;
            }
        }

        public Vector3 Position
        {
            get => transform.position;
            set => agent.nextPosition = value;
        }

        public Quaternion Rotation => transform.rotation;

        public bool AtDestination => Destination == Vector3.zero || agent.isStopped || !agent.hasPath ||
                                     transform.position.ManhattanDistance2d(Destination) < atDestinationDistance;

        public Vector3 Destination
        {
            get => destination;
            set
            {
                agent.destination = value;
                destination = value;
                //destination = agent.destination;
                
                if (destination == Vector3.zero || destination.ManhattanDistance2d(transform.position) < atDestinationDistance)
                    agent.isStopped = true;
                else
                    agent.isStopped = false;

                if (ReserveDestinationPosition) destPosBlocker.transform.position = destination;
            }
        }

        /// <summary>
        /// Create 'blocker' object with collider that is set to destination position
        /// to avoid many agents trying to go to the same position
        /// </summary>
        public bool ReserveDestinationPosition
        {
            get => reserveDestinationPosition;
            set
            {
                if (value && destPosBlocker == null) CreateDestinationBlocker();
                if (!value && destPosBlocker != null) Destroy(destPosBlocker);
                reserveDestinationPosition = value;
            }
        }

        public int DestinationBlockLayer
        {
            get => destinationBlockLayer;
            set
            {
                destinationBlockLayer = value;
                if (destPosBlocker != null) destPosBlocker.layer = value;
            }
        }

        #endregion

        #region Not public methods

        /// <summary>
        /// Removes destination position blocker
        /// </summary>
        protected virtual void OnDestroy()
        {
            Destroy(agent);
            if (destPosBlocker == null) return;
            Destroy(destPosBlocker);
        }

        protected virtual void Awake()
        {
            transform = base.transform;
            agent = gameObject.AddOrGetComponent<NavMeshAgent>();
            
            if(randomAvoidancePriority) agent.avoidancePriority = Random.Range(0, 100);
            
            //agent.updateRotation = false;
//            agent.velocity = destination;
//            agent.angularSpeed = destPosBlockerLayer;

            if (!ReserveDestinationPosition) return;
            CreateDestinationBlocker();
        }

        protected virtual void CreateDestinationBlocker()
        {
            if (destPosBlocker != null) return;
            destPosBlocker = new GameObject(name + " destination blocker");
            var coll = destPosBlocker.AddComponent<SphereCollider>();
            coll.isTrigger = true;
            DestinationBlockLayer = destinationBlockLayer;
        }

        #endregion
    }
}