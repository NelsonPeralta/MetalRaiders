using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullyAutomaticFire : MonoBehaviour
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
    public ThirdPersonScript tPersonController;
    public PlayerInventory pInventory;
    public WeaponProperties wProperties;
    public GeneralWeapProperties gwProperties;
    public ChildManager cManager;

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



        if (ThisisShooting && wProperties.isFullyAutomatic && !pController.isDualWielding && !pController.isDrawingWeapon)
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
                if(gwProperties.muzzleflashLight)
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
            //Spawn bullet from bullet spawnpoint
            var bullet = allPlayerScripts.playerController.objectPool.SpawnPooledBullet();
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

            wProperties.mainAudioSource.clip = wProperties.Fire;
            wProperties.mainAudioSource.Play();

            //tPersonController.anim.SetBool("Fire", false);

        }

    }

    public void Update()
    {
        if (pController != null)
        {
            if (!pController.isDualWielding)
            {
                if (wProperties)
                    nextFireInterval = wProperties.timeBetweenFABullets;

                if (!ThisisShooting)
                {
                    if (pController.isShooting /*|| Script.isShooting*/)
                    {
                        StartCoroutine(Fire(false, false));
                    }
                }


                if (pInventory.activeWeapIs == 0)
                    if (pInventory.weaponsEquiped[0])
                        wProperties = pInventory.weaponsEquiped[0].gameObject.GetComponent<WeaponProperties>();
                    else
                        ;
                else if (pInventory.activeWeapIs == 1)
                    if (pInventory.weaponsEquiped[1])
                        wProperties = pInventory.weaponsEquiped[1].gameObject.GetComponent<WeaponProperties>();

                if (pController.player.GetButtonUp("Shoot"))
                    hasButtonDown = false;
            }
        }
    }





    IEnumerator Fire(bool thisIsShootingRight, bool thisIsShootingLeft)
    {
        ThisisShooting = true;
        Start();

        /*wProperties.mainAudioSource.clip = wProperties.Fire;
        wProperties.mainAudioSource.Play();*/
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
        tPersonController.anim.Play("Fire");
        yield return new WaitForEndOfFrame();
    }
}
