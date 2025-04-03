using System.Collections;

using UnityEngine;

public class ZombieChasingState : ZombieState
{
    Vector3 distanceToTarget;
    float targetSpeed = 0f;

    public override void EnterState(ZombieController stateMachine)
    {
        stateMachine.SetDestination(stateMachine.GetPlayerTarget());
        stateMachine.SetSpeed(0f);
        targetSpeed = stateMachine.walkSpeed;
        stateMachine.Growling(true);
    }

    public override void UpdateState(ZombieController stateMachine)
    {
        base.UpdateState(stateMachine);
        // AI
        stateMachine.SetDestination(stateMachine.GetPlayerTarget());
        stateMachine.RecalculatePath();

        // Est dans la zone d'aggression ou en ressort
        if (stateMachine.GetDistanceToTarget() < stateMachine.aggressionDistance)
        {
            targetSpeed = stateMachine.runSpeed / stateMachine.transform.localScale.x;
        }
        else
        {
            targetSpeed = stateMachine.walkSpeed / stateMachine.transform.localScale.x;
        }

        // Est dans la zone d'attack
        if (stateMachine.GetDistanceToTarget() < stateMachine.attackDistance)
        {
            // stop and attack
            targetSpeed = 0f;
            stateMachine.SetDestination(stateMachine.transform);
            stateMachine.RecalculatePath();

            if (Random.Range(0, 2) == 0)
            {
                stateMachine.TransitionToState(stateMachine.attackingState1);
            }
            else
            {
                stateMachine.TransitionToState(stateMachine.attackingState2);
            }
        }

        // Movement
        stateMachine.SetTargetSpeed(targetSpeed);

        if (targetSpeed != 0)
        {
            float normalizedSpeed = stateMachine.GetSpeed() / stateMachine.runSpeed * 10;
            stateMachine.animator.SetFloat("movementSpeed", normalizedSpeed);
        }
        else
        {
            stateMachine.animator.SetFloat("movementSpeed", 0);
        }
    }

    public override void ExitState(ZombieController stateMachine)
    {
        stateMachine.SetSpeed(0f);
        stateMachine.Growling(false);
    }
}
