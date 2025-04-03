using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieStunnedState : ZombieState
{

    public override void EnterState(ZombieController stateMachine)
    {
        stateMachine.StartCoroutine(stateMachine.SetInvulnerability());
        stateMachine.animator.SetBool("isStunned", true);
        stateMachine.animator.Play("Hitstun", 0, 0f);
        stateMachine.GetComponentInParent<NavMeshAgent>().isStopped = true;
        stateMachine.GetComponentInParent<NavMeshAgent>().velocity = Vector3.zero;
        stateMachine.OnEndHitsun += CheckIfAnimationFinished;
        stateMachine.PlayHurt();
    }

    public override void UpdateState(ZombieController stateMachine)
    {
        base.UpdateState(stateMachine);
    }

    public override void ExitState(ZombieController stateMachine)
    {
        stateMachine.animator.SetBool("isStunned", false);
        stateMachine.GetComponentInParent<NavMeshAgent>().isStopped = false;
        stateMachine.OnEndHitsun -= CheckIfAnimationFinished;
        stateMachine.isInvulnerable = false;
    }

    public void CheckIfAnimationFinished(ZombieController stateMachine)
    {
        if (stateMachine.GetDistanceToTarget() > stateMachine.attackDistance)
        {
            stateMachine.TransitionToState(stateMachine.chasingState);
        }
        else
        {
            if (Random.Range(0, 2) == 0)
            {
                stateMachine.TransitionToState(stateMachine.attackingState1);
            }
            else
            {
                stateMachine.TransitionToState(stateMachine.attackingState2);
            }
        }
    }
}
