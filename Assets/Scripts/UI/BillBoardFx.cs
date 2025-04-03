using UnityEngine;

public class BillBoardFx : MonoBehaviour
{
    [SerializeField]
    private bool _needToReverse = false;

    private Transform _camTransform;

    void Start()
    {
        Init();
    }

    void Update()
    {
        if (_camTransform != null)
        {
            Vector3 pos = transform.position;
            Vector3 lookPos = _camTransform.position;
            lookPos.y = pos.y;
            Vector3 lookUp = Vector3.up;
            if (_needToReverse)
                transform.LookAt(2 * pos - lookPos, lookUp);
            else
                transform.LookAt(lookPos, lookUp);
        }
        else
		{
            Init();
        }
    }

    private void Init()
	{
        _camTransform = Camera.main?.transform;
        if (_camTransform == null)
        {
            Camera cam = FindObjectOfType<Camera>();
            _camTransform = cam?.transform;
        }
	}
}