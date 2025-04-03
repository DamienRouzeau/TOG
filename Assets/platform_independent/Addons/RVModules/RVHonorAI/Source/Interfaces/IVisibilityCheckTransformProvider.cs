// Created by Ronis Vision. All rights reserved
// 07.07.2020.

using UnityEngine;

namespace RVHonorAI
{
    /// <summary>
    /// Provides transform used for checking field of view (for other entities see if they can see this entity)
    /// </summary>
    public interface IVisibilityCheckTransformProvider
    {
        #region Properties

        /// <summary>
        /// Transform used for checking field of view (for other entities see if they can see this entity)
        /// </summary>
        Transform VisibilityCheckTransform { get; }

        #endregion
    }
}