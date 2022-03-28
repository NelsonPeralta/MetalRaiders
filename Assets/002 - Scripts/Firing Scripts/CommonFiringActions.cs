using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonFiringActions : MonoBehaviour
{
    public PlayerController playerController;
    public ThirdPersonScript thirdPersonScript;
    public GameObject muzzleflash;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AfterShootingAction(WeaponProperties weaponProperties)
    {
        weaponProperties.Recoil();
        weaponProperties.currentAmmo -= 1;

        weaponProperties.mainAudioSource.clip = weaponProperties.Fire;
        weaponProperties.mainAudioSource.Play();

        playerController.anim.Play("Fire", 0, 0f);
        StartCoroutine(Player3PSFiringAnimation());
    }

    void SpawnOtherFiringEffects()
    {
        //var mf = Instantiate(gwProperties.muzzleFlashEffect, gwProperties.bulletSpawnPoint.transform.position,
        //    gwProperties.bulletSpawnPoint.transform.rotation);
        //Destroy(mf, 1);

        ////If random muzzle is false
        //if (!gwProperties.randomMuzzleflash &&
        //    gwProperties.enableMuzzleflash == true /*&& !silencer*/)
        //{
        //    if (gwProperties.muzzleflashLight)
        //        gwProperties.muzzleParticles.Emit(1);
        //    //Light flash start
        //    StartCoroutine(gwProperties.MuzzleFlashLight());
        //}
        //else if (gwProperties.randomMuzzleflash == true)
        //{
        //    Debug.Log("In Random Muzzle Flash");
        //    //Only emit if random value is 1
        //    if (gwProperties.randomMuzzleflashValue == 1)
        //    {
        //        if (gwProperties.enableSparks == true)
        //        {
        //            Debug.Log("Emitted Random Spark");
        //            //Emit random amount of spark particles
        //            gwProperties.sparkParticles.Emit(Random.Range(gwProperties.minSparkEmission, gwProperties.maxSparkEmission));

        //        }
        //        if (gwProperties.enableMuzzleflash == true /*&& !silencer*/)
        //        {
        //            Debug.Log("Coroutine Muzzle Flashlight");
        //            gwProperties.muzzleParticles.Emit(1);
        //            //Light flash start
        //            StartCoroutine(gwProperties.MuzzleFlashLight());


        //        }
        //    }
        //}
    }


    IEnumerator Player3PSFiringAnimation()
    {
        thirdPersonScript.anim.Play("Fire");
        yield return new WaitForEndOfFrame();
    }

    public void SpawnMuzzleflash()
    {
        if (playerController.isAiming)
            return;
        StartCoroutine(SpawnMuzzleflash_Coroutine());
    }

    IEnumerator SpawnMuzzleflash_Coroutine()
    {
        muzzleflash.SetActive(false);
        yield return new WaitForEndOfFrame();
        muzzleflash.SetActive(true);
    }
}
