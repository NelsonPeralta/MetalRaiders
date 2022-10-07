using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class PlayerInventory : MonoBehaviourPun
{
    public delegate void PlayerInventoryEvent(PlayerInventory playerInventory);
    public PlayerInventoryEvent OnWeaponsSwitched, OnGrenadeChanged, OnActiveWeaponChanged, OnActiveWeaponChangedLate, OnAmmoChanged;
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
    [SerializeField] AmmoHUDCounter _activeAmmoHUDCounter;

    public AmmoHUDCounter activeAmmoHUDCounter
    {
        get { return _activeAmmoHUDCounter; }
        set
        {
            smallAmmoHudCounter.ChangeToHolstered();
            powerAmmoHudCounter.ChangeToHolstered();
            heavyAmmoHudCounter.ChangeToHolstered();

            _activeAmmoHUDCounter = value;
            activeAmmoHUDCounter.ChangeToDrawn();
            activeAmmoHUDCounter.ammoTextDrawn.text = activeWeapon.currentAmmo.ToString();
        }
    }
    public WeaponProperties activeWeapon
    {
        get { return _activeWeapon; }
        set
        {
            if (PV.IsMine)
            {

                _activeWeapon = value;
                try
                {
                    OnActiveWeaponChanged.Invoke(this);
                }
                catch { }
                _activeWeapon.gameObject.SetActive(true);
                pController.weaponAnimator = activeWeapon.GetComponent<Animator>();

                activeWeapon.OnCurrentAmmoChanged -= OnActiveWeaponAmmoChanged;
                activeWeapon.OnCurrentAmmoChanged += OnActiveWeaponAmmoChanged;
                if (activeWeapon.GetComponent<WeaponProperties>().ammoType == WeaponProperties.AmmoType.Light)
                    currentExtraAmmo = smallAmmo;
                else if (activeWeapon.GetComponent<WeaponProperties>().ammoType == WeaponProperties.AmmoType.Heavy)
                    currentExtraAmmo = heavyAmmo;
                else if (activeWeapon.GetComponent<WeaponProperties>().ammoType == WeaponProperties.AmmoType.Power)
                    currentExtraAmmo = powerAmmo;

                if (activeWeapon.GetComponent<WeaponProperties>().ammoType == WeaponProperties.AmmoType.Light)
                    activeAmmoHUDCounter = smallAmmoHudCounter;
                else if (activeWeapon.GetComponent<WeaponProperties>().ammoType == WeaponProperties.AmmoType.Heavy)
                    activeAmmoHUDCounter = heavyAmmoHudCounter;
                else if (activeWeapon.GetComponent<WeaponProperties>().ammoType == WeaponProperties.AmmoType.Power)
                    activeAmmoHUDCounter = powerAmmoHudCounter;

                PV.RPC("AssignWeapon", RpcTarget.Others, activeWeapon.codeName, true);
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
                _holsteredWeapon.gameObject.SetActive(false);
                PV.RPC("AssignWeapon", RpcTarget.Others, holsteredWeapon.codeName, false);
            }
        }
    }
    public bool hasSecWeap = false;

    [Space(20)]
    [Header("Equipped Weapons")]
    public GameObject[] weaponsEquiped = new GameObject[2];
    public int currentExtraAmmo = 0;

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

    [SerializeField] int _smallAmmo = 0;
    [SerializeField] int _heavyAmmo = 0;
    [SerializeField] int _powerAmmo = 0;

    public int smallAmmo
    {
        get { return _smallAmmo; }
        set
        {
            _smallAmmo = value;
            if (_smallAmmo > maxSmallAmmo)
                _smallAmmo = maxSmallAmmo;

            OnAmmoChanged?.Invoke(this);
        }
    }
    public int heavyAmmo
    {
        get { return _heavyAmmo; }
        set
        {
            _heavyAmmo = value;
            if (_heavyAmmo > maxHeavyAmmo)
                _heavyAmmo = maxHeavyAmmo;

            OnAmmoChanged?.Invoke(this);
        }
    }
    public int powerAmmo
    {
        get { return _powerAmmo; }
        set
        {
            _powerAmmo = value;
            if (_powerAmmo > maxPowerAmmo)
                _powerAmmo = maxPowerAmmo;

            OnAmmoChanged?.Invoke(this);
        }
    }

    [SerializeField] int _maxSmallAmmo = 144;
    [SerializeField] int _maxHeavyAmmo = 120;
    [SerializeField] int _maxPowerAmmo = 8;

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
                maxSmallAmmo *= 2;
                maxHeavyAmmo *= 2;
                maxPowerAmmo *= 2;

                smallAmmo = maxSmallAmmo;
                heavyAmmo = maxHeavyAmmo;
                powerAmmo = maxPowerAmmo;

                smallAmmoHudCounter.extraAmmoText.text = smallAmmo.ToString();
                heavyAmmoHudCounter.extraAmmoText.text = heavyAmmo.ToString();
                powerAmmoHudCounter.extraAmmoText.text = powerAmmo.ToString();

                //AmmoManager();
                ChangeActiveAmmoCounter();
            }
        }
    }
    public int maxSmallAmmo
    {
        get { return _maxSmallAmmo; }
        private set { _maxSmallAmmo = value; }
    }

    public int maxHeavyAmmo
    {
        get { return _maxHeavyAmmo; }
        private set { _maxHeavyAmmo = value; }
    }

    public int maxPowerAmmo
    {
        get { return _maxPowerAmmo; }
        private set { _maxPowerAmmo = value; }
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
        player.OnPlayerRespawnEarly += OnPlayerRespawnEarly_Delegate;
        OnAmmoChanged += OnAmmoChanged_Delegate;
        OnActiveWeaponChangedLate += OnActiveWeaponChangedLate_Delegate;
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
            maxSmallAmmo *= 2;
            maxHeavyAmmo *= 2;
            maxPowerAmmo *= 2;
            maxGrenades *= 2;

            smallAmmo = maxSmallAmmo;
            heavyAmmo = maxHeavyAmmo;
            powerAmmo = maxPowerAmmo;
            grenades = maxGrenades;
        }
        else if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
        {
            smallAmmo = 72;
            heavyAmmo = 60;
            powerAmmo = 8;
            grenades = 2;
        }
    }

    void OnActiveWeaponAmmoChanged(WeaponProperties weaponProperties)
    {
        activeAmmoHUDCounter.ammoTextDrawn.text = activeWeapon.currentAmmo.ToString();
        CheckLowAmmoIndicator();
    }

    void CheckLowAmmoIndicator()
    {
        lowAmmoIndicator.SetActive(false);
        noAmmoIndicator.SetActive(false);

        if (activeWeapon.currentAmmo == 0 && currentExtraAmmo == 0)
        {
            lowAmmoIndicator.SetActive(false);
            noAmmoIndicator.SetActive(true);
        }
        else if (activeWeapon.currentAmmo < activeWeapon.GetComponent<WeaponProperties>().ammoCapacity * 0.4f && currentExtraAmmo >= 0)
        {
            lowAmmoIndicator.SetActive(true);
            noAmmoIndicator.SetActive(false);
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

        player.GetComponent<PlayerUI>().activeWeaponIconText.text = $"<sprite={WeaponProperties.spriteIdDic[activeWeapon.codeName]}>";
        player.GetComponent<PlayerUI>().holsteredWeaponIconText.text = $"<sprite={WeaponProperties.spriteIdDic[holsteredWeapon.codeName]}>";

        heavyAmmoHudCounter.gameObject.SetActive(false);
        powerAmmoHudCounter.gameObject.SetActive(false);
        smallAmmoHudCounter.gameObject.SetActive(false);

        if (activeWeapon.GetComponent<WeaponProperties>().ammoType == WeaponProperties.AmmoType.Light)
        {
            smallAmmoHudCounter.gameObject.SetActive(true);
            smallAmmoHudCounter.ChangeToDrawn();
            //heavyAmmoHudCounter.ChangeToHolstered();
            //powerAmmoHudCounter.ChangeToHolstered();
        }
        else if (activeWeapon.GetComponent<WeaponProperties>().ammoType == WeaponProperties.AmmoType.Heavy)
        {
            heavyAmmoHudCounter.gameObject.SetActive(true);
            heavyAmmoHudCounter.ChangeToDrawn();

            //smallAmmoHudCounter.ChangeToHolstered();
            //powerAmmoHudCounter.ChangeToHolstered();
        }
        else if (activeWeapon.GetComponent<WeaponProperties>().ammoType == WeaponProperties.AmmoType.Power)
        {
            powerAmmoHudCounter.gameObject.SetActive(true);
            powerAmmoHudCounter.ChangeToDrawn();

            //smallAmmoHudCounter.ChangeToHolstered();
            //heavyAmmoHudCounter.ChangeToHolstered();
        }
    }

    public void UpdateAllExtraAmmoHuds()
    {
        //AmmoManager();
        smallAmmoHudCounter.UpdateExtraAmmo();
        heavyAmmoHudCounter.UpdateExtraAmmo();
        powerAmmoHudCounter.UpdateExtraAmmo();
    }

    void OnAmmoChanged_Delegate(PlayerInventory playerInventory)
    {
        smallAmmoHudCounter.extraAmmoText.text = smallAmmo.ToString();
        heavyAmmoHudCounter.extraAmmoText.text = heavyAmmo.ToString();
        powerAmmoHudCounter.extraAmmoText.text = powerAmmo.ToString();
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
}
