// Created by Ronis Vision. All rights reserved
// 07.07.2020.

using UnityEngine;

namespace RVHonorAI
{
    public class BasicDamageSystem : MonoBehaviour, IDamageSystem
    {
        #region Fields

        public float damageMultiplier = 1;
        public float armorMultiplier = 1;

        #endregion

        #region Public methods

        public float CalculateDamage(Character _damageReceiver, float _attack, DamageType _damageType)
        {
            var dmg = Random.Range(_attack * .5f, _attack);
            
            // armor protects only against physical dmg
            if (_damageType == DamageType.Physical) dmg -= Random.Range(0, _damageReceiver.Armor * armorMultiplier);
            if (dmg < 0) dmg = 0;

            return dmg;
        }

        public float CalculateAttack(Character _damageDealer) => _damageDealer.Damage * damageMultiplier;

        #endregion
    }
}