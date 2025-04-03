// Created by Ronis Vision. All rights reserved
// 07.10.2020.

using RVModules.RVSmartAI.Content.AI.DataProviders;
using RVModules.RVSmartAI.Content.AI.Tasks;
using RVModules.RVUtilities;
using UnityEngine;

namespace RVHonorAI.Content.Ai.Tasks
{
    /// <summary>
    /// Required context: IAttackProvider, IAttackSoundPlayer(optional), ICharacterAnimation(optional), IAttacker(if animationEventBasedAttack is false)
    /// </summary>
    public class AttackJob : AiJob
    {
        #region Fields

        [Header("If set to true, DealDamage should be called by unity event added to attack animation")]
        [SerializeField]
        private BoolProvider animationEventBasedAttack;

        [Header("Time in seconds after which attack will deal damage.")]
        [SerializeField]
        private FloatProvider damageTime;

        private bool attacked;
        private bool hasAnimation;
        private float attackTime;
        private float attackDuration;
        private ICharacterAnimation characterAnimation;
        private IAttackSoundPlayer attackSoundPlayer;
        private IAttacker attacker;

        #endregion

        #region Properties

        protected override string DefaultDescription => "Plays attack animation, plays attack sound and deal damage to current target";

        #endregion

        #region Not public methods

        protected override void OnContextUpdated()
        {
            base.OnContextUpdated();
            characterAnimation = GetComponentFromContext<ICharacterAnimationProvider>()?.CharacterAnimation;
            attackSoundPlayer = GetComponentFromContext<IAttackSoundPlayer>();
            attacker = GetComponentFromContext<IAttacker>();
            hasAnimation = characterAnimation != null;
        }

        protected override void OnJobStart()
        {
            attacked = false;
            attackDuration = 2;
            var currentAttack = attacker.CurrentAttack;
            if (currentAttack == null)
            {
                FinishJob();
                return;
            }

            attackDuration = currentAttack.AttackInterval;
            if (hasAnimation) characterAnimation.PlayAttackAnim();
            attackSoundPlayer?.PlayAttackSound();
        }

        protected override void OnJobUpdate(float _dt)
        {
            attackTime += UnityTime.DeltaTime;

            if (!attacked && animationEventBasedAttack.GetData() == false && attackTime >= damageTime)
            {
                attacker.Attack();
                attacked = true;
            }

            if (attackTime > attackDuration + UnityTime.DeltaTime) FinishJob();
        }

        protected override void OnJobFinish() => attackTime = 0;

        #endregion
    }
}