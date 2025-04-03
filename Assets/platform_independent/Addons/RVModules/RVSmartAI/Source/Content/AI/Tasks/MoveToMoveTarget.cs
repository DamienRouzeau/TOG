// Created by Ronis Vision. All rights reserved
// 21.03.2021.

namespace RVModules.RVSmartAI.Content.AI.Tasks
{
    public class MoveToMoveTarget : AiAgentBaseTask
    {
        #region Not public methods

        protected override void Execute(float _deltaTime) => movement.Destination = MoveTarget.position;

        #endregion
    }
}