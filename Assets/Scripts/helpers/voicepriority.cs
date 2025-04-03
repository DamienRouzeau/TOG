using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class voicepriority : MonoBehaviour
{
    public int mypiority = 0;

    public static AudioSource audiosource = null;
    public static int playinprio = 0;
    bool _enable = false;

    VoiceOver vo = null;

    private void Awake()
    {
        vo = gameObject.GetComponent<VoiceOver>();
    }
    private void Update()
    {
        if (vo) return;

        if (gameObject.activeInHierarchy)
        {
            if (!_enable)
            {
                if (mypiority >= playinprio)
                {
                    if (audiosource != null) audiosource.Stop();
                    AudioSource asrc = gameObject.GetComponent<AudioSource>();
                    asrc.Stop();
                    asrc.Play();
                    audiosource = asrc;
                    playinprio = mypiority;
                }
            }
            _enable = true;
        }
        else
        {
            if (_enable)
            {
                AudioSource asrc = gameObject.GetComponent<AudioSource>();
                asrc.Stop();
            }
            _enable = false;
        }
    }
}