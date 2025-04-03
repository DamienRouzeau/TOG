using System.Collections.Generic;
using UnityEngine;

public class SpawnpointReferences : MonoBehaviour
{
    #region Classes

    [System.Serializable]
    public class SpawnpointList
    {
        public string name;
        public int playerCountMin;
        public int playerCountMax;
        public Transform[] spawnpoints;
    }

    #endregion

    [SerializeField] private List<SpawnpointList> _spawnpoints = null;
    [SerializeField] private Transform _parentToSpawn = null;
    [SerializeField] private bool _showPoints = false;
    [Range(1, 16)]
    [SerializeField] private int _simulPlayerCount = 1;
    [Range(0, 16)]
    [SerializeField] private int _simulPlayerIndex = 0;

    public Transform[] GetSpawnpoints(int playerCount)
    {
        if (_spawnpoints != null)
        {
            foreach (SpawnpointList point in _spawnpoints)
            {
                if (point.playerCountMin > 0 && playerCount < point.playerCountMin)
                    continue;
                if (point.playerCountMax > 0 && playerCount > point.playerCountMax)
                    continue;
                return point.spawnpoints;
            }
        }
        return null;
    }

    public void TeleportPlayer()
	{
        ApplyToPlayer(true);
    }

    public void TeleportPlayerWithFadeIn()
    {
        ApplyToPlayer(true, true);
    }

    public void SetRespawn()
    {
        ApplyToPlayer(false);
    }

    private void ApplyToPlayer(bool teleport, bool withFadeIn = false)
	{
        int playerCount = GameflowBase.allFlows != null ? GameflowBase.allFlows.Length : 0;
        Transform[] spawnpoints = GetSpawnpoints(playerCount);
        int searchspawn = GameflowBase.GetActorIndexInTeam(GameflowBase.myId, GameflowBase.myTeam);

        if (searchspawn < 0)
		{
            searchspawn = GameflowBase.myId;
        }

        if (searchspawn < 0 || searchspawn >= spawnpoints.Length)
		{
            Debug.LogError($"Index {searchspawn} of spawnpoint not in range [0-{spawnpoints.Length}]!");
            return;
        }

        if (spawnpoints[searchspawn] == null)
		{
            Debug.LogError($"spawnpoint at index {searchspawn} is null!");
            return;
        }

        Transform spawnpoint = spawnpoints[searchspawn];
        if (spawnpoint != null)
        {
            if (teleport)
            {
                if (withFadeIn)
                    Player.myplayer.TeleportWithFadeIn(spawnpoint, _parentToSpawn);
                else
                    Player.myplayer.Teleport(spawnpoint.position, spawnpoint.rotation, _parentToSpawn);
            }
            Player.myplayer.SetStartSpawnPoint(spawnpoint);
        }
        else
        {
            Debug.LogError("No Spawnpoint found!!!");
        }
    }

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		if (_showPoints)
		{
            Transform[] points = GetSpawnpoints(_simulPlayerCount);
            for (int i = 0; i < _simulPlayerCount; ++i)
			{
                if (_simulPlayerIndex == 0 || _simulPlayerIndex == i + 1)
                    Gizmos.DrawCube(points[i].position, Vector3.one * 0.5f);
			}
        }
	}

#endif

}
