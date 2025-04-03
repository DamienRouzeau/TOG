/* ================================================================
   ---------------------------------------------------
   Project   :    Ultimate Event
   Publisher :    Infinite Dawn
   Author    :    Tamerlan Favilevich
   ---------------------------------------------------
   Copyright © Tamerlan Favilevich 2017 - 2018 All rights reserved.
   ================================================================ */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace UltimateEvent
{
    [Serializable]
    public struct TransfornParam
    {
        public GameObject gameObject;
        public Vector3 position;
    }

    /// <summary>
    /// Transform Translate Event used for translate gameobjects
    /// </summary>
    [Serializable]
    [UltimateEvent("Transform/Translate", 4, false)]
    public class UTransformTranslateEvent : Event
    {
        private const string ID = "Transform.Translate";
        public override string GetID { get { return ID; } }

        [SerializeField] private List<TransfornParam> transfornTranslateParams = new List<TransfornParam>();

        public override void Tick()
        {
            for (int i = 0; i < transfornTranslateParams.Count; i++)
                transfornTranslateParams[i].gameObject.transform.Translate(transfornTranslateParams[i].position * Time.deltaTime);
        }

#if UNITY_EDITOR

        private ReorderableList reorderableList;

        public override void OnEnable()
        {
            base.OnEnable();
            reorderableList = new ReorderableList(transfornTranslateParams, typeof(GameObject), true, true, true, true)
            {
                drawHeaderCallback = (rect) => { EditorGUI.LabelField(rect, "GameObjects"); },

                onAddCallback = (list) => { transfornTranslateParams.Add(new TransfornParam()); },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    TransfornParam element = transfornTranslateParams[index];
                    rect.y += 2;
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 100, 50), "GameObject :");
                    element.gameObject = (GameObject)EditorGUI.ObjectField(
                        new Rect(rect.x + 95, rect.y, 200, EditorGUIUtility.singleLineHeight),
                        element.gameObject, typeof(GameObject), true);

                    EditorGUI.LabelField(new Rect(rect.x + 305, rect.y, 100, EditorGUIUtility.singleLineHeight), "Translate Position:");
                    element.position = EditorGUI.Vector3Field(
                        new Rect(rect.x + 410, rect.y, 200, EditorGUIUtility.singleLineHeight), GUIContent.none,
                        element.position);
                    transfornTranslateParams[index] = element;
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