using UnityEngine;
using FIMSpace.FOptimizing;

#if OPT_SPINEANIMATOR_IMPORTED
using FIMSpace.FSpine; // Imported by hand

/// <summary>
/// FM: Helper class for single LOD level settings on FSpineAnimator
/// </summary>

// REMOVE "[CreateAssetMenu...]" AFTER CREATING LOD REFERENCE TO PUT IT INSIDE FOptimizers_Manager!
//[CreateAssetMenu(menuName = "Custom Optimizers/FSpineAnimator LOD (just first reference then remove this menu)")]

// AFTER CREATING SCRIPT CHANGE IT'S FILENAME FROM COMPONENT TYPE TO "LOD_FSpineAnimator""
public sealed class LOD_FSpineAnimator : FLOD_Base
{
    // !!!! Initial Values should be ones with THE BEST settings !!!! \\
    [Range(0f, 1f)]
    public float UsingBlend = 1f;


#region Initialization (few adjustements needed)


    /// PROVIDE SETTINGS FOR YOUR LOD
    public LOD_FSpineAnimator()
    {
        // If you don't want to use transitions (InterpolateBetween) - then set "SupportingTransitions" to false
        // But if you implement interpolation then set it to true
        SupportingTransitions = true; //!!!!!!!!!!!!!! TRUE OR FALSE - CHOOSE
        HeaderText = "Spine Animator LOD Settings";
    }

    /// DON'T CHANGE ANYTHING HERE
    public override FLOD_Base GetLODInstance()
    {
        return CreateInstance<LOD_FSpineAnimator>();
    }

    /// PROVIDE CORRECT COPYING
    // IMPLEMENTATION REQUIRED !!!
    public override FLOD_Base CreateNewCopy()
    {
        LOD_FSpineAnimator lodA = CreateInstance<LOD_FSpineAnimator>();
        lodA.CopyBase(this);
        lodA.UsingBlend = UsingBlend;
        return lodA;
    }

    /// PROVIDE APPLYING EXACT VALUES OF YOUR COMPONENTVARIABLES TO PARAMETERS OF LOD
    // IMPLEMENTATION REQUIRED !!!
    public override void SetSameValuesAsComponent(Component component)
    {
        if (component == null) Debug.LogError("[Custom OPTIMIZERS] Given component is null instead of FSpineAnimator!");

        FSpineAnimator comp = component as FSpineAnimator;

        if (comp != null)
        {
            UsingBlend = 1f - comp.BlendToOriginal;
            // Assigning component's true values to this LOD class instance
            //ParamOfFSpineAnimatorMultiplier = comp.ComponentVariable1;
            //Param2OfFSpineAnimatorMultiplier = comp.ComponentVariable2;
            //Param3OfFSpineAnimatorBool = comp.ComponentVariable3;
        }
    }


#endregion


#region Operations (Some adjustements REQUIRED)

    /// IMPLEMENT INTERPOLATION BETWEEN LOD VARIABLES, 
    /// IF YOU DON'T WANT TO THEN CHANGE SupportingTransitions TO FALSE INSIDE CONTRUCTOR (INITIALIZATION REGION)
    /// THEN YOU CAN ERASE THIS WHOLE METHOD
    public override void InterpolateBetween(FLOD_Base lodA, FLOD_Base lodB, float transitionToB)
    {
        base.InterpolateBetween(lodA, lodB, transitionToB);

        LOD_FSpineAnimator a = lodA as LOD_FSpineAnimator;
        LOD_FSpineAnimator b = lodB as LOD_FSpineAnimator;

        UsingBlend = Mathf.Lerp(a.UsingBlend, b.UsingBlend, transitionToB);
        // You don't need to define InterpolateBetween() method - then set 'SupportingTransitions = false' inside LODSController_FSpineAnimator script
    }


    /// IMPLEMENT APPLYING YOUR LOD SETTINGS TO TARGET COMPONENT (YOU CAN REFERENCE FROM INITIAL SETTINGS (there are not multipliers but components values, check SetSameValuesAsComponent() ) FOR MULTIPLIERS)
    // IMPLEMENTATION REQUIRED !!!
    public override void ApplySettingsToComponent(Component component, FLOD_Base initialSettingsReference)
    {
        // Casting LOD to correct type
        LOD_FSpineAnimator initialSettings = initialSettingsReference as LOD_FSpineAnimator;

#region Security

        // Checking if casting is right
        if (initialSettings == null) { Debug.Log("[Custom OPTIMIZERS] Target LOD is not FSpineAnimator LOD or is null"); return; }

#endregion

        // YOUR CODE
        FSpineAnimator comp = component as FSpineAnimator;

        comp.BlendToOriginal = 1f - (UsingBlend * initialSettings.UsingBlend);
        // Setting new settings to optimized component
        // YOUR CODE END

        base.ApplySettingsToComponent(component, initialSettingsReference); // Enable disable component basing on "Enable" variable from parent class
    }

#endregion


#region Auto Settings (YOU CAN ERASE IT ALL IF YOU DON'T WANT MAKE YOUR WORK FASTER WITH AUTO SETTINGS)


    /// IMPLEMENT AUTO SETTING PARAMETERS FOR DIFFERENT LOD FOR LODS COUNT (IF YOU DONT WANT YOU DONT NEED TO IMPLEMENT THIS)
    public override void SetAutoSettingsAsForLODLevel(int lodIndex, int lodCount, Component source)
    {
        FSpineAnimator comp = source as FSpineAnimator;
        if (comp == null) Debug.LogError("[Custom OPTIMIZERS] Given component for reference values is null or is not FSpineAnimator Component!");

        // REMEMBER: LOD = 0 is not nearest but one after nearest
        // Trying to auto configure universal LOD settings

        UsingBlend = 1f;

        name = "LOD" + (lodIndex + 2); // + 2 to view it in more responsive way for user inside inspector window
    }


    /// AUTO SETTING SETTINGS FOR CULLED LOD
    public override void SetSettingsAsForCulled(Component component)
    {
        base.SetSettingsAsForCulled(component);
        UsingBlend = 0f;

    }

    public override void SetSettingsAsForHidden(Component component)
    {
        base.SetSettingsAsForHidden(component);
        UsingBlend = 0f;
    }


    /// AUTO SETTING SETTINGS FOR NEAREST (HIGHEST QUALITY) LOD (DONT NEED TO DO THIS IF INITIAL VALUES FOR YOUR VARIABLES ARE ALREADY MAX)
    public override void SetSettingsAsForNearest(Component component)
    {
        base.SetSettingsAsForNearest(component);
        UsingBlend = 1f;
    }


#endregion


    // NOTHING TO CHANGE, BUT ASSIGN THIS LOD TO FOptimizers_Manager on scene by inspector window
    public override FComponentLODsController GenerateLODController(Component target, FOptimizer_Base optimizer)
    {
        FSpineAnimator c = target as FSpineAnimator;
        if (!c) c = target.GetComponentInChildren<FSpineAnimator>();
        if (c) if (!optimizer.ContainsComponent(c))
            {
                return new FComponentLODsController(optimizer, c, "Spine Animator Blending Properties", this);
            }

        return null;
    }

}

#endif