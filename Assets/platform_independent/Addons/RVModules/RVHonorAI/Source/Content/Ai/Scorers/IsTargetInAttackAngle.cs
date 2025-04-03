// Created by Ronis Vision. All rights reserved
// 21.06.2020.

using RVModules.RVSmartAI;
using RVModules.RVSmartAI.Content.AI.Scorers;
using UnityEngine;

namespace RVHonorAI.Content.Ai.Scorers
{
    /// <summary>
    /// Make sure ITargetProvider.Target won't be null before using this scorer
    /// Required context: ITargetProvider, IAttackAngle, IMovement
    /// </summary>
    public class IsTargetInAttackAngle : AiAgentBaseScorer
    {
        #region Public methods

        [SerializeField]
        private float notInAttackAngleScore;

        private ITargetProvider targetProvider;
        private IAttackAngle attackAngle;

        protected override string DefaultDescription => "Make sure ITargetProvider.Target won't be null before using this scorer" +
                                                        "\n Required context: ITargetProvider, IAttackAngle, IMovement";

        protected override void OnContextUpdated()
        {
            base.OnContextUpdated();
            targetProvider = ContextAs<ITargetProvider>();
            attackAngle = GetComponentFromContext<IAttackAngle>();
        }

        public override float Score(float _deltaTime)
        {
            //if (characterAi.Target as Object == null) return 0;
            var transformPosition = movement.Position;
            transformPosition.y = 0;
            var targetPosition = targetProvider.Target.Transform.position;
            targetPosition.y = 0;

            var forwardVector = movement.Rotation * Vector3.forward;
            var angle = Vector3.Angle(forwardVector, targetPosition - transformPosition);

            return angle < attackAngle.AttackAngle ? score : notInAttackAngleScore;
        }

        #endregion
    }
}