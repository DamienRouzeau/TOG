// Created by Ronis Vision. All rights reserved
// 19.02.2021.

using RVModules.RVSmartAI.Content.AI.DataProviders;
using UnityEngine;

namespace RVHonorAI.Content.Ai.DataProviders
{
    public class DangerPositionProvider : Vector3Provider
    {
        #region Fields

        private IDangerPositionProvider dangerPositionProvider;

        #endregion

        #region Not public methods

        protected override void OnContextUpdated()
        {
            base.OnContextUpdated();
            dangerPositionProvider = ContextAs<IDangerPositionProvider>();
        }

        protected override Vector3 ProvideData() => dangerPositionProvider.DangerPosition;

        #endregion
    }
}