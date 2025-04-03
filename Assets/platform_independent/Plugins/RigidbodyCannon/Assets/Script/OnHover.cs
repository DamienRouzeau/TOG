//#define DEBUG_ONHOVER

using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
	public static Button lastButtonHover = null;
	public static bool needPlayerBullet = false;

	[SerializeField]
	private Button _button = null;
	[SerializeField]
	private bool _onlyMaster = false;
	[SerializeField]
	private bool _triggerOnMouseClick = false;
	[SerializeField]
	private bool _needPlayerBullet = true;

	private static int _hoverCounter = 0;

	private IEnumerator Start()
	{
		if (_button == null)
			_button = gameObject.GetComponentInParent<Button>();
		if (_button == null)
			_button = gameObject.GetComponentInChildren<Button>();

		if (_needPlayerBullet)
		{
			while (!PhotonNetwork.IsConnectedAndReady)
				yield return null;
		}

		if (_button != null)
			_button.interactable = CanUseButton();
	}

	private void OnDisable()
	{
		OnPointerExit();
	}

	private void OnDestroy()
	{
		OnPointerExit();
	}

	private void OnPointerEnter()
	{
		if (lastButtonHover == null)
			_hoverCounter = 0;
		_hoverCounter++;
#if DEBUG_ONHOVER
		Debug.Log($"OnHover - OnPointerEnter  button {_button.name} hoverCounter {_hoverCounter}");
#endif
		if (CanUseButton())
		{
			lastButtonHover = _button;
			needPlayerBullet = _needPlayerBullet;
		}
	}

	private void OnPointerExit()
	{
		if (_button != null)
		{
#if DEBUG_ONHOVER
			Debug.Log($"OnHover - OnPointerExit  button {_button.name} hoverCounter {_hoverCounter}");
#endif
			_hoverCounter--;
			if (_hoverCounter <= 0)
			{
				_hoverCounter = 0;
				lastButtonHover = null;
			}
		}
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		OnPointerEnter();
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		OnPointerExit();
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		//Debug.Log($"OnHover - OnPointerClick  button {_button.name} hoverCounter {_hoverCounter}");
		if (_triggerOnMouseClick && CanUseButton())
		{
			OnClickInvoke();
		}
	}

	[ContextMenu("OnClickInvoke")]
	public void OnClickInvoke()
	{
#if DEBUG_ONHOVER
		Debug.Log($"OnHover - OnClickInvoke  button {_button.name} hoverCounter {_hoverCounter}");
#endif
		_button.onClick.Invoke();
	}

	private bool CanUseButton()
	{
		if (PhotonNetworkController.soloMode)
			return _button != null && _button.interactable;
		else
			return _button != null && _button.interactable && (!_onlyMaster || GameflowBase.instance == null || PhotonNetworkController.IsMaster());
	}
}
