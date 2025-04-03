// Created by Ronis Vision. All rights reserved
// 26.01.2021.

using RVModules.RVSmartAI.GraphElements;
using UnityEngine;

namespace RVHonorAI.Content.Ai.Scorers
{
    /// <summary>
    /// Returns score when ITargetListProvider.Targets has any entries.
    /// Required context: ITargetListProvider 
    /// </summary>
    public class SeeAnyTargets : AiScorer
    {
        #region Fields

        public float falseScore;

        //private ITargetListProvider targetListProvider;
        private ITargetInfosProvider targetInfosProvider;

        #endregion

        #region Properties

        protected override string DefaultDescription => "Returns score when ITargetListProvider.Targets has any entries." +
                                                        "\n Required context: ITargetListProvider";

        #endregion

        #region Public methods

        public override float Score(float _deltaTime)
        {
            var targets = targetInfosProvider.TargetInfos;

            foreach (var target in targets)
            {
                if (target.Target as Object == null) continue;
                return score;
            }

            return falseScore;
        }

        #endregion

        #region Not public methods

        protected override void OnContextUpdated()
        {
            base.OnContextUpdated();
            //targetListProvider = Context as ITargetListProvider;
            targetInfosProvider = ContextAs<ITargetInfosProvider>();
        }

        #endregion
    }
}