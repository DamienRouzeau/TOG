using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChoicesInputField : InputField
{
	private System.Action _onClick = null;

	public void Init(System.Action clickCbk)
	{
		_onClick = clickCbk;
	}
	
    public override void OnPointerClick(PointerEventData eventData)
	{
		_onClick?.Invoke();
	}
}
