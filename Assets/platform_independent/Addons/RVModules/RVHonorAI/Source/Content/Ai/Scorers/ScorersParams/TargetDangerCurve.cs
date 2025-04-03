// Created by Ronis Vision. All rights reserved
// 10.02.2021.

using RVModules.RVSmartAI.Content.AI.Scorers;
using RVModules.RVSmartAI.GraphElements;
using UnityEngine;

namespace RVHonorAI.Content.Ai.Scorers
{
    public class TargetDangerCurve : AiScorerCurveParams<TargetInfo>
    {
        [SerializeField]
        [Header("Danger value at time of 1 on curve")]
        private float danger;

        protected override float Score(TargetInfo _parameter) => GetScoreFromCurve(_parameter.Target.Danger / danger);
    }
}