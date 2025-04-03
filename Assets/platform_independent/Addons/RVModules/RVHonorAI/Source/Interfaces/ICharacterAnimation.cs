// Created by Ronis Vision. All rights reserved
// 16.03.2021.

using UnityEngine;

namespace RVHonorAI
{
    /// <summary>
    /// Used to communicate character animations handling with other systems
    /// </summary>
    public interface ICharacterAnimation
    {
        #region Properties

        /// <summary>
        /// Should Unity's root motion be used
        /// </summary>
        bool UseRootMotion { get; }

        /// <summary>
        /// Returns Animator component
        /// </summary>
        Animator Animator { get; }

        /// <summary>
        /// Inform character animation component if character is currently moving
        /// </summary>
        bool Moving { set; }

        /// <summary>
        /// Inform character animation component if character is currently rotating in place
        /// </summary>
        bool Rotating { set; }

        /// <summary>
        /// To animate rotation, this should be in -1 to 1 range, and set relevant property in animator that controlls rotation animation
        /// (check CharacterAnimation for example implementation)
        /// </summary>
        float RotatingSpeed { set; }

        #endregion

        #region Public methods

        /// <summary>
        /// 0 - normal, 1 - combat state
        /// </summary>
        void SetState(int _state);

        /// <summary>
        /// 
        /// </summary>
        void PlayAttackAnim();

        /// <summary>
        /// 
        /// </summary>
        void PlayDeathAnimation();

        #endregion
    }
}