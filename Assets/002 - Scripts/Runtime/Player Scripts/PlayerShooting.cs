using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerShooting : MonoBehaviourPun
{
    public delegate void PlayerShootingEvent(PlayerShooting playerShooting);
    public PlayerShootingEvent OnBulletSpawned;

    [Header("Other Scripts")]
    public PhotonView PV;
    public PlayerController playerController;
    public PlayerInventory pInventory;
    public ThirdPersonScript tPersonController;

    // Private variables
    int playerRewiredID;
    float fireInterval = 0;
    bool fireButtonDown = false;
    public float defaultBurstInterval
    {
        get { return 0.08f; }
    }

    private void Start()
    {
        _ignoreShootCounter = 2;
        playerController.OnPlayerFire += OnPlayerControllerFire_Delegate;
        playerController.OnPlayerFireButtonUp += OnPlayerControllerFireUp_Delegate;
    }

    void OnPlayerControllerFireUp_Delegate(PlayerController playerController)
    {
        fireButtonDown = false;
    }

    void OnPlayerControllerFire_Delegate(PlayerController playerController)
    {
        if (playerController.isDrawingWeapon)
            return;

        Shoot();
    }

    public void Shoot()
    {
        if (fireInterval > 0)
            return;
        WeaponProperties activeWeapon = pInventory.activeWeapon.GetComponent<WeaponProperties>();
        if (activeWeapon.firingMode == WeaponProperties.FiringMode.Burst)
            fireInterval = defaultBurstInterval * 5.5f;
        else
            fireInterval = 1 / (activeWeapon.fireRate / 60f);

        if (CanShootAuto(activeWeapon) || CanShootSingleOrBurst(activeWeapon))
        {
            fireButtonDown = true;
            if (activeWeapon.firingMode == WeaponProperties.FiringMode.Burst)
                ShootBurst(activeWeapon);
            else
                Shoot_Caller();
        }
    }

    bool CanShootAuto(WeaponProperties activeWeapon)
    {
        return (activeWeapon.firingMode == WeaponProperties.FiringMode.Auto);
    }

    bool CanShootSingleOrBurst(WeaponProperties activeWeapon)
    {
        return ((activeWeapon.firingMode == WeaponProperties.FiringMode.Single || activeWeapon.firingMode == WeaponProperties.FiringMode.Burst) && !fireButtonDown);
    }

    void ShootBurst(WeaponProperties activeWeapon)
    {
        for (int i = 0; i < 3; i++)
        {
            if (activeWeapon.currentAmmo > 0)
                StartCoroutine(ShootBurst_Coroutine(defaultBurstInterval * i));
        }
    }

    IEnumerator ShootBurst_Coroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        PV.RPC("Shoot_RPC", RpcTarget.All);
    }

    void Shoot_Caller()
    {
        WeaponProperties activeWeapon = pInventory.activeWeapon.GetComponent<WeaponProperties>();
        if (playerController.GetComponent<PhotonView>().IsMine)
            if (activeWeapon.currentAmmo <= 0)
                return;
        PV.RPC("Shoot_RPC", RpcTarget.All);
    }

    [PunRPC]
    void Shoot_RPC()
    {
        shoooo();
        {
            //int counter = 1;
            //WeaponProperties activeWeapon = pInventory.activeWeapon.GetComponent<WeaponProperties>();

            //if (activeWeapon.isShotgun)
            //    counter = activeWeapon.numberOfPellets;

            //for (int i = 0; i < counter; i++)
            //    if (activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Bullet)
            //    {
            //        try
            //        {
            //            if (!playerController.GetComponent<Player>().aimAssist.redReticuleIsOn)
            //                playerController.GetComponent<GeneralWeapProperties>().ResetLocalTransform();
            //            playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.localRotation *= activeWeapon.GetRandomSprayRotation();
            //        }
            //        catch { }

            //        var bullet = FindObjectOfType<GameObjectPool>().SpawnPooledBullet();
            //        if (PV.IsMine)
            //            bullet.layer = 8;
            //        else
            //            bullet.layer = 0;
            //        try
            //        {
            //            bullet.transform.position = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.position;
            //            bullet.transform.rotation = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.rotation;
            //        }
            //        catch
            //        {
            //            bullet.transform.position = GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.position;
            //            bullet.transform.rotation = GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.rotation;
            //        }



            //        //try { bullet.gameObject.GetComponent<Bullet>().player = playerController.GetComponent<Player>(); } catch { }
            //        //try { bullet.gameObject.GetComponent<Bullet>().allPlayerScripts = playerController.GetComponent<AllPlayerScripts>(); } catch { }
            //        //bullet.gameObject.GetComponent<Bullet>().range = activeWeapon.range;
            //        //bullet.gameObject.GetComponent<Bullet>().playerRewiredID = playerRewiredID;
            //        //try { bullet.gameObject.GetComponent<Bullet>().playerWhoShot = playerController.GetComponent<GeneralWeapProperties>().GetComponent<Player>(); } catch { }
            //        //bullet.gameObject.GetComponent<Bullet>().pInventory = pInventory;
            //        //try { bullet.gameObject.GetComponent<Bullet>().crosshairScript = playerController.GetComponent<Player>().cScript; } catch { }
            //        bullet.SetActive(true);
            //        GetComponent<CommonFiringActions>().SpawnMuzzleflash();
            //    }
            //    else if (activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Rocket || activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Grenade)
            //    {
            //        ExplosiveProjectile rocket = null;

            //        if (activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Rocket)
            //        {
            //            //rocket = Instantiate(playerController.GetComponent<GeneralWeapProperties>().rocketProjectilePrefab).GetComponent<Rocket>();
            //            rocket = Instantiate(playerController.GetComponent<GeneralWeapProperties>().rocketProjectilePrefab).GetComponent<ExplosiveProjectile>();
            //        }
            //        else if (activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Grenade)
            //            rocket = Instantiate(playerController.GetComponent<GeneralWeapProperties>().grenadeLauncherProjectilePrefab).GetComponent<ExplosiveProjectile>();

            //        if (PV.IsMine)
            //            rocket.gameObject.layer = 8;
            //        else
            //            rocket.gameObject.layer = 0;

            //        rocket.transform.position = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.position;
            //        rocket.transform.rotation = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.rotation;

            //        rocket.gameObject.GetComponent<ExplosiveProjectile>().player = playerController.GetComponent<GeneralWeapProperties>().GetComponent<Player>();
            //        GetComponent<CommonFiringActions>().SpawnMuzzleflash();
            //    }

            //if (PV.IsMine)
            //    activeWeapon.currentAmmo -= 1;

            //try
            //{
            //    playerController.weaponAnimator.Play("Fire", 0, 0f);
            //    StartCoroutine(Player3PSFiringAnimation());
            //    activeWeapon.Recoil();
            //}
            //catch { }
            //GetComponent<AudioSource>().clip = activeWeapon.Fire;
            //GetComponent<AudioSource>().Play();
            //OnBulletSpawned?.Invoke(this);
        }
    }

    void shaaaaa()
    {
        Debug.Log("shaaaaa");
    }

    int _ignoreShootCounter;
    void shoooo()
    {
        int counter = 1;
        WeaponProperties activeWeapon = pInventory.activeWeapon.GetComponent<WeaponProperties>();

        if (activeWeapon.isShotgun)
            counter = activeWeapon.numberOfPellets;

        for (int i = 0; i < counter; i++)
            if (activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Bullet)
            {
                if (_ignoreShootCounter % 2 == 0)
                {

                    try
                    {
                        if (!playerController.GetComponent<Player>().aimAssist.redReticuleIsOn)
                            playerController.GetComponent<GeneralWeapProperties>().ResetLocalTransform();
                        playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.localRotation *= activeWeapon.GetRandomSprayRotation();
                    }
                    catch { }

                    var bullet = FindObjectOfType<GameObjectPool>().SpawnPooledBullet();
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



                    try { bullet.gameObject.GetComponent<Bullet>().player = playerController.GetComponent<Player>(); } catch { }
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
                ExplosiveProjectile rocket = null;

                if (activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Rocket)
                {
                    //rocket = Instantiate(playerController.GetComponent<GeneralWeapProperties>().rocketProjectilePrefab).GetComponent<Rocket>();
                    rocket = Instantiate(playerController.GetComponent<GeneralWeapProperties>().rocketProjectilePrefab).GetComponent<ExplosiveProjectile>();
                }
                else if (activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Grenade)
                    rocket = Instantiate(playerController.GetComponent<GeneralWeapProperties>().grenadeLauncherProjectilePrefab).GetComponent<ExplosiveProjectile>();

                if (PV.IsMine)
                    rocket.gameObject.layer = 8;
                else
                    rocket.gameObject.layer = 0;

                rocket.transform.position = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.position;
                rocket.transform.rotation = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.rotation;

                rocket.gameObject.GetComponent<ExplosiveProjectile>().player = playerController.GetComponent<GeneralWeapProperties>().GetComponent<Player>();
                GetComponent<CommonFiringActions>().SpawnMuzzleflash();
            }

        if (!PV.IsMine)
            _ignoreShootCounter++;

        if (PV.IsMine)
            activeWeapon.currentAmmo -= 1;

        try
        {
            playerController.weaponAnimator.Play("Fire", 0, 0f);
            StartCoroutine(Player3PSFiringAnimation());
            activeWeapon.Recoil();
        }
        catch { }
        GetComponent<AudioSource>().clip = activeWeapon.Fire;
        GetComponent<AudioSource>().Play();
        OnBulletSpawned?.Invoke(this);
    }

    public void Update()
    {
        if (playerController)
            if (!PV.IsMine)
                return;

        FireCooldown();
    }
    IEnumerator Player3PSFiringAnimation()
    {
        //tPersonController.GetComponent<Animator>().Play("Fire");
        yield return new WaitForEndOfFrame();
    }
    void FireCooldown()
    {
        if (fireInterval <= 0)
            return;
        fireInterval -= Time.deltaTime;
    }
}
