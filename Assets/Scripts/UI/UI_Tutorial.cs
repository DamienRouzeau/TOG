using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

public class UI_Tutorial : MonoBehaviour
{
    public static UI_Tutorial myself = null;
    public enum TutoCondition
	{
        Teleport,
        TeleportOnPoint,
        TeleportOnMast,
        Musket,
        Mortar,
        Canon,
        Gold,
        Chest,
        Rope,
        MoveToGuide,
        None,
        PlayerOnBoat,
        WaitVoiceOver,
        Skull,
        Turret,
        BoatSunken,
        BoatFullPumped,
        FirstFinishLine,
        WaitGameObjectActivation,
        WeaponOnHand,
        Delay
    }

    public enum TutoEntry
    {
        None,
        NoWeapons,
        MusketOnBelt,
        MusketOnHands,
        MortarOnHands,
        StopBoat,
        StartBoat,
        ResetSkulls,
        StopTeleport,
        StartTeleport,
        ForcedTeleport,
        StopAIBoatToShoot,
        StartAIBoatToShoot,
        StopBoatRegeneration,
        StartBoatRegeneration
    }

    [System.Serializable]
    public class TutoStep
	{
        public int numStep = 0;
        public int maxStep = 0;
        public TutoCondition tutoCondition;        
        public string titleTextId = null;
        public string contentTextId = null;
        public string rewardTextId = null;
        public Sprite image = null;
        public Transform guideRoot = null;
        public GameObject targetObject;
        public float delay = 0f;
        public TutoEntry tutoEntry;
        public UnityEvent startEvent;
        public UnityEvent endEvent;
    }

    public TutoStep currentStep => _tutoSteps[_currentStep];


    [SerializeField]
    private List<TutoStep> _tutoSteps = null;
    [SerializeField]
    private TextMeshProUGUI _numText = null;
    [SerializeField]
    private TextMeshProUGUI _maxText = null;
    [SerializeField]
    private TextMeshProUGUI _titleText = null;
    [SerializeField]
    private TextMeshProUGUI _descText = null;
    [SerializeField]
    private Image _descImage = null;
    [SerializeField]
    private Transform _teleportGuide = null;

