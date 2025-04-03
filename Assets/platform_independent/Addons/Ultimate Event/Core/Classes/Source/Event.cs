/* ================================================================
   ---------------------------------------------------
   Project   :    Ultimate Event
   Publisher :    Infinite Dawn
   Author    :    Tamerlan Favilevich
   ---------------------------------------------------
   Copyright © Tamerlan Favilevich 2017 - 2018 All rights reserved.
   ================================================================ */

using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UltimateEvent.Reflection;
#endif

namespace UltimateEvent
{
    /// <summary>
    /// Base class for Events
    /// All custom events should extends from this class
    /// </summary>
    public abstract class Event : ScriptableObject
    {
        [SerializeField] private string eventName; //Event name
        [SerializeField] private float startTime = 0; //After how many seconds after the stratum event should begin
        [SerializeField] private float endTime = 100; //After how many seconds to stop the event after the start
        [SerializeField] private bool isStart = false; //Return true when event is running
        [SerializeField] private bool isReady = false; //Return true when event is end
        [SerializeField] private List<Event> eventDelay = new List<Event>(); //Events that must occur before the start of this event

        /// <summary>
        /// Event identification
        /// </summary>
        public abstract string GetID { get; }

        /// <summary>
        /// Called once when a script is enabled just before any of the Update methods is called the first time.
        /// Used for initialization variable, called at the MonoBehaviour Start()
        /// </summary>
        public virtual void Initialization() { }

        /// <summary>
        /// Called once when event start
        /// </summary>
        public virtual void OnStart() { }

        /// <summary>
        /// Called every frame while event running
        /// </summary>
        public virtual void Tick() { }

        /// <summary>
        /// Called once when event ending
        /// </summary>
        public virtual void OnEnd() { }

#if UNITY_EDITOR
        #region Base GUI

        #region Struct for base GUI
        private struct DelayEventParam
        {
            public string name;
            public Event dEvent;
        }
        #endregion

        #region Fields for base GUI
        private ReorderableList e_eventDelayList;
        private UEventBehaviour eventBehaviour;
        private SerializedObject serializedObject;
        private SerializedProperty[] serializedUEFields;
        private bool foldout;
        #endregion

        /// <summary>
        /// Called when the object becomes enabled and active.
        /// </summary>
        public virtual void OnEnable()
        {
            serializedObject = new SerializedObject(this);
            FieldInfo[] _UEFields = EventReflection.GetFields(GetType());
            serializedUEFields = new SerializedProperty[_UEFields.Length];
            for (int i = 0; i < _UEFields.Length; i++)
            {
                serializedUEFields[i] = serializedObject.FindProperty(_UEFields[i].Name);
            }
            e_eventDelayList = new ReorderableList(EventDelay, typeof(Event), true, true, true, true)
            {

                drawHeaderCallback = (rect) => { EditorGUI.LabelField(rect, "Delay Events"); },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 267, EditorGUIUtility.singleLineHeight), EventDelay[index].EventName);
                },

