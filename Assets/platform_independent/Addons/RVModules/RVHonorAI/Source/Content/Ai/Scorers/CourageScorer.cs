// Created by Ronis Vision. All rights reserved
// 16.01.2020.

using RVModules.RVSmartAI.GraphElements;

namespace RVHonorAI.Content.Ai.Scorers
{
    public class CourageScorer : AiScorer
    {
        #region Public methods

        public override float Score(float _deltaTime) => ContextAs<ICourageProvider>().Courage * score;

        #endregion
    }
}