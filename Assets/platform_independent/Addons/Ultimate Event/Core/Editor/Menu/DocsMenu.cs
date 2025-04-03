/* ================================================================
   ---------------------------------------------------
   Project   :    Ultimate Event
   Publisher :    Infinite Dawn
   Author    :    Tamerlan Favilevich
   ---------------------------------------------------
   Copyright © Tamerlan Favilevich 2017 - 2018 All rights reserved.
   ================================================================ */

using UnityEditor;
using UnityEngine;

namespace UltimateEvent.Editor
{
    public static class DocsMenu
    {
        public static string LocalURL = System.Uri.EscapeUriString("file:///" + Application.dataPath + "/Ultimate Event/Documentation/Ultimate Event Manual.pdf");
        public static string InternetURL = "https://docs.google.com/document/d/1HMv5WF94REj0ks_BZe72QWwWselXu5caNgQ8Li9K8EA/edit?usp=sharing";

        [MenuItem("Ultimate Event/Documentation", false, 1)]
        public static void OpenAPI()
        {
            string URL = (Application.internetReachability != NetworkReachability.NotReachable) ? InternetURL : LocalURL;
            Application.OpenURL(URL);
        }
    }
}