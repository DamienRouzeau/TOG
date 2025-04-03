using UnityEngine;
using System.Collections;

public class LaserControl : MonoBehaviour {

	// PUBLIC
	public LaserEffect laserShootRenderer;
	public ParticleSystem flareParticles;
	public ParticleSystem smokeParticles;
	public ParticleSystem sparksParticles;

	[Tooltip("Max lenght of the beam")]
	public float maxLength = 100f;

	// PRIVATE
	private Vector3 shootingAim = Vector3.zero;
	private bool laserEnabled = true;


	/// <summary>Enable the laser. By default the laser is enabled</summary>
	public bool EnableLaser
	{
		set{ laserEnabled = value;}
	}

	/// <summary>Activate and disactivate the laser. This won't work if the laser is disabled.</summary>
	public bool Activate
	{
		set {
			if(value && laserEnabled)
				laserShootRenderer.gameObject.SetActive(true);
			else if(laserShootRenderer.enabled)
			{
				laserShootRenderer.gameObject.SetActive(false);
				HideFlareParticles();
			}
		}
	}

	void Update ()
	{

		RaycastHit ray = new RaycastHit();
		// chec if the laser is activated
		if(laserShootRenderer.gameObject.activeSelf)
		{

			// the laser hit something
			if(Physics.Raycast(laserShootRenderer.transform.position,laserShootRenderer.transform.forward, out ray,maxLength))
			{
				// set the lenght of the laser
				laserShootRenderer.SetLength(Vector3.Distance(transform.position, ray.point));

				// activate flare the particles effect
				ShowFlareParticles(ray.point);

			}
			else
			{
				// set the max lenght of the laser
				laserShootRenderer.SetLength(maxLength);

				// deactivate particles
				HideFlareParticles();
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
		if(flareParticles.emission.rate.constantMax == 0f)
		{
			ParticleSystem.EmissionModule em = flareParticles.emission;
			ParticleSystem.MinMaxCurve rate = new ParticleSystem.MinMaxCurve();
			rate.constantMax = 20f;
			em.rate = rate;

			em = sparksParticles.emission;
			em.rate = rate;

			em = smokeParticles.emission;
			rate.constantMax = 5f;
			em.rate = rate;

		}

	}

	/// <summary>Disable the flares</summary>
	private void HideFlareParticles()
	{
		// stop all emissions
		ParticleSystem.EmissionModule em = flareParticles.emission;
		ParticleSystem.MinMaxCurve rate = new ParticleSystem.MinMaxCurve();
		rate.constantMax = 0f;
		em.rate = rate;

		em = sparksParticles.emission;
		em.rate = rate;

		em = smokeParticles.emission;
		em.rate = rate;

	}


}
