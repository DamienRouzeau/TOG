using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class difficulty_occultme : MonoBehaviour
{
    [Header("Display if Nr of Players")]
    public int biggerthan = 0;
    public int smallerthan = 0;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        while (!gameflowmultiplayer.areAllRacesLoaded)
            yield return null;
        int nrpl = gamesettings_difficulty.myself.GetNrOfPlayers();
        if (biggerthan != 0)
        {
            if (nrpl <= biggerthan)                Destroy(gameObject);
        }
        if (smallerthan != 0)
        {
            if (nrpl >= smallerthan)                Destroy(gameObject);
        }

    }

}
