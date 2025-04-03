// Created by Ronis Vision. All rights reserved
// 07.07.2020.

using System.Collections.Generic;

namespace RVHonorAI
{
    public interface ITargetInfosProvider
    {
        #region Properties

        /// <summary>
        /// List of current target info (targets we know about, not necessarily see them)
        /// </summary>
        List<TargetInfo> TargetInfos { get; }

        /// <summary>
        /// Dictionary of current target info (targets we know about, not necessarily see them)
        /// </summary>
        Dictionary<ITarget, TargetInfo> TargetInfosDict { get; }

        #endregion
    }
}