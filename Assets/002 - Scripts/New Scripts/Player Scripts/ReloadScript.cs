using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ReloadScript : MonoBehaviourPun
{
    public delegate void ReloadScriptEvent(ReloadScript reloadScript);
    public ReloadScriptEvent OnReloadStart;
    public ReloadScriptEvent OnReloadEnd;


    [Header("MANUAL LINKING")]
    public PlayerController pController;
    public PhotonView PV;

    const int DEFAULT_RELOAD_TIME = 1;

    [HideInInspector]
    [Header("Void Enablers")]
    bool reloadingMagInProgress;
    public bool openingWeaponForShells;
    public bool insertingShellInProgress;
    bool reloadingSingleInProgress;
    public bool reloadIsCanceled;

    [Header("ReloadCountdowns")]
    [Header("Magazine")]
    public float reloadCountdownMags;
    public float magsAmmoLeft = 2;

    [Header("Shells")]
    public int shellsToInsert;
    public float reloadOpenCountdown;
    public float reloadInsertCountdown;
    public float reloadCloseCountdown;
    public float shellsReloadOpenTime;
    public float shellsInsertTime;
    public float shellsReloadCloseTime;

    [Header("Single Ammo")]
    public float reloadSingleCountdown;
    public float singleReloadTime = 1f;

    [Header("Audio Source")]
    public AudioSource reloadAudioSource;

    private void Update()
    {
        if (pController.wProperties != null)
        {
            if (pController.wProperties.ammoReloadType == WeaponProperties.AmmoReloadType.Magazine)
            {
                if (reloadingMagInProgress)
                {
                    reloadCountdownMags -= Time.deltaTime;

                    if (reloadCountdownMags <= 0)
                    {
                        pController.TransferAmmo();
                        reloadingMagInProgress = false;
                        reloadCountdownMags = 0;
                        OnReloadEnd?.Invoke(this);
                    }

                    if (reloadIsCanceled)
                    {
                        reloadingMagInProgress = false;
                        reloadCountdownMags = 99; //Used to see in Inspector it works
                    }
                }
            }

            if (pController.wProperties.ammoReloadType == WeaponProperties.AmmoReloadType.Shell) ///////For Shotgun
            {
                if (openingWeaponForShells)
                {
                    reloadOpenCountdown -= Time.deltaTime;

                    if (reloadOpenCountdown <= 0)
                    {
                        Debug.Log("Inserting Shell");
                        pController.anim.Play("Insert Shell", 0, 0f);
                        shellsToInsert = shellsToInsert - 1;
                        pController.TransferAmmo();

                        openingWeaponForShells = false;
                        insertingShellInProgress = true;
                        reloadOpenCountdown = 0;

                        OnReloadEnd?.Invoke(this);

                    }
                    if (reloadIsCanceled)
                    {
                        openingWeaponForShells = false;
                        insertingShellInProgress = false;
                        reloadOpenCountdown = 99;
                    }


                }

                if (insertingShellInProgress)
                {
                    //pController.anim.Play("Insert Shell", 0, 0f);

                    reloadInsertCountdown -= Time.deltaTime;

                    if (reloadInsertCountdown <= 0)
                    {
                        if (shellsToInsert <= 0)
                        {
                            insertingShellInProgress = false;
                            reloadInsertCountdown = 0;

                            pController.anim.Play("Reload Close", 0, 0f);
                        }
                        else
                        {
                            pController.anim.Play("Insert Shell", 0, 0f);
                            shellsToInsert = shellsToInsert - 1;
                            pController.TransferAmmo();

                            reloadInsertCountdown = shellsInsertTime;

                            OnReloadEnd?.Invoke(this);

                        }
                        if (reloadIsCanceled)
                        {
                            openingWeaponForShells = false;
                            insertingShellInProgress = false;
                            reloadOpenCountdown = 99;
                            reloadInsertCountdown = 99;
                        }
                    }

                    if (reloadIsCanceled)
                    {
                        openingWeaponForShells = false;
                        insertingShellInProgress = false;
                        reloadOpenCountdown = 99;
                        reloadInsertCountdown = 99;
                    }

                }

                if (reloadIsCanceled)
                {
                    openingWeaponForShells = false;
                    insertingShellInProgress = false;
                    reloadOpenCountdown = 99;
                }
            }

            if (pController.wProperties.ammoReloadType == WeaponProperties.AmmoReloadType.Single)
            {
                if (reloadingSingleInProgress)
                {
                    reloadSingleCountdown -= Time.deltaTime;

                    if (reloadSingleCountdown <= 0)
                    {
                        pController.TransferAmmo();
                        reloadingSingleInProgress = false;
                        reloadSingleCountdown = 0;

                        OnReloadEnd?.Invoke(this);

                    }

                    if (reloadIsCanceled)
                    {
                        reloadingSingleInProgress = false;
                        reloadSingleCountdown = 99; //Used to see in Inspector it works
                    }
                }
            }
        }
    }

    public void CheckAmmoTypeType(bool isOutOfAmmo)
    {
        if (!pController.isReloading && pController.pInventory.activeWeapon.GetComponent<WeaponProperties>().ammoType == WeaponProperties.AmmoType.Light && pController.pInventory.smallAmmo != 0 /* && !isInspecting */)
        {
            if (pController.pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo < pController.pInventory.activeWeapon.GetComponent<WeaponProperties>().ammoCapacity)
            {
                ReloadAnimation(isOutOfAmmo);
            }
        }
        else if (!pController.isReloading && pController.pInventory.activeWeapon.GetComponent<WeaponProperties>().ammoType == WeaponProperties.AmmoType.Heavy && pController.pInventory.heavyAmmo != 0 /* && !isInspecting */)
        {
            if (pController.pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo < pController.pInventory.activeWeapon.GetComponent<WeaponProperties>().ammoCapacity)
            {
                ReloadAnimation(isOutOfAmmo);
            }
        }
        else if (!pController.isReloading && pController.pInventory.activeWeapon.GetComponent<WeaponProperties>().ammoType == WeaponProperties.AmmoType.Power && pController.pInventory.powerAmmo != 0 /* && !isInspecting */)
        {
            if (pController.pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo < pController.pInventory.activeWeapon.GetComponent<WeaponProperties>().ammoCapacity)
            {
                ReloadAnimation(isOutOfAmmo);
            }
        }

    }

    public void ReloadAnimation(bool isOutOfAmmo)
    {
        if (pController.wProperties.ammoReloadType == WeaponProperties.AmmoReloadType.Magazine)
        {
            if (PV.IsMine)
                for (int i = 0; i < pController.pInventory.allWeaponsInInventory.Length; i++)
                    if (pController.pInventory.allWeaponsInInventory[i].gameObject == pController.wProperties.gameObject)
                        PV.RPC("PlayReloadSound_RPC", RpcTarget.All, i);
            //reloadAudioSource.clip = pController.wProperties.Reload_1;
            //reloadAudioSource.Play();

            if (!isOutOfAmmo)
                pController.anim.Play("Reload Ammo Left", 0, 0f);
            else
                pController.anim.Play("Reload Out Of Ammo", 0, 0f);
            pController.ScopeOut();

            if (pController.gwProperties.bulletInMagRenderer != null)
            {
                pController.gwProperties.bulletInMagRenderer.GetComponent
                <SkinnedMeshRenderer>().enabled = true;
            }

            Reload(magsAmmoLeft, 0, 0, 0);
        }

        if (pController.wProperties.ammoReloadType == WeaponProperties.AmmoReloadType.Shell)
        {
            int ammoNeededToReload = pController.wProperties.ammoCapacity - pController.wProperties.currentAmmo;
            int ammoToReload = 0;

            if (ammoNeededToReload > pController.pInventory.currentExtraAmmo)
            {
                ammoToReload = pController.pInventory.currentExtraAmmo;
            }
            else if (ammoNeededToReload <= pController.pInventory.currentExtraAmmo)
            {
                ammoToReload = ammoNeededToReload;
            }

            shellsToInsert = ammoToReload;
            Reload(0, DEFAULT_RELOAD_TIME / 1f,
                DEFAULT_RELOAD_TIME / 1f, 0);
            pController.anim.Play("Reload Open", 0, 0f);
            pController.ScopeOut();


        }
        if (pController.wProperties.ammoReloadType == WeaponProperties.AmmoReloadType.Single)
        {
            if (PV.IsMine)
                for (int i = 0; i < pController.pInventory.allWeaponsInInventory.Length; i++)
                    if (pController.pInventory.allWeaponsInInventory[i].gameObject == pController.wProperties.gameObject)
                        PV.RPC("PlayReloadSound_RPC", RpcTarget.All, i);
            //reloadAudioSource.clip = pController.wProperties.Reload_1;
            //reloadAudioSource.Play();
            pController.anim.Play("Reload", 0, 0f);
            pController.ScopeOut();

            Reload(0, 0, 0, singleReloadTime);
        }

        pController.Player3PReloadAnimation();
    }

    public void Reload(float countdownTime, float shellOpenTime, float shellInsertTime, float singleAmmoTime)
    {
        if (pController.wProperties.ammoReloadType == WeaponProperties.AmmoReloadType.Magazine)
        {
            reloadingMagInProgress = true;
            reloadIsCanceled = false;
            reloadCountdownMags = countdownTime;
        }

        if (pController.wProperties.ammoReloadType == WeaponProperties.AmmoReloadType.Shell)
        {
            openingWeaponForShells = true;
            reloadIsCanceled = false;
            reloadOpenCountdown = shellOpenTime;
            shellsReloadOpenTime = shellOpenTime;
            reloadInsertCountdown = shellInsertTime;
            shellsInsertTime = shellInsertTime;
        }

        if (pController.wProperties.ammoReloadType == WeaponProperties.AmmoReloadType.Single)
        {
            reloadingSingleInProgress = true;
            reloadIsCanceled = false;
            reloadSingleCountdown = singleAmmoTime;
        }
    }

    [PunRPC]
    void PlayReloadSound_RPC(int activeWeaponIndex)
    {
        reloadAudioSource.clip = pController.pInventory.allWeaponsInInventory[activeWeaponIndex].GetComponent<WeaponProperties>().Reload_1;
        reloadAudioSource.Play();
    }
}
