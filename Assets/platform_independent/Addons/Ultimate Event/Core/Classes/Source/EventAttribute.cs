/* ================================================================
   ---------------------------------------------------
   Project   :    Ultimate Event
   Publisher :    Infinite Dawn
   Author    :    Tamerlan Favilevich
   ---------------------------------------------------
   Copyright © Tamerlan Favilevich 2017 - 2018 All rights reserved.
   ================================================================ */

using System;

namespace UltimateEvent
{
    /// <summary>
    /// Base event attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class UltimateEventAttribute : Attribute
    {
        private string name;    //Event name
        private int position;   //Event positon in list
        private bool hide;      //Hide event in list

        /// <summary>
        /// Constructor
        /// </summary>
        public UltimateEventAttribute(string name, int position, bool hide)
        {
            this.name = name;
            this.position = position;
            this.hide = hide;
        }

        /// <summary>
        /// Return event name
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Return event position in list
        /// </summary>
        public int Position { get { return position; } }

        /// <summary>
        /// Return event is hide in list
        /// </summary>
        public bool Hide { get { return hide; } }
    }

    /// <summary>
    /// UEProperty Attribute used for marking Fields which shound visible in Editor Manager
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class UEFieldAttribute : Attribute
    {
    }
}