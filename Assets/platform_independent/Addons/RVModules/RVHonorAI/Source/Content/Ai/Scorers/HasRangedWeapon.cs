// Created by Ronis Vision. All rights reserved
// 16.01.2020.

using RVModules.RVSmartAI;
using RVModules.RVSmartAI.GraphElements;

namespace RVHonorAI.Content.Ai.Scorers
{
    public class HasRangedWeapon : AiScorer
    {
        public float scoreNoRangedWeapon;

        #region Public methods

        public override float Score(float _deltaTime) => GetComponentFromContext<IAttacker>().CurrentAttack?.AttackType == AttackType.Shooting ? score : scoreNoRangedWeapon;

        #endregion
    }
}