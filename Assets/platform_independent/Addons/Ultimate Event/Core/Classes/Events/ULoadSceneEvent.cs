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
using UnityEngine.SceneManagement;

namespace UltimateEvent
{
    /// <summary>
    /// Load Scene Event used for load scene in runtime
    /// </summary>
    [Serializable]
    [UltimateEvent("Scene/Load Scene", 3, false)]
    public class ULoadSceneEvent : Event
    {
        private const string ID = "Load Scene Event";
        public override string GetID { get { return ID; } }

        [SerializeField] [UEField] private string sceneName;
        [SerializeField] [UEField] private LoadSceneMode loadSceneMode;

        /// <summary>
        /// Loading scene
        /// </summary>
        public override void OnStart()
        {
            SceneManager.LoadScene(sceneName, loadSceneMode);
        }

        /// <summary>
        /// Return scene name
        /// </summary>
        public string SceneName
        {
            get
            {
                return sceneName;
            }

            set
            {
                sceneName = value;
            }
        }
    }
}