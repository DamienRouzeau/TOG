using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ColorSettings", order = 1)]
public class ColorSettings : ScriptableObject
{
	#region Enums

	public enum ColorType
	{
		HealthGauge,
		PlayerName
	}

	#endregion

	#region EnumArrays

	[Serializable]
	public class TeamColorEnumArray : RREnumArray<boat_followdummy.TeamColor, Color> { }

	[Serializable]
	public class ColorEnumArray : RREnumArray<ColorType, TeamColorEnumArray> {}

	#endregion

	#region Properties

	public ColorEnumArray colors;

	#endregion
}
