using System.Collections;
using UnityEngine;
using Rewired;
using Photon.Pun;
using System;
using Photon.Realtime;
using Steamworks;
using System.Linq;

public class PlayerController : MonoBehaviourPun
{
    public static float WEAPON_DRAW_TIME = 0.65f;




    // Events
    public delegate void PlayerControllerEvent(PlayerController playerController);
    public PlayerControllerEvent OnPlayerSwitchWeapons, OnPlayerLongInteract, OnPlayerShortPressInteract,
        OnPlayerFire, OnPlayerFireButtonUp, OnPlayerTestButton, OnPLayerThrewGrenade,
        OnCrouchUp, OnCrouchDown, OnSprintStart, OnSprintStop, OnPlayerDeath,
        OnControllerTypeChangedToController, OnControllerTypeChangedToMouseAndKeyboard,
        OnPlayerScopeBtnDown, OnDualWieldedWeaponFireBtnUp;

    public Player player { get { return GetComponent<Player>(); } }
    public int rid
    {
        get { return _rid; }
        set
        {
            _rid = value;
            player.playerUI.gamepadCursor.rewiredPlayer = ReInput.players.GetPlayer(_rid);
            player.playerUI.gamepadCursor.player = player;

            player.movement.playerMotionTracker.minimapCamera.GetComponent<MotionTrackerCamera>().ChooseRenderTexture(value);
            rewiredPlayer = ReInput.players.GetPlayer(rid);
            print($"PlayerController rid: {_rid}");
        }
    }



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
    [SerializeField] int _rid;
    public CrosshairManager crosshairScript;
    public ReloadScript rScript;
    public PlayerWeaponSwapping wPickup;
    public DualWieldingReload dwReload;
    public PlayerMovement movement;
    public Melee melee;
    public FloatingCamera floatingCamera;
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
    public bool holstered, isRunning, isWalking;
    [HideInInspector]
    public bool isInspecting, aimSoundHasPlayed = false;

    public bool isReloading { get { return _currentlyReloadingTimer > 0; } }

    public bool reloadAnimationStarted, reloadWasCanceled, isFiring,
        isAiming, isCrouching, isDrawingWeapon, isSprinting;
    bool isDrawingThirdWeapon { get { return _drawingWeaponTime_thirdWeapon > 0; } }
    public bool isThrowingGrenade { get { return _currentlyThrowingGrenadeTimer > 0; } }

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
    public GameObject aimingComponentsPivot;

    [Header("Dual Wielding")]
    public Animator animDWRight;
    public Animator animDWLeft;
    public WeaponProperties dwRightWP;
    public WeaponProperties dwLeftWP;
    public bool isHoldingShootBtn
    {
        get { return _isHoldingShootBtn; }
        set
        {
            _preIsHoldingFireWeaponBtn = _isHoldingShootBtn;
            _isHoldingShootBtn = value; print($"{player.name} isHoldingShootBtn. {_preIsHoldingFireWeaponBtn} -> {value}");


            if (player.isAlive)
                if (value && !_preIsHoldingFireWeaponBtn)
                {

                    DisableSprint_RPC();
                    print($"{player.name} isHoldingShootBtn. {pInventory.activeWeapon.killFeedOutput} {_swordRecovery}");

                    if ((pInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Oddball
                        || pInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Sword
                        || pInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Flag)
                        && _swordRecovery <= 0 && _drawingWeaponTime <= 0)
                    {

                        if (player.isMine)
                        {
                            _swordRecovery = 1f / (pInventory.activeWeapon.fireRate / 60f);
                            Melee(true);
                        }
                        print("Do an Odball Melee and STOP");
                        return;
                    }
                }
        }
    }

    public bool isHoldingShootDualWieldedWeapon
    {
        get { return _isHoldingShootDualWieldedWeapon; }
        private set
        {
            _isHoldingShootDualWieldedWeapon = value;

            if (value == false)
                OnDualWieldedWeaponFireBtnUp?.Invoke(this);
        }
    }

    public bool isCurrentlyShootingForMotionTracker { get { return _isCurrentlyShootingReset > 0; } }



    public bool isHoldingScopeBtn;
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


    [SerializeField] LayerMask floatinCameraLayerMask;




    PlayerThirdPersonModelManager _playerThirdPersonModelManager;

    public PlayerThirdPersonModelManager playerThirdPersonModelManager { get { return _playerThirdPersonModelManager; } }
    public bool isMeleeing { get { return _isMeleeing; } set { _isMeleeing = value; if (value) _meleeCooldown = 0.8f; } }
    public bool cameraIsFloating { get { return _cameraIsFloating; } }
    float currentadsCounter
    {
        get { return _adsCounter; }
        set
        {
            //_currentRotationFix = Mathf.RoundToInt(value / 10) * 10;

            _adsCounter = Mathf.Clamp(value, 0, 1);

            {
                mainCam.fieldOfView = player.defaultVerticalFov - (Mathf.Abs(player.defaultVerticalFov - _tempFov) * _adsCounter);
                uiCam.fieldOfView = player.defaultVerticalFov - (Mathf.Abs(player.defaultVerticalFov - _tempFov) * _adsCounter);
                gunCam.fieldOfView = 60 - (10 * _adsCounter);
            }
        }
    }

    public float currentlyReloadingTimer { set { _currentlyReloadingTimer = value; } }
    public bool triggerOverHeatOnShootingBtnUp { set { _triggerOverHeatOnShootingBtnUp = value; } }
    public bool isReloadingThirWeaponAnimation { get { return _currentlyReloadingTimer_thirdWeapon > 0; } }


    bool _isMeleeing, _cameraIsFloating, _triggerOverHeatOnShootingBtnUp;
    float _meleeCooldown, _adsCounter;
    Transform _mainCamParent;
    Vector3 _lastMainCamLocalPos;
    Quaternion _lasMainCamLocalQuat;
    LayerMask _lastMainCamLayerMask;


    [SerializeField] bool _isHoldingShootBtn, _preIsHoldingFireWeaponBtn, _isHoldingSprintBtn, _isHoldingShootDualWieldedWeapon;
    [SerializeField]
    float _currentlyReloadingTimer, _completeReloadTimer, _currentlyThrowingGrenadeTimer, _isCurrentlyShootingReset,
        _drawingWeaponTime, _isCurrentlyShootingReset_thirdWeapon, _drawingWeaponTime_thirdWeapon, _swordRecovery;

    void Awake()
    {
        _rid = -99999;
        PV = GetComponent<PhotonView>();
        _mainCamParent = mainCam.transform.parent;
        _playerThirdPersonModelManager = GetComponent<PlayerThirdPersonModelManager>();
        OnPlayerTestButton += OnTestButton_Delegate;
    }
    public void Start()
    {

        if (!PV.IsMine)
        {
            gunCam.gameObject.SetActive(false);
            mainCam.gameObject.SetActive(false);
        }
        OnPlayerSwitchWeapons?.Invoke(this);
        player.OnPlayerRespawned += OnRespawn_Delegate;


        if (GameManager.instance.gameType == GameManager.GameType.Martian) SwitchGrenades_RPC();
    }

    private void Update()
    {
        if (_drawingWeaponTime > 0) _drawingWeaponTime -= Time.deltaTime; if (_drawingWeaponTime_thirdWeapon > 0) _drawingWeaponTime_thirdWeapon -= Time.deltaTime;
        if (_isCurrentlyShootingReset > 0) _isCurrentlyShootingReset -= Time.deltaTime; if (_isCurrentlyShootingReset_thirdWeapon > 0) _isCurrentlyShootingReset_thirdWeapon -= Time.deltaTime;
        if (_meleeCooldown > 0) _meleeCooldown -= Time.deltaTime;
        if (_currentlyReloadingTimer > 0) _currentlyReloadingTimer -= Time.deltaTime; if (_currentlyReloadingTimer_thirdWeapon > 0) _currentlyReloadingTimer_thirdWeapon -= Time.deltaTime;
        if (_currentlyThrowingGrenadeTimer > 0) _currentlyThrowingGrenadeTimer -= Time.deltaTime;
        if (_swordRecovery > 0) _swordRecovery -= Time.deltaTime;

        isReloadingRight = _currentlyReloadingTimer > 0; isReloadingLeft = _currentlyReloadingTimer_thirdWeapon > 0;

        //if (GameManager.instance.devMode)
        //{
        //    if (Input.GetKeyDown(KeyCode.Alpha9) && GameManager.instance.gameType == GameManager.GameType.GunGame)
        //        pInventory.playerGunGameManager.index++;
        //}




        if (_meleeCooldown <= 0)
        {
            _meleeCooldown = 0;
            _isMeleeing = false;
        }

        if (_completeReloadTimer > 0)
        {
            _completeReloadTimer -= Time.deltaTime;

            if (_completeReloadTimer <= 0)
            {
                TransferAmmo(player.playerInventory.activeWeapon);
                player.playerInventory.UpdateAllExtraAmmoHuds();
            }
        }

        if (_completeReloadTimer_thirdWeapon > 0)
        {
            _completeReloadTimer_thirdWeapon -= Time.deltaTime;

            if (_completeReloadTimer_thirdWeapon <= 0)
            {
                // transfer third weapon ammo
                TransferAmmo(player.playerInventory.thirdWeapon);
            }
        }




        try { weaponAnimator = pInventory.activeWeapon.GetComponent<Animator>(); } catch { }

        if (!GetComponent<Player>().isDead && !GetComponent<Player>().isRespawning && !pauseMenuOpen)
        {
            _disableSprintRPCCooldown -= Time.deltaTime;

            if (GameManager.instance.gameStarted)
            {
                Shooting();
                LeftShooting();
                AutoReloadThirdWeapon();

                if (!isSprinting)
                {
                    CheckReloadButton();
                    AutoReload();
                    AnimationCheck();
                }
            }
        }
        if (player.isMine)
        {
            if (rewiredPlayer.GetButtonUp("Shoot"))
                SendIsNotHoldingFireWeaponBtn();

            if (rewiredPlayer.GetButtonUp("Aim") && activeControllerType != ControllerType.Joystick) //  for dual wielding
            {
                SendIsNotHoldingDualWieldedFireWeaponBtn();
            }
            else if (rewiredPlayer.GetButtonUp("Throw Grenade") && activeControllerType == ControllerType.Joystick) // for dual wielding
            {
                SendIsNotHoldingDualWieldedFireWeaponBtn();
            }
        }





        if (PV.IsMine)
        {
            if (GameManager.instance.gameStarted)
            {
                StartButton();
                BackButton();
            }

            if (!cameraIsFloating)
            {
                if (isAiming)
                    currentadsCounter += Time.deltaTime * 6;
                else
                    currentadsCounter -= Time.deltaTime * 6;
            }

            if (!GameManager.instance.gameStarted)
                return;

            if (!GetComponent<Player>().isDead && !GetComponent<Player>().isRespawning)
            {
                if (!pauseMenuOpen)
                {
                    ToggleInvincible();
                    Sprint();
                    SwitchGrenades();
                    SwitchWeapons();
                    LongInteract();
                    MarkSpot();
                    GrenadeBtn();
                    Melee();
                    if (isSprinting)
                        return;
                    Scope();
                    Crouch();
                    HolsterAndInspect();
                    CheckDrawingWeapon();
                    FloatingCamera();
                }
            }

            TestButton();
            if (ReInput.controllers != null)
                activeControllerType = ReInput.controllers.GetLastActiveControllerType();
        }

    }

