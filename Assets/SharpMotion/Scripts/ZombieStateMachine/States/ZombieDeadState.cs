using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieDeadState : ZombieState
{
    private float destroyingTime = 5f;
    private float disolvingTime = 1.5f;
    private float startDestroyTime = float.MinValue;

    public override void EnterState(ZombieController stateMachine)
    {
        stateMachine.GetComponentInParent<NavMeshAgent>().isStopped = true;
        stateMachine.animator.SetBool("isDead", true);
        stateMachine.PlayDeath();
    }

    public override void UpdateState(ZombieController stateMachine)
    {
        base.UpdateState(stateMachine);

        if (startDestroyTime == float.MinValue)
        {
            startDestroyTime = Time.time;
        }

        if (Time.time - startDestroyTime >= disolvingTime)
        {
            stateMachine.PlayDisolving();
        }

        if (Time.time - startDestroyTime >= destroyingTime)
        {
            Debug.Log("Zombie destroyed");
            Object.Destroy(stateMachine.transform.parent.gameObject);
        }
    }

    public override void ExitState(ZombieController stateMachine)
    {
    }
}
