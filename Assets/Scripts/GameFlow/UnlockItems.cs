using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnlockItems : MonoBehaviour
{
	public enum UnlockCondition
	{
		Dead,
		DeadCount,
		HealthPercent,
		HealthValue,
		Enable,
		Disable,
		Team,
		FinishFirst,
		SkullCount,
		GoldCount
	}


	[System.Serializable]
	public class UnlockParams
	{
		public UnlockCondition condition = UnlockCondition.Dead;
		public Health health = null;
		public List<Health> healths = null;
		public GameObject target = null;
		public float param = 0f;
		public int numTeam = 0;

		public bool CheckCondition()
		{
			switch (condition)
			{
				case UnlockCondition.Dead:
					return health == null || health.dead;
				case UnlockCondition.DeadCount:
					return GetDeadCount() >= (int)param;
				case UnlockCondition.HealthValue:
					return health != null && health.currentHealth <= param;
				case UnlockCondition.HealthPercent:
					return health != null && health.currentHealth <= param * health.maxHealth * 0.01f;
				case UnlockCondition.Enable:
					return target != null && target.activeSelf;
				case UnlockCondition.Disable:
					return target != null && !target.activeSelf;
				case UnlockCondition.Team:
					return Player.myplayer.team == numTeam;
				case UnlockCondition.FinishFirst:
					return gameflowmultiplayer.finishTeams[0] == Player.myplayer.team;
				case UnlockCondition.SkullCount:
					return SaveManager.myself.profile.progression.GetUnlockedSkulls() >= param;
				case UnlockCondition.GoldCount:
					return gameflowmultiplayer.GetBoat(Player.myplayer.team).wonGold >= param;
				default:
					return false;
			}
		}

		private int GetDeadCount()
		{
			int count = 0;
			if (healths != null)
			{	
				foreach (Health h in healths)
				{
					if (h == null || h.dead)
						count++;
				}
			}
			return count;
		}
	}


	[SerializeField]
	private UnlockParams[] _conditions = null;
	[SerializeField]
	private UnityEvent _soloAction = null;
	[SerializeField]
	private UnityEvent _multiAction = null;
	[SerializeField]
	private bool _simulateTrigger = false;
	[SerializeField]
	private float _delayToCheckCondition = 1f;

	private bool _triggered = false;

	protected void OnEnable()
	{
		_triggered = false;
		StartCoroutine(CheckConditionsEnum(_delayToCheckCondition > 0f ? _delayToCheckCondition : 0.01f));
	}

	private IEnumerator CheckConditionsEnum(float delay)
	{
		if (_conditions == null)
			yield break;
		if (_conditions.Length == 0)
			yield break;
		if (_soloAction == null && _multiAction == null)
			yield break;

		while (!_triggered)
		{
			yield return new WaitForSeconds(delay);
			if (CheckConditions())
			{
				TriggerAction();
				_triggered = true;
			}
		}
	}

	private bool CheckConditions()
	{
		if (_simulateTrigger)
		{
			_simulateTrigger = false;
			return true;
		}
		foreach (UnlockParams condition in _conditions)
		{
			if (!condition.CheckCondition())
				return false;
		}
		return true;
	}

	private void TriggerAction()
	{
		if (gameflowmultiplayer.allFlows.Length < 2)
		{
			if (_soloAction != null)
				_soloAction.Invoke();
		}
		else
		{
			if (_multiAction != null)
				_multiAction.Invoke();
		}
	}
}
