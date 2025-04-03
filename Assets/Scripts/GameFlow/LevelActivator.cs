using UnityEngine;
using UnityEngine.Events;

public class LevelActivator : MonoBehaviour
{
    [SerializeField]
    private int _level = 0;
    [SerializeField]
    private UnityEvent _events = null;

    // Start is called before the first frame update
    void OnEnable()
    {
        if (_level > 0 && multiplayerlobby.startLevel == _level - 1)
            TriggerEvents();
    }

    public void TriggerEvents()
	{
        _events?.Invoke();
    }
}
