using System.Collections;
using UnityEngine;

public class selectflag : MonoBehaviour
{
    public enum LanguageType
	{
        Audio,
        Written,
        Both
	}

    public delegate void OnSelectFlag(string country, LanguageType languageType, bool selected);

    public static OnSelectFlag onSelectFlag = null;

    public LanguageType languageType = LanguageType.Audio;
    public string country = "";
    public GameObject root = null;
    public GameObject goSelected = null;

    private Animator _anim;
    private selectflag[] _allflags;
    private ParticleSystem _ps = null;
    private bool _isSelected = false;

    private void Awake()
    {
        Debug.Assert(!string.IsNullOrEmpty(country), "selectflag: No country!");
        Debug.Assert(root != null, "selectflag; no root!");
        _allflags = root.GetComponentsInChildren<selectflag>(true);
        _anim = transform.GetComponentInParent<Animator>();
        if (_anim != null)
            _anim.enabled = true;
        _ps = gameObject.FindInChildren("Flag_Particles")?.GetComponent<ParticleSystem>();
        if (_ps != null)
		{
            _ps.gameObject.SetActive(false);
        }
        StartCoroutine(InitSelectionEnum());
    }

    private IEnumerator InitSelectionEnum()
	{
        while (gamesettings_general.myself == null)
            yield return null;

        if (languageType == LanguageType.Written || languageType == LanguageType.Both)
        {
            if (!string.IsNullOrEmpty(gamesettings_general.myself.languageCode))
            {
                if (country.ToLower() == gamesettings_general.myself.languageCode.ToLower())
                    Select();
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(gamesettings_general.myself.languageAudioCode))
            {
                if (country.ToLower() == gamesettings_general.myself.languageAudioCode.ToLower())
                    Select();
            }
        }
    }

    IEnumerator PlayParticleSystem(float delay)
    {
        if (_ps != null)
        {
            _ps.gameObject.SetActive(true);
            _ps.Stop();
            _ps.Play();
            yield return new WaitForSeconds(delay);
            _ps.gameObject.SetActive(false);
        }
    }

    private void Select()
	{
        _isSelected = true;
        if (_anim != null)
        {
            _anim.StopPlayback();
            _anim.SetTrigger("IsSelected");
        }
        goSelected?.SetActive(true);
    }

    private void Unselect()
	{
        _isSelected = false;
        if (_anim != null && _anim.gameObject.activeInHierarchy)
            _anim.SetTrigger("UnSelect");
        if (_ps != null)
            _ps.gameObject.SetActive(false);
        goSelected?.SetActive(false);
    }

    public void ToggleFlag()
    {
        if (gameflowmultiplayer.myself != null && gameflowmultiplayer.myself.teamValidated)
            return;
        onSelectFlag?.Invoke(country, languageType, _isSelected);
        if (_isSelected)
            return;
        StopAllCoroutines();
        foreach (selectflag fl in _allflags)
        {
            if (fl != this && fl.languageType == languageType)
            {
                fl.Unselect();
            }
        }
        Select();
        gamesettings_general.LanguageChanged(country, languageType);
        StartCoroutine(PlayParticleSystem(2f));
    }

}
