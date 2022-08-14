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
    float defaultBurstInterval = 0.08f;

    private void Start()
    {
        playerController.OnPlayerFire += OnPlayerControllerFire_Delegate;
        playerController.OnPlayerFireButtonUp += OnPlayerControllerFireUp_Delegate;
    }

    void OnPlayerControllerFireUp_Delegate(PlayerController playerController)
    {
        fireButtonDown = false;
    }

    void OnPlayerControllerFire_Delegate(PlayerController playerController)
    {
        if (fireInterval > 0 || playerController.isDrawingWeapon)
            return;
        WeaponProperties activeWeapon = pInventory.activeWeapon.GetComponent<WeaponProperties>();
        if (activeWeapon.firingMode == WeaponProperties.FiringMode.Burst)
            fireInterval = defaultBurstInterval * 5.5f;
        else
            fireInterval = 1 / (activeWeapon.fireRate / 60f);

        if (CanShootAuto(activeWeapon) || CanShootSingleOrBurst(activeWeapon))
        {
            fireButtonDown = true;
            BulletSpawnPoint bsp = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.GetComponent<BulletSpawnPoint>();
            if (activeWeapon.firingMode == WeaponProperties.FiringMode.Burst)
                ShootBurst(activeWeapon);
            else
                PV.RPC("Shoot", RpcTarget.All);
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

        PV.RPC("Shoot", RpcTarget.All);
    }


    [PunRPC]
    public void Shoot()
    {
        int counter = 1;
        WeaponProperties activeWeapon = pInventory.activeWeapon.GetComponent<WeaponProperties>();
        if (activeWeapon.currentAmmo <= 0)
            return;

        if (activeWeapon.isShotgun)
            counter = activeWeapon.numberOfPellets;

        for (int i = 0; i < counter; i++)
            if (activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Bullet)
            {
                if (!playerController.GetComponent<Player>().aimAssist.redReticuleIsOn)
                    playerController.GetComponent<GeneralWeapProperties>().ResetLocalTransform();
                playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.localRotation *= activeWeapon.GetRandomSprayRotation();

                var bullet = FindObjectOfType<GameObjectPool>().SpawnPooledBullet();
                if (PV.IsMine)
                    bullet.layer = 8;
                else
                    bullet.layer = 0;
                bullet.transform.position = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.position;
                bullet.transform.rotation = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.rotation;

                bullet.gameObject.GetComponent<Bullet>().allPlayerScripts = playerController.GetComponent<AllPlayerScripts>();
                bullet.gameObject.GetComponent<Bullet>().range = activeWeapon.range;
                bullet.gameObject.GetComponent<Bullet>().playerRewiredID = playerRewiredID;
                bullet.gameObject.GetComponent<Bullet>().playerWhoShot = playerController.GetComponent<GeneralWeapProperties>().GetComponent<Player>();
                bullet.gameObject.GetComponent<Bullet>().pInventory = pInventory;
                bullet.gameObject.GetComponent<Bullet>().crosshairScript = playerController.GetComponent<Player>().cScript;
                bullet.SetActive(true);
                GetComponent<CommonFiringActions>().SpawnMuzzleflash();
            }
            else if (activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Rocket)
            {
                var rocket = Instantiate(playerController.GetComponent<GeneralWeapProperties>().rocketProjectilePrefab).gameObject;

                if (PV.IsMine)
                    rocket.layer = 8;
                else
                    rocket.layer = 0;
                rocket.transform.position = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.position;
                rocket.transform.rotation = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.rotation;

                rocket.gameObject.GetComponent<Rocket>().player = playerController.GetComponent<GeneralWeapProperties>().GetComponent<Player>();
                rocket.GetComponent<Rocket>().damage = activeWeapon.damage;
                GetComponent<CommonFiringActions>().SpawnMuzzleflash();
            }

        if (PV.IsMine)
            activeWeapon.currentAmmo -= 1;
        if (playerController.weaponAnimator != null)
        {
            playerController.weaponAnimator.Play("Fire", 0, 0f);
            StartCoroutine(Player3PSFiringAnimation());
        }
        GetComponent<AudioSource>().clip = activeWeapon.Fire;
        GetComponent<AudioSource>().Play();
        activeWeapon.Recoil();
        OnBulletSpawned?.Invoke(this);
    }

    public void Update()
    {
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
