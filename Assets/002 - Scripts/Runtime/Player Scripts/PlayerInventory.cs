using Photon.Pun;
using Steamworks;
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
    [SerializeField] WeaponProperties _activeWeapon, _holsteredWeapon, _thirdWeapon;
    [SerializeField] WeaponProperties _leftWeapon;

    public WeaponProperties activeWeapon
    {
        get
        {
            if (_oddball.gameObject.activeInHierarchy) return _oddball;
            if (_flag.gameObject.activeInHierarchy) return _flag;

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
                //pController.GetComponent<PlayerThirdPersonModelManager>().spartanModel.GetComponent<Animator>().Play("Draw");
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


    public WeaponProperties thirdWeapon
    {
        get
        {
            return _thirdWeapon;
        }

        set
        {
            if (value)
            {
                print($"showing third weapon");
                pController.SetDrawingThirdWeapon();
                pController.Descope();
                value.gameObject.SetActive(true);
                value.equippedModel.SetActive(true);

                pController.GetComponent<PlayerThirdPersonModelManager>().spartanModel.GetComponent<Animator>().SetBool("dw idle", true);
                pController.GetComponent<PlayerThirdPersonModelManager>().spartanModel.GetComponent<Animator>().SetBool("Idle Pistol", false);
                pController.GetComponent<PlayerThirdPersonModelManager>().spartanModel.GetComponent<Animator>().SetBool("Idle Rifle", false);
                pController.GetComponent<PlayerThirdPersonModelManager>().spartanModel.GetComponent<Animator>().Play("dw draw");

                value.GetComponent<Animator>().SetBool("idle", false);
                value.GetComponent<Animator>().SetBool("dw idle", true);
                value.GetComponent<Animator>().Play("dw draw");

                activeWeapon.GetComponent<Animator>().SetBool("idle", false);
                activeWeapon.GetComponent<Animator>().SetBool("dw idle", true);
                activeWeapon.GetComponent<Animator>().Play("dw idle force");
                weaponDrawAudioSource.clip = value.draw;
                weaponDrawAudioSource.Play();


                value.OnCurrentAmmoChanged -= OnActiveWeaponAmmoChanged;
                value.OnCurrentAmmoChanged += OnActiveWeaponAmmoChanged;
            }
            else
            {
                print($"third weapon becomes null");
                if (_thirdWeapon != null)
                {
                    print($"hiding third weapon");
                    _thirdWeapon.OnCurrentAmmoChanged -= OnActiveWeaponAmmoChanged;
                    _thirdWeapon.gameObject.SetActive(false);

                    activeWeapon.GetComponent<Animator>().SetBool("idle", true);
                    activeWeapon.GetComponent<Animator>().SetBool("dw idle", false);
                    activeWeapon.GetComponent<Animator>().SetBool("dw walk", false);

                    pController.GetComponent<PlayerThirdPersonModelManager>().spartanModel.GetComponent<Animator>().SetBool("dw idle", false);
                    StartCoroutine(ToggleTPPistolIdle(1));
                }
            }

            _thirdWeapon = value;
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

    public bool isDualWielding { get { return thirdWeapon && thirdWeapon.isDualWieldable; } }
    public bool hasADualWieldableWeapon { get { if (activeWeapon.isDualWieldable || holsteredWeapon.isDualWieldable) return true; return false; } }
    public bool activeWeaponIsDualWieldable { get { return activeWeapon.isDualWieldable; } }

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
    public GameObject lowAmmoIndicator, lowAmmoIndicatorLeft, lowAmmoIndicatorRight;
    public GameObject noAmmoIndicator, noAmmoIndicatorLeft, noAmmoIndicatorRight;

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
    public bool hasEnnemyFlag { get { return _flag.gameObject.activeInHierarchy; } }
    public bool holdingObjective
    {
        get
        {
            if (_oddball.gameObject.activeInHierarchy) return true;
            if (_flag.gameObject.activeInHierarchy) return true;

            return false;
        }
    }

    public Transform bulletTrailHolder { get { return _fakeBulletTrailHolder; } }

    [SerializeField] Transform _fakeBulletTrailHolder;
    [SerializeField] Transform _fakeBulleTrailPrefab;
    [SerializeField] List<FakeBulletTrail> _fakeBulletTrailPool = new List<FakeBulletTrail>();
    [SerializeField] PlayerGunGameManager _playerGunGameManager;
    [SerializeField] WeaponProperties _oddball, _flag;
    [SerializeField] List<WeaponProperties> _weaponsWithOverheat = new List<WeaponProperties>();





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
            FakeBulletTrail fbtt = Instantiate(_fakeBulleTrailPrefab, _fakeBulletTrailHolder).GetComponent<FakeBulletTrail>();
            _fakeBulletTrailPool.Add(fbtt);
            _fakeBulletTrailPool[i].GetComponent<FakeBulletTrailDisable>().player = player;
            fbtt.gameObject.SetActive(false);
        }








        foreach (GameObject w in allWeaponsInInventory)
        {
            GameManager.SetLayerRecursively(w, 3);

            if (w.GetComponent<WeaponProperties>().leftWeapon)
                GameManager.SetLayerRecursively(w.GetComponent<WeaponProperties>().leftWeapon.gameObject, 3);
        }
        GameManager.SetLayerRecursively(_oddball.gameObject, 3);
        GameManager.SetLayerRecursively(_flag.gameObject, 3);
    }




    public void Start()
    {
        Debug.Log("PlayerInventory Start");
        player.OnPlayerIdAssigned -= OnPlayerIdAndRewiredIdAssigned_Delegate;
        player.OnPlayerIdAssigned += OnPlayerIdAndRewiredIdAssigned_Delegate;


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

        _weaponsWithOverheat = allWeaponsInInventory.Where(item => item.GetComponent<WeaponProperties>().ammoProjectileType == WeaponProperties.AmmoProjectileType.Plasma &&
        item.GetComponent<WeaponProperties>().plasmaColor != WeaponProperties.PlasmaColor.Shard).Select(item => item.GetComponent<WeaponProperties>()).ToList();


        for (int j = _weaponsWithOverheat.Count; j-- > 0;)
        {
            if (_weaponsWithOverheat[j].isDualWieldable)
            {
                _weaponsWithOverheat.Add(_weaponsWithOverheat[j].leftWeapon);
            }
        }
    }

    private void Update()
    {
        LowAmmoIndicatorControl();
    }

    private void FixedUpdate()
    {
        foreach (WeaponProperties wp in _weaponsWithOverheat) wp.HandleOverheat();
    }

    void OnActiveWeaponAmmoChanged(WeaponProperties weaponProperties)
    {
        LowAmmoIndicatorControl();
    }

    void LowAmmoIndicatorControl()
    {
        if (activeWeapon)
        {

            if (!player.isDualWielding)
            {
                lowAmmoIndicatorLeft.SetActive(false); noAmmoIndicatorLeft.SetActive(false);



                //if (!player.playerController.isReloading)
                //{
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
                //}
                //else
                //{
                //    lowAmmoIndicator.SetActive(false);
                //    noAmmoIndicator.SetActive(false);
                //}
            }
            else
            {
                lowAmmoIndicator.SetActive(false); noAmmoIndicator.SetActive(false);


                //if (!player.playerController.isReloadingRight)
                //{
                if (activeWeapon.loadedAmmo == 0 && activeWeapon.spareAmmo == 0)
                {
                    lowAmmoIndicatorRight.SetActive(false);
                    noAmmoIndicatorRight.SetActive(true);
                }
                else if (activeWeapon.loadedAmmo < activeWeapon.ammoCapacity * 0.4f)
                {
                    lowAmmoIndicatorRight.SetActive(true);
                    noAmmoIndicatorRight.SetActive(false);
                }
                else
                {
                    lowAmmoIndicatorRight.SetActive(false);
                    noAmmoIndicatorRight.SetActive(false);
                }
                //}
                //else
                //{
                //    lowAmmoIndicator.SetActive(false);
                //    noAmmoIndicator.SetActive(false);
                //}



                //if (!player.playerController.isReloadingLeft)
                //{
                if (thirdWeapon.loadedAmmo == 0 && thirdWeapon.spareAmmo == 0)
                {
                    lowAmmoIndicatorLeft.SetActive(false);
                    noAmmoIndicatorLeft.SetActive(true);
                }
                else if (thirdWeapon.loadedAmmo < thirdWeapon.ammoCapacity * 0.4f)
                {
                    lowAmmoIndicatorLeft.SetActive(true);
                    noAmmoIndicatorLeft.SetActive(false);
                }
                else
                {
                    lowAmmoIndicatorLeft.SetActive(false);
                    noAmmoIndicatorLeft.SetActive(false);
                }
                //}
                //else
                //{
                //    lowAmmoIndicatorLeft.SetActive(false);
                //    noAmmoIndicatorLeft.SetActive(false);
                //}
            }
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


        pController.Descope();
        allPlayerScripts.aimAssist.ResetRedReticule();

        if (pController.isReloading && pController.pInventory.weaponsEquiped[1] != null)
        {
            rScript.reloadIsCanceled = true;
        }

        if (pController.pInventory.holsteredWeapon != null && !player.isDead && !player.isRespawning)
        {
            Debug.Log("SwitchWeapons");

            if (hasEnnemyFlag) PV.RPC("SwitchWeapons_RPC", RpcTarget.All, (int)SwitchWeapons_Mode.dropFlag);
            else if (playerOddballActive) PV.RPC("SwitchWeapons_RPC", RpcTarget.All, (int)SwitchWeapons_Mode.dropOddball);
            else if (isDualWielding) PV.RPC("SwitchWeapons_RPC", RpcTarget.All, (int)SwitchWeapons_Mode.dropThirdWeapon);
            else PV.RPC("SwitchWeapons_RPC", RpcTarget.All, (int)SwitchWeapons_Mode.normal);
        }

        ChangeActiveAmmoCounter();
    }
    void OnBulletSpawned_Delegate(PlayerShooting playerShooting)
    {
        LowAmmoIndicatorControl();
    }

    void OnReloadEnd_Delegate(ReloadScript reloadScript)
    {
        UpdateAllExtraAmmoHuds();
    }



    enum SwitchWeapons_Mode { normal, dropThirdWeapon, dropOddball, dropFlag }

    [PunRPC]
    void SwitchWeapons_RPC(int mode) // everyone receives this
    {
        // everyone do this locally
        if ((SwitchWeapons_Mode)mode == SwitchWeapons_Mode.normal || (SwitchWeapons_Mode)mode == SwitchWeapons_Mode.dropThirdWeapon)
        {
            if ((SwitchWeapons_Mode)mode == SwitchWeapons_Mode.dropThirdWeapon)
            {
                if (PV.IsMine) DropThirdWeapon();
                thirdWeapon = null;
            }
        }
        else
        {
            pController.playerThirdPersonModelManager.OnActiveWeaponChanged_PlayTPSAnimations_Delegate(this);
            pController.SetDrawingWeaponCooldown();
            if ((SwitchWeapons_Mode)mode == SwitchWeapons_Mode.dropFlag)
            {
                _flag.gameObject.SetActive(false); _flag.equippedModel.SetActive(false); _flag.holsteredModel.SetActive(false);
            }
            else if ((SwitchWeapons_Mode)mode == SwitchWeapons_Mode.dropOddball)
            {
                _oddball.gameObject.SetActive(false); _oddball.equippedModel.SetActive(false); _oddball.holsteredModel.SetActive(false);
            }

            activeWeapon.gameObject.SetActive(true);
            activeWeapon.equippedModel.SetActive(true); activeWeapon.holsteredModel.SetActive(false);
            PlayDrawSound();
            pController.playerThirdPersonModelManager.OnActiveWeaponChanged_PlayTPSAnimations_Delegate(this);
        }




        if (player.isMine) // Do locally right now if its mine
        {
            print($"SwitchWeapons_RPC {isDualWielding} {(SwitchWeapons_Mode)mode}");
            if ((SwitchWeapons_Mode)mode == SwitchWeapons_Mode.dropFlag)
            {
                try { OnActiveWeaponChanged?.Invoke(this); } catch { }
                try { OnActiveWeaponChangedLate.Invoke(this); } catch { }
            }
            else if ((SwitchWeapons_Mode)mode == SwitchWeapons_Mode.dropOddball)
            {
                try { OnActiveWeaponChanged?.Invoke(this); } catch { }
                try { OnActiveWeaponChangedLate.Invoke(this); } catch { }
            }
            else if ((SwitchWeapons_Mode)mode == SwitchWeapons_Mode.dropThirdWeapon)
            {
                //DropThirdWeapon();
            }
            else
            {
                WeaponProperties previousActiveWeapon = activeWeapon;
                WeaponProperties newActiveWeapon = holsteredWeapon;

                activeWeapon = newActiveWeapon;
                holsteredWeapon = previousActiveWeapon;
            }


            if ((SwitchWeapons_Mode)mode == SwitchWeapons_Mode.normal || (SwitchWeapons_Mode)mode == SwitchWeapons_Mode.dropThirdWeapon)
            {
                // do nothing
            }
            else if ((SwitchWeapons_Mode)mode == SwitchWeapons_Mode.dropFlag)
            {
                NetworkGameManager.instance.AskMasterClientToSpawnFlag(player.weaponDropPoint.position, player.weaponDropPoint.forward, (player.team == GameManager.Team.Red ? GameManager.Team.Blue : GameManager.Team.Red));
            }
            else if ((SwitchWeapons_Mode)mode == SwitchWeapons_Mode.dropOddball)
            {
                NetworkGameManager.instance.AskMasterClientToSpawnOddball(player.weaponDropPoint.position, player.weaponDropPoint.forward);
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
                        print($"AssignWeapon {codeName} actWeap");

                        _activeWeapon = weap.GetComponent<WeaponProperties>();
                        _activeWeapon.loadedAmmo = _activeWeapon.ammoCapacity;
                        pController.GetComponent<PlayerThirdPersonModelManager>().spartanModel.GetComponent<Animator>().Play("Draw");

                        _activeWeapon.gameObject.SetActive(true);
                        try { _previousActiveWeapon.gameObject.SetActive(false); } catch { }


                        pController.weaponAnimator = activeWeapon.GetComponent<Animator>();
                        OnActiveWeaponChanged?.Invoke(this);
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



        StartingWeapon = "ar";
        StartingWeapon2 = "pistol";
        yield return new WaitForEndOfFrame(); // Withou this it will think the Array is Empty
        StartingWeapon = "ar";
        StartingWeapon2 = "pistol";


        if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
        {
            StartingWeapon = "ar";
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
            StartingWeapon = "ar";
            StartingWeapon2 = "pistol";
        }



        if (GameManager.instance.gameType == GameManager.GameType.Hill)
        {
            StartingWeapon = "ar";
            StartingWeapon2 = "pistol";
        }


        if (GameManager.instance.gameMode == GameManager.GameMode.Versus && GameManager.instance.gameType == GameManager.GameType.Duals)
        {
            StartingWeapon = "smg";
            StartingWeapon2 = "rvv";
        }

        if (GameManager.instance.gameMode == GameManager.GameMode.Versus && GameManager.instance.gameType == GameManager.GameType.Swords)
        {
            StartingWeapon = "sword";
            StartingWeapon2 = "pistol";
        }

        if (GameManager.instance.gameMode == GameManager.GameMode.Versus && GameManager.instance.gameType == GameManager.GameType.Martian)
        {
            StartingWeapon = "pr";
            StartingWeapon2 = "pp";
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
                pController.GetComponent<PlayerThirdPersonModelManager>().spartanModel.GetComponent<Animator>().SetBool("dw idle", false);
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
                pController.GetComponent<PlayerThirdPersonModelManager>().spartanModel.GetComponent<Animator>().SetBool("dw idle", false);
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
        thirdWeapon = null; // do it mine or not


        if (GameManager.instance.gameType != GameManager.GameType.Fiesta && GameManager.instance.gameType != GameManager.GameType.GunGame)
            StartCoroutine(EquipStartingWeapon());
        else
            TriggerStartGameBehaviour();
    }

    void OnPLayerDeath_Delegate(Player player)
    {
        Debug.Log("OnPLayerDeath_Delegate");
        _oddball.gameObject.SetActive(false); _flag.gameObject.SetActive(false);
        try { activeWeapon.animator.SetBool("Run", false); } catch { }


        foreach (WeaponProperties wp in _weaponsWithOverheat) wp.ResetOverheat();
    }

    void OnActiveWeaponChangedLate_Delegate(PlayerInventory playerInventory)
    {
        try
        {
            LowAmmoIndicatorControl();
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
    public void SpawnFakeBulletTrail(int lenghtOfTrail, Quaternion spray, bool bipedIsMine,
        Vector3? muzzlePosition = null, Vector3? lookAtThisTarget = null)
    {
        outOfFakeBullets = true;
        foreach (FakeBulletTrail fbt in _fakeBulletTrailPool)
        {
            if (!fbt.gameObject.activeInHierarchy)
            {
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

                    GameManager.SetBulletTrailLayer(fbt.layerChangeTarget.gameObject, ll);
                }
                else
                {
                    GameManager.SetBulletTrailLayer(fbt.layerChangeTarget.gameObject, 0);
                }

                if (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On)
                {
                    GameManager.SetBulletTrailLayer(fbt.layerChangeTarget.gameObject, 0);
                }

                Debug.Log($"SpawnFakeBulletTrail: {lenghtOfTrail}");
                fbt.fakeBulletTrailDisable.timeBeforeDisabling = 0.1f;
                fbt.scaleToChange.localScale = Vector3.one;
                fbt.rotationToTarget.localRotation = Quaternion.identity;
                fbt.sprayRotation.localRotation = Quaternion.identity;

                fbt.scaleToChange.localScale = new Vector3(lenghtOfTrail * 0.15f, lenghtOfTrail * 0.15f, Mathf.Clamp(lenghtOfTrail, 0, 999));
                fbt.sprayRotation.localRotation *= spray;



                fbt.gameObject.SetActive(true);
                fbt.transform.parent = null;
                outOfFakeBullets = false;



                if (muzzlePosition != null && muzzlePosition != Vector3.zero)
                {
                    fbt.transform.position = (Vector3)muzzlePosition;
                    fbt.scaleToChange.localScale = Vector3.one;
                    fbt.scaleToChange.localScale = new Vector3(lenghtOfTrail * 0.15f, lenghtOfTrail * 0.15f, Mathf.Clamp(lenghtOfTrail, 0, 999));
                }

                if (lookAtThisTarget != null && lookAtThisTarget != Vector3.zero)
                {
                    fbt.rotationToTarget.LookAt((Vector3)lookAtThisTarget);
                }

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
        DropThirdWeapon();
        pController.DisableSprint();
        _oddball.gameObject.SetActive(true);
        _activeWeapon.gameObject.SetActive(false);
        UpdateThirdPersonGunModelsOnCharacter();
        pController.GetComponent<PlayerThirdPersonModelManager>().spartanModel.GetComponent<Animator>().Play("Draw");
    }


    public void EquipFlag()
    {
        DropThirdWeapon();
        pController.DisableSprint();
        _flag.gameObject.SetActive(true);
        _activeWeapon.gameObject.SetActive(false);
        UpdateThirdPersonGunModelsOnCharacter();
        pController.GetComponent<PlayerThirdPersonModelManager>().OnActiveWeaponChanged_PlayTPSAnimations_Delegate(this);
    }

    public void HideFlag()
    {
        _activeWeapon.gameObject.SetActive(true);
        _flag.gameObject.SetActive(false);
        pController.GetComponent<PlayerThirdPersonModelManager>().OnActiveWeaponChanged_PlayTPSAnimations_Delegate(this);
    }


    void OnPlayerIdAndRewiredIdAssigned_Delegate(Player p)
    {
        print($"PlayerInventory OnPlayerIdAndRewiredIdAssigned_Delegate {transform.root.name} {player.isMine} {GameManager.instance.gameType}");


        foreach (GameObject w in allWeaponsInInventory)
        {
            if (player.isMine && GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.Off)
            {
                if (pController.rid == 0)
                    GameManager.SetLayerRecursively(w, 24);
                else if (pController.rid == 1)
                    GameManager.SetLayerRecursively(w, 26);
                else if (pController.rid == 2)
                    GameManager.SetLayerRecursively(w, 28);
                else if (pController.rid == 3)
                    GameManager.SetLayerRecursively(w, 30);



                if (w.GetComponent<WeaponProperties>().leftWeapon)
                {
                    if (pController.rid == 0)
                        GameManager.SetLayerRecursively(w.GetComponent<WeaponProperties>().leftWeapon.gameObject, 24);
                    else if (pController.rid == 1)
                        GameManager.SetLayerRecursively(w.GetComponent<WeaponProperties>().leftWeapon.gameObject, 26);
                    else if (pController.rid == 2)
                        GameManager.SetLayerRecursively(w.GetComponent<WeaponProperties>().leftWeapon.gameObject, 28);
                    else if (pController.rid == 3)
                        GameManager.SetLayerRecursively(w.GetComponent<WeaponProperties>().leftWeapon.gameObject, 30);
                }
            }
            else
            {
                GameManager.SetLayerRecursively(w, 3);

                if (w.GetComponent<WeaponProperties>().leftWeapon)
                    GameManager.SetLayerRecursively(w.GetComponent<WeaponProperties>().leftWeapon.gameObject, 3);
            }
        }


        if (GameManager.instance.gameType == GameManager.GameType.Oddball)
        {
            if (player.isMine && GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.Off)
            {
                if (pController.rid == 0)
                    GameManager.SetLayerRecursively(_oddball.gameObject, 24);
                else if (pController.rid == 1)
                    GameManager.SetLayerRecursively(_oddball.gameObject, 26);
                else if (pController.rid == 2)
                    GameManager.SetLayerRecursively(_oddball.gameObject, 28);
                else if (pController.rid == 3)
                    GameManager.SetLayerRecursively(_oddball.gameObject, 30);
            }
            else
            {
                GameManager.SetLayerRecursively(_oddball.gameObject, 3);
            }
        }

        if (GameManager.instance.gameType == GameManager.GameType.CTF)
        {
            if (player.isMine && GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.Off)
            {
                if (pController.rid == 0)
                    GameManager.SetLayerRecursively(_flag.gameObject, 24);
                else if (pController.rid == 1)
                    GameManager.SetLayerRecursively(_flag.gameObject, 26);
                else if (pController.rid == 2)
                    GameManager.SetLayerRecursively(_flag.gameObject, 28);
                else if (pController.rid == 3)
                    GameManager.SetLayerRecursively(_flag.gameObject, 30);
            }
            else
            {
                GameManager.SetLayerRecursively(_flag.gameObject, 3);
            }
        }
    }

    public void DropThirdWeapon()
    {
        if (PV.IsMine && thirdWeapon)
        {
            print("DropThirdWeapon");
            NetworkGameManager.SpawnNetworkWeapon(thirdWeapon, player.weaponDropPoint.position, player.weaponDropPoint.forward, currAmmo: thirdWeapon.loadedAmmo, spareAmmo: thirdWeapon.spareAmmo);
            PV.RPC("RemoveThirdWeapon_RPC", RpcTarget.All);
        }
    }


    [PunRPC]
    void RemoveThirdWeapon_RPC()
    {
        thirdWeapon = null;
    }



    [PunRPC]
    void PickupThirdWeapon(Vector3 collidingWeaponPosition, bool dw)// Called from PlayerInteractableObjectHandler
    {
        if (dw)
        {
            print("PickupThirdWeapon RPC");
            LootableWeapon weaponToLoot = WeaponPool.instance.weaponPool.Where(item => item.spawnPointPosition == collidingWeaponPosition).FirstOrDefault();

            foreach (GameObject w in allWeaponsInInventory)
                if (w.GetComponent<WeaponProperties>().codeName == weaponToLoot.codeName)
                {
                    thirdWeapon = w.GetComponent<WeaponProperties>().leftWeapon;
                    thirdWeapon.loadedAmmo = weaponToLoot.networkAmmo;
                    thirdWeapon.spareAmmo = weaponToLoot.spareAmmo;
                }

            weaponToLoot.gameObject.SetActive(false);
        }
    }
}
