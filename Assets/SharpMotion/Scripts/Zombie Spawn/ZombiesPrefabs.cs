using UnityEngine;

public static class ZombiesPrefabs
{
    public static GameObject GhoulPrefab;
    public static GameObject InfectedPrefab;
    public static GameObject HumpedPrefab;

    public enum ZombieType
    {
        Ghoul,
        Infected,
        Humped
    }

    // Appelée pour initialiser les prefabs
    public static void Initialize()
    {
        GhoulPrefab = Resources.Load<GameObject>("GhoulPrefab");
        InfectedPrefab = Resources.Load<GameObject>("InfectedPrefab");
        HumpedPrefab = Resources.Load<GameObject>("HumpedPrefab");

        if (GhoulPrefab == null || InfectedPrefab == null || HumpedPrefab == null)
        {
            Debug.LogError("One or more prefab is null");
        }
        //Debug.Log("ZombiesPrefabs initialized");
    }
}
