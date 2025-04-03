// Created by Ronis Vision. All rights reserved
// 17.09.2020.

using RVModules.RVSmartAI.Content.AI.DataProviders;
using UnityEngine;

namespace RVHonorAI.Content.Ai.DataProviders
{
    public class DetectionRangeMulProvider : FloatProvider
    {
        #region Fields

        [SerializeField]
        private FloatProvider rangeMultiplier;

        #endregion

        #region Not public methods

        protected override float ProvideData() => ContextAs<IDetectionRangeProvider>().DetectionRange * rangeMultiplier;

        #endregion
    }
}