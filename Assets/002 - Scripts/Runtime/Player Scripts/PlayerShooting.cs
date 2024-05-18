using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerShooting : MonoBehaviourPun
{
    public delegate void PlayerShootingEvent(PlayerShooting playerShooting);
    public PlayerShootingEvent OnBulletSpawned;

    public Biped trackingTarget;

    [Header("Other Scripts")]
    public PhotonView PV;
    public PlayerController playerController;
    public PlayerInventory pInventory;
    public ThirdPersonScript tPersonController;

    // Private variables
    int playerRewiredID;
    [SerializeField] float fireInterval = 0, leftFireInterval = 0;
    [SerializeField] bool fireButtonDown = false, scopeBtnDown = false;
    [SerializeField] LayerMask _fakeBulletTrailCollisionLayerMask;

    GameObjectPool _gameObjectPool;

    public float defaultBurstInterval
    {
        get { return 0.08f; }
    }


    List<Quaternion> quats = new List<Quaternion>();

    private void Awake()
    {
        _gameObjectPool = FindObjectOfType<GameObjectPool>();
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
    }

    void OnPlayerControllerFire_Delegate(PlayerController playerController)
    {
        if (!_gameObjectPool) _gameObjectPool = FindObjectOfType<GameObjectPool>();

        if (playerController.isDrawingWeapon)
            return;

        Shoot();
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
        WeaponProperties sw = pInventory.activeWeapon.GetComponent<WeaponProperties>();
        if (wp)
            sw = wp;

        if ((fireInterval > 0 && !wp))
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
                fireInterval = defaultBurstInterval * 5.5f;
            else
                fireInterval = 1 / (sw.fireRate / 60f);
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
            else
                Shoot_Caller(isLeftWeapon);
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

    void Shoot_Caller(bool isLeftWeapon = false)
    {

        Debug.Log("Shoot_Caller");
        WeaponProperties activeWeapon = pInventory.activeWeapon.GetComponent<WeaponProperties>();

        if (isLeftWeapon)
            activeWeapon = pInventory.activeWeapon.leftWeapon;

        if (activeWeapon.loadedAmmo <= 0 || playerController.isReloading)
            return;

        shoooo(isLeftWeapon);
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
    void shoooo(bool isLeftWeapon = false)
    {
        if (playerController.GetComponent<Player>().isDead || playerController.GetComponent<Player>().isRespawning)
            return;

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
                        RaycastHit hit;
                        if (Physics.Raycast(player.mainCamera.transform.position, player.mainCamera.transform.forward, out hit, playerController.pInventory.activeWeapon.range, _fakeBulletTrailCollisionLayerMask))
                        {
                            int d = (int)Vector3.Distance(player.mainCamera.transform.position, hit.point);
                            StartCoroutine(pInventory.SpawnFakeBulletTrail(d, ranSprayQuat));
                        }
                        else

                            StartCoroutine(pInventory.SpawnFakeBulletTrail((int)playerController.pInventory.activeWeapon.range, ranSprayQuat));
                    }
                }
                else
                {

                }

                if (player.isMine || activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Plasma)
                {
                    Debug.Log("shoooo 2");
                    if (!_gameObjectPool) _gameObjectPool = FindObjectOfType<GameObjectPool>();
                    var bullet = _gameObjectPool.SpawnPooledBullet();

                    if (activeWeapon.targetTracking) bullet.GetComponent<Bullet>().trackingTarget = trackingTarget;

                    {
                        Debug.Log(_gameObjectPool);
                        Debug.Log(bullet);
                        Debug.Log(activeWeapon);
                        bullet.GetComponent<Bullet>().bluePlasma.SetActive(activeWeapon.plasmaColor == WeaponProperties.PlasmaColor.Blue && activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Plasma);
                        bullet.GetComponent<Bullet>().redPlasma.SetActive(activeWeapon.plasmaColor == WeaponProperties.PlasmaColor.Red && activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Plasma);
                        bullet.GetComponent<Bullet>().greenPlasma.SetActive(activeWeapon.plasmaColor == WeaponProperties.PlasmaColor.Green && activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Plasma);
                    }

                    try
                    {
                        if (!playerController.GetComponent<Player>().aimAssist.redReticuleIsOn && !playerController.GetComponent<Player>().aimAssist.invisibleAimAssistOn)
                            playerController.GetComponent<GeneralWeapProperties>().ResetLocalTransform();


                        if (activeWeapon.isShotgun)
                        {
                            quats[i] = Random.rotation;
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
                    //try { bullet.gameObject.GetComponent<Bullet>().allPlayerScripts = playerController.GetComponent<AllPlayerScripts>(); } catch { }
                    bullet.gameObject.GetComponent<Bullet>().range = (int)activeWeapon.range;
                    bullet.gameObject.GetComponent<Bullet>().speed = (int)activeWeapon.bulletSpeed;
                    //bullet.gameObject.GetComponent<Bullet>().playerRewiredID = playerRewiredID;
                    //try { bullet.gameObject.GetComponent<Bullet>().playerWhoShot = playerController.GetComponent<GeneralWeapProperties>().GetComponent<Player>(); } catch { }
                    //bullet.gameObject.GetComponent<Bullet>().pInventory = pInventory;
                    //try { bullet.gameObject.GetComponent<Bullet>().crosshairScript = playerController.GetComponent<Player>().cScript; } catch { }
                    bullet.SetActive(true);
                }
                GetComponent<CommonFiringActions>().SpawnMuzzleflash();
            }
            else if (activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Rocket || activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Grenade)
            {
                // Projectile does not spawn if ammo left is 0, lag
                if (playerController.player.isMine)
                    PV.RPC("SpawnFakeExplosiveProjectile_RPC", RpcTarget.All);

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
            activeWeapon.loadedAmmo -= 1;

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
    void SpawnFakeExplosiveProjectile_RPC()
    {
        WeaponProperties activeWeapon = pInventory.activeWeapon.GetComponent<WeaponProperties>();
        ExplosiveProjectile rocket = null;

        if (activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Rocket)
        {
            //rocket = Instantiate(playerController.GetComponent<GeneralWeapProperties>().rocketProjectilePrefab).GetComponent<Rocket>();
            rocket = Instantiate(playerController.GetComponent<GeneralWeapProperties>().rocketProjectilePrefab).GetComponent<ExplosiveProjectile>();
        }
        else if (activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Grenade)
            rocket = Instantiate(playerController.GetComponent<GeneralWeapProperties>().grenadeLauncherProjectilePrefab).GetComponent<ExplosiveProjectile>();

        Debug.Log($"{playerController.name} PlayerShooting: {rocket.name}");
        rocket.player = playerController.player;


        if (PV.IsMine)
            rocket.gameObject.layer = 8;
        else
            rocket.gameObject.layer = 0;

        rocket.transform.position = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.position;
        rocket.transform.rotation = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.rotation;

        rocket.gameObject.GetComponent<ExplosiveProjectile>().player = playerController.GetComponent<GeneralWeapProperties>().GetComponent<Player>();
        GetComponent<CommonFiringActions>().SpawnMuzzleflash();
    }

    public void Update()
    {
        if (playerController)
            FireCooldown();
    }
    IEnumerator Player3PSFiringAnimation()
    {
        //tPersonController.GetComponent<Animator>().Play("Fire");
        yield return new WaitForEndOfFrame();
    }
    void FireCooldown()
    {
        if (fireInterval > 0)
            fireInterval -= Time.deltaTime;

        if (leftFireInterval > 0)
            leftFireInterval -= Time.deltaTime;
    }
}
