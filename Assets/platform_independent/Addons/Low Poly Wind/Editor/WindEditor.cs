using UnityEditor;
using UnityEngine;

public class WindEditor : ShaderGUI
{
    int tab;
    
    string[] menuItems =
    { 
        "Trunk Colors", 
        "Movement", 
        "Settings" 
    };

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        //Shader properties
        MaterialProperty colorProperty = FindProperty("_Color", properties);

        MaterialProperty T1 = FindProperty("_ColorT1", properties);
        MaterialProperty T2 = FindProperty("_ColorT2", properties);
        MaterialProperty T3 = FindProperty("_ColorT3", properties);
        MaterialProperty T4 = FindProperty("_ColorT4", properties);
        MaterialProperty T5 = FindProperty("_ColorT5", properties);

        MaterialProperty texture = FindProperty("_MainTex", properties);
        MaterialProperty gloss = FindProperty("_Glossiness", properties);
        MaterialProperty metal = FindProperty("_Metallic", properties);

        MaterialProperty distX = FindProperty("_DistanceX", properties);
        MaterialProperty distY = FindProperty("_DistanceY", properties);
        MaterialProperty distZ = FindProperty("_DistanceZ", properties);

        MaterialProperty speedX = FindProperty("_SpeedX", properties);
        MaterialProperty speedY = FindProperty("_SpeedY", properties);
        MaterialProperty speedZ = FindProperty("_SpeedZ", properties);

        MaterialProperty worldSpace = FindProperty("_WorldSpace", properties);

        MaterialProperty trunkMov = FindProperty("_MoveTrunk", properties);
        MaterialProperty trunkSpeed = FindProperty("_TrunkSpeed", properties);

        MaterialProperty heightSpeed = FindProperty("_HeightSpeed", properties);
        MaterialProperty heightDist = FindProperty("_HeightDist", properties);
        MaterialProperty uheight = FindProperty("_UseHeight", properties);
        MaterialProperty minHeight = FindProperty("_MinHeight", properties);

        MaterialProperty precision = FindProperty("_Precision", properties);


        EditorGUILayout.LabelField("Low Poly Wind", EditorStyles.centeredGreyMiniLabel);

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        materialEditor.ShaderProperty(colorProperty, "Albedo");

        EditorGUILayout.Space();

        materialEditor.ShaderProperty(texture, "Texture");

        EditorGUILayout.Space();

        materialEditor.ShaderProperty(gloss, "Smoothness");
        materialEditor.ShaderProperty(metal, "Metallic");

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        
        tab = GUILayout.Toolbar(tab, menuItems);

        if (tab == 0)
        {
            EditorGUILayout.Space();

            EditorGUILayout.Space();

            materialEditor.ShaderProperty(T1, "     Color 1");
            materialEditor.ShaderProperty(T2, "     Color 2");
            materialEditor.ShaderProperty(T3, "     Color 3");
            materialEditor.ShaderProperty(T4, "     Color 4");
            materialEditor.ShaderProperty(T5, "     Color 5");

            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        EditorGUILayout.Space();

        if (tab == 1)
        {
            EditorGUILayout.Space();

            materialEditor.ShaderProperty(worldSpace, "World Space");
            
            EditorGUILayout.Space();

                EditorGUILayout.LabelField("X", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                materialEditor.ShaderProperty(distX, "Distance");
                materialEditor.ShaderProperty(speedX, "Speed");

            EditorGUILayout.EndVertical();

                EditorGUILayout.LabelField("Y", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                materialEditor.ShaderProperty(distY, "Distance");
                materialEditor.ShaderProperty(speedY, "Speed");

            EditorGUILayout.EndVertical();

                EditorGUILayout.LabelField("Z", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                materialEditor.ShaderProperty(distZ, "Distance");
                materialEditor.ShaderProperty(speedZ, "Speed");

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

        }

       if (tab == 2)
        {
            EditorGUILayout.Space();

            materialEditor.ShaderProperty(trunkMov, "Move Trunk");

            if (trunkMov.floatValue == 1)
                materialEditor.ShaderProperty(trunkSpeed, "    Trunk Speed");


            materialEditor.ShaderProperty(uheight, "Use Height");

            if (uheight.floatValue == 1)
            {
                materialEditor.ShaderProperty(heightDist, "    Distance");
                materialEditor.ShaderProperty(heightSpeed, "    Speed");
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            materialEditor.ShaderProperty(minHeight, "Minimum Height");

            EditorGUILayout.Space();

            materialEditor.ShaderProperty(precision, "Color Precision");
        }
    }
}