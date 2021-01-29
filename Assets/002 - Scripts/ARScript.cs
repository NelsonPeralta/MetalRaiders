using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARScript : MonoBehaviour
{
    //Animator component attached to weapon
    Animator anim;

    //[Header("Gun Camera")]
    //Main gun camera
    //public Camera gunCamera;

    [Header("Gun Camera Options")]
    [Tooltip("Default value for camera field of view (40 is recommended).")]
    public float defaultFov = 40.0f;

    [Header("UI Weapon Name")]
    [Tooltip("Name of the current weapon, shown in the game UI.")]
    public string weaponName;
    private string storedWeaponName;
    public int storedWeaponNumber;

    public string WeaponType;


    /*[Header("Weapon Sway")]
    //Enables weapon sway
    [Tooltip("Toggle weapon sway.")]
    public bool weaponSway;

    public float swayAmount = 0.02f;
    public float maxSwayAmount = 0.06f;
    public float swaySmoothValue = 4.0f;

    private Vector3 initialSwayPosition;*/

    //Used for fire rate
    private float lastFired;
    [Header("Weapon Settings")]
    //How fast the weapon fires, higher value means faster rate of fire
    [Tooltip("How fast the weapon fires, higher value means faster rate of fire.")]
    public float fireRate;
    //Eanbles auto reloading when out of ammo
    [Tooltip("Enables auto reloading when out of ammo.")]
    public bool autoReload;
    //Delay between shooting last bullet and reloading
    public float autoReloadDelay;
    //Check if reloading
    private bool isReloading;


    //Holstering weapon
    private bool hasBeenHolstered = false;
    //If weapon is holstered
    private bool holstered;
    //Check if running
    private bool isRunning;
    //Check if aiming
    private bool isAiming;
    //Check if walking
    private bool isWalking;
    //Check if inspecting weapon
    private bool isInspecting;

    //How much ammo is currently left
    private int currentAmmo;
    //Totalt amount of ammo
    [Tooltip("How much ammo the weapon should have.")]
    public int ammo;
    //Check if out of ammo
    private bool outOfAmmo;

    [Header("Bullet Settings")]
    //Bullet
    [Tooltip("How much force is applied to the bullet when shooting.")]
    public float bulletForce = 400.0f;
    [Tooltip("How long after reloading that the bullet model becomes visible " +
        "again, only used for out of ammo reload animations.")]
    public float showBulletInMagDelay = 0.6f;
    [Tooltip("The bullet model inside the mag, not used for all weapons.")]
    public SkinnedMeshRenderer bulletInMagRenderer;

    [Header("Grenade Settings")]
    public float grenadeSpawnDelay = 0.35f;

    /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    [Header("Muzzleflash Settings")]
    public bool randomMuzzleflash = false;
    //min should always bee 1
    private int minRandomValue = 1;

    [Range(2, 25)]
    public int maxRandomValue = 5;

    private int randomMuzzleflashValue;

    public bool enableMuzzleflash = true;
    public ParticleSystem muzzleParticles;
    public bool enableSparks = true;
    public ParticleSystem sparkParticles;
    public int minSparkEmission = 1;
    public int maxSparkEmission = 7;

    [Header("Muzzleflash Light Settings")]
    public Light muzzleflashLight;
    public float lightDuration = 0.02f;

    /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    [Header("Audio Source")]
    //Main audio source
    public AudioSource mainAudioSource;
    //Audio source used for shoot sound
    public AudioSource shootAudioSource;

    [Header("UI Components")]
    public Text currentAmmoText;
    public Text totalAmmoText;

    [System.Serializable]
    public class prefabs
    {
        [Header("Prefabs")]
        public Transform bulletPrefab;
        public Transform casingPrefab;
        public Transform grenadePrefab;
    }
    public prefabs Prefabs;

    [System.Serializable]
    public class spawnpoints
    {
        [Header("Spawnpoints")]
        //Casing spawn point array
        public Transform casingSpawnPoint;
        //Bullet prefab spawn from this point
        public Transform bulletSpawnPoint;

        public Transform grenadeSpawnPoint;
    }
    public spawnpoints Spawnpoints;

    [System.Serializable]
    public class soundClips
    {
        public AudioClip shootSound;
        public AudioClip takeOutSound;
        public AudioClip holsterSound;
        public AudioClip reloadSoundOutOfAmmo;
        public AudioClip reloadSoundAmmoLeft;
    }
    public soundClips SoundClips;

    private void Awake()
    {

        //Set the animator component
        anim = GetComponent<Animator>();
        //Set current ammo to total ammo value
        currentAmmo = ammo;

        //muzzleflashLight.enabled = false;
        
    }

    private void Start()
    {

        /* 
         * [Header("Prefabs")]
        public Transform bulletPrefab;
        public Transform casingPrefab;
        public Transform grenadePrefab;

         * [Header("Spawnpoints")]
    //Casing spawn point array
    public Transform casingSpawnPoint;
    //Bullet prefab spawn from this point
    public Transform bulletSpawnPoint;

    public Transform grenadeSpawnPoint;
}
public spawnpoints Spawnpoints;

[System.Serializable]
public class soundClips
{
    public AudioClip shootSound;
    public AudioClip takeOutSound;
    public AudioClip holsterSound;
    public AudioClip reloadSoundOutOfAmmo;
    public AudioClip reloadSoundAmmoLeft;
}
public soundClips SoundClips; */

        //gunCamera = GameObject.FindGameObjectWithTag("Player Camera").GetComponent<Camera>();
        currentAmmoText = GameObject.FindGameObjectWithTag("Current Ammo Text").GetComponent<Text>();
        totalAmmoText = GameObject.FindGameObjectWithTag("Total Ammo Text").GetComponent<Text>();

        bulletInMagRenderer = GameObject.FindGameObjectWithTag("Bullet Renderer").GetComponent<SkinnedMeshRenderer>();
        muzzleParticles = GameObject.FindGameObjectWithTag("Muzzleflash Particles").GetComponent<ParticleSystem>();
        sparkParticles = GameObject.FindGameObjectWithTag("Spark Particles").GetComponent<ParticleSystem>();
        muzzleflashLight = GameObject.FindGameObjectWithTag("Muzzleflash Light").GetComponent<Light>();

        muzzleflashLight.enabled = false;

        mainAudioSource = GetComponent<AudioSource>();
        shootAudioSource = GetComponent<AudioSource>();

        //Prefabs.bulletPrefab = (GameObject)Resources.Load("Bullet_Prefab", typeof(GameObject));

        Spawnpoints.casingSpawnPoint = GameObject.FindGameObjectWithTag("Casing Spawn Point").GetComponent<Transform>();
        Spawnpoints.bulletSpawnPoint = GameObject.FindGameObjectWithTag("Bullet Spawn Point").GetComponent<Transform>();
        Spawnpoints.grenadeSpawnPoint = GameObject.FindGameObjectWithTag("Grenade Spawn Point").GetComponent<Transform>();

        //Save the weapon name
        storedWeaponName = weaponName;
        //Set total ammo text from total ammo int
        totalAmmoText.text = ammo.ToString();

        /*
        //Weapon sway
        initialSwayPosition = transform.localPosition;
        */

        //Set the shoot sound to audio source
        shootAudioSource.clip = SoundClips.shootSound;
    }

    

    private void Update()
    {
        

        //If randomize muzzleflash is true, genereate random int values
        if (randomMuzzleflash == true)
        {
            randomMuzzleflashValue = Random.Range(minRandomValue, maxRandomValue);
        }

        
        //Set current ammo text from ammo int
        currentAmmoText.text = currentAmmo.ToString();

        //Continosuly check which animation 
        //is currently playing
        AnimationCheck();

        //Play knife attack 1 animation when Q key is pressed
        if (Input.GetKeyDown(KeyCode.Q) /* && !isInspecting */)
        {
            anim.Play("Knife Attack 1", 0, 0f);
        }
        //Play knife attack 2 animation when F key is pressed
        if (Input.GetKeyDown(KeyCode.F) /* && !isInspecting */)
        {
            anim.Play("Knife Attack 2", 0, 0f);
        }

        //Throw grenade when pressing G key
        if (Input.GetKeyDown(KeyCode.G) /* && !isInspecting */)
        {
            StartCoroutine(GrenadeSpawnDelay());
            //Play grenade throw animation
            anim.Play("GrenadeThrow", 0, 0.0f);
        }

        //If out of ammo
        if (currentAmmo == 0)
        {
            //Show out of ammo text
            //currentWeaponText.text = "OUT OF AMMO";
            //Toggle bool
            outOfAmmo = true;
            //Auto reload if true
            if (autoReload == true && !isReloading)
            {
                StartCoroutine(AutoReload());
            }
        }
        else
        {
            
            //Toggle bool
            outOfAmmo = false;
            //anim.SetBool ("Out Of Ammo", false);
        }

        //AUtomatic fire
        //Left click hold 
        if (Input.GetMouseButton(0) && !outOfAmmo && !isReloading /*&& !isInspecting && !isRunning*/)
        {
            //Shoot automatic
            if (Time.time - lastFired > 1 / fireRate)
            {
                lastFired = Time.time;

                //Remove 1 bullet from ammo
                currentAmmo -= 1;
                

                if (!isAiming) //if not aiming
                {
                    anim.Play("Fire", 0, 0f);
                    //If random muzzle is false
                    if (!randomMuzzleflash &&
                        enableMuzzleflash == true /*&& !silencer*/)
                    {
                        muzzleParticles.Emit(1);
                        //Light flash start
                        StartCoroutine(MuzzleFlashLight());
                    }
                    else if (randomMuzzleflash == true)
                    {
                        //Only emit if random value is 1
                        if (randomMuzzleflashValue == 1)
                        {
                            if (enableSparks == true)
                            {
                                //Emit random amount of spark particles
                                sparkParticles.Emit(Random.Range(minSparkEmission, maxSparkEmission));
                            }
                            if (enableMuzzleflash == true /*&& !silencer*/)
                            {
                                muzzleParticles.Emit(1);
                                //Light flash start
                                StartCoroutine(MuzzleFlashLight());
                            }
                        }
                    }
                }
                
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //Spawn bullet from bullet spawnpoint
                var bullet = (Transform)Instantiate(
                    Prefabs.bulletPrefab,
                    Spawnpoints.bulletSpawnPoint.transform.position,
                    Spawnpoints.bulletSpawnPoint.transform.rotation);

                //Add velocity to the bullet

                //bullet.gameObject.transform.Translate(Vector3.forward * Time.deltaTime * bulletForce);

                BulletDetector detectorScript = bullet.GetComponent<BulletDetector>();

                detectorScript.bSpeed = bulletForce;

                //Spawn casing prefab at spawnpoint
                Instantiate(Prefabs.casingPrefab,
                    Spawnpoints.casingSpawnPoint.transform.position,
                    Spawnpoints.casingSpawnPoint.transform.rotation);
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            }
        }

        //Inspect weapon when T key is pressed
        if (Input.GetKeyDown(KeyCode.T))
        {
            anim.SetTrigger("Inspect");
        }

        /////////////////////////////////////////////////////////////////////Toggle weapon holster when pressing Z key
        if (Input.GetKeyDown(KeyCode.Z) && !hasBeenHolstered)
        {
            holstered = true;

            mainAudioSource.clip = SoundClips.holsterSound;
            mainAudioSource.Play();

            hasBeenHolstered = true;
        }
        else if (Input.GetKeyDown(KeyCode.Z) && hasBeenHolstered)
        {
            holstered = false;

            mainAudioSource.clip = SoundClips.takeOutSound;
            mainAudioSource.Play();

            hasBeenHolstered = false;
        }

        //Holster anim toggle
        if (holstered == true)
        {
            anim.SetBool("Holster", true);
        }
        else
        {
            anim.SetBool("Holster", false);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //Reload 
        if (Input.GetKeyDown(KeyCode.R) && !isReloading /* && !isInspecting */)
        {
            //Reload
            Reload();
        }

        //Walking when pressing down WASD keys
        if (Input.GetKey(KeyCode.W) /* && !isRunning */ ||
            Input.GetKey(KeyCode.A) /* && !isRunning */ ||
            Input.GetKey(KeyCode.S) /* && !isRunning */ ||
            Input.GetKey(KeyCode.D) /* && !isRunning */)
        {
            anim.SetBool("Walk", true);
        }
        else
        {
            anim.SetBool("Walk", false);
        }

        
    }

    private IEnumerator GrenadeSpawnDelay()
    {

        //Wait for set amount of time before spawning grenade
        yield return new WaitForSeconds(grenadeSpawnDelay);
        //Spawn grenade prefab at spawnpoint
        Instantiate(Prefabs.grenadePrefab,
            Spawnpoints.grenadeSpawnPoint.transform.position,
            Spawnpoints.grenadeSpawnPoint.transform.rotation);
    }

    private IEnumerator AutoReload()
    {
        //Wait set amount of time
        yield return new WaitForSeconds(autoReloadDelay);

        if (outOfAmmo == true)
        {
            //Play diff anim if out of ammo
            anim.Play("Reload Out Of Ammo", 0, 0f);

            mainAudioSource.clip = SoundClips.reloadSoundOutOfAmmo;
            mainAudioSource.Play();

            //If out of ammo, hide the bullet renderer in the mag
            //Do not show if bullet renderer is not assigned in inspector
            if (bulletInMagRenderer != null)
            {
                bulletInMagRenderer.GetComponent
                <SkinnedMeshRenderer>().enabled = false;
                //Start show bullet delay
                StartCoroutine(ShowBulletInMag());
            }
        }
        //Restore ammo when reloading
        currentAmmo = ammo;
        outOfAmmo = false;
    }

    //Reload
    private void Reload()
    {

        if (outOfAmmo == true)
        {
            //Play diff anim if out of ammo
            anim.Play("Reload Out Of Ammo", 0, 0f);

            mainAudioSource.clip = SoundClips.reloadSoundOutOfAmmo;
            mainAudioSource.Play();

            //If out of ammo, hide the bullet renderer in the mag
            //Do not show if bullet renderer is not assigned in inspector
            if (bulletInMagRenderer != null)
            {
                bulletInMagRenderer.GetComponent
                <SkinnedMeshRenderer>().enabled = false;
                //Start show bullet delay
                StartCoroutine(ShowBulletInMag());
            }
        }
        else
        {
            //Play diff anim if ammo left
            anim.Play("Reload Ammo Left", 0, 0f);

            mainAudioSource.clip = SoundClips.reloadSoundAmmoLeft;
            mainAudioSource.Play();

            //If reloading when ammo left, show bullet in mag
            //Do not show if bullet renderer is not assigned in inspector
            if (bulletInMagRenderer != null)
            {
                bulletInMagRenderer.GetComponent
                <SkinnedMeshRenderer>().enabled = true;
            }
        }
        //Restore ammo when reloading
        currentAmmo = ammo;
        outOfAmmo = false;
    }

    //Enable bullet in mag renderer after set amount of time
    private IEnumerator ShowBulletInMag()
    {

        //Wait set amount of time before showing bullet in mag
        yield return new WaitForSeconds(showBulletInMagDelay);
        bulletInMagRenderer.GetComponent<SkinnedMeshRenderer>().enabled = true;
    }

    //Show light when shooting, then disable after set amount of time
    private IEnumerator MuzzleFlashLight()
    {

        muzzleflashLight.enabled = true;
        yield return new WaitForSeconds(lightDuration);
        muzzleflashLight.enabled = false;
    }

    //Check current animation playing
    private void AnimationCheck()
    {

        //Check if reloading
        //Check both animations
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Reload Out Of Ammo") ||
            anim.GetCurrentAnimatorStateInfo(0).IsName("Reload Ammo Left"))
        {
            isReloading = true;
        }
        else
        {
            isReloading = false;
        }
                
    }

    
    
}

