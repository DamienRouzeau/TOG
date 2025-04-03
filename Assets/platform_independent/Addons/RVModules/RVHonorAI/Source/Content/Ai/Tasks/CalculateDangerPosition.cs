// Created by Ronis Vision. All rights reserved
// 16.01.2020.

using System;
using System.Security.Cryptography;
using RVModules.RVSmartAI;
using RVModules.RVSmartAI.Content.AI.Tasks;
using RVModules.RVUtilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RVHonorAI.Content.Ai.Tasks
{
    /// <summary>
    /// Required context: ITargetsProvider, IDangerDirectionProvider
    /// </summary>
    public class CalculateDangerPosition : AiAgentBaseTask
    {
        [Header("Time interval between AiTask execution, in seconds")]
        public float callInterval = 1f;

        private float lastCallTime;

        private ITargetInfosProvider targetInfosProvider;
        private IDangerPositionProvider dangerPositionProvider;

        protected override void OnContextUpdated()
        {
            base.OnContextUpdated();
            targetInfosProvider = ContextAs<ITargetInfosProvider>();
            dangerPositionProvider = ContextAs<IDangerPositionProvider>();
        }

        protected override void Execute(float _deltaTime)
        {
            if (UnityTime.Time - lastCallTime < callInterval) return;
            lastCallTime = UnityTime.Time;

            if (targetInfosProvider.TargetInfos.Count == 0) return;

            var dangerPos = Vector3.zero;
            var dangerSum = 0f;

            foreach (var ti in targetInfosProvider.TargetInfos)
            {
                var target = ti.Target;
                if (target as Object == null) continue;

                var targetDanger = target.Danger;
                if (targetDanger < 0) targetDanger = 0;
                dangerPos += ti.LastSeenPosition * targetDanger;
                dangerSum += targetDanger;
            }

            if (dangerSum <= 0)
            {
                dangerPos /= targetInfosProvider.TargetInfos.Count;
            }
            else
            {
                dangerPos /= dangerSum;
            }

            dangerPositionProvider.DangerPosition = dangerPos;
        }
    }
}