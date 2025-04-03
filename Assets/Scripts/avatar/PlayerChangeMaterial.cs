using Photon.Pun;
using UnityEngine;

public class PlayerChangeMaterial : MonoBehaviour
{
	[SerializeField]
	ChangeMaterial _changeMat = null;

	private void Start()
	{
#if !UNITY_STANDALONE
		UpdateMaterial();
#endif
	}

	public void UpdateMaterial()
	{
		int idx = 0;

		Player player = gameObject.GetComponentInParent<Player>();
		if (player != null)
			idx = GameflowBase.myId;

		Player_avatar avatar = gameObject.GetComponentInParent<Player_avatar>();
		if (avatar != null)
			idx = avatar.actornumber;

		if (PhotonNetwork.CurrentRoom != null)
			idx += PhotonNetwork.CurrentRoom.GetHashCode();

		_changeMat.SetMaterialFromIndex(idx);
	}
}
