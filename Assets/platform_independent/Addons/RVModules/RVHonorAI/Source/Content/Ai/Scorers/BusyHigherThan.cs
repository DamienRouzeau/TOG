// Created by Ronis Vision. All rights reserved
// 24.06.2020.

using RVModules.RVLoadBalancer.Tasks;
using RVModules.RVSmartAI;
using RVModules.RVSmartAI.GraphElements;

namespace RVHonorAI.Content.Ai.Scorers
{
    public class BusyHigherThan : AiScorer
    {
        public int higherThan;

        public int scoreIfLower;

        private TaskHandler taskHandler;

        protected override void OnContextUpdated()
        {
            base.OnContextUpdated();
            taskHandler = GetComponentFromContext<IJobHandlerProvider>()?.AiJobHandler;
        }

        public override float Score(float _deltaTime) => taskHandler.BusyPriority > higherThan ? score : scoreIfLower;
    }
}