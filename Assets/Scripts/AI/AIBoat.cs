using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBoat : MonoBehaviour
{
    public enum PositionState
    {
        Near,
        Behind,
        Front
    }

    [SerializeField]
    private bool _allowFire = true;

    public bool needToUseSail => _needToUseSail;
    public bool canShoot => _canShoot;

    private List<ProjectileCannon> _canons = null;
    private Health _health = null;
    private boat_sinking _boatSinking = null;
    private boat_followdummy _boat = null;
    private float _distanceToOtherBoat = 0f;
    private PositionState _positionState = PositionState.Near;
    private float _distanceForState = 0f;
    private Vector3 _otherBoatposition = Vector3.zero;
    private bool _needToUseSail = false;
    private boat_canon[] _boatCanons = null;

    private bool _canShoot = true;

    // Start is called before the first frame update
    private void Start()
    {
        _health = GetComponent<Health>();

        float healthCoef = gamesettings_difficulty.myself.GetAiBoatLifeMultiplier();
        _health.maxHealth *= healthCoef;
        _health.startingHealth *= healthCoef;
        _health.currentHealth *= healthCoef;

        if (!PhotonNetworkController.IsMaster())
            return;

        _boat = GetComponent<boat_followdummy>();        
        _boatSinking = GetComponentInChildren<boat_sinking>(true);
        _canons = new List<ProjectileCannon>();

        foreach (ProjectileCannon cannon in gameObject.GetComponentsInChildren<ProjectileCannon>(true))
        {
            if (poolhelper.myself.IsBoatBulletPool(cannon.poolname))
                _canons.Add(cannon);
        }

        StartCoroutine(FireCanonsEnum());
        StartCoroutine(CanonsDirectionEnum());
        StartCoroutine(CheckHealthEnum());
        StartCoroutine(CheckUseSailEnum());
        StartCoroutine(CheckGoldEnum());
    }

	private void OnDestroy()
	{
        _boat = null;
        _health = null;
        _boatSinking = null;
        _canons = null;
    }

	public void SetDistanceToOtherBoat(float distance)
	{
        gamesettings_boat gsBoat = gamesettings_boat.myself;
        _distanceToOtherBoat = distance;
        if (_distanceToOtherBoat < -gsBoat.ai_boat_distance_limit_behind)
        {
            // Ai is behind
            _positionState = PositionState.Behind;
            _distanceForState = -gsBoat.ai_boat_distance_limit_behind - _distanceToOtherBoat;

        }
        else if (_distanceToOtherBoat < gsBoat.ai_boat_distance_limit_front)
        {
            // AI is near
            _positionState = PositionState.Near;
            _distanceForState = _distanceToOtherBoat;
        }
        else
        {
            // AI is front
            _positionState = PositionState.Front;
            _distanceForState = _distanceToOtherBoat - gsBoat.ai_boat_distance_limit_front;
        }
        //Debug.Log($"Raph - _positionState {_positionState} _distanceToOtherBoat {_distanceToOtherBoat} _distanceForState {_distanceForState} team {_boat.team}");
    }

    public void SetOtherBoatPosition(Vector3 v3Pos)
	{
        _otherBoatposition = v3Pos;
    }

    public void AllowToShoot(bool allow)
	{
        _canShoot = allow;
	}

    private IEnumerator FireCanonsEnum()
	{
        while (!CanFire())
            yield return null;

        gamesettings_boat gsBoat = gamesettings_boat.myself;

        while (CanFire())
        {
            float delay = gsBoat.ai_boat_fire_cannon_delay_min;
            if (_positionState == PositionState.Front)
			{
                float ratio = _distanceForState / gsBoat.ai_boat_distance_after_front_no_shoot;
                delay = Mathf.Lerp(delay, gsBoat.ai_boat_fire_cannon_delay_max, ratio);
            }
            yield return new WaitForSeconds(delay);
            FireCannons();
        }
    }

    private IEnumerator CanonsDirectionEnum()
	{
        while (!CanFire())
            yield return null;

        _boatCanons = new boat_canon[_canons.Count];

        for (int i = 0; i < _canons.Count; ++i)
		{
            _boatCanons[i] = _canons[i].GetComponentInParent<boat_canon>();
		}

        while (CanFire())
        {
            foreach (var boatCanon in _boatCanons)
            {
                if (boatCanon.CanTargetPosition(_otherBoatposition))
                {
                    boatCanon.SetOrientationWithPosition(_otherBoatposition, false, 5f);
                }
                else if (RaceManager.myself?.aiTargets != null && RaceManager.myself.aiTargets[_boat.team] != null)
				{
                    // Search another target
                    List<Transform> targets = RaceManager.myself.aiTargets[_boat.team].targets;
                    foreach (Transform tr in targets)
					{
                        if (tr.gameObject.activeInHierarchy)
						{
                            if (boatCanon.CanTargetPosition(tr.position))
							{
                                boatCanon.SetOrientationWithPosition(tr.position, false, 5f);
                                break;
                            }
                        }
					}
				}
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator CheckHealthEnum()
    {
        while (!CanPump())
            yield return null;

        gamesettings_boat gsBoat = gamesettings_boat.myself;

        while (CanPump())
        {
            if (_boatSinking.isSinking)
            {
                float delay = Random.Range(gsBoat.ai_boat_sinking_pump_delay_min, gsBoat.ai_boat_sinking_pump_delay_max);
                yield return new WaitForSeconds(delay);
                if (_boatSinking.isSinking)
                    _boatSinking.PlayerBucket(true);
            }
            else
			{
                yield return new WaitForSeconds(10f);
            }
        }
    }

    private IEnumerator CheckGoldEnum()
	{
        while (!CanFire())
            yield return null;

        gamesettings_boat gsBoat = gamesettings_boat.myself;

        if (gsBoat.ai_boat_spawn_gold_loosing_life <= 0f)
            yield break;

        float startHealth = _health.currentHealth;

        while (CanFire())
		{
            if (_health.currentHealth == 0f & startHealth > 0f)
			{
                Vector3 chestPos = transform.position + Vector3.up * 25f + transform.right * 10f;
                PhotonNetworkController.InstantiateSoloOrMulti("pirate_chest_Static_photon", chestPos, Quaternion.identity);
                PhotonNetworkController.InstantiateSoloOrMulti("coins_props_photon", chestPos + transform.right * 10f - transform.up * 5f, Quaternion.identity);
                PhotonNetworkController.InstantiateSoloOrMulti("coins_props_photon", chestPos - transform.right * 10f - transform.up * 5f, Quaternion.identity);
                PhotonNetworkController.InstantiateSoloOrMulti("coins_props_photon", chestPos + transform.forward * 10f - transform.up * 5f, Quaternion.identity);
                PhotonNetworkController.InstantiateSoloOrMulti("coins_props_photon", chestPos - transform.forward * 10f - transform.up * 5f, Quaternion.identity);
                startHealth = _health.currentHealth;
            }
            else if (_health.currentHealth < startHealth - gsBoat.ai_boat_spawn_gold_loosing_life)
			{
                PhotonNetworkController.InstantiateSoloOrMulti("coins_props_photon", transform.position + Vector3.up * 20f + transform.right * 10f, Quaternion.identity);
                startHealth = _health.currentHealth;
            }
            else if (_health.currentHealth > startHealth)
			{
                startHealth = _health.currentHealth;
            }
            yield return null;
		}
	}


    private IEnumerator CheckUseSailEnum()
	{
		while (!CanUseSail())
			yield return null;

        gamesettings_boat gsBoat = gamesettings_boat.myself;

        while (CanUseSail())
		{
            if (_boatSinking.isSinking)
			{
                _needToUseSail = false;
                yield return new WaitForSeconds(5f);
            }
			else if (_positionState == PositionState.Behind)
			{
                _needToUseSail = true;
				yield return new WaitForSeconds(10f);
                _needToUseSail = false;
                float delay = Random.Range(gsBoat.ai_boat_sail_behind_delay_max, gsBoat.ai_boat_sail_behind_delay_max);
                delay += gsBoat.boat_speed_modifier_duration;
                yield return new WaitForSeconds(delay);
            }
            else if (_positionState == PositionState.Near)
            {
                _needToUseSail = true;
                yield return new WaitForSeconds(10f);
                _needToUseSail = false;
                float delay = Random.Range(gsBoat.ai_boat_sail_near_delay_max, gsBoat.ai_boat_sail_near_delay_max);
                delay += gsBoat.boat_speed_modifier_duration;
                yield return new WaitForSeconds(delay);
            }
            else
			{
				_needToUseSail = false;
                yield return new WaitForSeconds(5f);
            }
		}

        _needToUseSail = false;
    }

    

    private bool CanFire()
    {
        if (!GameflowBase.areAllRacesStarted)
            return false;
        if (gameflowmultiplayer.GetTeamPosition(_boat.team) >= 0)
            return false;
        if (gameflowmultiplayer.gameplayEndRace)
            return false;
        
        return true;
    }

    private bool CanPump()
    {
        if (!GameflowBase.areAllRacesStarted)
            return false;
        return true;
    }

    private bool CanUseSail()
    {
        if (!GameflowBase.areAllRacesStarted)
            return false;
        return true;
    }

    private void FireCannons()
    {
        if (_health != null && _health.currentHealth <= 0f)
            return;
        if (_boatSinking.isSinking)
            return;
        if (!_canShoot)
            return;
        //if (_positionState == PositionState.Behind)
        //    return;

        gamesettings_boat gsBoat = gamesettings_boat.myself;

        foreach (ProjectileCannon cannon in _canons)
        {
            if (cannon != null && cannon.gameObject.activeInHierarchy)
            {
                bool shoot = true;
                float percentShoot = gsBoat.ai_boat_fire_cannon_percent_max * 0.01f;
                if (_positionState == PositionState.Front)
                {
                    percentShoot *= 1f - Mathf.Clamp01(_distanceForState / (gsBoat.ai_boat_distance_after_front_no_shoot - gsBoat.ai_boat_distance_limit_front));
                }
                shoot = Random.Range(0f, 1f) <= percentShoot;
                if (shoot)
                {
                    StartCanonFire(cannon);
                }
            }
        }
    }

    private void StartCanonFire(ProjectileCannon cannon)
	{
        StartCoroutine(FireCannonWithDelayEnum(cannon, Random.Range(0f, gamesettings_boat.myself.ai_boat_fire_cannon_delay_min)));
    }

    private IEnumerator FireCannonWithDelayEnum(ProjectileCannon cannon, float delay=0f)
	{
        if (delay > 0f)
            yield return new WaitForSeconds(delay);
        if (_allowFire && CanFire())
            cannon.FireCannon();
    }
}
