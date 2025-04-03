using UnityEngine;

public class PathFollowerController : MonoBehaviour
{
    [SerializeField]
    PathFollower m_target = null;

    [SerializeField]
    KeyCode m_speedUpControl = KeyCode.UpArrow;
    [SerializeField]
    KeyCode m_brakeControl = KeyCode.DownArrow;
    [SerializeField]
    KeyCode m_driftLeftControl = KeyCode.LeftArrow;
    [SerializeField]
    KeyCode m_driftRightControl = KeyCode.RightArrow;

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetworkController.IsMaster())
        {
            if (Input.GetKey(m_speedUpControl))
            {
                m_target.SpeedUp();
            }
            if (Input.GetKey(m_brakeControl))
            {
                m_target.Brake();
            }
            if (Input.GetKey(m_driftLeftControl))
            {
                m_target.DriftLeft();
            }
            if (Input.GetKey(m_driftRightControl))
            {
                m_target.DriftRight();
            }
        }
    }
}
