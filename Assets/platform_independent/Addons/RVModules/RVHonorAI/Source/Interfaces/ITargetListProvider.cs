// Created by Ronis Vision. All rights reserved
// 07.07.2020.

using System.Collections.Generic;

namespace RVHonorAI
{
    public interface ITargetListProvider
    {
        #region Properties

        /// <summary>
        /// List of targets
        /// </summary>
        List<ITarget> Targets { get; }

        #endregion
    }
}