// Created by Ronis Vision. All rights reserved
// 23.08.2019.

using System.Collections.Generic;
using UnityEngine;

namespace RVModules.RVSmartAI.Content.Scanners
{
    public interface IEnvironmentScanner
    {
        #region Public methods

        /// <summary>
        /// 
        /// </summary>
        List<Object> ScanEnvironment(Vector3 _position, float _range);

        #endregion
    }
}