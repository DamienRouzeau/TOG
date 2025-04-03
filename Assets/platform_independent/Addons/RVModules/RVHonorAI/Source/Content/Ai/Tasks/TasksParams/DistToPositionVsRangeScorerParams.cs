// Created by Ronis Vision. All rights reserved
// 02.06.2020.

using RVModules.RVSmartAI.Content.AI.DataProviders;
using RVModules.RVSmartAI.Content.AI.Scorers;
using UnityEngine;

namespace RVHonorAI.Content.Ai.Tasks
{
    public class DistToPositionVsRangeScorerParams : AiAgentBaseScorerCurveParams<Vector3>
    {
        [SerializeField]
        private Vector3Provider positionProvider;
        private IAttackRange attackRange;

        protected override string DefaultDescription => "Distance to position divided by attack range. Reuqired context: ITargetProvider, IAttackRange";

        protected override void OnContextUpdated()
        {
            base.OnContextUpdated();
            attackRange = GetComponentFromContext<IAttackRange>();
        }

        protected override float Score(Vector3 _parameter)
        {
            if (!positionProvider.ValidateData()) return 0;
            return GetScoreFromCurve(Vector3.Distance(positionProvider, _parameter) / attackRange.AttackRange);
        }
    }
}