using UnityEngine;

public class gamesettings_boat : MonoBehaviour
{
    public static gamesettings_boat myself = null;

    public string boatvoicename = "Voice_Overs_Captain";
    public string boat01objecttoremove = "Pirate_Male";
    public string boat02objecttoremove = "Pirate_Female";

    [Header("Boat Life")]
    public float boat_start_life = 1000f;
    public float boat_life_regeneration = 10f;
    public float boat_life_sunken_regeneration = 50f;

    [Header("Pump limitations")]
    public float pump_high_limit = 1.1f;
    public float pump_low_limit = 0.55f;
    public float pump_count_x_pl_1 = 10.0f;
    public float pump_count_x_pl_2 = 10.0f;
    public float pump_count_x_pl_3 = 10.0f;
    public float pump_count_x_pl_4 = 10.0f;
    public float pump_count_x_pl_5 = 10.0f;
    public float pump_count_x_pl_6 = 10.0f;
    public float pump_count_x_pl_7 = 10.0f;
    public float pump_count_x_pl_8 = 10.0f;

    [Header("Jauge limitations")]
    public float jauge_high_limit = 0.922f;
    public float jauge_low_limit = 0.234f;

    [Header("Game settings")]
    public float boat_shield_time = 3.0f;

    [Header("Boat-health sink treshold")]
    public float sinktreshold = 20.0f;

    [Header("Boat-speeds")]
    public bool override_boat_speeds = false;
    public float boat01_speed_start = 5f;
    public float boat01_speed_min = 3f;
    public float boat01_speed_max = 9f;
    public float boat02_speed_start = 5f;
    public float boat02_speed_min = 3f;
    public float boat02_speed_max = 9f;
    public AnimationCurve boat_speed_with_health = new AnimationCurve();

    [Header("Boat-speed_alteration")]
    public bool use_boat_speed_alteration = false;
    public float boat_distance_threshold_min = 0.01f;
    public float boat_distance_threshold_max = 0.1f;
    public float boat_first_speed_coef = 0.8f;
    public float boat_last_speed_coef = 1.2f;

    [Header("Boat-cordAtEnd")]
    public float boat_speed_modifier_duration = 2f;
    public AnimationCurve boat_speed_modifier_curve = new AnimationCurve();
    public bool is_boat_speed_modifier_additive = false;

    [Header("Boat-AI")]
    public float ai_boat_fire_cannon_delay_min = 5f;
    public float ai_boat_fire_cannon_delay_max = 15f;
    public float ai_boat_fire_cannon_percent_max = 75f;
    public float ai_boat_sinking_pump_delay_min = 1f;
    public float ai_boat_sinking_pump_delay_max = 2f;
    public float ai_boat_sail_behind_delay_min = 5f;
    public float ai_boat_sail_behind_delay_max = 8f;
    public float ai_boat_sail_near_delay_min = 8f;
    public float ai_boat_sail_near_delay_max = 12f;
    public float ai_boat_distance_limit_behind = 20f;
    public float ai_boat_distance_limit_front = 20f;
    public float ai_boat_distance_after_front_no_shoot = 20f;
    public float ai_boat_spawn_gold_loosing_life = 0f;

    [Header("Boat-Canons")]
    public float boat_fire_cannon_reload_delay = 1f;
    public float boat_fire_cannon_reload_distance = 0.35f;

    private void Awake()
	{
        myself = this;
    }
    public float GetPumpCounter()
    {
        return GetPumpCounter(PhotonNetworkController.myself.NumberOfPlayers());
    }

    public float GetPumpCounter(int nbrPlayerInBoat)
    {
        switch (nbrPlayerInBoat)
        {
            case 2: return (pump_count_x_pl_2);
            case 3: return (pump_count_x_pl_3);
            case 4: return (pump_count_x_pl_4);
            case 5: return (pump_count_x_pl_5);
            case 6: return (pump_count_x_pl_6);
            case 7: return (pump_count_x_pl_7);
            case 8: return (pump_count_x_pl_8);
        }
        return (pump_count_x_pl_1);
    }
}
