using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralWeapProperties : MonoBehaviour
{
    [Header("Other Scripts")]
    public ChildManager cManager;
    public PlayerInventory pInventory;
    public PlayerController pController;

    public bool hasFoundComponents = false;

    public GameObject currentActiveWeapon;
    public float showBulletInMagDelay = 0.6f;
    public SkinnedMeshRenderer bulletInMagRenderer;
    
    
    public bool randomMuzzleflash = true;
    public int minRandomValue = 1;    
    public int maxRandomValue = 5;
    public int randomMuzzleflashValue;
    public bool enableMuzzleflash = true;
    public ParticleSystem muzzleParticles;
    public GameObject muzzleFlashEffect;

    public bool enableSparks = true;
    public ParticleSystem sparkParticles;
    public int minSparkEmission = 1;
    public int maxSparkEmission = 7;
    
    public Light muzzleflashLight;
    public float lightDuration = 0.02f;    
    
    [Header("Prefabs")]
    public Transform bulletPrefab;
    public Transform bigCasingPrefab;
    public Transform smallCasingPrefab;
    public Transform shotgunShellPrefab;
    public Transform grenadeLauncherProjectilePrefab;
    public Transform rocketProjectilePrefab;


    [Header("Spawnpoints")]
    public Transform casingSpawnPoint;
    public Transform bulletSpawnPoint;
    public Transform grenadeSpawnPoint;

    [Header("UI Components")]
    public GameObject singleFireUIGO;
    public GameObject burstFireUIGO;
    public GameObject automaticFireUIGO;

    [Header("Bullets Shot by this player")]
    public List<Bullet> bulletsShotByplayer = new List<Bullet>();

    void Start()
    {

        if(hasFoundComponents == false)
        {
            cManager = GetComponent<ChildManager>();
            //pInventory = cManager.FindChildWithTagScript("Player Inventory").GetComponent<PlayerInventory>();
            pInventory.gwProperties = this;

            
            //muzzleParticles = cManager.FindChildWithTagScript("Muzzleflash Particles").GetComponent<ParticleSystem>();
            //sparkParticles = cManager.FindChildWithTagScript("Spark Particles").GetComponent<ParticleSystem>();
            //muzzleflashLight = cManager.FindChildWithTagScript("Muzzleflash Light").GetComponent<Light>();

            //muzzleflashLight.enabled = false;


            //Prefabs.bulletPrefab = (GameObject)Resources.Load("Bullet_Prefab", typeof(GameObject));

            //casingSpawnPoint = cManager.FindChildWithTagScript("Casing Spawn Point").GetComponent<Transform>();
            //bulletSpawnPoint = cManager.FindChildWithTagScript("Bullet Spawn Point").GetComponent<Transform>();
            //grenadeSpawnPoint = cManager.FindChildWithTagScript("Grenade Spawn Point").GetComponent<Transform>();

            hasFoundComponents = true;
        }

        //bulletInMagRenderer = GameObject.FindGameObjectWithTag("Bullet Renderer").GetComponent<SkinnedMeshRenderer>(); //The Mesh Renderer is different for every weapon because its the bullet inside the mag


        //grenadePrefab.GetComponent<GrenadeScript>().damage = grenadeDamage;
    }

    private void Update()
    {
        if (pInventory.activeWeapIs == 0)
        {
            currentActiveWeapon = pInventory.weaponsEquiped[0];
        }
        else if (pInventory.activeWeapIs == 1 && pInventory.weaponsEquiped[1] != null)
        {
            currentActiveWeapon = pInventory.weaponsEquiped[1];
        }
    }
    /*
    //Enable bullet in mag renderer after set amount of time
    public IEnumerator ShowBulletInMag()
    {

        //Wait set amount of time before showing bullet in mag
        yield return new WaitForSeconds(showBulletInMagDelay);
        bulletInMagRenderer.GetComponent<SkinnedMeshRenderer>().enabled = true;
    }
    */

    //Show light when shooting, then disable after set amount of time
    public IEnumerator MuzzleFlashLight()
    {
        if(muzzleflashLight)
            muzzleflashLight.enabled = true;
        yield return new WaitForSeconds(lightDuration);
            if(muzzleflashLight)
        muzzleflashLight.enabled = false;
    }

    public void CheckFireMode()
    {
        if (pInventory != null && pInventory.activeWeapon != null)
        {
            if (pInventory.activeWeapon.GetComponent<WeaponProperties>().canSelectFire)
            {
                if (pInventory.activeWeapon.GetComponent<WeaponProperties>().isSingleFire)
                {
                    singleFireUIGO.SetActive(true);
                    burstFireUIGO.SetActive(false);
                    automaticFireUIGO.SetActive(false);

                    if (!pController.isAiming)
                    {
                        pInventory.activeWeapon.GetComponent<WeaponProperties>().RedReticuleRange = 10;
                    }
                    else
                    {
                        pInventory.activeWeapon.GetComponent<WeaponProperties>().RedReticuleRange = pInventory.activeWeapon.GetComponent<WeaponProperties>().aimRRR;
                    }
                    pInventory.activeWeapon.GetComponent<WeaponProperties>().damage = 30;
                    pInventory.activeWeapon.GetComponent<WeaponProperties>().bulletSpeed = 250;
                    pInventory.activeWeapon.GetComponent<WeaponProperties>().isHeadshotCapable = true;
                    pInventory.activeWeapon.GetComponent<WeaponProperties>().isNormalBullet = false;
                }
                else if (pInventory.activeWeapon.GetComponent<WeaponProperties>().isBurstWeapon)
                {
                    singleFireUIGO.SetActive(true);
                    burstFireUIGO.SetActive(true);
                    automaticFireUIGO.SetActive(false);

                    if (!pController.isAiming)
                    {
                        pInventory.activeWeapon.GetComponent<WeaponProperties>().RedReticuleRange = 8;
                    }
                    else
                    {
                        pInventory.activeWeapon.GetComponent<WeaponProperties>().RedReticuleRange = pInventory.activeWeapon.GetComponent<WeaponProperties>().aimRRR;
                    }
                    pInventory.activeWeapon.GetComponent<WeaponProperties>().damage = 15;
                    pInventory.activeWeapon.GetComponent<WeaponProperties>().bulletSpeed = 250;
                    pInventory.activeWeapon.GetComponent<WeaponProperties>().isHeadshotCapable = true;
                    pInventory.activeWeapon.GetComponent<WeaponProperties>().isNormalBullet = false;
                }
                else if (pInventory.activeWeapon.GetComponent<WeaponProperties>().isFullyAutomatic)
                {
                    singleFireUIGO.SetActive(true);
                    burstFireUIGO.SetActive(true);
                    automaticFireUIGO.SetActive(true);

                    if (!pController.isAiming)
                    {
                        pInventory.activeWeapon.GetComponent<WeaponProperties>().RedReticuleRange = 6;
                    }
                    else
                    {
                        pInventory.activeWeapon.GetComponent<WeaponProperties>().RedReticuleRange = pInventory.activeWeapon.GetComponent<WeaponProperties>().aimRRR;
                    }
                    pInventory.activeWeapon.GetComponent<WeaponProperties>().damage = 30;
                    pInventory.activeWeapon.GetComponent<WeaponProperties>().bulletSpeed = 250;
                    pInventory.activeWeapon.GetComponent<WeaponProperties>().isHeadshotCapable = false;
                    pInventory.activeWeapon.GetComponent<WeaponProperties>().isNormalBullet = true; ;
                }
            }
            else
            {
                singleFireUIGO.SetActive(false);
                burstFireUIGO.SetActive(false);
                automaticFireUIGO.SetActive(false);
            }
        }
    }
}
