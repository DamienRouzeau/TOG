// Created by Ronis Vision. All rights reserved
// 07.07.2020.

using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace RVHonorAI
{
    public class Player : MonoBehaviour, ICharacter
    {
        #region Fields

        [SerializeField]
        private AiGroup group;

        [SerializeField]
        private Transform headTransform;

        #endregion

        #region Properties

        public Object GetObject => this;

        public float Danger => Health + DamagePerSecond;
        public float MaxHealth { get; }
        public UnityEvent OnKilled { get; set; }
        public float RunningSpeed { get; set; }
        public float WalkingSpeed { get; set; }
        public Transform Transform => transform;
        public Transform AimTransform => headTransform;
        public Transform HeadTransform => headTransform;

        public float Health { get; } = 100;

        public AiGroup AiGroup => group;

        public float DamagePerSecond => 40;

        public float Armor => 20;

        public float Courage => 50;

        public float AttackRange => 2;
        //public Vector3 AimOffset { get; } = Vector3.up * 1.5f;
        // public Vector3 AimPosition => transform.position + AimOffset;

        #endregion

        #region Public methods

        public void Heal(float _amount)
        {
        }

        public virtual bool IsEnemy(IRelationshipProvider _other, bool _contraCheck = false)
        {
            // check for both sides-relationship
            if (!_contraCheck && _other.IsEnemy(this, true)) return true;

//            if (treatNeutralCharactersAsEnemies)
//            {
//                return !IsAlly(_other);
//            }

            if (AiGroup == null || _other.AiGroup == null) return false;

            return ((IList) AiGroup.enemies).Contains(_other.AiGroup) || ((IList) _other.AiGroup.enemies).Contains(AiGroup);
        }

        public virtual bool IsAlly(IRelationshipProvider _other)
        {
            if (AiGroup == null || _other.AiGroup == null) return false;
            return AiGroup == _other.AiGroup;
        }

        AiGroup IRelationshipProvider.AiGroup { get; set; }


        #endregion

        public Transform VisibilityCheckTransform => HeadTransform;
        public Vector3 FovPosition => HeadTransform.position;
        public ITarget Target { get; }
        public TargetInfo TargetInfo { get; set; }
        public float ReceiveDamage(float _damage, Object _damageSource, DamageType _damageType, Vector3 hitPoint, Vector3 _hitForce = default,
            float forceRadius = default) => _damage;
    }
}