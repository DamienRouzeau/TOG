using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollowerModifierTrigger : MonoBehaviour
{
    public enum Mode
	{
        SpeedModifier,
        StopUntilAnimation,
        StopUntilAllDead
	}

    [Header("Choose a mode")]
    public Mode mode = Mode.SpeedModifier;

    [Header("SpeedModifier params")]
    public AnimationCurve modifierCurve = null;
    public float fDuration = 0f;

    [Header("StopUntilAnimation params")]
    public Animator enterAnimator = null;
    public string enterAnimName = null;
    public Animator exitAnimator = null;
    public string exitAnimName = null;

    [Header("StopUntilAllDead params")]
    public List<Health> _healths = null;

    private void OnTriggerEnter(Collider other)
    {
        if( other.transform.parent==null )
        {
            return;
        }
        boat_followdummy followdummy = other.transform.parent.GetComponent<boat_followdummy>();
        if( followdummy == null || followdummy.dummy==null )
        {
            return;
        }

        PathFollower pathFollower = followdummy.dummy.GetComponent<PathFollower>();
        if (pathFollower != null)
        {
            switch (mode)
            {
                case Mode.SpeedModifier:
                    pathFollower.SetModifier(modifierCurve, fDuration);
                    break;
                case Mode.StopUntilAnimation:
                    StartCoroutine(CheckAnimationEnum(pathFollower));
                    break;
                case Mode.StopUntilAllDead:
                    StartCoroutine(CheckHealthListEnum(pathFollower));
                    break;
            }
        }        
    }

    private IEnumerator CheckAnimationEnum(PathFollower pathFollower)
	{
        if (enterAnimator == null || string.IsNullOrEmpty(enterAnimName))
            pathFollower.SetInPause(true);

        while (true)
        {
            // Enter
            if (!pathFollower.isInPause)
            {
                if (enterAnimator != null && !string.IsNullOrEmpty(enterAnimName))
                {
                    AnimatorStateInfo enterInfo = enterAnimator.GetCurrentAnimatorStateInfo(0);
                    if (enterInfo.IsName(enterAnimName))
                    {
                        pathFollower.SetInPause(true);
                    }
                }
            }
            else
            {
                // Exit
                if (exitAnimator != null && !string.IsNullOrEmpty(exitAnimName))
                {
                    AnimatorStateInfo exitInfo = exitAnimator.GetCurrentAnimatorStateInfo(0);
                    if (exitInfo.IsName(exitAnimName))
                    {
                        break;
                    }
                }
                else if (enterAnimator != null && !string.IsNullOrEmpty(enterAnimName))
                {
                    AnimatorStateInfo enterInfo = enterAnimator.GetCurrentAnimatorStateInfo(0);
                    if (!enterInfo.IsName(enterAnimName))
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            yield return null;
        }

        pathFollower.SetInPause(false);
	}

    private IEnumerator CheckHealthListEnum(PathFollower pathFollower)
    {
        if (_healths == null || _healths.Count == 0)
            yield break;

        pathFollower.SetInPause(true);

        while (true)
        {
            bool areAllDead = true;
            for (int i = 0; i < _healths.Count; ++i)
			{
                if (_healths[i].currentHealth > 0f)
                    areAllDead = false;
			}

            if (areAllDead)
                break;

            yield return null;
        }

        pathFollower.SetInPause(false);
    }
}
