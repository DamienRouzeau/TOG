using UnityEngine;

public class PlayerActions : MonoBehaviour
{
	public void HideGuns()
	{
		if (Player.myplayer != null)
		{
			Player.myplayer.HideGuns();
		}
	}

	public void ShowGuns()
	{
		if (Player.myplayer != null)
		{
			Player.myplayer.ShowGuns();
		}
	}

	public void SendCommand(string name)
	{
		if (Player.myplayer != null)
		{
			Player.myplayer.ReceiveCommand(name);
		}
	}

	public void SetGoalFromName(string name)
	{
		if (Player.myplayer != null)
		{
			Player.myplayer.SetGoalFromName(name);
		}
	}

	public void UpdateGoalCounter()
	{
		if (Player.myplayer != null)
		{
			Player.myplayer.UpdateGoalCounter();
		}
	}

	public void IncrementGoalCounterMax()
	{
		if (Player.myplayer != null)
		{
			Player.myplayer.IncrementGoalCounterMax();
		}
	}

	public void ArchiveA2()
	{
		if (Player.myplayer != null)
		{
			Player.myplayer.ArchiveA2();
		} 
	}
    public void ArchiveB1()
    {
        if (Player.myplayer != null)
        {
            Player.myplayer.ArchiveB1();
        }
    }
    public void ArchiveB2()
    {
        if (Player.myplayer != null)
        {
            Player.myplayer.ArchiveB2();
        }
    }
    public void ArchiveC()
    {
        if (Player.myplayer != null)
        {
            Player.myplayer.ArchiveC();
        }
    }
    public void ArchiveAnomalie()
    {
        if (Player.myplayer != null)
        {
            Player.myplayer.ArchiveAnomalie();
        }
    }


    public void IncrementStat(string statId)
	{
		//Debug.Log("[STAT] IncrementStat " + statId + " " + GameflowBase.GetPlayerStat(statId));
		if (GameflowBase.instance != null)
			GameflowBase.IncrementPlayerStat(statId, 1);
		//Debug.Log("[STAT] IncrementStat " + statId + " " + GameflowBase.GetPlayerStat(statId));
	}

	public void AddTeamGold(int gold)
	{
		if (Player.myplayer != null)
		{
			Player.myplayer.SetCollectedTeamGold(Player.myplayer.teamGold + gold);
		}
	}

	public void AddProgress(string data)
	{
		if (Player.myplayer != null)
		{
			Player.myplayer.AddProgress(data);
		}
	}
	public void AddPercent()
	{
		if (Player.myplayer != null)
		{
			Player.myplayer.AddPercent();
		}
	}

	public void AddVRIKPlatform(GameObject parent)
	{
		if (Player.myplayer != null)
		{
			Player.myplayer.AddVRIKPlatform(parent);
		}
	}

	public void RemoveVRIKPlatform(GameObject parent)
	{
		if (Player.myplayer != null)
		{
			Player.myplayer.RemoveVRIKPlatform(parent);
		}
	}

	public void AddLife(int Gift)
	{
		if (Player.myplayer != null)
		{
			Player.myplayer.AddLife(Gift);
		}
	}
}
