using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[ExecuteInEditMode]
public class ZombieSpawner : MonoBehaviour
{
    // Zombie
    //public int photonViewID;
    [Header("SPAWNER PROPERTIES")]
    [Tooltip("Defines the type of zombie to spawn.")]
    public ZombiesPrefabs.ZombieType zombieType;
    [Tooltip("Defines whether the zombie emerges from the ground or not.")]
    public bool spawnFromGround = false;
    [Tooltip("Defines the time in second before the zombie spawns.")]
    [Range(0, 200)]
    public float spawnDelayInSecond = 0f;

    [Header("ZOMBIE PROPERTIES")]
    [Tooltip("Defines the maximum health points of the zombie.")]
    [Range(1.0f, 10000f)]
    public float health = 1000f;
    [Tooltip("Defines the size of the zombie.")]
    [Range (0.5f, 3)]
    public float size = 1f;
    [Tooltip("Defines the walking speed of the zombie.")]
    [Range(1, 5)]
    public int walkingSpeed = 2;
    [Tooltip("Defines the running speed of the zombie.")]
    [Range(1, 10)]
    public int runningSpeed = 5;
    [Tooltip("Defines how far the zombie will run from the player.")]
    [Range(1.0f, 50f)]
    public float aggressionZoneDistance = 10.0f;
    [Tooltip("Defines the zombie's attack power.")]
    [Range(1.0f, 10000f)]
    public float attackPower = 200f;

    [Header("ZOMBIE GRAPHIC CUSTOMIZATION")]
    [Tooltip("Add a wrench in the zombie's hand.")]
    public bool wrenchInHand;
    [Tooltip("Add an iron bar to the zombie's belly.")]
    public bool ironBarInBelly;
    [Tooltip("Add a growth to the zombie.")]
    public bool excrescence;
    [Tooltip("Add a pulsating alien heart to the zombie.")]
    public bool alienHeart;

    private float attackDistance = 1.5f;

    // Spawner
    private int spawnQuantity = 1;
    private Transform playerTarget;

    private void Awake()
    {
        ZombiesPrefabs.Initialize();
    }

    public void SpawnWithTarget(/*Transform playerTarget*/)
    {
        //this.playerTarget = playerTarget;

        if (spawnQuantity <= 0)
        {
            return;
        }
        SpawnType(zombieType);
        spawnQuantity -= 1;
    }

    public void SpawnType(ZombiesPrefabs.ZombieType type)
    {
        GameObject prefab = null;

        switch (type)
        {
            case ZombiesPrefabs.ZombieType.Ghoul:
                prefab = ZombiesPrefabs.GhoulPrefab;
                break;
            case ZombiesPrefabs.ZombieType.Infected:
                prefab = ZombiesPrefabs.InfectedPrefab;
                break;
            case ZombiesPrefabs.ZombieType.Humped:
                prefab = ZombiesPrefabs.HumpedPrefab;
                break;
        }

        if (prefab == null)
        {
            Debug.LogError("The prefab for the selected zombie type is null");
            return;
        }

  
            GameObject targetZombie = PhotonNetwork.Instantiate(prefab.name, transform.position, Quaternion.identity);
            targetZombie.transform.localScale = new Vector3(size, size, size);
            ZombieController targetZombieController = targetZombie.GetComponentInChildren<ZombieController>();

            if (targetZombieController == null)
            {
                Debug.LogError("ZombieController component not found on the instantiated zombie");
                return;
            }

            if (spawnFromGround)
            {
                targetZombieController.InitFirstState(targetZombieController.exitingGroundState);
            }
            else
            {
                targetZombieController.InitFirstState(targetZombieController.chasingState);
            }

            targetZombieController.SetPlayerTarget(playerTarget);
            targetZombieController.SetStats(walkingSpeed, runningSpeed, aggressionZoneDistance, attackDistance, size, health, attackPower);
            targetZombieController.SetEquipments(wrenchInHand, ironBarInBelly, excrescence, alienHeart);
        



        /*//Renseignement de l'ID de photonview sur le prefab du zombie
        photonViewID = this.GetComponent<PhotonView>().ViewID;
        prefab.GetComponent<PhotonView>().ViewID = photonViewID;*/

       
    }

    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        Handles.color = Color.red;
        Handles.DrawWireArc(transform.position + Vector3.up * 1, Vector3.up, Vector3.forward, 360, aggressionZoneDistance);
#endif
    }
}
