// Created by Ronis Vision. All rights reserved
// 02.06.2020.

using RVModules.RVSmartAI;
using RVModules.RVSmartAI.Content.AI.Scorers;
using UnityEngine;

namespace RVHonorAI.Content.Ai.Tasks
{
    /// <summary>
    /// 
    /// </summary>
    public class ProximityToRangeScorerParams : AiAgentBaseScorerCurveParams<Vector3>
    {
        private ITargetProvider targetProvider;
        private IAttackRange attackRange;

        protected override string DefaultDescription => "Distance to target minus range divided distance. Reuqired context: ITargetProvider, IAttackRange";

        [SerializeField]
        [Header("Distance at time of 1 on curve")]
        private float distance = 10;

        protected override void OnContextUpdated()
        {
            base.OnContextUpdated();
            targetProvider = ContextAs<ITargetProvider>();
            attackRange = GetComponentFromContext<IAttackRange>();
        }

        protected override float Score(Vector3 _parameter) =>
            GetScoreFromCurve(Mathf.Abs(Vector3.Distance(targetProvider.Target.Transform.position, _parameter) - attackRange.AttackRange) / distance);
    }
}