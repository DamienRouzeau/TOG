//#define DEBUG_RACE_EVENTS

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[ExecuteAlways]
public class RaceManager : MonoBehaviour
{
    #region Classes

    [System.Serializable]
    public class PathsData
    {
        public PathHall[] pathArray = null;
        public int currentSplineIdx = 0;

        public PathHall NextPath()
        {
            int count = pathArray?.Length ?? 0;
            if (count == 0)
                return null;
            if (count == 1)
                return pathArray[0];
            return SetCurrentPath( (currentSplineIdx + 1) % count);
        }

        public PathHall FirstPath()
        {
            int count = pathArray?.Length ?? 0;
            if (count == 0)
                return null;
            if (count >= 1)
                return pathArray[0];
            return null;
        }

        public PathHall SetCurrentPath(int pathIdx)
        {
            if (pathIdx >= 0 && pathIdx < pathArray.Length)
            {
                currentSplineIdx = pathIdx;
                pathArray[currentSplineIdx].Init();
                return pathArray[currentSplineIdx];
            }
            return null;
        }
    }

    [System.Serializable]
    public class AiTargetData
    {
        public List<Transform> targets = null;
    }

    #endregion

    #region EnumArrays

    [System.Serializable]
    public class BoatEnumArray : RREnumArray<boat_followdummy.TeamColor, boat_followdummy> { }

    [System.Serializable]
    public class PathDataEnumArray : RREnumArray<boat_followdummy.TeamColor, PathsData> { }

    [System.Serializable]
    public class TransformForEndRaceEnumArray : RREnumArray<boat_followdummy.TeamColor, Transform> { }

    [System.Serializable]
    public class TargetsForAIEnumArray : RREnumArray<boat_followdummy.TeamColor, AiTargetData> { }

    [System.Serializable]
    public class QuestDisplayEnumArray : RREnumArray<boat_followdummy.TeamColor, Transform> { }

    #endregion

    #region Properties

    public static RaceManager myself = null;

    public BoatEnumArray boats => _boats;
    public Animator[] doorAnimators => _doorAnimators;

    public AudioSource gamemusic => _gamemusic;

    public GameObject boat01voiceobjects => _boat01voiceobjects;
    public GameObject boat02voiceobjects => _boat02voiceobjects;

    public GameObject worldRoot => _worldRoot;

    public int totalGold => _totalGold;

    public TargetsForAIEnumArray aiTargets => _aiTargets;

    public UnityEvent finalOnEndRace => _finalOnEndRace;
    public float finalDelayToShowEndRaceScreen => _finalDelayToShowEndRaceScreen;
    public GameObject finalObjectCondition => _finalObjectCondition;

    [SerializeField] private BoatEnumArray _boats = null;
    [SerializeField] private PathDataEnumArray _paths = null;
    [SerializeField] private Animator[] _doorAnimators = null;
    [SerializeField] private AudioSource _gamemusic = null;
    [SerializeField] private GameObject _boat01voiceobjects = null;
    [SerializeField] private GameObject _boat02voiceobjects = null;
    [SerializeField] private GameObject _worldRoot = null;
    [SerializeField] private TransformForEndRaceEnumArray _trForEndRace = null;
    [SerializeField] private TargetsForAIEnumArray _aiTargets = null;
    [SerializeField] private UnityEvent _onFinishLineLost = null;
    [SerializeField] private UnityEvent _onFinishLineBlueTeamWon = null;
    [SerializeField] private UnityEvent _onFinishLineRedTeamWon = null;

    [Header("EndRace Final")]
    [SerializeField] private UnityEvent _finalOnEndRace = null;
    [SerializeField] private float _finalDelayToShowEndRaceScreen = 0f;
    [SerializeField] private GameObject _finalObjectCondition = null;

    [Header("Quests")]
    [SerializeField] ProgressionQuestDisplay _questDisplayPrefab = null;
    [SerializeField] QuestDisplayEnumArray _questDisplayAnchors = null;
    private ProgressionQuestDisplay _questDisplayInstance = null;

    [Header("Debug")]
    [Range(0f,1f)]
    public float simulatePosition = 0f;
    [Range(0f,2.6f)]
    public float simulateSinking = 0f;
    public bool enableSimulation = false;
    public bool startAtSimulation = false;
    private SplineController _splineBlue = null;
    private SplineController _splineRed = null;
    private GameObject _simulateFollow = null;

    [Header("End Screen TEST")]
    [SerializeField] private bool _testEndScreen = false;
    [SerializeField] private int _testPlayerCountTeamA = 8;
    [SerializeField] private int _testPlayerCountTeamB = 8;

