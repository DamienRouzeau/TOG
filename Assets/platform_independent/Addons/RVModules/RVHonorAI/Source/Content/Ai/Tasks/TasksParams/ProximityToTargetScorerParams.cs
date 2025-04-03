// Created by Ronis Vision. All rights reserved
// 16.01.2020.

using RVModules.RVSmartAI.Content.AI.Scorers;
using UnityEngine;

namespace RVHonorAI.Content.Ai.Tasks
{
    /// <summary>
    /// 
    /// </summary>
    public class ProximityToTargetScorerParams : ProximityAiScorerParams
    {
        #region Fields

        private ITargetProvider targetProvider;

        #endregion

        #region Properties

        public override Vector3 PositionToMeasure => targetProvider.Target.Transform.position;

        #endregion

        #region Not public methods

        protected override void OnContextUpdated()
        {
            base.OnContextUpdated();
            targetProvider = Context as ITargetProvider;
        }

        #endregion
    }
}