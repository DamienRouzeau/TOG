using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KDK_LaserControl : MonoBehaviour
{
	public enum State
	{
		USING,
		LOADING
	}

	// PUBLIC
	public LaserEffect laserShootRenderer;
	public List<GameObject> goListTouching;
	public ParticleSystem flareParticles;
	public ParticleSystem smokeParticles;
	public ParticleSystem sparksParticles;
	public Projectile projectile = null;
	public Image laserGauge = null;
	public TextMeshProUGUI laserGaugeValue = null;
	public float maxValue = 100f;
	public float maxTime = 20f;
	public float loadingDuration = 5f;
	public GameObject emptyIndication = null;

	[Tooltip("Max lenght of the beam")]
	public float maxLength = 100f;

	// PRIVATE
	private bool laserEnabled = true;
	private int _colLayerMask = 0;
	private State _state = State.USING;
	private float _usingTime = 0f;
	private float _loadingTime = 0f;

	private void Start()
	{
		_colLayerMask = physicslayermask.MaskForLayer(gameObject.layer);
		SetGoListTouching(false);
		UpdateGauge(1f);
	}

	/// <summary>Enable the laser. By default the laser is enabled</summary>
	public bool EnableLaser
	{
		set{ laserEnabled = value;}
	}

	/// <summary>Activate and disactivate the laser. This won't work if the laser is disabled.</summary>
	public bool Activate
	{
		set {
			if (value && laserEnabled)
			{
				laserShootRenderer.gameObject.SetActive(true);
				SetGoListTouching(false);
			}
			else if (laserShootRenderer.enabled)
			{
				laserShootRenderer.gameObject.SetActive(false);
				SetGoListTouching(false);
				HideFlareParticles();
			}
		}
	}

	void Update ()
	{
		switch (_state)
		{
			case State.USING:
				UpdateUsingState();
				break;
			case State.LOADING:
				UpdateLoadingState();
				break;
		}
		
	}

	private void UpdateUsingState()
	{
		// check if the laser is activated
		if (laserShootRenderer.gameObject.activeSelf)
		{
			Transform tr = laserShootRenderer.transform;
			// the laser hit something
			if (Physics.Raycast(tr.position, tr.forward, out RaycastHit ray, maxLength, _colLayerMask))
			{
				// set the lenght of the laser
				laserShootRenderer.SetLength(Vector3.Distance(transform.position, ray.point));

				// activate flare the particles effect
				ShowFlareParticles(ray.point);

				if (projectile != null)
				{
					projectile.TriggerHitCollider(Player.myplayer.gameObject, ray.collider.gameObject, ray.point, false);
				}
			}
			else
			{
				// set the max lenght of the laser
				laserShootRenderer.SetLength(maxLength);

				// deactivate particles
				HideFlareParticles();
			}

			if (maxTime > 0f)
			{
				_usingTime += Time.deltaTime;
				if (_usingTime > maxTime)
				{
					_loadingTime = 0f;
					_state = State.LOADING;
					laserEnabled = false;
					Activate = false;
					if (emptyIndication != null)
						emptyIndication.SetActive(true);
				}

				UpdateGauge(1f - Mathf.Clamp01(_usingTime / maxTime));
			}
		}
	}

	private void UpdateLoadingState()
	{
		_loadingTime += Time.deltaTime;
		if (_loadingTime > loadingDuration)
		{
			_usingTime = 0f;
			_state = State.USING;
			laserEnabled = true;
			if (emptyIndication != null)
				emptyIndication.SetActive(false);
		}
		if (loadingDuration > 0f)
		{
			UpdateGauge(Mathf.Clamp01(_loadingTime / loadingDuration));
		}
		else
		{
			UpdateGauge(1f);
		}
	}

	private void UpdateGauge(float ratio)
	{
		if (laserGauge != null)
			laserGauge.fillAmount = ratio;
		if (laserGaugeValue != null)
			laserGaugeValue.text = Mathf.CeilToInt(maxValue * ratio).ToString();
	}

	private void SetGoListTouching(bool touching)
	{
		if (goListTouching != null)
		{
			foreach (GameObject go in goListTouching)
			{
				go.SetActive(touching);
			}
		}
	}

	/// <summary>Enable the flares where the beam hits a collider.</summary>
	/// <param name="value">Point of collision of the beam</param>
	private void ShowFlareParticles(Vector3 point)
	{
		// detouch the flares a bit form the collision point
		flareParticles.transform.position = point - flareParticles.transform.forward * 0.1f;

		// enable the emissions
		if(flareParticles.emission.rateOverTime.constantMax == 0f)
		{
			ParticleSystem.EmissionModule em = flareParticles.emission;
			ParticleSystem.MinMaxCurve rate = new ParticleSystem.MinMaxCurve();
			rate.constantMax = 20f;
			em.rateOverTime = rate;

			em = sparksParticles.emission;
			em.rateOverTime = rate;

			em = smokeParticles.emission;
			rate.constantMax = 5f;
			em.rateOverTime = rate;

			SetGoListTouching(true);
		}
	}

	/// <summary>Disable the flares</summary>
	private void HideFlareParticles()
	{
		// stop all emissions
		ParticleSystem.EmissionModule em = flareParticles.emission;
		ParticleSystem.MinMaxCurve rate = new ParticleSystem.MinMaxCurve();
		rate.constantMax = 0f;
		em.rateOverTime = rate;

		em = sparksParticles.emission;
		em.rateOverTime = rate;

		em = smokeParticles.emission;
		em.rateOverTime = rate;

		SetGoListTouching(false);
	}


}
