using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSounds : MonoBehaviour
{
    [Header("Weapon Shooting Sounds")]
    public AudioClip GenericSound;

    [Space(20)]
    [Header("Weapon Realoading Sounds")]
    public AudioClip reloadSoundOutOfAmmo;
    public AudioClip reloadSoundAmmoLeft;

    [Space(20)] // Adds 20 Pixels of space between the previous line and these one in the inspector
    public AudioClip silencerShootSound;
    public AudioClip drawWeaponSound;
    public AudioClip holsterSound;
    public AudioClip aimSound;
    
}
