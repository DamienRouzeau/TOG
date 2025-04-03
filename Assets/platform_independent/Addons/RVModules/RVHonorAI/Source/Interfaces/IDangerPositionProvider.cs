// Created by Ronis Vision. All rights reserved
// 06.09.2020.

using UnityEngine;

namespace RVHonorAI
{
    /// <summary>
    /// todo replace this by graph variable, as its used by graphs only
    /// </summary>
    public interface IDangerPositionProvider
    {
        /// <summary>
        /// Danger direction - average of vectors from our position to all visible enemies, normalized
        /// This vector is actually reversed from danger (away from danger)
        /// </summary>
        Vector3 DangerPosition { get; set; }
    }
}