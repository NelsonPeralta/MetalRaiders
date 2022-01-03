using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FullyAutomaticFireRight : MonoBehaviour
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
    public WeaponProperties dwRightWP;

    public float nextFireInterval;
    private bool hasButtonDown = false;

    bool ThisIsShootingRight;
    bool hasRightButtonDown;

    private bool hasFoundComponents = false;

    //public void Start()
    //{
    //    if (ThisIsShootingRight)
    //    {
    //        pInventory.activeWeapon.GetComponent<WeaponProperties>().currentAmmo -= 1;
    //        pInventory.rightWeapon.GetComponent<WeaponProperties>().currentAmmo -= 1;
    //        pController.animDWRight.Play("Fire", 0, 0f);


    //        //If random muzzle is false
    //        if (!gwProperties.randomMuzzleflash &&
    //            gwProperties.enableMuzzleflash == true /*&& !silencer*/)
    //        {
    //            gwProperties.muzzleParticles.Emit(1);
    //            //Light flash start
    //            StartCoroutine(gwProperties.MuzzleFlashLight());
    //        }
    //        else if (gwProperties.randomMuzzleflash == true)
    //        {
    //            Debug.Log("In Random Muzzle Flash");
    //            //Only emit if random value is 1
    //            if (gwProperties.randomMuzzleflashValue == 1)
    //            {
    //                if (gwProperties.enableSparks == true)
    //                {
    //                    Debug.Log("Emitted Random Spark");
    //                    //Emit random amount of spark particles
    //                    gwProperties.sparkParticles.Emit(Random.Range(gwProperties.minSparkEmission, gwProperties.maxSparkEmission));

    //                }
    //                if (gwProperties.enableMuzzleflash == true /*&& !silencer*/)
    //                {
    //                    Debug.Log("Coroutine Muzzle Flashlight");
    //                    gwProperties.muzzleParticles.Emit(1);
    //                    //Light flash start
    //                    StartCoroutine(gwProperties.MuzzleFlashLight());


    //                }
    //            }
    //        }

    //        if (!dwRightWP.usesGrenades && !dwRightWP.usesRockets)
    //        {
    //            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //            //Spawn bullet from bullet spawnpoint
    //            var bullet = (Transform)Instantiate(gwProperties.bulletPrefab, gwProperties.bulletSpawnPoint.transform.position, gwProperties.bulletSpawnPoint.transform.rotation);
    //            bullet.gameObject.GetComponent<Bullet>().playerRewiredID = playerRewiredID;
    //            bullet.gameObject.GetComponent<Bullet>().playerWhoShot = gwProperties.gameObject.GetComponent<PlayerProperties>();
    //            if (pController.isDualWielding)
    //                bullet.gameObject.GetComponent<Bullet>().wProperties = dwRightWP;
    //            bullet.gameObject.GetComponent<Bullet>().crosshairScript = pController.gameObject.GetComponent<PlayerProperties>().cScript;
    //            bullet.gameObject.GetComponent<Bullet>().pInventory = pInventory;
    //            bullet.gameObject.GetComponent<Bullet>().raycastScript = pController.gameObject.GetComponent<PlayerProperties>().raycastScript;
    //            bullet.gameObject.GetComponent<Bullet>().crosshairScript = pController.gameObject.GetComponent<PlayerProperties>().cScript;

    //            SetTeamToBulletScript(bullet);

    //            BulletDetector detectorScript = bullet.GetComponent<BulletDetector>();

    //            //Spawn casing prefab at spawnpoint
    //            Instantiate(gwProperties.bigCasingPrefab, gwProperties.casingSpawnPoint.transform.position, gwProperties.casingSpawnPoint.transform.rotation);
    //        }

    //        dwRightWP.mainAudioSource.clip = dwRightWP.Fire;
    //        dwRightWP.mainAudioSource.Play();

    //    }

    //}

    //public void Update()
    //{

    //    if (pController.isDualWielding && pInventory.rightWeapon.GetComponent<WeaponProperties>().isFullyAutomatic)
    //    {
    //        nextFireInterval = pInventory.rightWeapon.GetComponent<WeaponProperties>().timeBetweenFABullets;
    //        dwRightWP = pController.dwLeftWP;

    //        if (pController.isShootingRight && !ThisIsShootingRight)
    //        {
    //            StartCoroutine(Fire(true, false));
    //            hasRightButtonDown = true;
    //        }

    //        if (pController.player.GetButtonUp("Shoot"))
    //        {
    //            hasRightButtonDown = false;
    //        }

    //    }
    //}





    //IEnumerator Fire(bool thisIsShootingRight, bool thisIsShootingLeft)
    //{
    //    if (thisIsShootingRight)
    //    {
    //        ThisIsShootingRight = true;
    //    }

    //    Start();
    //    /*wProperties.mainAudioSource.clip = wProperties.Fire;
    //    wProperties.mainAudioSource.Play();*/
    //    yield return new WaitForSeconds(nextFireInterval);
    //    ThisIsShootingRight = false;

    //}

    //public void SetTeamToBulletScript(Transform bullet)
    //{
    //    if (redTeam)
    //    {
    //        bullet.gameObject.GetComponent<Bullet>().redTeam = true;
    //    }
    //    else if (blueTeam)
    //    {
    //        bullet.gameObject.GetComponent<Bullet>().blueTeam = true;
    //    }
    //    else if (yellowTeam)
    //    {
    //        bullet.gameObject.GetComponent<Bullet>().yellowTeam = true;
    //    }
    //    else if (greenTeam)
    //    {
    //        bullet.gameObject.GetComponent<Bullet>().greenTeam = true;
    //    }
    //}
}
