// Created by Ronis Vision. All rights reserved
// 13.10.2020.

using UnityEngine;

namespace RVModules.RVSmartAI.Content.AI.Scorers
{
    public class MoveTargetIsInRangeScorer : AiAgentBaseScorer
    {
        #region Public methods

        public override float Score(float _deltaTime)
        {
            foreach (var cNearbyObject in NearbyObjects)
            {
                var comp = cNearbyObject as Component;
                if (comp == null) continue;

                if (comp.transform == MoveTarget) return score;
            }

            return 0;
        }

        #endregion
    }
}