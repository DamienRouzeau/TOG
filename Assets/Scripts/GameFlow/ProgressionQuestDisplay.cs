using UnityEngine;

public class ProgressionQuestDisplay : MonoBehaviour
{
	[SerializeField]
	private GameObject[] _blueLines = null;
	[SerializeField]
	private GameObject[] _redLines = null;
	[SerializeField]
	private GameObject[] _toDisableOnBoat = null;

	public void ShowTeam(boat_followdummy.TeamColor teamColor)
	{
		if (_blueLines != null)
		{
			foreach (GameObject go in _blueLines)
				go.SetActive(teamColor == boat_followdummy.TeamColor.Blue);
		}
		if (_redLines != null)
		{
			foreach (GameObject go in _redLines)
				go.SetActive(teamColor == boat_followdummy.TeamColor.Red);
		}
		if (_toDisableOnBoat != null)
		{
			foreach (GameObject go in _toDisableOnBoat)
				go.SetActive(false);
		}
	}
}
