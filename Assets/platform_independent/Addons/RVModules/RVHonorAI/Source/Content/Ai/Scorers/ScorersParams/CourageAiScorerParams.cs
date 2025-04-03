// Created by Ronis Vision. All rights reserved
// 16.01.2020.

using RVModules.RVSmartAI;
using RVModules.RVSmartAI.Content.AI.DataProviders;
using RVModules.RVSmartAI.Content.AI.Scorers;
using UnityEngine;

namespace RVHonorAI.Content.Ai.Scorers
{
    public class CourageAiScorerParams : AiAgentBaseScorerCurveParams<TargetInfo>
    {
        #region Fields

        [Header("Courage value at time of 1 on curve")]
        public FloatProvider courage;

        #endregion

        #region Not public methods

        protected override float Score(TargetInfo _parameter)
        {
            ICourageProvider courageProvider = _parameter.Target as ICourageProvider;
            if (courageProvider == null) return 0;

            return GetScoreFromCurve(courageProvider.Courage / courage);
        }

        #endregion
    }
}