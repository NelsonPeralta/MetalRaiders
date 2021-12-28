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
    public GameObjectPool gameObjectPool;

    // Private variables
    int playerRewiredID;
    float fireInterval = 0;
    bool fireButtonDown = false;
    float defaultBurstInterval = 0.1f;

    void Awake()
    {
        gameObjectPool = GameObjectPool.gameObjectPoolInstance;
    }
    private void Start()
    {
        playerController.OnPlayerFire += OnPlayerControllerFire_Delegate;
        playerController.OnPlayerFireUp += OnPlayerControllerFireUp_Delegate;
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
            fireInterval = defaultBurstInterval * 5;
        else
            fireInterval = 1 / (activeWeapon.fireRate / 60f);

        if (CanShootAuto(activeWeapon) || CanShootSingleOrBurst(activeWeapon))
        {
            fireButtonDown = true;
            BulletSpawnPoint bsp = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.GetComponent<BulletSpawnPoint>();
            if (activeWeapon.firingMode == WeaponProperties.FiringMode.Burst)
                ShootBurst();
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

    void ShootBurst()
    {
        for (int i = 0; i < 3; i++)
        {
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
        WeaponProperties activeWeapon = pInventory.activeWeapon.GetComponent<WeaponProperties>();

        //Spawn bullet from bullet spawnpoint
        playerController.GetComponent<GeneralWeapProperties>().ResetLocalTransform();
        playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.localRotation *= activeWeapon.GetRandomSprayRotation();

        var bullet = gameObjectPool.SpawnPooledBullet();
        if (PV.IsMine)
            bullet.layer = 28;
        else
            bullet.layer = 0;
        bullet.transform.position = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.position;
        bullet.transform.rotation = playerController.GetComponent<GeneralWeapProperties>().bulletSpawnPoint.transform.rotation;

        bullet.gameObject.GetComponent<Bullet>().allPlayerScripts = playerController.GetComponent<AllPlayerScripts>();
        bullet.gameObject.GetComponent<Bullet>().range = activeWeapon.range;
        bullet.gameObject.GetComponent<Bullet>().playerRewiredID = playerRewiredID;
        bullet.gameObject.GetComponent<Bullet>().playerWhoShot = playerController.GetComponent<GeneralWeapProperties>().GetComponent<PlayerProperties>();
        bullet.gameObject.GetComponent<Bullet>().pInventory = pInventory;
        bullet.gameObject.GetComponent<Bullet>().raycastScript = playerController.GetComponent<PlayerProperties>().raycastScript;
        bullet.gameObject.GetComponent<Bullet>().crosshairScript = playerController.GetComponent<PlayerProperties>().cScript;
        bullet.SetActive(true);
        GetComponent<CommonFiringActions>().SpawnMuzzleflash();

        activeWeapon.currentAmmo -= 1;
        if (playerController.anim != null)
        {
            playerController.anim.Play("Fire", 0, 0f);
            StartCoroutine(Player3PSFiringAnimation());
        }
        activeWeapon.mainAudioSource.clip = activeWeapon.Fire;
        activeWeapon.mainAudioSource.Play();
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
        tPersonController.anim.Play("Fire");
        yield return new WaitForEndOfFrame();
    }
    void FireCooldown()
    {
        if (fireInterval <= 0)
            return;
        fireInterval -= Time.deltaTime;
    }
}
