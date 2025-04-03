// Created by Ronis Vision. All rights reserved
// 13.10.2020.

using UnityEngine;

namespace RVModules.RVSmartAI.Content.AI.Scorers
{
    public class IsCloserOrFurtherToMoveTargetAiScorerParams : IsCloserOrFurtherThanAiScorerParams
    {
        #region Properties

        protected override Vector3 PositionToMeasure => MoveTarget.position;

        #endregion
    }
}