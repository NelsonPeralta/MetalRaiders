using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityEngine.SceneManagement;
using Photon.Pun;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviourPun
{
    [Header("Other Scripts")]
    public AllPlayerScripts allPlayerScripts;
    public WeaponSounds weapSounds;
    public PlayerProperties playerProperties;
    public PlayerInventory pInventory;
    public GeneralWeapProperties gwProperties;
    public WeaponProperties wProperties;
    public Animator anim;
    public Camera mainCam;
    public Camera gunCam;
    public CameraScript camScript;
    public FullyAutomaticFire fullyAutomaticFire;
    public FullyAutomaticFireLeft fullyAutomaticFireLeft;
    public BurstFire burstFire;
    public SingleFire singleFire;
    public FPSControllerLPFP.FpsControllerLPFP notMyFPSController;
    public Rewired.Player player;
    public int playerRewiredID;
    public CrosshairScript crosshairScript;
    public ReloadScript rScript;
    public WeaponPickUp wPickup;
    public DualWieldingReload dwReload;
    public Movement movement;
    public Melee melee;
    public ThirdPersonScript tPersonController;
    public ControllerType lastControllerType;

    public PhotonView PV;
    public PlayerManager playerManager;
    public GameObjectPool objectPool;

    Quaternion savedCamRotation;

    [HideInInspector]
    public bool hasBeenHolstered = false, holstered, isRunning, isWalking;
    [HideInInspector]
    public bool isInspecting, isShooting, aimSoundHasPlayed = false, hasFoundComponents = false;

    public bool isReloading, reloadAnimationStarted, reloadWasCanceled, isFiring,
        isAiming, isThrowingGrenade, isCrouching, isDrawingWeapon, isMeleeing, isSprinting;

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
    public bool isDualWielding;
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

    void Awake()
    {
        PV = GetComponent<PhotonView>();

        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
    }


    public void Start()
    {
        if (hasFoundComponents == false)
        {
            objectPool = GameObjectPool.gameObjectPoolInstance;
            SetPlayerIDInInput();
            StartCoroutine(FindComponents());
            ReferenceCameraToSpherecast();
        }

        if (PV.IsMine)
        {

        }
        else
        {
            gunCam.gameObject.SetActive(false);
            mainCam.gameObject.SetActive(false);
            allPlayerScripts.playerUIComponents.gameObject.SetActive(false);
        }



    }

    private IEnumerator FindComponents()
    {
        yield return new WaitForEndOfFrame();

        //aimingScript = childManager.FindChildWithTag("Scope BG").GetComponent<Aiming>();
        //sfxManager = childManager.FindChildWithTag("SFX").GetComponent<SFXManager>();
        //weapSounds = childManager.FindChildWithTag("Weapon Sounds").GetComponent<WeaponSounds>();

        //pInventory = childManager.FindChildWithTag("Player Inventory").GetComponent<PlayerInventory>();

        //pInventory = GameObject.FindGameObjectWithTag("Player Inventory").GetComponent<PlayerInventoryManager>();

        //playerProperties = GetComponent<PlayerProperties>();
        //gwProperties = GetComponent<GeneralWeapProperties>();
        //wProperties = childManager.FindChildWithTag("Weapon").GetComponent<WeaponProperties>();

        //fullyAutomaticFire = childManager.FindChildWithTagScript("Shooting Scripts").GetComponent<FullyAutomaticFire>();
        //burstFire = childManager.FindChildWithTagScript("Shooting Scripts").GetComponent<BurstFire>();
        //singleFire = childManager.FindChildWithTagScript("Shooting Scripts").GetComponent<SingleFire>();
        //playerProperties.SetTeamToFiringScripts();

        notMyFPSController = gameObject.GetComponent<FPSControllerLPFP.FpsControllerLPFP>();
        savedCamRotation = mainCam.transform.localRotation;

    }

    public void SetPlayerIDInInput()
    {
        player = ReInput.players.GetPlayer(playerRewiredID);
    }

    private void Update()
    {
        if (!PV.IsMine)
            return;

        //if (Input.GetMouseButtonDown(0))
        //{
        //    PV.RPC("RPC_Shoot_Projectile_Test", RpcTarget.All);
        //}

        UpdateWeaponPropertiesAndAnimator();
        if (playerProperties != null)
        {
            StartButton();
            BackButton();
            if (!playerProperties.isDead && !playerProperties.isRespawning)
            {

                if (!pauseMenuOpen)
                {
                    Sprint();
                    SwitchGrenades();
                    if (isSprinting)
                        return;
                    Shooting();
                    CheckReloadButton();
                    CheckAmmoForAutoReload();
                    Aiming();
                    //PV.RPC("Melee", RpcTarget.All);
                    Melee();
                    Crouch();
                    Grenade(); //TO DO: Spawn Grenades the same way as bullets
                    SelectFire();
                    //AutoReloadVoid();
                    HolsterAndInspect();
                    CheckDrawingWeapon();
                }
            }
        }

        AnimationCheck();

        //Debug.Log(wProperties.outOfAmmo);
        StartCoroutine(TestButton());
        if (ReInput.controllers != null)
            lastControllerType = ReInput.controllers.GetLastActiveControllerType();

    }


    /// <summary>
    /// ////////////////////////////////Updated Voids
    /// </summary>

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
                        wProperties = pInventory.weaponsEquiped[0].gameObject.GetComponent<WeaponProperties>();
                        anim = pInventory.weaponsEquiped[0].gameObject.GetComponent<Animator>();
                    }
                }

                else if (pInventory.activeWeapIs == 1)
                {
                    if (pInventory.weaponsEquiped[1] != null)
                    {
                        wProperties = pInventory.weaponsEquiped[1].gameObject.GetComponent<WeaponProperties>();
                        anim = pInventory.weaponsEquiped[1].gameObject.GetComponent<Animator>();
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

    void Sprint()
    {
        if (movement.direction == "Forward")
        {
            if (!movement.isGrounded || isReloading)
                return;
            if (lastControllerType == ControllerType.Keyboard || lastControllerType == ControllerType.Mouse)
            {
                if (player.GetButton("Sprint"))
                    EnableSprint();
                else if (player.GetButtonUp("Sprint"))
                    DisableSprint();
            }
            else if (lastControllerType == ControllerType.Joystick)
                if (player.GetButtonDown("Sprint"))
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
        anim.SetBool("Run", true);
        tPersonController.anim.SetBool("Sprint", true);
        tPersonController.anim.SetBool("Idle Rifle", false);
        tPersonController.anim.SetBool("Idle Pistol", false);
        playerProperties.playerVoice.volume = 0.1f;
        playerProperties.PlaySprintingSound();
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
        anim.SetBool("Run", false);
        tPersonController.anim.SetBool("Sprint", false);

        if (pInventory.activeWeapon.GetComponent<WeaponProperties>().pistolIdle)
        {
            tPersonController.anim.SetBool("Idle Pistol", true);
            tPersonController.anim.SetBool("Idle Rifle", false);
        }
        else
        {
            tPersonController.anim.SetBool("Idle Rifle", true);
            tPersonController.anim.SetBool("Idle Pistol", false);
        }

        playerProperties.StopPlayingPlayerVoice();
    }

    void Shooting()
    {
        if (playerProperties.isDead || isSprinting)
            return;

        if (!isDualWielding)
        {
            if (player.GetButton("Shoot") && !wProperties.outOfAmmo && !isReloading && !isShooting && !isInspecting && !isMeleeing && !isThrowingGrenade)
            {
                isShooting = true;

            }
            else
            {
                isShooting = false;
            }
        }

        if (isDualWielding)
        {
            if (player.GetButton("Shoot") && !dwRightWP.outOfAmmo && !isReloadingRight && !isShootingRight && !isMeleeing && !isThrowingGrenade && !isSprinting)
            {
                Debug.Log("Is Shooting Right");
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

    //[PunRPC]
    //void RPC_Shoot_Projectile_Test()
    //{
    //    GameObject bullet = objectPool.SpawnPooledBullet();

    //    bullet.transform.position = gameObject.transform.position;
    //    bullet.transform.rotation = gameObject.transform.rotation;
    //    bullet.SetActive(true);
    //}

    //[PunRPC]
    //public void ShootAutoTest()
    //{
    //    if (!PV.IsMine)
    //        return;
    //    if (wProperties.isFullyAutomatic && !isDualWielding && !isDrawingWeapon)
    //    {
    //        Debug.Log("Spawned Bullet and player is : " + wProperties.pController.name);
    //        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //        //Spawn bullet from bullet spawnpoint
    //        var bullet = objectPool.SpawnPooledBullet();
    //        bullet.transform.position = gwProperties.bulletSpawnPoint.transform.position;
    //        bullet.transform.rotation = gwProperties.bulletSpawnPoint.transform.rotation;

    //        bullet.gameObject.GetComponent<Bullet>().allPlayerScripts = this.allPlayerScripts;
    //        bullet.gameObject.GetComponent<Bullet>().range = wProperties.range;
    //        bullet.gameObject.GetComponent<Bullet>().playerRewiredID = playerRewiredID;
    //        bullet.gameObject.GetComponent<Bullet>().playerWhoShot = gwProperties.gameObject.GetComponent<PlayerProperties>().gameObject;
    //        bullet.gameObject.GetComponent<Bullet>().pInventory = pInventory;
    //        bullet.gameObject.GetComponent<Bullet>().raycastScript = playerProperties.raycastScript;
    //        bullet.gameObject.GetComponent<Bullet>().crosshairScript = playerProperties.cScript;
    //        //SetTeamToBulletScript(bullet.transform);
    //        bullet.SetActive(true);
    //    }
    //}

    void Aiming()
    {
        if (isAiming)
        {
            wProperties.RedReticuleRange = wProperties.aimRRR;
        }
        else
        {
            if (wProperties)
                if (wProperties.DefaultRedReticuleRange > 0)
                {
                    wProperties.RedReticuleRange = wProperties.DefaultRedReticuleRange;
                }
        }


        if (player.GetButtonDown("Aim") && !isReloading && !isRunning && !isInspecting)
        {
            if (wProperties.canAim)
            {
                if (isAiming == false)
                {
                    isAiming = true;
                    mainCam.fieldOfView = wProperties.aimFOV;

                    allPlayerScripts.aimingScript.playAimSound();

                    UpdateAimingLayers();
                }
                else
                {
                    isAiming = false;
                    mainCam.fieldOfView = playerProperties.defaultFov;
                    camScript.mouseSensitivity = camScript.defaultMouseSensitivy;

                    allPlayerScripts.aimingScript.playAimSound();


                    UpdateAimingLayers();
                }



            }

        }

        //if (movement.direction == "Forward")
        //{
        //    if (!movement.isGrounded)
        //        return;

        //    if (lastControllerType == ControllerType.Keyboard || lastControllerType == ControllerType.Mouse)
        //    {
        //        if (player.GetButton("Sprint"))
        //            EnableSprint();
        //        else if (player.GetButtonUp("Sprint"))
        //            DisableSprint();
        //    }
        //    else if (lastControllerType == ControllerType.Joystick)
        //        if (player.GetButtonDown("Sprint"))
        //            EnableSprint();
        //}
        //else
        //    DisableSprint();
    }

    public void ScopeIn()
    {

    }
    public void ScopeOut()
    {
        Debug.Log("Unscope Script");
        isAiming = false;
        mainCam.fieldOfView = playerProperties.defaultFov;
        camScript.mouseSensitivity = camScript.defaultMouseSensitivy;

        if (isAiming)
            allPlayerScripts.aimingScript.playAimSound();

        mainCam.transform.localRotation = Quaternion.Euler(0, 0, 0);
        //aimingComponentsPivot.transform.localRotation = Quaternion.Euler(7.5f, 0, 0);

        UpdateAimingLayers();
    }

    [PunRPC]
    void Melee()
    {
        if (!playerProperties.isDead)
        {
            if (player.GetButtonDown("Melee") && !isMeleeing && !isShooting && !isThrowingGrenade && !isSprinting)
            {
                Debug.Log("RPC Call: Melee");
                PV.RPC("Melee_RPC", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    void Melee_RPC()
    {
        melee.Knife();
        if (melee.playersInMeleeZone.Count > 0)
            meleeAudioSource.clip = melee.knifeSuccessSound;
        else
            meleeAudioSource.clip = melee.knifeFailSound;
        meleeAudioSource.Play();
        anim.Play("Knife Attack 2", 0, 0f);
        StartCoroutine(Melee3PS());
    }

    void Crouch()
    {
        if (player.GetButtonDown("Crouch"))
            EnableCrouch();
        else if (player.GetButtonUp("Crouch"))
            DisableCrouch();
    }

    void EnableCrouch()
    {
        Debug.Log("Crouching");
        movement.tPersonScripts.anim.SetBool("Jump", false);
        movement.tPersonScripts.anim.SetBool("Crouch", true);
        isCrouching = true;
        mainCam.GetComponent<Transform>().localPosition += new Vector3(0, -.35f, 0);
        gwProperties.bulletSpawnPoint.localPosition += new Vector3(0, -.35f, 0);
        GetComponent<CharacterController>().height = 1.25f;
        GetComponent<CharacterController>().center = new Vector3(0, -0.7f, 0);
    }

    public void DisableCrouch()
    {
        movement.tPersonScripts.anim.SetBool("Crouch", false);
        isCrouching = false;
        mainCam.GetComponent<Transform>().localPosition += new Vector3(0, .35f, 0);
        gwProperties.bulletSpawnPoint.localPosition = gwProperties.defaultBulletSpawnPoint;

        GetComponent<CharacterController>().height = 1.7f; // Default values on the prefab
        GetComponent<CharacterController>().center = new Vector3(0, -0.5f, 0); // Default values on the prefab
    }

    void Grenade()
    {
        if (player.GetButtonDown("Throw Grenade") && !isDualWielding && !isShooting && !isMeleeing && !isSprinting /* && !isInspecting */)
        {
            if (pInventory.grenades > 0 && !isThrowingGrenade)
            {
                ScopeOut();
                pInventory.grenades = pInventory.grenades - 1;
                anim.Play("GrenadeThrow", 0, 0.0f);
                PV.RPC("ThrowGrenade_RPC", RpcTarget.All);
                //StartCoroutine(GrenadeSpawnDelay());
                //StartCoroutine(ThrowGrenade3PS());
            }
        }

        if (isDualWielding)
        {
            if (player.GetButton("Throw Grenade") && !dwLeftWP.outOfAmmo && !isReloadingLeft && !isShootingLeft)
            {
                Debug.Log("Is Shooting Left");
                isShootingLeft = true;
            }
            else
            {
                isShootingLeft = false;
            }
        }
    }

    void CheckReloadButton()
    {
        if (!playerProperties.isDead)
        {
            if (player.GetButtonDown("Reload") && !isReloading && !wPickup.canPickupDW && !isDualWielding)
            {
                rScript.CheckAmmoTypeType(false);
            }
        }
    }

    void CheckAmmoForAutoReload()
    {
        if (!isDualWielding)
        {
            if (wProperties)
            {
                if (wProperties.currentAmmo <= 0)
                {
                    wProperties.outOfAmmo = true;
                    rScript.CheckAmmoTypeType(true);
                }
                else
                {
                    wProperties.outOfAmmo = false;
                }
            }
        }

        if (isDualWielding)
        {
            if (pInventory.rightWeaponCurrentAmmo <= 0)
            {
                pInventory.rightWeaponCurrentAmmo = 0;
                dwRightWP.outOfAmmo = true;
                dwReload.CheckAmmoTypeType(true, false);
            }
            else
            {
                dwRightWP.outOfAmmo = false;
            }

            if (pInventory.leftWeaponCurrentAmmo <= 0)
            {
                pInventory.leftWeaponCurrentAmmo = 0;
                dwLeftWP.outOfAmmo = true;
                dwReload.CheckAmmoTypeType(false, true);
            }
            else
            {
                dwLeftWP.outOfAmmo = false;
            }
        }
    }

    void ReloadVoid()
    {
        if (player.GetButtonDown("Reload"))
        {
            reloadWasCanceled = false;

            if (!isReloading && pInventory.activeWeapon.GetComponent<WeaponProperties>().smallAmmo && pInventory.smallAmmo != 0 /* && !isInspecting */)
            {
                if (pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo < pInventory.activeWeapon.GetComponent<WeaponProperties>().maxAmmoInWeapon)
                {

                    //Reload
                    //StartCoroutine(Reload());
                }
            }
            else if (!isReloading && pInventory.activeWeapon.GetComponent<WeaponProperties>().heavyAmmo && pInventory.heavyAmmo != 0 /* && !isInspecting */)
            {
                if (pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo < pInventory.activeWeapon.GetComponent<WeaponProperties>().maxAmmoInWeapon)
                {
                    //Reload
                    //StartCoroutine(Reload());
                }
            }
            else if (!isReloading && pInventory.activeWeapon.GetComponent<WeaponProperties>().powerAmmo && pInventory.powerAmmo != 0 /* && !isInspecting */)
            {
                if (pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo < pInventory.activeWeapon.GetComponent<WeaponProperties>().maxAmmoInWeapon)
                {
                    //Reload
                    //StartCoroutine(Reload());
                }
            }
        }
    }

    void CheckDrawingWeapon()
    {
        if (anim)
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Draw"))
                isDrawingWeapon = true;
            else
                isDrawingWeapon = false;
    }

    void SelectFire()
    {
        if (player.GetButtonDown("Select Fire"))
        {
            if (wProperties.canSelectFire)
            {
                if (wProperties.isFullyAutomatic) // Become Burst
                {
                    wProperties.isBurstWeapon = true;
                    wProperties.isFullyAutomatic = false;

                    wProperties.isNormalBullet = false;
                    wProperties.isHeadshotCapable = true;
                }
                else if (wProperties.isBurstWeapon) // Become Single Fire
                {
                    wProperties.isSingleFire = true;
                    wProperties.isBurstWeapon = false;

                    wProperties.isNormalBullet = false;
                    wProperties.isHeadshotCapable = true;
                }
                else if (wProperties.isSingleFire) // Become Full Auto
                {
                    wProperties.isFullyAutomatic = true;
                    wProperties.isSingleFire = false;

                    wProperties.isNormalBullet = true;
                    wProperties.isHeadshotCapable = false;
                }
            }
        }
    }

    void SwitchGrenades()
    {
        if (player.GetButtonDown("Switch Grenades") && PV.IsMine)
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

            allPlayerScripts.playerUIComponents.fragGrenadeIcon.SetActive(false);
            allPlayerScripts.playerUIComponents.stickyGrenadeIcon.SetActive(true);
        }
        else if (stickyGrenadesActive)
        {
            fragGrenadesActive = true;
            stickyGrenadesActive = false;

            allPlayerScripts.playerUIComponents.fragGrenadeIcon.SetActive(true);
            allPlayerScripts.playerUIComponents.stickyGrenadeIcon.SetActive(false);
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
            if (anim)
                anim.SetBool("Holster", true);
        }
        else
        {
            if (anim)
                anim.SetBool("Holster", false);
        }


        //Inspect weapon when T key is pressed
        ///////////////////////////////////////
        if (Input.GetKeyDown(KeyCode.T))
        {
            anim.Play("Reload Open", 0, 0f);
            //anim.SetTrigger("Inspect");
        }
    }

    private void AnimationCheck()
    {
        if (!isDualWielding)
        {
            if (wProperties != null)
            {
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Fire"))
                    isFiring = true;
                else
                    isFiring = false;

                if (wProperties.usesMags)
                {
                    //Check if reloading
                    //Check both animations
                    if (anim.GetCurrentAnimatorStateInfo(0).IsName("Reload Out Of Ammo") ||
                        anim.GetCurrentAnimatorStateInfo(0).IsName("Reload Ammo Left"))
                    {
                        isReloading = true;
                    }
                    else
                    {
                        isReloading = false;
                    }
                }

                if (wProperties.usesShells)
                {
                    //Check if reloading
                    //Check both animations
                    if (anim.GetCurrentAnimatorStateInfo(0).IsName("Reload Open") ||
                        anim.GetCurrentAnimatorStateInfo(0).IsName("Insert Shell") ||
                        anim.GetCurrentAnimatorStateInfo(0).IsName("Reload Close"))
                    {
                        isReloading = true;
                    }
                    else
                    {
                        isReloading = false;
                    }

                    //Check if inspecting weapon
                    if (anim.GetCurrentAnimatorStateInfo(0).IsName("Inspect"))
                    {
                        isInspecting = true;
                    }
                    else
                    {
                        isInspecting = false;
                    }
                }

                if (wProperties.usesRockets || wProperties.usesGrenades)
                {
                    if (anim.GetCurrentAnimatorStateInfo(0).IsName("Reload"))
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

        if (isDualWielding)
        {
            if (dwRightWP.usesMags)
            {
                //Check if reloading
                //Check both animations
                if (animDWRight.GetCurrentAnimatorStateInfo(0).IsName("Reload Out Of Ammo") ||
                    animDWRight.GetCurrentAnimatorStateInfo(0).IsName("Reload Ammo Left") ||
                    animDWRight.GetCurrentAnimatorStateInfo(0).IsName("Reload Ammo Left Take Out"))
                {
                    isReloadingRight = true;
                }
                else
                {
                    isReloadingRight = false;
                }
            }

            if (dwLeftWP.usesMags)
            {
                //Check if reloading
                //Check both animations
                if (animDWLeft.GetCurrentAnimatorStateInfo(0).IsName("Reload Out Of Ammo") ||
                    animDWLeft.GetCurrentAnimatorStateInfo(0).IsName("Reload Ammo Left") ||
                    animDWLeft.GetCurrentAnimatorStateInfo(0).IsName("Reload Ammo Left Take Out"))
                {
                    isReloadingLeft = true;
                }
                else
                {
                    isReloadingLeft = false;
                }
            }
        }

        if (anim != null)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("GrenadeThrow"))
            {
                isThrowingGrenade = true;
            }
            else
            {
                isThrowingGrenade = false;
            }
        }

        if (anim != null)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Knife Attack 2"))
            {
                isMeleeing = true;
            }
            else
            {
                isMeleeing = false;
            }
        }
    }

    IEnumerator ThrowGrenade3PS()
    {
        tPersonController.anim.Play("Throw Grenade");
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
        tPersonController.anim.Play("Reload");
        yield return new WaitForEndOfFrame();
    }

    IEnumerator Melee3PS()
    {
        tPersonController.anim.Play("Melee");
        StartCoroutine(ShowMeleeKnife());
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
    public void ThrowGrenade_RPC()
    {
        StartCoroutine(GrenadeSpawnDelay());
        StartCoroutine(ThrowGrenade3PS());
    }

    private IEnumerator GrenadeSpawnDelay()
    {

        //Wait for set amount of time before spawning grenade
        yield return new WaitForSeconds(pInventory.grenadeSpawnDelay);
        //Spawn grenade prefab at spawnpoint

        var grenade = Instantiate(pInventory.grenadePrefab);
        Destroy(grenade.gameObject);

        if (fragGrenadesActive)
        {
            grenade = Instantiate(pInventory.grenadePrefab,
               gwProperties.grenadeSpawnPoint.transform.position,
               gwProperties.grenadeSpawnPoint.transform.rotation);
            grenade.GetComponent<FragGrenade>().playerWhoThrewGrenade = playerProperties;
            grenade.GetComponent<FragGrenade>().playerRewiredID = playerRewiredID;
            //grenade.GetComponent<FragGrenade>().team = allPlayerScripts.playerMPProperties.team;
        }
        else if (stickyGrenadesActive)
        {
            grenade = Instantiate(pInventory.stickyGrenadePrefab,
               gwProperties.grenadeSpawnPoint.transform.position,
               gwProperties.grenadeSpawnPoint.transform.rotation);
            grenade.GetComponent<StickyGrenade>().playerWhoThrewGrenade = playerProperties;
            grenade.GetComponent<StickyGrenade>().playerRewiredID = playerRewiredID;
            //grenade.GetComponent<StickyGrenade>().team = allPlayerScripts.playerMPProperties.team;
        }

        foreach (GameObject hb in playerProperties.hitboxes)
            Physics.IgnoreCollision(grenade.GetComponent<Collider>(), hb.GetComponent<Collider>()); // Prevents the grenade from colliding with the player who threw it

        grenade.GetComponent<Rigidbody>().AddForce(gwProperties.grenadeSpawnPoint.transform.forward * grenadeThrowForce);
        Destroy(grenade.gameObject, 10);
    }

    //Reload

    /*
IEnumerator Reload()
{
    if (wProperties.usesMags)
    {
        wProperties.mainAudioSource.clip = wProperties.Reload_1;
        wProperties.mainAudioSource.Play();

        //Play diff anim if ammo left
        anim.Play("Reload Ammo Left", 0, 0f);

        sfxManager.mainAudioSource.clip = weapSounds.reloadSoundAmmoLeft;
        sfxManager.mainAudioSource.Play();

        //If reloading when ammo left, show bullet in mag
        //Do not show if bullet renderer is not assigned in inspector
        if (gwProperties.bulletInMagRenderer != null)
        {
            gwProperties.bulletInMagRenderer.GetComponent
            <SkinnedMeshRenderer>().enabled = true;
        }

        //Restore ammo when reloading

        yield return new WaitForSeconds(2);

        StartCoroutine(TransferAmmo());



        /*wProperties.currentAmmo = wProperties.ammo;
        wProperties.ammo = 1;*/

    /*

        if (wProperties.usesShells)
        {
            if (wProperties.currentAmmo == 1)
            {
                anim.Play("Reload Open (7 Case)", 0, 0f);
                yield return new WaitForSeconds(7f);
                StartCoroutine(TransferAmmo());
            }
            else if (wProperties.currentAmmo == 2)
            {
                anim.Play("Reload Open (6 Case)", 0, 0f);
                yield return new WaitForSeconds(6f);
                StartCoroutine(TransferAmmo());
            }
            else if (wProperties.currentAmmo == 3)
            {
                anim.Play("Reload Open (5 Case)", 0, 0f);
                yield return new WaitForSeconds(5f);
                StartCoroutine(TransferAmmo());
            }
            else if (wProperties.currentAmmo == 4)
            {
                anim.Play("Reload Open (4 Case)", 0, 0f);
                yield return new WaitForSeconds(4f);
                StartCoroutine(TransferAmmo());
            }
            else if (wProperties.currentAmmo == 5)
            {
                anim.Play("Reload Open (3 Case)", 0, 0f);
                yield return new WaitForSeconds(3f);
                StartCoroutine(TransferAmmo());
            }
            else if (wProperties.currentAmmo == 6)
            {
                anim.Play("Reload Open (2 Case)", 0, 0f);
                yield return new WaitForSeconds(2f);
                StartCoroutine(TransferAmmo());
            }
            else if (wProperties.currentAmmo == 7)
            {
                anim.Play("Reload Open (1 Case)", 0, 0f);
                yield return new WaitForSeconds(1f);
                StartCoroutine(TransferAmmo());
            }
            //Restore ammo when reloading
            //wProperties.currentAmmo = wProperties.ammo;
        }
    }
    */
    /*
    private IEnumerator AutoReload()
    {
        //Wait set amount of time
        // return new WaitForSeconds(autoReloadDelay); This Line Causes lag with animation when starting

        if (wProperties.usesMags)
        {

            //Play diff anim if out of ammo
            anim.Play("Reload Out Of Ammo", 0, 0f);

            wProperties.mainAudioSource.clip = wProperties.Reload_2;
            wProperties.mainAudioSource.Play();

            //If out of ammo, hide the bullet renderer in the mag
            //Do not show if bullet renderer is not assigned in inspector
            if (gwProperties.bulletInMagRenderer != null)
            {
                gwProperties.bulletInMagRenderer.GetComponent
                <SkinnedMeshRenderer>().enabled = false;
                //Start show bullet delay
                //StartCoroutine(gwProperties.ShowBulletInMag());
            }

            //Restore ammo when reloading
            //yield return new WaitForSeconds(4);
            //wProperties.currentAmmo = wProperties.ammo;
            //wProperties.outOfAmmo = false;

            yield return new WaitForSeconds(2f);
            StartCoroutine(TransferAmmo());
        }

        if (wProperties.usesShells && wProperties.outOfAmmo == true)
        {
            anim.Play("Reload Open", 0, 0f);
            yield return new WaitForSeconds(6.5f);
            StartCoroutine(TransferAmmo());

        }

        if (wProperties.usesGrenades && wProperties.outOfAmmo == true)
        {
            anim.Play("Reload", 0, 0f);
            //yield return new WaitForSeconds(2f);
            StartCoroutine(TransferAmmo());

        }

        if (wProperties.usesRockets && wProperties.outOfAmmo == true)
        {
            anim.Play("Reload", 0, 0f);
            //yield return new WaitForSeconds(2f);
            StartCoroutine(TransferAmmo());

        }
        /*
    if(wProperties.usesSingleAmmo)
    {
        if (wProperties.outOfAmmo == true)
        {
            //Play diff anim if out of ammo
            anim.Play("Reload", 0, 0f);


        }
        //Restore ammo when reloading
        wProperties.currentAmmo = wProperties.ammo;
    }*/






    IEnumerator ChangeCamRotation()
    {
        yield return new WaitForEndOfFrame();

        //cam.gameObject.transform.localRotation = Quaternion.Euler(-4.8f, 0, 0);
    }

    public void TransferAmmo()
    {

        if (wProperties.smallAmmo)
        {
            if (wProperties.usesShells)
            {
                pInventory.smallAmmo = pInventory.smallAmmo - 1;
                pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo = pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo + 1;
            }


            else
            {
                ammoWeaponIsMissing = pInventory.activeWeapon.GetComponent<WeaponProperties>().maxAmmoInWeapon - pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo;

                if (pInventory.smallAmmo >= ammoWeaponIsMissing)
                {
                    pInventory.smallAmmo = pInventory.smallAmmo - ammoWeaponIsMissing;
                    pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo = pInventory.activeWeapon.GetComponent<WeaponProperties>().maxAmmoInWeapon;
                }
                else if (pInventory.smallAmmo < ammoWeaponIsMissing)
                {
                    pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo = pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo + pInventory.smallAmmo;
                    pInventory.smallAmmo = 0;
                }
            }

        }

        else if (wProperties.heavyAmmo)
        {
            if (wProperties.usesShells)
            {
                pInventory.heavyAmmo = pInventory.heavyAmmo - 1;
                pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo = pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo + 1;
            }

            else
            {
                ammoWeaponIsMissing = pInventory.activeWeapon.GetComponent<WeaponProperties>().maxAmmoInWeapon - pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo;

                if (pInventory.heavyAmmo >= ammoWeaponIsMissing)
                {
                    pInventory.heavyAmmo = pInventory.heavyAmmo - ammoWeaponIsMissing;
                    pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo = pInventory.activeWeapon.GetComponent<WeaponProperties>().maxAmmoInWeapon;
                }
                else if (pInventory.heavyAmmo < ammoWeaponIsMissing)
                {
                    pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo = pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo + pInventory.heavyAmmo;
                    pInventory.heavyAmmo = 0;
                }
            }
        }
        else if (wProperties.powerAmmo)
        {

            if (wProperties.usesShells)
            {
                pInventory.powerAmmo = pInventory.powerAmmo - 1;
                pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo = pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo + 1;
            }

            else
            {
                ammoWeaponIsMissing = pInventory.activeWeapon.GetComponent<WeaponProperties>().maxAmmoInWeapon - pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo;

                if (pInventory.powerAmmo >= ammoWeaponIsMissing)
                {
                    pInventory.powerAmmo = pInventory.powerAmmo - ammoWeaponIsMissing;
                    pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo = pInventory.activeWeapon.GetComponent<WeaponProperties>().maxAmmoInWeapon;
                }
                else if (pInventory.powerAmmo < ammoWeaponIsMissing)
                {
                    pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo = pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo + pInventory.powerAmmo;
                    pInventory.powerAmmo = 0;
                }
            }
        }
        pInventory.activeWeapon.GetComponent<WeaponProperties>().ResetBulletToIgnoreRecoil();
    }

    public void TransferAmmoDW(bool reloadedRight, bool reloadedLeft)
    {
        if (reloadedRight)
        {
            ammoRightWeaponIsMissing = pInventory.rightWeapon.GetComponent<WeaponProperties>().maxAmmoInWeapon - pInventory.rightWeaponCurrentAmmo;

            if (pInventory.smallAmmo >= ammoRightWeaponIsMissing)
            {
                pInventory.smallAmmo = pInventory.smallAmmo - ammoRightWeaponIsMissing;
                pInventory.rightWeapon.GetComponent<WeaponProperties>().currentAmmo = pInventory.rightWeapon.GetComponent<WeaponProperties>().maxAmmoInWeapon;
            }
            else if (pInventory.smallAmmo < ammoRightWeaponIsMissing)
            {
                pInventory.rightWeapon.GetComponent<WeaponProperties>().currentAmmo = pInventory.rightWeapon.GetComponent<WeaponProperties>().currentAmmo + pInventory.smallAmmo;
                pInventory.smallAmmo = 0;
            }
        }

        if (reloadedLeft)
        {
            ammoLeftWeaponIsMissing = pInventory.leftWeapon.GetComponent<WeaponProperties>().maxAmmoInWeapon - pInventory.leftWeaponCurrentAmmo;

            if (pInventory.smallAmmo >= ammoLeftWeaponIsMissing)
            {
                pInventory.smallAmmo = pInventory.smallAmmo - ammoLeftWeaponIsMissing;
                pInventory.leftWeapon.GetComponent<WeaponProperties>().currentAmmo = pInventory.leftWeapon.GetComponent<WeaponProperties>().maxAmmoInWeapon;
            }
            else if (pInventory.smallAmmo < ammoLeftWeaponIsMissing)
            {
                pInventory.leftWeapon.GetComponent<WeaponProperties>().currentAmmo = pInventory.leftWeapon.GetComponent<WeaponProperties>().currentAmmo + pInventory.smallAmmo;
                pInventory.smallAmmo = 0;
            }
        }
    }

    void ReferenceCameraToSpherecast()
    {
        //childManager.FindChildWithTagScript("Crosshairs").GetComponent<CrosshairScript>().cameraScript = childManager.FindChildWithTagScript("Player Inventory").GetComponent<CameraScript>();
        //childManager.FindChildWithTagScript("Crosshairs").GetComponent<CrosshairScript>().initialMouSensitivity = childManager.FindChildWithTagScript("Player Inventory").GetComponent<CameraScript>().mouseSensitivity;
    }

    /*
    //Enable bullet in mag renderer after set amount of time
    private IEnumerator ShowBulletInMag()
    {

        //Wait set amount of time before showing bullet in mag
        yield return new WaitForSeconds(gwProperties.showBulletInMagDelay);
        gwProperties.bulletInMagRenderer.GetComponent<SkinnedMeshRenderer>().enabled = true;
    }

    //Show light when shooting, then disable after set amount of time
    private IEnumerator MuzzleFlashLight()
    {

        gwProperties.muzzleflashLight.enabled = true;
        yield return new WaitForSeconds(gwProperties.lightDuration);
        gwProperties.muzzleflashLight.enabled = false;
    }
    */

    IEnumerator TestButton()
    {
        if (player.GetButtonDown("Test Button"))
        {
            if (wProperties.usesShells)
            {
                int ammoNeededToReload = wProperties.maxAmmoInWeapon - wProperties.currentAmmo;
                int ammoToReload = 0;

                Debug.Log("Ammo needed = " + ammoNeededToReload);

                if (ammoNeededToReload > pInventory.currentExtraAmmo)
                {
                    ammoToReload = pInventory.currentExtraAmmo;
                    Debug.Log("Ammo to Reload = " + ammoToReload);
                }
                else if (ammoNeededToReload <= pInventory.currentExtraAmmo)
                {
                    ammoToReload = ammoNeededToReload;
                    Debug.Log("Ammo to Reload = " + ammoToReload);
                }

                for (int i = 0; i < ammoToReload; i++)
                {
                    anim.Play("Reload Open Test", 0, 0f);
                    yield return new WaitForSeconds(2f);
                    Debug.Log("Played Animation");
                }

                /*
                while (ammoToReload > 0)
                {
                    if (!isReloading)
                    {
                        anim.Play("Reload Open", 0, 0f);
                        reloadAnimationStarted = true;
                    }
                    else
                    {
                        ammoToReload = ammoToReload - 1;
                    }
                }
                */
            }
        }
    }

    /*
    IEnumerator ShellReload()
    {
        if (wProperties.currentAmmo == 1)
        {
            anim.Play("Reload Open (7 Case)", 0, 0f);
            yield return new WaitForSeconds(7f);
            StartCoroutine(TransferAmmo());
        }
        else if (wProperties.currentAmmo == 2)
        {
            anim.Play("Reload Open (6 Case)", 0, 0f);
            yield return new WaitForSeconds(6f);
            StartCoroutine(TransferAmmo());
        }
        else if (wProperties.currentAmmo == 3)
        {
            anim.Play("Reload Open (5 Case)", 0, 0f);
            yield return new WaitForSeconds(5f);
            StartCoroutine(TransferAmmo());
        }
        else if (wProperties.currentAmmo == 4)
        {
            anim.Play("Reload Open (4 Case)", 0, 0f);
            yield return new WaitForSeconds(4f);
            StartCoroutine(TransferAmmo());
        }
        else if (wProperties.currentAmmo == 5)
        {
            anim.Play("Reload Open (3 Case)", 0, 0f);
            yield return new WaitForSeconds(3f);
            StartCoroutine(TransferAmmo());
        }
        else if (wProperties.currentAmmo == 6)
        {
            anim.Play("Reload Open (2 Case)", 0, 0f);
            yield return new WaitForSeconds(2f);
            StartCoroutine(TransferAmmo());
        }
        else if (wProperties.currentAmmo == 7)
        {
            anim.Play("Reload Open (1 Case)", 0, 0f);
            yield return new WaitForSeconds(1f);
            StartCoroutine(TransferAmmo());
        }
    }
    */



    public void UpdateAimingLayers()
    {
        if (isAiming)
        {
            if (playerRewiredID == 0)
            {
                gunCam.cullingMask &= ~(1 << 24);
            }
            if (playerRewiredID == 1)
            {
                gunCam.cullingMask &= ~(1 << 25);
            }
            if (playerRewiredID == 2)
            {
                gunCam.cullingMask &= ~(1 << 26);
            }
            if (playerRewiredID == 3)
            {
                gunCam.cullingMask &= ~(1 << 27);
            }
        }

        else if (!isAiming)
        {
            if (playerRewiredID == 0)
            {
                gunCam.cullingMask |= (1 << 24);
            }
            if (playerRewiredID == 1)
            {
                gunCam.cullingMask |= (1 << 25);
            }
            if (playerRewiredID == 2)
            {
                gunCam.cullingMask |= (1 << 26);
            }
            if (playerRewiredID == 3)
            {
                gunCam.cullingMask |= (1 << 27);
            }
        }

        ///Player Layers
        /// 5 = UI
        /// 
        /// 24 = P1 FPS
        /// 25 = P2 FPS
        /// 26 = P3 FPS
        /// 27 = P4 FPS
        /// 
        /// 28 = P1 3PS
        /// 29 = P2 3PS
        /// 30 = P3 3PS
        /// 31 = P4 3PS
        /// cam.cullingMask &= ~(1 << 24); //Disable Layer 24
        /// gunCam.cullingMask |= (1 << 24); //Enable Layer 24
    }

    void StartButton()
    {
        if (player.GetButtonDown("Start") || player.GetButtonDown("Escape"))
        {
            Debug.Log($"Pausing game");
            TogglePauseGame();
        }
    }

    void BackButton()
    {
        if (player.GetButtonDown("Back"))
        {
            allPlayerScripts.scoreboardManager.OpenScoreboard();
        }
        else if (player.GetButtonUp("Back"))
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
            allPlayerScripts.playerUIComponents.singlePlayerPauseMenu.gameObject.SetActive(true);
            pauseMenuOpen = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked; // Must Unlock Cursor so it can detect buttons
            Cursor.visible = false;
            allPlayerScripts.playerUIComponents.singlePlayerPauseMenu.gameObject.SetActive(false);
            pauseMenuOpen = false;
        }
        //if (Time.timeScale != 0)
        //{
        //    Debug.Log($"Number of player: {StaticVariables.numberOfPlayers}");
        //    Time.timeScale = 0;
        //    if (StaticVariables.numberOfPlayers == 1 || StaticVariables.numberOfPlayers == 0)
        //        if (lastControllerType == ControllerType.Keyboard || lastControllerType == ControllerType.Mouse)
        //        {
        //            Debug.Log("Pause MaK");
        //            Cursor.lockState = CursorLockMode.None; // Must Unlock Cursor so it can detect buttons
        //            allPlayerScripts.playerUIComponents.singlePlayerPauseMenu.gameObject.SetActive(true);
        //        }
        //        else
        //            allPlayerScripts.playerUIComponents.splitScreenPauseMenu.gameObject.SetActive(true);
        //    else
        //        allPlayerScripts.playerUIComponents.splitScreenPauseMenu.gameObject.SetActive(true);
        //}
        //else
        //{
        //    Time.timeScale = 1;
        //    if (StaticVariables.numberOfPlayers == 1 || StaticVariables.numberOfPlayers == 0)
        //        Cursor.lockState = CursorLockMode.Locked;
        //    allPlayerScripts.playerUIComponents.splitScreenPauseMenu.gameObject.SetActive(false);
        //    allPlayerScripts.playerUIComponents.singlePlayerPauseMenu.gameObject.SetActive(false);
        //    allPlayerScripts.playerUIComponents.splitScreenPauseMenu.gameObject.SetActive(false);
        //}
    }

    public void ReturnToMainMenu()
    {
        Debug.Log("Returning to Main Menu");
        //Cursor.lockState = CursorLockMode.Locked;
        //Time.timeScale = 1;
        //SceneManager.LoadScene("000 - Main Menu");

        PhotonNetwork.LoadLevel(0);
        PhotonNetwork.LeaveRoom();
    }
}


