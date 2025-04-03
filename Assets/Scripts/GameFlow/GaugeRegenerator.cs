using UnityEngine;
using UnityEngine.Events;

public class GaugeRegenerator : MonoBehaviour
{
    [SerializeField]
    private Animator _anim = null;
	[SerializeField]
	private string _fullAnimName = null;
	[SerializeField]
	private string _regenerateAnimName = null;
	[SerializeField]
	private string _emptyAnimName = null;
	[SerializeField]
	private UnityEvent _fullEvents = null;
	[SerializeField]
	private UnityEvent _emptyEvents = null;

	public float currentRatio => _currentRatio;
	private float _currentRatio = 0f;

	public void SetFullAnim()
	{
		_anim.Play(_fullAnimName, 0, 0f);
		_anim.speed = 1f;
		if (_fullEvents != null)
			_fullEvents.Invoke();
	}

    public void SetRatio(float ratio)
	{
		_currentRatio = ratio;
		_anim.Play(_regenerateAnimName, 0, 1f - ratio);
		_anim.speed = 0f; 
	}

	public void SetEmptyAnim()
	{
		_anim.Play(_emptyAnimName, 0, 0f);
		_anim.speed = 1f;
		if (_emptyEvents != null)
			_emptyEvents.Invoke();
	}

	
	// FOR TESTS
	//private void Update()
	//{
	//	if (Input.GetKeyDown(KeyCode.S))
	//	{
	//		SetFullAnim();
	//	}

	//	if (Input.GetKeyDown(KeyCode.D))
	//	{
	//		SetEmptyAnim();
	//	}

	//	if (Input.GetKey(KeyCode.F))
	//	{
	//		_currentRatio += Time.deltaTime;
	//		_currentRatio = _currentRatio % 1f;
	//		SetRatio(_currentRatio); 
	//	}
	//}

}
