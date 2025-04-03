/* ================================================================
   ---------------------------------------------------
   Project   :    Ultimate Event
   Publisher :    Infinite Dawn
   Author    :    Tamerlan Favilevich
   ---------------------------------------------------
   Copyright © Tamerlan Favilevich 2017 - 2018 All rights reserved.
   ================================================================ */

using System.Collections.Generic;
using System.Linq;
using UltimateEvent.Utility;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UltimateEvent.Reflection;

namespace UltimateEvent.Editor
{
    /// <summary>
    /// Used for identify selected event
    /// </summary>
    public struct EventParam
    {
        public string listType;
        public int index;
        public Event uevent;
    }

    /// <summary>
    /// Ultimate Event wizard window
    /// </summary>
    public class EventManager : EditorWindow
    {
        #region EditorElements
        private UEventBehaviour ultimateEvent; //Selected Ultimate Event Behaviour
        private EventParam selectedParam; //Selected Event Paramiters
        private bool[] foldouts = new bool[4]; //Foldout array
        private bool isEditPressed;
        private string buttonName;
        private int currentID;
        #endregion

        #region ReorderableList
        private ReorderableList monoEventList;
        private ReorderableList enterEventList;
        private ReorderableList stayEventList;
        private ReorderableList exitEventList;
        #endregion

        #region GUIElements
        private GUIStyle style = new GUIStyle();
        private Vector2 scrollPosition;
        #endregion

        

        /// <summary>
        /// Called once when window initialization
        /// </summary>
        [MenuItem("Ultimate Event/Event Manager", false, 121)]
        private static void InitWindow()
        {
            // Get existing open window or if none, make a new one:
            EventManager window = (EventManager)GetWindow(typeof(EventManager));
            window.titleContent.text = "Event Manager";
            window.Show();
        }

        /// <summary>
        /// Called when the object becomes enabled and active
        /// </summary>
        private void OnEnable()
        {
            #region Initialization GUIStyle
            style.alignment = TextAnchor.MiddleCenter;
            #endregion
        }

        /// <summary>
        /// Called when the window gets keyboard/mouse focus
        /// </summary>
        private void OnFocus()
        {
            #region Initialization Selection Ultimate Event
            if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<UEventBehaviour>() != null)
                ultimateEvent = Selection.activeGameObject.GetComponent<UEventBehaviour>();
            else
                return;
            #endregion

