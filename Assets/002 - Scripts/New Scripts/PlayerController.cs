using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityEngine.SceneManagement;
using Photon.Pun;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviourPun
{
    // Events
    public delegate void PlayerControllerEvent(PlayerController playerController);
    public PlayerControllerEvent OnPlayerSwitchWeapons;
    public PlayerControllerEvent OnPlayerLongInteract;
    public PlayerControllerEvent OnPlayerFire;

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
    public CrosshairManager crosshairScript;
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
    public bool isInspecting, isShooting, aimSoundHasPlayed = false;

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
        objectPool = GameObjectPool.gameObjectPoolInstance;
        player = ReInput.players.GetPlayer(playerRewiredID);

        if (!PV.IsMine)
        {
            gunCam.gameObject.SetActive(false);
            mainCam.gameObject.SetActive(false);
            allPlayerScripts.playerUIComponents.gameObject.SetActive(false);
        }
        OnPlayerSwitchWeapons?.Invoke(this);
    }

    private void Update()
    {
        if (!PV.IsMine)
            return;

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
                    SwitchWeapons();
                    LongInteract();
                    if (isSprinting)
                        return;
                    Shooting();
                    CheckReloadButton();
                    CheckAmmoForAutoReload();
                    Aiming();
                    Melee();
                    Crouch();
                    Grenade(); //TO DO: Spawn Grenades the same way as bullets
                    HolsterAndInspect();
                    CheckDrawingWeapon();
                }
            }
        }

        AnimationCheck();
        TestButton();
        if (ReInput.controllers != null)
            lastControllerType = ReInput.controllers.GetLastActiveControllerType();

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

    void LongInteract()
    {
        if (player.GetButtonShortPressDown("Interact"))
        {
            OnPlayerLongInteract?.Invoke(this);
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

        if (pInventory.activeWeapon.GetComponent<WeaponProperties>().idleHandlingAnimationType == WeaponProperties.IdleHandlingAnimationType.Pistol)
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
            if (player.GetButton("Shoot") && !wProperties.isOutOfAmmo && !isReloading && !isShooting && !isInspecting && !isMeleeing && !isThrowingGrenade)
            {
                isShooting = true;
                OnPlayerFire?.Invoke(this);

            }
            else
            {
                isShooting = false;
            }
        }

        if (isDualWielding)
        {
            if (player.GetButton("Shoot") && !dwRightWP.isOutOfAmmo && !isReloadingRight && !isShootingRight && !isMeleeing && !isThrowingGrenade && !isSprinting)
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
    void Aiming()
    {
        if (isAiming)
        {
            wProperties.currentRedReticuleRange = wProperties.scopeRRR;
        }
        else
        {
            if (wProperties)
                if (wProperties.defaultRedReticuleRange > 0)
                {
                    wProperties.currentRedReticuleRange = wProperties.defaultRedReticuleRange;
                }
        }


        if (player.GetButtonDown("Aim") && !isReloading && !isRunning && !isInspecting)
        {
            if (wProperties.canScopeIn)
            {
                if (isAiming == false)
                {
                    isAiming = true;
                    mainCam.fieldOfView = wProperties.scopeFov;

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
    }
    public void ScopeOut()
    {
        if (!isAiming && !playerProperties.isDead)
            return;
        Debug.Log("Unscope Script");
        isAiming = false;
        mainCam.fieldOfView = playerProperties.defaultFov;
        camScript.mouseSensitivity = camScript.defaultMouseSensitivy;
        allPlayerScripts.aimingScript.playAimSound();

        mainCam.transform.localRotation = Quaternion.Euler(0, 0, 0);

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
        if (PV.IsMine)
        {
            melee.Knife();
            if (melee.playersInMeleeZone.Count > 0)
                meleeAudioSource.clip = melee.knifeSuccessSound;
            else
                meleeAudioSource.clip = melee.knifeFailSound;
        }
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
            if (player.GetButton("Throw Grenade") && !dwLeftWP.isOutOfAmmo && !isReloadingLeft && !isShootingLeft)
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
                    rScript.CheckAmmoTypeType(true);
                }
                else
                {
                }
            }
        }

        if (isDualWielding)
        {
            if (pInventory.rightWeaponCurrentAmmo <= 0)
            {
                pInventory.rightWeaponCurrentAmmo = 0;
                //dwReload.CheckAmmoTypeType(true, false);
            }
            else
            {
            }

            if (pInventory.leftWeaponCurrentAmmo <= 0)
            {
                pInventory.leftWeaponCurrentAmmo = 0;
                //dwReload.CheckAmmoTypeType(false, true);
            }
            else
            {
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

    void SwitchWeapons()
    {
        if (player.GetButtonDown("Switch Weapons"))
        {
            OnPlayerSwitchWeapons?.Invoke(this);
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

                if (wProperties.ammoReloadType == WeaponProperties.AmmoReloadType.Magazine)
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

                if (wProperties.ammoReloadType == WeaponProperties.AmmoReloadType.Shell)
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

                if (wProperties.ammoProjectileType == WeaponProperties.AmmoProjectileType.Rocket || wProperties.ammoProjectileType == WeaponProperties.AmmoProjectileType.Grenade)
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
    public void TransferAmmo()
    {

        if (wProperties.ammoType == WeaponProperties.AmmoType.Light)
        {
            if (wProperties.ammoReloadType == WeaponProperties.AmmoReloadType.Shell)
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

        else if (wProperties.ammoType == WeaponProperties.AmmoType.Heavy)
        {
            if (wProperties.ammoReloadType == WeaponProperties.AmmoReloadType.Shell)
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
        else if (wProperties.ammoType == WeaponProperties.AmmoType.Power)
        {

            if (wProperties.ammoReloadType == WeaponProperties.AmmoReloadType.Shell)
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
    void TestButton()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            try
            {
                OnlineMultiplayerManager.multiplayerManagerInstance.EndGame();

            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }

            try
            {
                OnlineSwarmManager.onlineSwarmManagerInstance.EndGame();

            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }
        }
    }
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
    }

    public void ReturnToMainMenu()
    {
        Debug.Log("Returning to Main Menu");

        PhotonNetwork.LoadLevel(0);
        PhotonNetwork.LeaveRoom();
    }

    public void SetPlayerIDInInput()
    {
        player = ReInput.players.GetPlayer(playerRewiredID);
    }
}


