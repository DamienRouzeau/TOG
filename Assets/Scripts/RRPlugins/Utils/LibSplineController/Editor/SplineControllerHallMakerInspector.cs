using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SplineControllerHallMaker))]
public class SplineControllerHallMakerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        float fVariance = ((SplineControllerHallMaker)target).fVariance;
        float fNewVariance = EditorGUILayout.FloatField("variance:", fVariance);

        if ( fVariance != fNewVariance || GUILayout.Button("make hall", GUILayout.ExpandWidth(false)))
        {
            ((SplineControllerHallMaker)target).fVariance = fNewVariance;
            ((SplineControllerHallMaker)target).MakeHall();
            EditorUtility.SetDirty(target);
        }

        if (GUILayout.Button("remove hall", GUILayout.ExpandWidth(false)))
        {
            ((SplineControllerHallMaker)target).EraseHall();
        }
    }
}