using UnityEngine;
using TMPro;
using AllMyScripts.Common.Version;

public class UI_ShowVersion : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _versionLabel = null;

	private void Start()
	{
		string version = "v" + Version.GetVersionNumber();
		Debug.Log("[VERSION] " + version);
		_versionLabel.text = version;
	}
}
