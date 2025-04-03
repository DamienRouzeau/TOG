using UnityEngine;

public class RandomSpawnersAnchors : RandomSpawnersReferences
{
    #region Properties

    [SerializeField]
    private GameObject _spawnPrefab = null;

    #endregion

    protected override void SetSpawnActivated(Transform tr, bool activated)
    {
        if (activated)
        {
            GameObject go = GameObject.Instantiate(_spawnPrefab, tr);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
        }
        else
        {
            if (tr.childCount > 0)
            {
                GameObject.Destroy(tr.GetChild(0));
            }
        }
    }
}