    private int _totalGold = 0;

    #endregion

    private void Awake()
    {
        myself = this;
        GameflowBase.CheckRandomSeed();
        Health.ResetInstanceCount();
        Random.InitState(GameflowBase.randomSeed);
    }

    protected void Start()
    {
        GameflowBase.onRaceEventDelegate += OnRaceEvent;
    }

    private void OnDestroy()
    {
        GameflowBase.onRaceEventDelegate -= OnRaceEvent;
        _boats = null;
        _paths = null;
        Health.ResetInstanceCount();
        myself = null;
    }

    protected void Update()
	{
#if UNITY_EDITOR
        if (enableSimulation)
        {
            if (_splineBlue == null)
                _splineBlue = _paths[boat_followdummy.TeamColor.Blue].FirstPath().GetSpline(0);
            Vector3 v3Pos = _splineBlue.GetPositionAtTimeForEditor(simulatePosition);
            Quaternion qRot = _splineBlue.GetRotationAtTimeForEditor(simulatePosition) * Quaternion.Euler(0, -90f, 0);
            boats[boat_followdummy.TeamColor.Blue].transform.position = v3Pos;
            boats[boat_followdummy.TeamColor.Blue].transform.rotation = qRot;
            boats[boat_followdummy.TeamColor.Blue].transform.position -= boats[boat_followdummy.TeamColor.Blue].transform.up * simulateSinking;

            if (_splineRed == null)
                _splineRed = _paths[boat_followdummy.TeamColor.Red].FirstPath().GetSpline(0);
            v3Pos = _splineRed.GetPositionAtTimeForEditor(simulatePosition);
            qRot = _splineRed.GetRotationAtTimeForEditor(simulatePosition) * Quaternion.Euler(0, -90f, 0);
            boats[boat_followdummy.TeamColor.Red].transform.position = v3Pos;
            boats[boat_followdummy.TeamColor.Red].transform.rotation = qRot;
            boats[boat_followdummy.TeamColor.Red].transform.position -= boats[boat_followdummy.TeamColor.Red].transform.up * simulateSinking;

            if (_simulateFollow == null)
            {
                Transform tr = transform.Find("FollowBoats");
                if (tr != null)
                    _simulateFollow = tr.gameObject;
                if (_simulateFollow == null)
                {
                    _simulateFollow = new GameObject("FollowBoats");
                    _simulateFollow.transform.SetParent(transform);
                }
            }
            _simulateFollow.transform.position = (boats[boat_followdummy.TeamColor.Blue].transform.position + boats[boat_followdummy.TeamColor.Red].transform.position) * 0.5f;
        }
        if (_testEndScreen && gameflowmultiplayer.myself != null && gameflowmultiplayer.myself.gameStarted && !gameflowmultiplayer.gameplayEndRace)
        {
            for (int i = 0; i < _testPlayerCountTeamA; ++i)
                GameflowBase.teamlistA[i] = GameflowBase.myId;
            for (int i = _testPlayerCountTeamA; i < GameflowBase.nrplayersperteam; ++i)
                GameflowBase.teamlistA[i] = -1;
            for (int i = 0; i < _testPlayerCountTeamB; ++i)
                GameflowBase.teamlistB[i] = GameflowBase.myId;
            for (int i = _testPlayerCountTeamB; i < GameflowBase.nrplayersperteam; ++i)
                GameflowBase.teamlistB[i] = -1;
            gameflowmultiplayer.myself.gameState = gameflowmultiplayer.GameState.EndGame;
            gameflowmultiplayer.gameplayEndRace = true;
        }
#endif
    }

    public IEnumerator MoveBoatsAtEndOfRace()
	{
        if (_trForEndRace[boat_followdummy.TeamColor.Blue] != null && _trForEndRace[boat_followdummy.TeamColor.Red] != null)
        {
            Player.myplayer.EnableTeleport(false);

            gamesettings_screen.myself.FadeOut(0.5f);
            while (gamesettings_screen.myself.faderunning)
                yield return null;

            boats[boat_followdummy.TeamColor.Blue].dummy = null;
            boats[boat_followdummy.TeamColor.Blue].transform.position = _trForEndRace[boat_followdummy.TeamColor.Blue].position;
            boats[boat_followdummy.TeamColor.Blue].transform.rotation = _trForEndRace[boat_followdummy.TeamColor.Blue].rotation;

            boats[boat_followdummy.TeamColor.Red].dummy = null;
            boats[boat_followdummy.TeamColor.Red].transform.position = _trForEndRace[boat_followdummy.TeamColor.Red].position;
            boats[boat_followdummy.TeamColor.Red].transform.rotation = _trForEndRace[boat_followdummy.TeamColor.Red].rotation;

            yield return new WaitForSeconds(0.2f);

            Transform endPos = boats[(boat_followdummy.TeamColor)Player.myplayer.team].boatResultScreen?.GetTargetForPlayerId(GameflowBase.myId);
            if (endPos != null)
			{
                Player.myplayer.Teleport(endPos.position, endPos.rotation);
                yield return new WaitForSeconds(1f);
            }

            gamesettings_screen.myself.FadeIn();
            while (gamesettings_screen.myself.faderunning)
                yield return null;

            Player.myplayer.EnableTeleport(true);
        }
    }

