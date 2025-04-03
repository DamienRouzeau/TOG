public class CurvedUIInputModule_TOG : CurvedUIInputModule 
{
    protected override void ProcessCustomRayController()
    {
        CustomControllerButtonState = false;
        base.ProcessCustomRayController();
    }
}
