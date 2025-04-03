using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RayFire
{
    [Serializable]
    public class RFFade
    {
        public FadeType fadeType;
        [Range (0f, 20f)]
        public float sizeFilter;
        [Range (1f, 180f)]
        public float lifeTime;
        [Range (0f, 20f)]
        public float lifeVariation;
        [Range (1f, 20f)]
        public float fadeTime;

        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////

        // Constructor
        public RFFade()
        {
            fadeType      = FadeType.None;
            sizeFilter    = 0f;
            lifeTime      = 10f;
            lifeVariation = 3f;
            fadeTime      = 5f;
        }

        // Copy from
        public void CopyFrom (RFFade fading)
        {
            fadeType      = fading.fadeType;
            sizeFilter    = fading.sizeFilter;
            lifeTime      = fading.lifeTime;
            lifeVariation = fading.lifeVariation;
            fadeTime      = fading.fadeTime;
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////

        // Fading init from parent node
        public void FadingParent (RayfireRigid scr, List<RayfireRigid> fadeObjects)
        {
            // No fading
            if (fadeType == FadeType.None)
                return;

            // No objects
            if (fadeObjects.Count == 0)
                return;

            // Life time fix
            if (lifeTime < 1f)
                lifeTime = 1f;

            // Add Fade script and init fading
            for (int i = 0; i < fadeObjects.Count; i++)
            {
                // Size check
                if (sizeFilter > 0 && fadeObjects[i].limitations.bboxSize > sizeFilter)
                    continue;

                // Init fading
                fadeObjects[i].fading.FadingFragment (fadeObjects[i]);
            }
        }

        // Fading init for fragment objects
        void FadingFragment (RayfireRigid scr)
        {
            // Get final life time
            if (lifeVariation > 0)
                lifeTime += Random.Range (0f, lifeVariation);

            // Exclude from simulation and keep object in scene
            if (fadeType == FadeType.SimExclude)
                scr.StartCoroutine (FadeExcludeCor (scr));

            // Exclude from simulation, move under ground, destroy
            else if (fadeType == FadeType.MoveDown)
                scr.StartCoroutine (FadeMoveDownCor (scr));

            // Start scale down and destroy
            else if (fadeType == FadeType.ScaleDown)
                scr.StartCoroutine (FadeScaleDownCor (scr));

            // Destroy object
            else if (fadeType == FadeType.Destroy)
                RayfireMan.DestroyOp (scr.gameObject, scr.rootParent, lifeTime);
        }

        /// /////////////////////////////////////////////////////////
        /// Coroutines
        /// /////////////////////////////////////////////////////////

        // Exclude from simulation and keep object in scene
        IEnumerator FadeExcludeCor (RayfireRigid scr)
        {
            // Wait life time
            yield return new WaitForSeconds (lifeTime);

            // Stop simulation
            scr.DestroyRb (scr.physics.rigidBody);
            scr.DestroyCollider (scr.physics.meshCollider);
            scr.DestroyRigid (scr);
        }

        // Exclude from simulation, move under ground, destroy
        IEnumerator FadeMoveDownCor (RayfireRigid scr)
        {
            // Wait life time
            yield return new WaitForSeconds (lifeTime);

            // Stop simulation
            scr.DestroyCollider (scr.physics.meshCollider);
            scr.physics.rigidBody.WakeUp();

            // Wait while fall under ground
            yield return new WaitForSeconds (fadeTime);

            // Check if fragment is the last child in root and delete root as well
            RayfireMan.DestroyFragment (scr.gameObject, scr.rootParent);
        }

        // Exclude from simulation, move under ground, destroy
        IEnumerator FadeScaleDownCor (RayfireRigid scr)
        {
            // Wait life time
            yield return new WaitForSeconds (lifeTime);

            // Scale object down during fade time
            float   waitStep   = 0.04f;
            int     steps      = (int)(fadeTime / waitStep);
            Vector3 vectorStep = Vector3.one / steps;
            while (steps > 0)
            {
                steps--;
                scr.transform.localScale -= vectorStep;
                yield return new WaitForSeconds (waitStep);

                if (steps < 4)
                    RayfireMan.DestroyFragment (scr.gameObject, scr.rootParent);
            }
        }
    }
}