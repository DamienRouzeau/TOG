using UnityEngine;

public class PlayerChangeWeapon : MonoBehaviour
{
	[SerializeField]
	private Player.WeaponType _weapon = Player.WeaponType.Musket;

	public void SetLeftHandWeapon()
	{
		if (Player.myplayer != null)
		{
			Player.myplayer.SetWeaponInPlace(_weapon, Player.WeaponPlace.LeftHand);
		}
	}

	public void SetRightHandWeapon()
	{
		if (Player.myplayer != null)
		{
			Player.myplayer.SetWeaponInPlace(_weapon, Player.WeaponPlace.RightHand);
		}
	}
}
