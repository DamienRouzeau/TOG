using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLineTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (multiplayerlobby.IsInEndlessRace)
            return;

        boat_followdummy followdummy = other.GetComponent<boat_followdummy>();

        if( followdummy == null )
        {
            // search in children
            followdummy = other.GetComponentInChildren<boat_followdummy>();
        }

        if (followdummy == null)
        {
            // search in parent
            followdummy = other.GetComponentInParent<boat_followdummy>();
        }

        if (followdummy == null || followdummy.dummy == null)
        {
            return;
        }

        followdummy.hasReachedFinishLine = true;

        gameflowmultiplayer.SetTeamFinish(followdummy.team);
    }
}
