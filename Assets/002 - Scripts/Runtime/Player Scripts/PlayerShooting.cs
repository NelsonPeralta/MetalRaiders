using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using System;

public class PlayerShooting : MonoBehaviourPun
{
    public delegate void PlayerShootingEvent(PlayerShooting playerShooting);
    public PlayerShootingEvent OnBulletSpawned;

    public Biped trackingTarget
    {
        get { return _trackingTarget; }

        set
        {
            _preTrackingTarget = _trackingTarget;
            _trackingTarget = value;


            if (_preTrackingTarget != _trackingTarget)
            {
                if (_trackingTarget != null) print($"{playerController.player.name} updated tracking target to {_trackingTarget.name}");
                else print("Cleared tracking target");

                if (playerController.player.isMine)
                    playerController.UpdateTrackingTargetForOtherPlayers(true, (_trackingTarget != null) ? _trackingTarget.originalSpawnPosition : Vector3.zero);
            }
        }
    }

    public float fireRecovery { get { return _fireRecovery; } }
    public bool overchargeReady { get { return _overchargeFloat >= WeaponProperties.OVERCHARGE_TIME_LOW; } }
    public bool overchargeReadyLeftWeapon { get { return _overchargeFloat_thirdWeapon >= WeaponProperties.OVERCHARGE_TIME_LOW; } }
    public bool fireButtonDown
    {
        get { return _fireButtonDown; }
        set
        {
            _fireButtonDown = value;
            print($"Fire Button Down: {value}");
        }
    }

    public bool isDualWielding { get { return playerController.player.isDualWielding; } }


    [Header("Other Scripts")]
    public PhotonView PV;
    public PlayerController playerController;
    public PlayerInventory pInventory;
    public ThirdPersonScript tPersonController;

    // Private variables
    int playerRewiredID;
    [SerializeField] float _fireRecovery = 0, _leftFireRecovery = 0, _overchargeFloat, _overchargeFloat_thirdWeapon;
    [SerializeField] bool _fireButtonDown = false, dualWieldedWeaponFireButnDown = false;
    [SerializeField] LayerMask _fakeBulletTrailCollisionLayerMask;


    [SerializeField] Biped _trackingTarget, _preTrackingTarget;
    [SerializeField] AudioSource _leftWeaponAudioSource;


    public float defaultBurstInterval
    {
        get { return 0.08f; }
    }


    List<Quaternion> quats = new List<Quaternion>();

    private void Awake()
    {
        if (GameManager.instance.connection == GameManager.Connection.Local)
            GetComponent<AudioSource>().spatialBlend = 0;

    }
    private void Start()
    {
        _ignoreShootCounter = 2;
        playerController.OnPlayerFire += OnPlayerControllerFire_Delegate;
        playerController.OnPlayerFireButtonUp += OnPlayerControllerFireUp_Delegate;

        playerController.OnPlayerScopeBtnDown += OnPlayerControllerScope_Delegate;
        playerController.OnDualWieldedWeaponFireBtnUp += OnPlayerFireDualWieldedWeaponButtonUp_Delegate;
        playerController.player.OnPlayerDeath += OnPlayerDeath_Delegate;
    }

    public void Update()
    {
        if (playerController)
        {
            FireCooldown();



            if (playerController.isHoldingShootBtn)
            {
                if (playerController.pInventory && playerController.pInventory.activeWeapon)
                    if (pInventory.activeWeapon.currentOverheat <= 0 && pInventory.activeWeapon.loadedAmmo > 0)
                        _overchargeFloat += Time.deltaTime;
            }
            else
            {
                _overchargeFloat = 0;
            }


            if (playerController.player.isDualWielding)
            {
                if (playerController.isHoldingShootDualWieldedWeapon)
                {
                    if (pInventory.thirdWeapon.currentOverheat <= 0 && pInventory.thirdWeapon.loadedAmmo > 0)
                        _overchargeFloat_thirdWeapon += Time.deltaTime;
                }
                else
                {
                    _overchargeFloat_thirdWeapon = 0;
                }
            }
        }
    }











