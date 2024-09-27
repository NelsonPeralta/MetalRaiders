using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScope : MonoBehaviour
{
    [Header("MANUAL LINKING")]
    public PlayerController pController;
    public GameObject aimingBG, splitscreenBg;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip aim;

    private bool hasFoundComponents = false;


    public void Update()
    {
        if (pController.isAiming == false)
        {
            aimingBG.SetActive(false);
            splitscreenBg.SetActive(false);
        }



        if (pController.isAiming == true)
        {
            if (pController.pInventory.activeWeapon.aimingMechanic == WeaponProperties.AimingMechanic.Scope)
            {
                if (GameManager.instance.nbLocalPlayersPreset == 1 || GameManager.instance.nbLocalPlayersPreset == 4)
                    aimingBG.SetActive(true);
                else if (GameManager.instance.nbLocalPlayersPreset == 3)
                {
                    if (pController.rid == 0)
                        splitscreenBg.SetActive(true);
                    else
                        aimingBG.SetActive(true);
                }
                else
                {
                    splitscreenBg.SetActive(true);
                }
            }
        }
    }

    public void playAimSound()
    {
        audioSource.clip = aim;
        audioSource.Play();
    }
}
