using System.Collections;
using UnityEngine;

public class activate_childs : MonoBehaviour
{
    public AudioSource audiototest=null;
    public float waitbeforeshow = 0.0f;
    public bool showobject = true;

    private void OnEnable()
    {
        StartCoroutine("Activater");
    }

    IEnumerator Activater()
    {
        if (audiototest != null)
        {
            if (!audiototest.isPlaying)
            {
                while (!audiototest.isPlaying) yield return null;
            }
            while (audiototest.isPlaying)
                yield return null;
        }
        else
            yield return null;

        if (waitbeforeshow != 0) yield return new WaitForSeconds(waitbeforeshow);

        foreach (Transform child in gameObject.GetComponentsInChildren<Transform>(true))
        {
            if (child.parent == gameObject.transform)
            {
                if (showobject)
                    child.gameObject.SetActive(true);
                else
                    child.gameObject.SetActive(false);
            }
        }
    }
}
