using UnityEngine;

public abstract class RandomSpawnersBase : MonoBehaviour
{
	#region Properties

	[SerializeField]
    private int _spawnInstanceMin = 0;
    [SerializeField]
    private int _spawnInstanceMax = 0;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    public void Init()
    {
        int randomCount = GetRandomInstanceCount(_spawnInstanceMin, _spawnInstanceMax);
        Debug.Log($"[SPAWN] Spawn count {randomCount} here {gameObject.name}!");
        SpawnInstances(randomCount);
    }

    public abstract int GetRandomInstanceCount(int min, int max);
    public abstract void SpawnInstances(int count);
}
