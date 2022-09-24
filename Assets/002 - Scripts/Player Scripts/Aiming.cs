using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aiming : MonoBehaviour
{
    [Header("MANUAL LINKING")]
    public PlayerController pController;
    public GameObject aimingBG;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip aim;

    private bool hasFoundComponents = false;


    public void Update()
    {
        if (pController.isAiming == true)
        {
            if (pController.pInventory.activeWeapon.aimingMechanic == WeaponProperties.AimingMechanic.Scope)
                aimingBG.SetActive(true);
        }

        if (pController.isAiming == false)
        {
            aimingBG.SetActive(false);
        }
    }

    public void playAimSound()
    {
        audioSource.clip = aim;
        audioSource.Play();
    }
}
