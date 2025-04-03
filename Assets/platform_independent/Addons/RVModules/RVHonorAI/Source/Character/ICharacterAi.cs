// Created by Ronis Vision. All rights reserved
// 20.03.2021.

using System.Collections.Generic;
using RVModules.RVSmartAI;
using RVModules.RVSmartAI.Content.AI.Contexts;
using UnityEngine;

namespace RVHonorAI
{
    /// <summary>
    /// todo description
    /// </summary>
    public interface ICharacterAi : IMovementProvider, IMovementScannerProvider, IEnvironmentScannerProvider, IMoveTargetProvider, IWaypointsProvider,
        INearbyObjectsProvider, ICharacterProvider, ITargetProvider, IFovMaskProvider, IDetectionRangeProvider, IFovPositionProvider, ICharacterStateProvider,
        IMovementSpeedProvider, ICharacterAnimationProvider, ICourageProvider, ITargetInfosProvider, IRelationshipProvider
    {
        /// <summary>
        /// Head transform. Used for checking fov, look at etc...
        /// </summary>
        Transform HeadTransform { get; set; }
        
        /// <summary>
        /// Unity's enabled wrapper
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Ai component
        /// </summary>
        Ai Ai { get; }

        /// <summary>
        /// Character's waypoints
        /// </summary>
        List<Waypoint> Waypoints { get; }
    }
}