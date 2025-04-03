// Created by Ronis Vision. All rights reserved
// 16.01.2020.

using RVModules.RVSmartAI.Content.AI.Tasks;

namespace RVHonorAI.Content.Ai.Tasks
{
    public class StopMoving : AiAgentBaseTask
    {
        #region Not public methods

        protected override void Execute(float _deltaTime) => movement.Destination = movement.Position;

        #endregion
    }
}