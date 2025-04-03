using UnityEngine;

public class gamesettings_player : MonoBehaviour
{
    public static gamesettings_player myself = null;

    public static readonly string[] SKIN_NAMES = new string[] { "Pirate_Player_Woman_A", "Pirate_Player_Woman_B", "Matey_Man01", "Matey_Man03", "TOG_Player_Model_Kid_A", "TOG_Player_Model_Kid_B" };

    [Header("Names")]
    public string[] piratenames =
    {
        "XavierGrayBeard",
        "PierreOfSouth",
        "ClaudeOfGems",
        "EvilSarah",
        "QuentinOfSea",
        "NoNameNoScore"
    };

    public string[] deathprefabnames = {
        "death",
        "death",
        "death",
        "death",
        "death",
        "death"
    };

    [Header("Skins")]
    public GameObject[] pirateSkin;

    [Header("Prefabs")]
    public GameObject playerPrefab = null;
    public GameObject avatarPrefab = null;
    public GameObject uiFeedbackPrefab = null;

    [Header("Shoot - Trail")]
    public float trailMinDistanceFromCannon = 0.2f;
    public float trailMinDistanceToShow = 0.1f;
    public float trailMaxLength = 10f;

    [Header("Avatar - scale / height")]
    [SerializeField]
    private float[] _pirateSkinNormalHeightsOfEyes = null;
    [SerializeField]
    private float[] _pirateSkinMinScales = null;
    [SerializeField]
    private float[] _pirateSkinMaxScales = null;
    [SerializeField]
    private float[] _pirateSkinMinHeight = null;
    [SerializeField]
    private float[] _pirateSkinMaxHeight = null;

    [Header("Items")]
    public float attachItemBottomAngleMax = 30f;

    [Header("Killable")]
    public bool canKill = false;

    [Header("Rotation with stick")]
    public bool rotationWithStickEnable = true;
    public float rotationWithStickThreshold = 0.9f;
    public float rotationWithStickDelay = 0.2f;

    [Header("Teleport with stick")]
    public bool teleportWithStickEnable = true;

    [Header("Teleport with touch")]
    public bool teleportWithTouchEnable = true;

    private void Awake()
	{
        myself = this;
        DontDestroyOnLoad(gameObject);
    }

	public string GetNameFromIndex(int idx)
    {
        return $"{RRLib.RRLanguageManager.instance.GetString("str_playersnumberone")} {idx+1}";
        //return piratenames[idx % piratenames.Length];
    }

    public int GetSkinIndexFromName(string name)
	{
        for (int i = 0; i < pirateSkin.Length; ++i)
		{
            if (pirateSkin[i].name == name)
                return i;
		}
        return -1;
	}

    public string GetSkinName(int idx)
	{
        if (idx >= 0 && idx < pirateSkin.Length)
            return pirateSkin[idx].name;
        return null;
    }

    public float GetScaleFromHeightOfEyesForSkin(string skinName, float height)
	{
        int num = GetSkinIndexFromName(skinName);
        if (num >= 0)
		{
            float normalHeight = _pirateSkinNormalHeightsOfEyes[num];
            float ratio = height / normalHeight;
            return Mathf.Clamp(ratio, _pirateSkinMinScales[num], _pirateSkinMaxScales[num]);
        }
        return 1f;
	}

    public float GetOffsetFromHeightOfEyesForSkin(string skinName, float height)
    {
        int num = GetSkinIndexFromName(skinName);
        if (num >= 0)
        {
            if (height < _pirateSkinMinHeight[num])
                return _pirateSkinMinHeight[num] - height;
            else if (height > _pirateSkinMaxHeight[num])
                return _pirateSkinMaxHeight[num] - height;
        }
        return 0f;
    }

}
