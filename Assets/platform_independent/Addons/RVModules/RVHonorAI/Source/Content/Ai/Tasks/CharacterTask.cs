// Created by Ronis Vision. All rights reserved
// 16.09.2020.

using RVModules.RVSmartAI.Content.AI.Tasks;

namespace RVHonorAI.Content.Ai.Tasks
{
    public abstract class CharacterTask : AiAgentBaseTask
    {
        private ICharacter character;
        
        protected override void OnContextUpdated()
        {
            base.OnContextUpdated();
            character = ContextAs<CharacterAi>().Character;
        }
    }
}