// Created by Ronis Vision. All rights reserved
// 01.02.2021.

using System;
using RVModules.RVUtilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RVHonorAI
{
    public class HonorAIManager : MonoSingleton<HonorAIManager>
    {
        #region Fields

        public Action<IDamageSystem> onDamageSystemChange;

        public Object damageSystemObject;

        private IDamageSystem damageSystem;

        [SerializeField]
        internal int totalCharactersCount;
        
        [SerializeField]
        internal int activeCharactersCount; 

        #endregion

        #region Properties

        public virtual IDamageSystem DamageSystem
        {
            get => damageSystem;
            set
            {
                damageSystem = value;
                onDamageSystemChange?.Invoke(damageSystem);
            }
        }

        #endregion

        #region Public methods

        public static float CalculateDamage(Character _character, float _attack, DamageType _damageType) =>
            instance.DamageSystem.CalculateDamage(_character, _attack, _damageType);

        public static float CalculateAttack(Character _character) => instance.DamageSystem.CalculateAttack(_character);

        #endregion

        #region Not public methods

        private void Awake()
        {
            DamageSystem = damageSystemObject as IDamageSystem;
            if (DamageSystem == null) Debug.LogError("There's no DamageSystem! Provide DamageSystem in HonorAIManager");
        }

        #endregion
    }
}