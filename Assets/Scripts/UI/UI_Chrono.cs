using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Chrono : MonoBehaviour
{
    [SerializeField]
    private Animator animator = null;
    [SerializeField]
    private Image fillImage = null;
    [SerializeField]
    private RectTransform nailTransform = null;

    private float startTimer = -1f;
    private float endTimer = -1f;
    private int previousTime = -1;
    private float startRemainTime = -1f; 

    public static void ShowChrono()
    {
        UI_Chrono[] allchronos = GameObject.FindObjectsOfType<UI_Chrono>();
        foreach (UI_Chrono chrono in allchronos)
        {
            chrono.ShowChrono(gameflowmultiplayer.gameRemainingTime);
        }
    }

    public void ShowChrono( float fRemainTime )
    {
        if (startTimer > 0f) // already shown
        {
            return;
        }
        startRemainTime = fRemainTime;
        startTimer = Time.time;
        endTimer = startTimer + fRemainTime;
        UpdateGraphic();
        animator.SetTrigger("BonusTime");
    }

    private void Update()
    {
        if( startTimer<0f )
        {
            return;
        }

        /*int currentTime = (int)( endTimer - Time.time );
        if( currentTime < 0 ) // end
        {
            startTimer = -1f;
            animator.SetTrigger("ChronoEnded");
        }
        if( currentTime != previousTime && currentTime<=gamesettings._myself.chronoAlarmTime && previousTime > gamesettings._myself.chronoAlarmTime) // alarm
        {
            animator.SetTrigger("AlarmOn");
        }
        previousTime = currentTime;*/
        int remain = (int)gameflowmultiplayer.gameRemainingTime;
        if (gameflowmultiplayer.gameplayEndRace)
        {
            startTimer = -1f;
            animator.SetTrigger("ChronoEnded");
        }
        if (remain != previousTime && remain <= gamesettings.myself.chronoAlarmTime && previousTime > gamesettings.myself.chronoAlarmTime) // alarm
        {
            animator.SetTrigger("AlarmOn");
        }
        previousTime = remain;
        UpdateGraphic();
    }

    private void UpdateGraphic()
    {
        float fCoeff = 1f - Mathf.Clamp01(gameflowmultiplayer.gameRemainingTime / startRemainTime);
        fillImage.fillAmount = fCoeff;
        nailTransform.localRotation = Quaternion.Euler(0f, 0f, -fCoeff * 360f);
    }

}
