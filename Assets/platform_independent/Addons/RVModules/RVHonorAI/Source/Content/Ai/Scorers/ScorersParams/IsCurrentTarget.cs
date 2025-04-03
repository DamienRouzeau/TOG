// Created by Ronis Vision. All rights reserved
// 10.02.2021.

using RVModules.RVSmartAI.GraphElements;

namespace RVHonorAI.Content.Ai.Scorers
{
    public class IsCurrentTarget : AiScorerParams<TargetInfo>
    {
        ITargetProvider targetProvider;

        protected override void OnContextUpdated() => targetProvider = ContextAs<ITargetProvider>();

        protected override float Score(TargetInfo _parameter)
        {
            if (targetProvider.TargetInfo == _parameter) return score;
            return 0;
        }
    }
}