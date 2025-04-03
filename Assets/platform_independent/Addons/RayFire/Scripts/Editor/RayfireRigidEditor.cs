using UnityEngine;
using UnityEditor;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireRigid))]
    public class RayfireRigidEditor : Editor
    {
        // Target
        RayfireRigid rigid = null;

        public override void OnInspectorGUI()
        {
            // Get target
            rigid = target as RayfireRigid;
            
            // Space
            GUILayout.Space (8);
            
            // Initialize
            if (Application.isPlaying == true)
                if (GUILayout.Button ("Initialize", GUILayout.Height (25)))
                    foreach (var targ in targets)
                        (targ as RayfireRigid).Initialize();
                
            GUILayout.Space (2); 
                
            // Begin
            GUILayout.BeginHorizontal();

            // Precache  buttons
            if (rigid.demolitionType == DemolitionType.ManualPrecache)
            {
                // Precache
                if (GUILayout.Button (" Scene Precache ", GUILayout.Height (25)))
                {
                    foreach (var targ in targets)
                    {
                        (targ as RayfireRigid).contactPoint = (targ as RayfireRigid).transform.TransformPoint (Vector3.zero);
                        (targ as RayfireRigid).ManualPrecache();
                    }
                }

                if (GUILayout.Button ("    Clear    ", GUILayout.Height (25)))
                    foreach (var targ in targets)
                        (targ as RayfireRigid).DeleteCache();
            }

            // Prefragment buttons
            else if (rigid.demolitionType == DemolitionType.ManualPrefragment)
            {
                // Prefragment
                if (GUILayout.Button ("Prefragment ", GUILayout.Height (25)))
                {
                    foreach (var targ in targets)
                    {
                        (targ as RayfireRigid).contactPoint = (targ as RayfireRigid).transform.TransformPoint (Vector3.zero);
                        (targ as RayfireRigid).ManualPrefragment();
                    }
                }

                if (GUILayout.Button (" Delete", GUILayout.Height (25)))
                    foreach (var targ in targets)
                        (targ as RayfireRigid).DeleteFragments();
            }

            // Cache buttons
            else if (rigid.demolitionType == DemolitionType.ManualPrefabPrecache)
            {
                // Precache
                if (GUILayout.Button (" Prefab Precache", GUILayout.Height (25)))
                {
                    foreach (var targ in targets)
                    {
                        (targ as RayfireRigid).contactPoint = (targ as RayfireRigid).transform.TransformPoint (Vector3.zero);
                        (targ as RayfireRigid).PrefabPrecache();
                    }
                }

                if (GUILayout.Button ("    Clear    ", GUILayout.Height (25)))
                    foreach (var targ in targets)
                        (targ as RayfireRigid).DeleteCache();
            }

            // // Cluster colliders
            // if (rigid.objectType == ObjectType.NestedCluster || rigid.objectType == ObjectType.ConnectedCluster)
            // {
            //     if (GUILayout.Button ("Create colliders", GUILayout.Height (25)))
            //         rigid.GenerateColliders();
            //
            //     if (GUILayout.Button ("    Clear    ", GUILayout.Height (25)))
            //         rigid.DeleteColliders();
            // }
            
            // End
            EditorGUILayout.EndHorizontal();
            
            // Manual Prefab precache
            if (rigid.demolitionType == DemolitionType.ManualPrefabPrecache)
            {
                if (rigid.HasRfMeshes == false)
                    GUILayout.Label ("WARNING: No Rf Meshes Precached yet");
                if (rigid.HasFragments == true)
                    GUILayout.Label ("WARNING: Has fragments");
                
                // Compress mesh data
                rigid.meshDemolition.compressPrefab = GUILayout.Toggle (rigid.meshDemolition.compressPrefab, "Compress Mesh data");
            }

            // Manual Precache warning
            if (rigid.demolitionType == DemolitionType.ManualPrecache)
            {
                if (rigid.HasMeshes == false)
                    GUILayout.Label ("WARNING: No Meshes Precached yet");
                if (rigid.HasFragments == true)
                    GUILayout.Label ("WARNING: Has fragments");
            }

            // Manual Prefragment warning
            else if (rigid.demolitionType == DemolitionType.ManualPrefragment)
            {
                if (rigid.HasFragments == false)
                    GUILayout.Label ("WARNING: No Fragments yet");
                if (rigid.HasMeshes == true)
                    GUILayout.Label ("WARNING: Has precached meshes");
            }

            // Demolition actions
            if (Application.isPlaying == true)
            {
                // Space
                GUILayout.Space (1);
                
                // Begin
                GUILayout.BeginHorizontal();
                
                // Demolish
                if (GUILayout.Button ("   Demolish    ", GUILayout.Height (22)))
                {
                    foreach (var targ in targets)
                    {
                        // Set type
                        if ((targ as RayfireRigid).demolitionType == DemolitionType.None)
                            (targ as RayfireRigid).demolitionType = DemolitionType.Runtime;

                        // Demolish
                        
                        (targ as RayfireRigid).DemolishObject();
                    }
                }

                // Activate
                if (GUILayout.Button ("Activate", GUILayout.Height (22)))
                {
                    foreach (var targ in targets)
                        //if ((targ as RayfireRigid).physics.rigidBody != null)
                            (targ as RayfireRigid).Activate();
                }

                // End
                EditorGUILayout.EndHorizontal();
            }

            // Demolition info
            if (rigid.limitations.demolished == false)
            {
                // Cache info
                if (rigid.HasMeshes == true)
                    GUILayout.Label ("Precached Meshes: " + rigid.meshes.Length);
                if (rigid.HasFragments == true)
                    GUILayout.Label ("Prefragmented: " + rigid.fragments.Count);
                if (rigid.HasRfMeshes == true)
                    GUILayout.Label ("Precached Serialized Meshes: " + rigid.rfMeshes.Length);
            }

            // Demolition info
            if (Application.isPlaying == true && rigid.enabled == true && rigid.initialization == RayfireRigid.InitType.AtStart)
            {
                // Space
                GUILayout.Space (3);

                // Info
                GUILayout.Label ("Info", EditorStyles.boldLabel);

                // Excluded
                if (rigid.physics.exclude == true)
                    GUILayout.Label ("Warning: Object excluded from simulation.");

                // Size
                GUILayout.Label ("    Size: " + rigid.limitations.bboxSize.ToString());

                // Demolition
                GUILayout.Label ("    Demolition depth: " + rigid.limitations.currentDepth.ToString() + "/" + rigid.limitations.depth.ToString());

                // Damage
                if (rigid.damage.enable == true)
                    GUILayout.Label ("    Damage applied: " + rigid.damage.currentDamage.ToString() + "/" + rigid.damage.maxDamage.ToString());

                // Bad mesh
                if (rigid.meshDemolition.badMesh > RayfireMan.inst.advancedDemolitionProperties.badMeshTry)
                    GUILayout.Label ("    Object has bad mesh and will not be demolished anymore");
            }

            // Space
            GUILayout.Space (3);

            // Draw script UI
            DrawDefaultInspector();
        }
    }
}