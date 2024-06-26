using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityEngine.SceneManagement;
using Photon.Pun;
using UnityEngine.EventSystems;
using ExitGames.Client.Photon.StructWrapping;
using System.Security.Cryptography;
using Photon.Realtime;
using Newtonsoft.Json.Bson;

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
    public PlayerMovement movement;
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
    public bool holstered, isRunning, isWalking;
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
    public bool isHoldingShootBtn { get { return _isHoldingShootBtn; } set { _isHoldingShootBtn = value; print($"isHoldingShootBtn {value}"); } }
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


    public bool isMeleeing { get { return _isMeleeing; } set { _isMeleeing = value; if (value) _meleeCooldown = 0.9f; } }
    public bool cameraisFloating { get { return _cameraIsFloating; } }
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


    bool _isMeleeing, _cameraIsFloating;
    float _meleeCooldown, _adsCounter;
    Transform _mainCamParent;
    Vector3 _lastMainCamLocalPos;
    Quaternion _lasMainCamLocalQuat;
    LayerMask _lastMainCamLayerMask;


    bool _isHoldingShootBtn;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        _mainCamParent = mainCam.transform.parent;
        _playerThirdPersonModelManager = GetComponent<PlayerThirdPersonModelManager>();
        OnPlayerTestButton += OnTestButton_Delegate;
    }
    public void Start()
    {
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

        try { weaponAnimator = pInventory.activeWeapon.GetComponent<Animator>(); } catch { }

        if (!GetComponent<Player>().isDead && !GetComponent<Player>().isRespawning && !pauseMenuOpen)
        {
            _disableSprintRPCCooldown -= Time.deltaTime;

            if (GameManager.instance.gameStarted)
            {
                Shooting();
                if (!isSprinting)
                {
                    LeftShooting();
                    CheckReloadButton();
                    CheckAmmoForAutoReload();
                    AnimationCheck();
                }
            }
        }

        if (PV.IsMine)
        {
            StartButton();
            BackButton();

            if (isAiming)
                currentadsCounter += Time.deltaTime * 6;
            else
                currentadsCounter -= Time.deltaTime * 6;

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
                    if (isSprinting)
                        return;
                    ScopeIn();
                    Melee();
                    Crouch();
                    Grenade(); //TO DO: Spawn Grenades the same way as bullets
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
        if (Input.GetKeyDown(KeyCode.Alpha9) && GameManager.instance.gameMode == GameManager.GameMode.Swarm)
        {
            player.isInvincible = !player.isInvincible;
            GetComponent<PlayerUI>().invincibleIcon.SetActive(player.isInvincible);
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
        if (GameManager.instance.gameType == GameManager.GameType.Oddball && player.playerInventory.playerOddballActive) return;

        if (rewiredPlayer.GetButtonShortPressDown("Interact"))
        {
            OnPlayerLongInteract?.Invoke(this);
        }
    }

    //TODO Make the player controller handle the third person script and models instead of the movement script
    void Sprint()
    {
        if (isHoldingShootBtn) return;
        if (isSprinting)
        {
            weaponAnimator.SetBool("Run", true);

            _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Rifle Sprint", false);
            _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Pistol Sprint", false);


            if (pInventory.activeWeapon.weaponType != WeaponProperties.WeaponType.Pistol)
                _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Rifle Sprint", true);
            else
                _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Pistol Sprint", true);

            _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Idle Rifle", true);
            _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Idle Pistol", true);
        }


        if (movement.movementDirection == PlayerMovement.PlayerMovementDirection.Forward)
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


        _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Rifle Sprint", false);
        _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Pistol Sprint", false);


        if (pInventory.activeWeapon.weaponType != WeaponProperties.WeaponType.Pistol)
            _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Rifle Sprint", true);
        else
            _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Pistol Sprint", true);

        _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Idle Rifle", false);
        _playerThirdPersonModelManager.thirdPersonScript.animator.SetBool("Idle Pistol", false);
        //GetComponent<Player>().playerVoice.volume = 0.1f;
        //GetComponent<Player>().PlaySprintingSound();
    }

    float _disableSprintRPCCooldown;

    void DisableSprint()
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
        isSprinting = false;
        weaponAnimator.SetBool("Run", false);

        GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Rifle Sprint", false);
        GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Pistol Sprint", false);

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
    void _StopShoot()
    {
        if (PV.IsMine)
            PV.RPC("_StopShoot_RPC", RpcTarget.All);
    }

    [PunRPC]
    void _StartShoot_RPC(Vector3 bipedOrSpp)
    {

        isHoldingShootBtn = true;
        if (!pInventory.activeWeapon.isOutOfAmmo && !isReloading &&
            !isHoldingShootBtn && !isInspecting && !isMeleeing && !isThrowingGrenade)
        {
            Debug.Log("_StartShoot_RPC");
            try
            {
                Debug.Log(GameManager.instance.orSpPos_Biped_Dict[bipedOrSpp]);
            }
            catch { }
            player.playerShooting.trackingTarget = null; if (bipedOrSpp != Vector3.zero) player.playerShooting.trackingTarget = GameManager.instance.orSpPos_Biped_Dict[bipedOrSpp];
            holstered = false;
            weaponAnimator.SetBool("Holster", false);
            GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Holster Rifle", false);

        }
        else
        {
            //isHoldingShootBtn = false;
        }
        Debug.Log($"{GetComponent<Player>().username}: _StartShoot_RPC {isHoldingShootBtn}");
    }

    [PunRPC]
    void _StopShoot_RPC()
    {
        if (player.isMine)
        {
            isHoldingShootBtn = false;
            OnPlayerFireButtonUp?.Invoke(this);
            Debug.Log($"{GetComponent<Player>().username}: _StopShoot_RPC {isHoldingShootBtn}");
        }
        else
        {
            StartCoroutine(StopShoot_Coroutine());
        }
    }

    IEnumerator StopShoot_Coroutine()
    {
        yield return new WaitForEndOfFrame();
        isHoldingShootBtn = false;
        OnPlayerFireButtonUp?.Invoke(this);
        Debug.Log($"{GetComponent<Player>().username}: _StopShoot_RPC {isHoldingShootBtn}");
    }

    void Shooting()
    {
        if (GetComponent<Player>().isDead || player.isRespawning)
            return;



        if ((rewiredPlayer.GetButtonDown("Shoot") || rewiredPlayer.GetButton("Shoot")) && !isHoldingShootBtn)
        {
            DisableSprint();

            isHoldingShootBtn = true;
            if (pInventory.activeWeapon.codeName.Equals("oddball"))
            {

                if (player.isMine) Melee(true);

                return;
            }
            else
            {
                print($"{player.name} _StartShoot");
                _StartShoot();
            }
        }

        if (isHoldingShootBtn && !pInventory.activeWeapon.codeName.Equals("oddball"))
        {
            print($"{player.name} OnPlayerFire");
            OnPlayerFire?.Invoke(this);
        }

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



    float _leftShootCooldown;
    void LeftShooting()
    {
        if (!isDualWielding)
            return;
        if (GetComponent<Player>().isDead || isSprinting || player.isRespawning)
            return;

        if (_leftShootCooldown > 0)
        {
            _leftShootCooldown -= Time.deltaTime;
        }
        else
        {
            if (rewiredPlayer.GetButton("Aim"))
            {
                pInventory.activeWeapon.leftWeapon.animator.Play("Fire", 0, 0f);
                _leftShootCooldown = 0.1f;
            }
        }
    }

    float _tempFov;
    void ScopeIn()
    {
        if (isDualWielding || pInventory.activeWeapon.scopeMagnification == WeaponProperties.ScopeMagnification.None)
            return;

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


                    PV.RPC("ToggleGlint_RPC", RpcTarget.AllViaServer, isAiming);



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
                    PV.RPC("ToggleGlint_RPC", RpcTarget.AllViaServer, isAiming);
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
        //mainCam.fieldOfView = GetComponent<Player>().defaultVerticalFov;
        //gunCam.fieldOfView = 60;
        //uiCam.fieldOfView = GetComponent<Player>().defaultVerticalFov;
        camScript.backEndMouseSens = camScript.frontEndMouseSens;
        allPlayerScripts.aimingScript.playAimSound();

        mainCam.transform.localRotation = Quaternion.Euler(0, 0, 0);
        gunCam.enabled = true;
        //PV.RPC("ToggleGlint_RPC", RpcTarget.AllViaServer, isAiming);
    }

    int _meleeCount = 0;
    float meleeMovementFactor = 0;
    void Melee(bool overwrite = false)
    {
        if (overwrite)
        {
            isMeleeing = true;
            //_meleeCount = melee.playersInMeleeZone.Count;
            //meleeMovementFactor = 1;

            rScript.reloadIsCanceled = true;

            PV.RPC("Melee_RPC", RpcTarget.All);
        }
        else if (!GetComponent<Player>().isDead)
        {
            if ((rewiredPlayer.GetButtonDown("Melee") || rewiredPlayer.GetButtonDown("MouseBtn4")) && !isMeleeing &&
                !isHoldingShootBtn && /*!isFiring &&*/ !isThrowingGrenade && !isSprinting)
            {
                isMeleeing = true;
                //_meleeCount = melee.playersInMeleeZone.Count;
                //meleeMovementFactor = 1;

                rScript.reloadIsCanceled = true;

                PV.RPC("Melee_RPC", RpcTarget.All);
            }
        }

        //if (meleeMovementFactor > 0)
        //{
        //    if (_meleeCount > 0)
        //    {
        //        Vector3 move = transform.forward * meleeMovementFactor;
        //        GetComponent<CharacterController>().Move(move * movement.defaultMaxSpeed * 6 * Time.deltaTime);
        //    }

        //    meleeMovementFactor -= Time.deltaTime * 5f;

        //    if (meleeMovementFactor <= 0)
        //    {
        //        _meleeCount = 0;
        //        meleeMovementFactor = 0;
        //    }
        //}
    }


    [PunRPC]
    void Melee_RPC()
    {
        print("Melee_RPC");
        if (PV.IsMine)
        {
            ScopeOut();
            melee.Knife();
        }
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
        movement.playerCapsule.localScale = new Vector3(transform.localScale.x, movement.crouchYScale, transform.localScale.z);

        if (movement.isGrounded)
            movement.rb.AddForce(Vector3.down * 2f, ForceMode.Impulse);
    }

    [PunRPC]
    void DisableCrouch_RPC()
    {
        movement.playerCapsule.localScale = new Vector3(transform.localScale.x, 1, transform.localScale.z);
    }



    void Grenade()
    {
        if ((rewiredPlayer.GetButtonDown("Throw Grenade") || rewiredPlayer.GetButtonDown("MouseBtn5")) && !isDualWielding &&
            !isHoldingShootBtn && /*!isFiring &&*/ !isMeleeing && !isSprinting /* && !isInspecting */)
        {
            if (pInventory.fragGrenades > 0 && !isThrowingGrenade)
            {
                rScript.reloadIsCanceled = true;
                ScopeOut();
                pInventory.fragGrenades = pInventory.fragGrenades - 1;
                weaponAnimator.Play("GrenadeThrow", 0, 0.0f);
                StartCoroutine(GrenadeSpawnDelay());
                PV.RPC("ThrowGrenade3PS_RPC", RpcTarget.All);
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
                if (pInventory.activeWeapon.loadedAmmo <= 0)
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
            if (pInventory.activeWeapon.loadedAmmo <= 0)
            {
                rScript.CheckAmmoTypeType(true);

            }

            if (pInventory.leftWeaponCurrentAmmo <= 0)
            {
                rScript.CheckAmmoTypeType(true, pInventory.leftWeapon);
            }
        }


        if (pInventory.activeWeapon.loadedAmmo <= 0 && pInventory.holsteredWeapon.loadedAmmo <= 0)
            player.PlayOutOfAmmoClip();
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
        if (GameManager.instance.gameType != GameManager.GameType.GunGame &&
            GameManager.instance.gameType != GameManager.GameType.Shotguns &&
            GameManager.instance.gameType != GameManager.GameType.Snipers &&
            GameManager.instance.gameType != GameManager.GameType.PurpleRain)
            if (rewiredPlayer.GetButtonDown("Switch Weapons"))
            {
                try
                {
                    Debug.Log("SwitchWeapons");
                    weaponAnimator = pInventory.activeWeapon.GetComponent<Animator>();
                    OnPlayerSwitchWeapons?.Invoke(this);
                }
                catch { }
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

    void FloatingCamera()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            if (GameManager.instance.localPlayers.Count == 1)
                if (!cameraisFloating)
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
                }
        }
    }

    [PunRPC]
    void SwitchGrenades_RPC()
    {
        if (fragGrenadesActive)
        {
            fragGrenadesActive = false;
            stickyGrenadesActive = true;

            //GetComponent<PlayerUI>().plasmaGrenadeBox.SetActive(false);
            GetComponent<PlayerUI>().plasmaGrenadeImage.gameObject.SetActive(true);
            GetComponent<PlayerUI>().plasmaGrenadeImage.GetComponent<IncreaseScaleThenScaleBackInTime>().Trigger();
            GetComponent<PlayerUI>().fragGrenadeImage.gameObject.SetActive(false);
            //GetComponent<PlayerUI>().fragGrenadeBox.SetActive(true);
        }
        else if (stickyGrenadesActive)
        {
            fragGrenadesActive = true;
            stickyGrenadesActive = false;

            //GetComponent<PlayerUI>().fragGrenadeBox.SetActive(true);
            //GetComponent<PlayerUI>().plasmaGrenadeBox.SetActive(false);

            GetComponent<PlayerUI>().plasmaGrenadeImage.gameObject.SetActive(false);
            GetComponent<PlayerUI>().fragGrenadeImage.gameObject.SetActive(true);
            GetComponent<PlayerUI>().fragGrenadeImage.GetComponent<IncreaseScaleThenScaleBackInTime>().Trigger();
        }
    }

    void HolsterAndInspect()
    {
        if (Input.GetKeyDown(KeyCode.Z))
            holstered = !holstered;

        //Holster anim toggle
        if (weaponAnimator)
        {
            weaponAnimator.SetBool("Holster", holstered);
            GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Holster Rifle", holstered);
        }

        //if (holstered == true)
        //{
        //    if (weaponAnimator)
        //    {
        //        weaponAnimator.SetBool("Holster", true);
        //        GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Holster Rifle", true);

        //    }
        //}
        //else
        //{
        //    if (weaponAnimator)
        //    {
        //        weaponAnimator.SetBool("Holster", false);
        //        GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().SetBool("Holster Rifle", false);

        //    }
        //}


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
        yield return new WaitForSeconds(0.2f);
        //Spawn grenade prefab at spawnpoint

        PV.RPC("SpawnGrenade_RPC", RpcTarget.All, fragGrenadesActive, GrenadePool.GetAvailableGrenadeIndex(fragGrenadesActive, player.playerDataCell.photonRoomIndex), gwProperties.grenadeSpawnPoint.position,
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
            pInventory.activeWeapon.loadedAmmo += 1;
        }
        else
        {
            ammoWeaponIsMissing = pInventory.activeWeapon.ammoCapacity - pInventory.activeWeapon.loadedAmmo;

            if (pInventory.activeWeapon.spareAmmo >= ammoWeaponIsMissing)
            {
                pInventory.activeWeapon.loadedAmmo = pInventory.activeWeapon.ammoCapacity;
                pInventory.activeWeapon.spareAmmo -= ammoWeaponIsMissing;
            }
            else if (pInventory.activeWeapon.spareAmmo < ammoWeaponIsMissing)
            {
                pInventory.activeWeapon.loadedAmmo += pInventory.activeWeapon.spareAmmo;
                pInventory.activeWeapon.spareAmmo = 0;
            }

            if (pInventory.leftWeapon)
            {
                ammoWeaponIsMissing = pInventory.leftWeapon.ammoCapacity - pInventory.leftWeapon.loadedAmmo;

                if (pInventory.leftWeapon.spareAmmo >= ammoWeaponIsMissing)
                {
                    pInventory.leftWeapon.loadedAmmo = pInventory.leftWeapon.ammoCapacity;
                    pInventory.leftWeapon.spareAmmo -= ammoWeaponIsMissing;
                }
                else if (pInventory.leftWeapon.spareAmmo < ammoWeaponIsMissing)
                {
                    pInventory.leftWeapon.loadedAmmo += pInventory.leftWeapon.spareAmmo;
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
                Cursor.lockState = CursorLockMode.None; // Must Unlock Cursor so it can detect buttons
                Cursor.visible = true;
                GetComponent<PlayerUI>().singlePlayerPauseMenu.gameObject.SetActive(true);
                pauseMenuOpen = true;
            }
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

        CurrentRoomManager.instance.leftRoomManually = true;

        GameManager.instance.previousScenePayloads.Add(GameManager.PreviousScenePayload.OpenCarnageReportAndCredits);
        GameManager.instance.previousScenePayloads.Add(GameManager.PreviousScenePayload.ResetPlayerDataCells);


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
        isSprinting = false;
        isHoldingShootBtn = false;
    }

    void OnRespawn_Delegate(Player player)
    {
        isSprinting = false;
        isHoldingShootBtn = false;
    }











    [PunRPC]
    void SpawnGrenade_RPC(bool fga, int ind, Vector3 sp, Quaternion sr, Vector3 forw)
    {
        Debug.Log($"SpawnGrenade_RPC {ind}");

        player.PlayThrowingGrenadeClip();

        GameObject nade = GrenadePool.GetGrenade(fga, ind);
        nade.transform.position = sp; nade.transform.rotation = sr;
        nade.GetComponent<ExplosiveProjectile>().player = GetComponent<Player>();

        foreach (PlayerHitbox hb in GetComponent<Player>().hitboxes)
            Physics.IgnoreCollision(nade.GetComponent<Collider>(), hb.GetComponent<Collider>()); // Prevents the grenade from colliding with the player who threw it


        nade.GetComponent<Rigidbody>().useGravity = true;
        nade.GetComponent<Rigidbody>().isKinematic = false;
        nade.GetComponent<Rigidbody>().velocity = Vector3.zero;
        nade.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        nade.SetActive(true);

        //int _throwF = (int)grenadeThrowForce; if (!fga) _throwF /= 10;
        foreach (Player p in GameManager.instance.pid_player_Dict.Values) Physics.IgnoreCollision(nade.GetComponent<Collider>(), p.playerCapsule.GetComponent<Collider>());
        nade.GetComponent<Rigidbody>().AddForce(forw * grenadeThrowForce);
    }


    [PunRPC]
    void ToggleGlint_RPC(bool fga)
    {
        try { pInventory.activeWeapon.glint.SetActive(fga); } catch { }
    }

}


