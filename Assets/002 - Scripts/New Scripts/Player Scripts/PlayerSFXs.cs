using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSFXs : MonoBehaviour
{
    [Header("Pickup Sounds")]
    public AudioSource ammoPickupAudioSource;
    [Header("Audio Clips")]
    public AudioClip cockingClip1;
    public AudioClip cockingClip2;
    public AudioClip aimingSound;
    public AudioClip walkingSound;

    [Header("Shooting and Reloading Source")]
    public AudioSource shooting;
    public AudioSource reloadingRight;
    public AudioSource reloadingLeft;

    public AudioSource mainAudioSource;

    public bool aimSoundHasPlayed = false;

    private void Start()
    {
        mainAudioSource = GameObject.FindGameObjectWithTag("Player").GetComponent<AudioSource>();
    }
}
