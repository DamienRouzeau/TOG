// Created by Ronis Vision. All rights reserved
// 23.08.2019.

using UnityEngine;

namespace RVModules.RVSmartAI.Content.Scanners
{
    /// <summary>
    /// Used for scanners, so any component implementing IScannable can be fetched by scanners
    /// </summary>
    public interface IScannable
    {
        #region Properties

        /// <summary>
        /// Returns Unity Object reference of IScannable object
        /// This should be some main/root Object, usually root transform
        /// </summary>
        Object GetObject { get; }

        #endregion
    }
}