    void ToggleInvincible()
    {
        //if (Input.GetKeyDown(KeyCode.Alpha9) && GameManager.instance.gameMode == GameManager.GameMode.Coop)
        //{
        //    player.isInvincible = !player.isInvincible;
        //    GetComponent<PlayerUI>().invincibleIcon.SetActive(player.isInvincible);
        //}
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


    int _framesInteractBtnHasBeenHeld;
    void LongInteract()
    {
        if (player.playerInventory.holdingObjective) _framesInteractBtnHasBeenHeld = -1;
        else
        {
            if (rewiredPlayer.GetButton("Interact"))
            {
                if (_framesInteractBtnHasBeenHeld < GameManager.DEFAULT_FRAMERATE / 3 && _framesInteractBtnHasBeenHeld > -1)
                    _framesInteractBtnHasBeenHeld = Mathf.Clamp(_framesInteractBtnHasBeenHeld + 1, 0, GameManager.DEFAULT_FRAMERATE / 3);
            }


            if (_framesInteractBtnHasBeenHeld == GameManager.DEFAULT_FRAMERATE / 3)
                OnPlayerShortPressInteract?.Invoke(this);
        }

        if (rewiredPlayer.GetButtonUp("Interact"))
        {
            _framesInteractBtnHasBeenHeld = 0;
        }





        //if (GameManager.instance.gameType == GameManager.GameType.Oddball && player.playerInventory.playerOddballActive) return;

        //if (rewiredPlayer.GetButtonShortPressDown("Interact"))
        //{
        //    //OnPlayerLongInteract?.Invoke(this);
        //    OnPlayerShortPressInteract?.Invoke(this);
        //}
    }


    public enum InteractResetMode { activeweapon, thirdweapon }
    public void ResetLongInteractFrameCounter(InteractResetMode irm)
    {
        if (irm == InteractResetMode.activeweapon)
            _framesInteractBtnHasBeenHeld = -999;
        else if (irm == InteractResetMode.thirdweapon)
            _framesMarkSpotHasBeenHeld = -999;
    }



    //TODO Make the player controller handle the third person script and models instead of the movement script
    void Sprint()
    {
        if (GameManager.instance.sprintMode == GameManager.SprintMode.Off) return;


        if (isHoldingShootBtn || player.hasEnnemyFlag || player.playerInventory.playerOddballActive) return;


        if (isSprinting)
        {
            currentlyReloadingTimer = 0;
            weaponAnimator.SetBool("Run", true);

            //_playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Rifle Sprint", false);
            //_playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Pistol Sprint", false);
            //_playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("sword sprint", false);

            //if (pInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Sword)
            //    _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("sword sprint", true);
            //else if (pInventory.activeWeapon.weaponType != WeaponProperties.WeaponType.Pistol)
            //    _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Rifle Sprint", true);
            //else
            //    _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Pistol Sprint", true);

            //_playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Idle Rifle", true);
            //_playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Idle Pistol", true);
        }


        if (movement.movementDirection == PlayerMovement.PlayerMovementDirection.Forward)
        {
            if (!movement.isGrounded || isReloading)
                return;
            if (activeControllerType == ControllerType.Keyboard || activeControllerType == ControllerType.Mouse)
            {
                if (rewiredPlayer.GetButton("Sprint") && !_isHoldingSprintBtn)
                {
                    _isHoldingSprintBtn = true;
                    EnableSprint();
                }
                else if (rewiredPlayer.GetButtonUp("Sprint"))
                {
                    _isHoldingSprintBtn = false;
                    DisableSprint();
                }
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
        if (player.playerInventory.isDualWielding) player.playerInventory.DropThirdWeapon();

        if (player.movement.blockPlayerMoveInput <= 0)
        {
            PV.RPC("EnableSprint_RPC", RpcTarget.All);
        }
    }

    [PunRPC]
    void EnableSprint_RPC()
    {
        if (isSprinting)
            return;
        isSprinting = true;
        OnSprintStart?.Invoke(this);
        Descope();
        weaponAnimator.SetBool("Run", true);






        //new 

        print($"EnableSprint_RPC {pInventory.activeWeapon.killFeedOutput} {pInventory.activeWeapon.weaponType}");


        _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("sword sprint", pInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Sword || player.hasEnnemyFlag);
        _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Rifle Sprint", pInventory.activeWeapon.weaponType != WeaponProperties.WeaponType.Pistol);
        _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Pistol Sprint", pInventory.activeWeapon.weaponType == WeaponProperties.WeaponType.Pistol && pInventory.activeWeapon.killFeedOutput != WeaponProperties.KillFeedOutput.Sword);



        //_playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Rifle Sprint", false);
        //_playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Pistol Sprint", false);
        //_playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("sword sprint", false);

        _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Idle Rifle", false);
        _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Idle Pistol", false);
        _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("sword idle", false);

        //







        //_playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Rifle Sprint", false);
        //_playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Pistol Sprint", false);


        //if (pInventory.activeWeapon.weaponType != WeaponProperties.WeaponType.Pistol)
        //    _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Rifle Sprint", true);
        //else
        //    _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Pistol Sprint", true);

        //_playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Idle Rifle", false);
        //_playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Idle Pistol", false);
        //GetComponent<Player>().playerVoice.volume = 0.1f;
        //GetComponent<Player>().PlaySprintingSound();
    }

    float _disableSprintRPCCooldown;

    public void DisableSprint()
    {
        if (_disableSprintRPCCooldown < 0)
        {
            _disableSprintRPCCooldown = 0.1f;

            if (GameManager.instance.connection == GameManager.Connection.Online) PV.RPC("DisableSprint_RPC", RpcTarget.All);
            else DisableSprint_RPC();
        }
    }

    [PunRPC]
    void DisableSprint_RPC()
    {
        if (!isSprinting)
            return;
        print("DisableSprint_RPC");
        isSprinting = false;
        weaponAnimator.SetBool("Run", false);

        GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Rifle Sprint", false);
        GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Pistol Sprint", false);
        GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("sword sprint", false);

        _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Idle Rifle", false);
        _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Idle Pistol", false);
        _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("sword idle", false);


        if (pInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Sword || player.hasEnnemyFlag)
        {
            _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("sword idle", true);
        }
        else if (pInventory.activeWeapon.GetComponent<WeaponProperties>().idleHandlingAnimationType == WeaponProperties.IdleHandlingAnimationType.Pistol)
        {
            GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Idle Pistol", true);
            //GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Idle Rifle", false);
        }
        else
        {
            GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Idle Rifle", true);
            //GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Idle Pistol", false);
        }

        GetComponent<Player>().StopPlayingPlayerVoice();
    }











    public void SetCurrentlyShootingReset()
    {
        _isCurrentlyShootingReset = 0.6f;
    }

    void Shooting() //  ***************
    {
        if (!GetComponent<Player>().isDead && !player.isRespawning)
        {
            if (player.isMine)
            {
                if ((rewiredPlayer.GetButtonDown("Shoot") || rewiredPlayer.GetButton("Shoot")) && !isHoldingShootBtn)
                {
                    SendIsHoldingFireWeaponBtn(true, (player.aimAssist.targetHitbox != null) ? player.aimAssist.targetHitbox.GetComponent<Hitbox>().biped.originalSpawnPosition : Vector3.zero, false);
                }
                else if (player.playerInventory.activeWeapon.loadedAmmo > 0 && player.playerInventory.activeWeapon.targetTracking && player.playerShooting.fireRecovery <= 0 && isHoldingShootBtn)
                    player.playerShooting.trackingTarget = (player.aimAssist.targetHitbox != null) ? GameManager.instance.instantiation_position_Biped_Dict[player.aimAssist.targetHitbox.GetComponent<Hitbox>().biped.originalSpawnPosition] : null;
            }



            if (!player.isDualWielding)
            {

                //Process Firing
                if (pInventory.activeWeapon.killFeedOutput != WeaponProperties.KillFeedOutput.Oddball && pInventory.activeWeapon.killFeedOutput != WeaponProperties.KillFeedOutput.Sword)
                    if (!isReloading && !isThrowingGrenade && !isMeleeing)
                    {
                        if (player.playerShooting && player.playerInventory)
                            if (player.playerShooting.fireRecovery <= 0 && player.playerInventory.activeWeapon.loadedAmmo > 0 && isHoldingShootBtn)
                            {

                                if (player.playerInventory.activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Plasma &&
                                    player.playerInventory.activeWeapon.plasmaColor != WeaponProperties.PlasmaColor.Shard)
                                {
                                    if (player.playerInventory.activeWeapon.overheatCooldown <= 0)
                                    {
                                        player.playerShooting.Shoot(player.playerInventory.activeWeapon);
                                    }
                                }
                                else
                                {
                                    player.playerShooting.Shoot(player.playerInventory.activeWeapon);

                                }
                            }
                    }
            }
            else
            {
                if (!isReloadingRight && !isThrowingGrenade && !isMeleeing)
                {
                    if (player.playerShooting && player.playerInventory)
                        if (player.playerShooting.fireRecovery <= 0 && player.playerInventory.activeWeapon.loadedAmmo > 0 && isHoldingShootBtn)
                        {

                            if (player.playerInventory.activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Plasma &&
                                player.playerInventory.activeWeapon.plasmaColor != WeaponProperties.PlasmaColor.Shard)
                            {
                                if (player.playerInventory.activeWeapon.overheatCooldown <= 0)
                                {
                                    player.playerShooting.Shoot(player.playerInventory.activeWeapon);
                                }
                            }
                            else
                            {
                                player.playerShooting.Shoot(player.playerInventory.activeWeapon);

                            }
                        }
                }
            }
        }
    }

    void LeftShooting()
    {

        //if (player.isDualWielding)
        //{
        //    if (player.playerInventory.thirdWeapon.loadedAmmo > 0 && player.playerInventory.thirdWeapon.targetTracking && player.playerShooting.revo <= 0 && isHoldingShootBtn)
        //        player.playerShooting.trackingTarget = (player.aimAssist.targetHitbox != null) ? GameManager.instance.instantiation_position_Biped_Dict[player.aimAssist.targetHitbox.GetComponent<Hitbox>().biped.originalSpawnPosition] : null;
        //}



        if (PV.IsMine && player.isDualWielding && !isHoldingShootDualWieldedWeapon)
            if (activeControllerType == ControllerType.Joystick)
            {
                if (rewiredPlayer.GetButton("Throw Grenade")) SendIsHoldingFireWeaponBtn(true, (player.aimAssist.targetHitbox != null) ? player.aimAssist.targetHitbox.GetComponent<Hitbox>().biped.originalSpawnPosition : Vector3.zero, true);
            }
            else
            {
                if (rewiredPlayer.GetButton("Aim")) SendIsHoldingFireWeaponBtn(true, (player.aimAssist.targetHitbox != null) ? player.aimAssist.targetHitbox.GetComponent<Hitbox>().biped.originalSpawnPosition : Vector3.zero, true);
            }





        if (pInventory.thirdWeapon && player.isAlive /*&& _isCurrentlyShootingReset_thirdWeapon <= 0*/ && pInventory.thirdWeapon.loadedAmmo > 0 && !isDrawingThirdWeapon && !isReloadingLeft)
        {
            if (isHoldingShootDualWieldedWeapon)
            {
                //_isCurrentlyShootingReset_thirdWeapon = 60f / player.playerInventory.thirdWeapon.fireRate;

                player.playerShooting.Shoot(pInventory.thirdWeapon);
            }




            //if (activeControllerType == ControllerType.Joystick)
            //{
            //    if (isHoldingShootDualWieldedWeapon)
            //    {
            //        _isCurrentlyShootingReset_thirdWeapon = 60f / player.playerInventory.thirdWeapon.fireRate;

            //        player.playerShooting.Shoot(pInventory.thirdWeapon);
            //    }
            //}
            //else
            //{
            //    if (isHoldingShootDualWieldedWeapon)
            //    {
            //        _isCurrentlyShootingReset_thirdWeapon = 60f / player.playerInventory.thirdWeapon.fireRate;

            //        player.playerShooting.Shoot(pInventory.thirdWeapon);
            //    }
            //}
        }
    }







    void _StartShoot()
    {
        print($"Player {player.name} is about to call _StartShoot_RPC");
        if (PV.IsMine)
        {
            try { Debug.Log("_StartShoot"); } catch (System.Exception e) { Debug.LogWarning(e); }
            try { Debug.Log(player.aimAssist); } catch (System.Exception e) { Debug.LogWarning(e); }
            try { Debug.Log(player.aimAssist.targetHitbox); } catch (System.Exception e) { Debug.LogWarning(e); }
            try { Debug.Log(player.aimAssist.targetHitbox.GetComponent<Hitbox>().biped); } catch (System.Exception e) { Debug.LogWarning(e); }
            try { Debug.Log(player.aimAssist.targetHitbox.GetComponent<Hitbox>().biped.originalSpawnPosition); } catch (System.Exception e) { Debug.LogWarning(e); }



            if (player.aimAssist.targetHitbox) PV.RPC("_StartShoot_RPC", RpcTarget.All, player.aimAssist.targetHitbox.GetComponent<Hitbox>().biped.originalSpawnPosition);
            else PV.RPC("_StartShoot_RPC", RpcTarget.All, Vector3.zero);
        }
    }
    void SendIsNotHoldingFireWeaponBtn()
    {
        if (PV.IsMine && PhotonNetwork.InRoom)
            PV.RPC("_StopShoot_RPC", RpcTarget.All, false);
    }

    void SendIsNotHoldingDualWieldedFireWeaponBtn()
    {
        if (PV.IsMine && PhotonNetwork.InRoom)
            PV.RPC("_StopShoot_RPC", RpcTarget.All, true);
    }

    [PunRPC]
    void _StartShoot_RPC(Vector3 bipedOrSpp)
    {

        isHoldingShootBtn = true;

        try
        {
            holstered = false;
            weaponAnimator.SetBool("Holster", false);
            GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Holster Rifle", false);
        }
        catch { }


        if (!pInventory.activeWeapon.isOutOfAmmo && !isReloading &&
            !isHoldingShootBtn && !isInspecting && !isMeleeing && !isThrowingGrenade)
        {
            Debug.Log("_StartShoot_RPC");
            try
            {
                Debug.Log(GameManager.instance.instantiation_position_Biped_Dict[bipedOrSpp]);
            }
            catch { }
            //player.playerShooting.trackingTarget = null; if (bipedOrSpp != Vector3.zero) player.playerShooting.trackingTarget = GameManager.instance.instantiation_position_Biped_Dict[bipedOrSpp];
        }
        else
        {
            //isHoldingShootBtn = false;
        }
        Debug.Log($"{GetComponent<Player>().username}: _StartShoot_RPC {isHoldingShootBtn}");
    }

    [PunRPC]
    void _StopShoot_RPC(bool isDw)
    {
        if (!isDw)
        {
            if (player.isMine)
            {
                isHoldingShootBtn = false;
                OnPlayerFireButtonUp?.Invoke(this);
                Debug.Log($"{GetComponent<Player>().username}: _StopShoot_RPC {isHoldingShootBtn}");


            }
            else
            {
                StartCoroutine(StopShoot_Coroutine(false));
            }
        }
        else
        {
            StartCoroutine(StopShoot_Coroutine(true));
        }
    }

    IEnumerator StopShoot_Coroutine(bool isDw)
    {
        yield return new WaitForEndOfFrame();

        Debug.Log($"StopShoot_Coroutine {GetComponent<Player>().username}: {isDw}");
        if (!isDw)
        {
            isHoldingShootBtn = false;
            OnPlayerFireButtonUp?.Invoke(this);
        }
        else
        {
            isHoldingShootDualWieldedWeapon = false;
        }
    }



    [PunRPC]
    void SendIsHoldingFireWeaponBtn(bool isCaller, Vector3 trackingTargetInstantiationPosition, bool isDualWieldedWeapon)
    {

        if (isCaller)
            PV.RPC("SendIsHoldingFireWeaponBtn", RpcTarget.All, false, trackingTargetInstantiationPosition, isDualWieldedWeapon);
        else
        {
            print($"SendIsHoldingFireWeaponBtn received {isDualWieldedWeapon} {trackingTargetInstantiationPosition}");
            if (!isDualWieldedWeapon)
            {

                try { Debug.Log(GameManager.instance.instantiation_position_Biped_Dict[trackingTargetInstantiationPosition]); } catch { }
                player.playerShooting.trackingTarget = null; if (trackingTargetInstantiationPosition != Vector3.zero) player.playerShooting.trackingTarget = GameManager.instance.instantiation_position_Biped_Dict[trackingTargetInstantiationPosition];


                //if (!pInventory.activeWeapon.isOutOfAmmo && !isReloading && !isHoldingShootBtn && !isInspecting && !isMeleeing && !isThrowingGrenade)
                {
                    holstered = false;
                    weaponAnimator.SetBool("Holster", false);
                    GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Holster Rifle", false);
                }


                isHoldingShootBtn = true;
            }
            else
            {
                if (trackingTargetInstantiationPosition != Vector3.zero) player.playerShooting.trackingTarget = GameManager.instance.instantiation_position_Biped_Dict[trackingTargetInstantiationPosition]; else player.playerShooting.trackingTarget = null;
                isHoldingShootDualWieldedWeapon = true;
            }
        }
    }

    [PunRPC]
    public void UpdateTrackingTargetForOtherPlayers(bool isCaller, Vector3 trackingTargetInstantiationPosition)
    {
        if (isCaller)
            PV.RPC("UpdateTrackingTargetForOtherPlayers", RpcTarget.All, false, trackingTargetInstantiationPosition);
        else
        {
            if (!player.isMine)
            {
                player.playerShooting.trackingTarget = (trackingTargetInstantiationPosition != Vector3.zero) ? GameManager.instance.instantiation_position_Biped_Dict[trackingTargetInstantiationPosition] : null;
            }
        }
    }



    float _leftShootCooldown;
    float _tempFov;
    void Scope()
    {
        //if (isDualWielding || pInventory.activeWeapon.scopeMagnification == WeaponProperties.ScopeMagnification.None)
        //    return;

        if (cameraIsFloating) return;


        if (!pInventory.isDualWielding)
            if (pInventory.activeWeapon.scopeMagnification != WeaponProperties.ScopeMagnification.None)
            {


                if (pInventory.activeWeapon.scopeMagnification == WeaponProperties.ScopeMagnification.Close ||
                    pInventory.activeWeapon.scopeMagnification == WeaponProperties.ScopeMagnification.Medium)
                {
                    _tempFov = 35.98f;
                    if (GameManager.instance.nbLocalPlayersPreset % 2 == 0) _tempFov = 18.45f;
                }
                else if (pInventory.activeWeapon.scopeMagnification == WeaponProperties.ScopeMagnification.Long)
                {
                    _tempFov = 17.14f;
                    if (GameManager.instance.nbLocalPlayersPreset % 2 == 0) _tempFov = 8.62f;



                }

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

                            //mainCam.fieldOfView = _tempFov;
                            //uiCam.fieldOfView = _tempFov;
                            if (pInventory.activeWeapon.aimingMechanic == WeaponProperties.AimingMechanic.Scope)
                                gunCam.enabled = false;
                            //else
                            //    gunCam.fieldOfView = 50;

                            allPlayerScripts.aimingScript.playAimSound();
                        }
                        else
                        {
                            isAiming = false;
                            //mainCam.fieldOfView = GetComponent<Player>().defaultVerticalFov;
                            //uiCam.fieldOfView = GetComponent<Player>().defaultVerticalFov;
                            camScript.backEndMouseSens = camScript.frontEndMouseSens;
                            gunCam.enabled = true;
                            //gunCam.fieldOfView = 60;

                            allPlayerScripts.aimingScript.playAimSound();
                        }
                    }
                }
                else if (rewiredPlayer.GetButtonUp("Aim"))
                {
                    if (activeControllerType == ControllerType.Keyboard || activeControllerType == ControllerType.Mouse)
                    {
                        Descope();
                    }
                }
            }








        if (isAiming && pInventory.activeWeapon.aimingMechanic == WeaponProperties.AimingMechanic.Scope)
        {
            player.playerUI.ToggleMotionTracker_ForAiming(false);
            player.playerUI.bottomRight.gameObject.SetActive(false);
            player.playerUI.topLeft.gameObject.SetActive(false);
            player.playerUI.topMiddle.gameObject.SetActive(false);
        }
        else if (!isAiming)
        {
            player.playerUI.bottomRight.gameObject.SetActive(true);
            player.playerUI.topLeft.gameObject.SetActive(true);
            player.playerUI.topMiddle.gameObject.SetActive(true);



            if (GameManager.instance.gameType != GameManager.GameType.Pro &&
            GameManager.instance.gameType != GameManager.GameType.Swat &&
            GameManager.instance.gameType != GameManager.GameType.Snipers &&
             GameManager.instance.gameType != GameManager.GameType.Retro)
                player.playerUI.ToggleMotionTracker_ForAiming(true);
            else
                player.playerUI.ToggleMotionTracker_ForAiming(false);
        }
    }


