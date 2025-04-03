// Created by Ronis Vision. All rights reserved
// 16.01.2020.

using RVModules.RVSmartAI.Content.AI.Tasks;

namespace RVHonorAI.Content.Ai.Tasks
{
    public abstract class CharacterTaskParams<T> : AiAgentBaseTaskParams<T>
    {
        #region Fields

        protected ICharacter character;

        #endregion

        #region Not public methods

        protected override void OnContextUpdated()
        {
            base.OnContextUpdated();
            character = (Context as ICharacterProvider).Character;
        }

        #endregion
    }
}