using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gamesettings_ui : MonoBehaviour
{
    public static gamesettings_ui myself = null;

    public enum _controltype
    {
        teleport,
        joystick
    };

    [Header("User Teleport Arc Settings")]
    public float hand_arc_angle_start = -40;
    public float hand_arc_angle_end = 70;
    public int hand_arc_resolution = 20;
    public float hand_arc_startwidth = 0.38f;
    public float hand_arc_endwidth = 0.25f;
    public float hand_arc_distance_min = 2f;
    public float hand_arc_distance_max = 12;
    public AnimationCurve hand_arc_distance_angle_evol = null;
    public float hand_arc_angle_coef = 1;
    public float hand_arc_angle_offset = 20;
    public float hand_arc_disable_near_avatar_distance = 0.5f;

    public _controltype controltype = _controltype.teleport;

    [Header("Color Settings")]
    public ColorSettings colorSettings = null;

    [Header("Dagger")]
    public float dagger_collision_exit_delay = 1f;

    // Start is called before the first frame update
    void Awake()
    {
        myself = this;
        DontDestroyOnLoad(gameObject);
    }
}
