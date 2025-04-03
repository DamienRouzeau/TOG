// Created by Ronis Vision. All rights reserved
// 26.02.2021.

using System.Collections.Generic;
using RVHonorAI.Animation;
using RVModules.RVLoadBalancer;
using RVModules.RVSmartAI.Content;
using RVModules.RVUtilities;
using UnityEngine;

namespace RVHonorAI
{
    /// <summary>
    /// Simple character animation component
    /// </summary>
    public class CharacterAnimation : MonoBehaviour, ICharacterAnimation, ICharacterAnimationContainer
    {
        #region Fields

        private static readonly int y = Animator.StringToHash("y");
        private static readonly int x = Animator.StringToHash("x");

        private static readonly int state1 = Animator.StringToHash("State");
        private static readonly int rotating = Animator.StringToHash("rotating");
        private static readonly int rotation = Animator.StringToHash("rotation");
        private static readonly int moving = Animator.StringToHash("moving");

        [SerializeField]
        private MovementAnimations movementAnimations;

        [SerializeField]
        private MovementAnimations combatMovementAnimations;

        [SerializeField]
        private SingleAnimations singleAnimations;

        private IMovement movement;

        private Vector2 velocity = Vector2.zero;

        private float velocityMul = 1;

        private new Transform transform;

        private Vector3 movementVelocity;

        [Tooltip("How quickly character transition between different animations")]
        [SerializeField]
        private float velocityDeltaSpeed = 3f;

        [SerializeField]
        private Animator animator;

        [SerializeField]
        private bool useRootMotion;

        [SerializeField]
        [HideInInspector]
        private bool autoUpdateAnimatorController = true;

        private int[] attackAnimations;

        #endregion

        #region Properties

        public bool UseRootMotion => useRootMotion;

        public virtual bool Rotating
        {
            set
            {
                if (Animator == null) return;
                //isRotating = value;
                Animator.SetBool(rotating, value);
            }
        }

        public virtual float RotatingSpeed
        {
            set => Animator?.SetFloat(rotation, value);
        }

        public virtual bool Moving
        {
            set => Animator.SetBool(moving, value);
        }

        public Animator Animator
        {
            get => animator;
            protected set => animator = value;
        }

        public MovementAnimations MovementAnimations
        {
            get => movementAnimations;
            set => movementAnimations = value;
        }

        public MovementAnimations CombatMovementAnimations
        {
            get => combatMovementAnimations;
            set => combatMovementAnimations = value;
        }

        public SingleAnimations SingleAnimations
        {
            get => singleAnimations;
            set => singleAnimations = value;
        }

        #endregion

        #region Public methods

        public virtual void FindReferences()
        {
            animator = GetComponentInParent<Animator>();
            movement = GetComponentInParent<IMovement>();
        }

        public virtual void SetState(int _state)
        {
            //state = _state;
            if (Animator == null) return;
            Animator.SetInteger(state1, _state);
        }

        public virtual void PlayAttackAnim()
        {
            if (SingleAnimations.attackingAnimations.Length == 0) return;
            Animator.SetTrigger(attackAnimations[Random.Range(0, SingleAnimations.attackingAnimations.Length)]);
        }

        public virtual void PlayDeathAnimation()
        {
            if (SingleAnimations.dyingAnimations.Length == 0) return;
            Animator.applyRootMotion = true;
            Animator.SetTrigger("dying" + Random.Range(0, SingleAnimations.dyingAnimations.Length));
        }

        #endregion

        #region Not public methods

        /// <summary>
        /// Register this component logic to LB system
        /// </summary>
        private void Register()
        {
            if (UseRootMotion)
                LB.Register(this, RootMotionAnimationLogic, new LoadBalancerConfig(LoadBalancerType.EveryXFrames, 0));
            else
                LB.Register(this, SetAnimVelocitiesFromAgentVelocity, new LoadBalancerConfig(LoadBalancerType.EveryXFrames, 0));
        }

        /// <summary>
        /// Unregister from LB system
        /// </summary>
        private void Unregister() => LB.Unregister(this);

        private void RootMotionAnimationLogic(float _dt)
        {
            //var worldDeltaPosition = (agent.nextPosition - transform.position);
//        var dx = Vector3.Dot(transform.right, worldDeltaPosition);
//        var dy = Vector3.Dot(transform.forward, worldDeltaPosition);

            SetAnimVelocitiesFromAgentVelocity(_dt);

            // decoupling from navmesh agent...
            movement.Position = transform.position;
//            agent.nextPosition = transform.position;

//        if (worldDeltaPosition.magnitude > agent.radius * .1f)
//        {
//            var deltaTime = UnityTime.DeltaTime;
//            transform.position = Vector3.MoveTowards(transform.position, agent.nextPosition,
//                (1 - animVsNavagentPullingWeight) * pullingVelocity * deltaTime);
//            agent.nextPosition =
//                Vector3.MoveTowards(agent.nextPosition, transform.position, animVsNavagentPullingWeight * pullingVelocity * deltaTime);
//        }
        }

        private void SetAnimVelocitiesFromAgentVelocity(float _dt)
        {
            movementVelocity = movement.Velocity;

            var rot = transform.rotation;
            var right = rot * Vector3.right;
            var forward = rot * Vector3.forward;

            var dx = Vector3.Dot(right, movementVelocity);
            var dy = Vector3.Dot(forward, movementVelocity);

            SetAnimValues(dx, dy, velocityDeltaSpeed);
        }

        private void SetAnimValues(float _dx, float _dy, float _velocityDeltaSpeed)
        {
            var deltaPosition = new Vector2(_dx, _dy);

            // never change this to move towards! it causes jittery animation blending and movement!
            velocity = Vector2.Lerp(velocity, deltaPosition, UnityTime.DeltaTime * _velocityDeltaSpeed);
            //velocity = Vector2.MoveTowards(velocity, deltaPosition, UnityTime.DeltaTime * velocityDeltaSpeed);

            animator.SetFloat(x, velocity.x * velocityMul);
            animator.SetFloat(y, velocity.y * velocityMul);
        }

        protected virtual void Awake()
        {
            // cache transform access
            transform = base.transform;
            FindReferences();

            // create attack animation hashes
            var atkAnimCount = SingleAnimations.attackingAnimations.Length;
            var atkAnimations = new List<int>(atkAnimCount);
            for (var i = 0; i < atkAnimCount; i++) atkAnimations.Add(Animator.StringToHash($"attacking{i}"));
            attackAnimations = atkAnimations.ToArray();
        }

        protected virtual void Start()
        {
            Animator.applyRootMotion = UseRootMotion;
            movement.UpdatePosition = !UseRootMotion;

            ICharacter character = GetComponentInParent<ICharacter>();
            // turn off animation events to avoid stupid Unity "AnimationEvent 'NewEvent' has no receiver" logs
            character.OnKilled.AddListener(() =>
            {
                if (this == null) return;
                if (Animator == null) return;
                Animator.fireEvents = false;
            });
        }

        protected virtual void OnEnable() => Register();

        protected virtual void OnDisable() => Unregister();

        #endregion

//    [SerializeField]
//    [HelpAttribute("How much animation transform will be pulled toward navigation agent vs vice-versa; it's trade-off between animation quality " +
//                   "vs navmesh accuracy, at 1 only animations will move ai, at 0 only navmesh agent.")]
//    [Range(0, 1)]
//    private float animVsNavagentPullingWeight = .5f;
//
//    [SerializeField]
//    [Range(0, 4)]
//    private float pullingVelocity = 2;

        //private bool isRotating;
    }
}