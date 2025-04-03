/* ================================================================
   ---------------------------------------------------
   Project   :    Ultimate Event
   Publisher :    Infinite Dawn
   Author    :    Tamerlan Favilevich
   ---------------------------------------------------
   Copyright © Tamerlan Favilevich 2017 - 2018 All rights reserved.
   ================================================================ */

using UnityEngine;
using UltimateEvent;
using Event = UltimateEvent.Event;

[System.Serializable]
[UltimateEvent("Debug/Debug Event", 8, false)]
public class UDebugEvent : Event
{
    private const string ID = "Debug Event";
    public override string GetID { get { return ID; } }

    private enum DebugLogType { Log, Warning, Error }
    [SerializeField] [UEField] private string log;
    [SerializeField] [UEField] private DebugLogType logType;

    public override void OnStart()
    {
        SetStart(true);
        switch (logType)
        {
            case DebugLogType.Log:
                Debug.Log(log);
                break;
            case DebugLogType.Warning:
                Debug.LogWarning(log);
                break;
            case DebugLogType.Error:
                Debug.LogError(log);
                break;
        }
        SetReady(true);
    }
}