            #region Initialization ReorderableLists
            monoEventList = new ReorderableList(ultimateEvent.MonoEvent, typeof(Event), true, true, true, true)
            {
                drawHeaderCallback = (rect) => { EditorGUI.LabelField(rect, "Mono Events"); },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    string element = ultimateEvent.MonoEvent[index].EventName;
                    rect.y += 2;
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 267, EditorGUIUtility.singleLineHeight), element);
                    if (isFocused) selectedParam = new EventParam { listType = "Mono", index = index };
                },

                onRemoveCallback = (reorderableList) =>
                {
                    selectedParam = new EventParam { listType = "Mono", index = 0 };
                    reorderableList.list.RemoveAt(reorderableList.index);
                },

                onAddDropdownCallback = (rect, list) =>
                {
                    GenericMenu menu = new GenericMenu();
                    Dictionary<string, Event> guids = GenerateGuids();
                    for (int i = 0; i < guids.Count; i++)
                    {
                        KeyValuePair<string, Event> kpv = guids.ElementAt(i);
                        menu.AddItem(new GUIContent(kpv.Key), false, GenericMenuFunction, new EventParam() { listType = "Mono", index = i, uevent = kpv.Value });
                    }

                    menu.ShowAsContext();
                }
            };

            enterEventList = new ReorderableList(ultimateEvent.EnterTriggerEvent, typeof(Event), true, true, true, true)
            {
                drawHeaderCallback = (rect) => { EditorGUI.LabelField(rect, "Enter Trigger Events"); },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    string element = ultimateEvent.EnterTriggerEvent[index].EventName;
                    rect.y += 2;
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 267, EditorGUIUtility.singleLineHeight), element);
                    if (isFocused) selectedParam = new EventParam { listType = "EnterTrigger", index = index };
                },

                onRemoveCallback = (reorderableList) =>
                {
                    selectedParam = new EventParam { listType = "EnterTrigger", index = 0 };
                    reorderableList.list.RemoveAt(reorderableList.index);
                },

                onAddDropdownCallback = (rect, list) =>
                {
                    GenericMenu menu = new GenericMenu();
                    Dictionary<string, Event> guids = GenerateGuids();
                    for (int i = 0; i < guids.Count; i++)
                    {
                        KeyValuePair<string, Event> kpv = guids.ElementAt(i);
                        menu.AddItem(new GUIContent(kpv.Key), false, GenericMenuFunction, new EventParam() { listType = "EnterTrigger", index = i, uevent = kpv.Value });
                    }
                    menu.ShowAsContext();
                }
            };

            stayEventList = new ReorderableList(ultimateEvent.StayTriggerEvent, typeof(Event), true, true, true, true)
            {
                drawHeaderCallback = (rect) => { EditorGUI.LabelField(rect, "Stay Trigger Events"); },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    string element = ultimateEvent.StayTriggerEvent[index].EventName;
                    rect.y += 2;
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 267, EditorGUIUtility.singleLineHeight), element);
                    if (isFocused) selectedParam = new EventParam { listType = "StayTrigger", index = index };
                },

                onRemoveCallback = (reorderableList) =>
                {
                    selectedParam = new EventParam { listType = "StayTrigger", index = 0 };
                    reorderableList.list.RemoveAt(reorderableList.index);
                },

                onAddDropdownCallback = (rect, list) =>
                {
                    GenericMenu menu = new GenericMenu();
                    Dictionary<string, Event> guids = GenerateGuids();
                    for (int i = 0; i < guids.Count; i++)
                    {
                        KeyValuePair<string, Event> kpv = guids.ElementAt(i);
                        menu.AddItem(new GUIContent(kpv.Key), false, GenericMenuFunction, new EventParam() { listType = "StayTrigger", index = i, uevent = kpv.Value });
                    }
                    menu.ShowAsContext();
                }
            };

            exitEventList = new ReorderableList(ultimateEvent.ExitTriggerEvent, typeof(Event), true, true, true, true)
            {
                drawHeaderCallback = (rect) => { EditorGUI.LabelField(rect, "Exit Trigger Events"); },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    string element = ultimateEvent.ExitTriggerEvent[index].EventName;
                    rect.y += 2;
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 267, EditorGUIUtility.singleLineHeight), element);
                    if (isFocused) selectedParam = new EventParam { listType = "ExitTrigger", index = index };
                },

                onRemoveCallback = (reorderableList) =>
                {
                    selectedParam = new EventParam { listType = "ExitTrigger", index = 0 };
                    reorderableList.list.RemoveAt(reorderableList.index);
                },

                onAddDropdownCallback = (rect, list) =>
                {
                    GenericMenu menu = new GenericMenu();
                    Dictionary<string, Event> guids = GenerateGuids();
                    for (int i = 0; i < guids.Count; i++)
                    {
                        KeyValuePair<string, Event> kpv = guids.ElementAt(i);
                        menu.AddItem(new GUIContent(kpv.Key), false, GenericMenuFunction, new EventParam() { listType = "ExitTrigger", index = i, uevent = kpv.Value });
                    }
                    menu.ShowAsContext();
                }
            };
            #endregion
        }

        /// <summary>
        /// Called for rendering and handling GUI events
        /// </summary>
        private void OnGUI()
        {
            #region Called when Ultimate Event null
            if (ultimateEvent == null)
            {
                GUILayout.Space(2);
                GUILayout.BeginVertical(EditorStyles.toolbar);
                GUILayout.Space(2);
                GUILayout.Label("Select Ultimate Event Behaviour...", style);
                GUILayout.EndVertical();
                return;
            }
            #endregion

            #region Drawing Ultimate Event Editor
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(EditorStyles.toolbar);
            GUILayout.Space(2);
            GUILayout.Label("Event Behaviour: " + ultimateEvent.gameObject.name, style);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(300), GUILayout.Height(Screen.height - 67));
            GUI.color = new Color32(211, 211, 211, 255);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.color = Color.white;
            foldouts[0] = EditorGUILayout.Foldout(foldouts[0], "Mono Events", true);
            if (foldouts[0])
                monoEventList.DoLayoutList();
            GUILayout.EndVertical();

            GUI.color = new Color32(211, 211, 211, 255);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.color = Color.white;
            foldouts[1] = EditorGUILayout.Foldout(foldouts[1], "Enter Trigger Events", true);
            if (foldouts[1])
                enterEventList.DoLayoutList();
            GUILayout.EndVertical();

            GUI.color = new Color32(211, 211, 211, 255);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.color = Color.white;
            foldouts[2] = EditorGUILayout.Foldout(foldouts[2], "Stay Trigger Events", true);
            if (foldouts[2])
                stayEventList.DoLayoutList();
            GUILayout.EndVertical();

            GUI.color = new Color32(211, 211, 211, 255);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.color = Color.white;
            foldouts[3] = EditorGUILayout.Foldout(foldouts[3], "Exit Trigger Events", true);
            if (foldouts[3])
                exitEventList.DoLayoutList();
            GUILayout.EndVertical();
            GUI.color = Color.white;
            GUILayout.EndVertical();

            GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Height(Screen.height - 67));
            EventTitleGUI();
            switch (selectedParam.listType)
            {
                case "Mono":
                    if (ultimateEvent.MonoEvent.Count > 0)
                        ultimateEvent.MonoEvent[selectedParam.index].OnGUI();
                    break;
                case "EnterTrigger":
                    if (ultimateEvent.EnterTriggerEvent.Count > 0)
                        ultimateEvent.EnterTriggerEvent[selectedParam.index].OnGUI();
                    break;
                case "StayTrigger":
                    if (ultimateEvent.StayTriggerEvent.Count > 0)
                        ultimateEvent.StayTriggerEvent[selectedParam.index].OnGUI();
                    break;
                case "ExitTrigger":
                    if (ultimateEvent.ExitTriggerEvent.Count > 0)
                        ultimateEvent.ExitTriggerEvent[selectedParam.index].OnGUI();
                    break;
                default:
                    GUILayout.Label("Select Event...", EditorStyles.centeredGreyMiniLabel);
                    break;
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
            #endregion
        }

        /// <summary>
        /// Generic menu function
        /// </summary>
        /// <param name="target"></param>
        private void GenericMenuFunction(object target)
        {
            EventParam createParam = (EventParam)target;
            Event newEvent = createParam.uevent;
            newEvent.EventName = newEvent.GetID;
            switch (createParam.listType)
            {
                case "Mono":
                    ultimateEvent.MonoEvent.Add(newEvent);
                    break;
                case "EnterTrigger":
                    ultimateEvent.EnterTriggerEvent.Add(newEvent);
                    break;
                case "StayTrigger":
                    ultimateEvent.StayTriggerEvent.Add(newEvent);
                    break;
                case "ExitTrigger":
                    ultimateEvent.ExitTriggerEvent.Add(newEvent);
                    break;
            }
        }

        private void EventTitleGUI()
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (selectedParam.listType == "Mono")
            {
                if (ultimateEvent.MonoEvent.Count > 0)
                {
                    if (!isEditPressed)
                    {
                        GUILayout.Label("Event: " + ultimateEvent.MonoEvent[selectedParam.index].EventName, EditorStyles.boldLabel);
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("New Event Name");
                        GUILayout.Space(50);
                        ultimateEvent.MonoEvent[selectedParam.index].EventName = EditorGUILayout.TextField(ultimateEvent.MonoEvent[selectedParam.index].EventName);
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.FlexibleSpace();
                    buttonName = isEditPressed ? "Apply" : "Edit";
                    if (GUILayout.Button(buttonName, EditorStyles.miniButtonLeft, GUILayout.Height(17)))
                    {
                        isEditPressed = !isEditPressed;
                    }
                    currentID = EditorGUIUtility.GetControlID(FocusType.Passive) + 100;
                    if (GUILayout.Button("Import", EditorStyles.miniButtonMid, GUILayout.Height(17)))
                    {
                        EditorGUIUtility.ShowObjectPicker<ScriptableObject>(null, false, "", currentID);
                    }
                    if (UnityEngine.Event.current.commandName == "ObjectSelectorClosed" && EditorGUIUtility.GetObjectPickerControlID() == currentID)
                    {
                        currentID = -1;
                        if (EditorGUIUtility.GetObjectPickerObject() != null)
                        {
                            if (EditorGUIUtility.GetObjectPickerObject() as Event)
                            {
                                ultimateEvent.MonoEvent[selectedParam.index] = (Event)EditorGUIUtility.GetObjectPickerObject();
                            }
                            else
                            {
                                Debug.Log("Selected Element is not Event");
                            }
                        }
                    }
                    if (GUILayout.Button("Export", EditorStyles.miniButtonRight, GUILayout.Height(17)))
                    {
                        ScriptableObjectUtility.CreateAsset(ultimateEvent.MonoEvent[selectedParam.index].GetType().FullName, ultimateEvent.MonoEvent[selectedParam.index].EventName);
                    }
                }
            }
            if (selectedParam.listType == "EnterTrigger")
            {
                if (ultimateEvent.EnterTriggerEvent.Count > 0)
                {
                    if (!isEditPressed)
                    {
                        GUILayout.Label("Event: " + ultimateEvent.EnterTriggerEvent[selectedParam.index].EventName, EditorStyles.boldLabel);
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("New Event Name");
                        GUILayout.Space(50);
                        ultimateEvent.EnterTriggerEvent[selectedParam.index].EventName = EditorGUILayout.TextField(ultimateEvent.EnterTriggerEvent[selectedParam.index].EventName);
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.FlexibleSpace();
                    buttonName = isEditPressed ? "Apply" : "Edit";
                    if (GUILayout.Button(buttonName, EditorStyles.miniButtonLeft, GUILayout.Height(17)))
                    {
                        isEditPressed = !isEditPressed;
                    }
                    currentID = EditorGUIUtility.GetControlID(FocusType.Passive) + 100;
                    if (GUILayout.Button("Import", EditorStyles.miniButtonMid, GUILayout.Height(17)))
                    {
                        EditorGUIUtility.ShowObjectPicker<ScriptableObject>(null, false, "", currentID);
                    }
                    if (UnityEngine.Event.current.commandName == "ObjectSelectorClosed" && EditorGUIUtility.GetObjectPickerControlID() == currentID)
                    {
                        currentID = -1;
                        if (EditorGUIUtility.GetObjectPickerObject() != null)
                        {
                            if (EditorGUIUtility.GetObjectPickerObject() as Event)
                            {
                                ultimateEvent.EnterTriggerEvent[selectedParam.index] = (Event)EditorGUIUtility.GetObjectPickerObject();
                            }
                            else
                            {
                                Debug.Log("Selected Element is not Event");
                            }
                        }
                    }
                    if (GUILayout.Button("Export", EditorStyles.miniButtonRight, GUILayout.Height(17)))
                    {
                        ScriptableObjectUtility.CreateAsset(ultimateEvent.EnterTriggerEvent[selectedParam.index].GetType().FullName, ultimateEvent.EnterTriggerEvent[selectedParam.index].EventName);
                    }
                }
            }
            if (selectedParam.listType == "StayTrigger")
            {
                if (ultimateEvent.StayTriggerEvent.Count > 0)
                {
                    if (!isEditPressed)
                    {
                        GUILayout.Label("Event: " + ultimateEvent.StayTriggerEvent[selectedParam.index].EventName, EditorStyles.boldLabel);
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("New Event Name");
                        GUILayout.Space(50);
                        ultimateEvent.StayTriggerEvent[selectedParam.index].EventName = EditorGUILayout.TextField(ultimateEvent.StayTriggerEvent[selectedParam.index].EventName);
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.FlexibleSpace();
                    buttonName = isEditPressed ? "Apply" : "Edit";
                    if (GUILayout.Button(buttonName, EditorStyles.miniButtonLeft, GUILayout.Height(17)))
                    {
                        isEditPressed = !isEditPressed;
                    }
                    currentID = EditorGUIUtility.GetControlID(FocusType.Passive) + 100;
                    if (GUILayout.Button("Import", EditorStyles.miniButtonMid, GUILayout.Height(17)))
                    {
                        EditorGUIUtility.ShowObjectPicker<ScriptableObject>(null, false, "", currentID);
                    }
                    if (UnityEngine.Event.current.commandName == "ObjectSelectorClosed" && EditorGUIUtility.GetObjectPickerControlID() == currentID)
                    {
                        currentID = -1;
                        if (EditorGUIUtility.GetObjectPickerObject() != null)
                        {
                            if (EditorGUIUtility.GetObjectPickerObject() as Event)
                            {
                                ultimateEvent.StayTriggerEvent[selectedParam.index] = (Event)EditorGUIUtility.GetObjectPickerObject();
                            }
                            else
                            {
                                Debug.Log("Selected Element is not Event");
                            }
                        }
                    }
                    if (GUILayout.Button("Export", EditorStyles.miniButtonRight, GUILayout.Height(17)))
                    {
                        ScriptableObjectUtility.CreateAsset(ultimateEvent.StayTriggerEvent[selectedParam.index].GetType().FullName, ultimateEvent.StayTriggerEvent[selectedParam.index].EventName);
                    }
                }
            }
            if (selectedParam.listType == "ExitTrigger")
            {
                if (ultimateEvent.ExitTriggerEvent.Count > 0)
                {
                    if (!isEditPressed)
                    {
                        GUILayout.Label("Event: " + ultimateEvent.ExitTriggerEvent[selectedParam.index].EventName, EditorStyles.boldLabel);
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("New Event Name");
                        GUILayout.Space(50);
                        ultimateEvent.ExitTriggerEvent[selectedParam.index].EventName = EditorGUILayout.TextField(ultimateEvent.ExitTriggerEvent[selectedParam.index].EventName);
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.FlexibleSpace();
                    buttonName = isEditPressed ? "Apply" : "Edit";
                    if (GUILayout.Button(buttonName, EditorStyles.miniButtonLeft, GUILayout.Height(17)))
                    {
                        isEditPressed = !isEditPressed;
                    }
                    currentID = EditorGUIUtility.GetControlID(FocusType.Passive) + 100;
                    if (GUILayout.Button("Import", EditorStyles.miniButtonMid, GUILayout.Height(17)))
                    {
                        EditorGUIUtility.ShowObjectPicker<ScriptableObject>(null, false, "", currentID);
                    }
                    if (UnityEngine.Event.current.commandName == "ObjectSelectorClosed" && EditorGUIUtility.GetObjectPickerControlID() == currentID)
                    {
                        currentID = -1;
                        if (EditorGUIUtility.GetObjectPickerObject() != null)
                        {
                            if (EditorGUIUtility.GetObjectPickerObject() as Event)
                            {
                                ultimateEvent.ExitTriggerEvent[selectedParam.index] = (Event)EditorGUIUtility.GetObjectPickerObject();
                            }
                            else
                            {
                                Debug.Log("Selected Element is not Event");
                            }
                        }
                    }
                    if (GUILayout.Button("Export", EditorStyles.miniButtonRight, GUILayout.Height(17)))
                    {
                        ScriptableObjectUtility.CreateAsset(ultimateEvent.ExitTriggerEvent[selectedParam.index].GetType().FullName, ultimateEvent.ExitTriggerEvent[selectedParam.index].EventName);
                    }
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        /// <summary>
        /// Guids generator
        /// </summary>
        private Dictionary<string, Event> GenerateGuids()
        {
            int count = EventReflection.GetClasses().Count;
            Dictionary<string, Event> guids = new Dictionary<string, Event>();
            List<Event> events = EventReflection.GetClasses();
            List<int> positions = new List<int>();
            for (int i = 0; i < count; i++)
            {
                positions.Add(EventReflection.GetAttribute(events[i].GetType()).Position);
            }
            positions.Sort();
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    if (!EventReflection.GetAttribute(events[j].GetType()).Hide && (positions[i] == EventReflection.GetAttribute(events[j].GetType()).Position))
                    {
                        guids.Add(EventReflection.GetAttribute(events[j].GetType()).Name, events[j]);
                    }
                }
            }
            return guids;
        }
    }
}