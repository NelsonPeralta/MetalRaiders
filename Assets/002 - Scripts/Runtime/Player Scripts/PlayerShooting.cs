using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using System;
using Steamworks;
using static GameObjectPool;

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
                if (_trackingTarget != null) Log.Print($"{playerController.player.name} updated tracking target to {_trackingTarget.name}");
                else Log.Print("Cleared tracking target");

                if (playerController.player.isMine)
                    playerController.UpdateTrackingTargetForOtherPlayers(true, (_trackingTarget != null) ? _trackingTarget.originalSpawnPosition : Vector3.zero);
            }
        }
    }

    public float fireRecovery { get { return _fireRecovery; } }
    public float leftWeaponFireRecovery { get { return _leftFireRecovery; } }
    public bool overchargeReady { get { return _overchargeFloat >= WeaponProperties.OVERCHARGE_TIME_LOW; } }
    public bool overchargeReadyLeftWeapon { get { return _overchargeFloat_thirdWeapon >= WeaponProperties.OVERCHARGE_TIME_LOW; } }
    public bool fireButtonDown
    {
        get { return _fireButtonDown; }
        set
        {
            _fireButtonDown = value;
            Log.Print($"Fire Button Down: {value}");
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
        if (GameManager.instance.connection == GameManager.NetworkType.Local || GameManager.instance.nbLocalPlayersPreset > 1)
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

        Log.Print($"OnPlayerControllerFireUp_Delegate 1");


        if (!playerController.player.isRespawning && !playerController.player.isDead)
            if (playerController.player.playerInventory.activeWeapon && playerController.player.playerInventory.activeWeapon.overcharge)
            {
                Log.Print($"OnPlayerControllerFireUp_Delegate 2 {pInventory.activeWeapon.overheatCooldown} {pInventory.activeWeapon.loadedAmmo}");

                if (pInventory.activeWeapon.overheatCooldown <= 0 && pInventory.activeWeapon.loadedAmmo > 0)
                {
                    Log.Print($"OnPlayerControllerFireUp_Delegate 3");

                    if (_overchargeFloat > (WeaponProperties.OVERCHARGE_TIME_FULL))
                    {
                        Log.Print("SHOOT OVERCHARGED SHOT");
                        ShootOverchargeWeapon(playerController.player.playerInventory.activeWeapon, true);
                    }
                    else
                    {
                        Log.Print("Shoot normal shot");
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
                Log.Print($"OnPlayerFireDualWieldedWeaponButtonUp_Delegate 2 {pInventory.thirdWeapon.overheatCooldown} {pInventory.thirdWeapon.loadedAmmo}");

                if (pInventory.thirdWeapon.overheatCooldown <= 0 && pInventory.thirdWeapon.loadedAmmo > 0)
                {
                    Log.Print($"OnPlayerFireDualWieldedWeaponButtonUp_Delegate 3");

                    if (_overchargeFloat_thirdWeapon > (WeaponProperties.OVERCHARGE_TIME_FULL))
                    {
                        Log.Print("OnPlayerFireDualWieldedWeaponButtonUp_Delegate SHOOT OVERCHARGED SHOT");
                        ShootOverchargeWeapon(playerController.player.playerInventory.thirdWeapon, true);
                    }
                    else
                    {
                        Log.Print("OnPlayerFireDualWieldedWeaponButtonUp_Delegate Shoot normal shot");
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
        Log.Print("Calling Shoot");
        if (playerController.isDrawingWeapon) return;
        /*if (wp == playerController.player.playerInventory.thirdWeapon)*/
        Log.Print($"Shoot start 1 {_fireRecovery} {_leftFireRecovery} {wp == playerController.player.playerInventory.activeWeapon}");





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
        Log.Print($"Shoot start 2 {wp == playerController.player.playerInventory.activeWeapon} {_fireRecovery} {_leftFireRecovery} {wp.overcharge} {wp}");

        if (CanShootAuto(wp) || CanShootSingleOrBurst(wp))
        {
            if (wp == playerController.player.playerInventory.activeWeapon)
                fireButtonDown = true;
            else
                dualWieldedWeaponFireButnDown = true;

            /*if (wp == playerController.player.playerInventory.thirdWeapon)*/
            Log.Print($"Shoot start 3 {wp == playerController.player.playerInventory.activeWeapon} {wp.overcharge} {wp}");


            if (wp.firingMode == WeaponProperties.FiringMode.Burst)
                ShootBurst(wp);
            else if (!wp.overcharge)
                Shoot_Caller(wp == playerController.player.playerInventory.thirdWeapon);
            else if (wp.overcharge)
            {
                //PrintOnlyInEditor.Log("shooting overcharg");
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
                    Log.Print("shooting overcharg");
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
                    Log.Print("shooting overcharg");
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
        Log.Print($"CanShootSingleOrBurst: {weaponToShoot.firingMode} {fireButtonDown} {dualWieldedWeaponFireButnDown} {pInventory.isDualWielding}");


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
        Log.Print("Shoot Burst");
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
        Log.Print($"Shoot_Caller: {playerController.player.name} Shoot_Caller {weap.name} {weap.loadedAmmo} {isLeftWeapon}");

        if (isLeftWeapon) weap = pInventory.thirdWeapon;

        if (!playerController.player.isDualWielding)
        {
            if (weap.loadedAmmo <= 0 || playerController.isReloading)
            {
                /*if (activeWeapon == playerController.player.playerInventory.thirdWeapon)*/
                Log.Print($"{playerController.player.name} Shoot_Caller {weap.name} {weap.loadedAmmo} isReloading: {playerController.isReloading}");
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
                    Log.Print($"{playerController.player.name} Shoot_Caller {weap.name} {weap.loadedAmmo} isReloading: {playerController.isReloading}");
                    return;
                }
            }
            else
            {
                if (weap.loadedAmmo <= 0 || playerController.isReloadingLeft)
                {
                    /*if (activeWeapon == playerController.player.playerInventory.thirdWeapon)*/
                    Log.Print($"{playerController.player.name} Shoot_Caller {weap.name} {weap.loadedAmmo} isReloading: {playerController.isReloading}");
                    return;
                }
            }
        }






        //if (activeWeapon.loadedAmmo <= 0 || playerController.isReloading)
        //{
        //    /*if (activeWeapon == playerController.player.playerInventory.thirdWeapon)*/
        //    Log.Print($"{playerController.player.name} Shoot_Caller {activeWeapon.name} {activeWeapon.loadedAmmo} isReloading: {playerController.isReloading}");
        //    return;
        //}

        shoooo(isLeftWeapon, overcharge);
        if (weap.killFeedOutput == WeaponProperties.KillFeedOutput.Plasma_Blaster) shoooo(isLeftWeapon, overcharge, true);
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
    List<RaycastHit> fakeBulletTrailRaycasthits = new List<RaycastHit>(16);
    readonly RaycastHit[] _fakeTrailHitsBuffer = new RaycastHit[16];

    void shoooo(bool isLeftWeapon = false, bool overcharge = false, bool fakeBulForPB = false)
    {
        // Cache frequently used components / properties to avoid repeated GetComponent calls
        var gwp = playerController.GetComponent<GeneralWeapProperties>();
        gwp.ResetLocalTransform();

        var pcPlayerComp = playerController.GetComponent<Player>();
        if (pcPlayerComp.isDead || pcPlayerComp.isRespawning)
            return;

        var activeWeapon = playerController.pInventory.activeWeapon;
        var activeWeaponType = playerController.pInventory.activeWeapon.weaponType;

        if (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On || activeWeaponType == WeaponProperties.WeaponType.Heavy)
        {
            var aimAssist = pcPlayerComp.aimAssist;
            var tpsCtrl = playerController.gwProperties.tpsBulletRotationToCameraCenterControl;
            tpsCtrl.localRotation = Quaternion.identity;

            if (!aimAssist.redReticuleIsOn && !aimAssist.invisibleAimAssistOn)
            {
                Log.Print("Third Person Mode NO aim assist");
                tpsCtrl.LookAt(playerController.player.playerCamera.playerCameraCenterPointCheck.target.position);
            }
            else
            {
                Log.Print("Third Person Mode aim assist");
                tpsCtrl.LookAt(pcPlayerComp.aimAssist.closestHbToCrosshairCenter.transform.position);
            }
        }

        playerController.SetCurrentlyShootingReset();
        playerController.player.assignActorPlayerTargetOnShootingSphere.TriggerBehaviour();

        int counter = 1;
        WeaponProperties weaponToShoot = pInventory.activeWeapon.GetComponent<WeaponProperties>();
        if (isLeftWeapon) weaponToShoot = pInventory.thirdWeapon;

        if (weaponToShoot.isShotgun)
        {
            counter = weaponToShoot.numberOfPellets;
            // you're overwriting counter with pelletSpawnPoints.Count in original; preserve that:
            counter = gwp.pelletSpawnPoints.Count;
            if (weaponToShoot.isShotgun)
            {
                // pre-fill quats list with identity if original did it (preserve original behavior)
                for (int j = 0; j < gwp.pelletSpawnPoints.Count; j++)
                    quats.Add(Quaternion.Euler(Vector3.zero));
            }
        }

        // Loop through pellets / shots
        for (int i = 0; i < counter; i++)
        {
            if (weaponToShoot.ammoProjectileType != WeaponProperties.AmmoProjectileType.Bullet &&
                weaponToShoot.ammoProjectileType != WeaponProperties.AmmoProjectileType.Plasma)
                continue;

            Player player = playerController.GetComponent<Player>();
            Quaternion ranSprayQuat = weaponToShoot.GetRandomSprayRotation();

            if (playerController.isAiming && weaponToShoot.hipSprayOnly)
                ranSprayQuat = Quaternion.identity;

            if (weaponToShoot.ammoProjectileType == WeaponProperties.AmmoProjectileType.Bullet || (fakeBulForPB && GameManager.instance.nbLocalPlayersPreset > 1))
            {
                if (weaponToShoot == playerController.player.playerInventory.thirdWeapon)
                    Log.Print("spawning FAKE bullet");

                // perform fake bullet trail logic (was using RaycastAll + LINQ)
                {
                    // RaycastNonAlloc into buffer to avoid allocations
                    int hitCount = Physics.RaycastNonAlloc(
                        player.mainCamera.transform.position,
                        player.mainCamera.transform.forward,
                        _fakeTrailHitsBuffer,
                        playerController.pInventory.activeWeapon.range,
                        _fakeBulletTrailCollisionLayerMask
                    );

                    // reuse list: clear and populate from buffer, skipping ManCannon (GetComponent check)
                    fakeBulletTrailRaycasthits.Clear();
                    for (int h = 0; h < hitCount; h++)
                    {
                        var rh = _fakeTrailHitsBuffer[h];
                        // preserve original filtering of ManCannon if present
                        if (rh.collider != null && rh.collider.GetComponent<ManCannon>() != null) continue;
                        fakeBulletTrailRaycasthits.Add(rh);
                    }

                    if (fakeBulletTrailRaycasthits.Count <= 0)
                    {
                        // no colliders found -> spawn max length fake trail
                        pInventory.SpawnFakeBulletTrail(fakeBulForPB ? PlayerInventory.FakeTrailColor.red : PlayerInventory.FakeTrailColor.yellow,
                            (int)weaponToShoot.range,
                            ranSprayQuat,
                            player.isMine,
                            muzzlePosition: weaponToShoot.tpsMuzzleFlash.transform.position);
                    }
                    else
                    {
                        // remove hits that originate from this player's root
                        for (int j = fakeBulletTrailRaycasthits.Count - 1; j >= 0; j--)
                        {
                            if (fakeBulletTrailRaycasthits[j].collider != null &&
                                fakeBulletTrailRaycasthits[j].collider.transform.root == player.transform)
                            {
                                fakeBulletTrailRaycasthits.RemoveAt(j);
                            }
                        }

                        // sort in-place by squared distance (preserves same order as Distance)
                        var camPos = player.mainCamera.transform.position;
                        fakeBulletTrailRaycasthits.Sort((a, b) =>
                        {
                            float da = (a.point - camPos).sqrMagnitude;
                            float db = (b.point - camPos).sqrMagnitude;
                            return da.CompareTo(db);
                        });

                        if (fakeBulletTrailRaycasthits.Count > 0)
                        {
                            // preserve original debug prints and branching logic
                            Log.Print("There is a collider at the center of our main camera. " +
                                $"The dot product is: {Vector3.Dot(fakeBulletTrailRaycasthits[0].point - player.mainCamera.transform.position, fakeBulletTrailRaycasthits[0].point - playerController.pInventory.activeWeapon.tpsMuzzleFlash.transform.position)}");

                            Log.Print($"The distance between the muzzle and the hit at the center of the camera is: " +
                                $"{Vector3.Distance(playerController.pInventory.activeWeapon.tpsMuzzleFlash.transform.position, fakeBulletTrailRaycasthits[0].point)}");

                            var firstPoint = fakeBulletTrailRaycasthits[0].point;
                            var muzzlePos = playerController.pInventory.activeWeapon.tpsMuzzleFlash.transform.position;
                            if (Vector3.Dot(firstPoint - player.mainCamera.transform.position, firstPoint - muzzlePos) > 0)
                            {
                                if (Vector3.Distance(muzzlePos, firstPoint) > 4f)
                                {
                                    pInventory.SpawnFakeBulletTrail(
                                        fakeBulForPB ? PlayerInventory.FakeTrailColor.red : PlayerInventory.FakeTrailColor.yellow,
                                        (int)Vector3.Distance(muzzlePos, firstPoint),
                                        ranSprayQuat,
                                        player.isMine,
                                        muzzlePosition: weaponToShoot.tpsMuzzleFlash.transform.position,
                                        lookAtThisTarget: (playerController.player.aimAssist.targetPointPosition != Vector3.zero ? playerController.player.aimAssist.targetPointPosition : firstPoint)
                                    );
                                }
                                else
                                {
                                    // do not show fake trail if too close (preserve original behavior)
                                }
                            }
                            else
                            {
                                pInventory.SpawnFakeBulletTrail(
                                    fakeBulForPB ? PlayerInventory.FakeTrailColor.red : PlayerInventory.FakeTrailColor.yellow,
                                    (int)playerController.pInventory.activeWeapon.range,
                                    ranSprayQuat,
                                    player.isMine,
                                    muzzlePosition: weaponToShoot.tpsMuzzleFlash.transform.position
                                );
                            }
                        }
                        else
                        {
                            pInventory.SpawnFakeBulletTrail(
                                fakeBulForPB ? PlayerInventory.FakeTrailColor.red : PlayerInventory.FakeTrailColor.yellow,
                                (int)playerController.pInventory.activeWeapon.range,
                                ranSprayQuat,
                                player.isMine,
                                muzzlePosition: weaponToShoot.tpsMuzzleFlash.transform.position
                            );
                        }
                    }
                }
            }

            // --- projectile spawning logic (unchanged) ---
            if (player.isMine || weaponToShoot.ammoProjectileType == WeaponProperties.AmmoProjectileType.Plasma)
            {
                GameObject bullet = null;

                if (!fakeBulForPB)
                {
                    GameObjectPool.BulletType bulletType = GameObjectPool.BulletType.normal;
                    if (weaponToShoot.ammoProjectileType == WeaponProperties.AmmoProjectileType.Plasma)
                    {
                        switch (weaponToShoot.plasmaColor)
                        {
                            case WeaponProperties.PlasmaColor.Blue: bulletType = GameObjectPool.BulletType.blue_plasma_round; break;
                            case WeaponProperties.PlasmaColor.Green: bulletType = GameObjectPool.BulletType.green_plasma_round; break;
                            case WeaponProperties.PlasmaColor.Shard: bulletType = GameObjectPool.BulletType.shard_round; break;
                            default: bulletType = GameObjectPool.BulletType.normal; break;
                        }
                    }
                    bullet = GameObjectPool.instance.SpawnPooledBullet(bulletType);
                }
                else
                {
                    Log.Print($"Shooting Plasma bullet 0 {fakeBulForPB == true} {overcharge} {weaponToShoot.targetTracking}");
                    if (GameManager.instance.nbLocalPlayersPreset == 1)
                    {
                        Log.Print($"Shooting Plasma bullet 1 {fakeBulForPB == true} {overcharge} {weaponToShoot.targetTracking}");
                        bullet = GameObjectPool.instance.SpawnPooledBullet(BulletType.red_plasma_round);
                        bullet.transform.localScale = Vector3.one;

                        var bComp = bullet.GetComponent<Bullet>();
                        if (bComp != null)
                        {
                            bComp.damage = 0;
                            bComp.speed = 150;
                            bComp.range = 100;
                            bComp.weaponProperties = weaponToShoot;
                        }

                        bullet.transform.position = gwp.bulletSpawnPoint.transform.position;
                        bullet.transform.rotation = gwp.bulletSpawnPoint.transform.rotation;
                        bullet.SetActive(true);
                        return;
                    }
                    else if (GameManager.instance.nbLocalPlayersPreset > 1)
                    {
                        Log.Print($"Shooting Plasma bullet 2 {fakeBulForPB == true} {overcharge} {weaponToShoot.targetTracking}");
                        return;
                    }
                    else
                    {
                        Log.Print($"Shooting Plasma bullet 3 {fakeBulForPB == true} {overcharge} {weaponToShoot.targetTracking}");
                    }
                }

                Log.Print($"Shooting Plasma bullet {fakeBulForPB == true} {overcharge} {weaponToShoot.targetTracking}");
                bullet.transform.localScale = Vector3.one;

                var bulletComp = bullet.GetComponent<Bullet>();
                if (bulletComp != null) bulletComp.weaponProperties = weaponToShoot;

                if (overcharge) bullet.transform.localScale = Vector3.one * 5f;

                if (bulletComp != null)
                {
                    bulletComp.overcharged = false;
                    bulletComp.trackingTarget = null;
                }

                if (weaponToShoot.targetTracking)
                {
                    Log.Print($"Bullet 1");
                    if (!weaponToShoot.overcharge)
                    {
                        Log.Print($"Bullet 2");
                        if (bulletComp != null) bulletComp.trackingTarget = trackingTarget;
                    }
                    else
                    {
                        Log.Print($"Bullet 3");
                        if (overcharge)
                        {
                            Log.Print($"Bullet 4");
                            if (bulletComp != null)
                            {
                                bulletComp.trackingTarget = trackingTarget;
                                bulletComp.overcharged = overcharge;
                                bulletComp.damage = 25;
                            }
                        }
                    }
                }
                Log.Print($"Active weapon has target tracking: {weaponToShoot.targetTracking}. PlayerShooting script has tracking target {trackingTarget}. {overcharge}");

                Log.Print(bullet);
                Log.Print(weaponToShoot);

                var disableComp = bullet.GetComponent<DisableAfterXSeconds>();
                if (disableComp != null) disableComp.enabled = false;

                if (!pcPlayerComp.aimAssist.redReticuleIsOn && !pcPlayerComp.aimAssist.invisibleAimAssistOn)
                    gwp.ResetLocalTransform();

                try
                {
                    if (weaponToShoot.isShotgun)
                        quats[i] = UnityEngine.Random.rotation;
                    else
                        gwp.bulletSpawnPoint.transform.localRotation *= ranSprayQuat;
                }
                catch { }

                if (weaponToShoot.isShotgun)
                {
                    quats[i] = UnityEngine.Random.rotation;
                    bullet.transform.position = gwp.pelletSpawnPoints[i].position;
                    bullet.transform.rotation = gwp.pelletSpawnPoints[i].rotation;
                    bullet.transform.rotation = Quaternion.RotateTowards(gwp.pelletSpawnPoints[i].rotation, quats[i], weaponToShoot.bulletSpray);
                }
                else
                {
                    bullet.transform.position = gwp.bulletSpawnPoint.transform.position;
                    bullet.transform.rotation = gwp.bulletSpawnPoint.transform.rotation;
                }

                // cache bullet component operations
                var bComp2 = bullet.GetComponent<Bullet>();
                if (bComp2 != null)
                {
                    bComp2.sourcePlayer = playerController.GetComponent<Player>();
                    bComp2.weaponProperties = weaponToShoot;
                    bComp2.damage = (int)(playerController.GetComponent<Player>().playerInventory.activeWeapon.damage * (player.isDualWielding ? 0.75f : 1));
                    if (overcharge) bComp2.damage *= 5;
                    bComp2.range = (int)weaponToShoot.range;
                    bComp2.speed = (int)weaponToShoot.bulletSpeed;

                    if (weaponToShoot.hybridHitscan && player.aimAssist.redReticuleIsOn)
                    {
                        bComp2.speed = 999;
                        Log.Print("hybrid hitscan working");
                    }

                    if (overcharge) bComp2.speed = (int)(weaponToShoot.bulletSpeed * 0.65f);
                }

                Log.Print($"bullet time test. Spawned at: {Time.time}");
                bullet.SetActive(true);

                if (weaponToShoot.plasmaColor != WeaponProperties.PlasmaColor.Shard)
                {
                    weaponToShoot.currentOverheat = Mathf.Clamp(weaponToShoot.currentOverheat + weaponToShoot.overheatPerShot, 0, 100);
                    if (overcharge) weaponToShoot.currentOverheat = 100;
                }
            }
            // End projectile spawning logic

            weaponToShoot.SpawnMuzzleflash();
        } // end for

        if (!PV.IsMine)
            _ignoreShootCounter++;

        if (PV.IsMine)
        {
            if (!overcharge)
            {
                Log.Print("removing 1 loaded ammo");
                weaponToShoot.loadedAmmo -= 1;
            }
            else
                weaponToShoot.loadedAmmo -= 10;
        }

        try
        {
            if (!playerController.player.playerInventory.isDualWielding)
                weaponToShoot.GetComponent<Animator>().Play("Fire", 0, 0f);
            else
                weaponToShoot.GetComponent<Animator>().Play("dw fire", 0, 0f);
        }
        catch (System.Exception e) { Log.PrintError(e); }

        Log.Print("Calling Recoil()");
        weaponToShoot.Recoil();

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








    //int _ignoreShootCounter;
    //List<RaycastHit> fakeBulletTrailRaycasthits = new List<RaycastHit>();

    //// size tuneable to expected maximum hits per frame
    //readonly RaycastHit[] _fakeTrailHitsBuffer = new RaycastHit[16];


    //void shoooo(bool isLeftWeapon = false, bool overcharge = false, bool fakeBulForPB = false)
    //{
    //    playerController.GetComponent<GeneralWeapProperties>().ResetLocalTransform();


    //    if (playerController.GetComponent<Player>().isDead || playerController.GetComponent<Player>().isRespawning)
    //        return;

    //    if (GameManager.instance.thirdPersonMode == GameManager.ThirdPersonMode.On || playerController.pInventory.activeWeapon.weaponType == WeaponProperties.WeaponType.Heavy)
    //    {
    //        if (!playerController.GetComponent<Player>().aimAssist.redReticuleIsOn && !playerController.GetComponent<Player>().aimAssist.invisibleAimAssistOn)
    //        {
    //            Log.Print("Third Person Mode NO aim assist");
    //            // if the player is allowed no aim assist at all
    //            // we will simply correct the angle so that the bullet is fired at whatever is at the center of the players main cam
    //            playerController.gwProperties.tpsBulletRotationToCameraCenterControl.localRotation = Quaternion.identity;

    //            playerController.gwProperties.tpsBulletRotationToCameraCenterControl.LookAt(playerController.player.playerCamera.playerCameraCenterPointCheck.target.position);
    //        }
    //        else
    //        {
    //            Log.Print("Third Person Mode aim assist");
    //            // if the player is allowed a certain amount of aim assist, do nothing special for now
    //            // make sure its reset so it does not come in control with the aim assist rotation control
    //            playerController.gwProperties.tpsBulletRotationToCameraCenterControl.localRotation = Quaternion.identity;
    //            playerController.gwProperties.tpsBulletRotationToCameraCenterControl.LookAt(playerController.player.aimAssist.closestHbToCrosshairCenter.transform.position);
    //            //if (playerController.player.aimAssist.targetPointPosition != Vector3.zero)
    //            //    playerController.gwProperties.tpsBulletRotationToCameraCenterControl.LookAt(playerController.player.aimAssist.targetPointPosition);
    //        }
    //    }

    //    playerController.SetCurrentlyShootingReset();

    //    playerController.player.assignActorPlayerTargetOnShootingSphere.TriggerBehaviour();

    //    //Log.Print($"shoooo 1 {isLeftWeapon}");

    //    int counter = 1;
    //    WeaponProperties weaponToShoot = pInventory.activeWeapon.GetComponent<WeaponProperties>();

    //    if (isLeftWeapon) weaponToShoot = pInventory.thirdWeapon;

    //    if (weaponToShoot.isShotgun)
    //    {
    //        counter = weaponToShoot.numberOfPellets;
    //        counter = playerController.GetComponent<GeneralWeapProperties>().pelletSpawnPoints.Count;
    //        if (weaponToShoot.isShotgun)
    //            for (int j = 0; j < playerController.GetComponent<GeneralWeapProperties>().pelletSpawnPoints.Count; j++)
    //                quats.Add(Quaternion.Euler(Vector3.zero));
    //    }

    //    for (int i = 0; i < counter; i++)
    //        if (weaponToShoot.ammoProjectileType == WeaponProperties.AmmoProjectileType.Bullet || weaponToShoot.ammoProjectileType == WeaponProperties.AmmoProjectileType.Plasma)
    //        {
    //            Player player = playerController.GetComponent<Player>();
    //            Quaternion ranSprayQuat = weaponToShoot.GetRandomSprayRotation();

    //            if (playerController.isAiming && weaponToShoot.hipSprayOnly)
    //                ranSprayQuat = Quaternion.identity;

    //            if (weaponToShoot.ammoProjectileType == WeaponProperties.AmmoProjectileType.Bullet || (fakeBulForPB == true && GameManager.instance.nbLocalPlayersPreset > 1))
    //            {
    //                //if (!player.isMine/* || GameManager.instance.connection == GameManager.Connection.Local*/)
    //                {
    //                    if (weaponToShoot == playerController.player.playerInventory.thirdWeapon) Log.Print("spawning FAKE bullet");




    //                    //if (GameManager.instance.nbLocalPlayersPreset > 0 || weaponToShoot.isShotgun)
    //                    {
    //                        fakeBulletTrailRaycasthits = Physics.RaycastAll(player.mainCamera.transform.position, player.mainCamera.transform.forward, playerController.pInventory.activeWeapon.range, _fakeBulletTrailCollisionLayerMask).ToList();

    //                        if (fakeBulletTrailRaycasthits.Count <= 0)
    //                        {

    //                            // if we find no colliders (default or hitboxes), shoot a fake trail at its maximum lenght
    //                            pInventory.SpawnFakeBulletTrail(fakeBulForPB ? PlayerInventory.FakeTrailColor.red : PlayerInventory.FakeTrailColor.yellow, (int)weaponToShoot.range,
    //                                ranSprayQuat, player.isMine,
    //                               muzzlePosition: weaponToShoot.tpsMuzzleFlash.transform.position);
    //                        }
    //                        else
    //                        {
    //                            for (int j = fakeBulletTrailRaycasthits.Count; j-- > 0;)
    //                                if (fakeBulletTrailRaycasthits[j].collider.transform.root == player.transform)
    //                                    fakeBulletTrailRaycasthits.Remove(fakeBulletTrailRaycasthits[j]);
    //                            fakeBulletTrailRaycasthits = fakeBulletTrailRaycasthits.OrderBy(item => Vector3.Distance(player.mainCamera.transform.position, item.point)).ToList();






    //                            if (fakeBulletTrailRaycasthits.Count > 0)
    //                            {

    //                                Log.Print("There is a collider at the center of our main camera. " +
    //                                $"The dot product is: {Vector3.Dot(fakeBulletTrailRaycasthits[0].point - player.mainCamera.transform.position, fakeBulletTrailRaycasthits[0].point - playerController.pInventory.activeWeapon.tpsMuzzleFlash.transform.position)}");

    //                                Log.Print($"The distance between the muzzle and the hit at the center of the camera is: " +
    //                                    $"{Vector3.Distance(playerController.pInventory.activeWeapon.tpsMuzzleFlash.transform.position, fakeBulletTrailRaycasthits[0].point)}");

    //                                if (Vector3.Dot(fakeBulletTrailRaycasthits[0].point - player.mainCamera.transform.position,
    //                                    fakeBulletTrailRaycasthits[0].point - playerController.pInventory.activeWeapon.tpsMuzzleFlash.transform.position) > 0)
    //                                {
    //                                    if (Vector3.Distance(playerController.pInventory.activeWeapon.tpsMuzzleFlash.transform.position, fakeBulletTrailRaycasthits[0].point) > 4)
    //                                    {
    //                                        pInventory.SpawnFakeBulletTrail(fakeBulForPB ? PlayerInventory.FakeTrailColor.red : PlayerInventory.FakeTrailColor.yellow, (int)Vector3.Distance(playerController.pInventory.activeWeapon.tpsMuzzleFlash.transform.position, fakeBulletTrailRaycasthits[0].point),
    //                                            ranSprayQuat, player.isMine,
    //                               muzzlePosition: weaponToShoot.tpsMuzzleFlash.transform.position,
    //                               lookAtThisTarget: (playerController.player.aimAssist.targetPointPosition != Vector3.zero ? playerController.player.aimAssist.targetPointPosition : fakeBulletTrailRaycasthits[0].point));
    //                                    }
    //                                    else
    //                                    {
    //                                        // The target may be too close to the player. The trail could be unrealistically warped
    //                                        // this could break immersion
    //                                        // we will NOT show a fake trail
    //                                    }
    //                                }
    //                                else
    //                                {
    //                                    // The target may be between the position of the camera and the end of the muzzle of the gun
    //                                    // the player may be in 3PS mode

    //                                    pInventory.SpawnFakeBulletTrail(fakeBulForPB ? PlayerInventory.FakeTrailColor.red : PlayerInventory.FakeTrailColor.yellow, (int)Vector3.Distance(playerController.pInventory.activeWeapon.tpsMuzzleFlash.transform.position, playerController.player.playerCamera.playerCameraCenterPointCheck.target.position),
    //                                ranSprayQuat, player.isMine,
    //                               muzzlePosition: weaponToShoot.tpsMuzzleFlash.transform.position);
    //                                }
    //                            }
    //                            else
    //                            {
    //                                pInventory.SpawnFakeBulletTrail(fakeBulForPB ? PlayerInventory.FakeTrailColor.red : PlayerInventory.FakeTrailColor.yellow, (int)playerController.pInventory.activeWeapon.range,
    //                                ranSprayQuat, player.isMine,
    //                               muzzlePosition: weaponToShoot.tpsMuzzleFlash.transform.position);
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //            else
    //            {

    //            }



    //            if (player.isMine || weaponToShoot.ammoProjectileType == WeaponProperties.AmmoProjectileType.Plasma)
    //            {
    //                GameObject bullet = null;

    //                if (!fakeBulForPB)
    //                {
    //                    GameObjectPool.BulletType bulletType = GameObjectPool.BulletType.normal;

    //                    if (weaponToShoot.ammoProjectileType == WeaponProperties.AmmoProjectileType.Plasma)
    //                    {
    //                        switch (weaponToShoot.plasmaColor)
    //                        {
    //                            case WeaponProperties.PlasmaColor.Blue:
    //                                bulletType = GameObjectPool.BulletType.blue_plasma_round;
    //                                break;
    //                            case WeaponProperties.PlasmaColor.Green:
    //                                bulletType = GameObjectPool.BulletType.green_plasma_round;
    //                                break;
    //                            case WeaponProperties.PlasmaColor.Shard:
    //                                bulletType = GameObjectPool.BulletType.shard_round;
    //                                break;
    //                            default:
    //                                bulletType = GameObjectPool.BulletType.normal;
    //                                break;
    //                        }
    //                    }

    //                    bullet = GameObjectPool.instance.SpawnPooledBullet(bulletType);
    //                }
    //                else
    //                {
    //                    Log.Print($"Shooting Plasma bullet 0 {fakeBulForPB == true} {overcharge} {weaponToShoot.targetTracking}");
    //                    if (GameManager.instance.nbLocalPlayersPreset == 1)
    //                    {
    //                        Log.Print($"Shooting Plasma bullet 1 {fakeBulForPB == true} {overcharge} {weaponToShoot.targetTracking}");
    //                        bullet = GameObjectPool.instance.SpawnPooledBullet(BulletType.red_plasma_round);
    //                        bullet.transform.localScale = Vector3.one;

    //                        bullet.GetComponent<Bullet>().damage = 0;
    //                        bullet.GetComponent<Bullet>().speed = 150;
    //                        bullet.GetComponent<Bullet>().range = 100;
    //                        bullet.GetComponent<Bullet>().weaponProperties = weaponToShoot;
    //                        bullet.transform.position = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.position;
    //                        bullet.transform.rotation = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.rotation;

    //                        bullet.SetActive(true);

    //                        return;
    //                    }
    //                    else if (GameManager.instance.nbLocalPlayersPreset > 1)
    //                    {
    //                        Log.Print($"Shooting Plasma bullet 2 {fakeBulForPB == true} {overcharge} {weaponToShoot.targetTracking}");
    //                        return;
    //                    }
    //                    else
    //                    {
    //                        Log.Print($"Shooting Plasma bullet 3 {fakeBulForPB == true} {overcharge} {weaponToShoot.targetTracking}");

    //                    }
    //                }



    //                Log.Print($"Shooting Plasma bullet {fakeBulForPB == true} {overcharge} {weaponToShoot.targetTracking}");
    //                bullet.transform.localScale = Vector3.one;
    //                try { bullet.gameObject.GetComponent<Bullet>().weaponProperties = weaponToShoot; } catch { }

    //                if (overcharge) bullet.transform.localScale = Vector3.one * 5;

    //                bullet.GetComponent<Bullet>().overcharged = false;
    //                bullet.GetComponent<Bullet>().trackingTarget = null;


    //                if (weaponToShoot.targetTracking)
    //                {
    //                    Log.Print($"Bullet 1");
    //                    if (!weaponToShoot.overcharge) // for Plasma Rifle{
    //                    {
    //                        Log.Print($"Bullet 2");

    //                        bullet.GetComponent<Bullet>().trackingTarget = trackingTarget;

    //                    }
    //                    else
    //                    {
    //                        Log.Print($"Bullet 3");

    //                        if (overcharge) // For plasma Pistol
    //                        {
    //                            Log.Print($"Bullet 4");

    //                            bullet.GetComponent<Bullet>().trackingTarget = trackingTarget;
    //                            bullet.GetComponent<Bullet>().overcharged = overcharge;
    //                            bullet.GetComponent<Bullet>().damage = 25;
    //                        }
    //                    }
    //                }
    //                Log.Print($"Active weapon has target tracking: {weaponToShoot.targetTracking}. PlayerShooting script has tracking target {trackingTarget}. {overcharge}");




    //                {
    //                    Log.Print(bullet);
    //                    Log.Print(weaponToShoot);

    //                    if (bullet.GetComponent<DisableAfterXSeconds>()) bullet.GetComponent<DisableAfterXSeconds>().enabled = false;
    //                }

    //                if (!playerController.GetComponent<Player>().aimAssist.redReticuleIsOn && !playerController.GetComponent<Player>().aimAssist.invisibleAimAssistOn)
    //                    playerController.GetComponent<GeneralWeapProperties>().ResetLocalTransform();


    //                try
    //                {
    //                    if (weaponToShoot.isShotgun)
    //                    {
    //                        quats[i] = UnityEngine.Random.rotation;
    //                    }
    //                    else
    //                        playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.localRotation *= ranSprayQuat;
    //                }
    //                catch { }


    //                if (weaponToShoot.isShotgun)
    //                {
    //                    quats[i] = UnityEngine.Random.rotation;

    //                    bullet.transform.position = playerController.GetComponent<GeneralWeapProperties>().pelletSpawnPoints[i].position;
    //                    bullet.transform.rotation = playerController.GetComponent<GeneralWeapProperties>().pelletSpawnPoints[i].rotation;

    //                    bullet.transform.rotation = Quaternion.RotateTowards(playerController.GetComponent<GeneralWeapProperties>().pelletSpawnPoints[i].rotation, quats[i], weaponToShoot.bulletSpray);
    //                }
    //                else
    //                {
    //                    bullet.transform.position = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.position;
    //                    bullet.transform.rotation = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.rotation;
    //                }



    //                try { bullet.gameObject.GetComponent<Bullet>().sourcePlayer = playerController.GetComponent<Player>(); } catch { }
    //                try { bullet.gameObject.GetComponent<Bullet>().weaponProperties = weaponToShoot; } catch { }
    //                try { bullet.gameObject.GetComponent<Bullet>().damage = (int)(playerController.GetComponent<Player>().playerInventory.activeWeapon.damage * (player.isDualWielding ? 0.75f : 1)); } catch { }


    //                if (overcharge) bullet.GetComponent<Bullet>().damage *= 5;



    //                bullet.gameObject.GetComponent<Bullet>().range = (int)weaponToShoot.range;
    //                bullet.gameObject.GetComponent<Bullet>().speed = (int)weaponToShoot.bulletSpeed;

    //                if (weaponToShoot.hybridHitscan /*&& weaponToShoot.ammoProjectileType == WeaponProperties.AmmoProjectileType.Bullet*/ && player.aimAssist.redReticuleIsOn) { bullet.gameObject.GetComponent<Bullet>().speed = 999; Log.Print("hybrid hitscan working"); }

    //                if (overcharge) bullet.gameObject.GetComponent<Bullet>().speed = (int)(weaponToShoot.bulletSpeed * 0.65f);
    //                Log.Print($"bullet time test. Spawned at: {Time.time}");

    //                //bullet.GetComponent<TrailRenderer>().enabled = GameManager.instance.nbLocalPlayersPreset == 1;
    //                bullet.SetActive(true);

    //                if (weaponToShoot.plasmaColor != WeaponProperties.PlasmaColor.Shard)
    //                {
    //                    weaponToShoot.currentOverheat = Mathf.Clamp(weaponToShoot.currentOverheat + weaponToShoot.overheatPerShot, 0, 100);

    //                    if (overcharge) weaponToShoot.currentOverheat = 100;
    //                }
    //            }
    //            weaponToShoot.SpawnMuzzleflash();
    //        }
    //        else if (weaponToShoot.ammoProjectileType == WeaponProperties.AmmoProjectileType.Rocket || weaponToShoot.ammoProjectileType == WeaponProperties.AmmoProjectileType.Grenade)
    //        {
    //            // Projectile does not spawn if ammo left is 0, lag
    //            if (playerController.player.isMine)
    //                if (weaponToShoot.ammoProjectileType == WeaponProperties.AmmoProjectileType.Rocket)
    //                {

    //                    PV.RPC("SpawnFakeExplosiveProjectile_RPC", RpcTarget.AllViaServer, GrenadePool.GetAvailableRocketAtIndex(playerController.player.playerDataCell.photonRoomIndex),
    //                    playerController.player.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.position, playerController.player.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.rotation.eulerAngles);
    //                }
    //                else if (weaponToShoot.ammoProjectileType == WeaponProperties.AmmoProjectileType.Grenade)
    //                {
    //                    PV.RPC("SpawnFakeExplosiveProjectile_RPC", RpcTarget.AllViaServer, GrenadePool.GetAvailableGrenadeLauncherProjectileAtIndex(playerController.player.playerDataCell.photonRoomIndex),
    //                    playerController.player.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.position, playerController.player.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.rotation.eulerAngles);
    //                }

    //            weaponToShoot.SpawnMuzzleflash();
    //        }

    //    if (!PV.IsMine)
    //        _ignoreShootCounter++;

    //    if (PV.IsMine)
    //    {
    //        if (!overcharge)
    //        {
    //            Log.Print("removing 1 loaded ammo");
    //            weaponToShoot.loadedAmmo -= 1;
    //        }
    //        else
    //            weaponToShoot.loadedAmmo -= 10;
    //    }

    //    try
    //    {
    //        if (!playerController.player.playerInventory.isDualWielding)
    //        {
    //            //tPersonController.GetComponent<Animator>().SetTrigger("Fire");
    //            weaponToShoot.GetComponent<Animator>().Play("Fire", 0, 0f);
    //        }
    //        else
    //        {
    //            weaponToShoot.GetComponent<Animator>().Play("dw fire", 0, 0f);
    //        }
    //    }
    //    catch (System.Exception e) { Log.PrintError(e); }
    //    Log.Print("Calling Recoil()");
    //    weaponToShoot.Recoil();


    //    if (!isLeftWeapon)
    //    {
    //        GetComponent<AudioSource>().clip = weaponToShoot.Fire;
    //        GetComponent<AudioSource>().Play();
    //    }
    //    else
    //    {
    //        _leftWeaponAudioSource.clip = weaponToShoot.Fire;
    //        _leftWeaponAudioSource.Play();
    //    }




    //    OnBulletSpawned?.Invoke(this);
    //}






    [PunRPC]
    void SpawnFakeExplosiveProjectile_RPC(int projectileIndex, Vector3 pos, Vector3 rot)
    {
        Log.Print($"SpawnFakeExplosiveProjectile_RPC {projectileIndex}");

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



        //Log.Print($"{playerController.name} PlayerShooting: {rocket.name}");
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
