using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieExitingGroundState : ZombieState
{
    private float animationDuration;
    private float animationStartTime;

    public override void EnterState(ZombieController stateMachine)
    {
        stateMachine.animator.SetBool("isExitingGround", true);
        AnimatorStateInfo stateInfo = stateMachine.animator.GetCurrentAnimatorStateInfo(0); 
        animationDuration = stateInfo.length;
        animationStartTime = Time.time;
    }

    public override void UpdateState(ZombieController stateMachine)
    {
        if (Time.time - animationStartTime >= animationDuration)
        {
            stateMachine.TransitionToState(stateMachine.chasingState);
        }
    }

    public override void ExitState(ZombieController stateMachine)
    {
        stateMachine.animator.SetBool("isExitingGround", false);
        stateMachine.PlayScream();
    }
}
