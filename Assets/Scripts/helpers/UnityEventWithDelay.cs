using UnityEngine;
using UnityEngine.Events;

public class UnityEventWithDelay : MonoBehaviour
{
    public UnityEvent unityEvent;

    [SerializeField]
    private float invokeAfter = 1f;
    [SerializeField]
    private bool autoDestroy = false;
    [SerializeField]
    private bool autoRepeat = false;
    [SerializeField]
    private bool resetOnEnable = false;

    private float elapsed = 0f;
    private bool invoked = false;

	private void OnEnable()
	{
        if (resetOnEnable)
            Reset();
    }

	// Update is called once per frame
	void Update()
    {
        if (invoked)
            return;
        elapsed += Time.deltaTime;
        if (elapsed > invokeAfter)
        {
            unityEvent.Invoke();
            invoked = true;
            if (autoDestroy)
                Destroy(this);
            if (autoRepeat)
                Reset();
        }
    }

    private void Reset()
    {
        elapsed = 0f;
        invoked = false;
    }
}