    void OnPlayerControllerFireUp_Delegate(PlayerController playerController)
    {
        fireButtonDown = false;

        print($"OnPlayerControllerFireUp_Delegate 1");


        if (!playerController.player.isRespawning && !playerController.player.isDead)
            if (playerController.player.playerInventory.activeWeapon && playerController.player.playerInventory.activeWeapon.overcharge)
            {
                print($"OnPlayerControllerFireUp_Delegate 2 {pInventory.activeWeapon.overheatCooldown} {pInventory.activeWeapon.loadedAmmo}");

                if (pInventory.activeWeapon.overheatCooldown <= 0 && pInventory.activeWeapon.loadedAmmo > 0)
                {
                    print($"OnPlayerControllerFireUp_Delegate 3");

                    if (_overchargeFloat > (WeaponProperties.OVERCHARGE_TIME_FULL))
                    {
                        print("SHOOT OVERCHARGED SHOT");
                        ShootOverchargeWeapon(playerController.player.playerInventory.activeWeapon, true);
                    }
                    else
                    {
                        print("Shoot normal shot");
                        ShootOverchargeWeapon(playerController.player.playerInventory.activeWeapon);
                    }
                }
            }
    }

    void OnPlayerControllerFire_Delegate(PlayerController playerController)
    {

        //if (playerController.isDrawingWeapon)
        //    return;

        //Shoot();
    }



    void OnPlayerFireDualWieldedWeaponButtonUp_Delegate(PlayerController playerController)
    {
        dualWieldedWeaponFireButnDown = false;


        if (!playerController.player.isRespawning && !playerController.player.isDead)
            if (playerController.player.playerInventory.thirdWeapon && playerController.player.playerInventory.thirdWeapon.overcharge)
            {
                print($"OnPlayerFireDualWieldedWeaponButtonUp_Delegate 2 {pInventory.thirdWeapon.overheatCooldown} {pInventory.thirdWeapon.loadedAmmo}");

                if (pInventory.thirdWeapon.overheatCooldown <= 0 && pInventory.thirdWeapon.loadedAmmo > 0)
                {
                    print($"OnPlayerFireDualWieldedWeaponButtonUp_Delegate 3");

                    if (_overchargeFloat_thirdWeapon > (WeaponProperties.OVERCHARGE_TIME_FULL))
                    {
                        print("OnPlayerFireDualWieldedWeaponButtonUp_Delegate SHOOT OVERCHARGED SHOT");
                        ShootOverchargeWeapon(playerController.player.playerInventory.thirdWeapon, true);
                    }
                    else
                    {
                        print("OnPlayerFireDualWieldedWeaponButtonUp_Delegate Shoot normal shot");
                        ShootOverchargeWeapon(playerController.player.playerInventory.thirdWeapon);
                    }
                }
            }
    }

    void OnPlayerControllerScope_Delegate(PlayerController playerController)
    {
        if (playerController.isDrawingWeapon)
            return;

        //Shoot(pInventory.activeWeapon.leftWeapon);
    }

    public void Shoot(WeaponProperties wp)
    {
        /*if (wp == playerController.player.playerInventory.thirdWeapon)*/
        print("Calling Shoot");
        if (playerController.isDrawingWeapon) return;
        /*if (wp == playerController.player.playerInventory.thirdWeapon)*/
        print($"Shoot start 1 {_fireRecovery} {_leftFireRecovery} {wp == playerController.player.playerInventory.activeWeapon}");





        if ((wp == playerController.player.playerInventory.activeWeapon && _fireRecovery > 0)) // active weapon
            return;
        if (isDualWielding && wp == playerController.player.playerInventory.thirdWeapon && _leftFireRecovery > 0) // dual wielded weapon
            return;



        if (wp == playerController.player.playerInventory.activeWeapon)
        {

            if (wp.firingMode == WeaponProperties.FiringMode.Burst)
                _fireRecovery = defaultBurstInterval * 5.5f;
            else
                _fireRecovery = 1 / (wp.fireRate / 60f);
        }
        else
        {
            _leftFireRecovery = 1 / (wp.fireRate / 60f);
        }

        /* if (wp == playerController.player.playerInventory.thirdWeapon)*/
        print($"Shoot start 2 {wp == playerController.player.playerInventory.activeWeapon} {_fireRecovery} {_leftFireRecovery} {wp.overcharge} {wp}");

        if (CanShootAuto(wp) || CanShootSingleOrBurst(wp))
        {
            if (wp == playerController.player.playerInventory.activeWeapon)
                fireButtonDown = true;
            else
                dualWieldedWeaponFireButnDown = true;

            /*if (wp == playerController.player.playerInventory.thirdWeapon)*/
            print($"Shoot start 3 {wp == playerController.player.playerInventory.activeWeapon} {wp.overcharge} {wp}");


            if (wp.firingMode == WeaponProperties.FiringMode.Burst)
                ShootBurst(wp);
            else if (!wp.overcharge)
                Shoot_Caller(wp == playerController.player.playerInventory.thirdWeapon);
            else if (wp.overcharge)
            {
                //print("shooting overcharg");
                //Shoot_Caller(isLeftWeapon);
            }
        }
    }





