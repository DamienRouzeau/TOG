using UnityEngine;
using FIMSpace.FOptimizing;
using FIMSpace.Basics;

/// <summary>
/// FM: Helper class for single LOD level settings on FBasic_RotateSpinSin
/// </summary>

// REMOVE "[CreateAssetMenu...]" AFTER CREATING LOD REFERENCE TO PUT IT INSIDE FOptimizers_Manager!
[CreateAssetMenu(menuName = "FImpossible Creations/DEMO/FBasic_RotateSpinSin LOD (just first reference then remove this menu)")]

// AFTER CREATING SCRIPT CHANGE IT'S FILENAME FROM COMPONENT TYPE TO "OptDemo_LOD_FBasic_RotateSpinSin""
public sealed class OptDemo_LOD_FBasic_RotateSpinSin : FLOD_Base
{
    // !!!! Initial Values should be ones with THE BEST settings !!!! \\
    [Space(4)]
    [Range(0f, 1f)]
    public float RotationRange = 1f;

    #region Initialization (few adjustements needed)


    /// PROVIDE SETTINGS FOR YOUR LOD
    public OptDemo_LOD_FBasic_RotateSpinSin()
    {
        // If you don't want to use transitions (InterpolateBetween) - then set "SupportingTransitions" to false
        // But if you implement interpolation then set it to true
        SupportingTransitions = false;
        HeaderText = "DEMO SpinSin LOD Settings";
    }

    /// DON'T CHANGE ANYTHING HERE
    public override FLOD_Base GetLODInstance()
    {
        return CreateInstance<OptDemo_LOD_FBasic_RotateSpinSin>();
    }

    /// PROVIDE CORRECT COPYING
    // IMPLEMENTATION REQUIRED !!!
    public override FLOD_Base CreateNewCopy()
    {
        OptDemo_LOD_FBasic_RotateSpinSin lodA = CreateInstance<OptDemo_LOD_FBasic_RotateSpinSin>();
        lodA.CopyBase(this);
        lodA.RotationRange = 1;
        return lodA;
    }

    /// PROVIDE APPLYING EXACT VALUES OF YOUR COMPONENTVARIABLES TO PARAMETERS OF LOD
    // IMPLEMENTATION REQUIRED !!!
    public override void SetSameValuesAsComponent(Component component)
    {
        if (component == null) Debug.LogError("Given component is null instead of FBasic_RotateSpinSin!");

        FBasic_RotateSpinSin comp = component as FBasic_RotateSpinSin;

        if (comp != null)
        {
            // Assigning component's true values to this LOD class instance
            RotationRange = comp.RotationRange;
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

        OptDemo_LOD_FBasic_RotateSpinSin a = lodA as OptDemo_LOD_FBasic_RotateSpinSin;
        OptDemo_LOD_FBasic_RotateSpinSin b = lodB as OptDemo_LOD_FBasic_RotateSpinSin;

        RotationRange = Mathf.Lerp(a.RotationRange, b.RotationRange, transitionToB);
        // You don't need to define InterpolateBetween() method - then set 'SupportingTransitions = false' inside LODSController_FBasic_RotateSpinSin script
    }


    /// IMPLEMENT APPLYING YOUR LOD SETTINGS TO TARGET COMPONENT (YOU CAN REFERENCE FROM INITIAL SETTINGS (there are not multipliers but components values, check SetSameValuesAsComponent() ) FOR MULTIPLIERS)
    // IMPLEMENTATION REQUIRED !!!
    public override void ApplySettingsToComponent(Component component, FLOD_Base initialSettingsReference)
    {
        // Casting LOD to correct type
        OptDemo_LOD_FBasic_RotateSpinSin initialSettings = initialSettingsReference as OptDemo_LOD_FBasic_RotateSpinSin;

        #region Security

        // Checking if casting is right
        if (initialSettings == null) { Debug.Log("Target LOD is not FBasic_RotateSpinSin LOD or is null"); return; }

        #endregion

        // YOUR CODE
        FBasic_RotateSpinSin comp = component as FBasic_RotateSpinSin;

        // Setting new settings to optimized component
        comp.RotationRange = RotationRange * initialSettings.RotationRange;
        // YOUR CODE END

        base.ApplySettingsToComponent(component, initialSettingsReference); // Enable disable component basing on "Enable" variable from parent class
    }

    #endregion


    #region Auto Settings (YOU CAN ERASE IT ALL IF YOU DON'T WANT MAKE YOUR WORK FASTER WITH AUTO SETTINGS)


    /// IMPLEMENT AUTO SETTING PARAMETERS FOR DIFFERENT LOD FOR LODS COUNT (IF YOU DONT WANT YOU DONT NEED TO IMPLEMENT THIS)
    public override void SetAutoSettingsAsForLODLevel(int lodIndex, int lodCount, Component source)
    {
        FBasic_RotateSpinSin comp = source as FBasic_RotateSpinSin;
        if (comp == null) Debug.LogError("Given component for reference values is null or is not FBasic_RotateSpinSin Component!");

        // REMEMBER: LOD = 0 is not nearest but one after nearest
        // Trying to auto configure universal LOD settings

        float mul = GetValueForLODLevel(1f, 0f, lodIndex, lodCount); // Starts from 0.75 (LOD1), then 0.5, 0.25 and 0.0 (Culled) if lod count is = 4

        // Your auto settings depending of LOD count
        // For example LOD count = 3, you want every next LOD go with parameters from 1f, to 0.6f, 0.3f, 0f - when culled
        if (lodIndex > 0) RotationRange = mul;

        name = "LOD" + (lodIndex + 2); // + 2 to view it in more responsive way for user inside inspector window
    }


    /// AUTO SETTING SETTINGS FOR CULLED LOD
    public override void SetSettingsAsForCulled(Component component)
    {
        base.SetSettingsAsForCulled(component);
        RotationRange = 0f;
    }


    /// AUTO SETTING SETTINGS FOR NEAREST (HIGHEST QUALITY) LOD (DONT NEED TO DO THIS IF INITIAL VALUES FOR YOUR VARIABLES ARE ALREADY MAX)
    public override void SetSettingsAsForNearest(Component component)
    {
        base.SetSettingsAsForNearest(component);

        //FBasic_RotateSpinSin comp = component as FBasic_RotateSpinSin;
        RotationRange = 1f;
    }

    public override void SetSettingsAsForHidden(Component component)
    {
        base.SetSettingsAsForHidden(component);
        Disable = true;
    }


    #endregion


    // NOTHING TO CHANGE, BUT ASSIGN THIS LOD TO FOptimizers_Manager
    public override FComponentLODsController GenerateLODController(Component target, FOptimizer_Base optimizer)
    {
        // Just for example code
        FBasic_RotateSpinSin c = target as FBasic_RotateSpinSin;
        if (!c) c = target.GetComponentInChildren<FBasic_RotateSpinSin>();
        if (c) if (!optimizer.ContainsComponent(c))
            {
                return new FComponentLODsController(optimizer, c, "DEMO SpinSin Properties", this);
            }

        return null;
    }

}
