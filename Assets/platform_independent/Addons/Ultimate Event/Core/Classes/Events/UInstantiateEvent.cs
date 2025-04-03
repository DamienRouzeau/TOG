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
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using UnityEngine;

namespace UltimateEvent
{
    /// <summary>
    /// Instantiate object param
    /// </summary>
    [Serializable]
    public struct InstantiateObject
    {
        public GameObject gameObject;
        public Vector3 position;
        public Vector3 rotation;
    }

    /// <summary>
    /// Instantiate event used for instantiate gameobjects
    /// </summary>
    [Serializable]
    [UltimateEvent("GameObject/Instantiate", 0, false)]
    public class UInstantiateEvent : Event
    {
        private const string ID = "Instantiate Event";
        public override string GetID { get { return ID; } }

        [SerializeField] private List<InstantiateObject> instantiateObjects = new List<InstantiateObject>();
        private Quaternion[] m_Rotations;

        /// <summary>
        /// Initialization variable
        /// </summary>
        public override void Initialization()
        {
            m_Rotations = new Quaternion[instantiateObjects.Count];
            for (int i = 0; i < instantiateObjects.Count; i++)
                m_Rotations[i] = Quaternion.Euler(instantiateObjects[i].rotation);
        }

        /// <summary>
        /// Instantiate all gameobjects 
        /// </summary>
        public override void OnStart()
        {
            for (int i = 0; i < instantiateObjects.Count; i++)
            {
                Instantiate(instantiateObjects[i].gameObject, instantiateObjects[i].position, m_Rotations[i]);
            }
            SetStart(true);
            SetReady(true);
        }

#if UNITY_EDITOR
        #region Fields
        private ReorderableList reorderableList;
        #endregion

        /// <summary>
        /// Initialization variable OnEnable
        /// </summary>
        public override void OnEnable()
        {
            base.OnEnable();
            reorderableList = new ReorderableList(instantiateObjects, typeof(GameObject), true, true, true, true)
            {
                drawHeaderCallback = (rect) => { EditorGUI.LabelField(rect, "GameObjects"); },

                onAddCallback = (list) => { instantiateObjects.Add(new InstantiateObject()); },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    InstantiateObject element = instantiateObjects[index];
                    rect.y += 2;
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 100, 50), "GameObject " + (index + 1) + ":");
                    element.gameObject = (GameObject)EditorGUI.ObjectField(
                        new Rect(rect.x + 95, rect.y, 200, EditorGUIUtility.singleLineHeight),
                        element.gameObject, typeof(GameObject), true);

                    EditorGUI.LabelField(new Rect(rect.x + 305, rect.y, 100, EditorGUIUtility.singleLineHeight), "Position:");
                    element.position = EditorGUI.Vector3Field(
                        new Rect(rect.x + 360, rect.y, 200, EditorGUIUtility.singleLineHeight), GUIContent.none,
                        element.position);

                    EditorGUI.LabelField(new Rect(rect.x + 575, rect.y, 100, EditorGUIUtility.singleLineHeight), "Rotation:");
                    element.rotation = EditorGUI.Vector3Field(
                        new Rect(rect.x + 633, rect.y, 200, EditorGUIUtility.singleLineHeight), GUIContent.none,
                        element.rotation);
                    instantiateObjects[index] = element;
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

        /// <summary>
        /// Return Instantiate Objects
        /// </summary>
        public List<InstantiateObject> InstantiateObjects
        {
            get
            {
                return instantiateObjects;
            }

            set
            {
                instantiateObjects = value;
            }
        }
    }
}