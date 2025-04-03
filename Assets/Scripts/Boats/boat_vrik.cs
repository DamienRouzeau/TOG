using UnityEngine;
using RootMotion.FinalIK;

public class boat_vrik : MonoBehaviour
{
    [SerializeField]
    private int _team = 0;

    private Quaternion _lastRotation;
    private Vector3 _lastPosition;

    public Vector3 lastMove => _lastMove;
    private Vector3 _lastMove;

    void OnEnable()
    {
        _lastPosition = transform.position;
        _lastRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        if (Player.myplayer.team >= 0)
        {
            _lastMove = transform.position - _lastPosition;
            Quaternion diffRot = transform.rotation * Quaternion.Inverse(_lastRotation);
            Vector3 pivot = transform.position;
            if (Player.myplayer.team == _team)
            {
                AddPlatformMotionOnVRIK(Player.myplayer.vrik, _lastMove, diffRot, pivot, "MyPlayer " + GameflowBase.myId + " on team " + _team);
            }
            foreach (Player_avatar avatar in Player.myplayer.avatars)
            {
                if (avatar.actornumber >= 0 && avatar.team == _team)
                {
                    AddPlatformMotionOnVRIK(avatar.vrik, _lastMove, diffRot, pivot, "Avatar " + avatar.actornumber + " on team " + _team);
                }
            }
        }
        _lastPosition = transform.position;
        _lastRotation = transform.rotation;
    }

    private void AddPlatformMotionOnVRIK(VRIK vrik, Vector3 diffPos, Quaternion diffRot, Vector3 pivot, string context = "")
    {
        if (vrik != null)
        {
            //Debug.Log($"AddPlatformMotionOnVRIK {context} {diffPos} {diffRot} {pivot}");
            vrik.solver.AddPlatformMotion(diffPos, diffRot, pivot);
        }
        else
        {
            Debug.LogError($"AddPlatformMotionOnVRIK vrik is null for " + context);
        }
    }

}
