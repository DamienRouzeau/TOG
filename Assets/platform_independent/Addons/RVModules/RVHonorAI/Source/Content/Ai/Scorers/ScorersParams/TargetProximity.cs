// Created by Ronis Vision. All rights reserved
// 16.01.2020.

using RVModules.RVSmartAI;
using RVModules.RVSmartAI.Content.AI.DataProviders;
using RVModules.RVSmartAI.Content.AI.Scorers;
using RVModules.RVUtilities.Extensions;
using UnityEngine;

namespace RVHonorAI.Content.Ai.Scorers
{
    /// <summary>
    /// Check distance in 2d using X and Z axis of vectors, and use this distance to return score based on configuration
    /// </summary>
    public class TargetProximity : AiAgentBaseScorerCurveParams<TargetInfo>
    {
        #region Fields

        [Header("Distance at time of 1 on curve"), SerializeField]
        private FloatProvider distance;

        #endregion

        #region Properties

        [SerializeField]
        private Vector3Provider positionToMeasure;

        #endregion

        #region Not public methods

        protected override float Score(TargetInfo _parameter) =>
            GetScoreFromCurve(_parameter.Target.Transform.position.ManhattanDistance2d(positionToMeasure) / distance);

        #endregion
    }
}