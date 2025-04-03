// Created by Ronis Vision. All rights reserved
// 20.07.2020.

using RVModules.RVSmartAI.GraphElements;
using UnityEngine;

namespace RVHonorAI.Content.Ai.Scorers
{
    public class CanSeeTarget : AiScorer
    {
        private ITargetProvider targetProvider;

        [SerializeField]
        protected float not;

        protected override string DefaultDescription => "Returns set score if we can see current target";

        protected override void OnContextUpdated() => targetProvider = ContextAs<ITargetProvider>();

        public override float Score(float _deltaTime) => targetProvider.TargetInfo.Visible ? score : not;
    }
}