    float zoomTime = 1f;
    void ScopeZoom()
    {
        float plusZoom = 0;
    }

    public void Descope()
    {
        if (!isAiming && !GetComponent<Player>().isDead)
            return;
        Debug.Log("Unscope Script");
        isAiming = false;
        //mainCam.fieldOfView = GetComponent<Player>().defaultVerticalFov;
        //gunCam.fieldOfView = 60;
        //uiCam.fieldOfView = GetComponent<Player>().defaultVerticalFov;
        camScript.backEndMouseSens = camScript.frontEndMouseSens;
        allPlayerScripts.aimingScript.playAimSound();

        mainCam.transform.localRotation = Quaternion.Euler(0, 0, 0);
        gunCam.enabled = true;
    }

    int _meleeCount = 0;
    float _meleeMovementFactor;
    bool _meleeSucc;
    void Melee(bool overwrite = false)
    {
        if (player.isAlive && player.isMine)
        {
            if ((overwrite) || ((rewiredPlayer.GetButtonDown("Melee") || rewiredPlayer.GetButtonDown("MouseBtn4")) && !isMeleeing && !isThrowingGrenade /*&& !isSprinting*/))
            {
                _meleeSucc = false;


                Descope();
                rScript.reloadIsCanceled = true;

                melee.PushIfAble();


                StartCoroutine(Melee_Coroutine(player.playerInventory.activeWeapon));
            }
        }

    }



