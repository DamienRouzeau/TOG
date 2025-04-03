using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class HandleVomitParticle : MonoBehaviour
{
    private ParticleSystem vomit;

    private void Awake()
    {   
        vomit = GetComponentInChildren<ParticleSystem>();     
    }
    public void PlayVomitParticle(ZombieController stateMachine)
    {
        vomit.Play();
    }
    public void StopVomitParticle(ZombieController stateMachine)
    {
        vomit.Stop();
    }
}
