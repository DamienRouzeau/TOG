using UnityEngine;

public class boat_finishFeedback : MonoBehaviour
{
    public enum FeedbackType
	{
        BlueWon,
        RedWon,
        Lost
	}

    [SerializeField]
    private Transform _feedbackRoot = null;
    [SerializeField]
    private GameObject _feedbackBlueWonPrefab = null;
    [SerializeField]
    private GameObject _feedbackRedWonPrefab = null;
    [SerializeField]
    private GameObject _feedbackLostPrefab = null;

    public void LaunchFeedback(FeedbackType type)
	{
        GameObject prefab = null;
        switch (type)
        {
            case FeedbackType.BlueWon:
                prefab = _feedbackBlueWonPrefab;
                break;
            case FeedbackType.RedWon:
                prefab = _feedbackRedWonPrefab;
                break;
            case FeedbackType.Lost:
                prefab = _feedbackLostPrefab;
                break;
        }
        if (prefab != null)
		{
            GameObject go = GameObject.Instantiate(prefab, _feedbackRoot);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            go.SetActive(true);

        }
	}

#if UNITY_EDITOR
    [ContextMenu("LaunchBlueWonFeedback")]
    public void LaunchBlueWonFeedback()
	{
        LaunchFeedback(FeedbackType.BlueWon);
    }

    [ContextMenu("LaunchRedWonFeedback")]
    public void LaunchRedWonFeedback()
    {
        LaunchFeedback(FeedbackType.RedWon);
    }

    [ContextMenu("LaunchLostFeedback")]
    public void LaunchLostFeedback()
    {
        LaunchFeedback(FeedbackType.Lost);
    }
#endif

}
