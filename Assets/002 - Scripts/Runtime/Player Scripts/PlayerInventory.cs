﻿using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInventory : MonoBehaviourPun
{
    public delegate void PlayerInventoryEvent(PlayerInventory playerInventory);
    public PlayerInventoryEvent OnWeaponsSwitched, OnGrenadeChanged, OnActiveWeaponChanged,
        OnActiveWeaponChangedLate, OnHolsteredWeaponChanged, OnAmmoChanged;


    public PlayerGunGameManager playerGunGameManager { get { return _playerGunGameManager; } }



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
    public AimAssistCone aimAssistCone;

    [Space(20)]
    [Header("Data")]
    public string StartingWeapon;
    public string StartingWeapon2;
    public int activeWeapIs = 0;
    [SerializeField] WeaponProperties _activeWeapon;
    [SerializeField] WeaponProperties _holsteredWeapon;
    [SerializeField] WeaponProperties _leftWeapon;

    public WeaponProperties activeWeapon
    {
        get
        {
            if (_oddball.gameObject.activeInHierarchy) return _oddball;

            return _activeWeapon;
        }
        set
        {
            if (PV.IsMine)
            {
                WeaponProperties preVal = _activeWeapon;
                WeaponProperties preHol = _holsteredWeapon;

                if (preVal != value)
                {
                    pController.CancelReloadCoroutine();
                }

                try
                {
                    preVal.equippedModel.SetActive(false);
                    preVal.holsteredModel.SetActive(false);
                }
                catch { }

                _activeWeapon = value;
                _activeWeapon.DisableMuzzleFlash();

                //if (GameManager.instance.gameType == GameManager.GameType.Fiesta && _activeWeapon.codeName.Equals("rpg")) { _activeWeapon.loadedAmmo = 1; _activeWeapon.spareAmmo = 0; }
                //if (GameManager.instance.gameType == GameManager.GameType.Fiesta && _activeWeapon.codeName.Equals("sniper")) { _activeWeapon.loadedAmmo = 4; _activeWeapon.spareAmmo = 0; }

                pController.Descope();
                pController.GetComponent<PlayerThirdPersonModelManager>().spartanModel.GetComponent<Animator>().Play("Draw");
                PV.RPC("AssignWeapon", RpcTarget.Others, activeWeapon.codeName, true);
                if (!player.isDead && !player.isRespawning)
                {
                    pController.SetDrawingWeaponCooldown();
                    pController.Descope();
                    _activeWeapon.gameObject.SetActive(true);
                }

                pController.weaponAnimator = activeWeapon.GetComponent<Animator>();
                pController.weaponAnimator.Play("Draw", 0, 0f);

                activeWeapon.OnCurrentAmmoChanged -= OnActiveWeaponAmmoChanged;
                activeWeapon.OnCurrentAmmoChanged += OnActiveWeaponAmmoChanged;

                //PV.RPC("AssignWeapon", RpcTarget.Others, activeWeapon.codeName, true);
                try { OnActiveWeaponChanged?.Invoke(this); } catch { }
                try { OnActiveWeaponChangedLate.Invoke(this); } catch { }
            }
            PlayDrawSound();

        }
    }
    public WeaponProperties holsteredWeapon
    {
        get { return _holsteredWeapon; }
        set
        {
            if (PV.IsMine)
            {
                WeaponProperties preVal = _holsteredWeapon;

                try
                {
                    //preVal.equippedModelB.SetActive(false);  DO NOT ENABLE THIS LINE. IT WILL HIDE THE 3PS MODEL
                    preVal.holsteredModel.SetActive(false);
                }
                catch { }

                _holsteredWeapon = value;
                PV.RPC("AssignWeapon", RpcTarget.Others, holsteredWeapon.codeName, false);
                OnHolsteredWeaponChanged?.Invoke(this);
                try { _holsteredWeapon.holsteredModel.SetActive(true); } catch (System.Exception e) { Debug.LogWarning(e); }
                _holsteredWeapon.gameObject.SetActive(false);
            }
        }
    }

    public WeaponProperties leftWeapon
    {
        get { return _leftWeapon; }
        set
        {
            if (value != null)
            {
                foreach (Transform child in activeWeapon.transform)
                    child.gameObject.SetActive(false);

                activeWeapon.rightWeapon.gameObject.SetActive(true);
                activeWeapon.leftWeapon.gameObject.SetActive(true);

                PlayerUI pui = player.GetComponent<PlayerUI>();
                pui.leftWeaponUiHolder.SetActive(true);
                pui.leftActiveAmmoText.text = value.loadedAmmo.ToString();
                pui.leftSpareAmmoText.text = value.spareAmmo.ToString();
                pui.leftWeaponIcon.sprite = value.weaponIcon;
            }
            else
            {
                foreach (Transform child in activeWeapon.transform)
                    child.gameObject.SetActive(true);

                activeWeapon.leftWeapon.gameObject.SetActive(false);
                activeWeapon.rightWeapon.gameObject.SetActive(false);
            }

            _leftWeapon = value;
        }
    }

    public bool isDualWielding { get { return leftWeapon; } }

    public bool hasSecWeap = false;

    [Space(20)]
    [Header("Equipped Weapons")]
    public GameObject[] weaponsEquiped = new GameObject[2];

    [Header("Grenades")]
    int _fragGrenades, _plasmaGrenades;
    public int maxGrenades = 4;
    public Transform grenadePrefab;
    public Transform stickyGrenadePrefab;
    public float grenadeSpawnDelay = 0.25f;

    [Header("Dual Wielding")]
    public GameObject rightWeapon;
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

    public AudioSource weaponDrawAudioSource;

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

    public int fragGrenades
    {
        get { return _fragGrenades; }
        set
        {
            _fragGrenades = value;
            OnGrenadeChanged?.Invoke(this);
        }
    }

    public int plasmaGrenades
    {
        get { return _plasmaGrenades; }
        set
        {
            _plasmaGrenades = value;
            OnGrenadeChanged?.Invoke(this);
        }
    }

    public bool playerOddballActive { get { return _oddball.gameObject.activeInHierarchy; } }
    public Transform bulletTrailPool { get { return _fakeBulletTrailHolder; } }

    [SerializeField] Transform _fakeBulletTrailHolder;
    [SerializeField] Transform _fakeBulleTrailPrefab;
    [SerializeField] List<Transform> _fakeBulletTrailPool = new List<Transform>();
    [SerializeField] PlayerGunGameManager _playerGunGameManager;
    [SerializeField] WeaponProperties _oddball;





    List<WeaponProperties> _allWeapons = new List<WeaponProperties>(); // To replace allWeaponsInInventory variable




    private void Awake()
    {
        if (!PV.IsMine)
        {
            try
            {
                //for (int i = 0; i < 100; i++)
                //{
                //    Transform fbtt = Instantiate(_fakeBulleTrailPrefab, _fakeBulletTrailHolder);
                //    _fakeBulletTrailPool.Add(fbtt);
                //    fbtt.gameObject.SetActive(false);
                //}

                allThirdPersonEquippedWeaponsHolder.SetActive(true);

            }
            catch (System.Exception e) { Debug.LogWarning(e); }
        }

        for (int i = 0; i < 50; i++)
        {
            Transform fbtt = Instantiate(_fakeBulleTrailPrefab, _fakeBulletTrailHolder);
            _fakeBulletTrailPool.Add(fbtt);
            _fakeBulletTrailPool[i].GetComponent<FakeBulletTrailDisable>().player = player;
            fbtt.gameObject.SetActive(false);
        }
    }
    public void Start()
    {
        Debug.Log("PlayerInventory Start");
        transform.root.GetComponent<Player>().OnPlayerIdAssigned -= OnPlayerIdAndRewiredIdAssigned_Delegate;
        transform.root.GetComponent<Player>().OnPlayerIdAssigned += OnPlayerIdAndRewiredIdAssigned_Delegate;


        //OnActiveWeaponChanged += crosshairScript.OnActiveWeaponChanged_Delegate;
        OnActiveWeaponChanged += aimAssistCone.OnActiveWeaponChanged;
        player.OnPlayerRespawnEarly += OnPlayerRespawnEarly_Delegate;
        player.OnPlayerDeath += OnPLayerDeath_Delegate;
        OnAmmoChanged += OnAmmoChanged_Delegate;
        OnActiveWeaponChangedLate += OnActiveWeaponChangedLate_Delegate;

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

            int c = 0;
            foreach (GameObject w in allWeaponsInInventory.ToList())
            {
                w.GetComponent<WeaponProperties>().index = c;
                c++;
            }
        }
        catch { }

        if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
        {
            fragGrenades = maxGrenades;
        }
        else if (GameManager.instance.gameType == GameManager.GameType.Swat
                || GameManager.instance.gameType == GameManager.GameType.Retro
                /*|| GameManager.instance.gameType == GameManager.GameType.GunGame*/)
            fragGrenades = 1;
        else if (GameManager.instance.gameMode == GameManager.GameMode.Versus)
        {
            fragGrenades = 2;
        }
        if (GameManager.instance.gameType == GameManager.GameType.Hill)
        {
            fragGrenades = 1;
        }
    }

    void OnActiveWeaponAmmoChanged(WeaponProperties weaponProperties)
    {
        CheckLowAmmoIndicator();
    }

    void CheckLowAmmoIndicator()
    {

        if (activeWeapon.loadedAmmo == 0 && activeWeapon.spareAmmo == 0)
        {
            lowAmmoIndicator.SetActive(false);
            noAmmoIndicator.SetActive(true);
        }
        else if (activeWeapon.loadedAmmo < activeWeapon.ammoCapacity * 0.4f)
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
        Debug.Log("SwitchWeapons");


        pController.Descope();
        allPlayerScripts.aimAssist.ResetRedReticule();

        if (pController.isReloading && pController.pInventory.weaponsEquiped[1] != null)
        {
            rScript.reloadIsCanceled = true;
        }
        Debug.Log("SwitchWeapons");

        if (pController.pInventory.holsteredWeapon != null && !player.isDead && !player.isRespawning)
        {
            Debug.Log("SwitchWeapons");
            PV.RPC("SwitchWeapons_RPC", RpcTarget.All);
        }

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
    public void SwitchWeapons_RPC()
    {
        if (player.isMine)
        {
            if (!isDualWielding)
            {
                Debug.Log($"SwitchWeapons {player.name}");
                WeaponProperties previousActiveWeapon = activeWeapon;
                WeaponProperties newActiveWeapon = holsteredWeapon;

                if (_oddball.gameObject.activeInHierarchy)
                {
                    previousActiveWeapon = _activeWeapon;
                    NetworkGameManager.instance.DropOddball(player.weaponDropPoint.position, player.weaponDropPoint.forward);
                }

                activeWeapon = newActiveWeapon;
                holsteredWeapon = previousActiveWeapon;

                _oddball.gameObject.SetActive(false);
            }
            else
            {
                NetworkGameManager.SpawnNetworkWeapon(leftWeapon, player.weaponDropPoint.position, player.weaponDropPoint.forward);
                //player.DropWeaponOnDeath(leftWeapon);
                leftWeapon = null;
            }
        }
    }

    [PunRPC]
    void AssignWeapon(string codeName, bool actWeap = true)
    {
        print($"{player.name} AssignWeapon");
        if (!PV.IsMine)
        {
            print($"AssignWeapon {codeName}");
            pController.SetDrawingWeaponCooldown();

            foreach (GameObject weap in allWeaponsInInventory)
            {
                if (weap.GetComponent<WeaponProperties>().codeName == codeName)
                {
                    WeaponProperties _previousActiveWeapon = _activeWeapon;

                    try
                    {
                        _previousActiveWeapon.equippedModel.SetActive(false);
                        _previousActiveWeapon.holsteredModel.SetActive(false);
                    }
                    catch { }
                    try
                    {
                        _holsteredWeapon.equippedModel.SetActive(false);
                        _holsteredWeapon.holsteredModel.SetActive(false);
                    }
                    catch { }




                    if (actWeap) // activeWeapon
                    {
                        _activeWeapon = weap.GetComponent<WeaponProperties>();
                        _activeWeapon.loadedAmmo = _activeWeapon.ammoCapacity;
                        pController.GetComponent<PlayerThirdPersonModelManager>().spartanModel.GetComponent<Animator>().Play("Draw");

                        _activeWeapon.gameObject.SetActive(true);
                        try { _previousActiveWeapon.gameObject.SetActive(false); } catch { }


                        pController.weaponAnimator = activeWeapon.GetComponent<Animator>();
                        //UpdateThirdPersonGunModelsOnCharacter();
                    }
                    else
                    {
                        _holsteredWeapon = weap.GetComponent<WeaponProperties>();
                        try { _holsteredWeapon.holsteredModel.SetActive(true); } catch (System.Exception e) { Debug.LogWarning(e); }
                    }



                    if (_activeWeapon) _activeWeapon.DisableMuzzleFlash();
                    try
                    {
                        _activeWeapon.equippedModel.SetActive(true);
                        _holsteredWeapon.holsteredModel.SetActive(true);
                    }
                    catch { }
                }
            }

            if (player.isMine)
            {
                pController.weaponAnimator = activeWeapon.GetComponent<Animator>();
                pController.weaponAnimator.Play("Draw", 0, 0f);
            }

            PlayDrawSound();
        }
    }
    public IEnumerator EquipStartingWeapon()
    {
        Debug.Log("EquipStartingWeapon");

        _oddball.gameObject.SetActive(false);
        GetWeaponProperties("pr").currentOverheat = 0;



        StartingWeapon = "smg";
        StartingWeapon2 = "pistol";
        yield return new WaitForEndOfFrame(); // Withou this it will think the Array is Empty
        StartingWeapon = "smg";
        StartingWeapon2 = "pistol";


        if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
        {
            StartingWeapon = "smg";
            StartingWeapon2 = "pistol";
        }
        if (GameManager.instance.gameType == GameManager.GameType.Pro)
        {
            StartingWeapon = "br";
            StartingWeapon2 = "ar";
        }
        if (GameManager.instance.gameType == GameManager.GameType.Snipers)
        {
            StartingWeapon = "sniper";
            StartingWeapon2 = "pistol";
        }

        if (GameManager.instance.gameType == GameManager.GameType.Rockets)
        {
            StartingWeapon = "rpg";
            StartingWeapon2 = "gl";
        }

        if (GameManager.instance.gameType == GameManager.GameType.Shotguns)
        {
            StartingWeapon = "shotgun";
            StartingWeapon2 = "pistol";
        }
        if (GameManager.instance.gameType == GameManager.GameType.PurpleRain)
        {
            StartingWeapon = "cl";
            StartingWeapon2 = "pistol";
        }



        if (GameManager.instance.gameType == GameManager.GameType.Swat)
        {
            StartingWeapon = "br";
            StartingWeapon2 = "ar";
        }

        if (GameManager.instance.gameMode == GameManager.GameMode.Coop
             || GameManager.instance.gameType == GameManager.GameType.Retro)
        {
            StartingWeapon = "smg";
            StartingWeapon2 = "pistol";
        }



        if (GameManager.instance.gameType == GameManager.GameType.Hill)
        {
            StartingWeapon = "smg";
            StartingWeapon2 = "pistol";
        }

        Debug.Log(GetWeaponProperties(StartingWeapon));
        GetWeaponProperties(StartingWeapon).spareAmmo = GetWeaponProperties(StartingWeapon).ammoCapacity * 3;
        try { GetWeaponProperties(StartingWeapon2).spareAmmo = GetWeaponProperties(StartingWeapon2).ammoCapacity * 3; } catch { }


        for (int i = 0; i < allWeaponsInInventory.Length; i++)
        {
            if (allWeaponsInInventory[i] != null)
            {
                if (allWeaponsInInventory[i].GetComponent<WeaponProperties>().codeName == StartingWeapon)
                {
                    try
                    {
                        //DisableAmmoHUDCounters();
                        weaponsEquiped[0] = allWeaponsInInventory[i].gameObject;
                        activeWeapon = weaponsEquiped[0].GetComponent<WeaponProperties>();
                        weaponsEquiped[0] = activeWeapon.gameObject;
                        activeWeapIs = 0;
                        activeWeapon.GetComponent<WeaponProperties>().loadedAmmo = activeWeapon.GetComponent<WeaponProperties>().ammoCapacity;
                        allWeaponsInInventory[i].gameObject.SetActive(true);
                        StartCoroutine(ToggleTPPistolIdle(1));
                    }
                    catch { }
                }
                else if (allWeaponsInInventory[i].GetComponent<WeaponProperties>().codeName == StartingWeapon2)
                {
                    allWeaponsInInventory[i].gameObject.SetActive(false);
                    weaponsEquiped[1] = allWeaponsInInventory[i].gameObject;
                    weaponsEquiped[1].GetComponent<WeaponProperties>().loadedAmmo = weaponsEquiped[1].GetComponent<WeaponProperties>().ammoCapacity;
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

        try { activeWeapon.animator.SetBool("Run", false); } catch { }
    }

    void AssignRandomWeapons()
    {
        Debug.Log("AssignRandomWeapons");
        var random = new System.Random();
        int ind = random.Next(allWeaponsInInventory.Length);
        StartingWeapon = allWeaponsInInventory[ind].GetComponent<WeaponProperties>().codeName;

        int ind2 = ind;
        while (ind2 == ind) { ind2 = random.Next(allWeaponsInInventory.Length); }

        StartingWeapon2 = allWeaponsInInventory[ind2].GetComponent<WeaponProperties>().codeName;

    }
    void UpdateDualWieldedWeaponAmmo()
    {
        rightWeaponCurrentAmmo = rightWeapon.GetComponent<WeaponProperties>().loadedAmmo;
        leftWeaponCurrentAmmo = leftWeapon.GetComponent<WeaponProperties>().loadedAmmo;
    }

    void UpdateThirdPersonGunModelsOnCharacter()
    {
        foreach (GameObject awgo in allWeaponsInInventory)
        {
            WeaponProperties wp = awgo.GetComponent<WeaponProperties>();
            try
            {
                if (wp != activeWeapon)
                {
                    wp.equippedModel.SetActive(false);
                }

                if (wp == activeWeapon)
                {
                    try { wp.equippedModel.SetActive(true); } catch (Exception e) { Debug.LogWarning($"{e}"); }
                }
            }
            catch (Exception e) { Debug.LogWarning($"{e}"); }
        }
    }


    public IEnumerator ToggleTPPistolIdle(int secondaryWeapon)
    {
        yield return new WaitForEndOfFrame();

        if (activeWeapon.GetComponent<WeaponProperties>().idleHandlingAnimationType == WeaponProperties.IdleHandlingAnimationType.Pistol)
        {
            //if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
            {
                pController.GetComponent<PlayerThirdPersonModelManager>().spartanModel.GetComponent<Animator>().SetBool("Idle Pistol", true);
                pController.GetComponent<PlayerThirdPersonModelManager>().spartanModel.GetComponent<Animator>().SetBool("Idle Rifle", false);
            }
            //else if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
            //{
            //    pController.GetComponent<PlayerThirdPersonModelManager>().humanModel.GetComponent<Animator>().SetBool("Idle Pistol", true);
            //    pController.GetComponent<PlayerThirdPersonModelManager>().humanModel.GetComponent<Animator>().SetBool("Idle Rifle", false);
            //}
        }
        else
        {
            //if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
            {
                pController.GetComponent<PlayerThirdPersonModelManager>().spartanModel.GetComponent<Animator>().SetBool("Idle Pistol", false);
                pController.GetComponent<PlayerThirdPersonModelManager>().spartanModel.GetComponent<Animator>().SetBool("Idle Rifle", true);
            }
            //else if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
            //{
            //    pController.GetComponent<PlayerThirdPersonModelManager>().humanModel.GetComponent<Animator>().SetBool("Idle Pistol", false);
            //    pController.GetComponent<PlayerThirdPersonModelManager>().humanModel.GetComponent<Animator>().SetBool("Idle Rifle", true);
            //}
        }
    }

    public void PlayDrawSound()
    {
        if (MapCamera.instance.gameObject.activeSelf)
            return;
        Debug.Log($"{player.name} Play Draw Sound");
        weaponDrawAudioSource.clip = activeWeapon.GetComponent<WeaponProperties>().draw;
        weaponDrawAudioSource.Play();
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
        if (GameManager.instance.gameType != GameManager.GameType.Fiesta && GameManager.instance.gameType != GameManager.GameType.GunGame)
            StartCoroutine(EquipStartingWeapon());
        else
            TriggerStartGameBehaviour();
    }

    void OnPLayerDeath_Delegate(Player player)
    {
        Debug.Log("OnPLayerDeath_Delegate");
        try { activeWeapon.animator.SetBool("Run", false); } catch { }
    }

    void OnActiveWeaponChangedLate_Delegate(PlayerInventory playerInventory)
    {
        try
        {
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


    bool outOfFakeBullets;
    public void SpawnFakeBulletTrail(int distanceToTravel, Quaternion spray, bool bipedIsMine)
    {
        outOfFakeBullets = true;
        foreach (Transform fbt in _fakeBulletTrailPool)
        {
            if (!fbt.gameObject.activeInHierarchy)
            {
                GameManager.SetLayerRecursively(fbt.gameObject, 0);

                if (bipedIsMine)
                {
                    int ll = 0;

                    if (player.rid == 0)
                        ll = 25;
                    else if (player.rid == 1)
                        ll = 27;
                    else if (player.rid == 2)
                        ll = 29;
                    else if (player.rid == 3)
                        ll = 31;

                    GameManager.SetLayerRecursively(fbt.gameObject, ll);
                }

                Debug.Log("SpawnFakeBulletTrail");
                fbt.transform.localScale = new Vector3(distanceToTravel * 0.5f, distanceToTravel * 0.5f, Mathf.Clamp(distanceToTravel, 0, 999));
                fbt.transform.localRotation *= spray;
                //fbt.GetComponent<FakeBulletTrailDisable>().timeBeforeDisabling = ((float)distanceToTravel / FakeBulletTrailDisable.Speed);
                fbt.GetComponent<FakeBulletTrailDisable>().timeBeforeDisabling = 0.1f;



                fbt.gameObject.SetActive(true);
                fbt.transform.parent = null;
                outOfFakeBullets = false;
                break;
            }
        }

        if (outOfFakeBullets)
        {
            GameManager.GetRootPlayer().killFeedManager.EnterNewFeed("Out of trails");
        }
    }

    public void TriggerStartGameBehaviour()
    {
        if (GameManager.instance.gameType == GameManager.GameType.GunGame)
        {
            StartingWeapon = _playerGunGameManager.gunIndex[_playerGunGameManager.index].codeName;
            StartingWeapon2 = "pistol";
            //grenades = 1;
        }

        if (GameManager.instance.gameType == GameManager.GameType.Fiesta)
            AssignRandomWeapons();


        if (GameManager.instance.gameType == GameManager.GameType.Fiesta || GameManager.instance.gameType == GameManager.GameType.GunGame)
            for (int i = 0; i < allWeaponsInInventory.Length; i++)
            {
                if (allWeaponsInInventory[i] != null)
                {
                    if (allWeaponsInInventory[i].GetComponent<WeaponProperties>().codeName == StartingWeapon)
                    {
                        try
                        {
                            //DisableAmmoHUDCounters();
                            weaponsEquiped[0] = allWeaponsInInventory[i].gameObject;
                            activeWeapon = weaponsEquiped[0].GetComponent<WeaponProperties>();
                            weaponsEquiped[0] = activeWeapon.gameObject;
                            activeWeapIs = 0;
                            activeWeapon.GetComponent<WeaponProperties>().loadedAmmo = activeWeapon.GetComponent<WeaponProperties>().ammoCapacity;
                            activeWeapon.spareAmmo = Mathf.Clamp(activeWeapon.ammoCapacity * 2, 0, activeWeapon.maxSpareAmmo);
                            allWeaponsInInventory[i].gameObject.SetActive(true);
                            StartCoroutine(ToggleTPPistolIdle(1));
                        }
                        catch { }
                    }
                    else if (allWeaponsInInventory[i].GetComponent<WeaponProperties>().codeName == StartingWeapon2)
                    {
                        allWeaponsInInventory[i].gameObject.SetActive(false);
                        weaponsEquiped[1] = allWeaponsInInventory[i].gameObject;
                        weaponsEquiped[1].GetComponent<WeaponProperties>().loadedAmmo = weaponsEquiped[1].GetComponent<WeaponProperties>().ammoCapacity;
                        holsteredWeapon = weaponsEquiped[1].GetComponent<WeaponProperties>();
                        holsteredWeapon.spareAmmo = Mathf.Clamp(holsteredWeapon.ammoCapacity * 2, 0, holsteredWeapon.maxSpareAmmo);
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


    public void EquipOddball()
    {
        _oddball.gameObject.SetActive(true);
        _activeWeapon.gameObject.SetActive(false);
        UpdateThirdPersonGunModelsOnCharacter();
        pController.GetComponent<PlayerThirdPersonModelManager>().spartanModel.GetComponent<Animator>().Play("Draw");
    }


    void OnPlayerIdAndRewiredIdAssigned_Delegate(Player p)
    {
        foreach (GameObject w in allWeaponsInInventory)
        {
            if (player.isMine)
            {
                if (pController.rid == 0)
                    GameManager.SetLayerRecursively(w, 24);
                else if (pController.rid == 1)
                    GameManager.SetLayerRecursively(w, 26);
                else if (pController.rid == 2)
                    GameManager.SetLayerRecursively(w, 28);
                else if (pController.rid == 3)
                    GameManager.SetLayerRecursively(w, 30);
            }
            else
            {
                GameManager.SetLayerRecursively(w, 3);
            }
        }
    }
}
