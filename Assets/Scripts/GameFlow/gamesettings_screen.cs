using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class gamesettings_screen : MonoBehaviour
{
    public static gamesettings_screen myself = null;

    public float start_fade_time_seconds = 2.0f;
    public float screen_fade_time_seconds = 1.0f;
    public bool faderunning = false;

    [Header("Reload Requested")]
    public float timebeforereload = 1.0f;
    public bool reloadgame = false;

    BeautifyEffect.Beautify fadeeffect = null;
    float colorfade = 0.0f;
    float startbrightness;
    float faderpart;

    GameObject fadescreen = null;

	private void Awake()
	{
        myself = this;
        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator Start()
	{
        Debug.Assert(Player.myplayer != null);
        Debug.Assert(Player.myplayer.cam != null);
        fadeeffect = Player.myplayer.cam.GetComponent<BeautifyEffect.Beautify>();
        Debug.Assert(fadeeffect != null);

        startbrightness = fadeeffect.brightness;
        if (startbrightness == 0.0f)
            startbrightness = 1.0f;
        colorfade = 0;
        faderpart = startbrightness / start_fade_time_seconds;
        fadeeffect.brightness = colorfade;

        while (!Player.myplayer.isPlayerSpawned)
            yield return null;

        yield return new WaitForSeconds(1);

        yield return Fader(faderpart);
    }

    public void FadeIn()
    {
        //Debug.Log("[FADE] FadeIn colorfade " + colorfade);
        if (colorfade != 0.0f) return;      // no fade
        if (fadeeffect == null) return;
        StartCoroutine(Fader(faderpart));
    }

    public void FadeOut(float seconds = 0.0f)
    {
        //Debug.Log("[FADE] FadeOut colorfade " + colorfade);
        if (colorfade < startbrightness) return;      // no fade
        if (seconds == 0.0f) seconds = screen_fade_time_seconds;
        fadeeffect = Player.myplayer.gameObject.GetComponentInChildren<BeautifyEffect.Beautify>();
        if (fadeeffect == null) return;
        startbrightness = fadeeffect.brightness;
        colorfade = startbrightness;
        faderpart = startbrightness / seconds;
        StartCoroutine(Fader(-faderpart));
    }

    IEnumerator Fader(float adder)
    {
        faderunning = true;
        //Debug.Log("[FADE] Fader adder " + adder);
        while (true)
        {
            colorfade += (adder * Time.deltaTime) / 2.0f;
            if (colorfade <= 0.0f)
            {
                colorfade = 0.0f;
                fadeeffect.brightness = colorfade;
                break;
            }
            if (colorfade >= startbrightness)
            {
                colorfade = startbrightness;
                fadeeffect.brightness = colorfade;
                break;
            }
            fadeeffect.brightness = colorfade;
            yield return null;
        }
        faderunning = false;
    }

    public void SetBrightness(float brightness)
	{
        fadeeffect.brightness = brightness;
    }

    private void Update()
    {
        if (reloadgame)
        {
            reloadgame = false;
            StartCoroutine(ReloadTheGame());
        }
    }

    IEnumerator ReloadTheGame()
    {
        yield return new WaitForSeconds(timebeforereload);
        multiplayerlobby.myself.LeaveRoom();
        while (multiplayerlobby.myself.insideroom)
            yield return null;
        apicalls.myself?.StopGameCounter();
        Scene scen = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scen.name);
    }

}
