using Viveport;

public class ViveportManager
{
    public static string APP_ID = "3b86a0ad-b624-4c9d-8b88-f3e9168bda47";
    public static string APP_KEY = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQC4Jb+AYyE8MzZYvVOu1HFGDmXjcgs35u1+ZhQi60l6w9HQd4U6uW/EiOy3g7YNhBKq5vbrFLvaOWabT0q9XLcsf8ecVAelKbjj+fkcHV0EVQWYPqhggW+kR1YCBn2feUtLWasiTiTkVaSVQLvtA6WwQTiuu+aMVbItCjksOwn8LQIDAQAB";

    private static System.Action<bool> _onResult = null;

    // Use this for initialization
    public static void Init(System.Action<bool> onResult = null)
    {
        Viveport.Core.Logger.Log("Init called");
        _onResult = onResult;
        Api.Init(InitStatusHandler, APP_ID);
    }

    static void GetLicence()
	{
        Viveport.Core.Logger.Log("GetLicence called");
        Api.GetLicense(new MyLicenseChecker(), APP_ID, APP_KEY);
    }


    private static void InitStatusHandler(int nResult)
    {
        if (nResult == 0)
        {
            Viveport.Core.Logger.Log("InitStatusHandler is successful");
            GetLicence();
        }
        else
        {
            Viveport.Core.Logger.Log("InitStatusHandler error : " + nResult);
            _onResult?.Invoke(false);
        }
    }

    class MyLicenseChecker : Api.LicenseChecker
    {
        public override void OnSuccess(long issueTime, long expirationTime, int latestVersion, bool updateRequired)
        {
            Viveport.Core.Logger.Log("[MyLicenseChecker] issueTime: " + issueTime);
            Viveport.Core.Logger.Log("[MyLicenseChecker] expirationTime: " + expirationTime);
            Viveport.Core.Logger.Log("[MyLicenseChecker] latestVersion: " + latestVersion);
            Viveport.Core.Logger.Log("[MyLicenseChecker] updateRequired: " + updateRequired);
            _onResult?.Invoke(true);
        }

        public override void OnFailure(int errorCode, string errorMessage)
        {
            Viveport.Core.Logger.Log("[MyLicenseChecker] errorCode: " + errorCode);
            Viveport.Core.Logger.Log("[MyLicenseChecker] errorMessage: " + errorMessage);
            _onResult?.Invoke(false);
        }
    }
}
