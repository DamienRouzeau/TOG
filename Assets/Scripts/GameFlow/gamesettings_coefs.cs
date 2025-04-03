using UnityEngine;

public class gamesettings_coefs : MonoBehaviour
{
    public static gamesettings_coefs myself = null;

    [Header("Boat Life Coefs")]
    public float boat_start_life_coef = 1f;
    public float boat_life_regeneration_coef = 1f;
    public float boat_life_sunken_regeneration_coef = 1f;

    private void Awake()
	{
        myself = this;
        DontDestroyOnLoad(gameObject);
    }
}
