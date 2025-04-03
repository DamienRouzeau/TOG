using System.Diagnostics;

public class ZombieState
{
    public virtual void EnterState(ZombieController stateMachine)
    { }

    public virtual void ExitState(ZombieController stateMachine)
    { }

    public virtual void UpdateState(ZombieController stateMachine)
    {
        if (stateMachine.currentHealth <= 0 && stateMachine.currentState != stateMachine.deadState)
        {
            stateMachine.TransitionToState(stateMachine.deadState);
        }

        if (stateMachine.CheckIfHealthLost() && stateMachine.currentState != stateMachine.deadState)
        {
            if (!stateMachine.isInvulnerable)
            {
                stateMachine.TransitionToState(stateMachine.stunnedState);
            }
        }
    }
}