    public void ShootOverchargeWeapon(WeaponProperties wp, bool overcharge = false)
    {
        if (playerController.isDrawingWeapon) return;

        if (wp != playerController.player.playerInventory.thirdWeapon) // active weapon
        {
            if ((_fireRecovery > 0 && !wp)) return;


            _fireRecovery = 1 / (wp.fireRate / 60f);

            if (CanShootAuto(wp) || CanShootSingleOrBurst(wp))
            {
                //fireButtonDown = true; // NO! Does not reset after bolt is shot. If player switches weapons, causes blank shot

                if (wp.overcharge)
                {
                    print("shooting overcharg");
                    Shoot_Caller(false, overcharge);
                }
            }
        }
        else // dual wielded weapon
        {
            if ((_leftFireRecovery > 0 && !wp)) return;


            _leftFireRecovery = 1 / (wp.fireRate / 60f);

            if (CanShootAuto(wp) || CanShootSingleOrBurst(wp))
            {
                //fireButtonDown = true; // NO! Does not reset after bolt is shot. If player switches weapons, causes blank shot

                if (wp.overcharge)
                {
                    print("shooting overcharg");
                    Shoot_Caller(true, overcharge);
                }
            }
        }


    }




    bool CanShootAuto(WeaponProperties activeWeapon)
    {
        return (activeWeapon.firingMode == WeaponProperties.FiringMode.Auto);
    }

    bool CanShootSingleOrBurst(WeaponProperties weaponToShoot)
    {
        print($"CanShootSingleOrBurst: {weaponToShoot.firingMode} {fireButtonDown} {dualWieldedWeaponFireButnDown} {pInventory.isDualWielding}");


        if (!pInventory.isDualWielding)
        {
            if (weaponToShoot.firingMode == WeaponProperties.FiringMode.Single || weaponToShoot.firingMode == WeaponProperties.FiringMode.Burst)
            {
                if (!fireButtonDown) return true;
            }
            else return false;
        }
        else
        {
            if (weaponToShoot.firingMode == WeaponProperties.FiringMode.Single || weaponToShoot.firingMode == WeaponProperties.FiringMode.Burst)
            {
                if (weaponToShoot == playerController.player.playerInventory.activeWeapon)
                {
                    if (!fireButtonDown) return true;
                    else return false;
                }
                else if (weaponToShoot == playerController.player.playerInventory.thirdWeapon)
                {
                    if (!dualWieldedWeaponFireButnDown) return true;
                    else return false;
                }
            }
            else
            {
                return false;
            }
            //return ((weaponToShoot.firingMode == WeaponProperties.FiringMode.Single || weaponToShoot.firingMode == WeaponProperties.FiringMode.Burst) && !fireButtonDown);
        }


        return false;
        //return ((weaponToShoot.firingMode == WeaponProperties.FiringMode.Single || weaponToShoot.firingMode == WeaponProperties.FiringMode.Burst)
        //    && ((!fireButtonDown && weaponToShoot == playerController.player.playerInventory.activeWeapon) || (pInventory.isDualWielding && !dualWieldedWeaponFireButnDown && weaponToShoot == playerController.player.playerInventory.thirdWeapon)));
    }

    void ShootBurst(WeaponProperties activeWeapon)
    {
        print("Shoot Burst");
        for (int i = 0; i < 3; i++)
        {
            if (activeWeapon.loadedAmmo > 0)
                StartCoroutine(ShootBurst_Coroutine(defaultBurstInterval * i));
        }
    }

