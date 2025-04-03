using UnityEngine;

public class gamesettings : MonoBehaviour
{
    public static gamesettings myself = null;

    public const string STAT_FIRE = "fire";
    public const string STAT_TOUCH = "touch";
    public const string STAT_DEATH = "death";
    public const string STAT_KILLS = "killer";
    public const string STAT_POINTS = "gold";
    public const string STAT_CHESTS = "treasure";
    public const string STAT_SAILOR = "sail";
    public const string STAT_SKIMMER = "scoop";
    public const string STAT_TURRETS = "turrets";
    public const string STAT_LAVA = "lava";
    public const string STAT_BOATSKILLER = "shooter";
    public const string STAT_CANONBAIT = "fireman";
    public const string STAT_BIRDS = "birds";
    public const string STAT_HEARTS = "hearts";
    public const string STAT_SHIELDS = "shields";
    public const string STAT_SPLINES = "splines";
    public const string STAT_MERMAIDS = "mermaids";
    public const string STAT_MONSTERS = "monsters";
    public const string STAT_SKULLS = "skulls";
    public const string STAT_ARCHIVES = "archives";
    public const string STAT_SCIENTISTS = "scientists";

    public enum EndButtons
    {
        END_REPLAY_RACE,
        END_NEXT_RACE,
        END_QUIT_RACE,
        END_CONTINUE
    }

    public float pathgamesize = 0.2f;

    [Header("Times")]
    [Tooltip("session time in second")]
    public float gameSessionTime = 1200f;
    [Tooltip("limit time to show chrono")]
    public int chronoLimitTime = 120;
    [Tooltip("Alarm time for chrono")]
    public int chronoAlarmTime = 10;
    [Tooltip("Alarm time for chrono")]
    public float maxTimeAfterFinishLine = 30f;


    public GameObject endGameBulletPrefab = null;

    public float startcountdownsize = 10.0f;
    public float delayAfterLoadToOpenDoors = 5.0f;
    public float delayAfterOpendDoorsToStartRace = 5.0f;
    public float delayToShowButtonsAtEndRace = 5.0f;

    [Header("Game Prefabs")]
    public UI_HealthJauge healthJaugePrefab;
    public UI_EndRaceResult endRaceResultPrefab;
    public UI_EndRaceResult endRaceResultPrefabOneTeam;

    [Header("Game Sounds")]
    public AudioClip pickupobject;
    public AudioClip attachobject;

    [Header("Total Gold In Race")]
    public bool useCustomTotalGoldInRace = false;
    public int customTotalGoldInRace = 0;

    [Header("Minimap")]
    public float finishLineOnSpline = 0.7f;
    public Sprite drawingMinimap = null;

    [Header("Items")]
    public float respawnAllItemsAtTime = 0f;

    [Header("End Buttons")]
    public bool showReplayButton = true;
    public bool showNextButton = true;

    private void Awake()
    {
        myself = gameObject.GetComponent<gamesettings>();
    }
}
