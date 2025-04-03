//#define DEBUG_BOATGOLD

using UnityEngine;
using TMPro;
using System.Collections;
using RRLib;

public class boat_gold : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _textField = null;
    [SerializeField]
    private Animator _anim = null;

    private boat_followdummy _boat = null;

    // Start is called before the first frame update
    void Start()
    {
        if (_textField == null)
            _textField = gameObject.GetComponent<TextMeshProUGUI>();
        if (_anim != null)
            _anim = gameObject.GetComponentInParent<Animator>();
        _boat = GetComponentInParent<boat_followdummy>();
        if (_boat != null)
            _boat.onGoldUpdatedCallback += OnGoldUpdated;
    }

	private void OnDestroy()
	{
        if (_boat != null)
            _boat.onGoldUpdatedCallback -= OnGoldUpdated;
    }

    private void OnGoldUpdated(int gold)
    {
#if DEBUG_BOATGOLD
        Debug.Log($"TotalGold - OnGoldUpdated {gold} for boat {_boat.teamColor}");
#endif

        if (gold == 0)
            _textField.text = RRLanguageManager.instance.GetString("str_goldempty");
        else
            _textField.text = gold.ToString();
        StartCoroutine(OnGoldAnimationEnum(0.5f));
    }

    private IEnumerator OnGoldAnimationEnum(float duration)
	{
        _anim.SetBool("Collecting", true);
        yield return new WaitForSeconds(duration);
        _anim.SetBool("Collecting", false);
    }
}
