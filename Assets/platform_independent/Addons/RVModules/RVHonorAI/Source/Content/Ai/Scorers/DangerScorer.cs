// Created by Ronis Vision. All rights reserved
// 16.01.2020.

using RVModules.RVSmartAI.Content.AI.Contexts;
using RVModules.RVSmartAI.GraphElements;

namespace RVHonorAI.Content.Ai.Scorers
{
    /// <summary>
    /// todo store allies list to remove all those wasteful isAlly checks 
    /// </summary>
    public class DangerScorer : AiScorer
    {
        private INearbyObjectsProvider nearbyObjectsProvider;
        private IRelationshipProvider relationshipProvider;
        private ITargetInfosProvider targetInfosProvider;

        protected override string DefaultDescription => "Calculated danger value using IDangerProvider from nearby objects \n" +
                                                        "Required context: INearbyObjectsProvider, IRelationshipProvider";

        protected override void OnContextUpdated()
        {
            base.OnContextUpdated();
            nearbyObjectsProvider = ContextAs<INearbyObjectsProvider>();
            relationshipProvider = ContextAs<IRelationshipProvider>();
            targetInfosProvider = ContextAs<ITargetInfosProvider>();
        }

        #region Public methods

        public override float Score(float _deltaTime)
        {
            float allyStrength = 0;
            float enemyStrength = 0;

            foreach (var targetInfo in targetInfosProvider.TargetInfos) enemyStrength += targetInfo.Target.Danger;

            foreach (var nearbyObject in nearbyObjectsProvider.NearbyObjects)
            {
                var otherNpc = nearbyObject as IRelationshipProvider;
                var otherDanger = nearbyObject as IDangerProvider;
                if (otherNpc == null || otherDanger == null) continue;

                if (otherNpc.IsAlly(relationshipProvider)) allyStrength += otherDanger.Danger;
            }

            if (enemyStrength < 1) enemyStrength = 1;
            if (allyStrength < 1) allyStrength = 1;
            return enemyStrength / allyStrength * score;
        }

        #endregion
    }
}