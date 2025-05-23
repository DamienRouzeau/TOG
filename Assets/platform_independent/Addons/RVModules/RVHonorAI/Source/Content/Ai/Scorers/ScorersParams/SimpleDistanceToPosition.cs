// Created by Ronis Vision. All rights reserved
// 16.09.2020.

using RVModules.RVSmartAI.Content.AI.DataProviders;
using RVModules.RVSmartAI.Content.AI.Scorers;
using UnityEngine;

namespace RVHonorAI.Content.Ai.Scorers
{
    public class SimpleDistanceToPosition : SimpleProximityAiScorerParams
    {
        #region Fields

        public Vector3Provider position;

        #endregion

        #region Properties

        protected override Vector3 PositionToMeasure => position;

        #endregion
    }
}