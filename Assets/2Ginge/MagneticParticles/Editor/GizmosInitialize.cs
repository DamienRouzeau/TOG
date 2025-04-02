using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GizmosInitialize : MonoBehaviour {

    [InitializeOnLoad]
    public class Startup
    {
        static Startup()
        {
            try
            {
                FileUtil.CopyFileOrDirectory("Assets/2Ginge/MagneticParticles/Gizmos", "Assets/Gizmos");
            }
            catch
            {
            }
        }
    }
}
