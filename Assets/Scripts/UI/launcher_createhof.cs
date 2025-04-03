using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.IO;

public class launcher_createhof : MonoBehaviour
{
    List<UI_EndRaceResult.EndPlayerData>[] playerList;
    bool bAllStatsOk = false;

    public UI_EndRaceResult.LayoutData[] layoutDatas = new UI_EndRaceResult.LayoutData[4];
    public UI_EndRaceResult.TeamPanel[] teamsPanel;
    public UI_EndRacePlayer racePlayerPrefab;
    public UI_EndRacePlayer raceLosePlayerPrefab;
    public TMP_Text totalTreasurePercent;

    public Camera m_renderCamera;
    public int resWidth = 1920, resHeight = 1080;

    private List<UI_EndRacePlayer> allpanels = null;

    private void Awake()
    {
    }

    public void SetScores(int nr, long totalscore,long[]scores, bool[] win)
    {
        long biggest = 0;
        if (scores[0] > biggest) biggest = scores[0];
        if (scores[1] > biggest) biggest = scores[1];

        for (int i=0;i<2;i++)
        {
            if (nr == 1)
            {
                if (scores[0] == biggest) i = 0;
                else
                    i = 1;
                int percent = (int)(scores[i] * 100f / (float)totalscore);
                if (teamsPanel[0] != null)
                    teamsPanel[0].score.text = scores[i].ToString() + " ( " + percent.ToString() + " % ) ";
                if (teamsPanel[0].result != null)
                {
                    if (win[i])
                        teamsPanel[0].result.text = RRLib.RRLanguageManager.instance.GetString("str_win");
                    else
                        teamsPanel[0].result.text = RRLib.RRLanguageManager.instance.GetString("str_loose");
                }
                break;
            }
            else
            {
                int percent = (int)(scores[i] * 100f / (float)totalscore);
                if (teamsPanel[i].score != null)
                    teamsPanel[i].score.text = scores[i].ToString() + " ( " + percent.ToString() + " % ) ";
                if (teamsPanel[i].result != null)
                {
                    if (biggest == scores[i])
                        teamsPanel[i].result.text = RRLib.RRLanguageManager.instance.GetString("str_win");
                    else
                        teamsPanel[i].result.text = RRLib.RRLanguageManager.instance.GetString("str_loose");
                }
            }
        }
    }

    

    public void CleanPlayersStructures(launcher_createhof item)
    {
        m_renderCamera = item.gameObject.FindInChildren("Camera").GetComponent<Camera>();
        if (allpanels != null)
        {
            foreach (UI_EndRacePlayer erp in allpanels)
                Destroy(erp.gameObject);
        }
        allpanels = new List<UI_EndRacePlayer>();
    }

    public void TakeScreenShoot(string rootpath,int count)
    {
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        m_renderCamera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.ARGB32, false);
        m_renderCamera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        m_renderCamera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        byte[] bytes = screenShot.EncodeToJPG();
        string filename = rootpath+"/EndScreen_"+count+".jpg";
        File.WriteAllBytes(filename, bytes);

        string spriteTitle = null;
        if (multiplayerlobby.product == multiplayerlobby.Product.TOG)
            spriteTitle = "Pdf/PdfTitle";

        PdfGenerator._CreatePDFwithScreenShoot(bytes, resWidth, resHeight, rootpath, "/EndScreen_" + count, spriteTitle);
    }


}
