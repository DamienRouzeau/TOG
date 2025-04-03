// Created by Ronis Vision. All rights reserved
// 13.08.2020.

using System;
using RVModules.RVCommonGameLibrary.Audio;
using UnityEngine;
using UnityEngine.Serialization;

namespace RVHonorAI
{
    [Serializable] public class Attack : MonoBehaviour, IAttack
    {
        #region Fields

        [FormerlySerializedAs("attack")]
        [SerializeField]
        private float damage = 40;

        [SerializeField]
        private float range = 2;

        [SerializeField]
        private float attackInterval = 2;

        [SerializeField]
        private float attackAngle = 20;

        public SoundConfig attackSound;
        public SoundConfig hitSound;

//        private AttackType weaponType;

        [SerializeField]
        protected DamageType damageType;

        #endregion

        #region Properties

        public DamageType DamageType => damageType;

        public float Damage => damage;

        public float Range => range;

        public float AttackInterval => attackInterval;

        public float AttackAngle => attackAngle;

        public virtual AttackType AttackType => AttackType.Melee;

        #endregion
    }

    public enum AttackType
    {
        Melee,
        Shooting
    }
}