// Created by Ronis Vision. All rights reserved
// 22.09.2020.

using System;
using RVModules.RVSmartAI.Content;

namespace RVModules.RVSmartAI.GraphElements
{
    [Serializable] public abstract class AiScorerParams<T> : AiGraphElement, IAiScorer
    {
        #region Fields

        public ScorerType scorerType;

        public float score = 1;

        #endregion

        #region Properties

        public ScorerType ScorerType => scorerType;

        #endregion

        #region Public methods

        public float Score_(object _parameter) => Score((T) _parameter);

        #endregion

        #region Not public methods

        protected abstract float Score(T _parameter);

        #endregion
    }
}