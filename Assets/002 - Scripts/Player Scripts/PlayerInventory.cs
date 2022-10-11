using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class PlayerInventory : MonoBehaviourPun
{
    public delegate void PlayerInventoryEvent(PlayerInventory playerInventory);
    public PlayerInventoryEvent OnWeaponsSwitched, OnGrenadeChanged, OnActiveWeaponChanged, OnActiveWeaponChangedLate, OnHolsteredWeaponChanged, OnAmmoChanged;
    [Header("Other Scripts")]
    public AllPlayerScripts allPlayerScripts;
    public PlayerSFXs sfxManager;
    public CrosshairManager crosshairScript;
    public PlayerController pController;
    public Player player;
    public GeneralWeapProperties gwProperties;
    public ReloadScript rScript;
    public DualWielding dWielding;
    public PhotonView PV;
    public PlayerWeaponSwapping playerWeaponSwapping;
    public PlayerShooting playerShooting;

    [Space(20)]
    [Header("Data")]
    public string StartingWeapon;
    public string StartingWeapon2;
    public int activeWeapIs = 0;
    [SerializeField] WeaponProperties _activeWeapon;
    [SerializeField] WeaponProperties _holsteredWeapon;

    public WeaponProperties activeWeapon
    {
        get { return _activeWeapon; }
        set
        {
            if (PV.IsMine)
            {

                _activeWeapon = value;
                _activeWeapon.gameObject.SetActive(true);

                pController.weaponAnimator = activeWeapon.GetComponent<Animator>();

                activeWeapon.OnCurrentAmmoChanged -= OnActiveWeaponAmmoChanged;
                activeWeapon.OnCurrentAmmoChanged += OnActiveWeaponAmmoChanged;

                PV.RPC("AssignWeapon", RpcTarget.Others, activeWeapon.codeName, true);
                try { OnActiveWeaponChanged?.Invoke(this); } catch { }
                try { OnActiveWeaponChangedLate.Invoke(this); } catch { }
            }


        }
    }
    public WeaponProperties holsteredWeapon
    {
        get { return _holsteredWeapon; }
        set
        {
            if (PV.IsMine)
            {
                _holsteredWeapon = value;
                PV.RPC("AssignWeapon", RpcTarget.Others, holsteredWeapon.codeName, false);
                OnHolsteredWeaponChanged?.Invoke(this);
                _holsteredWeapon.gameObject.SetActive(false);
            }
        }
    }
    public bool hasSecWeap = false;

    [Space(20)]
    [Header("Equipped Weapons")]
    public GameObject[] weaponsEquiped = new GameObject[2];

    [Header("Grenades")]
    int _grenades;
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

    [Header("HUD Components")]
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

    public GameObject allThirdPersonEquippedWeaponsHolder;

    bool _hasAmmoUpgrade;
    public bool hasAmmoUpgrade
    {
        get { return _hasAmmoUpgrade; }
        set
        {
            bool previousValue = _hasAmmoUpgrade;
            _hasAmmoUpgrade = value;

            if (value && !previousValue)
            {
                ChangeActiveAmmoCounter();
            }
        }
    }
    /// <summary>
    /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    /// 

    public int grenades
    {
        get { return _grenades; }
        set
        {
            _grenades = value;
            OnGrenadeChanged?.Invoke(this);
        }
    }

    private void Awake()
    {
        if (!PV.IsMine)
        {
            allThirdPersonEquippedWeaponsHolder.SetActive(true);
            foreach (GameObject w in allWeaponsInInventory)
                GameManager.SetLayerRecursively(w, 31);
        }
    }
    public void Start()
    {
        //OnActiveWeaponChanged += crosshairScript.OnActiveWeaponChanged_Delegate;
        player.OnPlayerRespawnEarly += OnPlayerRespawnEarly_Delegate;
        OnAmmoChanged += OnAmmoChanged_Delegate;
        OnActiveWeaponChangedLate += OnActiveWeaponChangedLate_Delegate;
        OnActiveWeaponChangedLate += crosshairScript.OnActiveWeaponChanged_Delegate;
        audioSource = GetComponent<AudioSource>();

        StartCoroutine(EquipStartingWeapon());

        try
        {
            pController.OnPlayerSwitchWeapons += OnPlayerSwitchWeapons_Delegate;
            //pController.OnPlayerLongInteract += OnPlayerSwitchWeapons_Delegate;
            rScript.OnReloadEnd += OnReloadEnd_Delegate;
            playerWeaponSwapping.OnWeaponPickup += OnPlayerWeaponSwapping_Delegate;

            //OnPlayerSwitchWeapons_Delegate(pController);
            playerShooting.OnBulletSpawned += OnBulletSpawned_Delegate;
            pController.GetComponent<ReloadScript>().OnReloadEnd += OnReloadEnd_Delegate;
        }
        catch { }

        if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
        {
            maxGrenades *= 2;

            grenades = maxGrenades;
        }
        else if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
        {
            grenades = 2;
        }
    }

    void OnActiveWeaponAmmoChanged(WeaponProperties weaponProperties)
    {
        CheckLowAmmoIndicator();
    }

    void CheckLowAmmoIndicator()
    {
        lowAmmoIndicator.SetActive(false);
        noAmmoIndicator.SetActive(false);

        if (activeWeapon.currentAmmo < activeWeapon.ammoCapacity * 0.4f)
        {
            lowAmmoIndicator.SetActive(true);
            noAmmoIndicator.SetActive(false);
        }
        if (activeWeapon.currentAmmo == 0 && activeWeapon.spareAmmo == 0)
        {
            lowAmmoIndicator.SetActive(false);
            noAmmoIndicator.SetActive(true);
        }
    }

    void OnPlayerWeaponSwapping_Delegate(PlayerWeaponSwapping playerWeaponSwapping)
    {
        //AmmoManager();
        ChangeActiveAmmoCounter();
        UpdateThirdPersonGunModelsOnCharacter();
    }

    void OnPlayerSwitchWeapons_Delegate(PlayerController playerController)
    {
        if (!PV.IsMine)
            return;

        pController.ScopeOut();
        crosshairScript.DeactivateRedCrosshair();
        allPlayerScripts.aimAssist.ResetRedReticule();

        if (pController.isReloading && pController.pInventory.weaponsEquiped[1] != null)
        {
            rScript.reloadIsCanceled = true;

        }

        if (pController.pInventory.weaponsEquiped[1] != null && !player.isDead && !player.isRespawning)
        {
            PV.RPC("SwitchWeapons", RpcTarget.All);
        }
        crosshairScript.UpdateReticule();

        ChangeActiveAmmoCounter();
    }
    void OnBulletSpawned_Delegate(PlayerShooting playerShooting)
    {
        CheckLowAmmoIndicator();
    }

    void OnReloadEnd_Delegate(ReloadScript reloadScript)
    {
        UpdateAllExtraAmmoHuds();
    }

    [PunRPC]
    public void SwitchWeapons()
    {
        WeaponProperties previousActiveWeapon = activeWeapon;
        WeaponProperties newActiveWeapon = holsteredWeapon;

        activeWeapon = newActiveWeapon;
        holsteredWeapon = previousActiveWeapon;
        UpdateThirdPersonGunModelsOnCharacter();
    }

    [PunRPC]
    void AssignWeapon(string codeName, bool actWeap = true)
    {
        if (!PV.IsMine)
        {
            foreach (GameObject weap in allWeaponsInInventory)
            {
                if (weap.GetComponent<WeaponProperties>().codeName == codeName)
                {
                    if (actWeap) // activeWeapon
                        _activeWeapon = weap.GetComponent<WeaponProperties>();
                    else
                        _holsteredWeapon = weap.GetComponent<WeaponProperties>();

                    try { OnActiveWeaponChangedLate.Invoke(this); } catch { }
                }

            }
        }
    }
    public IEnumerator EquipStartingWeapon()
    {
        yield return new WaitForEndOfFrame(); // Withou this it will think the Array is Empty

        if (GameManager.instance.gameType == GameManager.GameType.Slayer)
        {
            StartingWeapon = "m4";
            StartingWeapon2 = "m1911";
        }
        if (GameManager.instance.gameType == GameManager.GameType.Pro)
        {
            StartingWeapon = "m16";
            //StartingWeapon2 = "patriot";
        }
        if (GameManager.instance.gameType == GameManager.GameType.Snipers)
        {
            StartingWeapon = "r700";
            //StartingWeapon2 = "patriot";
        }

        if (GameManager.instance.gameType == GameManager.GameType.Rockets)
        {
            StartingWeapon = "rpg";
            StartingWeapon2 = "m32";
        }

        GetWeaponProperties(StartingWeapon).spareAmmo = GetWeaponProperties(StartingWeapon).ammoCapacity * 3;
        try { GetWeaponProperties(StartingWeapon2).spareAmmo = GetWeaponProperties(StartingWeapon2).ammoCapacity * 3; } catch { }


        if (GameManager.instance.gameType == GameManager.GameType.Fiesta)
        {
            AssignRandomWeapons();
        }



        for (int i = 0; i < allWeaponsInInventory.Length; i++)
        {
            if (allWeaponsInInventory[i] != null)
            {
                if (allWeaponsInInventory[i].GetComponent<WeaponProperties>().codeName == StartingWeapon)
                {
                    //DisableAmmoHUDCounters();
                    weaponsEquiped[0] = allWeaponsInInventory[i].gameObject;
                    //Debug.Log("Check 1");
                    activeWeapon = weaponsEquiped[0].GetComponent<WeaponProperties>();
                    weaponsEquiped[0] = activeWeapon.gameObject;
                    activeWeapIs = 0;
                    activeWeapon.GetComponent<WeaponProperties>().currentAmmo = activeWeapon.GetComponent<WeaponProperties>().ammoCapacity;
                    allWeaponsInInventory[i].gameObject.SetActive(true);
                    StartCoroutine(ToggleTPPistolIdle(1));
                }
                else if (allWeaponsInInventory[i].GetComponent<WeaponProperties>().codeName == StartingWeapon2)
                {
                    allWeaponsInInventory[i].gameObject.SetActive(false);
                    weaponsEquiped[1] = allWeaponsInInventory[i].gameObject;
                    weaponsEquiped[1].GetComponent<WeaponProperties>().currentAmmo = weaponsEquiped[1].GetComponent<WeaponProperties>().ammoCapacity;
                    holsteredWeapon = weaponsEquiped[1].GetComponent<WeaponProperties>();
                    hasSecWeap = true;
                }
                else if (allWeaponsInInventory[i].name != StartingWeapon)
                {
                    allWeaponsInInventory[i].gameObject.SetActive(false);
                }
            }
        }
        UpdateThirdPersonGunModelsOnCharacter();
        ChangeActiveAmmoCounter();
        if (PV.IsMine)
            PlayDrawSound();
    }

    void AssignRandomWeapons()
    {
        var random = new System.Random();
        int ind = random.Next(allWeaponsInInventory.Length);
        StartingWeapon = allWeaponsInInventory[ind].GetComponent<WeaponProperties>().codeName;

        int ind2 = ind;
        while (ind2 == ind) { ind2 = random.Next(allWeaponsInInventory.Length); }

        StartingWeapon2 = allWeaponsInInventory[ind2].GetComponent<WeaponProperties>().codeName;
    }
    void UpdateDualWieldedWeaponAmmo()
    {
        rightWeaponCurrentAmmo = rightWeapon.GetComponent<WeaponProperties>().currentAmmo;
        leftWeaponCurrentAmmo = leftWeapon.GetComponent<WeaponProperties>().currentAmmo;
    }

    void UpdateThirdPersonGunModelsOnCharacter()
    {
        foreach (GameObject awgo in allWeaponsInInventory)
        {
            WeaponProperties wp = awgo.GetComponent<WeaponProperties>();
            try { wp.equippedModelB.SetActive(false); } catch (Exception e) { Debug.LogWarning($"{e}"); }

            if (wp == activeWeapon)
                try { wp.equippedModelB.SetActive(true); } catch (Exception e) { Debug.LogWarning($"{e}"); }
        }
    }

    public void SwapGunsOnCharacter(int secondaryWeapon)
    {
        //Debug.Log("Active weapon:" + activeWeapon.name);

        foreach (GameObject weap in allWeaponsInInventory)
        {
            if (weap.GetComponent<WeaponProperties>().equippedModelA)
                weap.GetComponent<WeaponProperties>().equippedModelA.SetActive(false);
            if (weap.GetComponent<WeaponProperties>().unequippedModelA)
                weap.GetComponent<WeaponProperties>().unequippedModelA.SetActive(false);
        }

        if (activeWeapon.GetComponent<WeaponProperties>().equippedModelA &&
            activeWeapon.GetComponent<WeaponProperties>().unequippedModelA)
        {
            activeWeapon.GetComponent<WeaponProperties>().equippedModelA.SetActive(true);
            activeWeapon.GetComponent<WeaponProperties>().unequippedModelA.SetActive(false);
        }

        if (weaponsEquiped[1])
        {
            if (weaponsEquiped[secondaryWeapon].GetComponent<WeaponProperties>().equippedModelA &&
                weaponsEquiped[secondaryWeapon].GetComponent<WeaponProperties>().unequippedModelA)
            {
                weaponsEquiped[secondaryWeapon].GetComponent<WeaponProperties>().equippedModelA.SetActive(false);
                weaponsEquiped[secondaryWeapon].GetComponent<WeaponProperties>().unequippedModelA.SetActive(true);
            }
        }
    }

    public IEnumerator ToggleTPPistolIdle(int secondaryWeapon)
    {
        SwapGunsOnCharacter(secondaryWeapon);
        yield return new WaitForEndOfFrame();

        if (activeWeapon.GetComponent<WeaponProperties>().idleHandlingAnimationType == WeaponProperties.IdleHandlingAnimationType.Pistol)
        {
            if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
            {
                pController.GetComponent<PlayerThirdPersonModelManager>().spartanModel.GetComponent<Animator>().SetBool("Idle Pistol", true);
                pController.GetComponent<PlayerThirdPersonModelManager>().spartanModel.GetComponent<Animator>().SetBool("Idle Rifle", false);
            }
            else if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
            {
                pController.GetComponent<PlayerThirdPersonModelManager>().humanModel.GetComponent<Animator>().SetBool("Idle Pistol", true);
                pController.GetComponent<PlayerThirdPersonModelManager>().humanModel.GetComponent<Animator>().SetBool("Idle Rifle", false);
            }
        }
        else
        {
            if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
            {
                pController.GetComponent<PlayerThirdPersonModelManager>().spartanModel.GetComponent<Animator>().SetBool("Idle Pistol", false);
                pController.GetComponent<PlayerThirdPersonModelManager>().spartanModel.GetComponent<Animator>().SetBool("Idle Rifle", true);
            }
            else if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
            {
                pController.GetComponent<PlayerThirdPersonModelManager>().humanModel.GetComponent<Animator>().SetBool("Idle Pistol", false);
                pController.GetComponent<PlayerThirdPersonModelManager>().humanModel.GetComponent<Animator>().SetBool("Idle Rifle", true);
            }
        }
    }

    public void PlayDrawSound()
    {
        audioSource.clip = activeWeapon.GetComponent<WeaponProperties>().draw;
        audioSource.Play();
    }

    public void ChangeActiveAmmoCounter()
    {
        if (!activeWeapon)
            return;
    }

    public void UpdateAllExtraAmmoHuds()
    {
        //TODO: Deprecate this function
    }
    void OnAmmoChanged_Delegate(PlayerInventory playerInventory)
    {
    }

    void OnPlayerRespawnEarly_Delegate(Player player)
    {
        StartCoroutine(EquipStartingWeapon());
    }

    void OnActiveWeaponChangedLate_Delegate(PlayerInventory playerInventory)
    {
        try
        {
            PlayDrawSound();
            CheckLowAmmoIndicator();
            UpdateAllExtraAmmoHuds();
            UpdateThirdPersonGunModelsOnCharacter();
        }
        catch { }
    }

    public WeaponProperties GetWeaponProperties(string codeName)
    {
        for (int i = 0; i < allWeaponsInInventory.Length; i++)
            if (codeName == allWeaponsInInventory[i].GetComponent<WeaponProperties>().codeName)
                return allWeaponsInInventory[i].GetComponent<WeaponProperties>();
        return null;
    }
}
