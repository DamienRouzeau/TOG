using System.Collections;
using UnityEngine;

public class PlayerAddTarget : MonoBehaviour
{
    [SerializeField]
    private UI_FollowTarget _followTargetPrefab = null;
    [SerializeField]
    private GameObject _miniMapPrefab = null;
    [SerializeField]
    private bool _addOnStart = false;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        if (_addOnStart)
        {
            while (Player.myplayer == null)
                yield return null;
            AddPlayerTarget();
        }
    }

	private void OnDisable()
	{
        if (_addOnStart)
        {
            RemovePlayerTarget();
        }
    }

    private void OnEnable()
    {
        if (_addOnStart)
        {
            AddPlayerTarget();
        }
    }

    public void AddPlayerTarget()
	{
        if (Player.myplayer != null)
            Player.myplayer.AddTargetWithPrefab(transform, _followTargetPrefab, _miniMapPrefab);
	}

    public void RemovePlayerTarget()
    {
        if (Player.myplayer != null)
            Player.myplayer.RemoveTarget(transform, false);
    }
}
