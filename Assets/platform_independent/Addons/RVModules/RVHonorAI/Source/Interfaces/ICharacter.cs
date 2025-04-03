// Created by Ronis Vision. All rights reserved
// 29.01.2021.

using RVModules.RVSmartAI.Content.Scanners;
using UnityEngine.Events;

namespace RVHonorAI
{
    /// <summary>
    /// The most general character-defining contract that assumes only that:
    /// agent can move, can be targetet, can have relationship to other agents, can be damaged, can be scanned
    /// todo to consider if it would be good idea to have two - different lvl of detail interfaces of ai agents - like IAiAgent and ICharacter,
    /// with IAiAgent being very lightweight(like Ichar now) and ICharacter that would be much more HonorAi systems-specific 
    /// </summary>
    public interface ICharacter : IHealth, ITarget, IScannable, IDamageable, IRelationshipProvider, ITargetProvider
    {
        #region Properties

        /// <summary>
        /// UnityEvent called when character dies
        /// </summary>
        UnityEvent OnKilled { get; set; }

        /// <summary>
        /// Running speed, in m/s
        /// </summary>
        float RunningSpeed { get; set; }

        /// <summary>
        /// Walking speed, in m/s
        /// </summary>
        float WalkingSpeed { get; set; }

        //Transform Transform { get; }
//        Transform HeadTransform { get; }
//        float Health { get; }
//        AiGroup AiGroup { get; }
//        float DamagePerSecond { get; }
//        float Armor { get; }
//        float Courage { get; }
//        float AttackRange { get; }

        #endregion
    }
}