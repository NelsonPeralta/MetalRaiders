using System.Collections;
using UnityEngine;
using Photon.Pun;

public class SingleFire : MonoBehaviour
{
    public AllPlayerScripts allPlayerScripts;
    public CommonFiringActions commonFiringActions;
    public PhotonView PV;
    public GameObjectPool gameObjectPool;

    [Header("Other Scripts")]
    public int playerRewiredID;
    public bool redTeam = false;
    public bool blueTeam = false;
    public bool yellowTeam = false;
    public bool greenTeam = false;
    public PlayerProperties pProperties;
    public PlayerController pController;
    public ThirdPersonScript thirdPersonScript;
    public PlayerInventory pInventory;
    public GeneralWeapProperties gwProperties;
    public ChildManager cManager;
    public AudioSource shootingAudioSource;

    public float nextFireInterval;
    float fireInterval = 0;

    float defaultBurstInterval = 0.1f;

    private bool ThisisShooting = false;
    private bool hasButtonDown = false;

    private bool hasFoundComponents = false;

    void Awake()
    {
        gameObjectPool = GameObjectPool.gameObjectPoolInstance;
    }


    [PunRPC]
    public void ShootSingle(bool thisIsShootingRight, bool thisIsShootingLeft)
    {

        WeaponProperties activeWeapon = pInventory.activeWeapon.GetComponent<WeaponProperties>();

        if (!pProperties.isDead && !pController.isDualWielding && !pController.isDrawingWeapon)
        {
            for (int i = 0; i < activeWeapon.GetNumberOfBulletsToShoot(); i++)
            {
                if (activeWeapon.currentAmmo <= 0)
                    return;
                Debug.Log(activeWeapon.GetNumberOfBulletsToShoot());
                //Spawns projectile from bullet spawnpoint
                if (activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Bullet)
                {
                    gwProperties.ResetLocalTransform();
                    gwProperties.bulletSpawnPoint.transform.localRotation *= activeWeapon.GetRandomSprayRotation();

                    var bullet = gameObjectPool.SpawnPooledBullet();
                    if (PV.IsMine)
                        bullet.layer = 28;
                    else
                        bullet.layer = 0;
                    bullet.transform.position = gwProperties.bulletSpawnPoint.transform.position;
                    bullet.transform.rotation = gwProperties.bulletSpawnPoint.transform.rotation;

                    bullet.gameObject.GetComponent<Bullet>().allPlayerScripts = this.allPlayerScripts;
                    bullet.gameObject.GetComponent<Bullet>().range = activeWeapon.range;
                    bullet.gameObject.GetComponent<Bullet>().playerRewiredID = playerRewiredID;
                    bullet.gameObject.GetComponent<Bullet>().playerWhoShot = gwProperties.gameObject.GetComponent<PlayerProperties>();
                    bullet.gameObject.GetComponent<Bullet>().pInventory = pInventory;
                    bullet.gameObject.GetComponent<Bullet>().raycastScript = pProperties.raycastScript;
                    bullet.gameObject.GetComponent<Bullet>().crosshairScript = pProperties.cScript;
                    bullet.SetActive(true);
                    commonFiringActions.SpawnMuzzleflash();

                }
                else if (activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Grenade)
                {
                    var grenade = Instantiate(gwProperties.grenadeLauncherProjectilePrefab, gwProperties.bulletSpawnPoint.transform.position, gwProperties.bulletSpawnPoint.transform.rotation);
                    grenade.GetComponent<Rocket>().damage = activeWeapon.damage;
                    grenade.GetComponent<Rocket>().playerWhoThrewGrenade = pController.playerProperties;
                }
                else if (activeWeapon.ammoProjectileType == WeaponProperties.AmmoProjectileType.Rocket)
                {
                    var rocket = Instantiate(gwProperties.rocketProjectilePrefab, gwProperties.bulletSpawnPoint.transform.position, gwProperties.bulletSpawnPoint.transform.rotation);
                    rocket.GetComponent<Rocket>().damage = activeWeapon.damage;
                    rocket.GetComponent<Rocket>().playerWhoThrewGrenade = pController.playerProperties;
                }
            }

            activeWeapon.currentAmmo -= 1;
            pInventory.AmmoManager();
            if (pController.anim != null)
            {
                pController.anim.Play("Fire", 0, 0f);
                StartCoroutine(Player3PSFiringAnimation());
            }

            activeWeapon.Recoil();
            activeWeapon.mainAudioSource.clip = activeWeapon.Fire;
            activeWeapon.mainAudioSource.Play();
        }
    }

