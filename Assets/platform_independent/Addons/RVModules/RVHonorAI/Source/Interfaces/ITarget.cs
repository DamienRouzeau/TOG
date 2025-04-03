// Created by Ronis Vision. All rights reserved
// 05.09.2020.

using UnityEngine;

namespace RVHonorAI
{
    /// <summary>
    /// General Target contract. ITarget doesn't define type of target, it can be anything
    /// </summary>
    public interface ITarget : IVisibilityCheckTransformProvider, IDangerProvider
    {
//        /// <summary>
//        /// Target's position
//        /// </summary>
//        Vector3 Position { get; }
//
//        /// <summary>
//        /// Target's rotation
//        /// </summary>
//        Quaternion Rotation { get; }

        /// <summary>
        /// Target's main transform, for checking position, rotation etc.. usually root transform of entity
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// Transform for aiming at target, also used for checking fov with raycasts etc...
        /// </summary>
        Transform AimTransform { get; }

        /// <summary>
        /// Target's danger
        /// It may depend on many factors, like distance, firepower, strength, health etc...
        /// </summary>
        new float Danger { get; }
    }
}