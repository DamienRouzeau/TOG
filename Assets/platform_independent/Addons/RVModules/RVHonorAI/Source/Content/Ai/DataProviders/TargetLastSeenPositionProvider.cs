// Created by Ronis Vision. All rights reserved
// 17.09.2020.

using RVModules.RVSmartAI.Content.AI.DataProviders;
using UnityEngine;

namespace RVHonorAI.Content.Ai.DataProviders
{
    public class TargetLastSeenPositionProvider : Vector3Provider
    {
        protected override Vector3 ProvideData() => ContextAs<ITargetProvider>().TargetInfo.LastSeenPosition;
    }
}