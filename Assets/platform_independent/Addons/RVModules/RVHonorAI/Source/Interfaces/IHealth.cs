// Created by Ronis Vision. All rights reserved
// 05.09.2020.

namespace RVHonorAI
{
    /// <summary>
    /// Provides health and max health float values
    /// </summary>
    public interface IHealth
    {
        /// <summary>
        /// Health
        /// </summary>
        float Health { get; }

        /// <summary>
        /// Max health
        /// </summary>
        float MaxHealth { get; }
    }
}