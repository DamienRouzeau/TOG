using UnityEditor;
using UnityEngine;

namespace ThreeEyedGames.Mercator
{
    [CustomEditor(typeof(MercatorTerrainMaster))]
    public class MercatorTerrainMasterEditor : Editor
    {
        [MenuItem("GameObject/3D Object/Mercator Terrain Master")]
        private static void CreateMaster()
        {
            var go = new GameObject("Mercator Terrain Master");
            go.AddComponent<MercatorTerrainMaster>();
            Selection.activeGameObject = go;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var master = target as MercatorTerrainMaster;

            EditorGUI.BeginDisabledGroup(master.Terrain == null);
            if (GUILayout.Button("Create Stamp"))
            {
                var stamp = master.CreateTerrainStamp("Stamp", null, null);
                Selection.activeGameObject = stamp.gameObject;
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}