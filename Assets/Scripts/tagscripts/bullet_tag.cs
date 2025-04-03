using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet_tag : MonoBehaviour
{
	public GameObject myObject => _myObject;
	private GameObject _myObject = null;

	public Projectile myProjectile => _myProjectile;
	private Projectile _myProjectile = null;

	private void Awake()
	{
		_myObject = gameObject.FindInChildren("object");
		_myProjectile = GetComponent<Projectile>();
	}

}
