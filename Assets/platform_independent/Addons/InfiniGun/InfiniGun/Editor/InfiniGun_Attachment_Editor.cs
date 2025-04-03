using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InfiniGunAttachment)),CanEditMultipleObjects]
public class InfiniGun_Attachment_Editor : Editor{

    public override void OnInspectorGUI(){
        InfiniGunAttachment t = (InfiniGunAttachment)target;
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        GUILayout.Label("Attachment Setup",new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold, fontSize = 13},GUILayout.ExpandWidth(true));           
        EditorGUILayout.Space(); 
        t.attachmentType = (InfiniGunAttachment.AttachmentType)EditorGUILayout.EnumPopup(new GUIContent("Attachment Type","Determines Type of attachment."),t.attachmentType);

        if(t.attachmentType == InfiniGunAttachment.AttachmentType.Sight){
            EditorGUILayout.LabelField("",GUI.skin.horizontalSlider,GUILayout.MaxHeight(6));
            EditorGUILayout.Space();
            GUILayout.Label("Sight Setup",new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold},GUILayout.ExpandWidth(true));            
            t.sightCenterY = EditorGUILayout.FloatField(new GUIContent("Center Y","The point at which the center of the sight is located along the Y axis. Represented by the red wire sphere Gizmo."),t.sightCenterY);
            t.coneOfAccuracyModifier = EditorGUILayout.Slider(new GUIContent("Cone of Accuracy Modifier","Modifies the Cone of Accuracy on parent gun."),t.coneOfAccuracyModifier, -10,10);
            }

        if(t.attachmentType == InfiniGunAttachment.AttachmentType.Scope){
            EditorGUILayout.LabelField("",GUI.skin.horizontalSlider,GUILayout.MaxHeight(6));
            EditorGUILayout.Space();
            GUILayout.Label("Scope Setup",new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold},GUILayout.ExpandWidth(true));
            t.sightCenterY = EditorGUILayout.FloatField(new GUIContent("Center Y","The point at which the center of the sight is located along the Y axis. Represented by the red wire sphere Gizmo."),t.sightCenterY);
            t.coneOfAccuracyModifier = EditorGUILayout.Slider(new GUIContent("Cone of Accuracy Modifier","Modifies the Cone of Accuracy on parent gun."),t.coneOfAccuracyModifier, -10,10);
            t.ScopedFOV = EditorGUILayout.Slider(new GUIContent("Scoped FOV","Determines the camera's Field of View when Scoped."),t.ScopedFOV,15,90);
            t.scopeSway = EditorGUILayout.ToggleLeft(new GUIContent("Sway Scope?","Determines if the scope will sway."),t.scopeSway);
            if(t.scopeSway){EditorGUI.indentLevel++;t.scopeSwStam = EditorGUILayout.Slider(new GUIContent("Max Steady Time","Determines how long the player can steady the scope. A value of 0 will allow for infinite steadying"),t.scopeSwStam, 0, 15); EditorGUI.indentLevel--;}
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("Scope Overlay","Sprite that will fill the screen when Scoped."));
            t.scopeOverlay = (Sprite)EditorGUILayout.ObjectField(t.scopeOverlay,typeof(Sprite),false);
            EditorGUILayout.EndHorizontal();
            t.scopeIn = (AudioClip)EditorGUILayout.ObjectField(new GUIContent("Scope-In Sound","An Audio clip that will sound when scoping in."),t.scopeIn,typeof(AudioClip),false);
            t.useSeparateScopeCam = EditorGUILayout.ToggleLeft(new GUIContent("Use Separate Scope Camera","Makes new camera attached to the scope. This allows for better visual effects and movements when in the Scoped state. \n\n Disable this for use in multi-camera setup as this method won't work and a new method will need to be custom made. As of this version, This has not been tested in a multiplayer setup."), t.useSeparateScopeCam);
            t.willHideFlash = EditorGUILayout.ToggleLeft(new GUIContent("Hide Muzzle Flash","Should this scope disable the parent gun's muzzle flash?"), t.willHideFlash);
        
        }

        if(t.attachmentType == InfiniGunAttachment.AttachmentType.Grip || t.attachmentType == InfiniGunAttachment.AttachmentType.miscAttachment){
            EditorGUILayout.LabelField("",GUI.skin.horizontalSlider,GUILayout.MaxHeight(6));
            EditorGUILayout.Space();
            GUILayout.Label(t.attachmentType == InfiniGunAttachment.AttachmentType.Grip ? "Grip Setup":"Misc Attachment",new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold},GUILayout.ExpandWidth(true));
                        EditorGUILayout.Space();
           
            GUILayout.Label(new GUIContent("Recoil Force Modifier","Modifies the recoil forces on parent gun by percentage."),new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter});
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            GUILayout.BeginVertical(GUILayout.MaxWidth(30)); 
                EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                        GUILayout.Label("X");
                    GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                        t.recoilModifier.x = GUILayout.VerticalSlider(t.recoilModifier.x,100,-100);
                    GUILayout.FlexibleSpace(); 
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                        GUILayout.Label("%");
                        t.recoilModifier.x = EditorGUILayout.FloatField(t.recoilModifier.x,GUILayout.MaxWidth(38));
                    GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();


            GUILayout.BeginVertical(GUILayout.MaxWidth(30)); 
                EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                        GUILayout.Label("Y");
                    GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                        t.recoilModifier.y = GUILayout.VerticalSlider(t.recoilModifier.y,100,-100);
                    GUILayout.FlexibleSpace(); 
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                        GUILayout.Label("%");
                        t.recoilModifier.y = EditorGUILayout.FloatField(t.recoilModifier.y,GUILayout.MaxWidth(38));
                    GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            

            GUILayout.BeginVertical(GUILayout.MaxWidth(30)); 
                EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                        GUILayout.Label("Z");
                    GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                        t.recoilModifier.z = GUILayout.VerticalSlider(t.recoilModifier.z,100,-100);
                    GUILayout.FlexibleSpace(); 
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                        GUILayout.Label("%");
                        t.recoilModifier.z = EditorGUILayout.FloatField(t.recoilModifier.z,GUILayout.MaxWidth(38));
                    GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            t.coneOfAccuracyModifier = EditorGUILayout.Slider(new GUIContent("Cone of Accuracy Modifier","Modifies the Cone of Accuracy on parent gun."),t.coneOfAccuracyModifier, -10,10);
        }
        
        if(t.attachmentType == InfiniGunAttachment.AttachmentType.Suppressor||t.attachmentType == InfiniGunAttachment.AttachmentType.otherBarrelAttachment){
            EditorGUILayout.LabelField("",GUI.skin.horizontalSlider,GUILayout.MaxHeight(6));
            EditorGUILayout.Space();
            GUILayout.Label(t.attachmentType == InfiniGunAttachment.AttachmentType.Suppressor?"Suppressor Setup":"Barrel Attachment Setup",new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold},GUILayout.ExpandWidth(true));
           
            EditorGUILayout.Space();
           
            GUILayout.Label(new GUIContent("Recoil Force Modifier","Modifies the recoil forces on parent gun by percentage."),new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter});
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            GUILayout.BeginVertical(GUILayout.MaxWidth(30)); 
                EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                        GUILayout.Label("X");
                    GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                        t.recoilModifier.x = GUILayout.VerticalSlider(t.recoilModifier.x,100,-100);
                    GUILayout.FlexibleSpace(); 
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                        GUILayout.Label("%");
                        t.recoilModifier.x = EditorGUILayout.FloatField(t.recoilModifier.x,GUILayout.MaxWidth(38));
                    GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();


            GUILayout.BeginVertical(GUILayout.MaxWidth(30)); 
                EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                        GUILayout.Label("Y");
                    GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                        t.recoilModifier.y = GUILayout.VerticalSlider(t.recoilModifier.y,100,-100);
                    GUILayout.FlexibleSpace(); 
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                        GUILayout.Label("%");
                        t.recoilModifier.y = EditorGUILayout.FloatField(t.recoilModifier.y,GUILayout.MaxWidth(38));
                    GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            

            GUILayout.BeginVertical(GUILayout.MaxWidth(30)); 
                EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                        GUILayout.Label("Z");
                    GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                        t.recoilModifier.z = GUILayout.VerticalSlider(t.recoilModifier.z,100,-100);
                    GUILayout.FlexibleSpace(); 
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                        GUILayout.Label("%");
                        t.recoilModifier.z = EditorGUILayout.FloatField(t.recoilModifier.z,GUILayout.MaxWidth(38));
                    GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            t.coneOfAccuracyModifier = EditorGUILayout.Slider(new GUIContent("Cone of Accuracy Modifier","Modifies the Cone of Accuracy on parent gun."),t.coneOfAccuracyModifier, -10,10);
            t.willHideFlash = EditorGUILayout.ToggleLeft(new GUIContent("Hide Muzzle Flash","Does this Attachment hide or disable the muzzle flash on parent gun?"), t.willHideFlash);
        }
        
        
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("Box");
        GUILayout.Label(new GUIContent("Support Address","Need help? No Problem! We're always happy to help with any issue you may have."),new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter},GUILayout.ExpandWidth(true));
        EditorGUILayout.SelectableLabel(new GUIContent("support@aedangraves.info","Need help? No Problem! We're always happy to help with any issue you may have.").text,new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold, fontSize = 13},GUILayout.ExpandWidth(true));
        EditorGUILayout.EndVertical();
        if(GUI.changed){EditorUtility.SetDirty(t);}
    }
}
