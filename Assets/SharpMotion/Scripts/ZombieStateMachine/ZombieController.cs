using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.PlayerLoop;
using Photon.Pun;

public partial class ZombieController : MonoBehaviour 
{
    public delegate void EndAttackEventHandler(ZombieController stateMachine);
    public event EndAttackEventHandler OnEndAttack;

    public delegate void EndHitstunEventHandler(ZombieController stateMachine);
    public event EndAttackEventHandler OnEndHitsun;

    public delegate void OnTouchEventHandler(ZombieController stateMachine);
    public event EndAttackEventHandler OnTouch;

    private Health healthScript;
    private Player playerHealth;
    private Transform playerBodyTarget;

    [SerializeField] private ZombieEffects zombieEffects;

    [Header("Zombie Settings")]
    [Tooltip("The maximum health points of the zombie.")]
    public float maxHealth = 100f;
    [Tooltip("The currentSpeed at which the zombie walks.")]
    public float walkSpeed = 1.0f;
    [Tooltip("The currentSpeed at which the zombie runs.")]
    public float runSpeed = 1.0f;
    [Tooltip("The distance at which the zombie will switch from walking to running.")]
    public float aggressionDistance = 10f;
    [Tooltip("The distance at which the zombie will start attacking the player.")]
    public float attackDistance = 2f;
    [Tooltip("The attack power of the zombie")]
    public float attackPower = 1f;
    [Tooltip("The scale of the zombie")]
    public float zombieScale = 1.0f;

    private float acceleration = 6.0f;
    private float targetSpeed = 0f;

    [Space]
    [Header("FX")]
    public AudioSource audioSourceMouth;
    public AudioSource audioSourceFX;

    [Space]
    [Header("Debug Only")]
    public float currentHealth;
    private float previousHealth;
    [SerializeField] private Transform playerTarget;
    [SerializeField] private Transform target;
    [SerializeField] private float currentSpeed = 0f;
    [SerializeField] public bool isRunning;
    public bool isInvulnerable;


    private float invulnerabilityTime = 0.5f;
    private NavMeshAgent navMeshAgent;

    private void Start()
    {
        healthScript = GetComponent<Health>();
        healthScript.currentHealth = currentHealth;
        healthScript.startingHealth = currentHealth;
        healthScript.maxHealth = healthScript.startingHealth;
        HandleDissolveFX = GetComponent<HandleDissolveFX>();
    }
    private void OnUpdate()
    {
        navMeshAgent.speed = currentSpeed;
        currentHealth = healthScript.currentHealth;
    }
    [PunRPC]
    public void SetStats(float walkS, float runS, float aggroD, float attackD, float scale, float health, float attackPower)
    {
        walkSpeed = walkS;
        runSpeed = runS;
        aggressionDistance = aggroD;
        attackDistance = attackD;
        maxHealth = health;
        this.attackPower = attackPower;
        zombieScale = Mathf.Min(1, 1.5f / scale);
        SetHealth();
        animator.SetFloat("speedScaleModifier", zombieScale);
    }

    public void SetHealth()
    {
        currentHealth = maxHealth;
        previousHealth = currentHealth;
    }

    public void SetDestination(Transform target)
    {
        this.target = target;
    }

    public void SetPlayerTarget(Transform target)
    {
        this.playerTarget = target;
    }

    // Retourne le joueur acuellement ciblé ou sinon trouve et retourne le plus proche
    public Transform GetPlayerTarget()
    {
        if (playerTarget == null)
        {
            FindClosestPlayerTarget();          
        }
        playerHealth = playerTarget.GetComponentInParent<Player>();

        Transform pt = playerHealth.GetComponentInChildren<Camera>().transform;
        Debug.Log(pt);
        return pt;
    }
    //VR_Camera
    public float GetSpeed()
    {
        return currentSpeed;
    }

    // Change de player ciblé pour celui le plus proche
    public void FindClosestPlayerTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Transform closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
            if (distanceToPlayer < closestDistance && player.GetComponent<Player>() != null)
            {
                closestDistance = distanceToPlayer;
                closestPlayer = player.transform;
            }
        }

        SetPlayerTarget(closestPlayer);
    }

    [PunRPC]
    public void SetEquipments(bool wrenchInHand, bool ironBarInBelly, bool excrescence, bool alienHeart)
    {
        FindChildGameObjectByName(transform, "Wretch").SetActive(wrenchInHand);
        FindChildGameObjectByName(transform, "Metal Beam").SetActive(ironBarInBelly);
        FindChildGameObjectByName(transform, "Alien Organ").SetActive(alienHeart);
        FindChildGameObjectByName(transform, "Excrescence").SetActive(alienHeart);
    }

    private GameObject FindChildGameObjectByName(Transform parent, string name)
    {
        if (parent == null)
        {
            return null;
        }

        Transform childTransform = parent.Find(name);
        if (childTransform != null)
        {
            return childTransform.gameObject;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            GameObject foundChild = FindChildGameObjectByName(parent.GetChild(i), name);
            if (foundChild != null)
            {
                return foundChild;
            }
        }
        return null;
    }

    public void RecalculatePath()
    {
        if (target != null && navMeshAgent.GetComponent<NavMeshAgent>().enabled)
        {
            navMeshAgent.SetDestination(target.position);
        }
    }

    // La vitesse actuelle
    public void SetSpeed(float speed)
    {
        currentSpeed = speed;
        animator.SetFloat("movementSpeed", currentSpeed);

        isRunning = currentSpeed > walkSpeed ? true : false;
    }

    // La vitesse à atteindre selon l'accélération
    public void SetTargetSpeed(float tspeed)
    {
        targetSpeed = tspeed;
        SetSpeed(currentSpeed);
    }

    public float GetDistanceToTarget()
    {
        if (playerTarget != null)
        {
            return Vector3.Distance(playerTarget.position, transform.position);
        }
        else
        {
            return 0f;
        }
    }
    public bool CheckIfHealthLost()
    {
        if (currentHealth < previousHealth)
        {
            previousHealth = currentHealth;
            return true;
        }
        return false;
    }

    public void EndAttack()
    {
        OnEndAttack?.Invoke(this);
    }

    public void EndHitstunAnimation()
    {
        OnEndHitsun?.Invoke(this);
    }

    public void Touch()
    {
        OnTouch?.Invoke(this);
        if (GetDistanceToTarget() <= attackDistance)
        {
            PlayTouch();
            playerHealth.health -= attackPower;
        }
    }

    public IEnumerator SetInvulnerability()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityTime);
        isInvulnerable = false;
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR

        Handles.color = Color.yellow;
        Handles.DrawWireArc(transform.position + Vector3.up * 1, Vector3.up, Vector3.forward, 360, aggressionDistance);
        Handles.color = Color.red;
        Handles.DrawWireArc(transform.position + Vector3.up * 1, Vector3.up, Vector3.forward, 360, attackDistance);
#endif
    }
}