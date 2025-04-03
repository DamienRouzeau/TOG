// Created by Ronis Vision. All rights reserved
// 16.09.2020.

namespace RVHonorAI
{
    public interface IAttacker
    {
        /// <summary>
        /// Attack current target
        /// This is immediate attack, damage should be dealt in this method, or weapon should be shot if using ranged/shooting weapon.
        /// Animation-independent.
        /// </summary>
        void Attack();
        
        Attack CurrentAttack { get; }
    }
}