    IEnumerator Melee_Coroutine(WeaponProperties wp)
    {
        weaponAnimator.Play("Knife Attack 2", 0, 0f);
        PV.RPC("MeleeAnimations_RPC", RpcTarget.All);

        yield return new WaitForSeconds(0.1f);

        _meleeSucc = melee.MeleeDamage(wp);
        PV.RPC("Melee_RPC", RpcTarget.All, _meleeSucc);
    }





    [PunRPC]
    void MeleeAnimations_RPC()
    {
        print("MeleeAnimations_RPC");
        DisableSprint_RPC();
        isMeleeing = true;
        GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().ResetTrigger("Fire");

        weaponAnimator.Play("Knife Attack 2", 0, 0f);
        StartCoroutine(Melee3PS());
    }



    [PunRPC]
    void Melee_RPC(bool succ)
    {
        if (PV.IsMine && pInventory.isDualWielding)
        {
            pInventory.DropThirdWeapon();
        }



        if ((!player.isDead && !player.isRespawning))
        {
            print("Melee_RPC");
            if (!succ) melee.PlayMissClip();
            else
            {
                if (player.playerInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Sword)
                    player.playerInventory.activeWeapon.loadedAmmo--;
            }
            //if (succ) melee.PlaySuccClip(); else melee.PlayMissClip();
        }
        _meleeSucc = false;
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
        mainCam.GetComponent<Transform>().localPosition += new Vector3(0, -.30f, 0);
        gwProperties.bulletSpawnPoint.localPosition += new Vector3(0, -.30f, 0);


        PV.RPC("EnableCrouch_RPC", RpcTarget.All);
    }

    public void DisableCrouch()
    {
        OnCrouchUp?.Invoke(this);

        GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Crouch", false);
        isCrouching = false;
        mainCam.GetComponent<Transform>().localPosition += new Vector3(0, .30f, 0);
        gwProperties.bulletSpawnPoint.localPosition = gwProperties.defaultBulletSpawnPoint;


        PV.RPC("DisableCrouch_RPC", RpcTarget.All);
    }


