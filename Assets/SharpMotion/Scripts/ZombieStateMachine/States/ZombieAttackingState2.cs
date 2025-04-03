using UnityEngine;

public class ZombieAttackingState2 : ZombieAttackingState
{
    public override void EnterState(ZombieController stateMachine)
    {
        base.EnterState(stateMachine);
        stateMachine.animator.SetBool("isAttacking2", true);
    }

    public override void UpdateState(ZombieController stateMachine)
    {
        base.UpdateState(stateMachine);
    }

    public override void ExitState(ZombieController stateMachine)
    {
        base.ExitState(stateMachine);
        stateMachine.animator.SetBool("isAttacking2", false);
    }
}
