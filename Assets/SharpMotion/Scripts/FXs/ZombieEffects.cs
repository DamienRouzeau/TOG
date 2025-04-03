using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ZombieEffects", menuName = "ScriptableObjects/ZombieEffects", order = 1)]
public class ZombieEffects : ScriptableObject
{
    [Header("Screams")]
    public AudioClip scream1;
    public AudioClip scream2;

    [Header("Growls")]
    public AudioClip growl1;
    public AudioClip growl2;

    [Header("Pain Sounds")]
    public AudioClip pain1;
    public AudioClip pain2;

    [Header("Footstep Sound")]
    public AudioClip footstep1;

    [Header("Attack Sounds")]
    public AudioClip attack1;
    public AudioClip attack2;
    public AudioClip attack3;
    public AudioClip attackSwoosh;
    public AudioClip attackTouch;

    [Header("Death Sound")]
    public AudioClip death;
}