    public void TriggerFinishLine(bool win, int team)
	{
        if (win)
        {
            if (team == 0)
            {
                _onFinishLineBlueTeamWon?.Invoke();
                boat_finishFeedback fbk = _boats[boat_followdummy.TeamColor.Blue].boatFinishFeedback;
                fbk?.LaunchFeedback(boat_finishFeedback.FeedbackType.BlueWon);
            }
            else
            {
                _onFinishLineRedTeamWon?.Invoke();
                boat_finishFeedback fbk = _boats[boat_followdummy.TeamColor.Red].boatFinishFeedback;
                fbk?.LaunchFeedback(boat_finishFeedback.FeedbackType.RedWon);
            }
            if (UI_Tutorial.myself != null)
                UI_Tutorial.myself.OnTriggerCondition(UI_Tutorial.TutoCondition.FirstFinishLine);
        }
        else
        {
            _onFinishLineLost?.Invoke();
            boat_finishFeedback fbk = _boats[1 - team].boatFinishFeedback;
            fbk?.LaunchFeedback(boat_finishFeedback.FeedbackType.Lost);
        }
    }

    public IEnumerator ComputeGoldEnum()
	{
        if (gamesettings.myself.useCustomTotalGoldInRace)
		{
            _totalGold = gamesettings.myself.customTotalGoldInRace;
            yield break;
        }

        _totalGold = 0;
        //string text = "";
        int n = 0;
        foreach (Health h in gameObject.GetComponentsInChildren<Health>(true))
        {
            if (h.isTreasureType)
            {
                //text += $"RaceManager - ComputeGold - {boat_followdummy.GetPath(h.transform)} {h.deathGain} {h.healtObjectType} {h.currentHealth}\n";
                _totalGold += h.deathGain;
            }
            n++;
            if (n % 100 == 0)
                yield return null;
        }
        //Debug.Log(text);
    }

    private void OnRaceEvent(int team, bool isMe, string statId, object param)
    {
#if DEBUG_RACE_EVENTS
        Debug.Log($"OnRaceEvent team {team} - {statId} - param {param}");
#endif
        if (team < 0)
        {
            Debug.LogWarning($"OnRaceEvent team {team} is not valid for {statId} & param {param}");
            return;
        }

        switch (statId)
        {
            case gamesettings.STAT_SPLINES:
            {
                int pathIdx = (int)param;
                boat_followdummy.TeamColor teamColor = (boat_followdummy.TeamColor)team;
                PathHall path = _paths[teamColor].SetCurrentPath(pathIdx);
                if (path != null)
                    boats[teamColor].path = path;
            }
            break;
            case gamesettings.STAT_POINTS:
			{
                boat_followdummy.TeamColor teamColor = (boat_followdummy.TeamColor)team;
                int gold = (int)((double)param);
                boats[teamColor].AddGold(gold);
            }
            break;
        }
    }

    public void ShowQuestDisplay(boat_followdummy.TeamColor teamColor)
    {
        if (_questDisplayPrefab != null)
        {
            if (PhotonNetworkController.soloMode)
            {
                Transform anchor = _questDisplayAnchors[teamColor];
                if (anchor != null)
                {
                    _questDisplayInstance = GameObject.Instantiate<ProgressionQuestDisplay>(_questDisplayPrefab, anchor);
                    _questDisplayInstance.ShowTeam(teamColor);
                }
            }
        }
    }

    public void UpdateQuestDisplay()
	{
        if (_questDisplayInstance != null)
		{
            if (PhotonNetworkController.soloMode)
            {
                foreach (ProgressionValidator line in _questDisplayInstance.GetComponentsInChildren<ProgressionValidator>())
                    line.UpdateDisplay();
            }
        }
	}
}
