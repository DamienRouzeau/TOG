// Created by Ronis Vision. All rights reserved
// 24.06.2020.

using RVModules.RVLoadBalancer.Tasks;
using RVModules.RVSmartAI;
using RVModules.RVSmartAI.GraphElements;

namespace RVHonorAI.Content.Ai.Scorers
{
    public class BusyScorer : AiScorer
    {
        private TaskHandler taskHandler;

        protected override void OnContextUpdated()
        {
            base.OnContextUpdated();
            taskHandler = GetComponentFromContext<IJobHandlerProvider>()?.AiJobHandler;
        }

        protected override string DefaultDescription => "Returns IJobHandlerProvider.AiJobHandler.BusyPriority multiplied by score";

        public override float Score(float _deltaTime) => taskHandler.BusyPriority * score;
    }
}