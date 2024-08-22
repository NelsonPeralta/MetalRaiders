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

    [Header("Other Scripts")]
    public PhotonView PV;
    public PlayerController playerController;
    public PlayerInventory pInventory;
    public ThirdPersonScript tPersonController;

    // Private variables
    int playerRewiredID;
    [SerializeField] float _fireRecovery = 0, leftFireInterval = 0, _overchargeFloat;
    [SerializeField] bool fireButtonDown = false, scopeBtnDown = false;
    [SerializeField] LayerMask _fakeBulletTrailCollisionLayerMask;


    [SerializeField] Biped _trackingTarget, _preTrackingTarget;


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
        playerController.OnPlayerScopeBtnUp += OnPlayerControllerScopeUp_Delegate;
    }

    void OnPlayerControllerFireUp_Delegate(PlayerController playerController)
    {
        fireButtonDown = false;

        print($"OnPlayerControllerFireUp_Delegate 1");

        if (playerController.player.playerInventory.activeWeapon && playerController.player.playerInventory.activeWeapon.overcharge)
        {
            print($"OnPlayerControllerFireUp_Delegate 2 {pInventory.activeWeapon.overheatCooldown} {pInventory.activeWeapon.loadedAmmo}");

            if ((pInventory.activeWeapon.overheatCooldown <= 0 || pInventory.activeWeapon.allowSinglePlasmaBoltForNetworkedOverheat) && pInventory.activeWeapon.loadedAmmo > 0)
            {
                print($"OnPlayerControllerFireUp_Delegate 3");

                if (_overchargeFloat > (WeaponProperties.OVERCHARGE_TIME_FULL))
                {
                    print("SHOOT OVERCHARGED SHOT");
                    pInventory.activeWeapon.allowSinglePlasmaBoltForNetworkedOverheat = false;
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



    void OnPlayerControllerScopeUp_Delegate(PlayerController playerController)
    {
        scopeBtnDown = false;
    }
    void OnPlayerControllerScope_Delegate(PlayerController playerController)
    {
        if (playerController.isDrawingWeapon)
            return;

        Shoot(pInventory.activeWeapon.leftWeapon);
    }

    public void Shoot(WeaponProperties wp = null)
    {
        if (playerController.isDrawingWeapon) return;


        WeaponProperties sw = pInventory.activeWeapon.GetComponent<WeaponProperties>();
        if (wp)
            sw = wp;


        if ((_fireRecovery > 0 && !wp))
            return;
        if ((leftFireInterval > 0 && wp))
            return;

        bool isLeftWeapon = false;

        if (wp)
        {
            sw = wp;
            isLeftWeapon = true;
        }

        if (!isLeftWeapon)
        {

            if (sw.firingMode == WeaponProperties.FiringMode.Burst)
                _fireRecovery = defaultBurstInterval * 5.5f;
            else
                _fireRecovery = 1 / (sw.fireRate / 60f);
        }
        else
        {
            leftFireInterval = 1 / (sw.fireRate / 60f);
        }

        if (CanShootAuto(sw) || CanShootSingleOrBurst(sw))
        {
            if (!isLeftWeapon)
                fireButtonDown = true;
            else
                scopeBtnDown = true;


            if (sw.firingMode == WeaponProperties.FiringMode.Burst)
                ShootBurst(sw);
            else if (!sw.overcharge)
                Shoot_Caller(isLeftWeapon);
            else if (sw.overcharge)
            {
                //print("shooting overcharg");
                //Shoot_Caller(isLeftWeapon);
            }
        }
    }





    public void ShootOverchargeWeapon(WeaponProperties wp, bool overcharge = false)
    {
        if (playerController.isDrawingWeapon) return;


        if ((_fireRecovery > 0 && !wp))
            return;


        _fireRecovery = 1 / (wp.fireRate / 60f);

        if (CanShootAuto(wp) || CanShootSingleOrBurst(wp))
        {
            fireButtonDown = true;

            if (wp.overcharge)
            {
                print("shooting overcharg");
                Shoot_Caller(false, overcharge);
            }
        }
    }




    bool CanShootAuto(WeaponProperties activeWeapon)
    {
        return (activeWeapon.firingMode == WeaponProperties.FiringMode.Auto);
    }

    bool CanShootSingleOrBurst(WeaponProperties activeWeapon)
    {
        return ((activeWeapon.firingMode == WeaponProperties.FiringMode.Single || activeWeapon.firingMode == WeaponProperties.FiringMode.Burst) && ((!fireButtonDown && !pInventory.isDualWielding) || (!scopeBtnDown && pInventory.isDualWielding)));
    }

    void ShootBurst(WeaponProperties activeWeapon)
    {
        for (int i = 0; i < 3; i++)
        {
            if (activeWeapon.loadedAmmo > 0)
                StartCoroutine(ShootBurst_Coroutine(defaultBurstInterval * i));
        }
    }

    IEnumerator ShootBurst_Coroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        Shoot_RPC();
        //return;
        //PV.RPC("Shoot_RPC", RpcTarget.All);
    }

    void Shoot_Caller(bool isLeftWeapon = false, bool overcharge = false)
    {

        WeaponProperties activeWeapon = pInventory.activeWeapon.GetComponent<WeaponProperties>();
        Debug.Log($"{playerController.player.name} Shoot_Caller {activeWeapon.name} {activeWeapon.loadedAmmo}");

        if (isLeftWeapon)
            activeWeapon = pInventory.activeWeapon.leftWeapon;

        if (activeWeapon.loadedAmmo <= 0 || playerController.isReloading)
        {
            Debug.Log($"{playerController.player.name} Shoot_Caller {activeWeapon.name} {activeWeapon.loadedAmmo} isReloading: {playerController.isReloading}");
            return;
        }

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

    void shaaaaa()
    {
        Debug.Log("shaaaaa");
    }

    int _ignoreShootCounter;
    List<RaycastHit> fakeBulletTrailRaycasthits = new List<RaycastHit>();

    void shoooo(bool isLeftWeapon = false, bool overcharge = false)
    {
        if (playerController.GetComponent<Player>().isDead || playerController.GetComponent<Player>().isRespawning)
            return;

        playerController.SetCurrentlyShootingReset();

        playerController.player.assignActorPlayerTargetOnShootingSphere.TriggerBehaviour();

        Debug.Log("shoooo 1");

        int counter = 1;
        WeaponProperties activeWeapon = pInventory.activeWeapon.GetComponent<WeaponProperties>();

        if (isLeftWeapon)
            activeWeapon = pInventory.activeWeapon.leftWeapon;

        if (activeWeapon.isShotgun)
        {
            counter = activeWeapon.numberOfPellets;
            if (activeWeapon.isShotgun)
                for (int j = 0; j < activeWeapon.numberOfPellets; j++)
                    quats.Add(Quaternion.Euler(Vector3.zero));
        }

        for (int i = 0; i < counter; i++)
            if (activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Bullet || activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Plasma)
            {
                Player player = playerController.GetComponent<Player>();
                Quaternion ranSprayQuat = activeWeapon.GetRandomSprayRotation();

                if (playerController.isAiming && activeWeapon.hipSprayOnly)
                    ranSprayQuat = Quaternion.identity;

                if (activeWeapon.ammoProjectileType != WeaponProperties.AmmoProjectileType.Plasma)
                {
                    if (!player.isMine || GameManager.instance.connection == GameManager.Connection.Local)
                    {
                        print("spawning FAKE bullet");




                        fakeBulletTrailRaycasthits = Physics.RaycastAll(player.mainCamera.transform.position, player.mainCamera.transform.forward, playerController.pInventory.activeWeapon.range, _fakeBulletTrailCollisionLayerMask).ToList();

                        if (fakeBulletTrailRaycasthits.Count <= 0)
                        {
                            //print("spawning FAKE bullet easy");

                            pInventory.SpawnFakeBulletTrail((int)playerController.pInventory.activeWeapon.range, ranSprayQuat);
                        }
                        else
                        {
                            //print("spawning FAKE bullet comp");
                            for (int j = fakeBulletTrailRaycasthits.Count; j-- > 0;)
                            {
                                //do something
                                if (fakeBulletTrailRaycasthits[j].collider.transform.root == player.transform)
                                    fakeBulletTrailRaycasthits.Remove(fakeBulletTrailRaycasthits[j]);
                            }

                            fakeBulletTrailRaycasthits = fakeBulletTrailRaycasthits.OrderBy((d) => (d.collider.transform.position - transform.position).sqrMagnitude).ToList();


                            print($"spawning FAKE bullet {fakeBulletTrailRaycasthits[0].collider.name}");

                            int d = (int)Vector3.Distance(player.mainCamera.transform.position, fakeBulletTrailRaycasthits[0].point);
                            pInventory.SpawnFakeBulletTrail(d, ranSprayQuat);
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



                if (player.isMine || activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Plasma)
                {
                    Debug.Log("Shooting Plasma bullet");
                    var bullet = GameObjectPool.instance.SpawnPooledBullet();
                    bullet.GetComponent<Bullet>().overcharged = false;
                    try { bullet.gameObject.GetComponent<Bullet>().weaponProperties = activeWeapon; } catch { }


                    if (activeWeapon.targetTracking)
                    {
                        if (!activeWeapon.overcharge)
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
                    print($"Active weapon has target tracking: {activeWeapon.targetTracking}. PlayerShooting script has tracking target {trackingTarget}. {overcharge}");




                    {
                        Debug.Log(bullet);
                        Debug.Log(activeWeapon);
                        bullet.GetComponent<Bullet>().bluePlasma.SetActive(activeWeapon.plasmaColor == WeaponProperties.PlasmaColor.Blue && activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Plasma);
                        bullet.GetComponent<Bullet>().redPlasma.SetActive(activeWeapon.plasmaColor == WeaponProperties.PlasmaColor.Red && activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Plasma);
                        bullet.GetComponent<Bullet>().greenPlasma.SetActive(activeWeapon.plasmaColor == WeaponProperties.PlasmaColor.Green && activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Plasma);
                        bullet.GetComponent<Bullet>().shard.SetActive(activeWeapon.plasmaColor == WeaponProperties.PlasmaColor.Shard && activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Plasma);
                    }

                    try
                    {
                        if (!playerController.GetComponent<Player>().aimAssist.redReticuleIsOn && !playerController.GetComponent<Player>().aimAssist.invisibleAimAssistOn)
                            playerController.GetComponent<GeneralWeapProperties>().ResetLocalTransform();


                        if (activeWeapon.isShotgun)
                        {
                            quats[i] = UnityEngine.Random.rotation;
                            playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.localRotation = Quaternion.RotateTowards(playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.localRotation, quats[i], activeWeapon.bulletSpray);
                        }
                        else
                            playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.localRotation *= ranSprayQuat;
                    }
                    catch { }

                    //if (PV.IsMine)
                    //    bullet.layer = 8;
                    //else
                    //    bullet.layer = 0;
                    try
                    {
                        bullet.transform.position = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.position;
                        bullet.transform.rotation = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.rotation;
                    }
                    catch
                    {
                        bullet.transform.position = GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.position;
                        bullet.transform.rotation = GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.rotation;
                    }



                    try { bullet.gameObject.GetComponent<Bullet>().sourcePlayer = playerController.GetComponent<Player>(); } catch { }
                    try { bullet.gameObject.GetComponent<Bullet>().weaponProperties = activeWeapon; } catch { }
                    try { bullet.gameObject.GetComponent<Bullet>().damage = playerController.GetComponent<Player>().playerInventory.activeWeapon.damage; } catch { }


                    if (overcharge) bullet.GetComponent<Bullet>().damage *= 5;



                    //try { bullet.gameObject.GetComponent<Bullet>().allPlayerScripts = playerController.GetComponent<AllPlayerScripts>(); } catch { }
                    bullet.gameObject.GetComponent<Bullet>().range = (int)activeWeapon.range;
                    bullet.gameObject.GetComponent<Bullet>().speed = (int)activeWeapon.bulletSpeed;
                    //bullet.gameObject.GetComponent<Bullet>().playerRewiredID = playerRewiredID;
                    //try { bullet.gameObject.GetComponent<Bullet>().playerWhoShot = playerController.GetComponent<GeneralWeapProperties>().GetComponent<Player>(); } catch { }
                    //bullet.gameObject.GetComponent<Bullet>().pInventory = pInventory;
                    //try { bullet.gameObject.GetComponent<Bullet>().crosshairScript = playerController.GetComponent<Player>().cScript; } catch { }
                    bullet.SetActive(true);

                    if (activeWeapon.plasmaColor != WeaponProperties.PlasmaColor.Shard)
                    {
                        activeWeapon.currentOverheat = Mathf.Clamp(activeWeapon.currentOverheat + activeWeapon.overheatPerShot, 0, 100);

                        if (overcharge) activeWeapon.currentOverheat = 100;
                    }
                }
                activeWeapon.SpawnMuzzleflash();
            }
            else if (activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Rocket || activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Grenade)
            {
                // Projectile does not spawn if ammo left is 0, lag
                if (playerController.player.isMine)
                    if (activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Rocket)
                    {

                        PV.RPC("SpawnFakeExplosiveProjectile_RPC", RpcTarget.AllViaServer, GrenadePool.GetAvailableRocketAtIndex(playerController.player.playerDataCell.photonRoomIndex),
                        playerController.player.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.position, playerController.player.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.rotation.eulerAngles);
                    }
                    else if (activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Grenade)
                    {
                        PV.RPC("SpawnFakeExplosiveProjectile_RPC", RpcTarget.AllViaServer, GrenadePool.GetAvailableGrenadeLauncherProjectileAtIndex(playerController.player.playerDataCell.photonRoomIndex),
                        playerController.player.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.position, playerController.player.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.rotation.eulerAngles);
                    }

                activeWeapon.SpawnMuzzleflash();


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
                activeWeapon.loadedAmmo -= 1;
            }
            else
                activeWeapon.loadedAmmo -= 10;
        }

        try
        {
            activeWeapon.GetComponent<Animator>().Play("Fire", 0, 0f);
            tPersonController.GetComponent<Animator>().SetTrigger("Fire");
            //if (pInventory.isDualWielding)
            //    activeWeapon.leftWeapon.GetComponent<Animator>().Play("Fire", 0, 0f);
            //StartCoroutine(Player3PSFiringAnimation());
        }
        catch (System.Exception e) { Debug.LogError(e); }
        Debug.Log("Calling Recoil()");
        activeWeapon.Recoil();
        //if (pInventory.isDualWielding)
        //    activeWeapon.rightWeapon.Recoil();



        GetComponent<AudioSource>().clip = activeWeapon.Fire;
        GetComponent<AudioSource>().Play();
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
        }
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

        if (leftFireInterval > 0)
            leftFireInterval -= Time.deltaTime;
    }


    public void StopBurstFiring()
    {
        StopAllCoroutines();
    }
}
