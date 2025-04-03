
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
[CustomEditor(typeof(InfiniGun)),CanEditMultipleObjects,InitializeOnLoad]
public class InfiniGun_Editor : Editor
{
    
    private static GUIStyle buttonStyleOff = null;
    private static GUIStyle buttonStyleOn = null;
    InfiniGun t;
    static InfiniGun st;
    SerializedObject GTarg;
    SerializedProperty Attachments_GL;
    SerializedProperty fireSounds_GL;
    SerializedProperty suppressedSounds_GL;
    SerializedProperty dryShot_GL;
    SerializedProperty boltIn_GL;
    SerializedProperty boltOut_GL;
    static bool showFireSounds = false;
    static bool showSuppressedSounds = false;
    static bool showDryshotSounds = false;
    void OnEnable(){
        t = (InfiniGun)target;
        st = t;
        GTarg = new SerializedObject(t);
        Attachments_GL = GTarg.FindProperty("attachmentMountPoints");
        fireSounds_GL = GTarg.FindProperty("fireSounds");
        suppressedSounds_GL = GTarg.FindProperty("suppressedSounds");
        dryShot_GL = GTarg.FindProperty("dryShot");
        boltIn_GL = GTarg.FindProperty("boltIn");
        boltOut_GL = GTarg.FindProperty("boltOut");
    }

