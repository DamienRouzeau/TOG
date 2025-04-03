// Created by Ronis Vision. All rights reserved
// 13.10.2020.

namespace RVModules.RVSmartAI.Content.AI.Scorers
{
    /// <summary>
    /// 
    /// </summary>
    public class IsMovingAiScorer : AiAgentBaseScorer
    {
        #region Fields

        public float scoreNotMoving;

        #endregion

        #region Public methods

        public override float Score(float _deltaTime) => movement.Velocity.magnitude > .15f ? score : scoreNotMoving;

        #endregion
    }
}