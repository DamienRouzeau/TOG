using System.Collections.Generic;
using UnityEngine;

public class ChoiceButtons : MonoBehaviour
{
	public delegate void OnSetChoice(ChoiceType type, object data);

    #region Enums

    public enum ChoiceType
	{
        Cabin,
        EndRace,
		TeamSelection,
		TeamValidation
	}

	#endregion

	#region Properties

	public ChoiceType choiceType;
	[Header("Cabin params")]
	public string levelListId = null;
	public int levelIndex = 0;
	[Header("EndRace params")]
	public gamesettings.EndButtons endChoice;
	[Header("TeamSelection params")]
	public boat_followdummy.TeamColor team;
	[Header("TeamValidation params")]
	public bool validate;
	public bool chooseThis;

	public static OnSetChoice onSetChoiceCallback = null;

	#endregion

	public static void SetCabinLevelChoice(string levelListId, int levelIndex)
	{
		Dictionary<string, object> dic = new Dictionary<string, object>();
		dic["list"] = levelListId;
		dic["index"] = levelIndex;
		if (onSetChoiceCallback != null)
			onSetChoiceCallback(ChoiceType.Cabin, dic);
	}

    private void Update()
    {
        if(chooseThis)
        {
			SetChoice();
			chooseThis = false;
        }
    }

    public void SetChoice()
	{
		Debug.Log($"[CHOICE_BUTTONS] SetChoice  {choiceType} ");
		object data = null;
		switch (choiceType)
		{
			case ChoiceType.Cabin:
				Dictionary<string, object> dic = new Dictionary<string, object>();
				dic["list"] = levelListId;
				dic["index"] = levelIndex;
				data = dic;
				break;
			case ChoiceType.EndRace:
				data = endChoice;
				break;
			case ChoiceType.TeamSelection:
				data = team;
				break;
			case ChoiceType.TeamValidation:
				data = validate ? 1 : 0;
				break;
		}
		if (onSetChoiceCallback != null)
			onSetChoiceCallback(choiceType, data);
	}
}
