using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gamesettings_difficulty : MonoBehaviour
{
    public static gamesettings_difficulty myself = null;

    [Header("Difficulty Settings")]
    public float bullet_x_pl_1 = 1.0f;
    public float bullet_x_pl_2 = 1.0f;
    public float bullet_x_pl_3 = 0.75f;
    public float bullet_x_pl_4 = 0.75f;
    public float bullet_x_pl_5 = 0.65f;
    public float bullet_x_pl_6 = 0.65f;
    public float bullet_x_pl_7 = 0.5f;
    public float bullet_x_pl_8 = 0.5f;

    public float cannon_x_pl_1 = 1.0f;
    public float cannon_x_pl_2 = 1.0f;
    public float cannon_x_pl_3 = 0.75f;
    public float cannon_x_pl_4 = 0.75f;
    public float cannon_x_pl_5 = 0.65f;
    public float cannon_x_pl_6 = 0.65f;
    public float cannon_x_pl_7 = 0.5f;
    public float cannon_x_pl_8 = 0.5f;

    public float ai_boat_life_x_pl_1 = 2.0f;
    public float ai_boat_life_x_pl_2 = 2.0f;
    public float ai_boat_life_x_pl_3 = 1.75f;
    public float ai_boat_life_x_pl_4 = 1.75f;
    public float ai_boat_life_x_pl_5 = 1.65f;
    public float ai_boat_life_x_pl_6 = 1.65f;
    public float ai_boat_life_x_pl_7 = 1.5f;
    public float ai_boat_life_x_pl_8 = 1.5f;

    private int checkednrofplayers = 0;

    private void Awake()
	{
        myself = this;
        DontDestroyOnLoad(gameObject);
    }

    public float GetCannonMultiplier()
    {
        return GetCannonMultiplier(GetNrOfPlayers());
    }

    public float GetCannonMultiplier(int playerInBoatCount)
    {
        switch (playerInBoatCount)
        {
            case 2: return (cannon_x_pl_2);
            case 3: return (cannon_x_pl_3);
            case 4: return (cannon_x_pl_4);
            case 5: return (cannon_x_pl_5);
            case 6: return (cannon_x_pl_6);
            case 7: return (cannon_x_pl_7);
            case 8: return (cannon_x_pl_8);
        }
        return (cannon_x_pl_1);
    }

    public float GetBulletMultiplier()
    {
        return GetBulletMultiplier(GetNrOfPlayers());
    }

    public float GetBulletMultiplier(int playerInBoatCount)
    {
        switch (playerInBoatCount)
        {
            case 2: return (bullet_x_pl_2);
            case 3: return (bullet_x_pl_3);
            case 4: return (bullet_x_pl_4);
            case 5: return (bullet_x_pl_5);
            case 6: return (bullet_x_pl_6);
            case 7: return (bullet_x_pl_7);
            case 8: return (bullet_x_pl_8);
        }
        return (bullet_x_pl_1);
    }

    public float GetAiBoatLifeMultiplier()
    {
        return GetAiBoatLifeMultiplier(GetNrOfPlayers());
    }

    public float GetAiBoatLifeMultiplier(int playerCount)
    {
        switch (playerCount)
        {
            case 2: return (ai_boat_life_x_pl_2);
            case 3: return (ai_boat_life_x_pl_3);
            case 4: return (ai_boat_life_x_pl_4);
            case 5: return (ai_boat_life_x_pl_5);
            case 6: return (ai_boat_life_x_pl_6);
            case 7: return (ai_boat_life_x_pl_7);
            case 8: return (ai_boat_life_x_pl_8);
        }
        return (ai_boat_life_x_pl_1);
    }

	
	public int GetNrOfPlayers()
	{
		if (checkednrofplayers == 0)
		{
			if (gameflowmultiplayer.areAllRacesLoaded)
				return (PhotonNetworkController.myself.NumberOfPlayers());
			else
			{
				mp_dum[] cnt = multiplayerlobby.myself.GetComponentsInChildren<mp_dum>(true);
				// BOT COUNT MUST BE MODIFIED AND CALCULATED LATER : TO BE CHANGED
				botcount[] cnt2 = multiplayerlobby.myself.GetComponentsInChildren<botcount>(true);

				checkednrofplayers = (cnt.Length / 4) + cnt2.Length;
			}
		}
		return (checkednrofplayers);
	}
}
