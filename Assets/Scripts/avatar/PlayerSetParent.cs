using Photon.Pun;
using UnityEngine;

public class PlayerSetParent : MonoBehaviour
{
	public enum SetParentType
	{
		Ref,
		Me,
		Null
	}
	
	[SerializeField]
    private Transform _parent = null;
	[SerializeField]
	private bool autoSetReferencedParentAtStart = false;
	[SerializeField]
	private bool autoSetMyTransformAsParentAtStart = false;
	[SerializeField]
	private bool autoUnsetParentAtStart = false;
	[SerializeField]
	private bool applyToAvatars = false;
	[SerializeField]
	private PhotonView _photonView = null;

	private void Start()
	{
		if (autoSetReferencedParentAtStart)
			SetReferencedParent();
		else if (autoSetMyTransformAsParentAtStart)
			SetMyTransformAsParent();
		else if (autoUnsetParentAtStart)
			UnsetParent();
		
	}

	public void SetReferencedParent()
	{
		Debug.Log("[PLAYER_SET_PARENT] SetReferencedParent");
		SetParentForPlayerAndAvatars(_parent, SetParentType.Ref, true);
	}

	public void SetMyTransformAsParent()
	{
		Debug.Log("[PLAYER_SET_PARENT] SetMyTransformAsParent");
		SetParentForPlayerAndAvatars(transform, SetParentType.Me, true);
	}

	public void UnsetParent()
	{
		Debug.Log("[PLAYER_SET_PARENT] UnsetParent");
		SetParentForPlayerAndAvatars(null, SetParentType.Null, true);
	}

	private void SetParentForPlayerAndAvatars(Transform tr, SetParentType setParentType, bool sendInMulti = false)
	{
		if (Player.myplayer != null)
		{
			Player.myplayer.transform.SetParent(tr);
			pointfromhand.teleporttarget.transform.SetParent(tr);
			if (applyToAvatars)
			{
				if (Player.myplayer.avatars != null)
				{
					foreach (Player_avatar avatar in Player.myplayer.avatars)
					{
						if (avatar != null && avatar.id >= 0)
						{
							avatar.transform.SetParent(tr);
						}
					}
				}
			}
			else
			{
				if (sendInMulti && _photonView != null)
					SetParentAvatarsOnMulti(setParentType, Player.myplayer.id);
			}
		}
	}

	public void SetParentAvatarsOnMulti(SetParentType setParentType, int id)
	{
		Debug.Log($"[PLAYER_SET_PARENT] SetParentAvatarsOnMulti {setParentType} {id}");
		_photonView.RPC("RpcSetParentAvatarsOnMulti", RpcTarget.Others, setParentType, id);
	}

	[PunRPC]
	private void RpcSetParentAvatarsOnMulti(SetParentType setParentType, int id)
	{
		Debug.Log($"[PLAYER_SET_PARENT] RpcSetParentAvatarsOnMulti {setParentType} {id}");
		switch (setParentType)
		{
			case SetParentType.Ref:
				SetParentForAvatars(_parent, id);
				break;
			case SetParentType.Me:
				SetParentForAvatars(transform, id);
				break;
			case SetParentType.Null:
				SetParentForAvatars(null, id);
				break;
		}
	}

	private void SetParentForAvatars(Transform tr, int id)
	{
		if (Player.myplayer != null && Player.myplayer.avatars != null)
		{
			foreach (Player_avatar avatar in Player.myplayer.avatars)
			{
				if (avatar != null && avatar.id == id)
				{
					avatar.transform.SetParent(tr);
				}
			}
		}
	}
}
