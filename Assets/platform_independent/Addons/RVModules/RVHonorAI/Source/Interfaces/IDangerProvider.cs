// Created by Ronis Vision. All rights reserved
// 12.09.2020.

namespace RVHonorAI
{
    public interface IDangerProvider
    {
        /// <summary>
        /// Value for determining generally how strong and dangerous this entity is
        /// It should take into account thing like hp, damage per second etc
        /// </summary>
        float Danger { get; }
    }
}