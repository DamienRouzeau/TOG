using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptainVoiceOver : MonoBehaviour
{
    public enum CaptainVoices { victory, second, timeout }
    public enum LanguageVoice { en, fr, es, it }
    public enum GenderVoice { Male, female }

    [System.Serializable] public class voicesDico : RREnumArray<CaptainVoices, AudioClip> { };
    [System.Serializable] public class voicesLanguagesDico : RREnumArray<LanguageVoice, voicesDico> { };
    [System.Serializable] public class voicesGender : RREnumArray<GenderVoice, voicesLanguagesDico> { };


    public GenderVoice m_gender;
    public voicesGender m_voices;
    public AudioSource m_audioSource;

    public void PlayVoice( CaptainVoices captainVoices )
    {
        //Debug.Log($"[DEBUG_VOICEOVER] PlayVoice for captain {captainVoices}");

        LanguageVoice language = LanguageVoice.fr;
        if (gamesettings.myself !=null)
        {
            string lcode = gamesettings_general.myself.languageAudioCode.ToLower();
            if (lcode.Length > 2) lcode = lcode.Substring(0, 2);
            switch (lcode)
            {
                case "en":
                    language = LanguageVoice.en;
                    break;
                case "it":
                    language = LanguageVoice.it;
                    break;
                case "es":
                    language = LanguageVoice.es;
                    break;
            }
        }
        m_audioSource.clip = m_voices[m_gender][language][captainVoices];
        m_audioSource.Play();
    }

    

}
