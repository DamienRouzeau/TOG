/* ================================================================
   ---------------------------------------------------
   Project   :    Ultimate Event
   Publisher :    Infinite Dawn
   Author    :    Tamerlan Favilevich
   ---------------------------------------------------
   Copyright © Tamerlan Favilevich 2017 - 2018 All rights reserved.
   ================================================================ */

using UnityEngine;

namespace UltimateEvent
{
    /// <summary>
    /// Base Ultimate Trigger class
    /// </summary>
    public class UltimateTrigger
    {
        /// <summary>
        /// Create new UTrigger
        /// </summary>
        /// <param name="uTriggerType"></param>
        public static void Create(UTriggerType uTriggerType)
        {
            GameObject UTrigger = new GameObject();
            UTrigger.name = "UltimateTrigger" + " (" + uTriggerType + ")";
            switch (uTriggerType)
            {
                case UTriggerType.Box:
                    UTrigger.AddComponent<BoxCollider>();
                    UTrigger.GetComponent<BoxCollider>().isTrigger = true;
                    break;
                case UTriggerType.Sphere:
                    UTrigger.AddComponent<SphereCollider>();
                    UTrigger.GetComponent<SphereCollider>().isTrigger = true;
                    break;
                case UTriggerType.Capluse:
                    UTrigger.AddComponent<CapsuleCollider>();
                    UTrigger.GetComponent<CapsuleCollider>().isTrigger = true;
                    break;
                case UTriggerType.Mesh:
                    UTrigger.AddComponent<MeshCollider>();
                    UTrigger.GetComponent<MeshCollider>().isTrigger = true;
                    break;
            }

            //Add Required Paramiters
            UTrigger.AddComponent<Rigidbody>();
            UTrigger.AddComponent<UEventBehaviour>();

            //Set Default Paramiters
            UTrigger.GetComponent<Rigidbody>().useGravity = false;
        }

        /// <summary>
        /// Create new UTrigger
        /// </summary>
        /// <param name="name"></param>
        /// <param name="uTriggerType"></param>
        public static void Create(string name, UTriggerType uTriggerType)
        {
            GameObject UTrigger = new GameObject();
            UTrigger.name = name;
            switch (uTriggerType)
            {
                case UTriggerType.Box:
                    UTrigger.AddComponent<BoxCollider>();
                    UTrigger.GetComponent<BoxCollider>().isTrigger = true;
                    break;
                case UTriggerType.Sphere:
                    UTrigger.AddComponent<SphereCollider>();
                    UTrigger.GetComponent<SphereCollider>().isTrigger = true;
                    break;
                case UTriggerType.Capluse:
                    UTrigger.AddComponent<CapsuleCollider>();
                    UTrigger.GetComponent<CapsuleCollider>().isTrigger = true;
                    break;
                case UTriggerType.Mesh:
                    UTrigger.AddComponent<MeshCollider>();
                    UTrigger.GetComponent<MeshCollider>().isTrigger = true;
                    break;
            }

            //Add Required Paramiters
            UTrigger.AddComponent<Rigidbody>();
            UTrigger.AddComponent<UEventBehaviour>();

            //Set Default Paramiters
            UTrigger.GetComponent<Rigidbody>().useGravity = false;
        }

        /// <summary>
        /// Convert object in UTrigger
        /// </summary>
        public static void ConvertObject(GameObject gameObject, UTriggerType uTriggerType)
        {
            //Add Required Paramiters
            switch (uTriggerType)
            {
                case UTriggerType.Box:
                    if (!gameObject.GetComponent<BoxCollider>())
                        gameObject.AddComponent<BoxCollider>();
                    gameObject.GetComponent<BoxCollider>().isTrigger = true;
                    break;
                case UTriggerType.Sphere:
                    if (!gameObject.GetComponent<SphereCollider>())
                        gameObject.AddComponent<SphereCollider>();
                    gameObject.GetComponent<SphereCollider>().isTrigger = true;
                    break;
                case UTriggerType.Capluse:
                    if (!gameObject.GetComponent<CapsuleCollider>())
                        gameObject.AddComponent<CapsuleCollider>();
                    gameObject.GetComponent<CapsuleCollider>().isTrigger = true;
                    break;
                case UTriggerType.Mesh:
                    if (!gameObject.GetComponent<MeshCollider>())
                        gameObject.AddComponent<MeshCollider>();
                    gameObject.GetComponent<MeshCollider>().isTrigger = true;
                    break;
            }
            if (!gameObject.GetComponent<Rigidbody>())
                gameObject.AddComponent<Rigidbody>();
            if (!gameObject.GetComponent<UEventBehaviour>())
                gameObject.AddComponent<UEventBehaviour>();
            gameObject.GetComponent<Rigidbody>().useGravity = false;
        }

        /// <summary>
        /// Find UTrigger by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static UEventBehaviour Find(string name)
        {
            return GameObject.Find(name).GetComponent<UEventBehaviour>();
        }
    }
}