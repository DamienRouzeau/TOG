using System.Collections.Generic;
using UnityEngine;

public class CollectableCounter : MonoBehaviour
{
    public enum CollectableType
	{
        DNA
	}

    private static Dictionary<CollectableType, int> countersByType = null;

    [SerializeField]
    private CollectableType _type = CollectableType.DNA;

    // Start is called before the first frame update
    void Awake()
    {
        if (countersByType == null)
            countersByType = new Dictionary<CollectableType, int>();
        if (countersByType.ContainsKey(_type) == false)
            countersByType.Add(_type, 0);
        countersByType[_type]++;
    }

    public static int GetCounterFromType(CollectableType type)
	{
        if (countersByType != null && countersByType.ContainsKey(type))
            return countersByType[type];
        else
            return 0;
    }
}
