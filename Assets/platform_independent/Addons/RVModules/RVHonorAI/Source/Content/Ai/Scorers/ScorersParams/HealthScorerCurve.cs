// Created by Ronis Vision. All rights reserved
// 16.01.2020.

using RVModules.RVSmartAI;
using RVModules.RVSmartAI.Content.AI.Scorers;
using UnityEngine;

namespace RVHonorAI.Content.Ai.Scorers
{
    public class HealthScorerCurve : AiAgentBaseScorerCurveParams<TargetInfo>
    {
        #region Fields

        [Header("Health value at time of 1 on curve")]
        public float health = 100;

        protected override string DefaultDescription => "Target must have IHealth implemented";

        #endregion

        #region Not public methods

        protected override float Score(TargetInfo _parameter)
        {
            IHealth healthProvider = _parameter.Target as IHealth;
            if (healthProvider == null) return 0;
            return GetScoreFromCurve(healthProvider.Health / health);
        }

        #endregion
    }
}