using System.Collections;
using UnityEngine;

public class ThemeEnableObjects : MonoBehaviour
{
    [System.Serializable]
    public class GameObjectArray
	{
        public GameObject[] objects = null;
    }

    [System.Serializable]
    public class ThemeGameObjectArray : RREnumArray<multiplayerlobby.SkinTheme, GameObjectArray> { }

    [SerializeField]
    private ThemeGameObjectArray _objectsToEnable = null;
    [SerializeField]
    private ThemeGameObjectArray _objectsToDisable = null;

    // Start is called before the first frame update
    void Start()
    {
        SetTheme(multiplayerlobby.theme);
    }

    public void SetTheme(multiplayerlobby.SkinTheme theme)
	{
        GameObject[] goEnableList = _objectsToEnable[theme].objects;
        if (goEnableList != null)
        {
            foreach (GameObject go in goEnableList)
                go.SetActive(true);
        }

        GameObject[] goDisableList = _objectsToDisable[theme].objects;
        if (goDisableList != null)
        {
            foreach (GameObject go in goDisableList)
                go.SetActive(false);
        }
    }
}
