// Created by Ronis Vision. All rights reserved
// 16.01.2020.

using RVModules.RVSmartAI;
using RVModules.RVSmartAI.Content.AI.Scorers;
using RVModules.RVUtilities.Extensions;
using UnityEngine;

namespace RVHonorAI.Content.Ai.Scorers
{
    /// <summary>
    /// Required context: IMovement, ITargetProvider, IAttackRange
    /// </summary>
    public class IsTargetInAttackRange : AiAgentBaseScorer
    {
        #region Public methods

        public float scoreNotInRange;

        private ITargetProvider targetProvider;
        private IAttackRange attackRange;

        protected override string DefaultDescription => "Required context: IMovement, ITargetProvider, IAttackRange";

        protected override void OnContextUpdated()
        {
            base.OnContextUpdated();
            targetProvider = ContextAs<ITargetProvider>();
            attackRange = GetComponentFromContext<IAttackRange>();
        }

        public override float Score(float _deltaTime)
        {
            //if (characterAi.Target as Object == null) return 0;
            return Vector2.Distance(movement.Position.ToVector2(), targetProvider.Target.Transform.position.ToVector2()) <
                   attackRange.AttackRange
                ? score
                : scoreNotInRange;
        }

        #endregion
    }
}