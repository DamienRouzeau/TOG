// Created by Ronis Vision. All rights reserved
// 24.06.2020.

using RVModules.RVSmartAI;
using RVModules.RVSmartAI.Content.AI.Tasks;
using RVModules.RVUtilities;
using UnityEngine;

namespace RVHonorAI.Content.Ai.Tasks
{
    /// <summary>
    /// Sets CharacterState to flee for set amount of time and set it to normal after
    /// </summary>
    public class FleeJob : AiJob
    {
        [SerializeField]
        protected float currentFleeTime;

        [SerializeField]
        protected float fleeTime = 5;

        private ICharacterStateProvider characterStateProvider;

        protected override void OnContextUpdated()
        {
            base.OnContextUpdated();
            characterStateProvider = ContextAs<ICharacterStateProvider>();
        }

        protected override void OnJobUpdate(float _dt)
        {
            currentFleeTime += UnityTime.DeltaTime;
            if (currentFleeTime > fleeTime)
            {
                FinishJob();
            }
        }

        protected override void OnJobStart()
        {
            characterStateProvider.CharacterState = CharacterState.Flee;
        }

        protected override void OnJobFinish()
        {
            characterStateProvider.CharacterState = CharacterState.Normal;
            currentFleeTime = 0;
        }
    }
}