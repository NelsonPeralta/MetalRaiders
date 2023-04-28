using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityEngine.SceneManagement;
using Photon.Pun;
using UnityEngine.EventSystems;
using ExitGames.Client.Photon.StructWrapping;

public class PlayerController : MonoBehaviourPun
{
    // Events
    public delegate void PlayerControllerEvent(PlayerController playerController);
    public PlayerControllerEvent OnPlayerSwitchWeapons, OnPlayerLongInteract,
        OnPlayerFire, OnPlayerFireButtonUp, OnPlayerTestButton, OnPLayerThrewGrenade,
        OnCrouchUp, OnCrouchDown, OnSprintStart, OnSprintStop, OnPlayerDeath,
        OnControllerTypeChangedToController, OnControllerTypeChangedToMouseAndKeyboard,
        OnPlayerScopeBtnDown, OnPlayerScopeBtnUp;

    public Player player { get { return GetComponent<Player>(); } }

    Vector3 _originalCharacterControllerCenter;

    [Header("Other Scripts")]
    public AllPlayerScripts allPlayerScripts;
    public WeaponSounds weapSounds;
    public PlayerInventory pInventory;
    public GeneralWeapProperties gwProperties;
    public Animator weaponAnimator;
    public Camera mainCam;
    public Camera gunCam;
    public Camera uiCam;
    public PlayerCamera camScript;
    public Rewired.Player rewiredPlayer;
    public int rid;
    public CrosshairManager crosshairScript;
    public ReloadScript rScript;
    public PlayerWeaponSwapping wPickup;
    public DualWieldingReload dwReload;
    public Movement movement;
    public Melee melee;
    public ControllerType activeControllerType
    {
        get { return _activeControllerType; }
        private set
        {

            if (value == _activeControllerType)
                return;

            _activeControllerType = value;

            if (value == ControllerType.Joystick)
            {
                OnControllerTypeChangedToController?.Invoke(this);
                //Debug.Log("OnControllerTypeChangedToController");
            }
            else
            {
                OnControllerTypeChangedToMouseAndKeyboard?.Invoke(this);
                //Debug.Log("OnControllerTypeChangedToMouseAndKeyboard");
            }
        }
    }

    public bool isDualWielding
    {
        get { return pInventory.leftWeapon; }
    }

    ControllerType _activeControllerType;

    public PhotonView PV;

    Quaternion savedCamRotation;

    [HideInInspector]
    public bool hasBeenHolstered = false, holstered, isRunning, isWalking;
    [HideInInspector]
    public bool isInspecting, aimSoundHasPlayed = false;

    public bool isReloading, reloadAnimationStarted, reloadWasCanceled, isFiring,
        isAiming, isThrowingGrenade, isCrouching, isDrawingWeapon, isSprinting;

    //Used for fire rate
    private float lastFired;
    [Header("Weapon Settings")]
    //How fast the weapon fires, higher value means faster rate of fire
    [Tooltip("How fast the weapon fires, higher value means faster rate of fire.")]
    public float fireRate;
    //Eanbles auto reloading when out of ammo
    [Tooltip("Enables auto reloading when out of ammo.")]
    public bool autoReload;
    //Delay between shooting last bullet and reloading
    public float autoReloadDelay;
    //Check if reloading
    //private bool isReloading;

    [Header("Weapon Settings")]
    public int ammoWeaponIsMissing;
    public float grenadeThrowForce = 625.0f;
    public bool fragGrenadesActive = true;
    public bool stickyGrenadesActive;

    [Header("Aiming Script Components")]
    public Aiming aimingScript;
    public GameObject aimingComponentsPivot;

    [Header("Dual Wielding")]
    public Animator animDWRight;
    public Animator animDWLeft;
    public WeaponProperties dwRightWP;
    public WeaponProperties dwLeftWP;
    public bool isHoldingShootBtn, isHoldingScopeBtn;
    public bool isShootingRight;
    public bool isShootingLeft;
    public bool isReloadingRight;
    public bool isReloadingLeft;
    public int ammoRightWeaponIsMissing;
    public int ammoLeftWeaponIsMissing;

    public bool pauseMenuOpen;

    [Header("Audio Sources")]
    public AudioSource playerVoice;
    public AudioSource grenadeSwitchAudioSource;
    public AudioSource meleeAudioSource;


    public bool isMeleeing { get { return _isMeleeing; } set { _isMeleeing = value; if (value) _meleeCooldown = 0.9f; } }


