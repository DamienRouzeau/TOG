// Created by Ronis Vision. All rights reserved
// 07.07.2020.

namespace RVHonorAI
{
    public interface IDamageSystem
    {
        #region Public methods

        float CalculateDamage(Character _damageReceiver, float _attackValue, DamageType _damageType);
        float CalculateAttack(Character _damageDealer);

        #endregion
    }
}