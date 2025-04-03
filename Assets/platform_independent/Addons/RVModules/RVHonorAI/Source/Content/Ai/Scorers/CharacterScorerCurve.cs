// Created by Ronis Vision. All rights reserved
// 08.07.2020.

using RVModules.RVLoadBalancer;
using RVModules.RVSmartAI.Content.AI.Scorers;

namespace RVHonorAI.Content.Ai.Scorers
{
    public abstract class CharacterScorerCurve : CharacterScorerCurveBase, I<AiAgentBaseScorerCurve>, I<CharacterScorer>
    {    
    }
}