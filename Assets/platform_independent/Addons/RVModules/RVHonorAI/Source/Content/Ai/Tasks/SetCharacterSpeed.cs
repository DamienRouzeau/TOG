// Created by Ronis Vision. All rights reserved
// 07.07.2020.

using RVModules.RVSmartAI;
using RVModules.RVSmartAI.Content.AI.Tasks;
using RVModules.RVSmartAI.GraphElements;
using UnityEngine;

namespace RVHonorAI.Content.Ai.Tasks
{
    /// <summary>
    /// 
    /// </summary>
    public class SetCharacterSpeed : AiAgentBaseTask, ITaskWithScorers
    {
        [SerializeField]
        protected float speed;

        protected override void Execute(float _deltaTime)
        {
            var speedLocal = speed;
            speedLocal += Score(_deltaTime);
            movement.MovementSpeed = speedLocal;
            //characterAi.SetSpeed(speedLocal);
        }
    }
}