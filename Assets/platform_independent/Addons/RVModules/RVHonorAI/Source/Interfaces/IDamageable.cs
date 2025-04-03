// Created by Ronis Vision. All rights reserved
// 07.07.2020.

using UnityEngine;

namespace RVHonorAI
{
    public interface IDamageable
    {
        #region Public methods

        /// <summary>
        /// Returned value should be actual dealt damage (in after armor, etc.)
        /// </summary>
        float ReceiveDamage(float _damage, Object _damageSource, DamageType _damageType, Vector3 hitPoint = default, Vector3 _hitForce = default, float forceRadius = default);

        #endregion
    }
}