using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TowerDefManager))]
public class TowerDefManagerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        TowerDefManager tdm = target as TowerDefManager;

        GUIStyle headStyle = new GUIStyle();
        headStyle.fontSize = 20;
        headStyle.normal.textColor = Color.yellow;
        switch (tdm.state)
        {
            case TowerDefManager.TowerDefState.INTRO:
                EditorGUILayout.LabelField("INTRO", headStyle, GUILayout.Height(30));
                break;
            case TowerDefManager.TowerDefState.OUTRO:
                EditorGUILayout.LabelField("OUTRO", headStyle, GUILayout.Height(30));
                break;
            case TowerDefManager.TowerDefState.END:
                EditorGUILayout.LabelField("END", headStyle, GUILayout.Height(30));
                break;
            case TowerDefManager.TowerDefState.WAVE:
                EditorGUILayout.LabelField("CURRENT WAVE NUM " + (tdm.currentWave + 1), headStyle, GUILayout.Height(30));
                break;
            case TowerDefManager.TowerDefState.RECOVERY:
                EditorGUILayout.LabelField("RECOVERY WAVE NUM " + (tdm.currentWave + 1), headStyle, GUILayout.Height(30));
                break;
            case TowerDefManager.TowerDefState.FALLBACK:
                EditorGUILayout.LabelField("FALLBACK WAVE NUM " + (tdm.currentWave + 1), headStyle, GUILayout.Height(30));
                break;
            case TowerDefManager.TowerDefState.CONTINUE:
                EditorGUILayout.LabelField("CONTINUE WAVE NUM " + (tdm.currentWave + 1), headStyle, GUILayout.Height(30));
                break;
        }
        base.OnInspectorGUI();
    }
}
