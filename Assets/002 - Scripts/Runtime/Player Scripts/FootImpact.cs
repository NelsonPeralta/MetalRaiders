using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootImpact : MonoBehaviour
{
    public PlayerMovement movement;
    public AudioSource audioSource;
    public AudioClip footStepClip;
    public void PlayFootImpactClip()
    {
        if (movement.movementDirection == PlayerMovement.PlayerMovementDirection.Idle)
            return;
        audioSource.clip = footStepClip;
        audioSource.Play();
    }
}
