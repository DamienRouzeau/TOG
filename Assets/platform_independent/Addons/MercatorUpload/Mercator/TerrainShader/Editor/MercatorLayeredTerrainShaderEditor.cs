using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ThreeEyedGames.Mercator
{
    public class MercatorLayeredTerrainShaderEditor : ShaderGUI
    {
        private Texture _gearWheelIcon;

        const int NUM_LAYERS = 6;
        private int _layer = -1;
        private Vector2 _scrollPos = Vector2.zero;
        private LayerRepresentation _clipboard = null;

        public enum DebugMode
        {
            None = 0,
            Height,
            Normals,
            Slope,
            IdentifyRGBCMY
        }

        class LayerRepresentation
        {
            public static LayerRepresentation Get(int n, MaterialProperty[] properties)
            {
                LayerRepresentation instance = new LayerRepresentation();
                var type = typeof(LayerRepresentation);
                foreach (FieldInfo field in type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    MaterialProperty matProp = FindProperty("_Layer" + n + "_" + field.Name, properties);
                    var fieldType = field.FieldType;
                    if (fieldType == typeof(float))
                        field.SetValue(instance, matProp.floatValue);
                    else if (fieldType == typeof(Texture))
                        field.SetValue(instance, matProp.textureValue);
                    else if (fieldType == typeof(Color))
                        field.SetValue(instance, matProp.colorValue);
                }

                return instance;
            }

            public void WriteTo(int n, MaterialProperty[] properties)
            {
                var type = typeof(LayerRepresentation);
                foreach (FieldInfo field in type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    MaterialProperty matProp = FindProperty("_Layer" + n + "_" + field.Name, properties);
                    var fieldType = field.FieldType;
                    if (fieldType == typeof(float))
                        matProp.floatValue = (float)field.GetValue(this);
                    else if (fieldType == typeof(Texture))
                        matProp.textureValue = (Texture)field.GetValue(this);
                    else if (fieldType == typeof(Color))
                        matProp.colorValue = (Color)field.GetValue(this);
                }
            }

            float Enabled;
            Texture Albedo;
            Texture Normal;
            float Glossiness;
            float GlossFlip;
            float Metallic;
            float UV_ScaleX;
            float UV_ScaleY;
            float UV_Rotation;
            float UV2;
            float UV2_ScaleX;
            float UV2_ScaleY;
            float UV2_Rotation;
            float UV2_NoiseFactor;
            float UV2_NoiseScale;
            Color Color;
            float Blend_NoiseFactor;
            float Blend_Bias;
            float Blend_NoiseScale;
            float Height;
            float Height_Alpha0;
            float Height_Alpha1;
            float Height_Fade0;
            float Height_Fade1;
            float Height_Fade2;
            float Height_Fade3;
            float Slope;
            float Slope_Alpha0;
            float Slope_Alpha1;
            float Slope_Fade0;
            float Slope_Fade1;
            float Slope_Fade2;
            float Slope_Fade3;
        }

        private void CheckInitialized()
        {
            if (_gearWheelIcon == null)
            {
                var result = EditorGUIUtility.Load("Mercator/settings.png");
                _gearWheelIcon = result as Texture;
            }
        }

        private void ExchangeCurrentLayerWith(int target, MaterialProperty[] properties)
        {
            var currentLayer = LayerRepresentation.Get(_layer, properties);
            var targetLayer = LayerRepresentation.Get(target, properties);
            currentLayer.WriteTo(target, properties);
            targetLayer.WriteTo(_layer, properties);
            _layer = target;
        }

        private bool MenuButton(bool pressed, string caption, bool useAlpha, Texture tex, Color color, float texSizeFactor, bool border)
        {
            var texRect = GUILayoutUtility.GetAspectRect(1.0f, GUILayout.MinWidth(40), GUILayout.ExpandWidth(true), GUILayout.MaxWidth(80));
            pressed = GUI.Toggle(texRect, pressed, caption, "Button");
            if (tex != null)
            {
                Vector2 center = texRect.center;
                texRect.size *= texSizeFactor;
                texRect.center = center;
                GUI.DrawTexture(texRect, tex, ScaleMode.ScaleToFit, useAlpha, 0, color, 0.0f, 0.0f);
                if (border)
                    GUI.DrawTexture(texRect, tex, ScaleMode.ScaleToFit, false, 0, Color.black, 1.0f, 0.0f);
            }

            return pressed;
        }

        private void DrawTopMenu(MaterialProperty[] properties)
        {
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandHeight(false));
            GUILayout.BeginHorizontal();

            // Draw the settings button
            if (MenuButton(_layer == -1, "", true, _gearWheelIcon, Color.white, 0.8f, false))
                _layer = -1;

            // Draw the layer buttons
            for (int n = 0; n < NUM_LAYERS; ++n)
            {
                Texture albedo = FindProperty("_Layer" + n + "_Albedo", properties).textureValue;
                Color color = FindProperty("_Layer" + n + "_Color", properties).colorValue;
                if (MenuButton(_layer == n, (n + 1).ToString(), false, albedo, color, 0.85f, true))
                    _layer = n;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
        }

        private void DrawTopButtons(MaterialProperty[] properties)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(_layer == -1);

            EditorGUI.BeginDisabledGroup(_layer == 0);
            if (GUILayout.Button("◄"))
                ExchangeCurrentLayerWith(_layer - 1, properties);
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Copy"))
            {
                _clipboard = LayerRepresentation.Get(_layer, properties);
            }

            EditorGUI.BeginDisabledGroup(_clipboard == null);
            if (GUILayout.Button("Paste"))
            {
                _clipboard.WriteTo(_layer, properties);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(_layer == NUM_LAYERS - 1);
            if (GUILayout.Button("►"))
                ExchangeCurrentLayerWith(_layer + 1, properties);
            EditorGUI.EndDisabledGroup();

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            CheckInitialized();
            DrawTopMenu(properties);
            DrawTopButtons(properties);

            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
            EditorGUILayout.Space();

            if (_layer == -1)
                DrawSettingsGUI(materialEditor, properties);
            else
                DrawLayerGUI(materialEditor, properties, _layer);
        }

        private void DrawSettingsGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            EditorGUILayout.LabelField("SETTINGS", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Height", EditorStyles.boldLabel);
            materialEditor.ShaderProperty(FindProperty("_Prop_MinHeight", properties), "Minimum");
            materialEditor.ShaderProperty(FindProperty("_Prop_MaxHeight", properties), "Maximum");

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);

            materialEditor.ShaderProperty(FindProperty("_Prop_ClipWarning", properties), "Magenta Clip Warning");

            var prop_debugMode = FindProperty("_Prop_DebugMode", properties);
            prop_debugMode.floatValue = Convert.ToInt32(EditorGUILayout.EnumPopup("Debug Mode", (DebugMode)prop_debugMode.floatValue));
        }

        private void DrawLayerGUI(MaterialEditor materialEditor, MaterialProperty[] properties, int n)
        {
            var prop_albedo = FindProperty("_Layer" + n + "_Albedo", properties);
            GUIStyle style = GUI.skin.label;
            style.richText = true;
            string title = "<b>LAYER " + (n + 1) + "</b>";
            if (prop_albedo.textureValue != null)
                title += " (" + prop_albedo.textureValue.name + ")";
            EditorGUILayout.LabelField(title, style);

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            {
                var prop_enabled = FindProperty("_Layer" + n + "_Enabled", properties);
                var prop_normal = FindProperty("_Layer" + n + "_Normal", properties);
                var prop_glossiness = FindProperty("_Layer" + n + "_Glossiness", properties);
                var prop_glossflip = FindProperty("_Layer" + n + "_GlossFlip", properties);
                var prop_metallic = FindProperty("_Layer" + n + "_Metallic", properties);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label("Albedo/Gloss", GUILayout.Width(80));
                prop_albedo.textureValue = (Texture)EditorGUILayout.ObjectField(prop_albedo.textureValue, typeof(Texture2D), false, GUILayout.Width(80), GUILayout.Height(80));
                prop_enabled.floatValue = (prop_albedo.textureValue != null ? 1.0f : 0.0f);
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label("Normals", GUILayout.Width(80));
                prop_normal.textureValue = (Texture)EditorGUILayout.ObjectField(prop_normal.textureValue, typeof(Texture2D), false, GUILayout.Width(80), GUILayout.Height(80));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label("Glossiness");
                EditorGUI.indentLevel++;
                prop_glossiness.floatValue = EditorGUILayout.Slider(prop_glossiness.floatValue, 0, 1);
                prop_glossflip.floatValue = EditorGUILayout.Toggle("Invert texture glossiness", prop_glossflip.floatValue != 0.0f) ? 1.0f : 0.0f;
                EditorGUI.indentLevel--;
                GUILayout.Space(10);
                GUILayout.Label("Metallic");
                EditorGUI.indentLevel++;
                prop_metallic.floatValue = EditorGUILayout.Slider(prop_metallic.floatValue, 0, 1);
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            materialEditor.ShaderProperty(FindProperty("_Layer" + n + "_Color", properties), "Color");
            var prop_uvScaleX = FindProperty("_Layer" + n + "_UV_ScaleX", properties);
            var prop_uvScaleY = FindProperty("_Layer" + n + "_UV_ScaleY", properties);
            var uvScale = EditorGUILayout.Vector2Field("UV Scale", new Vector2(prop_uvScaleX.floatValue, prop_uvScaleY.floatValue));
            prop_uvScaleX.floatValue = uvScale.x;
            prop_uvScaleY.floatValue = uvScale.y;
            materialEditor.ShaderProperty(FindProperty("_Layer" + n + "_UV_Rotation", properties), "UV Rotation");

            var prop_useUV2 = FindProperty("_Layer" + n + "_UV2", properties);
            bool useUV2 = EditorGUILayout.Toggle("Tiling Reduction", prop_useUV2.floatValue != 0.0f);
            prop_useUV2.floatValue = useUV2 ? 1.0f : 0.0f;
            if (useUV2)
            {
                EditorGUI.indentLevel++;
                var prop_uv2ScaleX = FindProperty("_Layer" + n + "_UV2_ScaleX", properties);
                var prop_uv2ScaleY = FindProperty("_Layer" + n + "_UV2_ScaleY", properties);
                var uv2Scale = EditorGUILayout.Vector2Field("UV2 Scale", new Vector2(prop_uv2ScaleX.floatValue, prop_uv2ScaleY.floatValue));
                prop_uv2ScaleX.floatValue = uv2Scale.x;
                prop_uv2ScaleY.floatValue = uv2Scale.y;
                materialEditor.DefaultShaderProperty(FindProperty("_Layer" + n + "_UV2_Rotation", properties), "UV2 Rotation");
                materialEditor.DefaultShaderProperty(FindProperty("_Layer" + n + "_UV2_NoiseFactor", properties), "Noise Factor");
                materialEditor.DefaultShaderProperty(FindProperty("_Layer" + n + "_UV2_NoiseScale", properties), "Noise Scale");
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Blending", EditorStyles.boldLabel);
            materialEditor.DefaultShaderProperty(FindProperty("_Layer" + n + "_Blend_NoiseFactor", properties), "Noise Factor");
            materialEditor.DefaultShaderProperty(FindProperty("_Layer" + n + "_Blend_NoiseScale", properties), "Noise Scale");
            materialEditor.DefaultShaderProperty(FindProperty("_Layer" + n + "_Blend_Bias", properties), "Bias");

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);

            var prop_useHeight = FindProperty("_Layer" + n + "_Height", properties);
            bool useHeight = EditorGUILayout.Toggle("Height", prop_useHeight.floatValue != 0.0f);
            prop_useHeight.floatValue = useHeight ? 1.0f : 0.0f;
            if (useHeight)
            {
                EditorGUI.indentLevel++;
                materialEditor.DefaultShaderProperty(FindProperty("_Layer" + n + "_Height_Fade0", properties), "Fade-in Start");
                materialEditor.DefaultShaderProperty(FindProperty("_Layer" + n + "_Height_Fade1", properties), "Fade-in End");
                materialEditor.DefaultShaderProperty(FindProperty("_Layer" + n + "_Height_Fade2", properties), "Fade-out Start");
                materialEditor.DefaultShaderProperty(FindProperty("_Layer" + n + "_Height_Fade3", properties), "Fade-out End");
                materialEditor.DefaultShaderProperty(FindProperty("_Layer" + n + "_Height_Alpha0", properties), "Fade-in Alpha");
                materialEditor.DefaultShaderProperty(FindProperty("_Layer" + n + "_Height_Alpha1", properties), "Fade-out Alpha");
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }

            var prop_useSlope = FindProperty("_Layer" + n + "_Slope", properties);
            bool useSlope = EditorGUILayout.Toggle("Slope", prop_useSlope.floatValue != 0.0f);
            prop_useSlope.floatValue = useSlope ? 1.0f : 0.0f;
            if (useSlope)
            {
                EditorGUI.indentLevel++;
                materialEditor.DefaultShaderProperty(FindProperty("_Layer" + n + "_Slope_Fade0", properties), "Fade-in Start");
                materialEditor.DefaultShaderProperty(FindProperty("_Layer" + n + "_Slope_Fade1", properties), "Fade-in End");
                materialEditor.DefaultShaderProperty(FindProperty("_Layer" + n + "_Slope_Fade2", properties), "Fade-out Start");
                materialEditor.DefaultShaderProperty(FindProperty("_Layer" + n + "_Slope_Fade3", properties), "Fade-out End");
                materialEditor.DefaultShaderProperty(FindProperty("_Layer" + n + "_Slope_Alpha0", properties), "Fade-in Alpha");
                materialEditor.DefaultShaderProperty(FindProperty("_Layer" + n + "_Slope_Alpha1", properties), "Fade-out Alpha");
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }
    }
}