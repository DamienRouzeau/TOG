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
    public class InspectorEditor : UnityEditor.Editor
    {
        private GUIStyle style = new GUIStyle();

        /// <summary>
        /// Title with deflaut Color
        /// </summary>
        /// <param name="message"></param>
        public virtual void Title(string message)
        {
            GUI.color = new Color32(255, 255, 255, 255);
            GUILayout.BeginVertical("Button");
            EditorGUILayout.LabelField(message, style);
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Title with custom color
        /// </summary>
        /// <param name="message"></param>
        /// <param name="titleColor"></param>
        public virtual void Title(string message, Color32 titleColor)
        {
            GUI.color = titleColor;
            GUILayout.BeginVertical("Button");
            EditorGUILayout.LabelField(message, style);
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Panel with deflaut color
        /// </summary>
        public virtual void BeginPanel()
        {
            EditorGUILayout.Space();
            GUI.color = new Color32(150, 150, 150, 255);
            GUILayout.BeginVertical("Window", GUILayout.Height(1));
        }

        /// <summary>
        /// Panel with custom color
        /// </summary>
        /// <param name="panelColor"></param>
        public virtual void BeginPanel(Color32 panelColor)
        {
            EditorGUILayout.Space();
            GUI.color = panelColor;
            GUILayout.BeginVertical("Window", GUILayout.Height(1));
        }

        public virtual void EndPanel()
        {
            GUILayout.EndVertical();
        }

        public virtual void Line()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        public virtual void InitGUIStyle(FontStyle fontStyle, TextAnchor textAnchor)
        {
            style.fontStyle = fontStyle;
            style.alignment = textAnchor;
        }

        public virtual void InitGUIStyle(FontStyle fontStyle, TextAnchor textAnchor, int fontsize)
        {
            style.fontStyle = fontStyle;
            style.alignment = textAnchor;
            style.fontSize = fontsize;
        }

        public virtual void InitGUIStyle(FontStyle fontStyle, TextAnchor textAnchor, int fontsize, Color color)
        {
            style.fontStyle = fontStyle;
            style.alignment = textAnchor;
            style.fontSize = fontsize;
            style.normal.textColor = color;
        }

        public virtual void ProgressBar(float value, string label)
        {
            Rect rect = GUILayoutUtility.GetRect(20, 20, "TextField");
            EditorGUI.ProgressBar(rect, value, label);
        }

        public GUIStyle Style { get { return style; } }
    }
}