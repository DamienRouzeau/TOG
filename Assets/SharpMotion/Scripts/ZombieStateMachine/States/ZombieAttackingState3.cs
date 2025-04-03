using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAttackingState3 : ZombieAttackingState
{
    public override void EnterState(ZombieController stateMachine)
    {
        base.EnterState(stateMachine);
        stateMachine.animator.SetBool("isAttacking3", true);
    }

    public override void UpdateState(ZombieController stateMachine)
    {
        base.UpdateState(stateMachine);
    }

    public override void ExitState(ZombieController stateMachine)
    {
        base.ExitState(stateMachine);
        stateMachine.StopVomit();
        stateMachine.animator.SetBool("isAttacking3", false);
    }
}
