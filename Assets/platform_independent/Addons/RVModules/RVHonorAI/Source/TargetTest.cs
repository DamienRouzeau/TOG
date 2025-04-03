// Created by Ronis Vision. All rights reserved
// 19.02.2021.

using RVModules.RVSmartAI.Content.Scanners;
using UnityEngine;

namespace RVHonorAI
{
    /// <summary>
    /// Minimal implementation component that can be detected and attacked by AI
    /// </summary>
    public class TargetTest : MonoBehaviour, ITarget, IRelationshipProvider, IScannable
    {
        [SerializeField]
        private AiGroup aiGroup;

        [SerializeField]
        private Transform visibilityCheckTransform;

        [SerializeField]
        private Transform aimTransform;

        [SerializeField]
        private float danger;

        public Transform VisibilityCheckTransform => visibilityCheckTransform;

        public Transform Transform => transform;

        public Transform AimTransform => aimTransform;

        public float Danger => danger;

        public bool IsEnemy(IRelationshipProvider _other, bool _contraCheck = false) => true;

        public bool IsAlly(IRelationshipProvider _other) => false;

        public AiGroup AiGroup
        {
            get => aiGroup;
            set => aiGroup = value;
        }

        public Object GetObject => this;
    }
}