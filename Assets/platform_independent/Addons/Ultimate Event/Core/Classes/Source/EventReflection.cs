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
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UltimateEvent.Reflection
{
    /// <summary>
    /// Class used for get information about subclasses of Event
    /// Reflexive finds all created subclasses Events
    /// </summary>
    public class EventReflection
    {
        private static Type type = typeof(Event);
        private static Assembly assembly = Assembly.GetExecutingAssembly();

        /// <summary>
        /// Return all subclass type of Event
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Type> GetTypes()
        {
            Type[] types = assembly.GetTypes();
            IEnumerable<Type> subclasses = types.Where(t => t.IsSubclassOf(type));
            return subclasses;
        }

        /// <summary>
        /// Return all subclass of Event
        /// </summary>
        /// <returns></returns>
        public static List<Event> GetClasses()
        {
            List<Event> subClassNameList = new List<Event>();
            Type[] types = assembly.GetTypes();
            IEnumerable<Type> subclasses = types.Where(t => t.IsSubclassOf(type));
            foreach (Type type in subclasses)
            {
                Event subEvent = ScriptableObject.CreateInstance(type.FullName) as Event;
                subClassNameList.Add(subEvent);
            }
            return subClassNameList;
        }

        /// <summary>
        /// Return all subclass ID of Event
        /// </summary>
        /// <returns></returns>
        public static List<string> GetIDs()
        {
            List<string> subClassNameList = new List<string>();
            Type[] types = assembly.GetTypes();
            IEnumerable<Type> subclasses = types.Where(t => t.IsSubclassOf(type));
            foreach (Type type in subclasses)
            {
                Event subEvent = ScriptableObject.CreateInstance(type.FullName) as Event;
                string name = type.GetProperty("GetID").GetValue(subEvent, null) as string;
                subClassNameList.Add(name);
            }
            return subClassNameList;
        }

        /// <summary>
        /// Return all subclass name of Event
        /// </summary>
        /// <returns></returns>
        public static List<string> GetNames()
        {
            List<string> subClassNameList = new List<string>();
            Type[] types = assembly.GetTypes();
            IEnumerable<Type> subclasses = types.Where(t => t.IsSubclassOf(type));
            foreach (Type type in subclasses)
                subClassNameList.Add(type.FullName);
            return subClassNameList;
        }

        /// <summary>
        /// Create subclass of Event by name
        /// </summary>
        /// <returns>Event</returns>
        public static Event Create(string name)
        {
            Type[] types = assembly.GetTypes();
            IEnumerable<Type> subclasses = types.Where(t => t.IsSubclassOf(type));
            Event subEvent = null;
            foreach (Type type in subclasses)
                if (type.FullName == name)
                    subEvent = ScriptableObject.CreateInstance(type.FullName) as Event;
            return subEvent;
        }

        /// <summary>
        /// Get Ultimate Event Attribute from Event
        /// </summary>
        public static UltimateEventAttribute GetAttribute(Type t)
        {
            // Get instance of the attribute.
            UltimateEventAttribute MyAttribute =
                (UltimateEventAttribute) Attribute.GetCustomAttribute(t, typeof(UltimateEventAttribute));

            if (MyAttribute != null)
                return MyAttribute;
            else
                return null;
        }

        /// <summary>
        /// Returns all fields marked with the UEField attribute
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static FieldInfo[] GetFields(Type t)
        {
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
            IEnumerable<FieldInfo> fields = t.GetFields(flags).Where(field => field.IsDefined(typeof(UEFieldAttribute), false));
            return fields.ToArray();
        }
    }
}