    [PunRPC]
    void EnableCrouch_RPC()
    {
        isCrouching = true; // for motion tracker
        movement.playerCapsule.localScale = new Vector3(transform.localScale.x, movement.crouchYScale, transform.localScale.z);

        if (movement.isGrounded)
            movement.rb.AddForce(Vector3.down * 0.7f, ForceMode.Impulse);



        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast((player.mainCamera.transform.position + player.playerCapsule.transform.position) / 2, Vector3.down, out hit, (player.playerCapsule.GetComponent<CapsuleCollider>().height / 2) + 0.5f, GameManager.instance.ragdollHumpLayerMask))
        {
            //Debug.Log($"EnableCrouch_RPC Did Hit: {hit.collider.name}");
            if (hit.collider.GetComponent<RagdollLimbCollisionDetection>() && hit.collider.GetComponent<Rigidbody>() && hit.collider.GetComponent<Rigidbody>().velocity.magnitude < 20)
            {
                Debug.Log($"EnableCrouch_RPC Did Hit: {hit.collider.name} {hit.collider.GetComponent<Rigidbody>().velocity.magnitude}");
                hit.collider.GetComponent<Rigidbody>().AddForce(Vector3.up * 250 * hit.collider.GetComponent<Rigidbody>().mass);
            }
        }
    }

    [PunRPC]
    void DisableCrouch_RPC()
    {
        isCrouching = false;// for motion tracker
        movement.playerCapsule.localScale = new Vector3(transform.localScale.x, 1, transform.localScale.z);

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast((player.mainCamera.transform.position + player.playerCapsule.transform.position) / 2, Vector3.down, out hit, (player.playerCapsule.GetComponent<CapsuleCollider>().height / 2) + 0.5f, GameManager.instance.ragdollHumpLayerMask))
        {
            //Debug.Log($"DisableCrouch_RPC Did Hit: {hit.collider.name}");
            if (hit.collider.GetComponent<RagdollLimbCollisionDetection>() && hit.collider.GetComponent<Rigidbody>())
            {
                Debug.Log($"DisableCrouch_RPC Did Hit: {hit.collider.name} {hit.collider.GetComponent<Rigidbody>().velocity.magnitude}");
                hit.collider.GetComponent<Rigidbody>().AddForce(Vector3.up * 150 * hit.collider.GetComponent<Rigidbody>().mass);
            }
        }
    }



    void GrenadeBtn()
    {
        if ((rewiredPlayer.GetButtonDown("Throw Grenade") || rewiredPlayer.GetButtonDown("MouseBtn5")) /*&& !player.isDualWielding*/ /*&& !isMeleeing*/ /*&& !isSprinting*/)
        {
            if (!pInventory.isDualWielding)
            {
                ThrowGrenade();
            }
            else
            {
                if (activeControllerType == ControllerType.Joystick)
                {
                    // do nothing
                }
                else
                {
                    pInventory.DropThirdWeapon();
                    ThrowGrenade();
                }
            }
        }

    }


    void ThrowGrenade()
    {
        if (pInventory.fragGrenades > 0 && !isThrowingGrenade && !player.playerInventory.holdingObjective)
        {
            print($"Grenade 1");
            CancelReloadCoroutine();
            currentlyReloadingTimer = 0;
            player.playerShooting.StopBurstFiring();
            rScript.reloadIsCanceled = true;
            Descope();
            pInventory.fragGrenades = pInventory.fragGrenades - 1;
            PV.RPC("ThrowGrenade3PS_RPC", RpcTarget.All);
            StartCoroutine(GrenadeSpawnDelay());
            OnPLayerThrewGrenade?.Invoke(this);
            //StartCoroutine(GrenadeSpawnDelay());
            //StartCoroutine(ThrowGrenade3PS());
        }
    }



    void CheckReloadButton()
    {
        if (!player.isDead)
        {
            if (PV.IsMine && rewiredPlayer.GetButtonDown("Reload"))
            {
                Reload();
            }
        }
    }


    enum ReloadMode { Normal, LeftOnly, RightOnly, Both }
    private void Reload()
    {
        if (PV.IsMine)
        {
            if (!player.playerInventory.isDualWielding)
            {
                if (!isDrawingWeapon && !isThrowingGrenade && !isMeleeing && !isReloading)
                    if (player.playerInventory.activeWeapon.loadedAmmo < player.playerInventory.activeWeapon.ammoCapacity && player.playerInventory.activeWeapon.spareAmmo > 0)
                        PV.RPC("Reload_RPC", RpcTarget.All, 0);
            }
            else
            {
                if ((!isReloadingRight && player.playerInventory.activeWeapon.loadedAmmo < player.playerInventory.activeWeapon.ammoCapacity && player.playerInventory.activeWeapon.spareAmmo > 0) &&
                    (!isReloadingLeft && player.playerInventory.thirdWeapon.loadedAmmo < player.playerInventory.thirdWeapon.ammoCapacity && player.playerInventory.thirdWeapon.spareAmmo > 0))
                {
                    PV.RPC("Reload_RPC", RpcTarget.All, 3);
                }
                else if ((!isReloadingRight && player.playerInventory.activeWeapon.loadedAmmo < player.playerInventory.activeWeapon.ammoCapacity && player.playerInventory.activeWeapon.spareAmmo > 0))
                {
                    PV.RPC("Reload_RPC", RpcTarget.All, 2);

                }
                else if ((!isReloadingLeft && player.playerInventory.thirdWeapon.loadedAmmo < player.playerInventory.thirdWeapon.ammoCapacity && player.playerInventory.thirdWeapon.spareAmmo > 0))
                {
                    PV.RPC("Reload_RPC", RpcTarget.All, 1);

                }
                else
                {
                    print($"Unresolved case for reloading");
                }



                //if (!isReloading)
                //    if (player.playerInventory.activeWeapon.spareAmmo > 0 || player.playerInventory.thirdWeapon.spareAmmo > 0)
                //        PV.RPC("Reload_RPC", RpcTarget.All);


            }
        }
    }



    [PunRPC]
    void Reload_RPC(int reloadModeInt)
    {
        if ((ReloadMode)reloadModeInt == ReloadMode.Normal)
        {
            _completeReloadTimer = 1; // Used to trasnfer ammo
            currentlyReloadingTimer = 1.7f; // Used to lock animation time


            if (pInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Grenade_Launcher ||
                pInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.RPG ||
                pInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Shotgun ||
                pInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Sniper)
            {
                currentlyReloadingTimer = 2.8f; // Used to lock animation time
            }

            if (pInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Pistol) currentlyReloadingTimer = 1.2f;
            if (pInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Sniper) currentlyReloadingTimer = 2.3f;






            player.playerShooting.StopAllCoroutines();
            Descope();
            rScript.PlayReloadSound(Array.IndexOf(player.playerInventory.allWeaponsInInventory, player.playerInventory.activeWeapon.gameObject));


            if (pInventory.activeWeapon.ammoReloadType == WeaponProperties.AmmoReloadType.Magazine || pInventory.activeWeapon.ammoReloadType == WeaponProperties.AmmoReloadType.Generic)
            {
                if (pInventory.activeWeapon.ammoReloadType == WeaponProperties.AmmoReloadType.Magazine)
                    weaponAnimator.Play("Reload Ammo Left", 0, 0f);
                else if (pInventory.activeWeapon.ammoReloadType == WeaponProperties.AmmoReloadType.Generic)
                    weaponAnimator.Play("reload generic", 0, 0f);

                StartCoroutine(Reload3PS());
            }
        }
        else if ((ReloadMode)reloadModeInt == ReloadMode.LeftOnly)
        {
            // third weapon
            if (player.playerInventory.thirdWeapon /*&& player.playerInventory.thirdWeapon.loadedAmmo < player.playerInventory.thirdWeapon.ammoCapacity && _currentlyReloadingTimer_thirdWeapon <= 0*/)
            {
                player.playerInventory.thirdWeapon.GetComponent<Animator>().Play("dw reload", 0, 0f);



                _completeReloadTimer_thirdWeapon = 2f; // Used to trasnfer ammo
                _currentlyReloadingTimer_thirdWeapon = 3.5f; // Used to lock animation time

                if (pInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Revolver)
                {
                    _completeReloadTimer_thirdWeapon = 1.5f; // Used to trasnfer ammo
                    _currentlyReloadingTimer_thirdWeapon = 2.5f; // Used to lock animation time
                }

                //GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().Play("dw reload");

                rScript.PlayerLeftWeaponReloadSound();
            }

        }
        else if ((ReloadMode)reloadModeInt == ReloadMode.RightOnly)
        {
            _completeReloadTimer = 2; // Used to trasnfer ammo
            currentlyReloadingTimer = 3.5f; // Used to lock animation time

            if (pInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Revolver)
            {
                _completeReloadTimer = 1.5f; // Used to trasnfer ammo
                currentlyReloadingTimer = 2.5f; // Used to lock animation time
            }




            player.playerShooting.StopAllCoroutines();
            Descope();
            rScript.PlayReloadSound(Array.IndexOf(player.playerInventory.allWeaponsInInventory, player.playerInventory.activeWeapon.gameObject));


            if (pInventory.activeWeapon.ammoReloadType == WeaponProperties.AmmoReloadType.Magazine || pInventory.activeWeapon.ammoReloadType == WeaponProperties.AmmoReloadType.Generic)
                player.playerInventory.activeWeapon.GetComponent<Animator>().Play("dw reload", 0, 0f);
        }
        else if (((ReloadMode)reloadModeInt == ReloadMode.Both))
        {
            _completeReloadTimer = _completeReloadTimer_thirdWeapon = 2; // Used to trasnfer ammo
            currentlyReloadingTimer = _currentlyReloadingTimer_thirdWeapon = 3.5f; // Used to lock animation time


            if (pInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Revolver)
            {
                _completeReloadTimer = 1.5f; // Used to trasnfer ammo
                currentlyReloadingTimer = 2.5f; // Used to lock animation time
            }
            if (pInventory.thirdWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Revolver)
            {
                _completeReloadTimer_thirdWeapon = 1.5f; // Used to trasnfer ammo
                _currentlyReloadingTimer_thirdWeapon = 2.5f; // Used to lock animation time
            }



            player.playerShooting.StopAllCoroutines();
            Descope();
            rScript.PlayReloadSound(Array.IndexOf(player.playerInventory.allWeaponsInInventory, player.playerInventory.activeWeapon.gameObject));


            if (pInventory.activeWeapon.ammoReloadType == WeaponProperties.AmmoReloadType.Magazine || pInventory.activeWeapon.ammoReloadType == WeaponProperties.AmmoReloadType.Generic)
                player.playerInventory.activeWeapon.GetComponent<Animator>().Play("dw reload", 0, 0f);

            // third weapon
            player.playerInventory.thirdWeapon.GetComponent<Animator>().Play("dw reload", 0, 0f);
            GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().Play("dw reload");
            rScript.PlayerLeftWeaponReloadSound();
        }





















        return;
        _completeReloadTimer = 1; // Used to trasnfer ammo
        currentlyReloadingTimer = 1.7f; // Used to lock animation time


        if (pInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Grenade_Launcher ||
            pInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.RPG ||
            pInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Shotgun ||
            pInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Sniper)
        {
            currentlyReloadingTimer = 2.8f; // Used to lock animation time
        }

        if (pInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Pistol) currentlyReloadingTimer = 1.2f;
        if (pInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Sniper) currentlyReloadingTimer = 2.3f;



        if (player.playerInventory.isDualWielding)
        {
            _completeReloadTimer = 2; // Used to trasnfer ammo
            currentlyReloadingTimer = 4f; // Used to lock animation time

            if (pInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Revolver)
            {
                _completeReloadTimer = 1.5f; // Used to trasnfer ammo
                currentlyReloadingTimer = 3f; // Used to lock animation time
            }
        }




        player.playerShooting.StopAllCoroutines();
        Descope();
        rScript.PlayReloadSound(Array.IndexOf(player.playerInventory.allWeaponsInInventory, player.playerInventory.activeWeapon.gameObject));


        if (pInventory.activeWeapon.ammoReloadType == WeaponProperties.AmmoReloadType.Magazine || pInventory.activeWeapon.ammoReloadType == WeaponProperties.AmmoReloadType.Generic)
        {
            if (!player.playerInventory.isDualWielding)
            {
                if (pInventory.activeWeapon.ammoReloadType == WeaponProperties.AmmoReloadType.Magazine)
                    weaponAnimator.Play("Reload Ammo Left", 0, 0f);
                else if (pInventory.activeWeapon.ammoReloadType == WeaponProperties.AmmoReloadType.Generic)
                    weaponAnimator.Play("reload generic", 0, 0f);



                StartCoroutine(Reload3PS());
            }
            else
            {
                player.playerInventory.activeWeapon.GetComponent<Animator>().Play("dw reload", 0, 0f);
            }



        }








        // third weapon
        if (player.playerInventory.thirdWeapon /*&& player.playerInventory.thirdWeapon.loadedAmmo < player.playerInventory.thirdWeapon.ammoCapacity && _currentlyReloadingTimer_thirdWeapon <= 0*/)
        {
            player.playerInventory.thirdWeapon.GetComponent<Animator>().Play("dw reload", 0, 0f);



            _completeReloadTimer_thirdWeapon = 2f; // Used to trasnfer ammo
            _currentlyReloadingTimer_thirdWeapon = 4f; // Used to lock animation time

            if (pInventory.thirdWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Pistol)
            {
                _completeReloadTimer_thirdWeapon = 1.5f; // Used to trasnfer ammo
                _currentlyReloadingTimer_thirdWeapon = 3; // Used to lock animation time
            }

            GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().Play("dw reload");

            rScript.PlayerLeftWeaponReloadSound();



            //SoundManager.instance.PlayAudioClip(transform.position, player.playerInventory.thirdWeapon.ReloadShort);


            //if (pInventory.activeWeapon.ammoReloadType == WeaponProperties.AmmoReloadType.Magazine || pInventory.activeWeapon.ammoReloadType == WeaponProperties.AmmoReloadType.Generic)
            //{
            //    StartCoroutine(Reload3PS());
            //}
        }
    }

    void AutoReload()
    {
        if (pInventory.activeWeapon)
        //if (PV.IsMine && !isDualWielding && !isDrawingWeapon && !isThrowingGrenade && !isMeleeing)
        {
            if (!pInventory.isDualWielding)
            {
                if (pInventory.activeWeapon.loadedAmmo <= 0)
                    Reload();
            }
            else
            {
                if ((!isReloadingRight && player.playerInventory.activeWeapon.loadedAmmo == 0 && player.playerInventory.activeWeapon.spareAmmo > 0) &&
                    (!isReloadingLeft && player.playerInventory.thirdWeapon.loadedAmmo == 0 && player.playerInventory.thirdWeapon.spareAmmo > 0))
                {
                    PV.RPC("Reload_RPC", RpcTarget.All, 3);
                }
                else if ((!isReloadingRight && player.playerInventory.activeWeapon.loadedAmmo == 0 && player.playerInventory.activeWeapon.spareAmmo > 0))
                {
                    PV.RPC("Reload_RPC", RpcTarget.All, 2);

                }
                else if ((!isReloadingLeft && player.playerInventory.thirdWeapon.loadedAmmo == 0 && player.playerInventory.thirdWeapon.spareAmmo > 0))
                {
                    PV.RPC("Reload_RPC", RpcTarget.All, 1);

                }
                else
                {
                    //print($"Unresolved case for reloading");
                }
            }
        }


        //if (pInventory.activeWeapon.loadedAmmo <= 0 && pInventory.holsteredWeapon.loadedAmmo <= 0)
        //    player.PlayOutOfAmmoClip();
    }


    void AutoReloadThirdWeapon()
    {
        //if (player.playerInventory.thirdWeapon)
        //{
        //    if (player.playerInventory.thirdWeapon.loadedAmmo == 0 && player.playerInventory.thirdWeapon.spareAmmo > 0)
        //    {
        //        ReloadThirdWeapon();
        //    }
        //}
    }

    void ReloadThirdWeapon()
    {
        return;// impossible. Each arm of the third person model needs to be independant to independently reload each guns



        if (player.playerInventory.thirdWeapon && player.playerInventory.thirdWeapon.loadedAmmo < player.playerInventory.thirdWeapon.ammoCapacity && _currentlyReloadingTimer_thirdWeapon <= 0)
        {
            player.playerInventory.thirdWeapon.GetComponent<Animator>().Play("dw reload", 0, 0f);



            _completeReloadTimer_thirdWeapon = 2f; // Used to trasnfer ammo
            _currentlyReloadingTimer_thirdWeapon = 4f; // Used to lock animation time

            if (pInventory.thirdWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Pistol)
            {
                _completeReloadTimer_thirdWeapon = 1.5f; // Used to trasnfer ammo
                _currentlyReloadingTimer_thirdWeapon = 3; // Used to lock animation time
            }





            SoundManager.instance.PlayAudioClip(transform.position, player.playerInventory.thirdWeapon.ReloadShort);


            //if (pInventory.activeWeapon.ammoReloadType == WeaponProperties.AmmoReloadType.Magazine || pInventory.activeWeapon.ammoReloadType == WeaponProperties.AmmoReloadType.Generic)
            //{
            //    StartCoroutine(Reload3PS());
            //}
        }
    }


    void CheckDrawingWeapon()
    {
        //if (weaponAnimator)
        //    if (weaponAnimator.GetCurrentAnimatorStateInfo(0).IsName("Draw"))
        //        isDrawingWeapon = true;
        //    else
        //        isDrawingWeapon = false;


        isDrawingWeapon = _drawingWeaponTime > 0;
    }

    void SwitchWeapons()
    {
        if (GameManager.instance.gameType != GameManager.GameType.GunGame &&
            GameManager.instance.gameType != GameManager.GameType.Shotguns &&
            GameManager.instance.gameType != GameManager.GameType.Snipers &&
            GameManager.instance.gameType != GameManager.GameType.PurpleRain &&
            GameManager.instance.gameType != GameManager.GameType.Swords)
            if (rewiredPlayer.GetButtonDown("Switch Weapons"))
            {
                try
                {
                    Debug.Log("SwitchWeapons");
                    CancelReloadCoroutine();
                    weaponAnimator = pInventory.activeWeapon.GetComponent<Animator>();
                    currentlyReloadingTimer = 0;
                    OnPlayerSwitchWeapons?.Invoke(this);
                }
                catch { }
            }
    }
    void SwitchGrenades()
    {
        if (GameManager.instance.gameType != GameManager.GameType.Martian)
            if (rewiredPlayer.GetButtonDown("Switch Grenades") && PV.IsMine)
            {
                PV.RPC("SwitchGrenades_RPC", RpcTarget.All);
            }
    }



    void FloatingCamera()
    {
        //if (GameManager.instance.devMode)

        if (rewiredPlayer.GetButtonLongPressDown("floatingcamera") && PV.IsMine)
        {
            ToggleFloatingCamera();
        }
        else if (rewiredPlayer.GetButtonDown("floatingcamera") && PV.IsMine && cameraIsFloating)
        {
            PV.RPC("IncreaseFloatinCameraCounter_RPC", RpcTarget.All, false);
        }
        else if (rewiredPlayer.GetButtonDown("floatingcameraminus") && PV.IsMine && cameraIsFloating)
        {
            PV.RPC("IncreaseFloatinCameraCounter_RPC", RpcTarget.All, true);
        }
    }

    public void ToggleFloatingCamera()
    {
        if (PV.IsMine)
        {
            PV.RPC("ToggleFloatingCamera_RPC", RpcTarget.All);
        }
    }


    Vector3 _posBeforeTogglingFloatingCamera;
    [PunRPC]
    void ToggleFloatingCamera_RPC()
    {
        if (!cameraIsFloating)
        {
            _lastMainCamLayerMask = mainCam.cullingMask;
            _lastMainCamLocalPos = mainCam.transform.localPosition;
            _lasMainCamLocalQuat = mainCam.transform.localRotation;

            mainCam.transform.parent = null;
            mainCam.cullingMask = floatinCameraLayerMask;
            _cameraIsFloating = true;
            Debug.Log("Alpha0");
            uiCam.enabled = false;
            gunCam.enabled = false;


            floatingCamera.counter = 0;
            _posBeforeTogglingFloatingCamera = transform.position;
            player.transform.position = Vector3.up * -100;
            player.GetComponent<Rigidbody>().useGravity = false;
            player.GetComponent<Rigidbody>().isKinematic = true;
        }
        else
        {
            mainCam.cullingMask = _lastMainCamLayerMask;
            mainCam.transform.parent = _mainCamParent;
            mainCam.transform.localPosition = _lastMainCamLocalPos;
            mainCam.transform.localRotation = _lasMainCamLocalQuat;

            _cameraIsFloating = false;
            Debug.Log("Alpha0");
            uiCam.enabled = true;
            gunCam.enabled = true;


            player.transform.position = _posBeforeTogglingFloatingCamera;
            player.GetComponent<Rigidbody>().useGravity = true;
            player.GetComponent<Rigidbody>().isKinematic = false;
        }
    }

    [PunRPC]
    void IncreaseFloatinCameraCounter_RPC(bool actuallyMinus)
    {
        if (!actuallyMinus)
            floatingCamera.counter++;
        else
            floatingCamera.counter--;
    }




    [PunRPC]
    void SwitchGrenades_RPC()
    {
        if (fragGrenadesActive)
        {
            fragGrenadesActive = false;
            stickyGrenadesActive = true;

            player.playerUI.fragGrenadeImage.gameObject.SetActive(false);
            player.playerUI.plasmaGrenadeImage.gameObject.SetActive(true);
            player.playerUI.plasmaGrenadeImage.GetComponent<AudioSource>().Play();
            player.playerUI.plasmaGrenadeImage.GetComponent<IncreaseScaleThenScaleBackInTime>().Trigger();
        }
        else if (stickyGrenadesActive)
        {
            stickyGrenadesActive = false;
            fragGrenadesActive = true;

            player.playerUI.plasmaGrenadeImage.gameObject.SetActive(false);
            player.playerUI.fragGrenadeImage.gameObject.SetActive(true);
            player.playerUI.fragGrenadeImage.GetComponent<AudioSource>().Play();
            player.playerUI.fragGrenadeImage.GetComponent<IncreaseScaleThenScaleBackInTime>().Trigger();
        }
    }

    void HolsterAndInspect()
    {
        if (rewiredPlayer.GetButtonLongPressDown("holster") && PV.IsMine && !player.isDualWielding)
        {
            if (player.playerInventory.isDualWielding) player.playerInventory.DropThirdWeapon();
            PV.RPC("ToggleHolsterWeaponAnimation_RPC", RpcTarget.All, !holstered);
        }
    }


    [PunRPC]
    void ToggleHolsterWeaponAnimation_RPC(bool newVal)
    {
        holstered = newVal;
        if (weaponAnimator)
        {
            weaponAnimator.SetBool("Holster", holstered);
            GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Holster Rifle", holstered);
        }
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
                            //_reloadingRecovery = 1;
                        }
                        else
                        {
                            //isReloading = false;
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
                            //isReloading = true;
                        }
                        else
                        {
                            //isReloading = false;
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
                            //isReloading = true;

                            /*
                            if (wProperties.projectileToHide != null)
                            {
                                wProperties.projectileToHide.SetActive(true);
                            }*/
                        }

                        else
                        {
                            //isReloading = false;
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
                    //isThrowingGrenade = true;
                }
                else
                {
                    //isThrowingGrenade = false;
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
        GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().ResetTrigger("Fire");
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
        if (player.playerInventory.activeWeapon.killFeedOutput != WeaponProperties.KillFeedOutput.Sword)
            GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().Play("Melee");
        else
            GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().Play("sword attack");
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
        _currentlyThrowingGrenadeTimer = 1f;
        print("ThrowGrenade3PS_RPC");
        weaponAnimator.Play("GrenadeThrow", 0, 0.0f);
        StartCoroutine(ThrowGrenade3PS());
    }

    private IEnumerator GrenadeSpawnDelay()
    {
        GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().Play("Throw Grenade");

        //Wait for set amount of time before spawning grenade
        yield return new WaitForSeconds(0.2f);
        //Spawn grenade prefab at spawnpoint
        print($"Grenade 2");

        if (PV.IsMine && !player.isDead)
            PV.RPC("SpawnGrenade_RPC", RpcTarget.AllViaServer, fragGrenadesActive, GrenadePool.GetAvailableGrenadeIndex(fragGrenadesActive, player.playerDataCell.photonRoomIndex), gwProperties.grenadeSpawnPoint.position,
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
    public void TransferAmmo(WeaponProperties weap)
    {
        if (weap.ammoReloadType == WeaponProperties.AmmoReloadType.Shell)
        {
            weap.spareAmmo -= 1;
            weap.loadedAmmo += 1;
        }
        else
        {
            ammoWeaponIsMissing = weap.ammoCapacity - weap.loadedAmmo;

            if (weap.spareAmmo >= ammoWeaponIsMissing)
            {
                weap.loadedAmmo = weap.ammoCapacity;
                weap.spareAmmo -= ammoWeaponIsMissing;
            }
            else if (weap.spareAmmo < ammoWeaponIsMissing)
            {
                weap.loadedAmmo += weap.spareAmmo;
                weap.spareAmmo = 0;
            }
        }
    }

    void TestButton()
    {
        //if (Input.GetKeyDown(KeyCode.T))
        //{

        //    print(CurrentRoomManager.instance.youHaveInvites);
        //    print(CurrentRoomManager.instance.halfOfPlayersInRoomAreRandos);

        //    //OnPlayerTestButton?.Invoke(this);
        //}
    }
    void StartButton()
    {
        if (rewiredPlayer.GetButtonDown("Start") || rewiredPlayer.GetButtonDown("Escape"))
        {
            TogglePauseGame();
        }
    }

    void BackButton()
    {
        if (!player.isDead)
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
        print($"TogglePauseGame {pauseMenuOpen} {CurrentRoomManager.instance.gameOver} {GetComponent<PlayerUI>().singlePlayerPauseMenu}");
        if (!pauseMenuOpen)
        {
            if (!CurrentRoomManager.instance.gameOver)
            {
                GetComponent<PlayerUI>().singlePlayerPauseMenu.gameObject.SetActive(true);
                player.playerUI.gamepadCursor.gameObject.SetActive(true);
                player.playerUI.gamepadCursor.GetReady(ReInput.controllers.GetLastActiveControllerType());
                pauseMenuOpen = true;
            }
        }
        else
        {
            GetComponent<PlayerUI>().singlePlayerPauseMenu.gameObject.SetActive(false);
            player.playerUI.gamepadCursor.CloseFromPauseMenu();
            player.playerUI.gamepadCursor.gameObject.SetActive(false);
            pauseMenuOpen = false;
        }
    }

    public void QuitMatch()
    {
        Debug.Log("Returning to Main Menu");

        CurrentRoomManager.instance.leftRoomManually = true;

        GameManager.instance.previousScenePayloads.Add(GameManager.PreviousScenePayload.ResetPlayerDataCells);


        if (GameManager.instance.gameMode == GameManager.GameMode.Versus)
            FindObjectOfType<MultiplayerManager>().EndGame(false, actuallyQuit: true);
        if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
            FindObjectOfType<SwarmManager>().EndGame(false, actuallyQuit: true);
    }

    public void SetPlayerIDInInput()
    {
        rewiredPlayer = ReInput.players.GetPlayer(rid);
    }

    void OnTestButton_Delegate(PlayerController playerController)
    {
#if (UNITY_EDITOR)

        player.Damage(250, false, player.photonId);



        //allPlayerScripts.announcer.PlayGameOverClip();
        //WebManager.webManagerInstance.SaveMultiplayerStats(player.GetComponent<PlayerMultiplayerMatchStats>(), new List<Player>());
        //player.LeaveRoomWithDelay();
#endif

        {
            //LootableWeapon _firstWeapon = WeaponPool.instance.GetLootableWeapon(player.playerInventory.allWeaponsInInventory[3].GetComponent<WeaponProperties>().codeName);
            //Debug.Log($"First Weapon: {_firstWeapon.name}");
            //try { _firstWeapon.name = _firstWeapon.name.Replace("(Clone)", ""); } catch (System.Exception e) { Debug.Log(e); }
            ////try { _firstWeapon.transform.parent = null; } catch (System.Exception e) { Debug.Log(e); }
            //try { _firstWeapon.transform.position = player.weaponDropPoint.position; } catch (System.Exception e) { Debug.Log(e); }
            //try { _firstWeapon.spawnPointPosition = player.weaponDropPoint.position; } catch (System.Exception e) { Debug.Log(e); }
            ////try { _firstWeapon.GetComponent<Rigidbody>().velocity = Vector3.zero; } catch (System.Exception e) { Debug.Log(e); }
            //Debug.Log($"First Weapon: {_firstWeapon.GetComponent<Rigidbody>()}");
            //_firstWeapon.GetComponent<Rigidbody>().AddForce(player.weaponDropPoint.forward * 2000, ForceMode.Impulse);
            //try { _firstWeapon.localAmmo = 10; } catch (System.Exception e) { Debug.Log(e); }
            //try { _firstWeapon.spareAmmo = 10; } catch (System.Exception e) { Debug.Log(e); }


            //try { _firstWeapon.ttl = _firstWeapon.defaultTtl; } catch (System.Exception e) { Debug.Log(e); }
            //try { _firstWeapon.gameObject.SetActive(true); } catch (System.Exception e) { Debug.Log(e); }
            //_firstWeapon.GetComponent<Rigidbody>().AddForce(player.weaponDropPoint.forward * 200, ForceMode.Impulse);

        }

        return;

        //Transform pr = Instantiate(GameObjectPool.instance.playerRagdoll.gameObject, transform.position + new Vector3(0, 0.5f, 1), transform.rotation).transform;

        //Debug.Log(player.playerArmorManager.colorPalette);
        //pr.GetComponent<PlayerArmorManager>().armorDataString = player.playerArmorManager.armorDataString;
        ////pr.GetComponent<PlayerArmorManager>().armorDataString = "";
        //pr.GetComponent<PlayerArmorManager>().colorPalette = player.playerArmorManager.colorPalette;

        //ConfigureRagdollPosition(player.playerArmorManager.transform, pr.transform);
    }

    public void ConfigureRagdollPosition(Transform reference, Transform ragdollPart)
    {
        if (!ragdollPart.GetComponent<CharacterJoint>())
        {
            Debug.Log(ragdollPart.name);
            ragdollPart.localPosition = reference.localPosition;
            ragdollPart.localPosition = reference.localPosition;
        }

        ragdollPart.gameObject.layer = 0;

        for (int i = 0; i < reference.childCount; i++)
        {
            Transform ref_t = reference.GetChild(i);
            Transform rag_t = ragdollPart.GetChild(i);

            if (ref_t != null && rag_t != null)
                ConfigureRagdollPosition(ref_t, rag_t);
        }
    }
    public void OnDeath_Delegate(Player player)
    {
        Debug.Log("OnDeath_Delegate");
        _currentlyThrowingGrenadeTimer = 0;
        _currentlyReloadingTimer = _currentlyReloadingTimer_thirdWeapon = 0;

        CancelReloadCoroutine();
        isSprinting = false; _meleeSucc = false;
        isHoldingShootBtn = false;
    }

    void OnRespawn_Delegate(Player player)
    {
        _currentlyReloadingTimer = _completeReloadTimer = _currentlyThrowingGrenadeTimer = _isCurrentlyShootingReset = _drawingWeaponTime = 0;
        _completeReloadTimer_thirdWeapon = _currentlyReloadingTimer_thirdWeapon = _drawingWeaponTime_thirdWeapon = _isCurrentlyShootingReset_thirdWeapon = 0;

        CancelReloadCoroutine();
        isSprinting = false; _meleeSucc = false;
        isHoldingShootBtn = false;
    }





    public void CancelReloadCoroutine()
    {
        GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().ResetTrigger("Fire");
        _completeReloadTimer = -9;
    }

    public void SetDrawingWeaponCooldown()
    {
        _drawingWeaponTime = WEAPON_DRAW_TIME;

        if (pInventory.activeWeapon)
            if (pInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Pistol || pInventory.activeWeapon.killFeedOutput == WeaponProperties.KillFeedOutput.Plasma_Pistol)
                _drawingWeaponTime = 0.45f;
    }




    int _framesMarkSpotHasBeenHeld;
    void MarkSpot()
    {
        if (rewiredPlayer.GetButton("mark"))
        {
            if (_framesMarkSpotHasBeenHeld < GameManager.DEFAULT_FRAMERATE / 3 && _framesMarkSpotHasBeenHeld > -1)
                _framesMarkSpotHasBeenHeld = Mathf.Clamp(_framesMarkSpotHasBeenHeld + 1, 0, GameManager.DEFAULT_FRAMERATE / 3);

            if (_framesMarkSpotHasBeenHeld == GameManager.DEFAULT_FRAMERATE / 3)
            {
                print($"MARK SPOT HELD LONG {_framesMarkSpotHasBeenHeld}");

                player.playerInteractableObjectHandler.TriggerLongInteract();
            }
        }
        else if (rewiredPlayer.GetButtonUp("mark"))
        {
            print($"MarkSport {_framesMarkSpotHasBeenHeld}");


            if (_framesMarkSpotHasBeenHeld < GameManager.DEFAULT_FRAMERATE / 5 && _framesMarkSpotHasBeenHeld > -1)
            {
                //if (!player.playerInventory.isDualWielding)
                {
                    RaycastHit hit;


                    if (Physics.Raycast(camScript.transform.position, camScript.transform.TransformDirection(Vector3.forward), out hit, 100, GameManager.instance.markLayerMask))
                    {
                        Debug.Log($"Did Hit {hit.point} {hit.collider.name}");

                        if (hit.collider.transform.root.GetComponent<Player>())
                        {
                            if (GameManager.instance.teamMode == GameManager.TeamMode.None)
                                MarkerManager.instance.SpawnEnnSpotMarker(hit.point, player.playerId);
                            else
                            {
                                if (hit.collider.transform.root.GetComponent<Player>().team != player.team)
                                    PV.RPC("MarkSpot_RPC", RpcTarget.AllViaServer, hit.point, (int)player.team, true);
                                else
                                    PV.RPC("MarkSpot_RPC", RpcTarget.AllViaServer, hit.point, (int)player.team, false);
                            }
                        }
                        else
                        {
                            if (GameManager.instance.teamMode == GameManager.TeamMode.None)
                                MarkerManager.instance.SpawnNormalMarker(hit.point, player.photonId);
                            else
                                PV.RPC("MarkSpot_RPC", RpcTarget.AllViaServer, hit.point, (int)player.team, false);
                        }
                    }
                }
                //else
                //{
                //    if (player.playerInventory.thirdWeapon.loadedAmmo < player.playerInventory.thirdWeapon.ammoCapacity)
                //        ReloadThirdWeapon();
                //}
            }


            _framesMarkSpotHasBeenHeld = 0;
        }
    }




    [SerializeField] float _completeReloadTimer_thirdWeapon, _currentlyReloadingTimer_thirdWeapon;




    public void SetDrawingThirdWeapon()
    {
        _drawingWeaponTime_thirdWeapon = 1.1f;
    }



    [PunRPC]
    void MarkSpot_RPC(Vector3 pointt, int teamm, bool enSpot)
    {
        print("MarkSpot_RPC");


        if (enSpot)
        {

            if (GameManager.instance.connection == GameManager.Connection.Online)
            {
                foreach (Player p in GameManager.instance.GetAllPhotonPlayers())
                    if (p.team == (GameManager.Team)teamm && p.isMine)
                        MarkerManager.instance.SpawnEnnSpotMarker(pointt, p.PV.ViewID);
            }
            else if (GameManager.instance.connection == GameManager.Connection.Local)
            {
                foreach (Player p in GameManager.GetLocalPlayers())
                    if (p.team == (GameManager.Team)teamm && p.isMine)
                        MarkerManager.instance.SpawnEnnSpotMarker(pointt, p.PV.ViewID);
            }
        }
        else
        {
            if (GameManager.instance.connection == GameManager.Connection.Online)
            {

                foreach (Player p in GameManager.instance.GetAllPhotonPlayers())
                    if (p.team == (GameManager.Team)teamm && p.isMine)
                    {
                        MarkerManager.instance.SpawnNormalMarker(pointt, p.PV.ViewID);
                    }
            }
            else if (GameManager.instance.connection == GameManager.Connection.Local)
            {
                foreach (Player p in GameManager.GetLocalPlayers())
                    if (p.team == (GameManager.Team)teamm && p.isMine)
                    {
                        MarkerManager.instance.SpawnNormalMarker(pointt, p.PV.ViewID);
                    }
            }
        }
    }







    [PunRPC]
    void SpawnGrenade_RPC(bool fga, int ind, Vector3 sp, Quaternion sr, Vector3 forw)
    {
        Debug.Log($"SpawnGrenade_RPC {ind}");
        DisableSprint_RPC();
        player.PlayThrowingGrenadeClip();

        GameObject nade = GrenadePool.GetGrenade(fga, ind);
        nade.transform.position = sp; nade.transform.rotation = sr;
        nade.GetComponent<ExplosiveProjectile>().player = GetComponent<Player>();
        nade.GetComponent<ExplosiveProjectile>().IgnoreTheseCollidersFor1Second(GetComponent<Player>().hitboxes.Select(x => x.GetComponent<Collider>()).ToList());

        //foreach (PlayerHitbox hb in GetComponent<Player>().hitboxes)
        //    Physics.IgnoreCollision(nade.GetComponent<Collider>(), hb.GetComponent<Collider>()); // Prevents the grenade from colliding with the player who threw it


        nade.GetComponent<Rigidbody>().useGravity = true;
        nade.GetComponent<Rigidbody>().isKinematic = false;
        nade.GetComponent<Rigidbody>().velocity = Vector3.zero;
        nade.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        nade.SetActive(true);

        //int _throwF = (int)grenadeThrowForce; if (!fga) _throwF /= 10;
        foreach (Player p in GameManager.instance.GetAllPhotonPlayers())
            if (p != null)
                Physics.IgnoreCollision(nade.GetComponent<Collider>(), p.playerCapsule.GetComponent<Collider>());

        nade.GetComponent<Rigidbody>().AddForce(forw * grenadeThrowForce);

    }




    [PunRPC]
    public void UpdateIsMoving(bool nv, bool isCaller = true)
    {
        if (player.isMine && isCaller)
        {
            PV.RPC("UpdateIsMoving", RpcTarget.All, nv, false);
        }
        else
        {
            movement.UpdateIsMoving(nv);
        }
    }
}


