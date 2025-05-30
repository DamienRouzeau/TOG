﻿// Created by Ronis Vision. All rights reserved
// 13.10.2020.

using UnityEngine;

namespace RVModules.RVSmartAI.Content.AI.Scorers
{
    public class SimpleProximityToMeAiScorerParams : SimpleProximityAiScorerParams
    {
        #region Properties

        protected override Vector3 PositionToMeasure => movement.Position;

        #endregion
    }
}