    public override void OnInspectorGUI(){
        GTarg.Update();

            if (buttonStyleOff == null){
            buttonStyleOff = "Button";
            buttonStyleOn = new GUIStyle(buttonStyleOff);
            buttonStyleOn.normal.background = buttonStyleOn.active.background;
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("",GUI.skin.horizontalSlider);
        GUILayout.Label("Bullet Setup",new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold, fontSize = 13},GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();
        t.canFire = EditorGUILayout.ToggleLeft(new GUIContent("Can Fire?", "Can gun fire when equipped?"),t.canFire);
        EditorGUILayout.Space();
        t.methodType = (InfiniGun.MethodType)EditorGUILayout.EnumPopup(new GUIContent("Firing Method", "Determines which firing logic will be used. \n The Projectile method will create a particle system at the 'Muzzle Tip' and launch a particle that can be effected by other physics based forces. \n The Raycast method will cast a ray from the 'Muzzle Tip' and will NOT be effected by other physics based forces."),t.methodType);
        EditorGUILayout.Space();
        t.muzzleEnd = (Transform)EditorGUILayout.ObjectField(new GUIContent("Muzzle Tip", "The Muzzle tip is a transform representing the guns muzzle. The Muzzle Tip must be a child/sub-child to this object and should be placed where you wish the bullets to be fired from and facing outward from the gun."),t.muzzleEnd,typeof(Transform),true);
        if(!t.muzzleEnd){EditorGUILayout.HelpBox("The Muzzle Tip transform is required for operation. The Muzzle Tip must be a child/sub-child to this object and should be placed where you wish the bullets to be fired from and facing outward from the gun.",MessageType.Error);}
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField(new GUIContent("Damage Range", "Determines the minimum and maximum damage that can be dealt by a singe round."),GUILayout.MinWidth(130));
        EditorGUILayout.LabelField("Min",GUILayout.Width(20)); 
        t.minDamagePerround = EditorGUILayout.FloatField(t.minDamagePerround,GUILayout.MaxWidth(30));
        EditorGUILayout.MinMaxSlider(ref t.minDamagePerround,ref t.maxDamagePerround,0f,100f,GUILayout.ExpandWidth(true));
        t.maxDamagePerround = EditorGUILayout.FloatField(t.maxDamagePerround,GUILayout.MaxWidth(30));   
        EditorGUILayout.LabelField("Max",GUILayout.Width(25));
        EditorGUILayout.EndHorizontal();
    
        
        t.headShotMultiplier = EditorGUILayout.Slider(new GUIContent("Headshot Multiplier", "If the hit target allows; Landing a head shot will multiply the effective damage range by this value."),t.headShotMultiplier,1,10);
        t.damageDecreaseOverDistance = EditorGUILayout.Slider(new GUIContent("Damage Loss Perunit", "How much damage will be lost to distance perunit?"),t.damageDecreaseOverDistance,0,1);
        t.CollidesWith = EditorGUILayout.MaskField(new GUIContent("Collides With","Determines what Layers can be effected by the bullets."),t.CollidesWith,InternalEditorUtility.layers);
        
        if(t.methodType == InfiniGun.MethodType.Projectile){
            t.muzzleVelocity = EditorGUILayout.IntSlider(new GUIContent("Muzzle Velocity", "Determines how fast the projectile will travel."), t.muzzleVelocity,10,2000);
            t.bulletDrop = EditorGUILayout.Slider(new GUIContent("Bullet Drop", "Determines how much gravity will effect the projectile."), t.bulletDrop, 0, 15);
            t.bulletColor = EditorGUILayout.ColorField(new GUIContent("Bullet Color", "Color of the projectile."),t.bulletColor);
        }else{
            t.maxRange = EditorGUILayout.Slider(new GUIContent("Max Hit Register Dist.", "Determines how far the raycast will go."), t.maxRange,0,2500);
        }
        t.coneOfAccuracy = EditorGUILayout.Slider(new GUIContent("Cone of Accuracy", "If above zero, this determines the Cone Of Accuracy."),t.coneOfAccuracy,0,15);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("",GUI.skin.horizontalSlider);   
        GUILayout.Label("Ammo Capacity Settings",new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold, fontSize = 13},GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();
        t.reloadKey = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Reload Key", "Determines what key needs to be pressed to start a reload cycle."),t.reloadKey);
        t.ammoCapacity = EditorGUILayout.IntSlider(new GUIContent("Magazine Capacity", "Determines how big the magazine is."),t.ammoCapacity,0,900);
        t.reserveAmmoCapacity =  EditorGUILayout.IntSlider(new GUIContent("Ammo Reserve", "Determines how big the ammo reserve is."),t.reserveAmmoCapacity,0,999);
        t.reloadTime = EditorGUILayout.Slider(new GUIContent("Reload Time", "Determines how long a reload cycle takes."),t.reloadTime,0,30);
        t.equipPreloaded = EditorGUILayout.ToggleLeft(new GUIContent("Preloaded on First Equip?", "Determines if the player will need to load the gun the first time it's equipped."),t.equipPreloaded);
        t.sequentialReload = EditorGUILayout.ToggleLeft(new GUIContent("Load Gun Sequentially?", "Determines if the gun will reload one round at a time. This is useful for shotgun like firearms."), t.sequentialReload);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("",GUI.skin.horizontalSlider);
        GUILayout.Label("Available Fire Modes",new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold, fontSize = 13},GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();        
        if(t.SelectedFiremodes != null&&t.SelectedFiremodes.Count == 7){
            if(GUILayout.Button("Safe",t.SelectedFiremodes[0]?buttonStyleOn:buttonStyleOff)){t.SelectedFiremodes[0]= !t.SelectedFiremodes[0];}
            if(GUILayout.Button("Semi",t.SelectedFiremodes[1]?buttonStyleOn:buttonStyleOff)){t.SelectedFiremodes[1]= !t.SelectedFiremodes[1];}
            if(GUILayout.Button("Auto",t.SelectedFiremodes[2]?buttonStyleOn:buttonStyleOff)){t.SelectedFiremodes[2]= !t.SelectedFiremodes[2];}
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Burst",t.SelectedFiremodes[3]?buttonStyleOn:buttonStyleOff)){t.SelectedFiremodes[3]= !t.SelectedFiremodes[3];}
            if(GUILayout.Button("Auto Burst",t.SelectedFiremodes[4]?buttonStyleOn:buttonStyleOff)){t.SelectedFiremodes[4]= !t.SelectedFiremodes[4];}
            if(GUILayout.Button("Shotgun",t.SelectedFiremodes[5]?buttonStyleOn:buttonStyleOff)){t.SelectedFiremodes[5]= !t.SelectedFiremodes[5];}
            if(GUILayout.Button("Auto Shotgun",t.SelectedFiremodes[6]?buttonStyleOn:buttonStyleOff)){t.SelectedFiremodes[6]= !t.SelectedFiremodes[6];}
        }
        else{t.SelectedFiremodes = new List<bool>(){false,true,false,false,false,false,false};}
        EditorGUILayout.EndHorizontal();
        t.fireModeSwitchKey = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Fire Mode Switch Key", "When Pressed, The Gun's current firemode will change."),t.fireModeSwitchKey);
        if(t.SelectedFiremodes[1]||t.SelectedFiremodes[2]){EditorGUILayout.Space();
            GUILayout.BeginVertical("box");
            GUILayout.Label("Semi/Auto mode settings",new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold},GUILayout.ExpandWidth(true));
            t.fireRate = EditorGUILayout.IntSlider(new GUIContent("Fire Rate", "Gun's fire rate when in 'Semi' or any 'Auto' firemode."),t.fireRate,25,1500);
            GUILayout.EndVertical();
        }
        if(t.SelectedFiremodes[3]||t.SelectedFiremodes[4]){EditorGUILayout.Space();
            GUILayout.BeginVertical("box");
            GUILayout.Label("Burst mode settings",new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold},GUILayout.ExpandWidth(true));
            t.burstCount = EditorGUILayout.IntSlider(new GUIContent("Rounds Per-burst", "Determines how many rounds will be fired in one burst."),t.burstCount,2,9);
            t.burstRate = EditorGUILayout.IntSlider(new GUIContent("Burst Fire Rate", "Determines the fire rate of a burst shot."),t.burstRate,100,1000);
            GUILayout.EndVertical();
        }
        if(t.SelectedFiremodes[5]||t.SelectedFiremodes[6]){EditorGUILayout.Space();
            GUILayout.BeginVertical("box");
            GUILayout.Label("Shotgun mode settings",new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold},GUILayout.ExpandWidth(true));
            t.pelletCount = EditorGUILayout.IntSlider(new GUIContent("Pellets Per-shell", "Determines how many pellets are in a single shell."), t.pelletCount,2,9);
            GUILayout.EndVertical();
        }
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("",GUI.skin.horizontalSlider,GUILayout.MaxHeight(6));
        EditorGUILayout.Space();

        GUILayout.Label("ADS Settings",new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold, fontSize = 13},GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();
        t.canAimDownSights = EditorGUILayout.ToggleLeft(new GUIContent("Allow ADS?", "Allow player to aim down the sights of the gun?"), t.canAimDownSights);
        GUI.enabled = t.canAimDownSights;
        t.fOVOnADS = EditorGUILayout.Slider(new GUIContent("FOV on ADS", "The camera's Field Of View when the player is aiming down the sights."), t.fOVOnADS,10,60);
        t.aDSSmoothTime = EditorGUILayout.Slider(new GUIContent("ADS Smooth Time", "Determines how smooth the transition from hip fire to ADS is."),t.aDSSmoothTime,0.1f,5);
        t.gunADSCenterY = EditorGUILayout.FloatField(new GUIContent("Aiming Center Y", "The point at which the center of the gun's built in sight is located along the Y axis."),t.gunADSCenterY);
        GUI.enabled = true;
        t.steadyScope = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Steady Scope Key","Determines what key needs to be pressed to steady scope if attached and set to sway."),t.steadyScope);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("",GUI.skin.horizontalSlider,GUILayout.MaxHeight(6));
        EditorGUILayout.Space();

        GUILayout.Label("Attachment Settings",new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold, fontSize = 13},GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();       

        for(int i =0; i<Attachments_GL.arraySize;i++){
            SerializedProperty Ref = Attachments_GL.GetArrayElementAtIndex(i);
            SerializedProperty mntpointT = Ref.FindPropertyRelative("mountPoint");
            SerializedProperty atchPrefab = Ref.FindPropertyRelative("Attachment");
            EditorGUILayout.Space();
            GUILayout.BeginVertical("box");
            GUILayout.Label("Attachment No."+(i+1),new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold},GUILayout.ExpandWidth(true));
            mntpointT.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Attachment Mount Point", "Transform the below attachment will be parented to - attachment will inherit position, rotation, and scale from this transform. "), mntpointT.objectReferenceValue, typeof(Transform),true);
            atchPrefab.objectReferenceValue = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Attachment Prefab", "Attachment prefab will be instantiate as a child to the above mounting point. \n If an 'Attachment' component is found on the prefab, stat modifiers from the component will be applied to this gun object."), atchPrefab.objectReferenceValue, typeof(GameObject),false);
            if(GUILayout.Button(new GUIContent("Remove Attachment", "Remove this attachment and mount point entry."))){if(this.t.attachmentMountPoints[i].mountPoint&& this.t.attachmentMountPoints[i].mountPoint.GetComponentInChildren<InfiniGunAttachment>()){ DestroyImmediate(this.t.attachmentMountPoints[i].mountPoint.GetComponentInChildren<InfiniGunAttachment>().gameObject);}    this.t.attachmentMountPoints.RemoveAt(i);}
            GUILayout.EndVertical();
        }
          
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
         if(GUILayout.Button(new GUIContent("Add Attachment", "Add new attachment entry."))){ this.t.attachmentMountPoints.Add(new InfiniGun.AttachmentMountPoints());}
         if(GUILayout.Button(new GUIContent("Update Attachments", "Update and apply attachment stat modifiers on this gun. \n will automatically be updated upon entering playmode."))){ this.t.InitializeAttachments();}
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("",GUI.skin.horizontalSlider,GUILayout.MaxHeight(6));
        EditorGUILayout.Space();

        GUILayout.Label("Gun Movement Settings",new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold, fontSize = 13},GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();
        t.swayAmount = EditorGUILayout.Slider(new GUIContent("Sway Amount", "Determines how much the gun will sway when camera is moved."), t.swayAmount,1,100);
        EditorGUILayout.Space();
        t.useRecoil = EditorGUILayout.ToggleLeft(new GUIContent("Apply Recoil Forces?", "Determines if recoil forces will be applied to gun after firing."),t.useRecoil);
        GUI.enabled = t.useRecoil;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Recoil Force Intensity", "Determines how much force - independently in each direction - will be applied to gun."),GUILayout.MinWidth(90),GUILayout.MaxWidth(220));
        t.recoilForceIntensity = EditorGUILayout.Vector3Field("",t.recoilForceIntensity,GUILayout.MaxWidth(175));
        EditorGUILayout.EndHorizontal();
        t.recoilRotationMultiplier = EditorGUILayout.Slider(new GUIContent("Recoil Rotation Magnitude", "Determines the magnitude of recoil rotation."),t.recoilRotationMultiplier,0,10);
        GUI.enabled = true;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("",GUI.skin.horizontalSlider,GUILayout.MaxHeight(6));
        EditorGUILayout.Space();

        GUILayout.Label("Audio Settings",new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold, fontSize = 13},GUILayout.ExpandWidth(true));
        
        #region Normal Fire Sounds
        showFireSounds = EditorGUILayout.BeginFoldoutHeaderGroup(showFireSounds,new GUIContent("Gun Shot Clips", "Audio clips available to gun to use as a 'gun shot' sound."));
            if(showFireSounds){GUILayout.BeginVertical("box");
            for(int i=0; i<fireSounds_GL.arraySize; i++){
            SerializedProperty FS_ref = fireSounds_GL.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginHorizontal("box");
            FS_ref.objectReferenceValue = EditorGUILayout.ObjectField("Clip "+(i+1)+":",FS_ref.objectReferenceValue,typeof(AudioClip),false);
            if(GUILayout.Button(new GUIContent("X", "Remove this clip"),GUILayout.MaxWidth(20))){ this.t.fireSounds.RemoveAt(i);}
            EditorGUILayout.EndHorizontal();
        }
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            
            if(GUILayout.Button(new GUIContent("Add Clip", "Add new clip entry"))){ this.t.fireSounds.Add(null);}
            if(GUILayout.Button(new GUIContent("Remove All Clips", "Remove all clip entries"))){ this.t.fireSounds.Clear();}
            EditorGUILayout.EndHorizontal();
            GUILayout.EndVertical();
            }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space();
        #endregion

        #region Suppressed Fire Sounds
        showSuppressedSounds = EditorGUILayout.BeginFoldoutHeaderGroup(showSuppressedSounds,new GUIContent("Suppressed Gun Shot Clips", "Audio clips available to gun to use as a 'suppressed gun shot' sound."));
            if(showSuppressedSounds){GUILayout.BeginVertical("box");
            for(int i=0; i<suppressedSounds_GL.arraySize; i++){
            SerializedProperty SFS_ref = suppressedSounds_GL.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginHorizontal("box");
            SFS_ref.objectReferenceValue = EditorGUILayout.ObjectField("Clip "+(i+1)+":",SFS_ref.objectReferenceValue,typeof(AudioClip),false);
            if(GUILayout.Button(new GUIContent("X", "Remove this clip"),GUILayout.MaxWidth(20))){ this.t.suppressedSounds.RemoveAt(i);}
            EditorGUILayout.EndHorizontal();
        }
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            
            if(GUILayout.Button(new GUIContent("Add Clip", "Add new clip entry"))){ this.t.suppressedSounds.Add(null);}
            if(GUILayout.Button(new GUIContent("Remove All Clips", "Remove all clip entries"))){ this.t.suppressedSounds.Clear();}
            EditorGUILayout.EndHorizontal();
            GUILayout.EndVertical();
            }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space();
        #endregion

        #region Dry shot Sounds
        showDryshotSounds = EditorGUILayout.BeginFoldoutHeaderGroup(showDryshotSounds,new GUIContent("Dryfire Sounds", "Audio clips available to gun to use as a 'dry shot' sound when magazine is empty."));
            if(showDryshotSounds){GUILayout.BeginVertical("box");
            for(int i=0; i<dryShot_GL.arraySize; i++){
            SerializedProperty DFS_ref = dryShot_GL.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginHorizontal("box");
            DFS_ref.objectReferenceValue = EditorGUILayout.ObjectField("Clip "+(i+1)+":",DFS_ref.objectReferenceValue,typeof(AudioClip),false);
            if(GUILayout.Button(new GUIContent("X", "Remove this clip"),GUILayout.MaxWidth(20))){ this.t.dryShot.RemoveAt(i);}
            EditorGUILayout.EndHorizontal();
        }
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            
            if(GUILayout.Button(new GUIContent("Add Clip", "Add new clip entry"))){ this.t.dryShot.Add(null);}
            if(GUILayout.Button(new GUIContent("Remove All Clips", "Remove all clip entries"))){ this.t.dryShot.Clear();}
            EditorGUILayout.EndHorizontal();
            GUILayout.EndVertical();
            }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space();
        #endregion

        EditorGUILayout.Space();

        boltOut_GL.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Un-Bolt", "Audio clip that will be played when bolt/slide is pulled out."),boltOut_GL.objectReferenceValue,typeof(AudioClip),false);
        boltIn_GL.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Bolt Set", "Audio clip that will be played when bolt/slide is set."),boltIn_GL.objectReferenceValue,typeof(AudioClip),false);
        EditorGUILayout.Space();
        t.Reprime = EditorGUILayout.ToggleLeft(new GUIContent("Re-bolt After Fire", "Should a bolt cycle play after every shot?"),t.Reprime);
        EditorGUILayout.Space();
        t.sFXVolume = EditorGUILayout.Slider(new GUIContent("Volume", "Volume at which above audio clips will be played."),t.sFXVolume,0,1);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("",GUI.skin.horizontalSlider,GUILayout.MaxHeight(6));
        EditorGUILayout.Space();
        GUILayout.Label("Equip Settings",new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold, fontSize = 13},GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();
        t.gunClass = (InfiniGun.GunClass)EditorGUILayout.EnumPopup(new GUIContent("Gun Class","Determines what class the gun is."),t.gunClass);
        EditorGUILayout.Space();
        t.dropKey = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Drop Key", "When this key is pressed the gun will un-equip itself from Camera"), t.dropKey);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Equip Position Relative to Camera", "The position relative to the camera where the gun will be equipped  to."),GUILayout.MinWidth(90),GUILayout.MaxWidth(220));
        t.equipPosition = EditorGUILayout.Vector3Field("",t.equipPosition,GUILayout.MaxWidth(175));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();



        EditorGUILayout.Space();
        EditorGUILayout.LabelField("",GUI.skin.horizontalSlider,GUILayout.MaxHeight(6));
        EditorGUILayout.Space();
        GUILayout.Label("Visuals Settings",new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold, fontSize = 13},GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();
        
        t.autoGenerateUIElements = EditorGUILayout.ToggleLeft(new GUIContent("Auto Generate UI?", "Generate and use a basic UI at runtime?"),t.autoGenerateUIElements);
       
        if(t.autoGenerateUIElements){t.uiFont = (Font)EditorGUILayout.ObjectField(new GUIContent("Font to use in UI", "A custom font that will be used in generated UI."), t.uiFont,typeof(Font),false);}
        EditorGUILayout.Space();
        t.autoGenCrosshair = EditorGUILayout.ToggleLeft(new GUIContent("Auto Generate Crosshair?","Generate and use a basic crosshair at runtime?"),t.autoGenCrosshair);
        if(t.autoGenCrosshair){EditorGUILayout.BeginHorizontal(); EditorGUILayout.PrefixLabel(new GUIContent("Crosshair Sprite","Sprite to use a s a crosshair")); t.CrosshairImg = (Sprite)EditorGUILayout.ObjectField(t.CrosshairImg,typeof(Sprite),false);EditorGUILayout.EndHorizontal();}
        EditorGUILayout.Space();
        t.muzzleFlashPrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Muzzle Flash", "Prefab reference to a particle system gameobject to use as a muzzle flash effect. This will be instantiated at the Muzzle Tip."),t.muzzleFlashPrefab, typeof(GameObject),false);
        t.shellEjectionFX_prefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Shell Ejection Particals", "Prefab reference to a particle system gameobject to use as a shell ejection effect."),t.shellEjectionFX_prefab,typeof(GameObject),false);
        if(t.shellEjectionFX_prefab){t.shellEjectionFX_pos = (Transform)EditorGUILayout.ObjectField(new GUIContent("Ejection Window Position", "The Transform at which the 'Shell Ejection Particals' will be instantiated under."),t.shellEjectionFX_pos,typeof(Transform),true);}
        t.impactFX = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Impact Particles", "Prefab Reference to a particle system gameobject that will be instantiated at point of impact."),t.impactFX, typeof(GameObject),false);
        t.impactDecal = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Impact Decal", "Prefab reference to a gameobject to be used as a impact decal, and will be instantiated at point of impact."),t.impactDecal, typeof(GameObject),false);
        if(t.impactDecal){
            EditorGUI.indentLevel++;
            t.flipDecal = EditorGUILayout.ToggleLeft(new GUIContent("Flip Decal?", "Should the decal be flipped 180°?"),t.flipDecal);
            t.impactDecalDecayTime = EditorGUILayout.Slider(new GUIContent("Decal Decay", "Determine how long the decal can exist in the scene before being automatically destroyed."),t.impactDecalDecayTime,0,30);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();
        t.drawSetupGizmos = EditorGUILayout.ToggleLeft(new GUIContent("Draw Setup Gizmos", "Draw Gizmos to assist gun setup. \n\n Red, Green, and Blue Gizmo Lines represent recoil forces respectively. The Yellow Gizmo Line represents the recoil force bias. \n\n The Cyan Gizmo Wire Sphere's size represents Recoil rotation magnitude. The Red sphere represents the point at which the center of the gun's built in sight is located along the Y axis.\n\n The Magenta Gizmo Wire Sphere represents the Muzzle Tip. The Magenta Gizmo Lines represent the Cone of Accuracy."),t.drawSetupGizmos);

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("Box");
        GUILayout.Label(new GUIContent("Support Address","Need help? No Problem! We're always happy to help with any issue you may have."),new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter},GUILayout.ExpandWidth(true));
        EditorGUILayout.SelectableLabel(new GUIContent("support@aedangraves.info","Need help? No Problem! We're always happy to help with any issue you may have.").text,new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold, fontSize = 13},GUILayout.ExpandWidth(true));
        EditorGUILayout.EndVertical();
        if(GUI.changed){EditorUtility.SetDirty(t);GTarg.ApplyModifiedProperties();}
        
    }


    [MenuItem("CONTEXT/InfiniGun/Preset - Assault Rifle")]
    static void Preset_AR(){
        st.minDamagePerround = 18;
        st.maxDamagePerround = 24;
        st.headShotMultiplier = 1.8f;
        st.damageDecreaseOverDistance = 0.03f;
        if(st.methodType == InfiniGun.MethodType.Projectile){
            st.muzzleVelocity = 970;
            st.bulletDrop = 9.81f;
        }
        else{st.maxRange = 1000;}
        st.ammoCapacity = 30;
        st.reserveAmmoCapacity = 120;
        st.reloadTime = 2.5f;
        st.sequentialReload = false;
        st.SelectedFiremodes[0] = false;
        st.SelectedFiremodes[1] = true;
        st.SelectedFiremodes[2] = true;
        st.SelectedFiremodes[3] = false;
        st.SelectedFiremodes[4] = false;
        st.SelectedFiremodes[5] = false;
        st.SelectedFiremodes[6] = false;
        st.fireRate = 715;
        st.swayAmount = 25;
        st.recoilForceIntensity = new Vector3(1,1.2f,1.5f);
        st.recoilRotationMultiplier = 1.5f;
        EditorUtility.SetDirty(st);
    }

    
    [MenuItem("CONTEXT/InfiniGun/Preset - SMG")]
    static void Preset_SMG(){
        st.minDamagePerround = 12;
        st.maxDamagePerround = 20;
        st.headShotMultiplier = 2f;
        st.damageDecreaseOverDistance = 0.1f;
        if(st.methodType == InfiniGun.MethodType.Projectile){
            st.muzzleVelocity = 900;
            st.bulletDrop = 9.81f;
        }
        else{st.maxRange = 1000;}
        st.ammoCapacity = 25;
        st.reserveAmmoCapacity = 120;
        st.reloadTime = 2f;
        st.sequentialReload = false;
        st.SelectedFiremodes[0] = false;
        st.SelectedFiremodes[1] = true;
        st.SelectedFiremodes[2] = true;
        st.SelectedFiremodes[3] = false;
        st.SelectedFiremodes[4] = false;
        st.SelectedFiremodes[5] = false;
        st.SelectedFiremodes[6] = false;
        st.fireRate = 900;
        st.swayAmount = 75;
        st.recoilForceIntensity = new Vector3(1.5f,1.5f,1);
        st.recoilRotationMultiplier = 1.2f;
        EditorUtility.SetDirty(st);
    }


    [MenuItem("CONTEXT/InfiniGun/Preset - Sniper Rifle")]
    static void Preset_SR(){
        st.minDamagePerround = 60;
        st.maxDamagePerround = 85;
        st.headShotMultiplier = 2.5f;
        st.damageDecreaseOverDistance = 0.01f;
        if(st.methodType == InfiniGun.MethodType.Projectile){
            st.muzzleVelocity = 1200;
            st.bulletDrop = 9.81f;
        }
        else{st.maxRange = 10000;}
        st.ammoCapacity = 7;
        st.reserveAmmoCapacity = 45;
        st.reloadTime = 3f;
        st.sequentialReload = false;
        st.SelectedFiremodes[0] = false;
        st.SelectedFiremodes[1] = true;
        st.SelectedFiremodes[2] = false;
        st.SelectedFiremodes[3] = false;
        st.SelectedFiremodes[4] = false;
        st.SelectedFiremodes[5] = false;
        st.SelectedFiremodes[6] = false;
        st.fireRate = 65;
        st.swayAmount = 25;
        st.recoilForceIntensity = new Vector3(2,6,5);
        st.recoilRotationMultiplier = 2f;
        EditorUtility.SetDirty(st);
    }


    [MenuItem("CONTEXT/InfiniGun/Preset - Shotgun")]
    static void Preset_SG(){
        st.minDamagePerround = 25;
        st.maxDamagePerround = 35;
        st.headShotMultiplier = 2f;
        st.damageDecreaseOverDistance = 1f;
        if(st.methodType == InfiniGun.MethodType.Projectile){
            st.muzzleVelocity = 850;
            st.bulletDrop = 9.81f;
        }
        else{st.maxRange = 1000;}
        st.ammoCapacity = 8;
        st.reserveAmmoCapacity = 58;
        st.reloadTime = 4f;
        st.sequentialReload = true;
        st.SelectedFiremodes[0] = false;
        st.SelectedFiremodes[1] = false;
        st.SelectedFiremodes[2] = false;
        st.SelectedFiremodes[3] = false;
        st.SelectedFiremodes[4] = false;
        st.SelectedFiremodes[5] = true;
        st.SelectedFiremodes[6] = false;
        st.fireRate = 65;
        st.pelletCount = 9;
        st.swayAmount = 50;
        st.recoilForceIntensity = new Vector3(1,3,3);
        st.recoilRotationMultiplier = 2f;
        EditorUtility.SetDirty(st);
    }

       [MenuItem("CONTEXT/InfiniGun/Preset - DMR")]
    static void Preset_DMR(){
        st.minDamagePerround = 20;
        st.maxDamagePerround = 30;
        st.headShotMultiplier = 1.2f;
        st.damageDecreaseOverDistance = 0.2f;
        if(st.methodType == InfiniGun.MethodType.Projectile){
            st.muzzleVelocity = 970;
            st.bulletDrop = 9.81f;
        }
        else{st.maxRange = 10000;}
        st.ammoCapacity = 21;
        st.reserveAmmoCapacity = 120;
        st.reloadTime = 3.5f;
        st.sequentialReload = false;
        st.SelectedFiremodes[0] = false;
        st.SelectedFiremodes[1] = true;
        st.SelectedFiremodes[2] = false;
        st.SelectedFiremodes[3] = true;
        st.SelectedFiremodes[4] = false;
        st.SelectedFiremodes[5] = false;
        st.SelectedFiremodes[6] = false;
        st.fireRate = 700;
        st.burstCount = 3;
        st.burstRate = 900;
        st.swayAmount = 50;
        st.recoilForceIntensity = new Vector3(1f,1f,1f);
        st.recoilRotationMultiplier = 1f;
        EditorUtility.SetDirty(st);
    }

     [MenuItem("CONTEXT/InfiniGun/Preset - Pistol")]
    static void Preset_Pistol(){
        st.minDamagePerround = 35;
        st.maxDamagePerround = 45;
        st.headShotMultiplier = 1.5f;
        st.damageDecreaseOverDistance = 0.2f;
        if(st.methodType == InfiniGun.MethodType.Projectile){
            st.muzzleVelocity = 850;
            st.bulletDrop = 9.81f;
        }
        else{st.maxRange = 1000;}
        st.ammoCapacity = 18;
        st.reserveAmmoCapacity = 40;
        st.reloadTime = 3.5f;
        st.sequentialReload = false;
        st.SelectedFiremodes[0] = false;
        st.SelectedFiremodes[1] = true;
        st.SelectedFiremodes[2] = false;
        st.SelectedFiremodes[3] = false;
        st.SelectedFiremodes[4] = false;
        st.SelectedFiremodes[5] = false;
        st.SelectedFiremodes[6] = false;
        st.fireRate = 578;
        st.swayAmount = 50;
        st.recoilForceIntensity = new Vector3(1f,1f,5f);
        st.recoilRotationMultiplier = 3f;
        EditorUtility.SetDirty(st);
    }

}

