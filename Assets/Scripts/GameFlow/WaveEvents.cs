using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class WaveEvents : MonoBehaviour
{
    [System.Serializable]
    public class ActionOnWaveEvent
	{
        public TowerDefManager.TowerDefState state;
        public UnityEvent startAction = null;
        public float startDelay = 0f;
        public UnityEvent endAction = null;
        public float endDelay = 0f;
        public int waveMin = 0;
        public int waveMax = 0;
        public bool relativeWaves = false;
        public int level = 0;
    }

    [SerializeField]
    private List<ActionOnWaveEvent> _actions = null;
    [SerializeField]
    private List<TextMeshProUGUI> _updateNumWaveTexts = null;

    private void Awake()
    {
        TowerDefManager.onTowerDefState += OnTowerDefState;
    }

    // Update is called once per frame
    private void OnDestroy()
    {
        TowerDefManager.onTowerDefState -= OnTowerDefState;
    }

	public void OnEnable()
	{
        if (TowerDefManager.myself != null)
		{
            OnTowerDefState(TowerDefManager.myself.state, 
                TowerDefManager.myself.currentWave, 
                TowerDefManager.myself.relativeWave, 
                multiplayerlobby.startLevel);
        }
    }

	private void OnTowerDefState(TowerDefManager.TowerDefState state, int numWave, int relativeWave, int level)
	{
        if (_actions != null)
		{
            foreach (ActionOnWaveEvent action in _actions)
			{
                if (action.state == state)
				{
                    if (action.level > 0 && action.level - 1 != level)
                        continue;
                    if (action.relativeWaves)
                    {
                        if (action.waveMin > 0 && action.waveMin - 1 > relativeWave)
                            continue;
                        if (action.waveMax > 0 && action.waveMax - 1 < relativeWave)
                            continue;
                    }
                    else
                    {
                        if (action.waveMin > 0 && action.waveMin - 1 > numWave)
                            continue;
                        if (action.waveMax > 0 && action.waveMax - 1 < numWave)
                            continue;
                    }
                    if (gameObject.activeInHierarchy)
                        StartCoroutine(TriggerActionEnum(action));
                    else if (action.startDelay == 0f)
                        action.startAction?.Invoke();
                }
			}
		}
        if (state == TowerDefManager.TowerDefState.WAVE && _updateNumWaveTexts != null)
		{
            string text = (numWave + 1).ToString("00");
            foreach (TextMeshProUGUI tmp in _updateNumWaveTexts)
                tmp.text = text;
        }
    }

    private IEnumerator TriggerActionEnum(ActionOnWaveEvent action)
	{
        if (action.startDelay > 0f)
            yield return new WaitForSeconds(action.startDelay);
        action.startAction?.Invoke();
        yield return null;
        if (action.endDelay > 0f)
            yield return new WaitForSeconds(action.endDelay);
        action.endAction?.Invoke();
    }
}
