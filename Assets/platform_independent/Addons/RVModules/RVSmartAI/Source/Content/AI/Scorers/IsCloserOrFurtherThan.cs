// Created by Ronis Vision. All rights reserved
// 13.10.2020.

using RVModules.RVSmartAI.Content.AI.DataProviders;
using UnityEngine;

namespace RVModules.RVSmartAI.Content.AI.Scorers
{
    public class IsCloserOrFurtherThan : IsCloserOrFurtherThanAiScorer
    {
        #region Fields

        public Vector3Provider firstPosition;

        public Vector3Provider secondPosition;

        #endregion

        #region Properties

        protected override Vector3 SecondPositionToMeasure => secondPosition;
        protected override Vector3 PositionToMeasure => firstPosition;

        #endregion
    }
}