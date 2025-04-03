using UnityEngine;

public class EndlessSegment : MonoBehaviour
{
    public Vector3 vector
    {
        get
        {
            return end - start;
        }
    }

    public Vector3 start
    {
        get
        {
            return (_pathHallLeft.GetPositionAtTime(0, 0) + _pathHallRight.GetPositionAtTime(0, 0)) * 0.5f;
        }
    }

    public Vector3 end
    {
        get
        {
            return (_pathHallLeft.GetPositionAtTime(1, 0) + _pathHallRight.GetPositionAtTime(1, 0)) * 0.5f;
        }
    }

    public PathHall pathLeft => _pathHallLeft;
    public PathHall pathRight => _pathHallRight;

    public int numId => _numId;

    [SerializeField] PathHall _pathHallLeft = null;
    [SerializeField] PathHall _pathHallRight = null;

    private int _numId = 0;

    public void Init(int numId)
    {
        _pathHallLeft.Init();
        _pathHallRight.Init();
        _numId = numId;
    }
}
