// Created by Ronis Vision. All rights reserved
// 05.09.2020.

using RVModules.RVSmartAI.Content.AI.DataProviders;

namespace RVHonorAI.Content.Ai.DataProviders
{
    /// <summary>
    /// Provides ITarget
    /// Required context: ITargetProvider
    /// </summary>
    public class TargetProvider : DataProvider<ITarget>
    {
        protected override ITarget ProvideData() => ContextAs<ITargetProvider>().Target;
    }
}