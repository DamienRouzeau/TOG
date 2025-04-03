// Created by Ronis Vision. All rights reserved
// 14.03.2021.

using RVModules.RVSmartAI.GraphElements;
using UnityEngine;

namespace RVModules.RVSmartAI.Content.AI.Scorers
{
    public class VariableBool : AiScorer
    {
        [SerializeField]
        private string variableName;

        [SerializeField]
        private bool expectedBoolValue;

        public override float Score(float _deltaTime)
        {
            if (AiGraph.GraphAiVariables.GetBool(variableName) == expectedBoolValue) return score;
            return 0;
        }
    }
}