// Created by Ronis Vision. All rights reserved
// 14.03.2021.

using RVModules.RVSmartAI.Content.AI.DataProviders;
using RVModules.RVSmartAI.GraphElements;
using RVModules.RVUtilities;
using UnityEngine;

namespace RVModules.RVSmartAI.Content.AI.Scorers
{
    public class RandomChanceForScore : AiScorer
    {
        [SerializeField]
        private FloatProvider chancePerSecond;

        private float lastTimeCall;

        public override float Score(float _deltaTime)
        {
            float timeInterval = Mathf.Clamp01(UnityTime.Time - lastTimeCall);
            lastTimeCall = UnityTime.Time;
            var scored = RandomChance.Get(chancePerSecond * timeInterval);
            if (scored) return score;
            return 0;
        }
    }
}