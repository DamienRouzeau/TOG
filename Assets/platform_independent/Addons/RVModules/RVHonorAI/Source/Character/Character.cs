// Created by Ronis Vision. All rights reserved
// 21.03.2021.

using System;
using RVHonorAI.Animation;
using RVModules.RVCommonGameLibrary.Audio;
using RVModules.RVCommonGameLibrary.Gameplay;
using RVModules.RVLoadBalancer;
using RVModules.RVLoadBalancer.Tasks;
using RVModules.RVSmartAI;
using RVModules.RVSmartAI.Content;
using RVModules.RVUtilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace RVHonorAI
{
    /// <summary>
    /// Character description
    /// todo unity events with parameters
    /// </summary>
    [DefaultExecutionOrder(-100)] public class Character : MonoBehaviour, ICharacter, IAttackAngle, IAttackRange, IAttackSoundPlayer, IAttacker,
        IJobHandlerProvider, IObjectDetectionCallbacks, ITargetsDetectionCallbacks
    {
        #region Fields

        public RagdollCreator ragdollCreator;

        [SerializeField]
        public CharacterSounds characterSounds;

        [SerializeField]
        private FloatUnityEvent onReceivedDamage;

        [SerializeField]
        private UnityEvent onAttack;

        [SerializeField]
        private float walkingSpeed = 1f;

        [SerializeField]
        private float runningSpeed = 2f;

        [SerializeField]
        private float armor = 20;

        [SerializeField]
        private ICharacterAi characterAi;

        [SerializeField]
        private float health = 100;

        [SerializeField]
        private float maxHealth = 100;

        [SerializeField]
        private bool healthRegeneration;

        [SerializeField]
        [Tooltip("Health regeneration per second")]
        private float healthRegenerationSpeed = 1;

        [Tooltip("Should ragdoll be created when character is killed instaed of playing dying animation")]
        [SerializeField]
        private bool useRagdoll;

        [SerializeField]
        [FormerlySerializedAs("weapon")]
        private Attack attack;

        [SerializeField]
        protected bool reserveDestinationPosition;

        [SerializeField]
        private bool showGuiInfo;

        [Tooltip("Optional transform that will be aimed for by other characters for shooting etc")]
        [SerializeField]
        private Transform aimTransform;

        [SerializeField]
        private UnityEvent onKilled;

        [SerializeField]
        protected TaskHandler aiJobHandler = new TaskHandler {MaxRunningTasks = 20};

        [SerializeField]
        private bool removeDead = true;

        [Tooltip("Will remove dead after this many seconds. Applies to ragdoll also")]
        [SerializeField]
        private float removeDeadAfter = 30;

        [SerializeField]
        private bool useSoundPreset;

        [SerializeField]
        private SoundsPreset soundsPreset;

        private AudioSource audioSource;

        private float lastTimeAttack = float.MinValue;

        private IMovementRootMotionHandler movementRootMotionHandler;

        [SerializeField]
        private bool debugObjectDetecionEvents, debutTargetDetectionEvents;

        #endregion

        #region Properties

        public Transform AimTransform
        {
            get
            {
                if (aimTransform != null) return aimTransform;
                return HeadTransform != null ? HeadTransform : transform;
            }
            set => aimTransform = value;
        }

        public TaskHandler AiJobHandler => aiJobHandler;

        public float MaxHealth => maxHealth;

        public UnityEvent OnKilled
        {
            get => onKilled;
            set => onKilled = value;
        }

        /// <summary>
        /// Should ragdoll be created when character is killed instaed of playing dying animation
        /// </summary>
        public bool UseRagdoll => useRagdoll;

        /// <summary>
        /// Value for determining generally how strong and dangerous this char is
        /// </summary>
        public float Danger => (Health * .1f * DamagePerSecond) * .1f;

        public Transform Transform => transform;

        public Transform HeadTransform
        {
            get => CharacterAi.HeadTransform;
            protected set => CharacterAi.HeadTransform = value;
        }

        /// <summary>
        /// Character's health
        /// </summary>
        public float Health
        {
            get => health;
            protected set => health = Mathf.Clamp(value, 0, maxHealth);
        }

        /// <summary>
        /// Character's AiGroup. Used for detecting relationship to other characters
        /// </summary>
        public AiGroup AiGroup
        {
            get => CharacterAi.AiGroup;
            set => CharacterAi.AiGroup = value;
        }

        public virtual float DamagePerSecond => CurrentAttack == null ? 0 : CurrentAttack.Damage / CurrentAttack.AttackInterval;

        public virtual float Damage => CurrentAttack == null ? 0 : CurrentAttack.Damage;

        public virtual float Armor
        {
            get => armor;
            protected set => armor = value;
        }

        public virtual ICharacterAi CharacterAi
        {
            get => characterAi;
            private set => characterAi = value;
        }

        public virtual float AttackRange => CurrentAttack == null ? 0f : CurrentAttack.Range;

        public virtual float AttackAngle => CurrentAttack == null ? 0 : CurrentAttack.AttackAngle;

        public Object GetObject => this;

        public bool HealthRegeneration => healthRegeneration;

        public Transform VisibilityCheckTransform => CharacterAi.HeadTransform == null ? transform : CharacterAi.HeadTransform;

        public System.Action<float, Object> OnReceivedDamageAction { get; set; }

        /// <summary>
        /// Action called when character attacks his target
        /// Second argument is dealth damage
        /// </summary>
        public Action<IDamageable, float> OnAttackAction { get; set; }

        /// <summary>
        /// Used attack
        /// </summary>
        public Attack CurrentAttack
        {
            get => attack;
            protected set => attack = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool UseSoundPreset => useSoundPreset;

        /// <summary>
        /// Sounds preset
        /// </summary>
        public SoundsPreset SoundsPreset
        {
            get => soundsPreset;
            protected set => soundsPreset = value;
        }

        /// <summary>
        /// Running speed in m/s
        /// </summary>
        public float RunningSpeed
        {
            get => runningSpeed;
            set => runningSpeed = value;
        }

        /// <summary>
        /// Walking speed in m/s
        /// </summary>
        public float WalkingSpeed
        {
            get => walkingSpeed;
            set => walkingSpeed = value;
        }

        /// <summary>
        /// Unity event called when Character deals damage
        /// </summary>
        public UnityEvent OnAttack
        {
            get => onAttack;
            set => onAttack = value;
        }

        /// <summary>
        /// Unity event called when Character gets damage
        /// </summary>
        public FloatUnityEvent OnReceivedDamage
        {
            get => onReceivedDamage;
            set => onReceivedDamage = value;
        }

        ITarget ITargetProvider.Target => characterAi.Target;

        TargetInfo ITargetProvider.TargetInfo
        {
            get => characterAi.TargetInfo;
            set => characterAi.TargetInfo = value;
        }

        public Action<Object> OnNewObjectDetected { get; set; }
        public Action<Object> OnObjectNotDetectedAnymore { get; set; }
        public Action<ITarget> OnNewTargetDetected { get; set; }
        public Action<ITarget> OnTargetNotSeenAnymore { get; set; }
        public Action<ITarget> OnTargetVisibleAgain { get; set; }
        public Action<ITarget> OnTargetForget { get; set; }

        #endregion

        #region Public methods

        /// <summary>
        /// Receive set damage. Returned value is actual dealt damage  
        /// </summary>
        public virtual float ReceiveDamage(float _damage, Object _damageSource, DamageType _damageType, Vector3 _hitPoint = default,
            Vector3 _hitForce = default, float forceRadius = default)
        {
            if (this == null) return 0;

            //if (_source != null) MakeEnemyDynamic(_source);
            if (RandomChance.Get(characterSounds.chanceToPlayGotHitSound))
                AudioManager.Instance.PlaySound(transform.position, characterSounds.gotHitSound, audioSource, true);

            var dmg = HonorAIManager.CalculateDamage(this, _damage, _damageType);
            if (dmg <= 0)
            {
                dmg = 0;
            }
            else
            {
                OnReceivedDamageAction?.Invoke(dmg, _damageSource);
                OnReceivedDamage?.Invoke(dmg);
            }

            Health -= dmg;
            if (!(Health <= 0)) return dmg;

            Kill(_hitPoint, _hitForce, forceRadius);
            return dmg;
        }

        /// <summary>
        /// Instantly kills this character, plays die sound and creates ragdoll or play killed animation depending on configuration 
        /// </summary>
        public virtual void Kill() => Kill(Vector3.zero);

        /// <summary>
        /// Instantly kills this character, plays die sound and creates ragdoll or play killed animation depending on configuration 
        /// </summary>
        public virtual void Kill(Vector3 hitPoint, Vector3 hitForce = default, float forceRadius = default)
        {
            onKilled?.Invoke();
            audioSource.Stop();
            AudioManager.Instance.PlaySound(transform.position, characterSounds.dieSound);

            if (UseRagdoll) KilledRagdoll(hitPoint, hitForce, forceRadius);
            else KilledAnimation();
        }

//        /// <summary>
//        /// Add passed characters to dynamic enemies list, and make him enemy despite group relationship
//        /// todo not implemented!
//        /// </summary>
//        public void MakeEnemyDynamic(ICharacter _source) => throw new NotImplementedException();

        public virtual void Heal(float _amount) => Health += Mathf.Clamp(_amount, 0, float.MaxValue);

        public virtual bool IsEnemy(IRelationshipProvider _other, bool _contraCheck = false) => CharacterAi.IsEnemy(_other, _contraCheck);

        public virtual bool IsAlly(IRelationshipProvider _other) => CharacterAi.IsAlly(_other);

        /// <summary>
        /// Finds references to CharacterAi and AudioSource using GetComponentInChildren
        /// </summary>
        public virtual void FindReferences()
        {
            CharacterAi = GetComponentInChildren<CharacterAi>();
            audioSource = GetComponentInChildren<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }

        /// <summary>
        /// Plays foot step sound
        /// </summary>
        public virtual void PlayFootstepSound() => AudioManager.Instance.PlaySound(transform.position, characterSounds.footstepSounds);

        /// <summary>
        /// Plays attack sound with random chance
        /// </summary>
        public virtual void PlayAttackSound()
        {
            if (!RandomChance.Get(characterSounds.chanceToPlayAttackSound)) return;
            AudioManager.Instance.PlaySound(transform.position, characterSounds.attackSound);
        }

        /// <summary>
        /// Use attack, will call MeleeAttack if have melee attack,
        /// or Shoot if have shooting type attack
        /// </summary>
        public virtual void Attack()
        {
            // should we log somethig here?
            if (attack == null) return;

            if (UnityTime.Time - lastTimeAttack < attack.AttackInterval) return;
            lastTimeAttack = UnityTime.Time;

            switch (attack.AttackType)
            {
                case AttackType.Melee:
                    MeleeAttack();
                    return;
                case AttackType.Shooting:
                    Shoot();
                    break;
            }
        }

        #endregion

        #region Not public methods

        /// <summary>
        /// This deals damage to current target using current attack or returns if theres no current target
        /// </summary>
        protected virtual void MeleeAttack()
        {
            var target = CharacterAi.Target;

            PlayAttackAttackSound();

            if (transform == null || this == null || target as Object == null) return;

            if (IsTargetInAttackRange(target)) return;

            var damageable = target as IDamageable;
            float dmg = 0;
            if (damageable != null)
            {
                var damageType = DamageType.Physical;
                if (CurrentAttack != null) damageType = CurrentAttack.DamageType;
                dmg = damageable.ReceiveDamage(HonorAIManager.CalculateAttack(this), this, damageType);
            }

            // for some reason sometimes this (Character component) doesnt exist, but this code still executes
            if (this == null) return;
            if (dmg > 0) PlayAttackHitSound();

            OnAttackAction?.Invoke(damageable, dmg);
            OnAttack?.Invoke();
        }

        protected bool IsTargetInAttackRange(ITarget target) => Vector3.Distance(transform.position, target.Transform.position) > AttackRange;

        protected virtual void PlayAttackHitSound() =>
            AudioManager.Instance.PlaySound(transform.position, CurrentAttack.hitSound);

        protected virtual void PlayAttackAttackSound() =>
            AudioManager.Instance.PlaySound(transform.position, CurrentAttack.attackSound);

        protected virtual void OnEnable()
        {
            CharacterAi.Enabled = true;
            LB.Register(this, Tick, 1);
            HonorAIManager.Instance.activeCharactersCount++;
        }

        protected virtual void OnDisable()
        {
            CharacterAi.Enabled = false;
            LB.Unregister(this);
            HonorAIManager.Instance.activeCharactersCount--;
        }

        /// <summary>
        /// Shoots current attack at target,
        /// animation independent
        /// </summary>
        protected virtual void Shoot()
        {
            var target = CharacterAi.Target;
            if (target as Object == null) return;

            if (CurrentAttack != null && CurrentAttack.AttackType == AttackType.Shooting)
            {
                var sw = CurrentAttack as ShootingAttack;
                if (sw != null) sw.Shoot(this, target);
            }
        }

        protected virtual void KilledAnimation()
        {
            if (removeDead) RemoveDeadBody(gameObject);

            // todo instaed of referencing animation system, make anim system hook into killed event of character (to consider?)
            CharacterAi.CharacterAnimation.PlayDeathAnimation();
            if (movementRootMotionHandler != null) Destroy(movementRootMotionHandler as Object);
            Destroy(CharacterAi.Movement as Object);
            Destroy(GetComponent<LookAt>());
            if (characterAi.CharacterAnimation != null) (characterAi.CharacterAnimation as MonoBehaviour).enabled = false;
            Destroy(this);
            Destroy(characterAi.Movement as Object);
            Destroy(CharacterAi.Ai.gameObject);
            Destroy(CharacterAi as Object);
            Destroy(GetComponent<Collider>());
        }

        protected virtual void KilledRagdoll(Vector3 hitPoint = default, Vector3 hitForce = default, float forceRadius = default)
        {
            var ragdoll = ragdollCreator.Create(characterAi.Movement.Velocity, transform.localScale);
            ragdollCreator.ApplyPointForce(hitForce, hitPoint, forceRadius);
            if (removeDead && ragdoll != null) RemoveDeadBody(ragdoll);

            Destroy(gameObject);
        }

        protected virtual void RemoveDeadBody(GameObject _gameObject)
        {
            var destro = _gameObject.AddComponent<DestroyGameObjectAfter>();
            destro.destroyAfter = removeDeadAfter;
        }

        protected virtual void Awake()
        {
            if (debugObjectDetecionEvents)
            {
                OnNewObjectDetected += _o => Debug.Log($"{name} detected new object: {_o}", _o);
                OnObjectNotDetectedAnymore += _o => Debug.Log($"{name} not detected object anymore: {_o}", _o);
            }

            if (debutTargetDetectionEvents)
            {
                OnNewTargetDetected += _target => Debug.Log($"{name} saw new target: {_target}", _target as Object);
                OnTargetNotSeenAnymore += _target => Debug.Log($"{name} lost sight of target: {_target}", _target as Object);
                OnTargetVisibleAgain += _target => Debug.Log($"{name} saw target again: {_target}", _target as Object);
                OnTargetForget += _target => Debug.Log($"{name} forgot about target: {_target}", _target as Object);
            }

            SetupRagdoll();
            FindReferences();
            SetupMovement();

            if (UseSoundPreset) characterSounds = SoundsPreset.characterSounds;
            HonorAIManager.Instance.totalCharactersCount++;
        }

        protected virtual void SetupRagdoll()
        {
            ragdollCreator.sourceRoot = transform;
            ragdollCreator.Initialize();
        }

        protected virtual void Start()
        {
            if (showGuiInfo)
            {
                Transform t;
                Instantiate(Resources.Load<GameObject>("CharCanvas"), (t = transform).position + Vector3.up * 2.1f, t.rotation, t);
            }

            SetupRMHandler();
        }

        protected virtual void SetupMovement()
        {
            var movement = gameObject.GetComponent<IMovement>();
            if (movement != null) movement.ReserveDestinationPosition = reserveDestinationPosition;
        }

        protected virtual void SetupRMHandler()
        {
            if (!CharacterAi.CharacterAnimation.UseRootMotion) return;
            if (movementRootMotionHandler == null)
                movementRootMotionHandler = gameObject.AddComponent<NavMeshAgentRootMotionHandler>();
        }

        /// <summary>
        /// Health regen and fight sounds logic
        /// todo fight sounds immplement as ai tasks(?)
        /// to consider - health regen as ai task ?
        /// </summary>
        private void Tick(float _dt)
        {
            if (healthRegeneration) HealthRegenerationLogic();

            if (CharacterAi.CharacterState != CharacterState.Combat) return;
            if (characterSounds.fightSound == null) return;
            if (!RandomChance.Get(characterSounds.chanceToPlayFightSound)) return;

            AudioManager.Instance.PlaySound(transform.position, characterSounds.fightSound, audioSource, false);
        }

        /// <summary>
        /// Adds healthRegenerationSpeed to Health
        /// </summary>
        protected virtual void HealthRegenerationLogic() => Health += healthRegenerationSpeed;

        protected virtual void OnDestroy() => HonorAIManager.Instance.totalCharactersCount--;

        #endregion
    }
}