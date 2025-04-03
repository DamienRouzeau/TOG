//#define DEBUG_CANON_HANDLE

using UnityEngine;

public class boat_canon_handle : MonoBehaviour
{
	public GameObject currentTorch => _currentTorch;

	private Player.WeaponType _weaponAtLeftHand = Player.WeaponType.None;
	private Player.WeaponType _weaponAtRightHand = Player.WeaponType.None;
	private bool _isCanonHandle = false;
	private bool _isTriggerDetected = false;
	private GameObject _currentTorch = null;

	public void OnCanonHandle(bool bOn)
    {
#if DEBUG_CANON_HANDLE
		Debug.Log($"[CANON_HANDLE] OnCanonHandle {bOn}");
#endif
		_isCanonHandle = bOn;
		_isTriggerDetected = false;
	}


	private void Update()
	{
		if (_isCanonHandle && !_isTriggerDetected)
		{
			if (Player.pointright.triggerstate)
			{
				if (Player.myplayer.GetWeaponAtPlace(Player.WeaponPlace.LeftHand) != Player.WeaponType.Canon_Torche)
				{
					RestoreWeapons();
					_weaponAtLeftHand = Player.myplayer.GetWeaponAtPlace(Player.WeaponPlace.LeftHand);
					_weaponAtRightHand = Player.WeaponType.None;
					Player.myplayer.SetWeaponInPlace(Player.WeaponType.Canon_Torche, Player.WeaponPlace.LeftHand);
					Player.myplayer.onPlayerEvent += OnPlayerEvent;
					_currentTorch = Player.myplayer.GetAttachObjectOfPlace(Player.WeaponPlace.LeftHand).gameObject;
				}
				_isTriggerDetected = true;
			}
			if (Player.pointleft.triggerstate)
			{
				if (Player.myplayer.GetWeaponAtPlace(Player.WeaponPlace.RightHand) != Player.WeaponType.Canon_Torche)
				{
					RestoreWeapons();
					_weaponAtLeftHand = Player.WeaponType.None;
					_weaponAtRightHand = Player.myplayer.GetWeaponAtPlace(Player.WeaponPlace.RightHand);
					Player.myplayer.SetWeaponInPlace(Player.WeaponType.Canon_Torche, Player.WeaponPlace.RightHand);
					Player.myplayer.onPlayerEvent += OnPlayerEvent;
					_currentTorch = Player.myplayer.GetAttachObjectOfPlace(Player.WeaponPlace.RightHand).gameObject;
				}
				_isTriggerDetected = true;
			}
		}
		else if (!_isCanonHandle && !_isTriggerDetected)
		{
			Player.myplayer.onPlayerEvent -= OnPlayerEvent;
			RestoreWeapons();
			_isTriggerDetected = true;
		}

#if UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.Space))
		{
			//if (Vector3.Distance(Player.myplayer.GetFootPos(), transform.position) < 1)
			OnCanonHandle(true);
		}
#endif
	}

	private void OnPlayerEvent(Player.PlayerEvent evt, object data)
	{
		if (evt == Player.PlayerEvent.Teleport)
		{
			Player.myplayer.onPlayerEvent -= OnPlayerEvent;
			RestoreWeapons();
		}
	}

	private void RestoreWeapons()
	{
		if (Player.myplayer.GetWeaponAtPlace(Player.WeaponPlace.LeftHand) == Player.WeaponType.Canon_Torche)
			Player.myplayer.SetWeaponInPlace(_weaponAtLeftHand, Player.WeaponPlace.LeftHand);
		if (Player.myplayer.GetWeaponAtPlace(Player.WeaponPlace.RightHand) == Player.WeaponType.Canon_Torche)
			Player.myplayer.SetWeaponInPlace(_weaponAtRightHand, Player.WeaponPlace.RightHand);
		_currentTorch = null;
	}

}
