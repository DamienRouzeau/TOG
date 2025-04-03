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
    public class CreateEvent
    {
        #region Menu Items
        [MenuItem("Ultimate Event/Create/Mono", false, 22)]
        private static void Mono()
        {
            GameObject go = new GameObject();
            go.name = "Mono Event";
            go.AddComponent<UEventBehaviour>();
        }

        [MenuItem("Ultimate Event/Create/Box UTrigger", false, 23)]
        private static void BoxTrigger()
        {
            UltimateTrigger.Create(UTriggerType.Box);
        }

        [MenuItem("Ultimate Event/Create/Sphere UTrigger", false, 24)]
        private static void SphereTrigger()
        {
            UltimateTrigger.Create(UTriggerType.Sphere);
        }

        [MenuItem("Ultimate Event/Create/Capluse UTrigger", false, 25)]
        private static void CapluseTrigger()
        {
            UltimateTrigger.Create(UTriggerType.Capluse);
        }

        [MenuItem("Ultimate Event/Create/Mesh UTrigger", false, 26)]
        private static void MeshTrigger()
        {
            UltimateTrigger.Create(UTriggerType.Mesh);
        }
        #endregion
    }
}