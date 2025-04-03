using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace VRKB
{
    public class HapticsManagerSteamVR2 : MonoBehaviour
    {
        public SteamVR_Action_Vibration VibrateAction;

        public void OnKeyPress(KeyBehaviour key, Collider collider, bool autoRepeat)
        {
            Vibrate(collider);
        }

        public void Vibrate(Collider collider)
        {
            SteamVR_Behaviour_Pose pose = collider.GetComponentInParent<SteamVR_Behaviour_Pose>();
            if (pose == null)
                return;

            VibrateAction.Execute(0, 0.1f, 25.0f, 0.5f, pose.inputSource);
        }
    }
}