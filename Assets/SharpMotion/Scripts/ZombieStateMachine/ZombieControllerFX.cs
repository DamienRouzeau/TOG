using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ZombieController : MonoBehaviour
{
    private HandleDissolveFX HandleDissolveFX;
    private bool isGrowling = false;
    private Coroutine growlCoroutine;

    public void PlayAudioOnMouth(AudioClip clip, float vol, float minPitch, float MaxPitch)
    {
        audioSourceMouth.Stop();
        audioSourceMouth.clip = clip;
        audioSourceMouth.Play();
    }

    public void PlayAudioOnFX(AudioClip clip, float vol, float minPitch, float MaxPitch)
    {
        audioSourceFX.Stop();
        audioSourceFX.clip = clip;
        audioSourceFX.volume = vol;
        audioSourceFX.pitch = Random.Range(minPitch, MaxPitch);

        audioSourceFX.Play();
    }

    public void PlayGrowl()
    {
        AudioClip randomGrowlClip = Random.value < 0.5f ? zombieEffects.growl1 : zombieEffects.growl2;
        PlayAudioOnMouth(randomGrowlClip, 1, 1, 1);
    }
    public void PlayFootStep()
    {
        AudioClip footstepClip = zombieEffects.footstep1;
        PlayAudioOnFX(footstepClip, 0.1f, .7f, 1.3f);
    }
    public void PlayAttack()
    {
        AudioClip randomAttackClip = Random.value < 0.5f ? zombieEffects.attack1 : zombieEffects.attack2;
        PlayAudioOnMouth(randomAttackClip, 1, 1, 1);
    }
    public void PlayVomit()
    {
        PlayAudioOnMouth(zombieEffects.attack3 ,1, 1, 1);
        HandleVomitParticle vomit = GetComponentInChildren<HandleVomitParticle>();
        vomit.PlayVomitParticle(this);
    }
    public void StopVomit()
    {
        HandleVomitParticle vomit = GetComponentInChildren<HandleVomitParticle>();
        vomit.StopVomitParticle(this);
    }
    public void PlaySwoosh()
    {
        PlayAudioOnFX(zombieEffects.attackSwoosh, 1, .9f, 1.1f);
    }
    public void PlayTouch()
    {
        PlayAudioOnFX(zombieEffects.attackTouch, 1, .8f, 1.2f);
    }
    public void PlayDeath()
    {
        PlayAudioOnMouth(zombieEffects.death, 1, 1, 1);
    }
    public void PlayHurt()
    {
        AudioClip randomPainClip = Random.value < 0.5f ? zombieEffects.pain1 : zombieEffects.pain2;
        PlayAudioOnMouth(randomPainClip, 1, 1, 1); ;
    }
    public void PlayScream()
    {
        AudioClip randomScream = Random.value < 0.5f ? zombieEffects.scream1 : zombieEffects.scream2;
        PlayAudioOnMouth(randomScream, 1, .7f, 1.3f);
    }
    public void Growling(bool growling)
    {
        if (growling && !isGrowling)
        {
            isGrowling = true;
            growlCoroutine = StartCoroutine(RandomGrowl());
        }
        else if (!growling && isGrowling)
        {
            isGrowling = false;
            if (growlCoroutine != null)
            {
                StopCoroutine(growlCoroutine);
                growlCoroutine = null;
            }
        }
    }
    // Son ambiant du zombie
    private IEnumerator RandomGrowl()
    {
        while (isGrowling)
        {
            yield return new WaitForSeconds(Random.Range(1f, 6f));
            if (isGrowling)
            {
                PlayGrowl();
            }
        }
    }
    public void PlayDisolving()
    {
        Debug.Log("Disolving");
        HandleDissolveFX.Dissolve();
 /*       disolvingCoroutine = StartCoroutine(Disolving());*/
    }

    // "Dissout" le zombie
/*    private IEnumerator Disolving()
    {
        while (isDisolving)
        {
            transform.localPosition -= new Vector3(0, 0.01f, 0);

            yield return new WaitForSeconds(0.1f);
        }
    }*/

}
