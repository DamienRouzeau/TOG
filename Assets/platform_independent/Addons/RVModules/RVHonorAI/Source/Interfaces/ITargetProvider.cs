// Created by Ronis Vision. All rights reserved
// 07.07.2020.

namespace RVHonorAI
{
    /// <summary>
    /// This represents agent that can have target (to aim at, attack etc.)
    /// </summary>
    public interface ITargetProvider
    {
        #region Properties

        /// <summary>
        /// Current target
        /// </summary>
        ITarget Target { get; }
        
        /// <summary>
        /// Current target info
        /// </summary>
        TargetInfo TargetInfo { get; set; }

        #endregion
    }
}