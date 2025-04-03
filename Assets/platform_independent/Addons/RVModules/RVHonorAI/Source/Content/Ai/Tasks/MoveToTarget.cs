// Created by Ronis Vision. All rights reserved
// 16.01.2020.

using RVModules.RVSmartAI.Content.AI.Tasks;

namespace RVHonorAI.Content.Ai.Tasks
{
    /// <summary>
    /// Moves  to target position or it's last seen position if it is not visible
    /// </summary>
    public class MoveToTarget : AiAgentBaseTask
    {
        private ITargetProvider targetProvider;

        #region Not public methods

        protected override string DefaultDescription => "Moves  to target position or it's last seen position if it is not visible";

        protected override void OnContextUpdated()
        {
            base.OnContextUpdated();
            targetProvider = ContextAs<ITargetProvider>();
        }

        protected override void Execute(float _deltaTime)
        {
            movement.Destination = targetProvider.TargetInfo.Visible ? targetProvider.Target.Transform.position : targetProvider.TargetInfo.LastSeenPosition;
        }

        #endregion
    }
}