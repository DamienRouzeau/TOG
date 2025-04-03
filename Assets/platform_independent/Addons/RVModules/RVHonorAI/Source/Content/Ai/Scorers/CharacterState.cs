// Created by Ronis Vision. All rights reserved
// 16.01.2020.

using RVModules.RVSmartAI;
using RVModules.RVSmartAI.GraphElements;

namespace RVHonorAI.Content.Ai.Scorers
{
    public class CharacterState : AiScorer
    {
        #region Fields

        public RVHonorAI.CharacterState state;

        public float falseScore;

        #endregion

        #region Public methods

        public override float Score(float _deltaTime) => ContextAs<ICharacterStateProvider>().CharacterState == state ? score : falseScore;

        #endregion
    }
}