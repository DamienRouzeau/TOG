// Created by Ronis Vision. All rights reserved
// 16.01.2020.

using System.Collections.Generic;
using RVModules.RVSmartAI.GraphElements;
using UnityEngine;

namespace RVHonorAI.Content.Ai.Tasks
{
    /// <summary>
    /// Sets ITargetProvider.TargetInfo to the highest scored one from ITargetInfosProvider.TargetInfos
    /// </summary>
    public class SelectBestTarget : AiTaskParams<TargetInfo>
    {
        #region Fields

        private ITargetInfosProvider targetInfosProvider;
        private ITargetProvider targetProvider;
        private List<TargetInfo> nonNullTargets = new List<TargetInfo>();

        protected override string DefaultDescription => "Sets ITargetProvider.TargetInfo to the highest scored one from ITargetInfosProvider.TargetInfos" +
                                                        "\n Required context: ITargetInfosProvider, ITargetProvider";

        protected override void OnContextUpdated()
        {
            targetInfosProvider = ContextAs<ITargetInfosProvider>();
            targetProvider = ContextAs<ITargetProvider>();
        }

        #endregion

        #region Not public methods

        protected override void Execute(float _deltaTime)
        {
            nonNullTargets.Clear();
            // remove null targets as we cant rely on scanning
            for (var i = 0; i < targetInfosProvider.TargetInfos.Count; i++)
            {
                var targetInfo = targetInfosProvider.TargetInfos[i];
                if (targetInfo.Target as Object == null) continue;
                nonNullTargets.Add(targetInfo);
            }

            targetProvider.TargetInfo = GetBest(nonNullTargets);
        }

        #endregion
    }
}