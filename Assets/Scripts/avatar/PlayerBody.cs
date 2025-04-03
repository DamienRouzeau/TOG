using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBody : MonoBehaviour
{
    public bool hasBeenHit;
    public float lastHitDamage;
    public int idShooter;

    public void AddDamage(float damage, int shooter = -1)
	{
        lastHitDamage += damage;
        hasBeenHit = true;
        idShooter = shooter;
    }
}
