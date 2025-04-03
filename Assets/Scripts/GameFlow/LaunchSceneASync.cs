//#define DEBUG_LOAD_SCENE

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LaunchSceneASync : MonoBehaviour
{
    public static LaunchSceneASync myself = null;

    public delegate void OnLoadScene(string prevScene, string newScene);

    public static string previousLoadedScene = "Cabin";
    public static string currentLoadedScene = "Cabin";
    public static OnLoadScene onLoadSceneCallback = null;

    public string sceneName = null;
    public LoadSceneMode loadSceneMode = LoadSceneMode.Single;
    public bool launchAtStart = true;
    public float launchAfterDelay = 0f;
    public float fadeOutDuration = 0f;
    public bool waitToActivateScene = false;

    public bool isLoadingScene => _isLoadingScene;
    private static bool _isLoadingScene = false;

    public bool isStartingLoadingScene => _isStartingLoadingScene;
    private static bool _isStartingLoadingScene = false;

    private bool _isWaitingActivation = false;
    public bool launchScene;

	private void Awake()
	{
        // Keep singleton on first instance
        if (myself == null)
            myself = this;
    }


	// Start is called before the first frame update
	IEnumerator Start()
    {
#if DEBUG_LOAD_SCENE
        Debug.Log($"[LOAD_SCENE] Start launchAfterDelay {launchAfterDelay} launchAtStart {launchAtStart}");
#endif
        if (launchAfterDelay > 0f)
            yield return new WaitForSeconds(launchAfterDelay);
        if (launchAtStart)
            LaunchScene();
    }
    private void Update()
    {
        if(launchScene)
        {
            LaunchScene();
            launchScene = false;
        }
    }

    [ContextMenu("LaunchScene")]
    public void LaunchScene()
	{
        _isStartingLoadingScene = true;

#if DEBUG_LOAD_SCENE
        Debug.Log($"[LOAD_SCENE] LaunchScene _isLoadingScene {_isLoadingScene} initial instance {this == myself}");
#endif
        if (this == myself)
        {
            StartCoroutine(LaunchSceneEnum());
        }
        else
		{
            myself.sceneName = sceneName;
            myself.loadSceneMode = loadSceneMode;
            myself.fadeOutDuration = fadeOutDuration;
            myself.waitToActivateScene = waitToActivateScene;
            myself.launchAtStart = launchAtStart;
            myself.LaunchScene();
        }
	}

    public void ActivateScene()
	{
        if (this == myself)
            _isWaitingActivation = false;
        else
            myself.ActivateScene();
	}

    private IEnumerator LaunchSceneEnum()
    {
#if USE_DEMO_MODE
        int count = GameLoader.GetDemoCounter();
        if (count <= 0)
            yield break;
#endif

#if DEBUG_LOAD_SCENE
        if (_isLoadingScene)
            Debug.Log($"[LOAD_SCENE] LaunchSceneEnum wait end of previous loading -> next: {sceneName} {loadSceneMode} {gameObject.name}");
#endif

        while (_isLoadingScene)
            yield return null;

#if DEBUG_LOAD_SCENE
        Debug.Log($"[LOAD_SCENE] LaunchSceneEnum {sceneName} {loadSceneMode} {gameObject.name}");
#endif
        if (!string.IsNullOrEmpty(sceneName))
        {
            _isLoadingScene = true;
            _isStartingLoadingScene = false;

            _isWaitingActivation = false;
            if (waitToActivateScene)
                _isWaitingActivation = true;

            previousLoadedScene = currentLoadedScene;
            currentLoadedScene = sceneName;            

            if (loadSceneMode == LoadSceneMode.Single && !launchAtStart)
            {
#if DEBUG_LOAD_SCENE
                Debug.Log($"[LOAD_SCENE] LaunchSceneEnum FadeOut fadeOutDuration {fadeOutDuration}");
#endif
                gamesettings_general.myself.FadeOutMusicVolume(fadeOutDuration);
                gamesettings_screen.myself.FadeOut(fadeOutDuration);
                while (gamesettings_screen.myself.faderunning)
                    yield return null;
                Player.myplayer.UnlinkPlayers();
            }

            if (onLoadSceneCallback != null)
                onLoadSceneCallback(previousLoadedScene, currentLoadedScene);
#if DEBUG_LOAD_SCENE
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
#endif
            Application.backgroundLoadingPriority = ThreadPriority.Low;
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
            if (op == null)
			{
                Debug.LogError($"LoadSceneAsync {sceneName} not found!");
                yield break;
			}
            op.allowSceneActivation = false;
            yield return null;

#if DEBUG_LOAD_SCENE
            stopwatch.Stop();
            Debug.Log($"[LOAD_SCENE] LaunchSceneEnum " + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            stopwatch.Start();
#endif
            yield return null;
            while (op.progress < 0.9f)
            {
#if DEBUG_LOAD_SCENE
                stopwatch.Stop();
				Debug.Log("[LOAD_SCENE] LaunchSceneEnum progress " + op.progress + " " + stopwatch.ElapsedMilliseconds);
				stopwatch.Reset();
				stopwatch.Start();
#endif
                yield return null;
            }

#if DEBUG_LOAD_SCENE
            stopwatch.Stop();
            Debug.Log($"[LOAD_SCENE] LaunchSceneEnum loaded progress {op.progress} " + stopwatch.ElapsedMilliseconds);
#endif

            if (waitToActivateScene)
			{
                while (_isWaitingActivation)
                    yield return null;
			}

            if (launchAtStart)
            {
#if DEBUG_LOAD_SCENE
                Debug.Log($"[LOAD_SCENE] LaunchSceneEnum FadeOut fadeOutDuration {fadeOutDuration}");
#endif
                gamesettings_general.myself.FadeOutMusicVolume(fadeOutDuration);
                gamesettings_screen.myself.FadeOut(fadeOutDuration);
                while (gamesettings_screen.myself.faderunning)
                    yield return null;
                Player.myplayer.UnlinkPlayers();
            }

#if DEBUG_LOAD_SCENE
            stopwatch.Reset();
            stopwatch.Start();
            Debug.Log($"[LOAD_SCENE] LaunchSceneEnum allowSceneActivation");
#endif
            op.allowSceneActivation = true;

            yield return null;            

            while (!op.isDone)
            {
                yield return null;
            }

#if DEBUG_LOAD_SCENE
            stopwatch.Stop();
            Debug.Log($"[LOAD_SCENE] LaunchSceneEnum Activated " + stopwatch.ElapsedMilliseconds);
#endif
            _isLoadingScene = false;
        }
    }
}
