using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RRLib;

public class UI_CabinLevel : MonoBehaviour
{
    [SerializeField]
    private Animator _animator = null;
	[SerializeField]
	private GameObject _skullsRoot = null;
	[SerializeField]
	private Image[] _skullImages = null;
	[SerializeField]
	private GameObject _locked = null;
	[SerializeField]
	private GameObject _bestScoreRoot = null;
	[SerializeField]
	private TextMeshProUGUI _bestScoreText = null;
	[SerializeField]
	private TextMeshProUGUI _goalTitle = null;
	[SerializeField]
	private RRLocalizedTextMP _previousRaceName = null;
	[SerializeField]
	private TextMeshProUGUI _goalToUnlock = null;
	[SerializeField]
	private GameObject _headerRoot = null;
	[SerializeField]
	private RRLocalizedTextMP _headerText = null;
	[SerializeField]
	private GameObject _justUnlockedRoot = null;

	private void Awake()
	{
#if USE_STANDALONE
		_animator.enabled = false;
#endif
	}

	public void ShowSkulls(bool show)
	{
		_skullsRoot.SetActive(show);
	}

	public void SetSkullDisplay(int idx, bool display)
	{
		if (idx < _skullImages.Length)
			_skullImages[idx].color = display ? Color.white : Color.black;
	}

	public void SetLocked(bool locked, bool showGoal = true)
	{
		_locked.SetActive(locked);
		_previousRaceName.gameObject.SetActive(showGoal);
		_goalToUnlock.gameObject.SetActive(showGoal);
		_goalTitle.gameObject.SetActive(showGoal);
	}

	public void SetPreviousRaceName(string textId)
	{
		_previousRaceName.SetTextId(textId);
	}

	public void SetGoalToUnlock(string goal)
	{
		_goalToUnlock.text = goal;
	}

	public void ShowBestScore(int score)
	{
		_bestScoreRoot.SetActive(score > 0);
		_bestScoreText.text = score.ToString();
	}

	public void ShowHeader(bool show, string textId = null)
	{
		_headerRoot.SetActive(show);
		if (textId != null)
			_headerText.SetTextId(textId);
	}

	public void SetJustUnlockedLevel(bool unlocked)
	{
		_justUnlockedRoot.SetActive(unlocked);
	}
}
