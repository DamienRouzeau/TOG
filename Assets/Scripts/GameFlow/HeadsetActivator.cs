using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class HeadsetActivator : MonoBehaviour
{
	[SerializeField]
	private GameObject[] _targetsToEnable = null;
	[SerializeField]
	private GameObject[] _targetsToDisable = null;
	[SerializeField]
	private string _headsetName = "oculus";
	[Header("Debug")]
	public string showCurrentHeadsetName = "";

	private void Start()
	{
		List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();

		UnityEngine.XR.InputDevices.GetDevicesWithRole(UnityEngine.XR.InputDeviceRole.RightHanded, devices);

		foreach (var device in devices)
		{
			Debug.Log("Device " + device);

			var inputFeatures = new List<UnityEngine.XR.InputFeatureUsage>();
			if (device.TryGetFeatureUsages(inputFeatures))
			{
				foreach (var feature in inputFeatures)
				{
					Debug.Log("Feature " + feature.name);
				}
			}

			UnityEngine.XR.HapticCapabilities capabilities;
			if (device.TryGetHapticCapabilities(out capabilities))
			{
				Debug.Log("capabilities " + capabilities);
			}
		}

		showCurrentHeadsetName = SteamVR.instance != null ? SteamVR.instance.hmd_TrackingSystemName : _headsetName;
		bool activated = showCurrentHeadsetName == _headsetName;
		EnableTargets(_targetsToEnable, activated);
		EnableTargets(_targetsToDisable, !activated);
	}

	private void EnableTargets(GameObject[] targets, bool enable)
	{
		if (targets != null)
		{
			foreach (GameObject go in targets)
			{
				go.SetActive(enable);
			}
		}
	}
}
