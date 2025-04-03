using System.Collections;
using UnityEngine;

public class ThemeReplaceObjects : MonoBehaviour
{
    [System.Serializable]
    public class ThemeArray : RREnumArray<multiplayerlobby.SkinTheme, GameObject> { }

    public bool autoStart = true;

    [SerializeField]
    private ThemeArray _themePrefabs = null;
    [SerializeField]
    private GameObject[] _objectsToReplace = null;
    [SerializeField]
    private float _delayBettweenInstances = 0.1f;
    [SerializeField]
    private bool _deleteOldObject = false;
    [SerializeField]
    private string _childName = null;
    [SerializeField]
    private Vector3 _offset = Vector3.zero;
    [SerializeField]
    private Vector3 _scale = Vector3.one;
    [SerializeField]
    private Vector3 _rotation = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        if (autoStart)
            ReplaceObjects(_delayBettweenInstances);
    }

    public void SetPrefabForTheme(multiplayerlobby.SkinTheme theme, GameObject prefab)
	{
        _themePrefabs[multiplayerlobby.theme] = prefab;
    }

    public void ReplaceObjects(float delay=0f, bool force = false, int layer=-1)
	{
        GameObject prefab = _themePrefabs[multiplayerlobby.theme];
        if (force || prefab != null)
        {
            if (delay > 0f)
                StartCoroutine(ReplaceObjects(prefab, delay, layer));
            else
                ReplaceObjects(prefab, layer);
        }
    }

    private void ReplaceObjects(GameObject prefab, int layer=-1)
    {
        for (int i = 0; i < _objectsToReplace.Length; ++i)
        {
            GameObject go = _objectsToReplace[i];
            _objectsToReplace[i] = ReplaceObject(go, prefab, layer);
        }
    }

    private IEnumerator ReplaceObjects(GameObject prefab, float delay, int layer=-1)
	{
        for (int i = 0; i < _objectsToReplace.Length; ++i)
        {
            GameObject go = _objectsToReplace[i];
            _objectsToReplace[i] = ReplaceObject(go, prefab, layer);
            if (delay > 0f)
                yield return new WaitForSeconds(delay);
            else
                yield return null;
        }
    }

    private GameObject ReplaceObject(GameObject go, GameObject prefab, int layer = -1)
	{
        GameObject goToReturn = go;

        Transform root = go.transform;
        Vector3 offset = _offset;
        Vector3 scale = _scale;
        Vector3 rotation = _rotation;

        if (_deleteOldObject)
        {
            offset += root.localPosition;
            scale = Vector3.Scale(root.localScale, scale);
            rotation = (Quaternion.Euler(rotation) * root.localRotation).eulerAngles;
            root = root.parent;
            GameObject.Destroy(go);
        }
        else
        {
            foreach (Renderer rd in go.GetComponentsInChildren<Renderer>())
                rd.enabled = false;

            if (!string.IsNullOrEmpty(_childName))
            {
                Transform tr = root.Find(_childName);
                if (tr != null)
                    root = tr;
            }
        }
        GameObject instance = prefab != null ? Instantiate(prefab) : new GameObject();
        instance.transform.SetParent(root);
        instance.transform.localPosition = offset;
        instance.transform.localScale = scale;
        instance.transform.localRotation = Quaternion.Euler(rotation);

        if (layer >= 0)
            instance.SetLayerRecursively(layer);

        if (_deleteOldObject)
            goToReturn = instance;

        return goToReturn;
    }
}
