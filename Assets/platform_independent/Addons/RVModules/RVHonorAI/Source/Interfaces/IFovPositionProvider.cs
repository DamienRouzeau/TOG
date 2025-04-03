// Created by Ronis Vision. All rights reserved
// 05.09.2020.

using UnityEngine;

namespace RVHonorAI
{
    /// <summary>
    /// Provides position from which to check field of view (head, eyes etc.)
    /// todo maybe this should provide transform, to be consistent with other position related providers
    /// </summary>
    public interface IFovPositionProvider
    {
        /// <summary>
        /// Fov 'root' position - usually chacter's eyes/head
        /// </summary>
        Vector3 FovPosition { get; }
    }
}