using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrepareToLoadScene : MonoBehaviour
{
    // Start is called before the first frame update
    void OnEnable()
    {
        Player.myplayer.transform.SetParent(null, true);
        DontDestroyOnLoad(Player.myplayer.gameObject);
        StartCoroutine(LoadSceneASynch());
    }

    private IEnumerator LoadSceneASynch()
	{
        yield return new WaitForSeconds(3f);

        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Starterhub_Poseidon");
    }
}
