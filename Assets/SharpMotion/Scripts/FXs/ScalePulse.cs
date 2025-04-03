using System.Collections;
using UnityEngine;

public class ScalePulse : MonoBehaviour
{
    public float scalePercentage = 10f;
    public float speed = 30f;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }
    private void OnEnable()
    {
        StartCoroutine(ChangeScale());
    }
    IEnumerator ChangeScale()
    {
        while (true)
        {
            yield return StartCoroutine(ScaleUp());
            yield return StartCoroutine(ScaleDown());
        }
    }

    IEnumerator ScaleUp()
    {
        Vector3 targetScale = originalScale * (1 + scalePercentage / 100f);
        while (Vector3.Distance(transform.localScale, targetScale) > 0.01f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * speed);
            yield return null;
        }
        transform.localScale = targetScale;
    }

    IEnumerator ScaleDown()
    {
        Vector3 targetScale = originalScale;
        while (Vector3.Distance(transform.localScale, targetScale) > 0.01f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * speed);
            yield return null;
        }
        transform.localScale = targetScale;
    }
}