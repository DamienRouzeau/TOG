// Created by Ronis Vision. All rights reserved
// 09.07.2020.

using RVModules.RVSmartAI;
using RVModules.RVSmartAI.Content.AI.Tasks;
using UnityEngine;

namespace RVHonorAI.Content.Ai.Tasks
{
    public class SetMovementSpeed : AiAgentBaseTask
    {
        #region Fields

        [SerializeField]
        private MovementSpeed movementSpeed;

        #endregion

        #region Not public methods

        protected override void Execute(float _deltaTime) => ContextAs<IMovementSpeedProvider>().MovementSpeed = movementSpeed;

        #endregion
    }
}