    public void Update()
    {
        if (!PV.IsMine)
            return;

        if (pController != null)
        {
            if (!pController.isDualWielding)
            {
                WeaponProperties activeWeapon = pInventory.activeWeapon.GetComponent<WeaponProperties>();
                if (activeWeapon)
                {
                    nextFireInterval = 1 / (activeWeapon.fireRate / 60f);
                    if (activeWeapon.firingMode == WeaponProperties.FiringMode.Burst)
                        nextFireInterval = defaultBurstInterval * 5;
                }

                if (pController.isShooting && !ThisisShooting && !hasButtonDown)
                {

                    if (activeWeapon.firingMode == WeaponProperties.FiringMode.Single)
                    {
                        PV.RPC("ShootSingle", RpcTarget.All, false, false);
                        hasButtonDown = true;
                        StartFiringIntervalCooldown();
                    }
                    else if (activeWeapon.firingMode == WeaponProperties.FiringMode.Burst)
                    {
                        ShootBurst();
                        hasButtonDown = true;
                        StartFiringIntervalCooldown();
                    }
                }

                if (pInventory != null)
                {
                    if (pInventory.activeWeapIs == 0)
                        if (pInventory.weaponsEquiped[0] != null)
                            activeWeapon = pInventory.weaponsEquiped[0].gameObject.GetComponent<WeaponProperties>();

                        else if (pInventory.activeWeapIs == 1)
                            activeWeapon = pInventory.weaponsEquiped[1].gameObject.GetComponent<WeaponProperties>();
                }

                if (pController.player.GetButtonUp("Shoot"))
                    hasButtonDown = false;
            }
        }
        FireIntervalCooldown();
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

        PV.RPC("ShootSingle", RpcTarget.All, false, false);
    }


    [PunRPC]
    public void FireSingle(bool thisIsShootingRight, bool thisIsShootingLeft)
    {
        if (ThisisShooting)
            return;

        PV.RPC("ShootSingle", RpcTarget.All);

        StartFiringIntervalCooldown();
    }

    IEnumerator FindComponents()
    {
        yield return new WaitForEndOfFrame();

        pController = gameObject.GetComponentInParent<PlayerController>();
        //pInventory = cManager.FindChildWithTag("Player Inventory").GetComponent<PlayerInventory>();
        //wProperties = cManager.FindChildWithTag("Weapon").GetComponent<WeaponProperties>();
        gwProperties = gameObject.GetComponentInParent<GeneralWeapProperties>();
    }

    public void SetTeamToBulletScript(Transform bullet)
    {
        if (redTeam)
        {
            bullet.gameObject.GetComponent<Bullet>().redTeam = true;
        }
        else if (blueTeam)
        {
            bullet.gameObject.GetComponent<Bullet>().blueTeam = true;
        }
        else if (yellowTeam)
        {
            bullet.gameObject.GetComponent<Bullet>().yellowTeam = true;
        }
        else if (greenTeam)
        {
            bullet.gameObject.GetComponent<Bullet>().greenTeam = true;
        }
    }

    IEnumerator Player3PSFiringAnimation()
    {
        thirdPersonScript.anim.Play("Fire");
        yield return new WaitForEndOfFrame();
    }

    void StartFiringIntervalCooldown()
    {
        fireInterval = nextFireInterval;
        ThisisShooting = true;
    }

    void FireIntervalCooldown()
    {
        if (!ThisisShooting)
            return;
        fireInterval -= Time.deltaTime;

        if (fireInterval <= 0)
            ThisisShooting = false;
    }
}
