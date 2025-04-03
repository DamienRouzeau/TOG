using UnityEngine;
using Valve.VR.InteractionSystem;

public class boat_pump : MonoBehaviour
{
    [SerializeField]
    private LinearMapping _linear = null;

    public void SetValue(float val)
	{
        if (_linear != null)
            _linear.value = val;
    }

    public float GetValue()
    {
        if (_linear != null)
            return _linear.value;
        return -1f;
    }

    // DEBUG PUMP
    /*
    private void Update()
	{
        if (Input.GetKey(KeyCode.PageDown))
		{
            SetValue(Mathf.Max(GetValue() - Time.deltaTime * 5f, 0f));
        }
        if (Input.GetKey(KeyCode.PageUp))
        {
            SetValue(Mathf.Min(GetValue() + Time.deltaTime * 5f, 1f));
        }
    }
    */
}
