/* ================================================================
   ---------------------------------------------------
   Project   :    Ultimate Event
   Publisher :    Infinite Dawn
   Author    :    Tamerlan Favilevich
   ---------------------------------------------------
   Copyright © Tamerlan Favilevich 2017 - 2018 All rights reserved.
   ================================================================ */

using System.Collections.Generic;
using UnityEngine;

namespace UltimateEvent
{
    /// <summary>
    /// Base Ultimate Event Behaviour class
    /// </summary>
    public class UEventBehaviour : MonoBehaviour
    {
        #region Event Lists
        [SerializeField] private List<Event> monoEvent = new List<Event>(); //Mono Event List
        [SerializeField] private List<Event> enterTriggerEvent = new List<Event>(); //Trigger Enter Event List
        [SerializeField] private List<Event> stayTriggerEvent = new List<Event>(); //Trigger Stay Event List
        [SerializeField] private List<Event> exitTriggerEvent = new List<Event>(); //Trigger Exit Event List
        #endregion

        /// <summary>
        /// Initialization all events
        /// </summary>
        private void Start()
        {
            for (int i = 0; i < monoEvent.Count; i++)
                monoEvent[i].Initialization();
            for (int i = 0; i < enterTriggerEvent.Count; i++)
                enterTriggerEvent[i].Initialization();
            for (int i = 0; i < stayTriggerEvent.Count; i++)
                stayTriggerEvent[i].Initialization();
            for (int i = 0; i < exitTriggerEvent.Count; i++)
                exitTriggerEvent[i].Initialization();
        }

        /// <summary>
        /// Invoke all mono events
        /// Send to Mono events the current frame
        /// </summary>
        private void Update()
        {
            for (int i = 0; i < monoEvent.Count; i++)
            {
                OnStartEvent(monoEvent, i);
                OnTickEvent(monoEvent, i);
                OnEndEvent(monoEvent, i);
            }
        }

        /// <summary>
        /// Invoke all enter trigger events
        /// </summary>
        /// <param name="enterCollider"></param>
        private void OnTriggerEnter(Collider enterCollider)
        {
            for (int i = 0; i < enterTriggerEvent.Count; i++)
            {
                OnStartEvent(enterTriggerEvent, i);
                OnTickEvent(enterTriggerEvent, i);
                OnEndEvent(enterTriggerEvent, i);
            }
        }

        /// <summary>
        /// Invoke all stay trigger events
        /// Send to events the current frame
        /// </summary>
        /// <param name="stayCollider"></param>
        private void OnTriggerStay(Collider stayCollider)
        {
            for (int i = 0; i < stayTriggerEvent.Count; i++)
            {
                OnStartEvent(stayTriggerEvent, i);
                OnTickEvent(stayTriggerEvent, i);
                OnEndEvent(stayTriggerEvent, i);
            }
        }

        /// <summary>
        /// Invoke all exit trigger events
        /// </summary>
        /// <param name="exitCollider"></param>
        private void OnTriggerExit(Collider exitCollider)
        {
            for (int i = 0; i < exitTriggerEvent.Count; i++)
            {
                OnStartEvent(exitTriggerEvent, i);
                OnTickEvent(exitTriggerEvent, i);
                OnEndEvent(exitTriggerEvent, i);
            }
        }

        /// <summary>
        /// OnStart event logic
        /// </summary>
        /// <param name="events"></param>
        /// <param name="index"></param>
        private void OnStartEvent(List<Event> events, int index)
        {
            if (!events[index].IsStart && events[index].DelayEventsIsReady && (events[index].StartTime <= 0))
            {
                events[index].OnStart();
                events[index].SetStart(true);
            }
            else if ((events[index].StartTime > 0) && events[index].DelayEventsIsReady)
            {
                events[index].StartTime -= Time.deltaTime;
            }
        }

        /// <summary>
        /// OnTick event logic
        /// </summary>
        /// <param name="events"></param>
        /// <param name="index"></param>
        private void OnTickEvent(List<Event> events, int index)
        {
            if (events[index].IsStart && !events[index].IsReady)
            {
                events[index].Tick();
            }
        }

        /// <summary>
        /// OnEnd event logic
        /// </summary>
        /// <param name="events"></param>
        /// <param name="index"></param>
        private void OnEndEvent(List<Event> events, int index)
        {
            if (!events[index].IsReady && (events[index].EndTime != 100) && (events[index].EndTime <= 0))
            {
                events[index].OnEnd();
                events[index].SetReady(true);
            }
            else if ((events[index].EndTime != 100) && (events[index].EndTime > 0))
            {
                events[index].EndTime -= Time.deltaTime;
            }
        }

        /// <summary>
        /// Mono Event List
        /// </summary>
        public List<Event> MonoEvent { get { return monoEvent; } }

        /// <summary>
        /// Enter Trigger Event List
        /// </summary>
        public List<Event> EnterTriggerEvent { get { return enterTriggerEvent; } }

        /// <summary>
        /// Stay Trigger Event List
        /// </summary>
        public List<Event> StayTriggerEvent { get { return stayTriggerEvent; } }

        /// <summary>
        /// Exit Trigger Event List
        /// </summary>
        public List<Event> ExitTriggerEvent { get { return exitTriggerEvent; } }
    }
}