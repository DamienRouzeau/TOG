using RRLib;
using UnityEngine;

public class RandomSpawnersReferences : RandomSpawnersBase
{
    #region Properties
    
    [SerializeField]
    protected Transform[] _references = null;

    #endregion

    public override int GetRandomInstanceCount(int min, int max)
    {
        int referenceCount = _references?.Length ?? 0;
        if (referenceCount == 0)
        {
            Debug.LogWarning($"[SPAWN] Not enough spawn reference here {gameObject.name}!");
        }
        if (min == 0)
        {
            Debug.LogWarning($"[SPAWN] Not enough spawn instance min here {gameObject.name}!");
        }
        if (max < min)
        {
            Debug.LogWarning($"[SPAWN] Not enough spawn instance max here {gameObject.name}!");
        }
        int randomMin = Mathf.Min(min, referenceCount);
        int randomMax = Mathf.Min(max, referenceCount);
        return Random.Range(randomMin, randomMax + 1);
    }

    public override void SpawnInstances(int count)
    {
        int referenceCount = _references?.Length ?? 0;
        if (referenceCount == 0)
            return;

        for (int i = 0; i < referenceCount; ++i)
        {
            SetSpawnActivated(_references[i], false);
        }

        RRRndArray rndArray = new RRRndArray((uint)referenceCount);

        for (int i = 0; i < count; ++i)
        {
            int idx = (int)rndArray.ChooseValue();
            //Debug.Log($"[SPAWN] Spawn idx {idx} here {gameObject.name}!");
            SetSpawnActivated(_references[idx], true);
        }
    }

    protected virtual void SetSpawnActivated(Transform tr, bool activated)
    {
        if (tr != null)
        {
            tr.gameObject.SetActive(activated);
        }
    }
}
