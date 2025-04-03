using UnityEngine;

public class PlayerDamages : MonoBehaviour
{
    [SerializeField]
    private float _damagesBySecond = 10f;

    private bool _damagesActivated = false;

    public void StartDamages()
	{
        _damagesActivated = true;
    }

    public void StopDamages()
	{
        _damagesActivated = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_damagesActivated)
		{
            float damages = Time.deltaTime * _damagesBySecond;
            Player.myplayer.playerBody.AddDamage(damages);
		}
    }
}
