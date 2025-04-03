using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RRLib;
using UnityEngine.Events;

public class UI_PlayerGoals : MonoBehaviour
{
    [System.Serializable]
    public class PlayerGoal
	{
        public string name;
        public Sprite icon;
        public string titleId;
        public int counterMax;
	}

    [Header("Goal UI")]
    [SerializeField]
    private RRLocalizedTextMP _goalTitle = null;
    [SerializeField]
    private Image _goalIcon = null;
    [SerializeField]
    private TextMeshProUGUI _goalCounter = null;
    [SerializeField]
    private PlayerGoal[] _goals = null;
    [SerializeField]
    private UnityEvent _onGoalReached = null;
    [Header("Goal Anim")]
    [SerializeField]
    private RRLocalizedTextMP _goalAnimTitle = null;
    [SerializeField]
    private Image _goalAnimIcon = null;
    [SerializeField]
    private TextMeshProUGUI _goalAnimCounter = null; 

    private int _counterMax = 1;
    private int _counter = 0;

    public void SetTitle(string titleId)
	{
        _goalTitle.SetTextId(titleId);
        _goalAnimTitle.SetTextId(titleId);
    }

    public void SetIcon(Sprite sprite)
	{
        _goalIcon.sprite = sprite;
        _goalAnimIcon.sprite = sprite;
    }

    public void SetCounter(int counter, int max)
    {
        _goalCounter.text = $"{counter}/{max}";
        _goalAnimCounter.text = $"{counter}/{max}";
    }

    public void SetCounter(string counter)
	{
        _goalCounter.text = counter;
        _goalAnimCounter.text = counter;
    }

    public void SetGoalFromName(string name)
	{
        foreach(PlayerGoal goal in _goals)
		{
            if (goal.name == name)
			{
                SetTitle(goal.titleId);
                SetIcon(goal.icon);
                _counter = 0;
                _counterMax = goal.counterMax;
                SetCounter(_counter, _counterMax);
            }
		}
	}

    public void UpdateCounterMaxFromEnnemies()
	{
        _counterMax = 0;
        if (TowerDefManager.myself != null)
            _counterMax = TowerDefManager.myself.ComputeEnemies();
        SetCounter(_counter, _counterMax);
    }

    public void UpdateGoalCounter()
    {
        _counter++;
        Debug.Log("[GOAL] UpdateGoalCounter " + _counter);
        SetCounter(_counter, _counterMax);
        _onGoalReached?.Invoke();
    }

    public void IncrementGoalCounterMax()
    {
        _counterMax++;
        Debug.Log("[GOAL] IncrementGoalCounterMax " + _counterMax);
        SetCounter(_counter, _counterMax);
    }
}
