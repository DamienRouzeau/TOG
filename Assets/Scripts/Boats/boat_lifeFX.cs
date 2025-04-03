using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boat_lifeFX : MonoBehaviour
{
    public enum LifeFxType
	{
        Medium,
        Big
	}

    [System.Serializable]
    public class FxList
    {
        public float lifeThresholdMin = 0f;
        public float lifeThresholdMax = 100f;
        public float duration = 1f;
        public List<GameObject> goList = null;

        public bool isActive => _isActive;
        private bool _isActive = false;

        public void SetActive(bool active)
		{
            _isActive = active;
            foreach (GameObject go in goList)
            {
                go.SetActive(active);
            }
        }
    }

    [System.Serializable] public class FxListByType : RREnumArray<LifeFxType, FxList> { }

    public FxListByType fxLists = null;

    public void TriggerGivenLife(float givenLife)
    {
        foreach (FxList list in fxLists)
		{
            if (givenLife > list.lifeThresholdMin && givenLife < list.lifeThresholdMax)
			{
                StartCoroutine(TriggerFxEnum(list));
            }
		}
    }

    private IEnumerator TriggerFxEnum(FxList list)
	{
        while (list.isActive)
            yield return null;

        list.SetActive(true);

        yield return new WaitForSeconds(list.duration);

        list.SetActive(false);
    }
}
