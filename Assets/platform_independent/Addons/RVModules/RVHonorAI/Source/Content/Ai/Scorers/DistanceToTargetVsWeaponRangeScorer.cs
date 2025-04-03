// Created by Ronis Vision. All rights reserved
// 12.09.2020.

using RVModules.RVSmartAI.Content.AI.Scorers;
using UnityEngine;

namespace RVHonorAI.Content.Ai.Scorers
{
    /// <summary>
    /// Returns percentage of used range to current target
    /// Required context: IAttackRange, IMovement, ITargetProvider
    /// </summary>
    public class DistanceToTargetVsWeaponRangeScorer : AiAgentBaseScorerCurve
    {
        #region Public methods

        //[Header("Distance at time of 1 on curve")]
        //public float distance = 10;

        private IAttackRange attackRange;
        private ITargetProvider targetProvider;

        protected override string DefaultDescription => "Returns percentage of used range to current target\n" +
                                                        " Required context: IAttackRange, IMovement, ITargetProvider";

        protected override void OnContextUpdated()
        {
            base.OnContextUpdated();
            targetProvider = ContextAs<ITargetProvider>();
            attackRange = GetComponentFromContext<IAttackRange>();
        }


        public override float Score(float _deltaTime)
        {
//        var weaponConfigRange = 1.5f;
//
//        var character = characterAi.Character;
//        if (character.Weapon != null) weaponConfigRange = character.Weapon.Range;

            return GetScoreFromCurve(Vector3.Distance(movement.Position, targetProvider.TargetInfo.Target.Transform.position) / attackRange.AttackRange);
        }

        #endregion
    }
}