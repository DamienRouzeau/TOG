using UnityEngine;

public class SpawnItem : MonoBehaviour
{
    [SerializeField]
    private GameObject _objectToInstantiate = null;
    [SerializeField]
    private Transform _trParent = null;
    [SerializeField]
    private Vector3 _posOffset = Vector3.zero;
    [SerializeField]
    private Vector3 _rotOffset = Vector3.zero;
    [SerializeField]
    private Vector3 _scaleOffset = Vector3.one;
    [SerializeField]
    private bool _instantiateOnStart = false;

    // Start is called before the first frame update
    void Start()
    {
        if (_instantiateOnStart)
            InstantiateObject();
    }

    public void InstantiateObject()
    {
        GameObject go = GameObject.Instantiate(_objectToInstantiate, _trParent);
        go.transform.localPosition = _posOffset;
        go.transform.localRotation = Quaternion.Euler(_rotOffset);
        go.transform.localScale = _scaleOffset;
    }
}
