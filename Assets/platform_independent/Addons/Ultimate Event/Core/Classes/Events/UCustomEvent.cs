/* ================================================================
   ---------------------------------------------------
   Project   :    Ultimate Event
   Publisher :    Infinite Dawn
   Author    :    Tamerlan Favilevich
   ---------------------------------------------------
   Copyright © Tamerlan Favilevich 2017 - 2018 All rights reserved.
   ================================================================ */

using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace UltimateEvent
{
    [Serializable]
    public class AdvancedEvent : UnityEvent { }

    /// <summary>
    /// Custom event used base Unity Event system
    /// </summary>
    [Serializable]
    [UltimateEvent("Unity/UnityEvent", 2, false)]
    public class UCustomEvent : Event
    {
        private const string ID = "UnityEvent";
        public override string GetID { get { return ID; } }

        [SerializeField] [UEField] private AdvancedEvent advancedEvent = new AdvancedEvent();


        /// <summary>
        /// Start all custom events
        /// </summary>
        public override void OnStart()
        {
            SetStart(true);
            advancedEvent.Invoke();
            SetReady(true);
        }

        /// <summary>
        /// Return custom event
        /// </summary>
        public AdvancedEvent CustomEvent
        {
            get
            {
                return advancedEvent;
            }

            set
            {
                advancedEvent = value;
            }
        }
    }
}