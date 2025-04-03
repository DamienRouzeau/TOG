using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_Skulls : MonoBehaviour
{
    [SerializeField]
    private Image[] _skullImages = null;
	[SerializeField]
	private Animator[] _skullAnims = null;

	public void SetSkullDisplayWithDelay(int idx, bool display, bool animated, float delay)
	{
		StartCoroutine(SetSkullDisplayWithDelayEnum(idx, display, animated, delay));
	}

	private IEnumerator SetSkullDisplayWithDelayEnum(int idx, bool display, bool animated, float delay)
	{
		yield return new WaitForSeconds(delay);
		SetSkullDisplay(idx, display, animated);
	}

	public void SetSkullDisplay(int idx, bool display, bool animated)
	{
		if (idx < _skullImages.Length)
		{
			_skullAnims[idx].enabled = animated;
			if (animated)
			{	
				_skullAnims[idx].SetTrigger(display ? "Show" : "Hide");
			}
			else
			{
				_skullImages[idx].color = display ? Color.white : Color.black;
			}
		}
	}
}
