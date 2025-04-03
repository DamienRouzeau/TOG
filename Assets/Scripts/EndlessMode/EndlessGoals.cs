using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessGoals : MonoBehaviour
{
	#region Enums

	public enum GoalCategory
	{
		TreasureHunt,
		CoinHunt,
		BoatKill,
		TurrelKill,
		RockKill,
		Dead
	}

	public enum TimeMode
	{
		NoLimit,
		WaitEndOfTime,
		FailedOnTime
	}

	public enum ResultGame
	{
		WaitForAllGoals,
		Accomplished,
		Failed
	}

	public enum GoalState
	{
		Empty,
		InProgress,
		Done,
		NotEnabled
	}

	#endregion

	#region Delegates

	public delegate void OnGoalsEndGameDelegate(ResultGame resultGame);

	#endregion

	#region Classes

	[System.Serializable]
	public class Goal
	{
		public GoalCategory category;
		public int counter;
		public ResultGame result;
		public GoalState state => _state;
		public float ratio => (float)_count / (float)counter;

		private GoalState _state;
		private int _count;

		public GoalState UpdateGoal()
		{
			if (counter == 0)
			{
				_state = GoalState.NotEnabled;
			}
			else
			{
				_count++;
				if (_count == counter)
					_state = GoalState.Done;
				else
					_state = GoalState.InProgress;
			}
			return _state;
		}
	}

	#endregion

	#region Properties

	public OnGoalsEndGameDelegate onGoalsEndGameCbk;
	public List<Goal> goals;
	public TimeMode timeMode = TimeMode.FailedOnTime;
	public float timeInSecond = 5f * 60f;

	public float currentTime => _time;

	private float _time = 0f;
	private bool _ingame = false;

	#endregion

	private IEnumerator Start()
	{
		while (!GameflowBase.areAllRacesStarted)
			yield return null;

		GameflowBase.onRaceEventDelegate += OnRaceEvent;

		StartTime();
	}

	private void OnDestroy()
	{
		GameflowBase.onRaceEventDelegate -= OnRaceEvent;
	}

	private void Update()
	{
		if (_ingame)
		{
			_time += Time.deltaTime;
			if (_time > timeInSecond)
			{
				switch (timeMode)
				{
					case TimeMode.FailedOnTime:
						EndGame(ResultGame.Failed);
						break;
				}
			}
		}
	}


	public void StartTime()
	{
		_ingame = true;
	}

	public void EndGame(ResultGame result)
	{
		Debug.Log($"EndGame {result}");
		_ingame = false;
		if (onGoalsEndGameCbk != null)
			onGoalsEndGameCbk(result);
	}

	public void UpdateGoal(GoalCategory category)
	{
		//Debug.Log($"UpdateGoal {category}");
		// Already accomplished
		if (_ingame == false || CheckIfAllGoalsAreDone())
			return;

		foreach (Goal goal in goals)
		{
			if (goal.category == category && goal.state != GoalState.Done)
			{
				Debug.Log($"UpdateGoal {category}");
				if (goal.UpdateGoal() == GoalState.Done)
				{
					if (goal.result != ResultGame.WaitForAllGoals)
					{
						EndGame(goal.result);
						return;
					}
				}
			}
		}
		if (CheckIfAllGoalsAreDone())
		{
			EndGame(ResultGame.Accomplished);
		}
	}

	public bool CheckIfAllGoalsAreDone()
	{
		bool needToWaitForAllGoals = false;
		foreach (Goal goal in goals)
		{
			if (goal.state == GoalState.Done && goal.result == ResultGame.Accomplished)
				return true;
			if (goal.state != GoalState.Done && goal.result == ResultGame.WaitForAllGoals)
				return false;
			if (goal.result == ResultGame.WaitForAllGoals)
				needToWaitForAllGoals = true;
		}
		//Debug.Log($"CheckIfAllGoalsAreDone {needToWaitForAllGoals}");
		return needToWaitForAllGoals;
	}

	private void OnRaceEvent(int team, bool isMe, string statId, object param)
	{
		Debug.Log($"OnRaceEvent team {team} isMe {isMe} - {statId} - param {param}");
		if (team != GameflowBase.myTeam)
			return;

		switch (statId)
		{
			case gamesettings.STAT_CHESTS:
				UpdateGoal(GoalCategory.TreasureHunt);
				break;
			case gamesettings.STAT_POINTS:
				UpdateGoal(GoalCategory.CoinHunt);
				break;
			case gamesettings.STAT_KILLS:
				UpdateGoal(GoalCategory.BoatKill);
				break;
			case gamesettings.STAT_TURRETS:
				UpdateGoal(GoalCategory.TurrelKill);
				break;
			case gamesettings.STAT_LAVA:
				UpdateGoal(GoalCategory.RockKill);
				break;
		}
	}
}

