//#define DEBUG_GUARDIAN

using System.Collections;
using UnityEngine;

public class guardiandetection : MonoBehaviour
{
    private static GameObject _instantiatedVoiceOverM = null;
    private static GameObject _instantiatedVoiceOverF = null;
    private static int _triggerCounter = 0;
    private int _myTeam = -1;
    private bool _enteredInTrigger = false;

    private void Awake()
    {
        GameObject obj = gameObject;
        boat_followdummy bfd = obj.GetComponentInParent<boat_followdummy>();
        if (bfd != null)
           _myTeam = bfd.team;
    }

	private void OnDisable()
	{
        if (_enteredInTrigger)
            _triggerCounter--;
    }

	private void OnDestroy()
	{
        OnTriggerExit(null);
    }

	private void OnTriggerEnter(Collider other)
    {
        if (Player.myplayer == null)
            return;

        if (other.GetComponentInParent<Player>() == null)
        {
#if DEBUG_GUARDIAN
            Debug.LogWarning("[GUARDIAN] OBJECT TRIGGERED REJECTED:" + gameObject.name + " with " + other.gameObject.name);
#endif
            return;
        }

		if (Player.myplayer.guardian_appearance != null)
        {
            if ((_myTeam == -1) || (_myTeam == GameflowBase.myTeam))
            {
#if DEBUG_GUARDIAN
                Debug.Log("[GUARDIAN] OBJECT TRIGGERED:" + gameObject.name + " with " + other.gameObject.name);
#endif
                if (_triggerCounter == 0)
                {
                    if (!Player.myplayer.guardian_appearance.activeInHierarchy)
                    {
                        Player.myplayer.guardian_appearance.SetActive(true);
                        StartCoroutine("PlayVoice");
                    }
                }
                _triggerCounter++;
                _enteredInTrigger = true;
            }
        }
    }

    IEnumerator PlayVoice()
    {
        yield return new WaitForSeconds(1.0f);

        if (_myTeam == -1 || GameflowBase.myTeam == 0)
        {
            if (gamesettings_general.myself.guardian_vo_female != null && _instantiatedVoiceOverF == null)
            {
                _instantiatedVoiceOverF = Instantiate(gamesettings_general.myself.guardian_vo_female);
                DontDestroyOnLoad(_instantiatedVoiceOverF);
            }
            if (_instantiatedVoiceOverF != null)
                _instantiatedVoiceOverF.SetActive(true);
        }
        else
        {
            if (gamesettings_general.myself.guardian_vo_male != null && _instantiatedVoiceOverM == null)
            {
                _instantiatedVoiceOverM = Instantiate(gamesettings_general.myself.guardian_vo_male);
                DontDestroyOnLoad(_instantiatedVoiceOverM);
            }
            if (_instantiatedVoiceOverM != null)
                _instantiatedVoiceOverM.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (Player.myplayer == null)
            return;
        
        if (Player.myplayer.guardian_appearance != null)
        {
            if (Player.myplayer.guardian_appearance.activeInHierarchy)
            {
                if ((_myTeam == -1) || (_myTeam == GameflowBase.myTeam))
                {
                    _triggerCounter--;
                    _enteredInTrigger = false;
                    if (_triggerCounter > 0)
                        return;

                    StopCoroutine("PlayVoice");

                    Player.myplayer.guardian_appearance.SetActive(false);
                    if (_instantiatedVoiceOverF != null)
                        _instantiatedVoiceOverF.SetActive(false);
                    if (_instantiatedVoiceOverM != null)
                        _instantiatedVoiceOverM.SetActive(false);
                }
            }
        }
    }

}
