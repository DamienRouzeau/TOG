// Created by Ronis Vision. All rights reserved
// 16.03.2021.

using RVModules.RVSmartAI.Content.AI.DataProviders;
using RVModules.RVUtilities.Extensions;
using UnityEngine;

namespace RVModules.RVSmartAI.Content.AI.Scorers
{
    public class ProximityToPosition : AiAgentBaseScorerCurveParams<Vector3>
    {
        #region Fields

        [Header("Distance at time of 1 on curve")]
        public FloatProvider distance;

        [SerializeField]
        private Vector3Provider positionToMeasure;

        #endregion

        #region Not public methods

        protected override float Score(Vector3 _parameter)
        {
            if (!positionToMeasure.ValidateData()) return 0;
            return GetScoreFromCurve(_parameter.ManhattanDistance2d(positionToMeasure) / distance);
        }

        #endregion
    }
}