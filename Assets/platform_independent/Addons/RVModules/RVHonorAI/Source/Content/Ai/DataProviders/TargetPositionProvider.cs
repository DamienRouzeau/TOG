// Created by Ronis Vision. All rights reserved
// 17.09.2020.

using RVModules.RVSmartAI.Content.AI.DataProviders;
using UnityEngine;

namespace RVHonorAI.Content.Ai.DataProviders
{
    public class TargetPositionProvider : Vector3Provider
    {
        private ITargetProvider targetProvider;
        
        protected override void OnContextUpdated()
        {
            base.OnContextUpdated();
            targetProvider = ContextAs<ITargetProvider>();
        }

        protected override Vector3 ProvideData() => targetProvider.Target.Transform.position;

        public override bool ValidateData() => targetProvider.Target as Object != null;
    }
}