// Created by Ronis Vision. All rights reserved
// 13.10.2020.

using RVModules.RVUtilities.Extensions;
using UnityEngine;

namespace RVModules.RVSmartAI.Content.AI.Scorers
{
    public abstract class SimpleProximityToMeAiScorer : AiAgentBaseScorer
    {
        #region Properties

        protected abstract Vector3 PositionToMeasure { get; }

        #endregion

        #region Public methods

        public override float Score(float _deltaTime) => PositionToMeasure.ManhattanDistance2d(movement.Position) * score;

        #endregion
    }
}