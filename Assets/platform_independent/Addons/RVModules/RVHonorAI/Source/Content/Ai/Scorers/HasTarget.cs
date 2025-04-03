// Created by Ronis Vision. All rights reserved
// 16.01.2020.

using RVModules.RVSmartAI;
using RVModules.RVSmartAI.GraphElements;
using UnityEngine;

namespace RVHonorAI.Content.Ai.Scorers
{
    /// <summary>
    /// 
    /// </summary>
    public class HasTarget : AiScorer
    {
        #region Public methods

        [SerializeField]
        private float not;

        private ITargetProvider targetProvider;

        protected override void OnContextUpdated()
        {
            base.OnContextUpdated();
            targetProvider = ContextAs<ITargetProvider>();
        }

        public override float Score(float _deltaTime) => targetProvider.TargetInfo.Target as Object != null ? score : not;

        #endregion
    }
}