    private int _currentStep = 0;
    //private boat_followdummy _boat = null;
    private float _startTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        myself = this;
        ApplyCurrentStep();
    }

	private void Update()
	{
		if (_currentStep < _tutoSteps.Count)
		{
            TutoStep step = currentStep;
            if (step.tutoCondition == TutoCondition.MoveToGuide)
			{
                Vector3 playerPos = Player.myplayer.GetFootPos();
                Vector3 diff = _teleportGuide.position - playerPos;
                float sqrDist = diff.sqrMagnitude;
                float distMax = 0.3f;
                if (sqrDist < distMax * distMax)
				{
                    NextStep();
				}
            }
            else if (step.tutoCondition == TutoCondition.WaitGameObjectActivation)
			{
                if (step.targetObject != null && step.targetObject.activeSelf)
				{
                    NextStep();
                }
			}
            else if (step.tutoCondition == TutoCondition.Delay)
			{
                if (Time.time - _startTimer > step.delay)
				{
                    _startTimer = 0f;
                    NextStep();
				}
			}
		}
	}

	public void ResetStep()
    {
        _currentStep = 0;
        ApplyCurrentStep();
    }

    public void NextStep()
	{
        if (currentStep != null)
            currentStep.endEvent?.Invoke();
        if (_currentStep < _tutoSteps.Count - 1)
        {
            _currentStep++;
            ApplyCurrentStep();
        }
    }

    public void ApplyCurrentStep()
	{
        ApplyStep(currentStep);
    }

    private void ApplyStep(TutoStep step)
    {
        _numText.text = step.numStep.ToString();
        _maxText.text = "/" + step.maxStep.ToString();
        if (!string.IsNullOrEmpty(step.titleTextId))
            _titleText.text = RRLib.RRLanguageManager.instance.GetString(step.titleTextId);
        if (!string.IsNullOrEmpty(step.contentTextId))
            _descText.text = RRLib.RRLanguageManager.instance.GetString(step.contentTextId);
        _descImage.sprite = step.image;
        _descImage.gameObject.SetActive(step.image != null);
        if (step.guideRoot != null)
		{
            _teleportGuide.position = step.guideRoot.position;
            _teleportGuide.gameObject.SetActive(true);
        }
        else
		{
            _teleportGuide.gameObject.SetActive(false);
        }
        if (step.tutoCondition == TutoCondition.Delay)
		{
            _startTimer = Time.time;
        }
        Player.myplayer.SetForcedTeleportTarget(null);
        switch (step.tutoEntry)
        {
            case TutoEntry.NoWeapons:
                Player.myplayer.SetWeaponInPlace(Player.WeaponType.None, Player.WeaponPlace.LeftHand);
                Player.myplayer.SetWeaponInPlace(Player.WeaponType.None, Player.WeaponPlace.RightHand);
                Player.myplayer.SetWeaponInPlace(Player.WeaponType.None, Player.WeaponPlace.LeftBelt);
                Player.myplayer.SetWeaponInPlace(Player.WeaponType.None, Player.WeaponPlace.RightBelt);
                break;
            case TutoEntry.MusketOnBelt:
                Player.myplayer.SetWeaponInPlace(Player.WeaponType.None, Player.WeaponPlace.LeftHand);
                Player.myplayer.SetWeaponInPlace(Player.WeaponType.None, Player.WeaponPlace.RightHand);
                Player.myplayer.SetWeaponInPlace(Player.WeaponType.Musket, Player.WeaponPlace.LeftBelt);
                Player.myplayer.SetWeaponInPlace(Player.WeaponType.Musket, Player.WeaponPlace.RightBelt);
                break;
            case TutoEntry.MusketOnHands:
                Player.myplayer.SetWeaponInPlace(Player.WeaponType.Musket, Player.WeaponPlace.LeftHand);
                Player.myplayer.SetWeaponInPlace(Player.WeaponType.Musket, Player.WeaponPlace.RightHand);
                Player.myplayer.SetWeaponInPlace(Player.WeaponType.None, Player.WeaponPlace.LeftBelt);
                Player.myplayer.SetWeaponInPlace(Player.WeaponType.None, Player.WeaponPlace.RightBelt);
                break;
            case TutoEntry.MortarOnHands:
                Player.myplayer.SetWeaponInPlace(Player.WeaponType.TOG_Biggun, Player.WeaponPlace.LeftHand);
                Player.myplayer.SetWeaponInPlace(Player.WeaponType.TOG_Biggun, Player.WeaponPlace.RightHand);
                Player.myplayer.SetWeaponInPlace(Player.WeaponType.None, Player.WeaponPlace.LeftBelt);
                Player.myplayer.SetWeaponInPlace(Player.WeaponType.None, Player.WeaponPlace.RightBelt);
                break;
            case TutoEntry.StopBoat:
                gameflowmultiplayer.GetBoat(0)?.pathFollower?.SetInPause(true);
                gameflowmultiplayer.GetBoat(1)?.pathFollower?.SetInPause(true);
                break;
            case TutoEntry.StartBoat:
                gameflowmultiplayer.GetBoat(0)?.pathFollower?.SetInPause(false);
                gameflowmultiplayer.GetBoat(1)?.pathFollower?.SetInPause(false);
                break;
            case TutoEntry.ResetSkulls:
                string levelId = gamesettings_general.myself.levelSettings.GetLevelIdFromListIndex(gameflowmultiplayer.levelListId, gameflowmultiplayer.levelIndex);
                Debug.Assert(levelId != null);
                SaveManager.ProgressionLevelData level = SaveManager.myself.profile.progression.GetLevelFromId(levelId);
                Debug.Assert(level != null);
                level.ResetSkulls();
                break;
            case TutoEntry.StopTeleport:
                Player.myplayer.EnableTeleport(false);
                break;
            case TutoEntry.StartTeleport:
                Player.myplayer.EnableTeleport(true);
                break;
            case TutoEntry.ForcedTeleport:
                Player.myplayer.EnableTeleport(true);
                Player.myplayer.SetForcedTeleportTarget(step.guideRoot);
                break;
            case TutoEntry.StopAIBoatToShoot:
                gameflowmultiplayer.GetAIBoat()?.AllowToShoot(false);
                break;
            case TutoEntry.StartAIBoatToShoot:
                gameflowmultiplayer.GetAIBoat()?.AllowToShoot(true);
                break;
            case TutoEntry.StopBoatRegeneration:
                gameflowmultiplayer.GetBoat(0)?.SetRegenerateHealth(false);
                gameflowmultiplayer.GetBoat(1)?.SetRegenerateHealth(false);
                break;
            case TutoEntry.StartBoatRegeneration:
                gameflowmultiplayer.GetBoat(0)?.SetRegenerateHealth(true);
                gameflowmultiplayer.GetBoat(1)?.SetRegenerateHealth(true);
                break;
        }
        step.startEvent?.Invoke();
        if (step.tutoCondition == TutoCondition.None)
            NextStep();
    }

    public void OnTriggerCondition(TutoCondition condition)
	{
        if (_currentStep < _tutoSteps.Count)
        {
            TutoStep step = currentStep;
            if (step.tutoCondition == condition)
            {
                NextStep();
            }
        }
    }
}
