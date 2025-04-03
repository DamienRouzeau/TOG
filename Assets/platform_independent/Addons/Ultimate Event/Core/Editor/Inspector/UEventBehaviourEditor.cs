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
    [CustomEditor(typeof(UEventBehaviour))]
    [CanEditMultipleObjects]
    public class UEventBehaviourEditor : InspectorEditor
    {
        private SerializedProperty e_MonoEvents;
        private SerializedProperty e_EnterTEvents;
        private SerializedProperty e_StayTEvents;
        private SerializedProperty e_ExitTEvents;

        private bool foldoutMono;
        private bool foldoutEnterT;
        private bool foldoutStayT;
        private bool foldoutExitT;

        private void OnEnable()
        {
            e_MonoEvents = serializedObject.FindProperty("monoEvent");
            e_EnterTEvents = serializedObject.FindProperty("enterTriggerEvent");
            e_StayTEvents = serializedObject.FindProperty("stayTriggerEvent");
            e_ExitTEvents = serializedObject.FindProperty("exitTriggerEvent");
            InitGUIStyle(FontStyle.Bold, TextAnchor.MiddleCenter);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            BeginPanel();
            Title("Event Behaviour");
            GUILayout.BeginVertical("Button");
            GUILayout.Space(7);
            EditorGUILayout.HelpBox("Event List (ReadOnly)", MessageType.Info);
            GUILayout.Space(3);
            GUILayout.BeginVertical("HelpBox");
            foldoutMono = EditorGUILayout.Foldout(foldoutMono, "Mono Events", true);
            if (foldoutMono)
                for (int i = 0; i < e_MonoEvents.arraySize; i++)
                {
                    Event curEvent = e_MonoEvents.GetArrayElementAtIndex(i).objectReferenceValue as Event;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Event " + (i + 1) + ":", GUILayout.Width(55));
                    GUILayout.Label(curEvent.EventName);
                    GUILayout.EndHorizontal();
                }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("HelpBox");
            foldoutEnterT = EditorGUILayout.Foldout(foldoutEnterT, "Enter Trigger Events", true);
            if (foldoutEnterT)
                for (int i = 0; i < e_EnterTEvents.arraySize; i++)
                {
                    Event curEvent = e_EnterTEvents.GetArrayElementAtIndex(i).objectReferenceValue as Event;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Event " + (i + 1) + ":", GUILayout.Width(55));
                    GUILayout.Label(curEvent.EventName);
                    GUILayout.EndHorizontal();
                }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("HelpBox");
            foldoutStayT = EditorGUILayout.Foldout(foldoutStayT, "Stay Trigger Events", true);
            if (foldoutStayT)
                for (int i = 0; i < e_StayTEvents.arraySize; i++)
                {
                    Event curEvent = e_StayTEvents.GetArrayElementAtIndex(i).objectReferenceValue as Event;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Event " + (i + 1) + ":", GUILayout.Width(55));
                    GUILayout.Label(curEvent.EventName);
                    GUILayout.EndHorizontal();
                }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("HelpBox");
            foldoutExitT = EditorGUILayout.Foldout(foldoutExitT, "Exit Trigger Events", true);
            if (foldoutExitT)
                for (int i = 0; i < e_ExitTEvents.arraySize; i++)
                {
                    Event curEvent = e_ExitTEvents.GetArrayElementAtIndex(i).objectReferenceValue as Event;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Event " + (i + 1) + ":", GUILayout.Width(55));
                    GUILayout.Label(curEvent.EventName);
                    GUILayout.EndHorizontal();
                }
            GUILayout.EndVertical();
            GUILayout.Space(7);
            GUILayout.EndVertical();
            EndPanel();
            serializedObject.ApplyModifiedProperties();
        }

    }
}