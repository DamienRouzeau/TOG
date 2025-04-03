using UnityEngine;

public class ZombieAttackingState : ZombieState
{
    public override void EnterState(ZombieController stateMachine)
    {
        stateMachine.OnEndAttack += CheckIfAnimationFinished;

        stateMachine.SetDestination(stateMachine.GetPlayerTarget());
        stateMachine.SetSpeed(0f);
        stateMachine.SetTargetSpeed(0f);
    }

    public override void UpdateState(ZombieController stateMachine)
    {
        base.UpdateState(stateMachine);

        if (stateMachine.GetPlayerTarget() == null)
        {
            return;
        }

        // Regarde le player
        Quaternion currentRotation = stateMachine.transform.parent.rotation;
        Vector3 direction = stateMachine.GetPlayerTarget().position - stateMachine.transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
            stateMachine.transform.parent.rotation = Quaternion.Slerp(currentRotation, targetRotation, 0.1f);
        }
    }
    public override void ExitState(ZombieController stateMachine)
    {
        stateMachine.OnEndAttack -= CheckIfAnimationFinished;
    }

    public void CheckIfAnimationFinished(ZombieController stateMachine)
    {
        if (stateMachine.GetDistanceToTarget() > stateMachine.attackDistance)
        {
            stateMachine.TransitionToState(stateMachine.chasingState);
        }
        else
        {
            var nextState = DetermineNextAttackState(stateMachine);
            stateMachine.TransitionToState(nextState);
        }
    }

    private ZombieState DetermineNextAttackState(ZombieController stateMachine)
    {
        if (stateMachine.transform.parent.tag == "HumpedMutant")
        {
            switch (Random.Range(0, 3))
            {
                case 0: return stateMachine.attackingState1;
                case 1: return stateMachine.attackingState2;
                default: return stateMachine.attackingState3;
            }
        }
        else
        {
            return Random.Range(0, 2) == 0 ? stateMachine.attackingState1 : stateMachine.attackingState2;
        }
    }
}
