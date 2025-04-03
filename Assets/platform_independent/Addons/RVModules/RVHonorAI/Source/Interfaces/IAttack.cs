// Created by Ronis Vision. All rights reserved
// 12.09.2020.
namespace RVHonorAI
{
    public interface IAttack
    {
        DamageType DamageType { get; }
        float Damage { get; }
        float Range { get; }
        float AttackInterval { get; }
        float AttackAngle { get; }
        AttackType AttackType { get; }
    }
}