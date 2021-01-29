using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualWieldingReload : MonoBehaviour
{
    [Header("MANUAL LINKING")]
    public PlayerController pController;

    [HideInInspector]
    [Header("Void Enablers")]
    bool reloadingRightMagInProgress;
    bool reloadingLeftMagInProgress;
    public bool reloadRightIsCanceled;
    public bool reloadLeftIsCanceled;

    [Header("ReloadCountdowns")]
    [Header("Magazine")]
    public float reloadRightCountdownMags;
    public float reloadLeftCountdownMags;
    public float magsAmmoLeft = 2;

    private void Update()
    {
        if (pController.isDualWielding)
        {
            if (pController.dwRightWP != null)
            {
                if (pController.dwRightWP.usesMags)
                {
                    if (reloadingRightMagInProgress)
                    {
                        reloadRightCountdownMags -= Time.deltaTime;

                        if (reloadRightCountdownMags <= 1f)
                        {
                            Debug.Log("Reloaded Right");
                            pController.TransferAmmoDW(true, false);
                            reloadingRightMagInProgress = false;
                            reloadRightCountdownMags = 0;
                        }

                        if (reloadRightIsCanceled)
                        {
                            reloadingRightMagInProgress = false;
                            reloadRightCountdownMags = 99; //Used to see in Inspector it works
                        }
                    }
                }

                if (pController.dwLeftWP.usesMags)
                {
                    if (pController.dwLeftWP != null)
                    {
                        if (reloadingLeftMagInProgress)
                        {
                            reloadLeftCountdownMags -= Time.deltaTime;

                            if (reloadLeftCountdownMags <= 1f)
                            {
                                Debug.Log("Reloaded Left");
                                pController.TransferAmmoDW(false, true);
                                reloadingLeftMagInProgress = false;
                                reloadLeftCountdownMags = 0;
                            }

                            if (reloadLeftIsCanceled)
                            {
                                reloadingLeftMagInProgress = false;
                                reloadLeftCountdownMags = 99; //Used to see in Inspector it works
                            }
                        }
                    }
                }
            }
        }
    }

    public void CheckAmmoTypeType(bool rightIsOutOfAmmo, bool leftIsOutOfAmmo)
    {
        if (rightIsOutOfAmmo)
        {
            if (!pController.isReloadingRight && pController.pInventory.smallAmmo != 0 /* && !isInspecting */)
            {
                if (pController.pInventory.rightWeaponCurrentAmmo < pController.pInventory.rightWeapon.GetComponent<WeaponProperties>().maxAmmoInWeapon)
                {
                    ReloadAnimation(rightIsOutOfAmmo, leftIsOutOfAmmo);
                    Debug.Log("Tracker 1");
                }
            }
        }

        if (leftIsOutOfAmmo)
        {
            if (!pController.isReloadingLeft && pController.pInventory.smallAmmo != 0 /* && !isInspecting */)
            {
                if (pController.pInventory.leftWeaponCurrentAmmo < pController.pInventory.leftWeapon.GetComponent<WeaponProperties>().maxAmmoInWeapon)
                {
                    ReloadAnimation(rightIsOutOfAmmo, leftIsOutOfAmmo);
                    Debug.Log("Tracker 2");
                }
            }
        }
    }

    public void ReloadAnimation(bool rightIsOutOfAmmo, bool leftIsOutOfAmmo)
    {
        if (rightIsOutOfAmmo)
        {
            if (pController.dwRightWP.usesMags)
            {
                pController.animDWRight.Play("Reload Ammo Left", 0, 0f);

                Reload(magsAmmoLeft, 0, true, false);
            }
        }

        if (leftIsOutOfAmmo)
        {
            if (pController.dwLeftWP.usesMags)
            {
                pController.animDWLeft.Play("Reload Ammo Left", 0, 0f);

                Reload(0, magsAmmoLeft + 0.1f, false, true);
            }
        }
    }

    public void Reload(float rightCountdownTime, float leftCountdownTime, bool rightReload, bool leftReload)
    {
        if (rightReload)
        {
            if (pController.dwRightWP.usesMags)
            {
                reloadingRightMagInProgress = true;
                reloadRightIsCanceled = false;
                reloadRightCountdownMags = rightCountdownTime;
            }
        }

        if (leftReload)
        {
            if (pController.dwLeftWP.usesMags)
            {
                reloadingLeftMagInProgress = true;
                reloadLeftIsCanceled = false;
                reloadLeftCountdownMags = leftCountdownTime;
            }
        }
    }
}
