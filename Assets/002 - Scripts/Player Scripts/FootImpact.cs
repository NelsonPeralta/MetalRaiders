using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootImpact : MonoBehaviour
{
    public AudioClip footStepClip;
    private void OnTriggerEnter(Collider other)
    {
        GetComponent<AudioSource>().clip = footStepClip;
        GetComponent<AudioSource>().Play();
    }
}
