// Created by Ronis Vision. All rights reserved
// 08.07.2020.

using RVModules.RVSmartAI.Content.AI.Scorers;

namespace RVHonorAI.Content.Ai.Scorers
{
    /// <summary>
    /// Returns health percentage multiplied by score 
    /// </summary>
    public class HealthPercentScorer : AiScorerCurve
    {
        private IHealth health;

        protected override void OnContextUpdated()
        {
            base.OnContextUpdated();
            health = GetComponentFromContext<IHealth>();
        }

        public override float Score(float _deltaTime) => GetScoreFromCurve(health.Health / health.MaxHealth);
    }
}