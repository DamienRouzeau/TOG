using UnityEngine;
using UnityEditor;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireShatter))]
    public class RayfireShatterEditor : Editor
    {
        // Draw gizmo
        [DrawGizmo (GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawGizmosSelected (RayfireShatter shatter, GizmoType gizmoType)
        {
            // Color preview
            if (shatter.colorPreview == true)
                ColorPreview (shatter);

            /*        // Custom point cloud preview
                    if (shatter.type == FragType.Custom)
                    {
                        if (shatter.custom.localPoints.Length > 0)
                        {
                            // Gizmo properties
                            Gizmos.color = Color.green;
                            Gizmos.matrix = shatter.transform.localToWorldMatrix;
            
                            // Gizmo center line
                            for (int i = 0; i < shatter.custom.localPoints.Length; i++)
                                Gizmos.DrawSphere(shatter.custom.localPoints[i], 0.03f);
                        }
                    }*/
        }

        // Preview variables
        bool resetState = false;

        // Show center move handle
        private void OnSceneGUI()
        {
            // Get shatter
            RayfireShatter shatter   = target as RayfireShatter;
            Transform      transform = shatter.transform;

            // Point3 handle
            if (shatter.showCenter == true)
            {
                Quaternion oldRotation = transform.rotation * shatter.centerDirection;
                EditorGUI.BeginChangeCheck();
                Quaternion newRotation = Handles.RotationHandle (oldRotation, transform.TransformPoint (shatter.centerPosition));
                if (EditorGUI.EndChangeCheck() == true)
                    Undo.RecordObject (shatter, "Center Rotate");

                //oldRotation.Normalize();
                //newRotation.Normalize();



                shatter.centerDirection = Quaternion.Inverse (transform.rotation) * newRotation;

                //shatter.centerDirection.Normalize();

                // Move center handle
                EditorGUI.BeginChangeCheck();
                Vector3 centerHandle = Handles.PositionHandle (transform.TransformPoint (shatter.centerPosition), newRotation);
                shatter.centerPosition = transform.InverseTransformPoint (centerHandle);
                if (EditorGUI.EndChangeCheck() == true)
                    Undo.RecordObject (shatter, "Center Move");

                shatter.centerRotation = shatter.centerDirection.eulerAngles;
            }
        }

        public override void OnInspectorGUI()
        {
            // Get shatter
            RayfireShatter shatter = target as RayfireShatter;

            // Get inspector width
            // float width = EditorGUIUtility.currentViewWidth - 20f;

            // Space
            GUILayout.Space (8);

            // Fragment 
            if (GUILayout.Button ("Fragment", GUILayout.Height (25)))
            {
                // TODO cHECK IF OBJECTS WAS MOVED

                foreach (var targ in targets)
                    (targ as RayfireShatter).Fragment();

                // Scale preview if preview turn on
                if (shatter.previewScale > 0 && shatter.scalePreview == true)
                    ScalePreview (shatter);
            }

            // Space
            GUILayout.Space (1);

            // Fragmentation section Begin
            GUILayout.BeginHorizontal();

            // Delete last
            if (GUILayout.Button (" Delete Last ", GUILayout.Height (22)))
            {
                foreach (var targ in targets)
                    (targ as RayfireShatter).DeleteFragmentsLast();
                resetState = true;
                ResetScale (shatter, 0f);
            }

            // Delete all fragments
            if (GUILayout.Button (" Delete All ", GUILayout.Height (22)))
            {
                foreach (var targ in targets)
                    (targ as RayfireShatter).DeleteFragmentsAll();
                resetState = true;
                ResetScale (shatter, 0f);
            }

            // Fragmentation section End
            EditorGUILayout.EndHorizontal();

            // Space
            GUILayout.Space (1);

            // Preview section Begin
            GUILayout.BeginHorizontal();

            // Show center toggle
            shatter.showCenter = GUILayout.Toggle (shatter.showCenter, " Show Center  ", "Button");

            // Reset center
            if (GUILayout.Button ("Reset Center"))
            {
                foreach (var targ in targets)
                    (targ as RayfireShatter).ResetCenter();
            }

            // Preview section End
            EditorGUILayout.EndHorizontal();

            // Space
            GUILayout.Space (1);

            // Preview toggles begin
            GUILayout.BeginHorizontal();

            // Start check for scale toggle change
            EditorGUI.BeginChangeCheck();
            shatter.scalePreview = GUILayout.Toggle (shatter.scalePreview, " Scale Preview ", "Button");
            if (EditorGUI.EndChangeCheck() == true)
            {
                if (shatter.scalePreview == true)
                    ScalePreview (shatter);
                else
                {
                    resetState = true;
                    ResetScale (shatter, 0f);
                }
            }

            // Color preview toggle
            shatter.colorPreview = GUILayout.Toggle (shatter.colorPreview, "Color Preview", "Button");

            // Preview toggles end
            EditorGUILayout.EndHorizontal();

            // Space
            GUILayout.Space (3);

            // Preview section Begin
            GUILayout.BeginHorizontal();

            // Label
            GUILayout.Label ("Scale Preview", GUILayout.Width (90));

            // Start check for slider change
            EditorGUI.BeginChangeCheck();
            shatter.previewScale = GUILayout.HorizontalSlider (shatter.previewScale, 0f, 0.99f);
            if (EditorGUI.EndChangeCheck() == true)
                if (shatter.scalePreview == true)
                    ScalePreview (shatter);

            // Reset scale if fragments were deleted
            ResetScale (shatter, shatter.previewScale);

            // Preview section End
            EditorGUILayout.EndHorizontal();

            // Space
            GUILayout.Space (3);

            if (shatter.fragmentsLast.Count > 0)
            {
                // Info section Begin
                GUILayout.BeginHorizontal();

                // Label
                GUILayout.Label ("Roots: " + shatter.rootChildList.Count);

                // Label
                GUILayout.Label ("Last Fragments: " + shatter.fragmentsLast.Count);

                // Label
                GUILayout.Label ("Total Fragments: " + shatter.fragmentsAll.Count);
                
                // Info section End
                EditorGUILayout.EndHorizontal();

                // Space
                GUILayout.Space (3);
            }

            // Draw script UI
            DrawDefaultInspector();

            GUILayout.Space (5);

            // Changelog check
            if (GUILayout.Button ("Get FBX Exporter", GUILayout.Height (20)))
                Application.OpenURL ("https://assetstore.unity.com/packages/essentials/fbx-exporter-101408");

        }

        // Reset original object and fragments scale
        void ResetScale (RayfireShatter shatter, float scaleValue)
        {
            // Reset scale
            if (resetState == true && scaleValue == 0f)
            {
                if (shatter.skinnedMeshRend != null)
                    shatter.skinnedMeshRend.enabled = true;

                if (shatter.meshRenderer != null)
                    shatter.meshRenderer.enabled = true;

                if (shatter.fragmentsLast.Count > 0)
                    foreach (GameObject fragment in shatter.fragmentsLast)
                        if (fragment != null)
                            fragment.transform.localScale = Vector3.one;

                resetState = false;
            }
        }

        // Scale fragments
        void ScalePreview (RayfireShatter shatter)
        {
            if (shatter.fragmentsLast.Count > 0 && shatter.previewScale > 0f)
            {
                // Do not scale
                if (shatter.skinnedMeshRend != null)
                    shatter.skinnedMeshRend.enabled = false;
                if (shatter.meshRenderer != null)
                    shatter.meshRenderer.enabled = false;

                foreach (GameObject fragment in shatter.fragmentsLast)
                    if (fragment != null)
                        fragment.transform.localScale = Vector3.one * Mathf.Lerp (1f, 0.6f, shatter.previewScale);
                resetState = true;
            }

            if (shatter.previewScale == 0f)
            {
                ResetScale (shatter, 0f);
            }
        }

        // Color preview
        static void ColorPreview (RayfireShatter shatter)
        {
            if (shatter.fragmentsLast.Count > 0)
            {
                Random.InitState (1);
                foreach (Transform root in shatter.rootChildList)
                {
                    if (root != null)
                    {
                        MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>();
                        foreach (var mf in meshFilters)
                        {
                            Gizmos.color = new Color (Random.Range (0.2f, 0.8f), Random.Range (0.2f, 0.8f), Random.Range (0.2f, 0.8f));
                            Gizmos.DrawMesh (mf.sharedMesh, mf.transform.position, mf.transform.rotation, mf.transform.lossyScale * 1.01f);
                        }
                    }
                }
            }
        }
    }
}