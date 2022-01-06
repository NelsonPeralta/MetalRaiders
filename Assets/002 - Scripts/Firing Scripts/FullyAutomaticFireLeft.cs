using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullyAutomaticFireLeft : MonoBehaviour
{
    [Header("Other Scripts")]
    public int playerRewiredID;
    public bool redTeam = false;
    public bool blueTeam = false;
    public bool yellowTeam = false;
    public bool greenTeam = false;
    public PlayerController pController;
    public PlayerInventory pInventory;
    public GeneralWeapProperties gwProperties;
    public WeaponProperties dwLeftWP;

    public float nextFireInterval;
    private bool hasButtonDown = false;

    bool ThisIsShootingLeft;
    bool hasLeftButtonDown;

    private bool hasFoundComponents = false;

    public void Start()
    {
        if (ThisIsShootingLeft)
        {
            pInventory.leftWeapon.GetComponent<WeaponProperties>().currentAmmo -= 1;
            pController.animDWLeft.Play("Fire", 0, 0f);


            //If random muzzle is false
            if (!gwProperties.randomMuzzleflash &&
                gwProperties.enableMuzzleflash == true /*&& !silencer*/)
            {
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
            var bullet = (Transform)Instantiate(gwProperties.bulletPrefab, gwProperties.bulletSpawnPoint.transform.position, gwProperties.bulletSpawnPoint.transform.rotation);
            bullet.gameObject.GetComponent<Bullet>().playerRewiredID = playerRewiredID;
            bullet.gameObject.GetComponent<Bullet>().playerWhoShot = gwProperties.gameObject.GetComponent<PlayerProperties>();
            if(pController.isDualWielding)
                bullet.gameObject.GetComponent<Bullet>().wProperties = dwLeftWP;
            bullet.gameObject.GetComponent<Bullet>().crosshairScript = pController.gameObject.GetComponent<PlayerProperties>().cScript;
            bullet.gameObject.GetComponent<Bullet>().pInventory = pInventory;
            bullet.gameObject.GetComponent<Bullet>().raycastScript = pController.gameObject.GetComponent<PlayerProperties>().raycastScript;
            bullet.gameObject.GetComponent<Bullet>().crosshairScript = pController.gameObject.GetComponent<PlayerProperties>().cScript;

            SetTeamToBulletScript(bullet);

            BulletDetector detectorScript = bullet.GetComponent<BulletDetector>();

            //Spawn casing prefab at spawnpoint
            Instantiate(gwProperties.bigCasingPrefab, gwProperties.casingSpawnPoint.transform.position, gwProperties.casingSpawnPoint.transform.rotation);

            dwLeftWP.mainAudioSource.clip = dwLeftWP.Fire;
            dwLeftWP.mainAudioSource.Play();

        }

    }

    public void Update()
    {

        //if (pController.isDualWielding && pInventory.leftWeapon.GetComponent<WeaponProperties>().isFullyAutomatic)
        //{
        //    nextFireInterval = pInventory.leftWeapon.GetComponent<WeaponProperties>().timeBetweenFABullets;
        //    //Debug.Log(nextFireInterval);
        //    dwLeftWP = pController.dwLeftWP;

        //    if (pController.isShootingLeft && !ThisIsShootingLeft)
        //    {
        //        StartCoroutine(Fire(false, true));
        //        hasLeftButtonDown = true;
        //    }

        //    if (pController.player.GetButtonUp("Throw Grenade"))
        //    {
        //        hasLeftButtonDown = false;
        //    }

        //}
    }





    IEnumerator Fire(bool thisIsShootingRight, bool thisIsShootingLeft)
    {
        if (thisIsShootingLeft)
        {
            ThisIsShootingLeft = true;
        }

        Start();
        /*wProperties.mainAudioSource.clip = wProperties.Fire;
        wProperties.mainAudioSource.Play();*/
        yield return new WaitForSeconds(nextFireInterval);
        ThisIsShootingLeft = false;

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
}