    bool _isMeleeing;
    float _meleeCooldown;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        OnPlayerTestButton += OnTestButton_Delegate;
    }
    public void Start()
    {
        _originalCharacterControllerCenter = GetComponent<CharacterController>().center;
        rewiredPlayer = ReInput.players.GetPlayer(rid);

        if (!PV.IsMine)
        {
            gunCam.gameObject.SetActive(false);
            mainCam.gameObject.SetActive(false);
        }
        OnPlayerSwitchWeapons?.Invoke(this);
        player.OnPlayerRespawned += OnRespawn_Delegate;
    }

    private void Update()
    {
        if (_meleeCooldown > 0)
            _meleeCooldown -= Time.deltaTime;

        if (_meleeCooldown <= 0)
        {
            _meleeCooldown = 0;
            _isMeleeing = false;
        }



        if (!GetComponent<Player>().isDead && !GetComponent<Player>().isRespawning && !isSprinting && !pauseMenuOpen)
        {
            if (GameManager.instance.gameStarted)
            {
                Shooting();
                LeftShooting();
                CheckReloadButton();
                CheckAmmoForAutoReload();
                AnimationCheck();
            }
        }

        if (PV.IsMine)
        {
            StartButton();
            BackButton();

            if (!GameManager.instance.gameStarted)
                return;

            if (!GetComponent<Player>().isDead && !GetComponent<Player>().isRespawning)
            {
                if (!pauseMenuOpen)
                {
                    Sprint();
                    SwitchGrenades();
                    SwitchWeapons();
                    LongInteract();
                    if (isSprinting)
                        return;
                    ScopeIn();
                    Melee();
                    Crouch();
                    Grenade(); //TO DO: Spawn Grenades the same way as bullets
                    HolsterAndInspect();
                    CheckDrawingWeapon();
                }
            }

            TestButton();
            if (ReInput.controllers != null)
                activeControllerType = ReInput.controllers.GetLastActiveControllerType();
        }

    }
    void UpdateWeaponPropertiesAndAnimator()
    {
        if (!isDualWielding)
        {
            if (pInventory != null)
            {
                if (pInventory.activeWeapIs == 0)
                {
                    if (pInventory.weaponsEquiped[0] != null)
                    {
                        pInventory.activeWeapon = pInventory.weaponsEquiped[0].gameObject.GetComponent<WeaponProperties>();
                        weaponAnimator = pInventory.weaponsEquiped[0].gameObject.GetComponent<Animator>();
                    }
                }

                else if (pInventory.activeWeapIs == 1)
                {
                    if (pInventory.weaponsEquiped[1] != null)
                    {
                        pInventory.activeWeapon = pInventory.weaponsEquiped[1].gameObject.GetComponent<WeaponProperties>();
                        weaponAnimator = pInventory.weaponsEquiped[1].gameObject.GetComponent<Animator>();
                    }
                }
            }
        }

        if (isDualWielding)
        {
            dwRightWP = pInventory.rightWeapon.GetComponent<WeaponProperties>();
            dwLeftWP = pInventory.leftWeapon.GetComponent<WeaponProperties>();

            animDWRight = pInventory.rightWeapon.GetComponent<Animator>();
            animDWLeft = pInventory.leftWeapon.GetComponent<Animator>();
        }
    }

    void LongInteract()
    {
        if (rewiredPlayer.GetButtonShortPressDown("Interact"))
        {
            OnPlayerLongInteract?.Invoke(this);
        }
    }

    //TODO Make the player controller handle the third person script and models instead of the movement script
    void Sprint()
    {
        if (movement.direction == "Forward")
        {
            if (!movement.isGrounded || isReloading)
                return;
            if (activeControllerType == ControllerType.Keyboard || activeControllerType == ControllerType.Mouse)
            {
                if (rewiredPlayer.GetButton("Sprint"))
                    EnableSprint();
                else if (rewiredPlayer.GetButtonUp("Sprint"))
                    DisableSprint();
            }
            else if (activeControllerType == ControllerType.Joystick)
                if (rewiredPlayer.GetButtonDown("Sprint"))
                    EnableSprint();
        }
        else
            DisableSprint();
    }

    public void EnableSprint()
    {
        PV.RPC("EnableSprint_RPC", RpcTarget.All);
    }

    [PunRPC]
    void EnableSprint_RPC()
    {
        if (isSprinting)
            return;
        isSprinting = true;
        OnSprintStart?.Invoke(this);
        ScopeOut();
        weaponAnimator.SetBool("Run", true);
        GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Sprint", true);
        GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Idle Rifle", false);
        GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Idle Pistol", false);
        //GetComponent<Player>().playerVoice.volume = 0.1f;
        //GetComponent<Player>().PlaySprintingSound();
    }

    void DisableSprint()
    {
        PV.RPC("DisableSprint_RPC", RpcTarget.All);
    }

    [PunRPC]
    void DisableSprint_RPC()
    {
        if (!isSprinting)
            return;
        isSprinting = false;
        weaponAnimator.SetBool("Run", false);
        GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Sprint", false);

        if (pInventory.activeWeapon.GetComponent<WeaponProperties>().idleHandlingAnimationType == WeaponProperties.IdleHandlingAnimationType.Pistol)
        {
            GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Idle Pistol", true);
            GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Idle Rifle", false);
        }
        else
        {
            GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Idle Rifle", true);
            GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Idle Pistol", false);
        }

        GetComponent<Player>().StopPlayingPlayerVoice();
    }

    void _StartShoot()
    {
        if (PV.IsMine)
            PV.RPC("_StartShoot_RPC", RpcTarget.All);
    }
    void _StopShoot()
    {
        if (PV.IsMine)
            PV.RPC("_StopShoot_RPC", RpcTarget.All);
    }

    [PunRPC]
    void _StartShoot_RPC()
    {

        if (!pInventory.activeWeapon.isOutOfAmmo && !isReloading &&
            !isHoldingShootBtn && !isInspecting && !isMeleeing && !isThrowingGrenade)
        {
            isHoldingShootBtn = true;
        }
        else
        {
            isHoldingShootBtn = false;
        }
        Debug.Log($"{GetComponent<Player>().nickName}: _StartShoot_RPC {isHoldingShootBtn}");
    }

    [PunRPC]
    void _StopShoot_RPC()
    {
        isHoldingShootBtn = false;
        OnPlayerFireButtonUp?.Invoke(this);
        Debug.Log($"{GetComponent<Player>().nickName}: _StopShoot_RPC {isHoldingShootBtn}");
    }

    void Shooting()
    {
        if (GetComponent<Player>().isDead || isSprinting || player.isRespawning)
            return;



        if ((rewiredPlayer.GetButtonDown("Shoot") || rewiredPlayer.GetButton("Shoot")) && !isHoldingShootBtn)
        {
            _StartShoot();
        }

        if (isHoldingShootBtn)
            OnPlayerFire?.Invoke(this);

        if (rewiredPlayer.GetButtonUp("Shoot"))
        {
            _StopShoot();
        }
        return;

        if (rewiredPlayer.GetButtonUp("Shoot"))
        {
            OnPlayerFireButtonUp?.Invoke(this);
        }
        if (!isDualWielding)
        {
            if (rewiredPlayer.GetButton("Shoot") && !pInventory.activeWeapon.isOutOfAmmo && !isReloading && !isHoldingShootBtn && !isInspecting && !isMeleeing && !isThrowingGrenade)
            {
                isHoldingShootBtn = true;
                OnPlayerFire?.Invoke(this);

            }
            else
            {
                isHoldingShootBtn = false;
            }

        }

        if (isDualWielding)
        {
            if (rewiredPlayer.GetButton("Shoot") && !dwRightWP.isOutOfAmmo && !isReloadingRight && !isShootingRight && !isMeleeing && !isThrowingGrenade && !isSprinting)
            {
                isShootingRight = true;
            }
            else
            {
                isShootingRight = false;
            }
        }

        /*
        if (wProperties)
            if (wProperties.projectileToHide != null && wProperties.outOfAmmo)
                wProperties.projectileToHide.SetActive(false);*/
    }


    void LeftShooting()
    {
        if (!isDualWielding)
            return;
        if (GetComponent<Player>().isDead || isSprinting || player.isRespawning)
            return;



        if ((rewiredPlayer.GetButtonDown("Aim") || rewiredPlayer.GetButton("Aim")) && !isHoldingScopeBtn)
        {
            if (!pInventory.activeWeapon.leftWeapon.isOutOfAmmo && !isReloadingLeft &&
            !isHoldingScopeBtn && !isInspecting && !isMeleeing && !isThrowingGrenade)
            {
                isHoldingScopeBtn = true;
            }
            else
            {
                isHoldingScopeBtn = false;
            }
        }

        if (isHoldingScopeBtn)
            OnPlayerScopeBtnDown?.Invoke(this);

        if (rewiredPlayer.GetButtonUp("Aim"))
        {
            isHoldingScopeBtn = false;
            OnPlayerScopeBtnUp?.Invoke(this);
        }
    }

    void ScopeIn()
    {
        if (isDualWielding)
            return;

        if (isAiming)
        {
            pInventory.activeWeapon.currentRedReticuleRange = pInventory.activeWeapon.scopeRRR;
        }
        else
        {
            if (pInventory.activeWeapon)
                if (pInventory.activeWeapon.defaultRedReticuleRange > 0)
                {
                    pInventory.activeWeapon.currentRedReticuleRange = pInventory.activeWeapon.defaultRedReticuleRange;
                }
        }


        if (rewiredPlayer.GetButtonDown("Aim") && !isReloading && !isRunning && !isInspecting)
        {
            if (pInventory.activeWeapon.aimingMechanic != WeaponProperties.AimingMechanic.None)
            {
                if (isAiming == false)
                {
                    isAiming = true;
                    mainCam.fieldOfView = pInventory.activeWeapon.scopeFov;
                    uiCam.fieldOfView = pInventory.activeWeapon.scopeFov;
                    if (pInventory.activeWeapon.aimingMechanic == WeaponProperties.AimingMechanic.Scope)
                        gunCam.enabled = false;
                    else
                        gunCam.fieldOfView = pInventory.activeWeapon.scopeFov;

                    allPlayerScripts.aimingScript.playAimSound();
                }
                else
                {
                    isAiming = false;
                    mainCam.fieldOfView = GetComponent<Player>().defaultFov;
                    uiCam.fieldOfView = GetComponent<Player>().defaultFov;
                    camScript.mouseSensitivity = camScript.defaultMouseSensitivy;
                    gunCam.enabled = true;
                    gunCam.fieldOfView = 60;

                    allPlayerScripts.aimingScript.playAimSound();
                }
            }
        }
    }


    float zoomTime = 1f;
    void ScopeZoom()
    {
        float plusZoom = 0;
    }

    public void ScopeOut()
    {
        if (!isAiming && !GetComponent<Player>().isDead)
            return;
        Debug.Log("Unscope Script");
        isAiming = false;
        mainCam.fieldOfView = GetComponent<Player>().defaultFov;
        gunCam.fieldOfView = 60;
        uiCam.fieldOfView = GetComponent<Player>().defaultFov;
        camScript.mouseSensitivity = camScript.defaultMouseSensitivy;
        allPlayerScripts.aimingScript.playAimSound();

        mainCam.transform.localRotation = Quaternion.Euler(0, 0, 0);
        gunCam.enabled = true;
    }

    int _meleeCount = 0;
    float meleeMovementFactor = 0;
    void Melee()
    {
        if (!GetComponent<Player>().isDead)
        {
            if (rewiredPlayer.GetButtonDown("Melee") && !isMeleeing &&
                !isHoldingShootBtn && /*!isFiring &&*/ !isThrowingGrenade && !isSprinting)
            {
                isMeleeing = true;
                _meleeCount = melee.playersInMeleeZone.Count;
                Debug.Log("RPC Call: Melee");
                meleeMovementFactor = 1;

                rScript.reloadIsCanceled = true;

                PV.RPC("Melee_RPC", RpcTarget.All);
            }
        }

        if (meleeMovementFactor > 0)
        {
            if (_meleeCount > 0)
            {
                Vector3 move = transform.forward * meleeMovementFactor;
                GetComponent<CharacterController>().Move(move * movement.defaultSpeed * 6 * Time.deltaTime);
            }

            meleeMovementFactor -= Time.deltaTime * 5f;

            if (meleeMovementFactor <= 0)
            {
                _meleeCount = 0;
                meleeMovementFactor = 0;
            }
        }
    }


    [PunRPC]
    void Melee_RPC()
    {
        if (PV.IsMine)
            melee.Knife();
        weaponAnimator.Play("Knife Attack 2", 0, 0f);
        StartCoroutine(Melee3PS());
    }

    void Crouch()
    {
        if (rewiredPlayer.GetButtonDown("Crouch"))
            EnableCrouch();
        else if (rewiredPlayer.GetButtonUp("Crouch"))
            DisableCrouch();
    }

    void EnableCrouch()
    {
        Debug.Log("Crouching");
        OnCrouchDown?.Invoke(this);
        GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Jump", false);
        GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Crouch", true);
        isCrouching = true;
        mainCam.GetComponent<Transform>().localPosition += new Vector3(0, -.35f, 0);
        gwProperties.bulletSpawnPoint.localPosition += new Vector3(0, -.35f, 0);
        GetComponent<CharacterController>().height = 1.25f;
        GetComponent<CharacterController>().center = new Vector3(0, -0.7f, 0);
    }

    public void DisableCrouch()
    {
        OnCrouchUp?.Invoke(this);

        GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Crouch", false);
        isCrouching = false;
        mainCam.GetComponent<Transform>().localPosition += new Vector3(0, .35f, 0);
        gwProperties.bulletSpawnPoint.localPosition = gwProperties.defaultBulletSpawnPoint;

        GetComponent<CharacterController>().height = 1.7f; // Default values on the prefab
        GetComponent<CharacterController>().center = _originalCharacterControllerCenter; // Default values on the prefab 
    }

    void Grenade()
    {
        if (rewiredPlayer.GetButtonDown("Throw Grenade") && !isDualWielding &&
            !isHoldingShootBtn && /*!isFiring &&*/ !isMeleeing && !isSprinting /* && !isInspecting */)
        {
            if (pInventory.grenades > 0 && !isThrowingGrenade)
            {
                rScript.reloadIsCanceled = true;
                ScopeOut();
                pInventory.grenades = pInventory.grenades - 1;
                weaponAnimator.Play("GrenadeThrow", 0, 0.0f);
                StartCoroutine(GrenadeSpawnDelay());
                //PV.RPC("ThrowGrenade3PS_RPC", RpcTarget.All);
                OnPLayerThrewGrenade?.Invoke(this);
                //StartCoroutine(GrenadeSpawnDelay());
                //StartCoroutine(ThrowGrenade3PS());
            }
        }
    }

    void CheckReloadButton()
    {
        if (!GetComponent<Player>().isDead)
        {
            if (PV.IsMine && rewiredPlayer.GetButtonDown("Reload") && !isReloading && !isDualWielding)
            {
                PV.RPC("CheckRealodButton_RPC", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    void CheckRealodButton_RPC()
    {
        rScript.CheckAmmoTypeType(false);
    }

    void CheckAmmoForAutoReload()
    {
        if (!isDualWielding && !isDrawingWeapon && !isThrowingGrenade && !isMeleeing)
        {
            if (pInventory.activeWeapon)
            {
                if (pInventory.activeWeapon.currentAmmo <= 0)
                {
                    rScript.CheckAmmoTypeType(true);
                }
                else
                {
                }
            }
        }

        if (isDualWielding)
        {
            if (pInventory.activeWeapon.currentAmmo <= 0)
            {
                rScript.CheckAmmoTypeType(true);

            }

            if (pInventory.leftWeaponCurrentAmmo <= 0)
            {
                rScript.CheckAmmoTypeType(true, pInventory.leftWeapon);
            }
        }
    }

    void CheckDrawingWeapon()
    {
        if (weaponAnimator)
            if (weaponAnimator.GetCurrentAnimatorStateInfo(0).IsName("Draw"))
                isDrawingWeapon = true;
            else
                isDrawingWeapon = false;
    }

    void SwitchWeapons()
    {
        if (rewiredPlayer.GetButtonDown("Switch Weapons"))
        {
            weaponAnimator = pInventory.activeWeapon.GetComponent<Animator>();
            OnPlayerSwitchWeapons?.Invoke(this);
        }
    }
    void SwitchGrenades()
    {
        if (rewiredPlayer.GetButtonDown("Switch Grenades") && PV.IsMine)
        {
            grenadeSwitchAudioSource.Play();
            PV.RPC("SwitchGrenades_RPC", RpcTarget.All);
        }
    }

    [PunRPC]
    void SwitchGrenades_RPC()
    {
        if (fragGrenadesActive)
        {
            fragGrenadesActive = false;
            stickyGrenadesActive = true;

            GetComponent<PlayerUI>().fragGrenadeIcon.SetActive(false);
            GetComponent<PlayerUI>().stickyGrenadeIcon.SetActive(true);
        }
        else if (stickyGrenadesActive)
        {
            fragGrenadesActive = true;
            stickyGrenadesActive = false;

            GetComponent<PlayerUI>().fragGrenadeIcon.SetActive(true);
            GetComponent<PlayerUI>().stickyGrenadeIcon.SetActive(false);
        }
    }

    void HolsterAndInspect()
    {
        if (Input.GetKeyDown(KeyCode.Z) && !hasBeenHolstered)
        {
            holstered = true;

            //sfxManager.mainAudioSource.clip = weapSounds.holsterSound;
            //sfxManager.mainAudioSource.Play();

            hasBeenHolstered = true;
        }
        else if (Input.GetKeyDown(KeyCode.Z) && hasBeenHolstered)
        {
            holstered = false;

            //sfxManager.mainAudioSource.clip = weapSounds.drawWeaponSound;
            //sfxManager.mainAudioSource.Play();

            hasBeenHolstered = false;
        }

        //Holster anim toggle
        if (holstered == true)
        {
            if (weaponAnimator)
                weaponAnimator.SetBool("Holster", true);
        }
        else
        {
            if (weaponAnimator)
                weaponAnimator.SetBool("Holster", false);
        }


        //Inspect weapon when T key is pressed
        ///////////////////////////////////////
        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //    weaponAnimator.Play("Reload Open", 0, 0f);
        //    //anim.SetTrigger("Inspect");
        //}
    }

    private void AnimationCheck()
    {
        try
        {

            if (!isDualWielding)
            {
                if (pInventory.activeWeapon != null)
                {
                    if (weaponAnimator.GetCurrentAnimatorStateInfo(0).IsName("Fire"))
                    {
                        isFiring = true;
                    }
                    else
                    {

                        isFiring = false;
                    }

                    if (pInventory.activeWeapon.ammoReloadType == WeaponProperties.AmmoReloadType.Magazine)
                    {
                        //Check if reloading
                        //Check both animations
                        if (weaponAnimator.GetCurrentAnimatorStateInfo(0).IsName("Reload Out Of Ammo") ||
                            weaponAnimator.GetCurrentAnimatorStateInfo(0).IsName("Reload Ammo Left"))
                        {
                            isReloading = true;
                        }
                        else
                        {
                            isReloading = false;
                        }
                    }

                    if (pInventory.activeWeapon.ammoReloadType == WeaponProperties.AmmoReloadType.Shell)
                    {
                        //Check if reloading
                        //Check both animations
                        if (weaponAnimator.GetCurrentAnimatorStateInfo(0).IsName("Reload Open") ||
                            weaponAnimator.GetCurrentAnimatorStateInfo(0).IsName("Insert Shell") ||
                            weaponAnimator.GetCurrentAnimatorStateInfo(0).IsName("Reload Close"))
                        {
                            isReloading = true;
                        }
                        else
                        {
                            isReloading = false;
                        }

                        //Check if inspecting weapon
                        if (weaponAnimator.GetCurrentAnimatorStateInfo(0).IsName("Inspect"))
                        {
                            isInspecting = true;
                        }
                        else
                        {
                            isInspecting = false;
                        }
                    }

                    if (pInventory.activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Rocket /* || pInventory.activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Grenade */)
                    {
                        if (weaponAnimator.GetCurrentAnimatorStateInfo(0).IsName("Reload"))
                        {
                            isReloading = true;

                            /*
                            if (wProperties.projectileToHide != null)
                            {
                                wProperties.projectileToHide.SetActive(true);
                            }*/
                        }

                        else
                        {
                            isReloading = false;
                        }
                    }
                }
            }

            //if (isDualWielding)
            //{
            //    if (dwRightWP.usesMags)
            //    {
            //        //Check if reloading
            //        //Check both animations
            //        if (animDWRight.GetCurrentAnimatorStateInfo(0).IsName("Reload Out Of Ammo") ||
            //            animDWRight.GetCurrentAnimatorStateInfo(0).IsName("Reload Ammo Left") ||
            //            animDWRight.GetCurrentAnimatorStateInfo(0).IsName("Reload Ammo Left Take Out"))
            //        {
            //            isReloadingRight = true;
            //        }
            //        else
            //        {
            //            isReloadingRight = false;
            //        }
            //    }

            //    if (dwLeftWP.usesMags)
            //    {
            //        //Check if reloading
            //        //Check both animations
            //        if (animDWLeft.GetCurrentAnimatorStateInfo(0).IsName("Reload Out Of Ammo") ||
            //            animDWLeft.GetCurrentAnimatorStateInfo(0).IsName("Reload Ammo Left") ||
            //            animDWLeft.GetCurrentAnimatorStateInfo(0).IsName("Reload Ammo Left Take Out"))
            //        {
            //            isReloadingLeft = true;
            //        }
            //        else
            //        {
            //            isReloadingLeft = false;
            //        }
            //    }
            //}

            if (weaponAnimator != null)
            {
                if (weaponAnimator.GetCurrentAnimatorStateInfo(0).IsName("GrenadeThrow"))
                {
                    isThrowingGrenade = true;
                }
                else
                {
                    isThrowingGrenade = false;
                }
            }

            //if (weaponAnimator != null)
            //{
            //    if (weaponAnimator.GetCurrentAnimatorStateInfo(0).IsName("Knife Attack 2"))
            //    {
            //        isMeleeing = true;
            //    }
            //    else
            //    {
            //        isMeleeing = false;
            //    }
            //}
        }
        catch { }
    }

    IEnumerator ThrowGrenade3PS()
    {
        GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().Play("Throw Grenade");
        yield return new WaitForEndOfFrame();
    }

    public void Player3PReloadAnimation()
    {
        if (PV.IsMine)
            PV.RPC("Player3PReloadAnimation_RPC", RpcTarget.All);
    }

    [PunRPC]
    void Player3PReloadAnimation_RPC()
    {
        StartCoroutine(Reload3PS());
    }

    public IEnumerator Reload3PS()
    {
        GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().Play("Reload");
        GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetTrigger("Reload 0");
        yield return new WaitForEndOfFrame();
    }

    IEnumerator Melee3PS()
    {
        GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().Play("Melee");
        //StartCoroutine(ShowMeleeKnife());
        yield return new WaitForEndOfFrame();
    }

    IEnumerator ShowMeleeKnife()
    {
        melee.knifeGameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        melee.knifeGameObject.SetActive(false);
    }

























    /// <summary>
    /// /////////////////////////////////////////////////////////////////////////////////// Coroutines ////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    /// <returns></returns>
    /// 

    [PunRPC]
    public void ThrowGrenade3PS_RPC()
    {
        StartCoroutine(ThrowGrenade3PS());
    }

    private IEnumerator GrenadeSpawnDelay()
    {
        GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().Play("Throw Grenade");

        //Wait for set amount of time before spawning grenade
        yield return new WaitForSeconds(pInventory.grenadeSpawnDelay);
        //Spawn grenade prefab at spawnpoint

        PV.RPC("SpawnGrenade_RPC", RpcTarget.All, gwProperties.grenadeSpawnPoint.position,
            gwProperties.grenadeSpawnPoint.rotation, gwProperties.grenadeSpawnPoint.forward);

        //var grenade = Instantiate(pInventory.grenadePrefab);
        //Destroy(grenade.gameObject);

        //if (fragGrenadesActive)
        //{
        //    grenade = Instantiate(pInventory.grenadePrefab,
        //       gwProperties.grenadeSpawnPoint.transform.position,
        //       gwProperties.grenadeSpawnPoint.transform.rotation);
        //    grenade.GetComponent<ExplosiveProjectile>().player = GetComponent<Player>();
        //    //grenade.GetComponent<FragGrenade>().team = allPlayerScripts.playerMPProperties.team;
        //}
        //else if (stickyGrenadesActive)
        //{
        //    grenade = Instantiate(pInventory.stickyGrenadePrefab,
        //       gwProperties.grenadeSpawnPoint.transform.position,
        //       gwProperties.grenadeSpawnPoint.transform.rotation);
        //    grenade.GetComponent<ExplosiveProjectile>().player = GetComponent<Player>();
        //    //grenade.GetComponent<StickyGrenade>().team = allPlayerScripts.playerMPProperties.team;
        //}

        //foreach (PlayerHitbox hb in GetComponent<Player>().hitboxes)
        //    Physics.IgnoreCollision(grenade.GetComponent<Collider>(), hb.GetComponent<Collider>()); // Prevents the grenade from colliding with the player who threw it

        //grenade.GetComponent<Rigidbody>().AddForce(gwProperties.grenadeSpawnPoint.transform.forward * grenadeThrowForce);
        //Destroy(grenade.gameObject, 10);
    }
    public void TransferAmmo()
    {
        if (pInventory.activeWeapon.ammoReloadType == WeaponProperties.AmmoReloadType.Shell)
        {
            pInventory.activeWeapon.spareAmmo -= 1;
            pInventory.activeWeapon.currentAmmo += 1;
        }
        else
        {
            ammoWeaponIsMissing = pInventory.activeWeapon.ammoCapacity - pInventory.activeWeapon.currentAmmo;

            if (pInventory.activeWeapon.spareAmmo >= ammoWeaponIsMissing)
            {
                pInventory.activeWeapon.currentAmmo = pInventory.activeWeapon.ammoCapacity;
                pInventory.activeWeapon.spareAmmo -= ammoWeaponIsMissing;
            }
            else if (pInventory.activeWeapon.spareAmmo < ammoWeaponIsMissing)
            {
                pInventory.activeWeapon.currentAmmo += pInventory.activeWeapon.spareAmmo;
                pInventory.activeWeapon.spareAmmo = 0;
            }

            if (pInventory.leftWeapon)
            {
                ammoWeaponIsMissing = pInventory.leftWeapon.ammoCapacity - pInventory.leftWeapon.currentAmmo;

                if (pInventory.leftWeapon.spareAmmo >= ammoWeaponIsMissing)
                {
                    pInventory.leftWeapon.currentAmmo = pInventory.leftWeapon.ammoCapacity;
                    pInventory.leftWeapon.spareAmmo -= ammoWeaponIsMissing;
                }
                else if (pInventory.leftWeapon.spareAmmo < ammoWeaponIsMissing)
                {
                    pInventory.leftWeapon.currentAmmo += pInventory.leftWeapon.spareAmmo;
                    pInventory.leftWeapon.spareAmmo = 0;
                }
            }
        }
    }

    void TestButton()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            OnPlayerTestButton?.Invoke(this);
        }
    }
    void StartButton()
    {
        if (rewiredPlayer.GetButtonDown("Start") || rewiredPlayer.GetButtonDown("Escape"))
        {
            Debug.Log($"Pausing game");
            TogglePauseGame();
        }
    }

    void BackButton()
    {
        if (rewiredPlayer.GetButtonDown("Back"))
        {
            allPlayerScripts.scoreboardManager.OpenScoreboard();
        }
        else if (rewiredPlayer.GetButtonUp("Back"))
        {
            allPlayerScripts.scoreboardManager.CloseScoreboard();
        }
    }

    public void TogglePauseGame()
    {
        Debug.Log("Toggling Pause Menu");
        if (!pauseMenuOpen)
        {
            Cursor.lockState = CursorLockMode.None; // Must Unlock Cursor so it can detect buttons
            Cursor.visible = true;
            GetComponent<PlayerUI>().singlePlayerPauseMenu.gameObject.SetActive(true);
            pauseMenuOpen = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked; // Must Unlock Cursor so it can detect buttons
            Cursor.visible = false;
            GetComponent<PlayerUI>().singlePlayerPauseMenu.gameObject.SetActive(false);
            pauseMenuOpen = false;
        }
    }

    public void QuitMatch()
    {
        Debug.Log("Returning to Main Menu");

        if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
            FindObjectOfType<MultiplayerManager>().EndGame(false);
        if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
            FindObjectOfType<SwarmManager>().EndGame(false);
    }

    public void SetPlayerIDInInput()
    {
        rewiredPlayer = ReInput.players.GetPlayer(rid);
    }

    void OnTestButton_Delegate(PlayerController playerController)
    {
        player.BasicDamage(50);
    }
    public void OnDeath_Delegate(Player player)
    {
        isSprinting = false;
        isHoldingShootBtn = false;
    }

    void OnRespawn_Delegate(Player player)
    {
        isSprinting = false;
        isHoldingShootBtn = false;
    }











    [PunRPC]
    void SpawnGrenade_RPC(Vector3 sp, Quaternion sr, Vector3 forw)
    {
        var grenade = Instantiate(pInventory.grenadePrefab);
        Destroy(grenade.gameObject);

        if (fragGrenadesActive)
        {
            grenade = Instantiate(pInventory.grenadePrefab,
               sp,
               sr);
            grenade.GetComponent<ExplosiveProjectile>().player = GetComponent<Player>();
            //grenade.GetComponent<FragGrenade>().team = allPlayerScripts.playerMPProperties.team;
        }
        else if (stickyGrenadesActive)
        {
            grenade = Instantiate(pInventory.stickyGrenadePrefab,
               sp,
               sr);
            grenade.GetComponent<ExplosiveProjectile>().player = GetComponent<Player>();
            //grenade.GetComponent<StickyGrenade>().team = allPlayerScripts.playerMPProperties.team;
        }

        foreach (PlayerHitbox hb in GetComponent<Player>().hitboxes)
            Physics.IgnoreCollision(grenade.GetComponent<Collider>(), hb.GetComponent<Collider>()); // Prevents the grenade from colliding with the player who threw it

        grenade.GetComponent<Rigidbody>().AddForce(forw * grenadeThrowForce);
        Destroy(grenade.gameObject, 10);
    }
}


