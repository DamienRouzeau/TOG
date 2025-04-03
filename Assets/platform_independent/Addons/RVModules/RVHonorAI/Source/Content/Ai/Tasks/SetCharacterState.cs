// Created by Ronis Vision. All rights reserved
// 06.09.2020.

using RVModules.RVSmartAI;
using RVModules.RVSmartAI.GraphElements;
using UnityEngine;

namespace RVHonorAI.Content.Ai.Tasks
{
    public class SetCharacterState : AiTask
    {
        [SerializeField]
        private CharacterState state;

        protected override void Execute(float _deltaTime) => ContextAs<ICharacterStateProvider>().CharacterState = state;
    }
}