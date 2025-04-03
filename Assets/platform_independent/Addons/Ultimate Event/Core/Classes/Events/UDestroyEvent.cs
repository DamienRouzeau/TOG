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
    /// Destroy Event used for destroy gameobjects
    /// </summary>
    [Serializable]
    [UltimateEvent("GameObject/Destroy", 1, false)]
    public class UDestroyEvent : Event
    {
        private const string ID = "Destroy Event";
        public override string GetID { get { return ID; } }

        [SerializeField] private List<string> names = new List<string>();
        [SerializeField] private List<GameObject> gameObjects = new List<GameObject>();
        

        /// <summary>
        /// Destroy all chose gameobjects
        /// </summary>
        public override void OnStart()
        {
            for (int i = 0; i < gameObjects.Count; i++)
                Destroy(gameObjects[i]);
            for (int i = 0; i < names.Count; i++)
                if (GameObject.Find(names[i]) != null)
                    Destroy(GameObject.Find(names[i]));
        }

#if UNITY_EDITOR
        private ReorderableList nameReorderableList;
        private ReorderableList objectReorderableList;

        /// <summary>
        /// Initialization variable OnEnable
        /// </summary>
        public override void OnEnable()
        {
            base.OnEnable();
            nameReorderableList = new ReorderableList(names, typeof(string), true, true, true, true)
            {
                drawHeaderCallback = (rect) => { EditorGUI.LabelField(rect, "Find by name"); },

                onAddCallback = (reorderableList) => { reorderableList.list.Add(""); },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    rect.y += 2;
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 100, 50), "Name:");
                    names[index] = EditorGUI.TextField(
                        new Rect(rect.x + 50, rect.y, 200, EditorGUIUtility.singleLineHeight),
                        names[index]);
                }
            };

            objectReorderableList = new ReorderableList(gameObjects, typeof(GameObject), true, true, true, true)
            {
                drawHeaderCallback = (rect) => { EditorGUI.LabelField(rect, "GameObjects"); },

                onAddCallback = (reorderableList) => { reorderableList.list.Add(null); },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    rect.y += 2;
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 100, 50), "GameObject:");
                    gameObjects[index] = (GameObject)EditorGUI.ObjectField(
                        new Rect(rect.x + 85, rect.y, 200, EditorGUIUtility.singleLineHeight),
                        gameObjects[index], typeof(GameObject), true);
                }
            };
        }

        public override void FieldGUILayout()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            nameReorderableList.DoLayoutList();
            GUILayout.Space(3);
            objectReorderableList.DoLayoutList();
            GUILayout.EndVertical();
        }
#endif

        /// <summary>
        /// Return gameObject names
        /// </summary>
        public List<string> Names
        {
            get
            {
                return names;
            }

            set
            {
                names = value;
            }
        }

        /// <summary>
        /// Return gameObjects
        /// </summary>
        public List<GameObject> GameObjects
        {
            get
            {
                return gameObjects;
            }

            set
            {
                gameObjects = value;
            }
        }
    }
}