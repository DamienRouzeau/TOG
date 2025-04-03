using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayEventList : MonoBehaviour
{
    [SerializeField]
    private List<UnityEvent> _events = null;

    private int _numEvent = -1;

	public void Reset()
	{
        _numEvent = -1;
	}

	public void PlayNextEvent()
	{
        _numEvent++;
        PlayEvent(_numEvent);
    }

    public void PlayEvent(int num)
	{
        if (num >= 0 && num < _events.Count)
            _events[num].Invoke();
    }
}
