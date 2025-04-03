using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieRunState : ZombieState
{
    public override void EnterState(ZombieController stateMachine)
    {
        //Debug.Log("Zombie est entrain de courir");
    }

    public override void UpdateState(ZombieController stateMachine)
    {
        stateMachine.RecalculatePath();
    }

    public override void ExitState(ZombieController stateMachine)
    {
        // Code à exécuter lors de la sortie de cet état
    }
}
