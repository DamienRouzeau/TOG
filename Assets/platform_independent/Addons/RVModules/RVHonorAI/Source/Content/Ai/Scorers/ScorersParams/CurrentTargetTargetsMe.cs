// Created by Ronis Vision. All rights reserved
// 25.02.2021.

using RVModules.RVSmartAI.GraphElements;
using UnityEngine;

namespace RVHonorAI.Content.Ai.Scorers
{
    /// <summary>
    /// Returns score if our current target also targets(attack) us
    /// </summary>
    public class CurrentTargetTargetsMe : AiScorerParams<TargetInfo>
    {
        #region Fields

        private ITargetProvider targetProvider;
        private ITarget ourChar;

        #endregion

        #region Not public methods

        protected override void OnContextUpdated()
        {
            targetProvider = ContextAs<ITargetProvider>();
            ourChar = GetComponentFromContext<ITarget>();
        }

        protected override float Score(TargetInfo _parameter)
        {
            if (targetProvider.Target as Object == null) return 0;
            var targetsTargetProvider = _parameter.Target as ITargetProvider;
            if (targetsTargetProvider as Object == null) return 0;
            if (targetsTargetProvider.Target == ourChar) return score;
            return 0;
        }

        #endregion
    }
}