    IEnumerator ShootBurst_Coroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (pInventory.activeWeapon.firingMode == WeaponProperties.FiringMode.Burst && !playerController.isDrawingWeapon)
            Shoot_RPC();
        //return;
        //PV.RPC("Shoot_RPC", RpcTarget.All);
    }

    void Shoot_Caller(bool isLeftWeapon = false, bool overcharge = false)
    {

        WeaponProperties weap = pInventory.activeWeapon.GetComponent<WeaponProperties>();
        /*if (activeWeapon == playerController.player.playerInventory.thirdWeapon)*/
        Debug.Log($"Shoot_Caller: {playerController.player.name} Shoot_Caller {weap.name} {weap.loadedAmmo} {isLeftWeapon}");

        if (isLeftWeapon) weap = pInventory.thirdWeapon;

        if (!playerController.player.isDualWielding)
        {
            if (weap.loadedAmmo <= 0 || playerController.isReloading)
            {
                /*if (activeWeapon == playerController.player.playerInventory.thirdWeapon)*/
                Debug.Log($"{playerController.player.name} Shoot_Caller {weap.name} {weap.loadedAmmo} isReloading: {playerController.isReloading}");
                return;
            }
        }
        else
        {
            if (!isLeftWeapon)
            {
                if (weap.loadedAmmo <= 0 || playerController.isReloadingRight)
                {
                    /*if (activeWeapon == playerController.player.playerInventory.thirdWeapon)*/
                    Debug.Log($"{playerController.player.name} Shoot_Caller {weap.name} {weap.loadedAmmo} isReloading: {playerController.isReloading}");
                    return;
                }
            }
            else
            {
                if (weap.loadedAmmo <= 0 || playerController.isReloadingLeft)
                {
                    /*if (activeWeapon == playerController.player.playerInventory.thirdWeapon)*/
                    Debug.Log($"{playerController.player.name} Shoot_Caller {weap.name} {weap.loadedAmmo} isReloading: {playerController.isReloading}");
                    return;
                }
            }
        }






        //if (activeWeapon.loadedAmmo <= 0 || playerController.isReloading)
        //{
        //    /*if (activeWeapon == playerController.player.playerInventory.thirdWeapon)*/
        //    Debug.Log($"{playerController.player.name} Shoot_Caller {activeWeapon.name} {activeWeapon.loadedAmmo} isReloading: {playerController.isReloading}");
        //    return;
        //}

        shoooo(isLeftWeapon, overcharge);
        //Shoot_RPC();
        return;
        PV.RPC("Shoot_RPC", RpcTarget.All);
    }

    [PunRPC]
    void Shoot_RPC()
    {
        shoooo();
    }


    int _ignoreShootCounter;
    List<RaycastHit> fakeBulletTrailRaycasthits = new List<RaycastHit>();

    void shoooo(bool isLeftWeapon = false, bool overcharge = false)
    {
        playerController.GetComponent<GeneralWeapProperties>().ResetLocalTransform();


        if (playerController.GetComponent<Player>().isDead || playerController.GetComponent<Player>().isRespawning)
            return;

        if (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On)
        {
            if (!playerController.GetComponent<Player>().aimAssist.redReticuleIsOn && !playerController.GetComponent<Player>().aimAssist.invisibleAimAssistOn)
            {
                // if the player is allowed no aim assist at all
                // we will simply correct the angle so that the bullet is fired at whatever is at the center of the players main cam
                playerController.gwProperties.tpsRotationToCameraCenterControl.localRotation = Quaternion.identity;
                playerController.gwProperties.tpsRotationToCameraCenterControl.LookAt(playerController.player.playerCamera.playerCameraCenterPointCheck.target.position);
            }
            else
            {
                // if the player is allowed a certain amount of aim assist, do nothing special for now
                // make sure its reset so it does not come in control with the aim assist rotation control
                playerController.gwProperties.tpsRotationToCameraCenterControl.localRotation = Quaternion.identity;
            }
        }

        playerController.SetCurrentlyShootingReset();

        playerController.player.assignActorPlayerTargetOnShootingSphere.TriggerBehaviour();

        //Debug.Log($"shoooo 1 {isLeftWeapon}");

        int counter = 1;
        WeaponProperties weaponToShoot = pInventory.activeWeapon.GetComponent<WeaponProperties>();

        if (isLeftWeapon) weaponToShoot = pInventory.thirdWeapon;

        if (weaponToShoot.isShotgun)
        {
            counter = weaponToShoot.numberOfPellets;
            counter = playerController.GetComponent<GeneralWeapProperties>().pelletSpawnPoints.Count;
            if (weaponToShoot.isShotgun)
                for (int j = 0; j < playerController.GetComponent<GeneralWeapProperties>().pelletSpawnPoints.Count; j++)
                    quats.Add(Quaternion.Euler(Vector3.zero));
        }

        for (int i = 0; i < counter; i++)
            if (weaponToShoot.ammoProjectileType == WeaponProperties.AmmoProjectileType.Bullet || weaponToShoot.ammoProjectileType == WeaponProperties.AmmoProjectileType.Plasma)
            {
                Player player = playerController.GetComponent<Player>();
                Quaternion ranSprayQuat = weaponToShoot.GetRandomSprayRotation();

                if (playerController.isAiming && weaponToShoot.hipSprayOnly)
                    ranSprayQuat = Quaternion.identity;

                if (weaponToShoot.ammoProjectileType != WeaponProperties.AmmoProjectileType.Plasma)
                {
                    //if (!player.isMine/* || GameManager.instance.connection == GameManager.Connection.Local*/)
                    {
                        if (weaponToShoot == playerController.player.playerInventory.thirdWeapon) print("spawning FAKE bullet");




                        fakeBulletTrailRaycasthits = Physics.RaycastAll(player.mainCamera.transform.position, player.mainCamera.transform.forward, playerController.pInventory.activeWeapon.range, _fakeBulletTrailCollisionLayerMask).ToList();

                        if (fakeBulletTrailRaycasthits.Count <= 0)
                        {

                            // if we find no colliders (default or hitboxes), shoot a fake trail at its maximum lenght
                            pInventory.SpawnFakeBulletTrail((int)weaponToShoot.range,
                                ranSprayQuat, player.isMine,
                               muzzlePosition: weaponToShoot.tpsMuzzleFlash.transform.position);
                        }
                        else
                        {
                            for (int j = fakeBulletTrailRaycasthits.Count; j-- > 0;)
                                if (fakeBulletTrailRaycasthits[j].collider.transform.root == player.transform)
                                    fakeBulletTrailRaycasthits.Remove(fakeBulletTrailRaycasthits[j]);
                            fakeBulletTrailRaycasthits = fakeBulletTrailRaycasthits.OrderBy(item => Vector3.Distance(player.mainCamera.transform.position, item.point)).ToList();






                            if (fakeBulletTrailRaycasthits.Count > 0)
                            {

                                print("There is a collider at the center of our main camera. " +
                                $"The dot product is: {Vector3.Dot(fakeBulletTrailRaycasthits[0].point - player.mainCamera.transform.position, fakeBulletTrailRaycasthits[0].point - playerController.pInventory.activeWeapon.tpsMuzzleFlash.transform.position)}");

                                print($"The distance between the muzzle and the hit at the center of the camera is: " +
                                    $"{Vector3.Distance(playerController.pInventory.activeWeapon.tpsMuzzleFlash.transform.position, fakeBulletTrailRaycasthits[0].point)}");

                                if (Vector3.Dot(fakeBulletTrailRaycasthits[0].point - player.mainCamera.transform.position,
                                    fakeBulletTrailRaycasthits[0].point - playerController.pInventory.activeWeapon.tpsMuzzleFlash.transform.position) > 0)
                                {
                                    if (Vector3.Distance(playerController.pInventory.activeWeapon.tpsMuzzleFlash.transform.position, fakeBulletTrailRaycasthits[0].point) > 4)
                                    {
                                        pInventory.SpawnFakeBulletTrail((int)Vector3.Distance(playerController.pInventory.activeWeapon.tpsMuzzleFlash.transform.position, fakeBulletTrailRaycasthits[0].point),
                                            ranSprayQuat, player.isMine,
                               muzzlePosition: weaponToShoot.tpsMuzzleFlash.transform.position,
                               lookAtThisTarget: fakeBulletTrailRaycasthits[0].point);
                                    }
                                    else
                                    {
                                        // The target may be too close to the player. The trail could be unrealistically warped
                                        // this could break immersion
                                        // we will NOT show a fake trail
                                    }
                                }
                                else
                                {
                                    // The target may be between the position of the camera and the end of the muzzle of the gun
                                    // we will NOT show a fake trail
                                }


                                //pInventory.SpawnFakeBulletTrail((int)Vector3.Distance(player.mainCamera.transform.position, fakeBulletTrailRaycasthits[0].point), ranSprayQuat, player.isMine);
                            }
                            else
                            {
                                pInventory.SpawnFakeBulletTrail((int)playerController.pInventory.activeWeapon.range,
                                ranSprayQuat, player.isMine,
                               muzzlePosition: weaponToShoot.tpsMuzzleFlash.transform.position);
                            }
                        }




                        //print("spawning FAKE bullet");
                        //RaycastHit hit;
                        //if (Physics.Raycast(player.mainCamera.transform.position, player.mainCamera.transform.forward, out hit, playerController.pInventory.activeWeapon.range, _fakeBulletTrailCollisionLayerMask))
                        //{
                        //    print($"spawning FAKE bullet {hit.collider.name}");

                        //    int d = (int)Vector3.Distance(player.mainCamera.transform.position, hit.point);
                        //    pInventory.SpawnFakeBulletTrail(d, ranSprayQuat);
                        //}
                        //else
                        //{
                        //    print($"spawning FAKE bullet {hit.collider.name}");
                        //    pInventory.SpawnFakeBulletTrail((int)playerController.pInventory.activeWeapon.range, ranSprayQuat);
                        //}
                    }
                }
                else
                {

                }



                if (player.isMine || weaponToShoot.ammoProjectileType == WeaponProperties.AmmoProjectileType.Plasma)
                {
                    Debug.Log($"Shooting Plasma bullet {overcharge} {weaponToShoot.targetTracking}");
                    var bullet = GameObjectPool.instance.SpawnPooledBullet();
                    bullet.transform.localScale = Vector3.one;

                    if (overcharge) bullet.transform.localScale = Vector3.one * 5;

                    bullet.GetComponent<Bullet>().overcharged = false;
                    bullet.GetComponent<Bullet>().trackingTarget = null;
                    try { bullet.gameObject.GetComponent<Bullet>().weaponProperties = weaponToShoot; } catch { }


                    if (weaponToShoot.targetTracking)
                    {
                        if (!weaponToShoot.overcharge)
                            bullet.GetComponent<Bullet>().trackingTarget = trackingTarget;
                        else
                        {
                            if (overcharge)
                            {
                                bullet.GetComponent<Bullet>().trackingTarget = trackingTarget;
                                bullet.GetComponent<Bullet>().overcharged = overcharge;
                                bullet.GetComponent<Bullet>().damage = 25;
                            }
                        }
                    }
                    print($"Active weapon has target tracking: {weaponToShoot.targetTracking}. PlayerShooting script has tracking target {trackingTarget}. {overcharge}");




                    {
                        Debug.Log(bullet);
                        Debug.Log(weaponToShoot);
                        bullet.GetComponent<Bullet>().bluePlasma.SetActive(weaponToShoot.plasmaColor == WeaponProperties.PlasmaColor.Blue && weaponToShoot.ammoProjectileType == WeaponProperties.AmmoProjectileType.Plasma);
                        bullet.GetComponent<Bullet>().redPlasma.SetActive(weaponToShoot.plasmaColor == WeaponProperties.PlasmaColor.Red && weaponToShoot.ammoProjectileType == WeaponProperties.AmmoProjectileType.Plasma);
                        bullet.GetComponent<Bullet>().greenPlasma.SetActive(weaponToShoot.plasmaColor == WeaponProperties.PlasmaColor.Green && weaponToShoot.ammoProjectileType == WeaponProperties.AmmoProjectileType.Plasma);
                        bullet.GetComponent<Bullet>().shard.SetActive(weaponToShoot.plasmaColor == WeaponProperties.PlasmaColor.Shard && weaponToShoot.ammoProjectileType == WeaponProperties.AmmoProjectileType.Plasma);
                    }

                    if (!playerController.GetComponent<Player>().aimAssist.redReticuleIsOn && !playerController.GetComponent<Player>().aimAssist.invisibleAimAssistOn)
                        playerController.GetComponent<GeneralWeapProperties>().ResetLocalTransform();


                    try
                    {
                        if (weaponToShoot.isShotgun)
                        {
                            quats[i] = UnityEngine.Random.rotation;
                            //playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.localRotation = Quaternion.RotateTowards(playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.localRotation, quats[i], activeWeapon.bulletSpray);
                        }
                        else
                            playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.localRotation *= ranSprayQuat;
                    }
                    catch { }

                    //if (PV.IsMine)
                    //    bullet.layer = 8;
                    //else
                    //    bullet.layer = 0;

                    if (weaponToShoot.isShotgun)
                    {
                        quats[i] = UnityEngine.Random.rotation;

                        bullet.transform.position = playerController.GetComponent<GeneralWeapProperties>().pelletSpawnPoints[i].position;
                        bullet.transform.rotation = playerController.GetComponent<GeneralWeapProperties>().pelletSpawnPoints[i].rotation;

                        bullet.transform.rotation = Quaternion.RotateTowards(playerController.GetComponent<GeneralWeapProperties>().pelletSpawnPoints[i].rotation, quats[i], weaponToShoot.bulletSpray);
                    }
                    else
                    {
                        bullet.transform.position = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.position;
                        bullet.transform.rotation = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.rotation;
                    }



                    try { bullet.gameObject.GetComponent<Bullet>().sourcePlayer = playerController.GetComponent<Player>(); } catch { }
                    try { bullet.gameObject.GetComponent<Bullet>().weaponProperties = weaponToShoot; } catch { }
                    try { bullet.gameObject.GetComponent<Bullet>().damage = (int)(playerController.GetComponent<Player>().playerInventory.activeWeapon.damage * (player.isDualWielding ? 0.72f : 1)); } catch { }


                    if (overcharge) bullet.GetComponent<Bullet>().damage *= 5;



                    //try { bullet.gameObject.GetComponent<Bullet>().allPlayerScripts = playerController.GetComponent<AllPlayerScripts>(); } catch { }
                    bullet.gameObject.GetComponent<Bullet>().range = (int)weaponToShoot.range;
                    bullet.gameObject.GetComponent<Bullet>().speed = (int)weaponToShoot.bulletSpeed;

                    if (weaponToShoot.hybridHitscan && weaponToShoot.ammoProjectileType == WeaponProperties.AmmoProjectileType.Bullet && player.aimAssist.redReticuleIsOn) { bullet.gameObject.GetComponent<Bullet>().speed = 999; print("hybrid hitscan working"); }

                    if (overcharge) bullet.gameObject.GetComponent<Bullet>().speed = (int)(weaponToShoot.bulletSpeed * 0.65f);
                    //bullet.gameObject.GetComponent<Bullet>().playerRewiredID = playerRewiredID;
                    //try { bullet.gameObject.GetComponent<Bullet>().playerWhoShot = playerController.GetComponent<GeneralWeapProperties>().GetComponent<Player>(); } catch { }
                    //bullet.gameObject.GetComponent<Bullet>().pInventory = pInventory;
                    //try { bullet.gameObject.GetComponent<Bullet>().crosshairScript = playerController.GetComponent<Player>().cScript; } catch { }
                    print($"bullet spawned at: {Time.time}");
                    bullet.SetActive(true);

                    if (weaponToShoot.plasmaColor != WeaponProperties.PlasmaColor.Shard)
                    {
                        weaponToShoot.currentOverheat = Mathf.Clamp(weaponToShoot.currentOverheat + weaponToShoot.overheatPerShot, 0, 100);

                        if (overcharge) weaponToShoot.currentOverheat = 100;
                    }
                }
                weaponToShoot.SpawnMuzzleflash();
            }
            else if (weaponToShoot.ammoProjectileType == WeaponProperties.AmmoProjectileType.Rocket || weaponToShoot.ammoProjectileType == WeaponProperties.AmmoProjectileType.Grenade)
            {
                // Projectile does not spawn if ammo left is 0, lag
                if (playerController.player.isMine)
                    if (weaponToShoot.ammoProjectileType == WeaponProperties.AmmoProjectileType.Rocket)
                    {

                        PV.RPC("SpawnFakeExplosiveProjectile_RPC", RpcTarget.AllViaServer, GrenadePool.GetAvailableRocketAtIndex(playerController.player.playerDataCell.photonRoomIndex),
                        playerController.player.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.position, playerController.player.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.rotation.eulerAngles);
                    }
                    else if (weaponToShoot.ammoProjectileType == WeaponProperties.AmmoProjectileType.Grenade)
                    {
                        PV.RPC("SpawnFakeExplosiveProjectile_RPC", RpcTarget.AllViaServer, GrenadePool.GetAvailableGrenadeLauncherProjectileAtIndex(playerController.player.playerDataCell.photonRoomIndex),
                        playerController.player.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.position, playerController.player.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.rotation.eulerAngles);
                    }

                weaponToShoot.SpawnMuzzleflash();


                //Debug.Log($"{playerController.name} PlayerShooting: AmmoProjectileType.Rocket");
                //ExplosiveProjectile rocket = null;

                //if (activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Rocket)
                //{
                //    //rocket = Instantiate(playerController.GetComponent<GeneralWeapProperties>().rocketProjectilePrefab).GetComponent<Rocket>();
                //    rocket = Instantiate(playerController.GetComponent<GeneralWeapProperties>().rocketProjectilePrefab).GetComponent<ExplosiveProjectile>();
                //}
                //else if (activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Grenade)
                //    rocket = Instantiate(playerController.GetComponent<GeneralWeapProperties>().grenadeLauncherProjectilePrefab).GetComponent<ExplosiveProjectile>();

                //Debug.Log($"{playerController.name} PlayerShooting: {rocket.name}");
                //rocket.player = playerController.player;


                ////if (PV.IsMine)
                ////    rocket.gameObject.layer = 8;
                ////else
                ////    rocket.gameObject.layer = 0;

                //rocket.transform.position = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.position;
                //rocket.transform.rotation = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.rotation;

                //rocket.gameObject.GetComponent<ExplosiveProjectile>().player = playerController.GetComponent<GeneralWeapProperties>().GetComponent<Player>();

                //if (activeWeapon.muzzleFlash)
                //    activeWeapon.SpawnMuzzleflash();
                //else
                //    GetComponent<CommonFiringActions>().SpawnMuzzleflash();
            }

        if (!PV.IsMine)
            _ignoreShootCounter++;

        if (PV.IsMine)
        {
            if (!overcharge)
            {
                print("removing 1 loaded ammo");
                weaponToShoot.loadedAmmo -= 1;
            }
            else
                weaponToShoot.loadedAmmo -= 10;
        }

        try
        {
            if (!playerController.player.playerInventory.isDualWielding)
            {
                //tPersonController.GetComponent<Animator>().SetTrigger("Fire");
                weaponToShoot.GetComponent<Animator>().Play("Fire", 0, 0f);
            }
            else
            {
                weaponToShoot.GetComponent<Animator>().Play("dw fire", 0, 0f);
            }
            //if (pInventory.isDualWielding)
            //    activeWeapon.leftWeapon.GetComponent<Animator>().Play("Fire", 0, 0f);
            //StartCoroutine(Player3PSFiringAnimation());
        }
        catch (System.Exception e) { Debug.LogError(e); }
        Debug.Log("Calling Recoil()");
        weaponToShoot.Recoil();
        //if (pInventory.isDualWielding)
        //    activeWeapon.rightWeapon.Recoil();


        if (!isLeftWeapon)
        {
            GetComponent<AudioSource>().clip = weaponToShoot.Fire;
            GetComponent<AudioSource>().Play();
        }
        else
        {
            _leftWeaponAudioSource.clip = weaponToShoot.Fire;
            _leftWeaponAudioSource.Play();
        }




        OnBulletSpawned?.Invoke(this);
    }

    [PunRPC]
    void SpawnFakeExplosiveProjectile_RPC(int projectileIndex, Vector3 pos, Vector3 rot)
    {
        print($"SpawnFakeExplosiveProjectile_RPC {projectileIndex}");

        WeaponProperties activeWeapon = pInventory.activeWeapon.GetComponent<WeaponProperties>();

        if (activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Rocket)
        {
            GrenadePool.SpawnRocket(playerController.player, projectileIndex, pos, rot);
        }
        else if (activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Grenade)
        {
            GrenadePool.SpawnGrenadeLauncherProjectile(playerController.player, projectileIndex, pos, rot);
        }







        //ExplosiveProjectile rocket = null;

        //if (activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Rocket)
        //{
        //    //rocket = Instantiate(playerController.GetComponent<GeneralWeapProperties>().rocketProjectilePrefab).GetComponent<Rocket>();
        //    rocket = Instantiate(playerController.GetComponent<GeneralWeapProperties>().rocketProjectilePrefab).GetComponent<ExplosiveProjectile>();
        //}
        //else if (activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Grenade)
        //    rocket = Instantiate(playerController.GetComponent<GeneralWeapProperties>().grenadeLauncherProjectilePrefab).GetComponent<ExplosiveProjectile>();




        //foreach (PlayerHitbox hb in playerController.player.hitboxes)
        //    Physics.IgnoreCollision(rocket.GetComponent<Collider>(), hb.GetComponent<Collider>()); // Prevents the grenade from colliding with the player who threw it



        //Debug.Log($"{playerController.name} PlayerShooting: {rocket.name}");
        //rocket.player = playerController.player;


        //if (PV.IsMine)
        //    rocket.gameObject.layer = 8;
        //else
        //    rocket.gameObject.layer = 0;

        //rocket.transform.position = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.position;
        //rocket.transform.rotation = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.rotation;

        //rocket.gameObject.GetComponent<ExplosiveProjectile>().player = playerController.GetComponent<GeneralWeapProperties>().GetComponent<Player>();
    }


    IEnumerator Player3PSFiringAnimation()
    {
        //tPersonController.GetComponent<Animator>().Play("Fire");
        yield return new WaitForEndOfFrame();
    }
    void FireCooldown()
    {
        if (_fireRecovery > 0)
            _fireRecovery -= Time.deltaTime;

        if (_leftFireRecovery > 0)
            _leftFireRecovery -= Time.deltaTime;
    }


    public void StopBurstFiring()
    {
        StopAllCoroutines();
    }


    void OnPlayerDeath_Delegate(Player p)
    {
        _overchargeFloat = _overchargeFloat_thirdWeapon = 0;
    }
}
