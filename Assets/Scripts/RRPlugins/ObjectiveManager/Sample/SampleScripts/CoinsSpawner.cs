using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RRObjective
{
    public class CoinsSpawner : MonoBehaviour
    {
        [SerializeField]
        private GameObject coinPrefab = null;
        [SerializeField]
        private PathHall[] pathHall = null;
        [SerializeField]
        private float coinsBySecond = 1f;

        private float m_fSpawnTimer;


        // Start is called before the first frame update
        void Start()
        {
            ComputeNextSpawnTime();
        }

        // Update is called once per frame
        void Update()
        {
            if( RRObjectiveManager.instance.state== RRObjectiveManager.ObjectifManagerState.playing && Time.time > m_fSpawnTimer )
            {
                ComputeNextSpawnTime();
                SpawnCoin();
            }
        }

        private void ComputeNextSpawnTime()
        {
            m_fSpawnTimer = Time.time + (1f / coinsBySecond);
        }

        private void SpawnCoin()
        {
            Debug.Assert(pathHall != null, "path hall not define in Coins Spawner");
            Debug.Assert(coinPrefab != null, "coinPrefab not define in Coins Spawner");
            for( int i=0; i<pathHall.Length; i++ )
            {
                GameObject coin = GameObject.Instantiate(coinPrefab);
                coin.transform.position = pathHall[i].GetPositionAtTime(Random.Range(0f, 1f), Random.Range(0f, 1f));
            }
        }
    }
}
