using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RRObjective
{
    public class CoinsCollector : MonoBehaviour
    {
        [SerializeField]
        RRObjectivePlayer objectivePlayer = null;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag != "Player")
            {
                GameObject.Destroy(collision.gameObject);
                RRObjectiveManager.instance.UpdateObjective("Coins", objectivePlayer.playerId, new ObjectiveInt.IntObjData(1));
            }
        }
    }
}
