// Created by Ronis Vision. All rights reserved
// 12.09.2020.
namespace RVHonorAI
{
    public interface ICourageProvider
    {
        /// <summary>
        /// Courage. Lower value means character will flee instead of fighting with lower enemie's advantage
        /// </summary>
        float Courage { get; set; }
    }
}