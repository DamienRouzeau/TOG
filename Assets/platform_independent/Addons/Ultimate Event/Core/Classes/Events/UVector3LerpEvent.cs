/* ================================================================
   ---------------------------------------------------
   Project   :    Ultimate Event
   Publisher :    Infinite Dawn
   Author    :    Tamerlan Favilevich
   ---------------------------------------------------
   Copyright © Tamerlan Favilevich 2017 - 2018 All rights reserved.
   ================================================================ */

using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace UltimateEvent
{
    /// <summary>
    /// Vector3 lerp param
    /// </summary>
    [Serializable]
    public struct LerpParam
    {
        public GameObject gameObject;
        public Vector3 position;
        public float time;
    }

    /// <summary>
    /// Transform Translate Event used for translate gameobjects
    /// </summary>
    [Serializable]
    [UltimateEvent("Transform/Lerp", 5, false)]
    public class UVector3LerpEvent : Event
    {
        private const string ID = "Lerp";
        public override string GetID { get { return ID; } }

        [SerializeField] private List<LerpParam> lerpParams = new List<LerpParam>();
        private bool[] lerpReady;

        /// <summary>
        /// Called on Start
        /// </summary>
        public override void Initialization()
        {
            lerpReady = new bool[lerpParams.Count];
            for (int i = 0; i < lerpReady.Length; i++)
                lerpReady[i] = false;
        }

        /// <summary>
        /// Called every frame while event running
        /// </summary>
        public override void Tick()
        {
            for (int i = 0; i < lerpParams.Count; i++)
            {
                if (!lerpReady[i])
                {
                    lerpParams[i].gameObject.transform.position = Vector3.Lerp(lerpParams[i].gameObject.transform.position, lerpParams[i].position, lerpParams[i].time * Time.deltaTime);
                    if (lerpParams[i].gameObject.transform.position == lerpParams[i].position)
                        lerpReady[i] = true;
                }
            }
            for (int i = 0; i < lerpReady.Length; i++)
            {
                Debug.Log(lerpReady[i]);
                if (!lerpReady[i])
                    return;
                SetReady(true);
            }
        }

#if UNITY_EDITOR

        private ReorderableList reorderableList;

        public override void OnEnable()
        {
            reorderableList = new ReorderableList(lerpParams, typeof(GameObject), true, true, true, true)
            {
                drawHeaderCallback = (rect) => { EditorGUI.LabelField(rect, "GameObjects"); },

                onAddCallback = (list) => { lerpParams.Add(new LerpParam()); },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    LerpParam element = lerpParams[index];
                    rect.y += 2;
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 100, 50), "GameObject :");
                    element.gameObject = (GameObject)EditorGUI.ObjectField(
                        new Rect(rect.x + 95, rect.y, 200, EditorGUIUtility.singleLineHeight),
                        element.gameObject, typeof(GameObject), true);

                    EditorGUI.LabelField(new Rect(rect.x + 320, rect.y, 100, EditorGUIUtility.singleLineHeight), "Lerp Position:");
                    element.position = EditorGUI.Vector3Field(
                        new Rect(rect.x + 410, rect.y, 200, EditorGUIUtility.singleLineHeight), GUIContent.none,
                        element.position);

                    EditorGUI.LabelField(new Rect(rect.x + 640, rect.y, 100, EditorGUIUtility.singleLineHeight), "Time:");
                    element.time = EditorGUI.FloatField(
                        new Rect(rect.x + 690, rect.y, 200, EditorGUIUtility.singleLineHeight), GUIContent.none,
                        element.time);
                    lerpParams[index] = element;

                }
            };
        }

        public override void FieldGUILayout()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            reorderableList.DoLayoutList();
            GUILayout.EndVertical();
        }
#endif
    }
}