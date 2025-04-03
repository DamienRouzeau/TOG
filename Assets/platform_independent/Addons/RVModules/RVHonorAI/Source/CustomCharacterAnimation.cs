// Created by Ronis Vision. All rights reserved
// 06.09.2020.

using UnityEngine;

namespace RVHonorAI
{
    public class CustomCharacterAnimation : MonoBehaviour, ICharacterAnimation
    {
        public bool UseRootMotion { get; }
        public Animator Animator { get; }
        public bool Moving { get; set; }
        public bool Rotating { get; set; }
        public float RotatingSpeed { get; set; }
        public void SetState(int _state) => throw new System.NotImplementedException();

        public void PlayAttackAnim() => throw new System.NotImplementedException();

        public void PlayDeathAnimation() => throw new System.NotImplementedException();
    }
}