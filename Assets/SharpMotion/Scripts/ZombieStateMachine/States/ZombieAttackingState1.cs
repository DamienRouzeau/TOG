using UnityEngine;

public class ZombieAttackingState1 : ZombieAttackingState
{
    public override void EnterState(ZombieController stateMachine)
    {
        base.EnterState(stateMachine);
        stateMachine.animator.SetBool("isAttacking1", true);
    }

    public override void UpdateState(ZombieController stateMachine)
    {
        base.UpdateState(stateMachine);
    }

    public override void ExitState(ZombieController stateMachine)
    {
        base.ExitState(stateMachine);
        stateMachine.animator.SetBool("isAttacking1", false);
    }
}
