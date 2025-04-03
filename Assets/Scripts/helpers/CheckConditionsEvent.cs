using UnityEngine;
using UnityEngine.Events;

public class CheckConditionsEvent : MonoBehaviour
{
	#region Enums

	public enum GroupConditionType
	{
		AND,
		OR
	}

	#endregion

	#region Classes

	[System.Serializable]
	public class GameObjectCondition
	{
		public GameObject goToCheck = null;
		public bool enableValue = true;
		public bool activeSelfOnly = false;

		public bool CheckCondition()
		{
			if (goToCheck == null)
				return false;
			if (activeSelfOnly)
				return goToCheck.activeSelf == enableValue;
			else
				return goToCheck.activeInHierarchy == enableValue;
		}
	}

	#endregion

	[SerializeField]
	private GroupConditionType _type = GroupConditionType.AND;
	[SerializeField]
	private GameObjectCondition[] _conditions = null;
	[SerializeField]
	private UnityEvent _onConditionsValid = null;
	[SerializeField]
	private bool _autoCheck = false;
	[SerializeField]
	private float _autoCheckDelay = 1f;

	private float _checkLastTime = 0f;

	public void CheckConditions()
	{
		if (AreConditionsValid())
		{
			_onConditionsValid?.Invoke();
			if (Player.myplayer != null)
				Player.myplayer.AddProgress("CC_" + gameObject.name);
			_autoCheck = false;
		}
	}

	public bool AreConditionsValid()
	{
		if (_conditions != null && _conditions.Length > 0)
		{
			switch (_type)
			{
				case GroupConditionType.AND:
					foreach (GameObjectCondition condition in _conditions)
					{
						if (!condition.CheckCondition())
							return false;
					}
					return true;
				case GroupConditionType.OR:
					foreach (GameObjectCondition condition in _conditions)
					{
						if (condition.CheckCondition())
							return true;
					}
					return false;
			}
		}
		return false;
	}

	public void SetAutoCheckDelay(float delay)
	{
		_autoCheck = true;
		_autoCheckDelay = delay;
		_checkLastTime = 0f;
	}

	private void Update()
	{
		if (_autoCheck)
		{
			_checkLastTime += Time.deltaTime;
			if (_checkLastTime > _autoCheckDelay)
			{
				_checkLastTime -= _autoCheckDelay;
				CheckConditions();
			}
		}
	}
}
