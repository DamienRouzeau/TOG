// Created by Ronis Vision. All rights reserved
// 06.09.2020.

namespace RVHonorAI
{
    /// <summary>
    /// todo desc
    /// </summary>
    public interface IRelationshipProvider
    {
        /// <summary>
        /// Check's relationship to other
        /// </summary>
        bool IsEnemy(IRelationshipProvider _other, bool _contraCheck = false);

        /// <summary>
        /// Check's relationship to other
        /// </summary>
        bool IsAlly(IRelationshipProvider _other);

        /// <summary>
        /// Our ai group
        /// </summary>
        AiGroup AiGroup { get; set; }
    }
}