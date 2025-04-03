using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(InfiniGunTarget)), CanEditMultipleObjects]
public class InfiniGun_Target_Editor : Editor{
    public override void OnInspectorGUI(){
        InfiniGunTarget t = (InfiniGunTarget)target;
        EditorGUILayout.Space();
        GUI.enabled = false;
        t.isDead = EditorGUILayout.ToggleLeft("Target is Dead?", t.isDead);
        GUI.enabled = true;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("",GUI.skin.horizontalSlider,GUILayout.MaxHeight(6));
        EditorGUILayout.Space();
        GUILayout.Label("Health Settings",new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold, fontSize = 13},GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();
        t.CanDie = EditorGUILayout.ToggleLeft(new GUIContent("Target can Die?","Determines if the target can die."), t.CanDie);
        t.healthPoints = EditorGUILayout.IntSlider(new GUIContent("Health Points","Determines how many health points this target has."), t.healthPoints,0,1000);
            EditorGUILayout.Space();
        t.allowHeadshot = EditorGUILayout.ToggleLeft(new GUIContent("Allow Headshot Multipiers?","Determines if a headshot effects damage taken."),t.allowHeadshot);
        EditorGUI.indentLevel++;
        if(t.allowHeadshot){t.headPosition = EditorGUILayout.Vector3Field(new GUIContent("Head Position","Position of head on this target."),t.headPosition); t.headRadius = EditorGUILayout.Slider(new GUIContent("Radius:","Radius of head on this target."),t.headRadius,1,10);}
        EditorGUI.indentLevel--;

            EditorGUILayout.Space();
        t.Regenerate = EditorGUILayout.ToggleLeft(new GUIContent("Allow Regeneration?","Determines if this target is allowed to regenerate health points over time."), t.Regenerate);
        if(t.Regenerate){
            EditorGUI.indentLevel++;
            t.linearRegen = EditorGUILayout.ToggleLeft(new GUIContent("Linear Regeneration?","Is the regeneration process linear in nature?"),t.linearRegen);
            t.regenerationTick = EditorGUILayout.Slider(new GUIContent(t.linearRegen?"Regeneration Speed":"Regeneration Tick",t.linearRegen?"Determines how fast health is regenerated.":"Determines how much time passes between each tick."),t.regenerationTick,0.1f,25);
            if(!t.linearRegen){t.regenerationIncrement = EditorGUILayout.Slider(new GUIContent("Regeneration Increment","Determines how much health is regenerated each tick."),t.regenerationIncrement,0,75);}
            t.waitToRegenerate = EditorGUILayout.Slider(new GUIContent("Wait to Regenerate","Determines how much time passes after taking damage before health starts regenerating."),t.waitToRegenerate,0,15);

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.LabelField("Health status: "+ (t.isDead? "Dead":("%"+(int)((t.internalHealthPoints/100)*100))),new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold});
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("",GUI.skin.horizontalSlider,GUILayout.MaxHeight(6));
        EditorGUILayout.Space();
        GUILayout.Label("GUI Settings",new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold, fontSize = 13},GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();
        t.useWorldSpaceGui = EditorGUILayout.ToggleLeft(new GUIContent("Draw Basic Health Meter?","Should A basic health meter be drawn?"),t.useWorldSpaceGui);
        if(t.useWorldSpaceGui){t.healthColor = EditorGUILayout.GradientField(new GUIContent("Health Meter Color","The Color of the health meter. From Right to Left, represents full health and low health respectively."),t.healthColor);}
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(new GUIContent("Hit Marker Sprite","A sprite that will flash on screen when target is hit."));
        t.hitMarkerSprite = (Sprite)EditorGUILayout.ObjectField(t.hitMarkerSprite,typeof(Sprite),false);
        EditorGUILayout.EndHorizontal();
        if(t.hitMarkerSprite){t.hitSound = (AudioClip)EditorGUILayout.ObjectField(new GUIContent("Hit Sound Clip","An Audio clip that will sound when target is hit."),t.hitSound,typeof(AudioClip),false);}
        EditorGUILayout.Space();
        t.drawGizmos = EditorGUILayout.ToggleLeft(new GUIContent("Draw Setup Gizmos","Draw Gizmos to assist with target setup. \n The Cyan Wire Sphere Gizmo represents the Head position and Radius."),t.drawGizmos);
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("Box");
        GUILayout.Label(new GUIContent("Support Address","Need help? No Problem! We're always happy to help with any issue you may have."),new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter},GUILayout.ExpandWidth(true));
        EditorGUILayout.SelectableLabel(new GUIContent("support@aedangraves.info","Need help? No Problem! We're always happy to help with any issue you may have.").text,new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold, fontSize = 13},GUILayout.ExpandWidth(true));
        EditorGUILayout.EndVertical();
        if(GUI.changed){EditorUtility.SetDirty(t);}
    }
}
