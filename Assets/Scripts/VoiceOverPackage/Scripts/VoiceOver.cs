/********************************************************************
	created:	2020/04/15
	file base:	VoiceOver
	file ext:	cs
	author:		Alessandro Maione
	version:	1.0.0
	
	purpose:	plays a localized gender based audio clip each time the component is enabled
*********************************************************************/

//#define DEBUG_VOICEOVER

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// plays a localized gender based audio clip each time the component is enabled
/// </summary>
public class VoiceOver : MonoBehaviour
{

    #region Classes

    /// <summary>
    /// class used to store info about the localized audio to be played
    /// </summary>
    [System.Serializable]
    public class LocalizedAudioList
    {

        #region Fields

        [Header( "Options" )]
        /// <summary>
        /// language name string (used in inspector to give a readable name to the component)
        /// </summary>
        [HideInInspector]
        public string Lang = "";

        /// <summary>
        /// language code
        /// </summary>
        [Tooltip( "language code" )]
        public LanguageVoice Language;

        /// <summary>
        /// audio clips to be played
        /// </summary>
        [Tooltip( "audio clips to be played" )]
        public AudioClip[] Audios = System.Array.Empty<AudioClip>();

        /// <summary>
        /// index of clip to play
        /// </summary>
        [Tooltip( "index of clip to play" )]
        public int Next = 0;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="lang">language code</param>
        public LocalizedAudioList( LanguageVoice lang )
        {
            Language = lang;
            InitLabel( lang );
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// inits the Lang label to get nice inspector view
        /// </summary>
        /// <param name="lang"></param>
        public void InitLabel( LanguageVoice lang )
        {
            Lang = lang.ToString().ToUpper();
        }

        /// <summary>
        /// Fetches an audio clip from list (if any)
        /// Then increments the index of next clip to fecth
        /// </summary>
        /// <returns></returns>
        public AudioClip FetchAudioClip()
        {
            if ( Audios != null )
            {
                if ( Audios.Length > 0 )
                {
                    AudioClip res = Audios[Next];
                    Next = ( Next + 1 ) % Audios.Length;
                    //Debug.Log($"VOICEOVER FetchAudioClip {Lang} {Audios.Length} {Next} {res.name}" );
                    return res;
                }
            }

            return null;
        }

        #endregion Methods

    }

    #endregion Classes

    #region Fields

    /// <summary>
    /// auid source
    /// </summary>
    public AudioSource m_audioSource;
    /// <summary>
    /// audio (voice) gender
    /// </summary>
    [Tooltip( "audio (voice) gender" )]
    public GenderVoice Gender;
    /// <summary>
    /// language code as reported in LanguageVoice enum
    /// </summary>
    [Tooltip( "LanguageVoice" )]
    public LanguageVoice Language;
    /// <summary>
    /// if true, the language is fetched by gamesettings and overwrites the Language parameter above
    /// </summary>
    [Tooltip( "if true, the language is fetched by gamesettings and overwrites the Language parameter above" )]
    public bool AutoLanguage = true;

    [Header( "Gender based localized audios" )]
    /// <summary>
    /// Neutral gender localized audio list
    /// </summary>
    [Tooltip( "Neutral gender localized audio list" )]
    public LocalizedAudioList[] Neutral = null;
    /// <summary>
    /// Male gender localized audio list
    /// </summary>
    [Tooltip( "Male gender localized audio list" )]
    public LocalizedAudioList[] Male = null;
    /// <summary>
    /// Female gender localized audio list
    /// </summary>
    [Tooltip( "Female gender localized audio list" )]
    public LocalizedAudioList[] Female = null;

    [Tooltip("Fill this in if you did not attach the voicepriority scrip")]
    public int mypiority = 0;

    [Tooltip("You can have a delay between 2 plays of this VoiceOver")]
    public float delayBetweenPlays = 0f;

    /// <summary>
    /// Check this if you want to play voice over each time the gameObject is enabled
    /// </summary>
    [Tooltip( "Check this if you want to play voice over each time the gameObject is enabled" )]
    public bool playVoiceOverWhenEnable = true;

    public UnityEvent endVoiceOverAction = null;

    private bool _isWaitingEndOfPlay = false;

    public static bool isPlayingVoice => _isPlayingVoice;
    private static bool _isPlayingVoice = false;

    private float _elapsedTimeFromLastPlay = 0f;

    #endregion Fields

    #region Methods

    private void Awake()
    {
        voicepriority vp = gameObject.GetComponent<voicepriority>();
        if (vp)            mypiority = vp.mypiority;
    }

	private void OnDestroy()
	{		
        if (_isWaitingEndOfPlay)
		{
            if (voicepriority.playinprio == mypiority)
                voicepriority.playinprio = 0;
#if DEBUG_VOICEOVER
            Debug.Log($"[DEBUG_VOICEOVER] Voice over destroyed but was playing on {gameObject.name} with prio {mypiority} - current prio {voicepriority.playinprio}");
#endif
            _isPlayingVoice = false;
            _isWaitingEndOfPlay = false;
        }
    }

    private void OnDisable()
    {
        if (_isWaitingEndOfPlay)
        {
            if (voicepriority.playinprio == mypiority)
                voicepriority.playinprio = 0;
#if DEBUG_VOICEOVER
            Debug.Log($"[DEBUG_VOICEOVER] Voice over disabled but was playing on {gameObject.name} with prio {mypiority} - current prio {voicepriority.playinprio}");
#endif
            _isPlayingVoice = false;
            _isWaitingEndOfPlay = false;
        }
    }

	private void Update()
	{
		if (delayBetweenPlays > 0f)
		{
            _elapsedTimeFromLastPlay += Time.deltaTime;
        }
	}

