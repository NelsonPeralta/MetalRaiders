using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BushEnteringNoise : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip bushEnteringClip;

    private void OnTriggerEnter(Collider other)
    {
        if (audioSource.isPlaying)
            return;
        audioSource.clip = bushEnteringClip;
        audioSource.Play();
    }
}
