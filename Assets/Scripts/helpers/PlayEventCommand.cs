using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayEventCommand : MonoBehaviour
{
    [System.Serializable]
    public class EventCommand
	{
        public string name;
        public UnityEvent action;
	}

    [SerializeField]
    private List<EventCommand> _commands = null;

    public void PlayCommand(string name)
    {
        foreach (EventCommand command in _commands)
		{
            if (command.name == name)
                command.action?.Invoke();
		}
    }
}
