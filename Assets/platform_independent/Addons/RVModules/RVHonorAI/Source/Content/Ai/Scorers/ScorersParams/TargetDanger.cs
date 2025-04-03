// Created by Ronis Vision. All rights reserved
// 10.02.2021.

using RVModules.RVSmartAI.GraphElements;

namespace RVHonorAI.Content.Ai.Scorers
{
    public class TargetDanger : AiScorerParams<TargetInfo>
    {
        protected override float Score(TargetInfo _parameter) => _parameter.Target.Danger * score;
    }
}