// Created by Ronis Vision. All rights reserved
// 23.06.2020.

using System;
using RVModules.RVSmartAI;
using RVModules.RVSmartAI.Content.AI.DataProviders;
using RVModules.RVSmartAI.Content.AI.Tasks;
using RVModules.RVUtilities;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace RVHonorAI.Content.Ai.Tasks
{
    /// <summary>
    /// Allows to rotate ai agent toward target using simple transform manipulation and by using root motion
    /// todo configurable allow aiming while moving
    /// </summary>
    public class AimTowardTarget : AiJob
    {
        protected override string DefaultDescription => "";

        private float currentRotationSpeed;

        // does it make sense to have smth like this? if ai cant aim while moving it should be implemented in graph logic, not aiming logic
//        [SerializeField]
//        private BoolProvider canAimWhileMoving;

        [SerializeField]
        private float rotationSpeed = 160;

        private ITargetProvider targetProvider;
        private ICharacterAnimation characterAnimation;

        private float timeInRightAngle = 0;
        private float timeInRightAngleToFinishAiming = 2;

        private void OnEnable() => name = "rotateTowardTarget";

        protected override void OnContextUpdated()
        {
            base.OnContextUpdated();
            targetProvider = ContextAs<ITargetProvider>();
            characterAnimation = ContextAs<ICharacterAnimationProvider>().CharacterAnimation;
        }

        protected override void OnJobUpdate(float _dt)
        {
            var Target = targetProvider.Target;

            if (Target as Object == null)
            {
                FinishJob();
                return;
            }

            var myTransform = movement.Transform;
            var transformPosition = myTransform.position;
            transformPosition.y = 0;
            var targetPosition = Target.Transform.position;
            targetPosition.y = 0;

            var angle = Vector3.SignedAngle(myTransform.forward, (targetPosition - transformPosition), Vector3.up);

            float deadZone = 10;
            float targetRotationSpeed = 0;

            targetRotationSpeed = Mathf.Lerp(0, 1, Math.Abs(angle) * .04f);

            if (angle < 0) targetRotationSpeed *= -1;

            currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, targetRotationSpeed, UnityTime.DeltaTime * 5);

            var rotSpeed = currentRotationSpeed * rotationSpeed * UnityTime.DeltaTime;

            //var vel = characterAnimation.UseRootMotion ? characterAnimation.Animator.velocity : movement.Velocity;
            var vel = movement.Velocity;;

            var charMoving = vel.sqrMagnitude > 1;

//            if (charMoving && !canAimWhileMoving)
//            {
//                FinishJob();
//                return;
//            }

            characterAnimation.Moving = charMoving;
            characterAnimation.Rotating = !charMoving;
            
            // for animation we want normalized rotation speed value!
            characterAnimation.RotatingSpeed = currentRotationSpeed;

            // we take control over rotation of our character to allow facing other direction for aiming when moving
            if (!characterAnimation.UseRootMotion || charMoving)
            {
                myTransform.Rotate(Vector3.up, rotSpeed, Space.Self);
            }

            if (Math.Abs(angle) < deadZone)
            {
                timeInRightAngle += UnityTime.DeltaTime;
                if (timeInRightAngle > timeInRightAngleToFinishAiming) FinishJob();
            }
        }

        protected override void OnJobStart()
        {
            timeInRightAngle = 0;
            movement.UpdateRotation = false;
        }

        protected override void OnJobFinish()
        {
            characterAnimation.Rotating = false;
            characterAnimation.Moving = false;

            if (Context as Object == null) return;
            movement.UpdateRotation = true;
        }
    }
}