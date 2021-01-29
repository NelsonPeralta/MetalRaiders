using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayCockingSound : MonoBehaviour
{
    public AudioClip ac1;
    public AudioClip ac2;

    public AudioSource aSource1;
    public AudioSource aSource2;
    void Start()
    {
        aSource1.clip = ac1;
        aSource2.clip = ac2;

        
    }

    void playsound()
    {
        aSource1.Play();
        aSource2.Play();
    }
}
