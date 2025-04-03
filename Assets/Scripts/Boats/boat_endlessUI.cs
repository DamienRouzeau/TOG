using UnityEngine;
using TMPro;

public class boat_endlessUI : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _timeText = null;
    [SerializeField]
    TextMeshProUGUI _bluePositionText = null;
    [SerializeField]
    TextMeshProUGUI _redPositionText = null;
    [SerializeField]
    TextMeshProUGUI _blueDistanceText = null;
    [SerializeField]
    TextMeshProUGUI _redDistanceText = null;

    private float _startTime = 0f;
    private int _timer = 0;
    private int _currentTime = 0;
    private float _distanceMax = 0f;
    private string _first = "1st";
    private string _second = "2nd";

	private void Start()
	{
		// TODO define first & second localized texts
	}

	private void Update()
	{
		if (_startTime > 0f)
		{
            int currentTime = Mathf.FloorToInt(Time.time - _startTime);
            if (_currentTime != currentTime)
			{
                SetTime(_timer - currentTime);
                _currentTime = currentTime;
                if (currentTime >= _timer)
                {
                    _startTime = 0f;
                }
            }
		}
	}

	public void StartTimer(int timeInSecond)
	{
        Debug.Assert(timeInSecond > 0, "Bad time in StartTimer!");
        _startTime = Time.time;
        _timer = timeInSecond;
        _currentTime = 0;
        SetTime(timeInSecond);
    }

    public void SetDistanceMax(float distance)
	{
        _distanceMax = distance;
    }

    public void SetDistancesInSplineRatio(float blueInSplineRatio, float redInSplineRatio)
	{
        SetDistancesInMeter(_distanceMax * blueInSplineRatio, _distanceMax * redInSplineRatio);
    }

    public void SetDistancesInMeter(float blueInMeter, float redInMeter)
	{
        _blueDistanceText.text = $"{(int)blueInMeter}m";
        _redDistanceText.text = $"{(int)redInMeter}m";
        bool isBlueFirst = blueInMeter > redInMeter;
        _bluePositionText.text = isBlueFirst ? _first : _second;
        _redPositionText.text = isBlueFirst ? _second : _first;
    }

    private void SetTime(int timeInSecond)
	{
        int minutes = timeInSecond / 60;
        int seconds = timeInSecond % 60;
        _timeText.text = $"{(minutes.ToString("00"))}'{(seconds.ToString("00"))}''";
    }
}
