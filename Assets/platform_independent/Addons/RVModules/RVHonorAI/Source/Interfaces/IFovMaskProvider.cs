// Created by Ronis Vision. All rights reserved
// 07.07.2020.

using UnityEngine;

namespace RVHonorAI
{
    public interface IFovMaskProvider
    {
        #region Properties

        LayerMask FovMask { get; }

        #endregion
    }
}