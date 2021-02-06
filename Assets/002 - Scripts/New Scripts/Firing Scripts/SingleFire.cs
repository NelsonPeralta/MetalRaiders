using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleFire : MonoBehaviour
{
    public AllPlayerScripts allPlayerScripts;
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
    public WeaponProperties wProperties;
    public GeneralWeapProperties gwProperties;
    public ChildManager cManager;
    public AudioSource shootingAudioSource;

    public float nextFireInterval;

    private bool ThisisShooting = false;
    private bool hasButtonDown = false;

    private bool hasFoundComponents = false;

    public void Start()
    {
        if (hasFoundComponents == false)
        {
            cManager = gameObject.GetComponentInParent<ChildManager>();
            StartCoroutine(FindComponents());

            hasFoundComponents = true;

        }

        if (ThisisShooting && wProperties.isSingleFire && !pController.isDualWielding && !pController.isDrawingWeapon)
        {
            if (pController.anim != null)
            {
                wProperties.currentAmmo -= 1;
                pController.anim.Play("Fire", 0, 0f);
                StartCoroutine(Player3PSFiringAnimation());
            }


            //If random muzzle is false
            if (!gwProperties.randomMuzzleflash &&
                gwProperties.enableMuzzleflash == true /*&& !silencer*/)
            {
                if(gwProperties.muzzleParticles)
                    gwProperties.muzzleParticles.Emit(1);
                //Light flash start
                StartCoroutine(gwProperties.MuzzleFlashLight());
            }
            else if (gwProperties.randomMuzzleflash == true)
            {
                Debug.Log("In Random Muzzle Flash");
                //Only emit if random value is 1
                if (gwProperties.randomMuzzleflashValue == 1)
                {
                    if (gwProperties.enableSparks == true)
                    {
                        Debug.Log("Emitted Random Spark");
                        //Emit random amount of spark particles
                        gwProperties.sparkParticles.Emit(Random.Range(gwProperties.minSparkEmission, gwProperties.maxSparkEmission));

                    }
                    if (gwProperties.enableMuzzleflash == true /*&& !silencer*/)
                    {
                        Debug.Log("Coroutine Muzzle Flashlight");
                        gwProperties.muzzleParticles.Emit(1);
                        //Light flash start
                        StartCoroutine(gwProperties.MuzzleFlashLight());


                    }
                }
            }

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Spawns projectile from bullet spawnpoint
            if (!wProperties.usesGrenades && !wProperties.usesRockets)
            {
                var bullet = allPlayerScripts.playerBulletPool.SpawnPooledGameObject();
                bullet.transform.position = gwProperties.bulletSpawnPoint.transform.position;
                bullet.transform.rotation = gwProperties.bulletSpawnPoint.transform.rotation;
                
                bullet.gameObject.GetComponent<Bullet>().allPlayerScripts = this.allPlayerScripts;
                bullet.gameObject.GetComponent<Bullet>().range = wProperties.range;
                //var bullet = (Transform)Instantiate(gwProperties.bulletPrefab, gwProperties.bulletSpawnPoint.transform.position, gwProperties.bulletSpawnPoint.transform.rotation);
                bullet.gameObject.GetComponent<Bullet>().playerRewiredID = playerRewiredID;
                bullet.gameObject.GetComponent<Bullet>().playerWhoShot = gwProperties.gameObject.GetComponent<PlayerProperties>().gameObject;
                bullet.gameObject.GetComponent<Bullet>().pInventory = pInventory;
                bullet.gameObject.GetComponent<Bullet>().raycastScript = pProperties.raycastScript;
                bullet.gameObject.GetComponent<Bullet>().crosshairScript = pProperties.cScript;
                bullet.SetActive(true);
                var mf = Instantiate(gwProperties.muzzleFlashEffect, gwProperties.bulletSpawnPoint.transform.position,
                gwProperties.bulletSpawnPoint.transform.rotation);
                Destroy(mf, 1);
                wProperties.Recoil();

                SetTeamToBulletScript(bullet.transform);

                BulletDetector detectorScript = bullet.GetComponent<BulletDetector>();


                //Spawn casing prefab at spawnpoint
                //Instantiate(gwProperties.bigCasingPrefab, gwProperties.casingSpawnPoint.transform.position, gwProperties.casingSpawnPoint.transform.rotation);
            }
            else if (wProperties.usesGrenades)
            {
                var grenade = Instantiate(gwProperties.grenadeLauncherProjectilePrefab, gwProperties.bulletSpawnPoint.transform.position, gwProperties.bulletSpawnPoint.transform.rotation);
                grenade.GetComponent<Rocket>().damage = wProperties.damage;
                grenade.GetComponent<Rocket>().playerWhoThrewGrenade = pController.gameObject;
            }
            else if (wProperties.usesRockets)
            {
                var rocket = Instantiate(gwProperties.rocketProjectilePrefab, gwProperties.bulletSpawnPoint.transform.position, gwProperties.bulletSpawnPoint.transform.rotation);
                rocket.GetComponent<Rocket>().damage = wProperties.damage;
                rocket.GetComponent<Rocket>().playerWhoThrewGrenade = pController.gameObject;
            }

            wProperties.mainAudioSource.clip = wProperties.Fire;
            wProperties.mainAudioSource.Play();
        }

        /////////////////////////////////////////////////////////////////////////////////////////

    }

    public void Update()
    {
        if (pController != null)
        {
            if (!pController.isDualWielding)
            {
                if (wProperties != null)
                {
                    nextFireInterval = wProperties.timeBetweenSingleBullets;
                }

                if (pController.isShooting && !ThisisShooting && !hasButtonDown)
                {
                    StartCoroutine(SingleFireVoid(false, false));
                    hasButtonDown = true;
                }

                if (pInventory != null)
                {
                    if (pInventory.activeWeapIs == 0)
                    {
                        if (pInventory.weaponsEquiped[0] != null)
                        {
                            wProperties = pInventory.weaponsEquiped[0].gameObject.GetComponent<WeaponProperties>();
                        }
                    }

                    else if (pInventory.activeWeapIs == 1)
                    {
                        wProperties = pInventory.weaponsEquiped[1].gameObject.GetComponent<WeaponProperties>();
                    }
                }

                if (pController.player.GetButtonUp("Shoot"))
                {
                    hasButtonDown = false;
                }
            }
        }
    }





    IEnumerator SingleFireVoid(bool thisIsShootingRight, bool thisIsShootingLeft)
    {
        ThisisShooting = true;
        Start();

        yield return new WaitForSeconds(nextFireInterval);
        ThisisShooting = false;

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
}
