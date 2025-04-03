// Created by Ronis Vision. All rights reserved
// 16.01.2020.

using RVModules.RVSmartAI.GraphElements;

namespace RVHonorAI.Content.Ai.Scorers
{
    /// <summary>
    /// Returns IHealth.Health multiplied by score
    /// Required context: IHealth
    /// </summary>
    public class HealthScorer : AiScorer
    {
        protected override string DefaultDescription => "Returns IHealth.Health multiplied by score.\nRequired context: IHealth";

        #region Public methods

        public override float Score(float _deltaTime) => GetComponentFromContext<IHealth>().Health * score;

        #endregion
    }
    
}