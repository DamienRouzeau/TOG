using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TriggerArea : MonoBehaviour
{
    [SerializeField, Range(0, 20)]
    private float width = 2;
    [SerializeField, Range(0, 20)]
    private float length = 1;
    [SerializeField, Range(0, 20)]
    private float height = 1;

    [Space]
    public List<ZombieSpawner> zombieSpawners = new List<ZombieSpawner> { };

    private BoxCollider _collider;

    private void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (_collider == null)
            {
                _collider = GetComponentInChildren<BoxCollider>();
                if (_collider == null)
                {
                    _collider = gameObject.AddComponent<BoxCollider>();
                    _collider.isTrigger = true;
                }
            }
            UpdateColliderSize();
        }
#endif
    }

    private void UpdateColliderSize()
    {
        if (_collider != null)
        {
            _collider.size = new Vector3(width, height, length);
            _collider.center = new Vector3(0, height / 2, 0);
        }
    }

    /*private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (var spawner in zombieSpawners)
            {
                StartCoroutine(SpawnWithDelay(spawner.spawnDelayInSecond,spawner, other));
            }
        }
    }*/ 
    public void Spawn()
    {
        
        
            foreach (var spawner in zombieSpawners)
            {
                StartCoroutine(SpawnWithDelay(spawner.spawnDelayInSecond,spawner));
            }
        
    }

    private void OnDrawGizmos()
    {
        if (_collider == null)
        {
            return;
        }

        Gizmos.color = Color.green;
        Vector3 colliderCenter = transform.position + _collider.center;
        Vector3 colliderSize = _collider.size;

        // Wireframe du collider
        Gizmos.DrawWireCube(colliderCenter, colliderSize);

        // Surface au sol du collider, légerement surélevée
        Vector3 elevation = new Vector3(0, 0.01f, 0);
        Vector3[] corners = new Vector3[4];
        corners[0] = colliderCenter + new Vector3(-colliderSize.x / 2, 0, -colliderSize.z / 2) + elevation;
        corners[1] = colliderCenter + new Vector3(colliderSize.x / 2, 0, -colliderSize.z / 2) + elevation;
        corners[2] = colliderCenter + new Vector3(colliderSize.x / 2, 0, colliderSize.z / 2) + elevation;
        corners[3] = colliderCenter + new Vector3(-colliderSize.x / 2, 0, colliderSize.z / 2) + elevation;

        Gizmos.color = new Color(0, 1, 0, 0.25f);
        Gizmos.DrawCube(colliderCenter - new Vector3(0, colliderSize.y / 2, 0) + elevation, new Vector3(colliderSize.x, 0, colliderSize.z));
    }

    private IEnumerator SpawnWithDelay(float delay, ZombieSpawner spawner/*, Collider other*/)
    {
        yield return new WaitForSeconds(delay);
        spawner.SpawnWithTarget(/*other.transform*/);
    }
}
