// Created by Ronis Vision. All rights reserved
// 23.02.2021.

using RVHonorAI.Animation;

namespace RVHonorAI
{
    public interface ICharacterAnimationContainer
    {
        MovementAnimations MovementAnimations { get; set; }
        MovementAnimations CombatMovementAnimations { get; set; }
        SingleAnimations SingleAnimations { get; set; }
    }
}