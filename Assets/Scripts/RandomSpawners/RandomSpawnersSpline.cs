using UnityEngine;

public class RandomSpawnersSpline : RandomSpawnersBase
{
    #region Properties

    [SerializeField]
    private SplineController _spawnSpline = null;
    [SerializeField]
    private GameObject _spawnPrefab = null;

    #endregion

    public override int GetRandomInstanceCount(int min, int max)
    {
        if (min == 0)
        {
            Debug.LogWarning($"[SPAWN] Not enough spawn instance min here {gameObject.name}!");
        }
        if (max < min)
        {
            Debug.LogWarning($"[SPAWN] Not enough spawn instance max here {gameObject.name}!");
        }
        return Random.Range(min, max + 1);
    }

    public override void SpawnInstances(int count)
    {
        Transform tr = transform;
        if (tr.childCount > 0)
        {
            for (int i = tr.childCount - 1; i >= 0; --i)
                GameObject.Destroy(tr.GetChild(i).gameObject);
        }

        for (int i = 0; i < count; ++i)
        {
            float randomTime = Random.Range(0f, 1f );
            GameObject go = GameObject.Instantiate(_spawnPrefab, tr);
            go.transform.position = _spawnSpline.GetPositionAtTime(randomTime);
            go.transform.localScale = Vector3.one;
            go.transform.rotation = _spawnSpline.GetRotationAtTime(randomTime);
        }
    }
}
