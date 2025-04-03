using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_SetYourGuns : MonoBehaviour
{
	public void OnClickMuskets()
	{
		Player.myplayer.ChangeTypeOfWeaponData(0, Player.WeaponType.Musket);
		Player.myplayer.ChangeTypeOfWeaponData(1, Player.WeaponType.Musket);
	}

	public void OnClickBigGuns()
	{
		Player.myplayer.ChangeTypeOfWeaponData(0, Player.WeaponType.TOG_Biggun);
		Player.myplayer.ChangeTypeOfWeaponData(1, Player.WeaponType.TOG_Biggun);
	}

	public void OnClickBothWeapons()
	{
		if (Player.myplayer.GetTypeOfWeaponData(0) == Player.WeaponType.Musket)
		{
			Player.myplayer.ChangeTypeOfWeaponData(0, Player.WeaponType.TOG_Biggun);
			Player.myplayer.ChangeTypeOfWeaponData(1, Player.WeaponType.Musket);
		}
		else
		{
			Player.myplayer.ChangeTypeOfWeaponData(0, Player.WeaponType.Musket);
			Player.myplayer.ChangeTypeOfWeaponData(1, Player.WeaponType.TOG_Biggun);
		}
	}
}
