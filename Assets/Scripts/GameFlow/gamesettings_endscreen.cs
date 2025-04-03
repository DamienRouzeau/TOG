using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gamesettings_endscreen : MonoBehaviour
{
    public static gamesettings_endscreen myself = null;

    [System.Serializable]
    public class BackgroundAssociation
	{
        public Sprite bkg;
        public string name;
	}

    [Header("Backgrounds")]
    public List<BackgroundAssociation> bkgs;
    public Sprite defaultBkg;

    private void Awake()
    {
        myself = this;
        DontDestroyOnLoad(gameObject);
    }

    public Sprite GetBackgroundFromLevelName(string name)
	{
        if (!string.IsNullOrEmpty(name))
        {
            if (bkgs != null && bkgs.Count > 0)
            {
                foreach (var bkg in bkgs)
                {
                    if (name.ToLower().Contains(bkg.name.ToLower()))
                    {
                        return bkg.bkg;
                    }
                }
            }
        }
        return defaultBkg;
	}
}
