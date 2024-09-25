using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralWeapProperties : MonoBehaviour
{
    [Header("Other Scripts")]
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
    public Vector3 defaultBulletSpawnPoint;
    public Transform bulletSpawnPoint;
    public List<Transform> pelletSpawnPoints = new List<Transform>();
    public Transform grenadeSpawnPoint;

    [Header("UI Components")]
    public GameObject singleFireUIGO;
    public GameObject burstFireUIGO;
    public GameObject automaticFireUIGO;

    [Header("Bullets Shot by this player")]
    public List<Bullet> bulletsShotByplayer = new List<Bullet>();

    Quaternion originalBulletLocalRotation;
    void Start()
    {
        originalBulletLocalRotation = bulletSpawnPoint.localRotation;
        defaultBulletSpawnPoint = bulletSpawnPoint.localPosition;
        if(hasFoundComponents == false)
        {
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

    public void ResetLocalTransform()
    {
        bulletSpawnPoint.localRotation = originalBulletLocalRotation;
    }
}
