using AllMyScripts.Common.Version;
using System.Collections;
using TMPro;
using UnityEngine;

public class UI_CabinTeams : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _playerCountText = null;
    [SerializeField]
    private GameObject _playerLinePrefab = null;
    [SerializeField]
    private Transform[] _playerLineAnchors = null;
	[SerializeField]
	private TextMeshProUGUI _versionText = null;

	// Start is called before the first frame update
	void Start()
    {
		StartCoroutine(RefreshPlayerCountEnum(5f));

		if (_versionText != null)
			_versionText.text = $"v. {Version.GetVersionNumber()}";
	}

	public void UpdatePlayerCount()
	{
		GameflowBase.SearchAllFlows();
		int playerCount = GameflowBase.allFlows != null ? GameflowBase.allFlows.Length : 0;
		string textId = playerCount > 1 ? "str_playersnumbermore" : "str_playersnumberone";
		_playerCountText.text = $"{playerCount} {RRLib.RRLanguageManager.instance.GetString(textId)}";

		for (int i = 0; i < _playerLineAnchors.Length; ++i)
		{
			Transform trAnchor = _playerLineAnchors[i];
			if (i < playerCount)
			{
				CabinPlayerLine line = null;
				if (trAnchor.childCount == 0)
				{
					line = GameObject.Instantiate(_playerLinePrefab).GetAddComponent<CabinPlayerLine>();
					line.transform.SetParent(trAnchor);
					line.transform.localPosition = Vector3.zero;
					line.transform.localRotation = Quaternion.identity;
					line.transform.localScale = Vector3.one;
				}
				else
				{
					line = trAnchor.GetComponentInChildren<CabinPlayerLine>(true);
				}
				if (line != null)
				{
					if (GameflowBase.allFlows != null && i < GameflowBase.allFlows.Length)
					{
						line.gameObject.SetActive(true);
						int num = GameflowBase.allFlows[i].actorNum;
						line.SetPlayerName(GameflowBase.piratenames[num]);
						bool ready = GameflowBase.allFlows[i].nameValidated;
						line.SetReady(ready);
					}
					else
					{
						line.gameObject.SetActive(false);
					}
				}
			}
			else
			{
				while (trAnchor.childCount > 0)
				{
					GameObject.DestroyImmediate(trAnchor.GetChild(0).gameObject);
				}
			}
		}
	}

	private IEnumerator RefreshPlayerCountEnum(float delay)
	{
		while (true)
		{
			yield return new WaitForSeconds(delay);
			UpdatePlayerCount();
		}
	}
}
