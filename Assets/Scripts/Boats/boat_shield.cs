using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boat_shield : MonoBehaviour
{
    public float timer = 0.0f;
    bool activate = false;
    Animator myanim = null;

    private void Awake()
    {
        myanim = gameObject.GetComponent<Animator>();
    }

    public void ActivateShield(float t)
    {
        timer = t;
        activate = true;
        gameObject.SetActive(true);
        if (myanim)
            myanim.SetTrigger("ShieldIn");
    }

    private void Update()
    {
        if (activate)
        {
            timer -= Time.deltaTime;
            if (timer <= 0.0f)
            {
                activate = false;
                timer = 0.0f;
                if (myanim)
                    myanim.SetTrigger("ShieldOut");
                StartCoroutine("RemoveShield");
            }
        }
    }

    IEnumerator RemoveShield()
    {
        yield return new WaitForSeconds(1);
        gameObject.SetActive(false);
    }
}