	/// <summary>
	/// Play an audio clip depending on selected gender and language
	/// This method is called in OnEnable
	/// </summary>
	public void PlayVoiceOver()
    {
        if (delayBetweenPlays > 0f)
		{
            if (_elapsedTimeFromLastPlay > 0f && _elapsedTimeFromLastPlay < delayBetweenPlays)
                return;
            _elapsedTimeFromLastPlay = 0f;
        }

#if DEBUG_VOICEOVER
        Debug.Log($"[DEBUG_VOICEOVER] PlayVoiceOver on {gameObject.name} with prio {mypiority} - current prio {voicepriority.playinprio}");
#endif
		if ( AutoLanguage )
            UpdateLanguage();

        if (mypiority >= voicepriority.playinprio)
        {
            m_audioSource.clip = GetClip(Gender, Language);
            if (m_audioSource.clip != null)
            {
                //Debug.Log($"[DEBUG_VOICEOVER] PlayVoiceOver m_audioSource.clip {m_audioSource.clip.name}");
                if (voicepriority.audiosource != null)
                    voicepriority.audiosource.Stop();
                m_audioSource.Play();
                _isPlayingVoice = true;
#if DEBUG_VOICEOVER
                Debug.Log($"[DEBUG_VOICEOVER] PlayVoiceOver m_audioSource.Play clip {m_audioSource.clip} mypiority {mypiority} >= {voicepriority.playinprio}");
#endif
                voicepriority.playinprio = mypiority;
                voicepriority.audiosource = m_audioSource;
                StartCoroutine(RequestFreePriority());
            }
#if DEBUG_VOICEOVER
            else
            {
                Debug.Log($"[DEBUG_VOICEOVER] PlayVoiceOver no clip found !!! Gender {Gender} Language {Language}");
            }
#endif
        }
#if DEBUG_VOICEOVER
        else
        {
            Debug.Log($"[DEBUG_VOICEOVER] PlayVoiceOver rejected !!! mypiority {mypiority} < {voicepriority.playinprio}");
        }
#endif
    }

    IEnumerator RequestFreePriority()
    {
        _isWaitingEndOfPlay = true;
#if DEBUG_VOICEOVER
        Debug.Log($"[DEBUG_VOICEOVER] RequestFreePriority start m_audioSource.isPlaying {m_audioSource.isPlaying} voicepriority.playinprio {voicepriority.playinprio} mypiority {mypiority}");
#endif
        while (m_audioSource.isPlaying)
            yield return null;

        if (voicepriority.playinprio == mypiority)
            voicepriority.playinprio = 0;
#if DEBUG_VOICEOVER
        Debug.Log($"[DEBUG_VOICEOVER] RequestFreePriority end voicepriority.playinprio {voicepriority.playinprio}");
#endif
        if (UI_Tutorial.myself != null)
            UI_Tutorial.myself.OnTriggerCondition(UI_Tutorial.TutoCondition.WaitVoiceOver);

        endVoiceOverAction?.Invoke();

        _isPlayingVoice = false;
        _isWaitingEndOfPlay = false;
    }


    private AudioClip GetClip( GenderVoice gender, LanguageVoice language )
    {
        AudioClip res = null;
        LocalizedAudioList localizedAudioList = null;

        switch ( gender )
        {
            case GenderVoice.Neutral:
                localizedAudioList = System.Array.Find( Neutral, lal => lal.Language == Language );
                if ( localizedAudioList != null )
                    res = localizedAudioList.FetchAudioClip();
                break;
            case GenderVoice.Male:
                localizedAudioList = System.Array.Find( Male, lal => lal.Language == Language );
                if ( localizedAudioList != null )
                    res = localizedAudioList.FetchAudioClip();
                break;
            case GenderVoice.Female:
                localizedAudioList = System.Array.Find( Female, lal => lal.Language == Language );
                if ( localizedAudioList != null )
                    res = localizedAudioList.FetchAudioClip();
                break;

            default:
                Debug.LogError( "undefined gender" );
                break;
        }

        if ( !res )
            Debug.LogWarning( "no audio clip found for gender " + gender + " / language " + language + " in " + gameObject.name );

        return res;
    }

    private void Reset()
    {
        m_audioSource = GetComponentInChildren<AudioSource>();

        string[] langs = Enum.GetNames( typeof( LanguageVoice ) );
        Male = new LocalizedAudioList[langs.Length];
        Female = new LocalizedAudioList[langs.Length];
        Neutral = new LocalizedAudioList[langs.Length];

        for ( int i = 0; i < langs.Length; i++ )
        {
            Male[i] = new LocalizedAudioList( (LanguageVoice)i );
            Female[i] = new LocalizedAudioList( (LanguageVoice)i );
            Neutral[i] = new LocalizedAudioList( (LanguageVoice)i );
        }
    }

    private void OnEnable()
    {
        if (playVoiceOverWhenEnable)
            PlayVoiceOver();
    }

    private void UpdateLanguage()
    {
        int cnti = 0;
        foreach ( string lang in Enum.GetNames( typeof( LanguageVoice ) ) )
        {
            string lancode = gamesettings_general.myself?.languageAudioCode?.ToLower() ?? "fr";
            if (lancode.Length > 2)
                lancode = lancode.Substring(0,2);
            if ( lancode == lang.ToLower() )
            {
                Language = (LanguageVoice)cnti;
                return;
            }
            cnti++;
        }
    }

    private void OnValidate()
    {
        UpdateLabels( Neutral );
        UpdateLabels( Male );
        UpdateLabels( Female );
    }

    private void UpdateLabels( LocalizedAudioList[] localizedAudioList )
    {
        foreach ( var item in localizedAudioList )
            item.InitLabel( item.Language );
    }

    #endregion Methods

}
