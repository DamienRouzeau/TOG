/* ================================================================
   ---------------------------------------------------
   Project   :    Ultimate Event
   Publisher :    Infinite Dawn
   Author    :    Tamerlan Favilevich
   ---------------------------------------------------
   Copyright © Tamerlan Favilevich 2017 - 2018 All rights reserved.
   ================================================================ */

using UnityEditor;

namespace UltimateEvent.Editor
{
    public class EventConvert
    {
        #region Menu Items
        [MenuItem("Ultimate Event/Utility/Convert Selected Objects/Mono", false, 32)]
        private static void ConvertToMono()
        {
            Convert();
        }

        /// <summary>
        /// Converter to Box Trigger
        /// </summary>
        [MenuItem("Ultimate Event/Utility/Convert Selected Objects/Box UTrigger", false, 33)]
        private static void ConvertToBox()
        {
            Convert(UTriggerType.Box);
        }

        /// <summary>
        /// Converter to Sphere Trigger
        /// </summary>
        [MenuItem("Ultimate Event/Utility/Convert Selected Objects/Sphere UTrigger", false, 34)]
        private static void ConvertToSphere()
        {
            Convert(UTriggerType.Sphere);
        }

        /// <summary>
        /// Converter to Capsule Trigger
        /// </summary>
        [MenuItem("Ultimate Event/Utility/Convert Selected Objects/Capsule UTrigger", false, 35)]
        private static void ConvertToCapsule()
        {
            Convert(UTriggerType.Capluse);
        }

        /// <summary>
        /// Converter to Mesh Trigger
        /// </summary>
        [MenuItem("Ultimate Event/Utility/Convert Selected Objects/Mesh UTrigger", false, 36)]
        private static void ConvertToMesh()
        {
            Convert(UTriggerType.Mesh);
        }

        /// <summary>
        /// Converting object
        /// </summary>
        /// <param name="triggerType"></param>
        private static void Convert(UTriggerType triggerType)
        {
            if (Selection.gameObjects.Length > 1)
            {
                if (EditorUtility.DisplayDialog("Warning", "Selected " + Selection.gameObjects.Length + " objects, do you really want to convert each of them into a trigger?", "Yes", "No"))
                {
                    for (int i = 0; i < Selection.gameObjects.Length; i++)
                    {
                        UltimateTrigger.ConvertObject(Selection.gameObjects[i], triggerType);
                    }
                }
            }
            else if (Selection.gameObjects.Length == 1)
            {
                UltimateTrigger.ConvertObject(Selection.gameObjects[0], triggerType);
            }
            else if (Selection.gameObjects.Length == 0)
            {
                if (EditorUtility.DisplayDialog("Warning", "No selected objects to convert, select one or more objects from hierarchy", "Ok")) { }
            }
        }

        /// <summary>
        /// Converting object
        /// </summary>
        private static void Convert()
        {
            if (Selection.gameObjects.Length > 1)
            {
                if (EditorUtility.DisplayDialog("Warning", "Selected " + Selection.gameObjects.Length + " objects, do you really want to convert each of them into a trigger?", "Yes", "No"))
                {
                    for (int i = 0; i < Selection.gameObjects.Length; i++)
                    {
                        Selection.gameObjects[i].AddComponent<UEventBehaviour>();
                    }
                }
            }
            else if (Selection.gameObjects.Length == 1)
            {
                Selection.gameObjects[0].AddComponent<UEventBehaviour>();
            }
            else if (Selection.gameObjects.Length == 0)
            {
                if (EditorUtility.DisplayDialog("Warning", "No selected objects to convert, select one or more objects from hierarchy", "Ok")) { }
            }

        }
        #endregion
    }
}