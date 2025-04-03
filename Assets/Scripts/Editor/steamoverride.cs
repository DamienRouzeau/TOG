using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class steamoverride : EditorWindow
{
    SerializedProperty componentNameProp;
    static EditorWindow mywindow;

    [MenuItem("Window/SetVRMasterBuild")]

    public static void ShowWindow()
    {
        mywindow = EditorWindow.GetWindow(typeof(steamoverride));
        mywindow.minSize = new Vector2(200, 200);
    }


    void OnGUI()
    {
        GUILayout.TextField("Aute Enable:"+ Valve.VR.SteamVR_Settings.instance.autoEnableVR);

        if (GUILayout.Button("Toggle"))
        {
            if (Valve.VR.SteamVR_Settings.instance.autoEnableVR)
            {
                /*
                GameObject mas = Instantiate(Resources.Load("MasterCanvas") as GameObject);
                mas.name = "MasterCanvas";
                if (mas) Destroy(mas);
                */
                Valve.VR.SteamVR_Settings.instance.autoEnableVR = false;
                UnityEditor.PlayerSettings.virtualRealitySupported = false;
            }
            else
            {
                /*
                GameObject mas = GameObject.Find("MasterCanvas");
                if (mas)                    DestroyImmediate(mas);
                */
                Valve.VR.SteamVR_Settings.instance.autoEnableVR = true;
            }
        }
    }
}
