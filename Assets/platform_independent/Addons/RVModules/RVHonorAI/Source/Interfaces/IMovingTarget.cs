// Created by Ronis Vision. All rights reserved
// 05.09.2020.

using UnityEngine;

namespace RVHonorAI
{
    /// <summary>
    /// ITarget with Velocity
    /// </summary>
    public interface IMovingTarget : ITarget
    {
        /// <summary>
        /// Current target velocity in m/s
        /// </summary>
        Vector3 Velocity { get; }
    }
}