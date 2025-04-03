
using UnityEngine;
using UnityEngine.AI;

public partial class ZombieController : MonoBehaviour
{
    public Animator animator;

    public ZombieState currentState;

    public ZombieState exitingGroundState = new ZombieExitingGroundState();
    public ZombieState chasingState = new ZombieChasingState();
    public ZombieState attackingState1 = new ZombieAttackingState1();
    public ZombieState attackingState2 = new ZombieAttackingState2();
    public ZombieState attackingState3 = new ZombieAttackingState3();
    public ZombieState stunnedState = new ZombieStunnedState();
    public ZombieState deadState = new ZombieDeadState();

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        navMeshAgent = GetComponentInParent<NavMeshAgent>();
    }

    void Update()
    {
        OnUpdate();

        if (currentState != null)
        {
            currentState.UpdateState(this);
            OnUpdate();
        }
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
    }
    public void InitFirstState(ZombieState newState)
    {      
        currentState = newState;

        if (currentState != null)
        {
            currentState.EnterState(this);
        }

        if (currentState != exitingGroundState)
        {
            PlayScream();
        }
    }
    public void TransitionToState(ZombieState newState)
    {
        if (currentState != null)
        {
            currentState.ExitState(this);
        }

        currentState = newState;
        //Debug.Log(_currentState.ToString());

        if (currentState != null)
        {
            currentState.EnterState(this);
        }
    }

}