                onAddDropdownCallback = (rect, list) =>
                {
                    GenericMenu menu = new GenericMenu();
                    List<DelayEventParam> guids = GetAvalibleEvents();
                    for (int i = 0; i < guids.Count; i++)
                    {
                        menu.AddItem(new GUIContent(guids[i].name), false, (objects) => { EventDelay.Add((Event)objects); }, guids[i].dEvent);
                    }
                    menu.ShowAsContext();
                }
            };
        }

        /// <summary>
        /// Custom GUI which display on window
        /// </summary>
        public virtual void OnGUI()
        {
            HeaderGUILayout();
            FieldGUILayout();
        }

        /// <summary>
        /// Header GUI in Event Manager
        /// Used only in UnityEditor
        /// </summary>
        public virtual void HeaderGUILayout()
        {
            GUI.color = new Color32(211, 211, 211, 255);
            GUILayout.BeginVertical("HelpBox");
            GUI.color = Color.white;
            if (EditorApplication.isPlaying)
            {
                GUILayout.Label("Status:");
                GUILayout.Label("Event is start: " + IsStart);
                GUILayout.Label("Event is ready: " + IsReady);
            }
            else
            {
                GUILayout.Label("Status: not available in Editor Mode");
                GUILayout.Label("Event is start: -");
                GUILayout.Label("Event is ready: -");
            }
            if (startTime > 0)
                GUILayout.Label("Start time: " + startTime);
            else
                GUILayout.Label("Start time: Immediately");
            if (endTime <= 99.99)
                GUILayout.Label("End time: " + endTime);
            else
                GUILayout.Label("End time: Unlimited");
            GUILayout.BeginHorizontal();
            startTime = EditorGUILayout.FloatField(startTime, GUILayout.Width(30));
            EditorGUILayout.MinMaxSlider(ref startTime, ref endTime, 0, 100);
            if (endTime < 100)
                endTime = EditorGUILayout.FloatField(endTime, GUILayout.Width(30));
            else
                GUILayout.Label("∞", EditorStyles.textField, GUILayout.Width(30));
            GUILayout.EndHorizontal();
            foldout = EditorGUILayout.Foldout(foldout, "Event Delay", true);
            if (foldout)
                e_eventDelayList.DoLayoutList();
            EditorGUILayout.HelpBox("Start time: After how many seconds after the stratum event should begin\nEnd time: After how many seconds to stop the event after the start\nFor more information see the documentation", MessageType.Info);
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Field GUI draw all fields marked with the UEField attribute
        /// Used only in UnityEditor
        /// </summary>
        public virtual void FieldGUILayout()
        {
            serializedObject.Update();
            GUILayout.BeginVertical(EditorStyles.helpBox);
            for (int i = 0; i < serializedUEFields.Length; i++)
            {
                EditorGUILayout.PropertyField(serializedUEFields[i], true);
            }
            GUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Return all avalible created events on current UEventBehaviour
        /// </summary>
        /// <returns></returns>
        private List<DelayEventParam> GetAvalibleEvents()
        {
            List<DelayEventParam> list = new List<DelayEventParam>();
            if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<UEventBehaviour>() != null)
                eventBehaviour = Selection.activeGameObject.GetComponent<UEventBehaviour>();
            else
                return list;

            for (int i = 0; i < eventBehaviour.MonoEvent.Count; i++)
                if (this != eventBehaviour.MonoEvent[i])
                    list.Add(new DelayEventParam { name = "Mono Event/" + eventBehaviour.MonoEvent[i].EventName, dEvent = eventBehaviour.MonoEvent[i] });
            for (int i = 0; i < eventBehaviour.EnterTriggerEvent.Count; i++)
                if (this != eventBehaviour.EnterTriggerEvent[i])
                    list.Add(new DelayEventParam { name = "Enter Trigger Event/" + eventBehaviour.EnterTriggerEvent[i].EventName, dEvent = eventBehaviour.EnterTriggerEvent[i] });
            for (int i = 0; i < eventBehaviour.StayTriggerEvent.Count; i++)
                if (this != eventBehaviour.StayTriggerEvent[i])
                    list.Add(new DelayEventParam { name = "Stay Trigger Event/" + eventBehaviour.StayTriggerEvent[i].EventName, dEvent = eventBehaviour.StayTriggerEvent[i] });
            for (int i = 0; i < eventBehaviour.ExitTriggerEvent.Count; i++)
                if (this != eventBehaviour.ExitTriggerEvent[i])
                    list.Add(new DelayEventParam { name = "Exit Trigger Event/" + eventBehaviour.ExitTriggerEvent[i].EventName, dEvent = eventBehaviour.ExitTriggerEvent[i] });
            return list;
        }

        #endregion
#endif

        /// <summary>
        /// Set event start
        /// </summary>
        /// <param name="isStart"></param>
        public void SetStart(bool isStart)
        {
            this.isStart = isStart;
        }

        /// <summary>
        /// Set event ready
        /// </summary>
        /// <param name="isReady"></param>
        public void SetReady(bool isReady)
        {
            this.isReady = isReady;
        }

        /// <summary>
        /// Event name
        /// </summary>
        public string EventName { get { return eventName; } set { eventName = value; } }

        /// <summary>
        /// Return event is running
        /// </summary>
        public bool IsStart { get { return isStart; } }

        /// <summary>
        /// Return event is end
        /// </summary>
        public bool IsReady { get { return isReady; } }

        /// <summary>
        /// Return start time
        /// </summary>
        public float StartTime { get { return startTime; } set { startTime = value; } }

        /// <summary>
        /// Return end time
        /// </summary>
        public float EndTime { get { return endTime; } set { endTime = value; } }

        /// <summary>
        /// Events that must occur before the start of this event
        /// </summary>
        public List<Event> EventDelay { get { return eventDelay; } set { eventDelay = value; } }

        /// <summary>
        /// Return EventDelay is ready state
        /// </summary>
        public bool DelayEventsIsReady
        {
            get
            {
                for (int i = 0; i < eventDelay.Count; i++)
                {
                    if (!eventDelay[i].IsReady)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}