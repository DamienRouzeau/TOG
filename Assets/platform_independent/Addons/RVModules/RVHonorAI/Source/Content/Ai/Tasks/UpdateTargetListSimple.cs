// Created by Ronis Vision. All rights reserved
// 23.01.2020.

using RVModules.RVSmartAI.Content.AI.Contexts;
using RVModules.RVSmartAI.GraphElements;

namespace RVHonorAI.Content.Ai.Tasks
{
    /// <summary>
    /// Compared to UpdateTargetList this one dont update memory stuff (TargetInfo)
    /// and works on ITargetListProvider insted of ITargetInfosProvider
    /// </summary>
    public class UpdateTargetListSimple : AiTask
    {
        private ITargetListProvider targetListProvider;
        private INearbyObjectsProvider nearbyObjectsProvider;
        private IRelationshipProvider ourCharacter;

        protected override void OnContextUpdated()
        {
            targetListProvider = Context as ITargetListProvider;
            nearbyObjectsProvider = Context as INearbyObjectsProvider;
            ourCharacter = ContextAs<IRelationshipProvider>();
        }

        protected override void Execute(float _deltaTime)
        {
            var targets = targetListProvider.Targets;
            targets.Clear();

            foreach (var o in nearbyObjectsProvider.NearbyObjects)
            {
                ITarget target = o as ITarget;
                IRelationshipProvider relationshipProvider = target as IRelationshipProvider;
                if (relationshipProvider == null) continue;
                if (relationshipProvider.IsEnemy(ourCharacter)) targets.Add(target);
            }
        }
    }
}