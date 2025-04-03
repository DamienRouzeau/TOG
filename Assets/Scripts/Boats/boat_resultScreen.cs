using UnityEngine;

public class boat_resultScreen : MonoBehaviour
{
    [SerializeField]
    Transform[] _playerPositions = null;

    public Transform GetTargetForPlayerId(int id)
	{
        if (_playerPositions != null)
        {
            if (id >= 0 && id < _playerPositions.Length)
                return _playerPositions[id];
        }
        return null;
    }
}
