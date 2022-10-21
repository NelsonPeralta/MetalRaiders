using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootImpact : MonoBehaviour
{
    public Movement movement;
    public AudioSource audioSource;
    public AudioClip footStepClip;
    public void PlayFootImpactClip()
    {
        if (movement.direction == "Idle")
            return;
        audioSource.clip = footStepClip;
        audioSource.Play();
    }
}
