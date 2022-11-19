using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollPrefab : MonoBehaviour
{
    public Transform ragdollHead;
    public Transform ragdollChest;
    public Transform ragdollHips;
    [Space(10)]
    public Transform ragdollUpperArmLeft;
    public Transform ragdollUpperArmRight;
    [Space(10)]
    public Transform ragdollLowerArmLeft;
    public Transform ragdollLowerArmRight;
    [Space(10)]
    public Transform ragdollUpperLegLeft;
    public Transform ragdollUpperLegRight;
    [Space(10)]
    public Transform ragdollLowerLegLeft;
    public Transform ragdollLowerLegRight;

    public List<AudioClip> deathClips;

    private void Start()
    {
        int randomSound = Random.Range(0, deathClips.Count);
        GetComponent<AudioSource>().clip = deathClips[randomSound];
        GetComponent<AudioSource>().Play();
    }
}