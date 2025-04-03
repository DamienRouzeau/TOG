using UnityEngine;

public class VersionActivator : MonoBehaviour
{
    [SerializeField]
    private GameObject[] _targets = null;
    [SerializeField]
    private bool _value = false;
    [SerializeField]
    private bool _useStandalone = false;
    [SerializeField]
    private bool _useLauncher = false;    

    // Start is called before the first frame update
    void Start()
    {
        if (_useStandalone)
        {
            if (PhotonNetworkController.soloMode)
                ActivateTargets(_value);
            else
                ActivateTargets(!_value);
        }
        if (_useLauncher)
        {
#if USE_LAUNCHER
            ActivateTargets(_value);
#else
            ActivateTargets(!_value);
#endif
        }
    }

    private void ActivateTargets(bool activate)
	{
        foreach (GameObject target in _targets)
        {
            Debug.Log($"[VA] ActivateTargets {target.name} {activate}");
            target.SetActive(activate);
        }
	}
}
