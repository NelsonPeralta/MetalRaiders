using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerInventory : MonoBehaviourPun
{
    [Header("Other Scripts")]
    public AllPlayerScripts allPlayerScripts;
    public PlayerSFXs sfxManager;
    public CrosshairManager crosshairScript;
    public PlayerController pController;
    public PlayerProperties pProperties;
    public ChildManager cManager;
    public GeneralWeapProperties gwProperties;
    public ReloadScript rScript;
    public DualWielding dWielding;
    public PhotonView PV;

    [Space(20)]
    [Header("Data")]
    public string StartingWeapon;
    public string StartingWeapon2;
    public int activeWeapIs = 0;
    public WeaponProperties activeWeapon;
    public WeaponProperties holsteredWeapon;
    public bool hasSecWeap = false;

    [Space(20)]
    [Header("Equipped Weapons")]
    public GameObject[] weaponsEquiped = new GameObject[2];
    public int currentAmmo = 0;
    public int currentExtraAmmo = 0;

    [Header("Grenades")]
    public int grenades = 0;
    public int maxGrenades = 4;
    public Transform grenadePrefab;
    public Transform stickyGrenadePrefab;
    public float grenadeSpawnDelay = 0.25f;

    [Header("Dual Wielding")]
    public GameObject rightWeapon;
    public GameObject leftWeapon;
    public int rightWeaponCurrentAmmo;
    public int leftWeaponCurrentAmmo;

    [Space(20)]
    [Header("Unequipped Weapons")]
    public GameObject[] allWeaponsInInventory = new GameObject[25];

    [Space(20)]
    [Header("Ammo")]
    public int smallAmmo = 0;
    public int heavyAmmo = 0;
    public int powerAmmo = 0;

    [Space(10)]
    public int maxSmallAmmo = 72;
    public int maxHeavyAmmo = 60;
    public int maxPowerAmmo = 8;

    [Header("HUD Components")]
    public AmmoHUDCounter smallAmmoHudCounter;
    public AmmoHUDCounter heavyAmmoHudCounter;
    public AmmoHUDCounter powerAmmoHudCounter;
    public GameObject lowAmmoIndicator;
    public GameObject noAmmoIndicator;

    [Header("Weapon Meshes On Player")]
    public GameObject weaponMesh1;
    public GameObject weaponMesh2;
    public GameObject weaponMesh1Location2;
    public GameObject weaponMesh2Location2;
    public GameObject weaponMesh1Location3;
    public GameObject weaponMesh2Location3;

    AudioSource audioSource;

    /// <summary>
    /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>

    public void Start()
    {
        audioSource = GetComponent<AudioSource>();
        cManager = GetComponent<ChildManager>();

        StartCoroutine(EquipStartingWeapon());

        pController.OnPlayerSwitchWeapons += OnPlayerSwitchWeapons_Delegate;
        pController.OnPlayerLongInteract += OnPlayerSwitchWeapons_Delegate;
        pController.OnPlayerFire += OnPlayerFire_Delegate;
        rScript.OnReloadEnd += OnReloadEnd_Delegate;

        OnPlayerSwitchWeapons_Delegate(pController);
    }

    void OnPlayerSwitchWeapons_Delegate(PlayerController playerController)
    {
        if (!PV.IsMine)
            return;

        Debug.Log("On Player Switch Weapons Delegate");
        AmmoManager();

        if (pController.player.GetButtonDown("Switch Weapons") && !pProperties.isDead)
        {
            pController.ScopeOut();
            crosshairScript.DeactivateRedCrosshair();
            allPlayerScripts.aimAssist.ResetRedReticule();

            if (pController.isReloading && pController.pInventory.weaponsEquiped[1] != null)
            {
                rScript.reloadIsCanceled = true;

            }

            if (pController.pInventory.weaponsEquiped[1] != null && !pProperties.isDead && !pProperties.isRespawning)
            {
                PV.RPC("SwitchWeapons", RpcTarget.All);
            }
            crosshairScript.UpdateReticule();
        }

        UpdateActiveWeapon();
        AmmoManager();
        changeAmmoCounter();

        if (pController.isDualWielding)
        {
            UpdateDualWieldedWeaponAmmo();

            if (pController.player.GetButtonDown("Switch Weapons"))
            {
                pController.isDualWielding = false;
                pProperties.DropActiveWeapon(leftWeapon);

                activeWeapon.GetComponent<WeaponProperties>().currentAmmo = rightWeapon.GetComponent<WeaponProperties>().currentAmmo;
                activeWeapon.gameObject.SetActive(true);

                rightWeapon.SetActive(false);
                leftWeapon.SetActive(false);

                rightWeapon = null;
                leftWeapon = null;
            }
        }

        CheckIfLowAmmo();
    }

    void OnPlayerFire_Delegate(PlayerController playerController)
    {
        // TODO: Merge the 2 firing scripts (FullyAutomaticFire and SingleFire) into a single script. Create and Event in that script that will trigger this function after the bullet is spawned. The Coroutine will then no longer be necessary.
        StartCoroutine(OnPlayerFire_Coroutine());
    }

    IEnumerator OnPlayerFire_Coroutine()
    {
        yield return new WaitForEndOfFrame();
        UpdateActiveWeapon();
        AmmoManager();
        changeAmmoCounter();
    }

    void OnReloadEnd_Delegate(ReloadScript reloadScript)
    {
        UpdateActiveWeapon();
        AmmoManager();
        changeAmmoCounter();
    }

    [PunRPC]
    public void SwitchWeapons()
    {
        if (hasSecWeap == true)
        {
            if (weaponsEquiped[0].gameObject.activeSelf)
            {
                //DisableAmmoHUDCounters();
                weaponsEquiped[1].gameObject.SetActive(true);
                weaponsEquiped[0].gameObject.SetActive(false);

                activeWeapon = weaponsEquiped[1].GetComponent<WeaponProperties>();
                activeWeapIs = 1;

                UpdateActiveWeapon();
                AmmoManager();
                changeAmmoCounter();
                StartCoroutine(ToggleTPPistolIdle(0));
            }
            else if (weaponsEquiped[1].gameObject.activeSelf)
            {
                //DisableAmmoHUDCounters();
                weaponsEquiped[1].gameObject.SetActive(false);
                weaponsEquiped[0].gameObject.SetActive(true);

                activeWeapon = weaponsEquiped[0].GetComponent<WeaponProperties>();
                activeWeapIs = 0;

                UpdateActiveWeapon();
                AmmoManager();
                changeAmmoCounter();
                StartCoroutine(ToggleTPPistolIdle(1));
            }
            playDrawSound();
            crosshairScript.UpdateReticule();

            string test = weaponsEquiped[0].GetComponent<WeaponProperties>().weaponType.ToString();
            //Debug.Log(test);
        }
    }

    public void UpdateActiveWeapon()
    {
        if (activeWeapIs == 0)
        {
            if (weaponsEquiped[0] != null)
            {
                activeWeapon = weaponsEquiped[0].GetComponent<WeaponProperties>();
            }
        }
        else if (activeWeapIs == 1)
        {
            if (weaponsEquiped[1] != null)
            {
                activeWeapon = weaponsEquiped[1].GetComponent<WeaponProperties>();
            }
        }
    }
    public IEnumerator EquipStartingWeapon()
    {
        yield return new WaitForEndOfFrame(); // Withou this it will think the Array is Empty

        for (int i = 0; i < allWeaponsInInventory.Length; i++)
        {
            if (allWeaponsInInventory[i] != null)
            {
                if (allWeaponsInInventory[i].name == StartingWeapon)
                {
                    //DisableAmmoHUDCounters();
                    weaponsEquiped[0] = allWeaponsInInventory[i].gameObject;
                    //Debug.Log("Check 1");
                    activeWeapon = weaponsEquiped[0].GetComponent<WeaponProperties>();
                    weaponsEquiped[0] = activeWeapon.gameObject;
                    activeWeapIs = 0;
                    activeWeapon.GetComponent<WeaponProperties>().currentAmmo = activeWeapon.GetComponent<WeaponProperties>().maxAmmoInWeapon;
                    allWeaponsInInventory[i].gameObject.SetActive(true);
                    StartCoroutine(ToggleTPPistolIdle(1));
                }
                else if (allWeaponsInInventory[i].name == StartingWeapon2)
                {
                    allWeaponsInInventory[i].gameObject.SetActive(false);
                    weaponsEquiped[1] = allWeaponsInInventory[i].gameObject;
                    weaponsEquiped[1].GetComponent<WeaponProperties>().currentAmmo = weaponsEquiped[1].GetComponent<WeaponProperties>().maxAmmoInWeapon;
                    //Debug.Log("Check 1");
                    hasSecWeap = true;
                }
                else if (allWeaponsInInventory[i].name != StartingWeapon)
                {
                    allWeaponsInInventory[i].gameObject.SetActive(false);
                }
            }
        }
        changeAmmoCounter();
        if (PV.IsMine)
            playDrawSound();
    }

    void AmmoManager()
    {
        if (activeWeapon != null)
        {
            currentAmmo = activeWeapon.gameObject.GetComponent<WeaponProperties>().currentAmmo;
            //Debug.Log($"Active Weapon: {activeWeapon}\nAmmo: {activeWeapon.gameObject.GetComponent<WeaponProperties>().currentAmmo}");

            if (activeWeapon.GetComponent<WeaponProperties>().smallAmmo)
            {
                currentExtraAmmo = smallAmmo;
            }

            else if (activeWeapon.GetComponent<WeaponProperties>().heavyAmmo)
            {
                currentExtraAmmo = heavyAmmo;
            }

            else if (activeWeapon.GetComponent<WeaponProperties>().powerAmmo)
            {
                currentExtraAmmo = powerAmmo;
            }
        }
    }

    void UpdateDualWieldedWeaponAmmo()
    {
        rightWeaponCurrentAmmo = rightWeapon.GetComponent<WeaponProperties>().currentAmmo;
        leftWeaponCurrentAmmo = leftWeapon.GetComponent<WeaponProperties>().currentAmmo;
    }

    public void SwapGunsOnCharacter(int secondaryWeapon)
    {
        //Debug.Log("Active weapon:" + activeWeapon.name);

        foreach (GameObject weap in allWeaponsInInventory)
        {
            if (weap.GetComponent<WeaponProperties>().thirdPersonModelEquipped)
                weap.GetComponent<WeaponProperties>().thirdPersonModelEquipped.SetActive(false);
            if (weap.GetComponent<WeaponProperties>().thirdPersonModelUnequipped)
                weap.GetComponent<WeaponProperties>().thirdPersonModelUnequipped.SetActive(false);
        }

        if (activeWeapon.GetComponent<WeaponProperties>().thirdPersonModelEquipped &&
            activeWeapon.GetComponent<WeaponProperties>().thirdPersonModelUnequipped)
        {
            activeWeapon.GetComponent<WeaponProperties>().thirdPersonModelEquipped.SetActive(true);
            activeWeapon.GetComponent<WeaponProperties>().thirdPersonModelUnequipped.SetActive(false);
        }

        if (weaponsEquiped[1])
        {
            if (weaponsEquiped[secondaryWeapon].GetComponent<WeaponProperties>().thirdPersonModelEquipped &&
                weaponsEquiped[secondaryWeapon].GetComponent<WeaponProperties>().thirdPersonModelUnequipped)
            {
                weaponsEquiped[secondaryWeapon].GetComponent<WeaponProperties>().thirdPersonModelEquipped.SetActive(false);
                weaponsEquiped[secondaryWeapon].GetComponent<WeaponProperties>().thirdPersonModelUnequipped.SetActive(true);
            }
        }

        /*
        Debug.Log("Active weapon:" + activeWeapon.name);
        this.weaponMesh1.SetActive(false);
        this.weaponMesh2.SetActive(false);
        this.weaponMesh1Location2.SetActive(false);
        this.weaponMesh2Location2.SetActive(false);

        Mesh weaponMesh1 = activeWeapon.GetComponent<WeaponProperties>().weaponMesh;
        Material weaponMaterial1 = activeWeapon.GetComponent<WeaponProperties>().weaponMaterial;

        if (!activeWeapon.GetComponent<WeaponProperties>().location2 && !activeWeapon.GetComponent<WeaponProperties>().location3)
        {
            this.weaponMesh1.SetActive(true);

            this.weaponMesh1.GetComponent<MeshFilter>().mesh = weaponMesh1;
            this.weaponMesh1.GetComponent<MeshRenderer>().material = weaponMaterial1;
        }
        else if (activeWeapon.GetComponent<WeaponProperties>().location2)
        {
            this.weaponMesh1Location2.SetActive(true);

            this.weaponMesh1Location2.GetComponent<MeshFilter>().mesh = weaponMesh1;
            this.weaponMesh1Location2.GetComponent<MeshRenderer>().material = weaponMaterial1;
        }
        else if (activeWeapon.GetComponent<WeaponProperties>().location3)
        {
            this.weaponMesh1Location3.GetComponent<MeshFilter>().mesh = weaponMesh1;
            this.weaponMesh1Location3.GetComponent<MeshRenderer>().material = weaponMaterial1;
        }

        if (weaponsEquiped[1])
        {
            Mesh weaponMesh2 = weaponsEquiped[secondaryWeapon].GetComponent<WeaponProperties>().weaponMesh;
            Material weaponMaterial2 = weaponsEquiped[secondaryWeapon].GetComponent<WeaponProperties>().weaponMaterial;

            if (!weaponsEquiped[secondaryWeapon].GetComponent<WeaponProperties>().location2 &&
                !weaponsEquiped[secondaryWeapon].GetComponent<WeaponProperties>().location3)
            {
                this.weaponMesh2.SetActive(true);

                this.weaponMesh2.GetComponent<MeshFilter>().mesh = weaponMesh2;
                this.weaponMesh2.GetComponent<MeshRenderer>().material = weaponMaterial2;
            }
            else if (weaponsEquiped[secondaryWeapon].GetComponent<WeaponProperties>().location2)
            {
                this.weaponMesh2Location2.SetActive(true);

                this.weaponMesh2Location2.GetComponent<MeshFilter>().mesh = weaponMesh2;
                this.weaponMesh2Location2.GetComponent<MeshRenderer>().material = weaponMaterial2;
            }
            else if (weaponsEquiped[secondaryWeapon].GetComponent<WeaponProperties>().location3)
            {
                this.weaponMesh2Location3.GetComponent<MeshFilter>().mesh = weaponMesh2;
                this.weaponMesh2Location3.GetComponent<MeshRenderer>().material = weaponMaterial2;
            }
        }*/
    }

    public IEnumerator ToggleTPPistolIdle(int secondaryWeapon)
    {
        SwapGunsOnCharacter(secondaryWeapon);
        yield return new WaitForEndOfFrame();

        if (activeWeapon.GetComponent<WeaponProperties>().pistolIdle)
        {
            pController.tPersonController.anim.SetBool("Idle Pistol", true);
            pController.tPersonController.anim.SetBool("Idle Rifle", false);
        }
        else
        {
            pController.tPersonController.anim.SetBool("Idle Rifle", true);
            pController.tPersonController.anim.SetBool("Idle Pistol", false);
        }
    }

    public void playDrawSound()
    {
        audioSource.clip = activeWeapon.GetComponent<WeaponProperties>().draw;
        audioSource.Play();
    }

    void DisableAmmoHUDCounters()
    {
        smallAmmoHudCounter.changeToHolstered();
        heavyAmmoHudCounter.changeToHolstered();
        powerAmmoHudCounter.changeToHolstered();
    }

    public void changeAmmoCounter()
    {
        if (!activeWeapon)
            return;
        Debug.Log("The active weapon is:" + activeWeapon.name);
        if (activeWeapon.GetComponent<WeaponProperties>().getAmmoType() == "Small")
        {
            smallAmmoHudCounter.changeToDrawn();
            heavyAmmoHudCounter.changeToHolstered();
            powerAmmoHudCounter.changeToHolstered();
        }
        else if (activeWeapon.GetComponent<WeaponProperties>().getAmmoType() == "Heavy")
        {
            smallAmmoHudCounter.changeToHolstered();
            heavyAmmoHudCounter.changeToDrawn();
            powerAmmoHudCounter.changeToHolstered();
        }
        else if (activeWeapon.GetComponent<WeaponProperties>().getAmmoType() == "Power")
        {
            smallAmmoHudCounter.changeToHolstered();
            heavyAmmoHudCounter.changeToHolstered();
            powerAmmoHudCounter.changeToDrawn();
        }
    }

    public void UpdateAllExtraAmmoHuds()
    {
        smallAmmoHudCounter.UpdateExtraAmmo();
        heavyAmmoHudCounter.UpdateExtraAmmo();
        powerAmmoHudCounter.UpdateExtraAmmo();
    }

    public void CheckIfLowAmmo()
    {
        if (currentAmmo == 0 && currentExtraAmmo == 0)
        {
            lowAmmoIndicator.SetActive(false);
            noAmmoIndicator.SetActive(true);
        }
        else if (currentAmmo < activeWeapon.GetComponent<WeaponProperties>().maxAmmoInWeapon * 0.4f && currentExtraAmmo >= 0)
        {
            lowAmmoIndicator.SetActive(true);
            noAmmoIndicator.SetActive(false);
        }
        else
        {
            lowAmmoIndicator.SetActive(false);
            noAmmoIndicator.SetActive(false);
        }
    }
}
