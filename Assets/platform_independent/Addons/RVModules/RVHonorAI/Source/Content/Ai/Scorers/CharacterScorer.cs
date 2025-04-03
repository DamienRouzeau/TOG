// Created by Ronis Vision. All rights reserved
// 16.01.2020.

using RVModules.RVSmartAI.Content.AI.Scorers;

namespace RVHonorAI.Content.Ai.Scorers
{
    /// <summary>
    /// Base scorer for using with Character class, provides CharacterAi reference from context using ICharacterProvider 
    /// </summary>
    public abstract class CharacterScorer : AiAgentBaseScorer
    {
        #region Fields

        protected ICharacter character;

        #endregion

        #region Not public methods

        protected override void OnContextUpdated()
        {
            base.OnContextUpdated();
            character = (Context as ICharacterProvider)?.Character;
        }

        #endregion
    }
}