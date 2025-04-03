using RRLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessManager : MonoBehaviour
{
    [SerializeField] boat_followdummy _boatLeft = null;
    [SerializeField] boat_followdummy _boatRight = null;
    [SerializeField] EndlessSegment _startingSegment = null;
    [SerializeField] EndlessSegment _finishSegment = null;
    [SerializeField] EndlessSegment[] _segmentPrefabs = null;
    [SerializeField] int _segmentInstanceCount = 3;
    [SerializeField] EndlessGoals _goals = null;

    private PathFollower _pathFollowerLeft = null;
    private PathFollower _pathFollowerRight = null;
    private List<EndlessSegment> _segments = null;
    private int _currentSegmentLeft = 0;
    private int _currentSegmentRight = 0;
    private bool _isGameEnded = false;

	private void Awake()
	{
        gameflowmultiplayer.CheckRandomSeed();
        Random.InitState(gameflowmultiplayer.randomSeed);
    }

	// Start is called before the first frame update
	IEnumerator Start()
    {
        // Activate Endless mode
        gameflowmultiplayer.gameMode = gameflowmultiplayer.GameMode.Endless;

        _startingSegment.Init(0);
        Vector3 pos = Vector3.forward * _startingSegment.end.z;

        _segments = new List<EndlessSegment>();
        _segments.Add(_startingSegment);

        yield return null;

        for (int i = 1; i < _segmentInstanceCount; ++i)
        {
            EndlessSegment segment = InstantiateSegment();
            segment.transform.position = Vector3.forward * pos.z;
            segment.Init(i);
            pos = segment.end;
            _segments.Add(segment);
            yield return null;
        }

        // Wait for dummy of left boat
        while (_boatLeft.dummy == null)
            yield return null;

        _pathFollowerLeft = _boatLeft.pathFollower;

        // Wait for dummy of right boat
        while (_boatRight.dummy == null)
            yield return null;

        _pathFollowerRight = _boatRight.pathFollower;

        InitCallbacks();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (gameObject.GetComponentInChildren<StartWithoutVR>() == null)
                gameObject.AddComponent<StartWithoutVR>();
        }
    }

    private void OnDestroy()
    {
        ReleaseCallbacks();
    }

    private void InitCallbacks()
    {
        _pathFollowerLeft.AddOnPathEvent(OnPathFollowerEvent);
        _pathFollowerRight.AddOnPathEvent(OnPathFollowerEvent);
        _goals.onGoalsEndGameCbk += OnGoalsEndGame;
    }

    private void ReleaseCallbacks()
    {
        _pathFollowerLeft?.RemoveOnPathEvent(OnPathFollowerEvent);
        _pathFollowerRight?.RemoveOnPathEvent(OnPathFollowerEvent);
        if (_goals != null)
            _goals.onGoalsEndGameCbk -= OnGoalsEndGame;
    }

    private void OnGoalsEndGame(EndlessGoals.ResultGame resultGame)
    {
        Debug.Log($"OnGoalsEndGame {resultGame}");
        gameflowmultiplayer.SetTeamEndGoals(resultGame);
        _isGameEnded = true;
    }

    private void ChangePathFollowerToSegment(PathFollower path, EndlessSegment segment, bool bLeft)
    {
        path.SetupNewPathHall(bLeft ? segment.pathLeft : segment.pathRight);
    }

    private EndlessSegment InstantiateSegment()
    {
        if (_isGameEnded)
        {
            return GameObject.Instantiate<EndlessSegment>(_finishSegment);
        }

        int prefabCount = _segmentPrefabs.Length;
        int prefabNum = Random.Range(0, prefabCount);
        EndlessSegment prefab = _segmentPrefabs[prefabNum];
        EndlessSegment segment = GameObject.Instantiate<EndlessSegment>(prefab);
        return segment;
    }

    private void OnPathFollowerEvent(PathFollower path, PathFollower.PathEvent pathEvent, object data)
    {
        switch (pathEvent)
        {
            case PathFollower.PathEvent.PathEnded:
                OnPathEnded(path);
                break;
        }
    }

    private EndlessSegment GetSegmentFromNumId(int numId)
    {
        foreach (EndlessSegment segment in _segments)
        {
            if (segment.numId == numId)
                return segment;
        }
        return null;
    }

    private void OnPathEnded(PathFollower path)
    {
        if (path == _pathFollowerLeft)
        {
            _currentSegmentLeft++;
            EndlessSegment segment = GetSegmentFromNumId(_currentSegmentLeft);
            if (segment != null)
                ChangePathFollowerToSegment(_pathFollowerLeft, segment, true);
        }
        if (path == _pathFollowerRight)
        {
            _currentSegmentRight++;
            EndlessSegment segment = GetSegmentFromNumId(_currentSegmentRight);
            if (segment != null)
                ChangePathFollowerToSegment(_pathFollowerRight, segment, false);
        }

        int minSegment = Mathf.Min(_currentSegmentLeft, _currentSegmentRight);
        int maxSegment = Mathf.Max(_currentSegmentLeft, _currentSegmentRight);

        EndlessSegment secondSegment = _segments[1];
        if (secondSegment.numId < minSegment - 1)
        {
            GameObject.Destroy(_segments[1].gameObject);
            _segments.RemoveAt(1);
        }
        EndlessSegment lastSegment = _segments[_segments.Count-1];
        if (lastSegment.numId < maxSegment + _segmentInstanceCount - 1)
        {
            EndlessSegment segment = InstantiateSegment();
            segment.transform.position = Vector3.forward * lastSegment.end.z;
            segment.Init(lastSegment.numId + 1);
            _segments.Add(segment);
        }
    }
}
