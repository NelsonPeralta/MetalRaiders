using System.Collections;
using UnityEngine;
using Photon.Pun;

public class SingleFire : MonoBehaviour
{
    public AllPlayerScripts allPlayerScripts;
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

        if (activeWeapon.isSingleFire && !pController.isDualWielding && !pController.isDrawingWeapon)
        {
            //Spawns projectile from bullet spawnpoint
            if (!activeWeapon.usesGrenades && !activeWeapon.usesRockets)
            {
                var bullet = gameObjectPool.SpawnPooledBullet();
                bullet.transform.position = gwProperties.bulletSpawnPoint.transform.position;
                bullet.transform.rotation = gwProperties.bulletSpawnPoint.transform.rotation;

                bullet.gameObject.GetComponent<Bullet>().allPlayerScripts = this.allPlayerScripts;
                bullet.gameObject.GetComponent<Bullet>().range = activeWeapon.range;
                bullet.gameObject.GetComponent<Bullet>().playerRewiredID = playerRewiredID;
                bullet.gameObject.GetComponent<Bullet>().playerWhoShot = gwProperties.gameObject.GetComponent<PlayerProperties>().gameObject;
                bullet.gameObject.GetComponent<Bullet>().pInventory = pInventory;
                bullet.gameObject.GetComponent<Bullet>().raycastScript = pProperties.raycastScript;
                bullet.gameObject.GetComponent<Bullet>().crosshairScript = pProperties.cScript;
                bullet.SetActive(true);

            }
            else if (activeWeapon.usesGrenades)
            {
                var grenade = Instantiate(gwProperties.grenadeLauncherProjectilePrefab, gwProperties.bulletSpawnPoint.transform.position, gwProperties.bulletSpawnPoint.transform.rotation);
                grenade.GetComponent<Rocket>().damage = activeWeapon.damage;
                grenade.GetComponent<Rocket>().playerWhoThrewGrenade = pController.gameObject;
            }
            else if (activeWeapon.usesRockets)
            {
                var rocket = Instantiate(gwProperties.rocketProjectilePrefab, gwProperties.bulletSpawnPoint.transform.position, gwProperties.bulletSpawnPoint.transform.rotation);
                rocket.GetComponent<Rocket>().damage = activeWeapon.damage;
                rocket.GetComponent<Rocket>().playerWhoThrewGrenade = pController.gameObject;
            }

            Debug.Log("Fired Single");

            activeWeapon.currentAmmo -= 1;
            if (pController.anim != null)
            {
                pController.anim.Play("Fire", 0, 0f);
                StartCoroutine(Player3PSFiringAnimation());
            }

            activeWeapon.Recoil();
            activeWeapon.mainAudioSource.clip = activeWeapon.Fire;
            activeWeapon.mainAudioSource.Play();
        }

        /////////////////////////////////////////////////////////////////////////////////////////

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
                    nextFireInterval = activeWeapon.timeBetweenSingleBullets;

                if (pController.isShooting && !ThisisShooting && !hasButtonDown)
                {

                    if (activeWeapon.isSingleFire)
                    {
                        PV.RPC("ShootSingle", RpcTarget.All, false, false);
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
