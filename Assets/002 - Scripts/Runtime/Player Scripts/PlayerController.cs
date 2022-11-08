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
    public PlayerControllerEvent OnPlayerSwitchWeapons, OnPlayerLongInteract,
        OnPlayerFire, OnPlayerFireButtonUp, OnPlayerTestButton, OnPLayerThrewGrenade,
        OnCrouchUp, OnCrouchDown, OnSprintStart, OnSprintStop;

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
    public ControllerType activeControllerType;

    public PhotonView PV;

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
    }

    private void Update()
    {
        if (!PV.IsMine)
            return;

        //UpdateWeaponPropertiesAndAnimator();
        {
            StartButton();
            BackButton();
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
                    Shooting();
                    CheckReloadButton();
                    CheckAmmoForAutoReload();
                    ScopeIn();
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
            activeControllerType = ReInput.controllers.GetLastActiveControllerType();

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
        GetComponent<Player>().playerVoice.volume = 0.1f;
        GetComponent<Player>().PlaySprintingSound();
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

    void Shooting()
    {
        if (GetComponent<Player>().isDead || isSprinting)
            return;

        if (rewiredPlayer.GetButtonUp("Shoot"))
        {
            OnPlayerFireButtonUp?.Invoke(this);
        }
        if (!isDualWielding)
        {
            if (rewiredPlayer.GetButton("Shoot") && !pInventory.activeWeapon.isOutOfAmmo && !isReloading && !isShooting && !isInspecting && !isMeleeing && !isThrowingGrenade)
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
    void ScopeIn()
    {
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
    public void ScopeOut()
    {
        if (!isAiming && !GetComponent<Player>().isDead)
            return;
        Debug.Log("Unscope Script");
        isAiming = false;
        mainCam.fieldOfView = GetComponent<Player>().defaultFov;
        gunCam.fieldOfView = 60;
        camScript.mouseSensitivity = camScript.defaultMouseSensitivy;
        allPlayerScripts.aimingScript.playAimSound();

        mainCam.transform.localRotation = Quaternion.Euler(0, 0, 0);
        gunCam.enabled = true;
    }

    [PunRPC]
    float meleeMovementFactor = 0;
    void Melee()
    {
        if (!GetComponent<Player>().isDead)
        {
            if (rewiredPlayer.GetButtonDown("Melee") && !isMeleeing && !isShooting && !isThrowingGrenade && !isSprinting)
            {
                Debug.Log("RPC Call: Melee");
                meleeMovementFactor = 1;
                PV.RPC("Melee_RPC", RpcTarget.All);
            }
        }

        if (meleeMovementFactor > 0)
        {
            if (melee.playersInMeleeZone.Count > 0)
            {
                Vector3 move = transform.forward * meleeMovementFactor;
                GetComponent<CharacterController>().Move(move * movement.defaultSpeed * 10 * Time.deltaTime);
            }

            meleeMovementFactor -= Time.deltaTime * 5f;
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
        if (rewiredPlayer.GetButtonDown("Throw Grenade") && !isDualWielding && !isShooting && !isMeleeing && !isSprinting /* && !isInspecting */)
        {
            if (pInventory.grenades > 0 && !isThrowingGrenade)
            {
                ScopeOut();
                pInventory.grenades = pInventory.grenades - 1;
                weaponAnimator.Play("GrenadeThrow", 0, 0.0f);
                PV.RPC("ThrowGrenade_RPC", RpcTarget.All);
                OnPLayerThrewGrenade?.Invoke(this);
                //StartCoroutine(GrenadeSpawnDelay());
                //StartCoroutine(ThrowGrenade3PS());
            }
        }

        if (isDualWielding)
        {
            if (rewiredPlayer.GetButton("Throw Grenade") && !dwLeftWP.isOutOfAmmo && !isReloadingLeft && !isShootingLeft)
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
        if (!GetComponent<Player>().isDead)
        {
            if (rewiredPlayer.GetButtonDown("Reload") && !isReloading && !isDualWielding)
            {
                rScript.CheckAmmoTypeType(false);
            }
        }
    }

    void CheckAmmoForAutoReload()
    {
        if (!isDualWielding)
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
        if (Input.GetKeyDown(KeyCode.T))
        {
            weaponAnimator.Play("Reload Open", 0, 0f);
            //anim.SetTrigger("Inspect");
        }
    }

    private void AnimationCheck()
    {
        if (!isDualWielding)
        {
            if (pInventory.activeWeapon != null)
            {
                if (weaponAnimator.GetCurrentAnimatorStateInfo(0).IsName("Fire"))
                    isFiring = true;
                else
                    isFiring = false;

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

        if (weaponAnimator != null)
        {
            if (weaponAnimator.GetCurrentAnimatorStateInfo(0).IsName("Knife Attack 2"))
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
        yield return new WaitForEndOfFrame();
    }

    IEnumerator Melee3PS()
    {
        GetComponent<PlayerThirdPersonModelManager>().thirdPersonScript.GetComponent<Animator>().Play("Melee");
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
            grenade.GetComponent<ExplosiveProjectile>().player = GetComponent<Player>();
            //grenade.GetComponent<FragGrenade>().team = allPlayerScripts.playerMPProperties.team;
        }
        else if (stickyGrenadesActive)
        {
            grenade = Instantiate(pInventory.stickyGrenadePrefab,
               gwProperties.grenadeSpawnPoint.transform.position,
               gwProperties.grenadeSpawnPoint.transform.rotation);
            grenade.GetComponent<ExplosiveProjectile>().player = GetComponent<Player>();
            //grenade.GetComponent<StickyGrenade>().team = allPlayerScripts.playerMPProperties.team;
        }

        foreach (GameObject hb in GetComponent<Player>().hitboxes)
            Physics.IgnoreCollision(grenade.GetComponent<Collider>(), hb.GetComponent<Collider>()); // Prevents the grenade from colliding with the player who threw it

        grenade.GetComponent<Rigidbody>().AddForce(gwProperties.grenadeSpawnPoint.transform.forward * grenadeThrowForce);
        Destroy(grenade.gameObject, 10);
    }
    public void TransferAmmo()
    {

        // Old Reload
        #region
        //if (pInventory.activeWeapon.ammoType == WeaponProperties.AmmoType.Light)
        //{
        //    if (pInventory.activeWeapon.ammoReloadType == WeaponProperties.AmmoReloadType.Shell)
        //    {
        //        pInventory.smallAmmo = pInventory.smallAmmo - 1;
        //        pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo = pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo + 1;
        //    }


        //    else
        //    {
        //        ammoWeaponIsMissing = pInventory.activeWeapon.GetComponent<WeaponProperties>().ammoCapacity - pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo;

        //        if (pInventory.smallAmmo >= ammoWeaponIsMissing)
        //        {
        //            pInventory.smallAmmo = pInventory.smallAmmo - ammoWeaponIsMissing;
        //            pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo = pInventory.activeWeapon.GetComponent<WeaponProperties>().ammoCapacity;
        //        }
        //        else if (pInventory.smallAmmo < ammoWeaponIsMissing)
        //        {
        //            pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo = pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo + pInventory.smallAmmo;
        //            pInventory.smallAmmo = 0;
        //        }
        //    }

        //}

        //else if (pInventory.activeWeapon.ammoType == WeaponProperties.AmmoType.Heavy)
        //{
        //    if (pInventory.activeWeapon.ammoReloadType == WeaponProperties.AmmoReloadType.Shell)
        //    {
        //        pInventory.heavyAmmo = pInventory.heavyAmmo - 1;
        //        pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo = pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo + 1;
        //    }

        //    else
        //    {
        //        ammoWeaponIsMissing = pInventory.activeWeapon.GetComponent<WeaponProperties>().ammoCapacity - pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo;

        //        if (pInventory.heavyAmmo >= ammoWeaponIsMissing)
        //        {
        //            pInventory.heavyAmmo = pInventory.heavyAmmo - ammoWeaponIsMissing;
        //            pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo = pInventory.activeWeapon.GetComponent<WeaponProperties>().ammoCapacity;
        //        }
        //        else if (pInventory.heavyAmmo < ammoWeaponIsMissing)
        //        {
        //            pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo = pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo + pInventory.heavyAmmo;
        //            pInventory.heavyAmmo = 0;
        //        }
        //    }
        //}
        //else if (pInventory.activeWeapon.ammoType == WeaponProperties.AmmoType.Power)
        //{

        //    if (pInventory.activeWeapon.ammoReloadType == WeaponProperties.AmmoReloadType.Shell)
        //    {
        //        pInventory.powerAmmo = pInventory.powerAmmo - 1;
        //        pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo = pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo + 1;
        //    }

        //    else
        //    {
        //        ammoWeaponIsMissing = pInventory.activeWeapon.GetComponent<WeaponProperties>().ammoCapacity - pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo;

        //        if (pInventory.powerAmmo >= ammoWeaponIsMissing)
        //        {
        //            pInventory.powerAmmo = pInventory.powerAmmo - ammoWeaponIsMissing;
        //            pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo = pInventory.activeWeapon.GetComponent<WeaponProperties>().ammoCapacity;
        //        }
        //        else if (pInventory.powerAmmo < ammoWeaponIsMissing)
        //        {
        //            pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo = pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo + pInventory.powerAmmo;
        //            pInventory.powerAmmo = 0;
        //        }
        //    }
        //}
        #endregion


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
        //try
        //{
        //    //WebManager.webManagerInstance.SaveMultiplayerStats(GetComponent<PlayerMultiplayerMatchStats>());
        //    if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
        //        GetComponent<Player>().LeaveRoomWithDelay();
        //    //FindObjectOfType<MultiplayerManager>().EndGame();
        //    if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
        //        FindObjectOfType<SwarmManager>().EndGame();
        //}
        //catch (System.Exception e) { Debug.Log(e); }
        //GetComponent<Player>().Damage(23, false, GetComponent<PhotonView>().ViewID, new Vector3(1, 2, 1));
    }
}


