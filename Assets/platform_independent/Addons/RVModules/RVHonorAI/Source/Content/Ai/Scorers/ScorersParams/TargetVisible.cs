// Created by Ronis Vision. All rights reserved
// 03.10.2020.

using RVModules.RVSmartAI.GraphElements;
using UnityEngine;

namespace RVHonorAI.Content.Ai.Scorers
{
    public class TargetVisible : AiScorerParams<TargetInfo>
    {
        #region Fields

        [SerializeField]
        protected float not;

        #endregion

        #region Not public methods

        protected override float Score(TargetInfo _parameter) => _parameter.Visible ? score : not;

        #endregion
    }
}