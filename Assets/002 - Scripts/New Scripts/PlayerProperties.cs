using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerProperties : MonoBehaviourPunCallbacks, IDamageable
{
    public AllPlayerScripts allPlayerScripts;

    [Header("Models")]
    public GameObject firstPersonModels;
    public GameObject thirdPersonModels;

    [Header("Player Info")]
    public int maxHealth = 100;
    public int maxShield = 150;
    public float Health = 100;
    public float Shield = 150;
    public float meleeDamage = 150;
    public bool isDead = false;
    public float respawnTime = 5;
    public int playerRewiredID;
    public float respawnCountdown;
    public bool respawnStarted;
    public int playerWhoKilledThisPlayerID; // Revenge Medal

    public bool hasShield = false;
    public bool needsHealthPack = false;
    public bool needsShieldPack = false;
    public bool hasMotionTracker = false;

    [Header("Other Scripts")]
    public PlayerInventory pInventory;
    public WeaponProperties wProperties;
    //public ChildManager cManager;
    public PlayerController pController;
    public MultiplayerManager multiplayerManager;
    public SwarmMode swarmMode;
    public Movement mScript;
    public CrosshairScript cScript;
    public WeaponPickUp wPickup;
    public RaycastScript raycastScript;
    public PlayerSurroundings pSurroundings;

    [Header("Camera Options")]
    [Tooltip("Default value for camera field of view (40 is recommended).")]
    public float defaultFov = 60.0f;
    public float defaultSensitivity = 150f;
    public float activeSensitivity;

    [Header("UI Components Text")]
    public Text currentAmmoText;
    public Text dualWieldedWeaponAmmo;
    public Text totalAmmoText;
    public Text totalAmmoText2;
    public Text fragGrenadeText;
    public Text InformerText;
    public Text playerLivesText;

    [Header("UI Components Game Objects")]
    public GameObject GrenadeInfo;
    public GameObject AmmoInfo2;
    public GameObject shieldGO;
    public GameObject motionTrackerGO;
    public GameObject playerLivesIcon;

    [Header("Sliders")]
    public Slider shieldSlider;
    public Slider healthSlider;
    public int shieldRechargeRate = 75;
    public int healthRegenerationRate = 50;
    public float shieldRechargeDelay = 3f;
    public float healthRegenerationDelay = 4f;
    public bool healthRegenerating;
    public bool shieldRechargeAllowed;
    public bool healthRegenerationAllowed;
    public bool armorHasBeenHit = false;
    public bool healthHasBeenHit = false;

    public float shieldRechargeCountdown;
    public float healthRegenerationCountdown;

    private bool hasFoundComponents = false;

    [Header("Cameras")]
    public Camera mainCamera;
    public Camera gunCamera;
    public Camera deathCamera;

    public Vector3 mainOriginalCameraPosition;

    public GameObject thirdPersonDeathSpawnPoint;
    public GameObject thirsPersonDeathGO;
    public GameObject thirdPersonGO;
    public int thirdPersonGOLayer;

    [Header("Hitboxes")]
    public GameObject[] hitboxes = new GameObject[10];
    public CharacterController characterController;

    [Header("Drop Spawn Points")]
    public GameObject weaponDropSpawnPoint1;
    public GameObject weaponDropSpawnPoint2;
    [Space(15)]
    public GameObject smallAmmoPackDrop;
    public GameObject heavyAmmoDropPack;
    public GameObject powerAmmoPackDrop;
    public GameObject fragGrenadePackDrop;

    [Header("Ragdoll")]
    public RagdollSpawn ragdollScript;

    [Header("Player Voice")]
    public AudioSource playerVoice;
    public AudioClip[] meleeClips;
    public AudioClip[] hurtClips;
    public AudioClip[] deathClips;

    [Header("Shield Sounds")]
    public AudioSource shieldAudioSource;
    public AudioSource shieldAlarmAudioSource;
    public AudioClip shieldStartClip;
    public AudioClip shieldDownClip;
    public AudioClip healthStartClip;
    public AudioClip shieldHitClip;
    public AudioClip shieldAlarmClip;

    private void Start()
    {
        activeSensitivity = defaultSensitivity;

        if (!hasFoundComponents)
        {
            //cManager = gameObject.GetComponent<ChildManager>();

            //pInventory = cManager.FindChildWithTag("Player Inventory").GetComponent<PlayerInventoryManager>();
            //SetSliders();
            SetHealthAndShieldValues();

            pController = GetComponent<PlayerController>();

            //shieldGO = cManager.FindChildWithTagScript("Shield Slider").gameObject;
            //motionTrackerGO = cManager.FindChildWithTagScript("Motion Tracker").gameObject;

            hasFoundComponents = true;
        }

        //gunCamera = GameObject.FindGameObjectWithTag("Player Camera").GetComponent<Camera>();
        //currentAmmoText = cManager.FindChildWithTag("Current Ammo Text").GetComponent<Text>();
        //totalAmmoText = cManager.FindChildWithTag("Total Ammo Text").GetComponent<Text>();
        //fragGrenadeText = cManager.FindChildWithTag("Frag Grenade Ammo Text").GetComponent<Text>();

        //GetCameras();
        //GetHitboxes();

        mainOriginalCameraPosition = new Vector3(mainCamera.transform.localPosition.x, mainCamera.transform.localPosition.y, mainCamera.transform.localPosition.z);
        thirdPersonGOLayer = thirdPersonGO.layer;

        if (swarmMode != null)
        {
            playerLivesIcon.SetActive(true);
            playerLivesText.gameObject.SetActive(true);
            playerLivesText.text = swarmMode.playerLives.ToString();
        }

        if (pController.PV.IsMine)
        {

        }
        else
        {
            firstPersonModels.layer = 23; // 24 = P1 FPS
            thirdPersonModels.layer = 0; // 0 = Default

            ChildManager childManager = firstPersonModels.GetComponent<ChildManager>();
            for (int i = 0; i < childManager.allChildren.Count; i++)
            {
                childManager.allChildren[i].layer = 23;
            }

            childManager = thirdPersonModels.GetComponent<ChildManager>();
            for (int i = 0; i < childManager.allChildren.Count; i++)
            {
                childManager.allChildren[i].layer = 0;
            }
        }

    }

    public void TakeDamage(int damage)
    {
        pController.PV.RPC("RPC_TakeDamage", RpcTarget.All, damage);
    }

    [PunRPC]
    void RPC_TakeDamage(int damage)
    {
        if (!pController.PV.IsMine)
            return;

        SetHealth(10, false, 0);
    }

    private void FixedUpdate()
    {
        fragGrenadeText.text = pInventory.grenades.ToString();

        if (!pController.isDualWielding)
        {

            //currentAmmoText.text = pInventory.currentAmmo.ToString();

            if (pInventory.activeWeapIs == 0)
            {
                if (pInventory.weaponsEquiped[0] != null)
                {
                    wProperties = pInventory.weaponsEquiped[0].gameObject.GetComponent<WeaponProperties>();

                    /*
                    if (wProperties.smallAmmo)
                    {
                        totalAmmoText.text = pInventory.smallAmmo.ToString();
                    }

                    else if (wProperties.heavyAmmo)
                    {
                        totalAmmoText.text = pInventory.heavyAmmo.ToString();
                    }

                    else if (wProperties.powerAmmo)
                    {
                        totalAmmoText.text = pInventory.powerAmmo.ToString();
                    }*/
                }
            }

            else if (pInventory.activeWeapIs == 1)
            {
                wProperties = pInventory.weaponsEquiped[1].gameObject.GetComponent<WeaponProperties>();

                /*
                if (wProperties.smallAmmo)
                {
                    totalAmmoText.text = pInventory.smallAmmo.ToString();
                }

                else if (wProperties.heavyAmmo)
                {
                    totalAmmoText.text = pInventory.heavyAmmo.ToString();
                }

                else if (wProperties.powerAmmo)
                {
                    totalAmmoText.text = pInventory.powerAmmo.ToString();
                }
                */
            }
        }

        if (pController.isDualWielding)
        {
            totalAmmoText.text = pInventory.smallAmmo.ToString();
            totalAmmoText2.text = totalAmmoText.text;

            AmmoInfo2.SetActive(true);
            GrenadeInfo.SetActive(false);

            currentAmmoText.text = pInventory.rightWeaponCurrentAmmo.ToString();

            dualWieldedWeaponAmmo.text = pInventory.leftWeaponCurrentAmmo.ToString();
        }
        else
        {
            AmmoInfo2.SetActive(false);
            GrenadeInfo.SetActive(true);
        }


        ////////////////////////////////////////////////////////////////////////////// Health and shield
        ///

        if (hasMotionTracker)
        {
            motionTrackerGO.SetActive(true);
        }
        else
        {
            motionTrackerGO.SetActive(false);
        }

        if (!hasShield)
        {
            shieldGO.SetActive(false);
        }
        else
        {
            shieldGO.SetActive(true);
        }

        HealthAndshieldRecharge();

        if (respawnStarted)
        {
            RespawnCountdown();
        }

        CheckRRIsOn();
    }

    /// <summary>
    /// /////////////////////////////////////////////////////////////////////////////////////// shield Slider Voids
    /// </summary>
    /*
    void SetSliders()
    {
        shieldSlider = cManager.FindChildWithTagScript("Shield Slider").GetComponent<Slider>();
        healthSlider = cManager.FindChildWithTagScript("Health Slider").GetComponent<Slider>();
    }
    */
    public void SetShield(int shieldDamage)
    {
        pController.Unscope();
        armorHasBeenHit = true;

        shieldRechargeCountdown = shieldRechargeDelay;
        healthRegenerationCountdown = healthRegenerationDelay;

        armorHasBeenHit = true;

        if (shieldSlider.value < shieldDamage)
        {
            Shield = 0;
            shieldSlider.value = 0;
            PlayShieldDownSound();
            StartCoroutine(PlayShieldAlarmSound());
        }
        else
        {
            Shield = Shield - shieldDamage;
            shieldSlider.value = shieldSlider.value - shieldDamage;
            PlayShieldHitSound();
        }
    }


    /// <summary>
    /// /////////////////////////////////////////////////////////////////////////////////// Health Slider Voids
    /// </summary>

    public void SetHealth(int healthDamage, bool headshot, int playerWhoShotThisPlayer)
    {
        pController.Unscope();
        healthHasBeenHit = true;

        shieldRechargeCountdown = shieldRechargeDelay;
        healthRegenerationCountdown = healthRegenerationDelay;

        if (healthSlider.value < healthDamage || headshot)
        {
            Health = 0;
            healthSlider.value = 0;
        }
        else
        {
            Health = Health - healthDamage;
            healthSlider.value = healthSlider.value - healthDamage;
            PlayHurtSound();
        }

        if (Health <= 0)
        {
            if (!headshot)
            {
                //Death(false, playerWhoShotThisPlayer);
                pController.PV.RPC("Die", RpcTarget.All, false, playerWhoShotThisPlayer);
                PlayDeathSound();
            }
            else
            {
                //Death(true, playerWhoShotThisPlayer);
                pController.PV.RPC("Die", RpcTarget.All, true, playerWhoShotThisPlayer);
                PlayDeathSound();
            }
        }
    }

    public void BleedthroughDamage(float damage, bool headshot, int playerWhoKilledThisPlayer)
    {
        pController.Unscope();
        shieldRechargeCountdown = shieldRechargeDelay;
        healthRegenerationCountdown = healthRegenerationDelay;

        if (!headshot)
        {
            if (hasShield)
            {
                if (Shield > 0)
                {
                    healthHasBeenHit = true;
                    armorHasBeenHit = true;

                    float damageLeft = damage - Shield;

                    if (damageLeft < 0)
                    {
                        damageLeft = 0;
                    }

                    Shield = Shield - damage;
                    shieldSlider.value = Shield;
                    PlayShieldHitSound();

                    if (Shield < 0)
                    {
                        Shield = 0;
                        shieldSlider.value = 0;
                        PlayShieldDownSound();
                        StartCoroutine(PlayShieldAlarmSound());
                    }

                    if (Shield == 0)
                    {
                        Health = Health - damageLeft;
                        healthSlider.value = healthSlider.value - damageLeft;
                        PlayHurtSound();
                    }

                    if (Health <= 0)
                    {
                        //Death(true, playerWhoKilledThisPlayer);
                        pController.PV.RPC("Die", RpcTarget.All, true, playerWhoKilledThisPlayer);
                        PlayDeathSound();
                    }
                }
                else if (Shield <= 0)
                {
                    healthHasBeenHit = true;
                    Health = Health - damage;
                    healthSlider.value = Health;

                    PlayHurtSound();

                    if (Health <= 0)
                    {
                        //Death(true, playerWhoKilledThisPlayer);
                        pController.PV.RPC("Die", RpcTarget.All, true, playerWhoKilledThisPlayer);
                        PlayDeathSound();
                    }
                }
            }
            else
            {
                healthHasBeenHit = true;
                Health = Health - damage;
                healthSlider.value = Health;

                PlayHurtSound();

                if (Health <= 0)
                {
                    //Death(true, playerWhoKilledThisPlayer);
                    pController.PV.RPC("Die", RpcTarget.All, true, playerWhoKilledThisPlayer);
                    PlayDeathSound();
                }
            }
        }
        else
        {
            Health = Health - damage;
            healthSlider.value = Health;

            Shield = 0;
            shieldSlider.value = 0;
            PlayShieldDownSound();
            StartCoroutine(PlayShieldAlarmSound());

            healthHasBeenHit = true;
            armorHasBeenHit = true;

            if (Health <= 0)
            {
                //Death(true, playerWhoKilledThisPlayer);
                pController.PV.RPC("Die", RpcTarget.All, true, playerWhoKilledThisPlayer);
                PlayDeathSound();
            }
        }
    }


    ////////////////////////////////////////////////////////////////////////////// Health and shield Recharge
    ///
    void HealthAndshieldRecharge()
    {
        if (armorHasBeenHit && hasShield)
        {
            shieldRechargeAllowed = false;
            shieldRechargeCountdown -= Time.deltaTime;

            if (shieldRechargeCountdown < 0 && hasShield && !needsShieldPack)
            {
                shieldRechargeAllowed = true;
                armorHasBeenHit = false;

            }
        }

        if (healthHasBeenHit)
        {
            healthRegenerationAllowed = false;
            healthRegenerationCountdown -= Time.deltaTime;

            if (healthRegenerationCountdown < 0 && !needsHealthPack)
            {
                healthRegenerationAllowed = true;
                healthHasBeenHit = false;
            }
            else if (healthRegenerationCountdown < 0 && !hasShield && !needsHealthPack)
            {
                healthRegenerationAllowed = true;
                healthHasBeenHit = false;
            }
        }

        if (shieldRechargeAllowed && shieldSlider.value < maxShield)
        {
            shieldSlider.value = shieldSlider.value + (shieldRechargeRate * 0.01f);
            Shield = shieldSlider.value;

            if (!shieldAudioSource.isPlaying)
            {
                PlayShieldStartSound();
                shieldAlarmAudioSource.Stop();
            }
        }

        if (healthRegenerationAllowed && healthSlider.value < maxHealth)
        {
            healthSlider.value = healthSlider.value + (healthRegenerationRate * 0.01f);
            Health = healthSlider.value;

            if (!healthRegenerating)
            {
                PlayHealthStartSound();
                healthRegenerating = true;
            }
        }

        if (Health == maxHealth)
        {
            healthRegenerating = false;
        }
    }

    [PunRPC]
    void Die(bool headshot, int playerWhoKilledThisPlayer)
    {
        if (isDead)
            return;

        UpdateMPPoints(playerRewiredID, playerWhoKilledThisPlayer);
        Debug.Log("Player Death Script"); 
        pController.Unscope();
        gameObject.GetComponent<ScreenEffects>().orangeScreen.SetActive(false);
        isDead = true;

        pController.isShooting = false;

        playerWhoKilledThisPlayerID = playerWhoKilledThisPlayer;

        if (multiplayerManager != null)
        {
            multiplayerManager.AddToScore(playerWhoKilledThisPlayerID);
        }

        int ogThirdPersonLayer = thirdPersonGO.layer;

        float rotationX = mainCamera.gameObject.GetComponent<Transform>().transform.rotation.x;
        float rotationY = mainCamera.gameObject.GetComponent<Transform>().transform.rotation.y;
        float rotationZ = mainCamera.gameObject.GetComponent<Transform>().transform.rotation.z;

        float positionX = mainCamera.gameObject.GetComponent<Transform>().transform.position.x;
        float positionY = mainCamera.gameObject.GetComponent<Transform>().transform.position.y;
        float positionZ = mainCamera.gameObject.GetComponent<Transform>().transform.position.z;

        mainCamera.gameObject.GetComponent<Transform>().transform.Rotate(30, 0, 0);
        mainCamera.gameObject.GetComponent<Transform>().transform.localPosition = new Vector3(mainOriginalCameraPosition.x, 2, -2.5f);

        if (playerRewiredID == 0)
        {
            //mainCamera.cullingMask |= (1 << 28);
            gunCamera.cullingMask &= ~(1 << 24);
            //Debug.Log("Player 1 Camera2");
        }
        else if (playerRewiredID == 1)
        {
            //Debug.Log("Player 2 Camera1");
            //mainCamera.cullingMask |= (1 << 29);
            gunCamera.cullingMask &= ~(1 << 25);
            //Debug.Log("Player 2 Camera2");
        }
        else if (playerRewiredID == 2)
        {
            //mainCamera.cullingMask |= (1 << 30);
            gunCamera.cullingMask &= ~(1 << 26);
        }
        else if (playerRewiredID == 3)
        {
            //mainCamera.cullingMask |= (1 << 31);
            gunCamera.cullingMask &= ~(1 << 27);
        }

        foreach (GameObject go in hitboxes)
        {
            if (go != null)
            {
                go.layer = 23;
                go.SetActive(false);

                if (go.GetComponent<BoxCollider>() != null)
                {
                    go.GetComponent<BoxCollider>().enabled = false;
                }

                if (go.GetComponent<SphereCollider>() != null)
                {
                    go.GetComponent<SphereCollider>().enabled = false;
                }

                characterController.enabled = false;
            }
        }

        foreach (GameObject go in thirdPersonGO.GetComponent<ChildManager>().allChildren)
        {
            if (go != null)
            {
                go.layer = 23;
            }
        }

        DropAllOnDeath();
        SpawnRagdoll();
        //pController.PV.RPC("SpawnRagdoll", RpcTarget.All);

        //var go1 = Instantiate(thirsPersonDeathGO, thirdPersonDeathSpawnPoint.transform.position, thirdPersonDeathSpawnPoint.transform.rotation);

        respawnCountdown = respawnTime;
        respawnStarted = true;

        shieldAlarmAudioSource.Stop();
        shieldAudioSource.Stop();
    }

    //[PunRPC]
    void SpawnRagdoll()
    {

        var ragdoll = pController.objectPool.SpawnPooledPlayerRagdoll();

        // LAG with the Head and Chest, unknown cause
        //////////////////////////////
        
        //ragdoll.GetComponent<RagdollPrefab>().ragdollHead.position = ragdollScript.Head.position;
        //Debug.Log("Player Head Pos: " + ragdollScript.Head.position + "; Ragdoll head position: " + ragdoll.GetComponent<RagdollPrefab>().ragdollHead.position);
        //ragdoll.GetComponent<RagdollPrefab>().ragdollChest.position = ragdollScript.Chest.position;
        ragdoll.GetComponent<RagdollPrefab>().ragdollHips.position = ragdollScript.Hips.position;

        //ragdoll.GetComponent<RagdollPrefab>().ragdollHead.rotation = ragdollScript.Head.rotation;
        //ragdoll.GetComponent<RagdollPrefab>().ragdollChest.rotation = ragdollScript.Chest.rotation;
        ragdoll.GetComponent<RagdollPrefab>().ragdollHips.rotation = ragdollScript.Hips.rotation;



        ragdoll.GetComponent<RagdollPrefab>().ragdollUpperArmLeft.position = ragdollScript.UpperArmLeft.position;
        ragdoll.GetComponent<RagdollPrefab>().ragdollUpperArmRight.position = ragdollScript.UpperArmRight.position;

        ragdoll.GetComponent<RagdollPrefab>().ragdollUpperArmLeft.rotation = ragdollScript.UpperArmLeft.rotation;
        ragdoll.GetComponent<RagdollPrefab>().ragdollUpperArmRight.rotation = ragdollScript.UpperArmRight.rotation;



        ragdoll.GetComponent<RagdollPrefab>().ragdollLowerArmLeft.position = ragdollScript.LowerArmLeft.position;
        ragdoll.GetComponent<RagdollPrefab>().ragdollLowerArmRight.position = ragdollScript.LowerArmRight.position;

        ragdoll.GetComponent<RagdollPrefab>().ragdollLowerArmLeft.rotation = ragdollScript.LowerArmLeft.rotation;
        ragdoll.GetComponent<RagdollPrefab>().ragdollLowerArmRight.rotation = ragdollScript.LowerArmRight.rotation;



        ragdoll.GetComponent<RagdollPrefab>().ragdollUpperLegLeft.position = ragdollScript.UpperLegLeft.position;
        ragdoll.GetComponent<RagdollPrefab>().ragdollUpperLegRight.position = ragdollScript.UpperLegRight.position;

        ragdoll.GetComponent<RagdollPrefab>().ragdollUpperLegLeft.rotation = ragdollScript.UpperLegLeft.rotation;
        ragdoll.GetComponent<RagdollPrefab>().ragdollUpperLegRight.rotation = ragdollScript.UpperLegRight.rotation;



        ragdoll.GetComponent<RagdollPrefab>().ragdollLowerLegLeft.position = ragdollScript.LowerLegLeft.position;
        ragdoll.GetComponent<RagdollPrefab>().ragdollLowerLegRight.position = ragdollScript.LowerLegRight.position;

        ragdoll.GetComponent<RagdollPrefab>().ragdollLowerLegLeft.rotation = ragdollScript.LowerLegLeft.rotation;
        ragdoll.GetComponent<RagdollPrefab>().ragdollLowerLegRight.rotation = ragdollScript.LowerLegRight.rotation;

        ragdoll.SetActive(true);
    }

    void RespawnCountdown()
    {
        if (respawnStarted)
        {
            respawnCountdown -= Time.deltaTime;
        }

        if (respawnCountdown <= 0)
        {
            if (swarmMode != null)
            {
                if (swarmMode.playerLives > 0)
                {
                    //Respawn();
                    pController.PV.RPC("Die", RpcTarget.All);
                    respawnStarted = false;
                    respawnCountdown = 0;
                }
            }
            else
            {
                //Respawn();
                pController.PV.RPC("Respawn", RpcTarget.All);
                respawnStarted = false;
                respawnCountdown = 0;
            }
        }
    }

    [PunRPC]
    void Respawn()
    {
        if (!isDead)
            return;
        isDead = false;
        mainCamera.gameObject.GetComponent<Transform>().transform.Rotate(-30, 0, 0);
        mainCamera.gameObject.GetComponent<Transform>().transform.localPosition = new Vector3(mainOriginalCameraPosition.x, mainOriginalCameraPosition.y, mainOriginalCameraPosition.z);

            if (playerRewiredID == 0)
            {
                mainCamera.cullingMask &= ~(1 << 28);
                gunCamera.cullingMask |= (1 << 24);
            }
            else if (playerRewiredID == 1)
            {
                mainCamera.cullingMask &= ~(1 << 29);
                gunCamera.cullingMask |= (1 << 25);
            }
            else if (playerRewiredID == 2)
            {
                mainCamera.cullingMask |= (1 << 30);
                gunCamera.cullingMask |= (1 << 26);
            }
            else if (playerRewiredID == 3)
            {
                //mainCamera.cullingMask |= (1 << 31);
                gunCamera.cullingMask |= (1 << 27);
            }


        foreach (GameObject go in thirdPersonGO.GetComponent<ChildManager>().allChildren)
        {
            if (go != null)
            {

                if (playerRewiredID == 0)
                {
                    if (pController.PV.IsMine)
                        go.layer = 28;
                    else
                        go.layer = 29;
                }
                else if (playerRewiredID == 1)
                {
                    go.layer = 29;
                }
                else if (playerRewiredID == 2)
                {
                    go.layer = 30;
                }
                else if (playerRewiredID == 3)
                {
                    go.layer = 31;
                }
            }
        }

        Health = maxHealth;
        healthSlider.value = maxHealth;

        if (hasShield)
        {
            Shield = maxShield;
            shieldSlider.value = maxShield;
        }

        pInventory.smallAmmo = 48;
        pInventory.heavyAmmo = 30;
        pInventory.powerAmmo = 0;
        pInventory.grenades = 2;

        StartCoroutine(pInventory.EquipStartingWeapon());
        if (pController.isDualWielding)
        {
            pInventory.leftWeapon.SetActive(false);
            pInventory.leftWeapon = null;
            pInventory.rightWeapon.SetActive(false);
            pInventory.rightWeapon = null;
        }
        pInventory.weaponsEquiped[1] = null;

        if (multiplayerManager != null)
        {
            int randomSpawn = Random.Range(0, multiplayerManager.GenericSpawns.Length + 1);

            Debug.Log("Number of spawns = " + multiplayerManager.GenericSpawns.Length);
            Debug.Log("Randwom spawn is = " + multiplayerManager.GenericSpawns[randomSpawn].gameObject.name);

            gameObject.transform.position = new Vector3(multiplayerManager.GenericSpawns[randomSpawn].gameObject.transform.position.x,
                multiplayerManager.GenericSpawns[randomSpawn].gameObject.transform.position.y + 2,
                multiplayerManager.GenericSpawns[randomSpawn].gameObject.transform.position.z);

            gameObject.transform.rotation = multiplayerManager.GenericSpawns[randomSpawn].gameObject.transform.rotation;
        }

        if (swarmMode != null)
        {
            int randomSpawn = Random.Range(0, swarmMode.GenericSpawns.Length);

            Debug.Log("Number of spawns = " + swarmMode.GenericSpawns.Length);
            Debug.Log("Randwom spawn is = " + swarmMode.GenericSpawns[randomSpawn].gameObject.name);

            gameObject.transform.position = new Vector3(swarmMode.GenericSpawns[randomSpawn].gameObject.transform.position.x,
                swarmMode.GenericSpawns[randomSpawn].gameObject.transform.position.y + 2,
                swarmMode.GenericSpawns[randomSpawn].gameObject.transform.position.z);

            gameObject.transform.rotation = swarmMode.GenericSpawns[randomSpawn].gameObject.transform.rotation;

            swarmMode.playerLives = swarmMode.playerLives - 1;
            swarmMode.UpdatePlayerLives();
        }

        foreach (GameObject go in hitboxes)
        {
            if (go != null)
            {
                go.SetActive(true);
                go.layer = 13;

                if (go.GetComponent<BoxCollider>() != null)
                {
                    go.GetComponent<BoxCollider>().enabled = true;
                }

                if (go.GetComponent<SphereCollider>() != null)
                {
                    go.GetComponent<SphereCollider>().enabled = true;
                }

                characterController.enabled = true;
            }
        }
    }

    void SetHealthAndShieldValues()
    {
        Health = maxHealth;
        Shield = maxShield;

        healthSlider.value = maxHealth;
        shieldSlider.value = maxShield;
        healthSlider.maxValue = maxHealth;
        shieldSlider.maxValue = maxShield;
    }


    public void DropActiveWeapon(GameObject weaponEquippedToDrop)
    {
        if (weaponEquippedToDrop.GetComponent<WeaponProperties>().currentAmmo > 0)
        {
            foreach (GameObject weaponToDrop in weaponDropSpawnPoint1.GetComponent<ItemDropScript>().weapons)
            {
                if (weaponToDrop != null)
                {
                    if (weaponEquippedToDrop.name == weaponToDrop.name)
                    {
                        var weaponDropped1 = Instantiate(weaponToDrop, weaponDropSpawnPoint1.transform.position, weaponDropSpawnPoint1.transform.rotation);
                        weaponDropped1.name = weaponDropped1.name.Replace("(Clone)", "");

                        if (!mScript.isMovingForward)
                        {
                            weaponDropped1.GetComponent<Rigidbody>().AddForce(transform.forward * 250);
                        }
                        else
                        {
                            weaponDropped1.GetComponent<Rigidbody>().AddForce(transform.forward * 500);
                        }
                        weaponDropped1.GetComponent<LootableWeapon>().ammoInThisWeapon = weaponEquippedToDrop.GetComponent<WeaponProperties>().currentAmmo;
                        weaponDropped1.GetComponent<LootableWeapon>().extraAmmo = 0;

                        if (weaponEquippedToDrop.GetComponent<WeaponProperties>().isDualWieldable)
                        {
                            weaponDropped1.GetComponent<LootableWeapon>().isDualWieldable = true;
                        }

                        Destroy(weaponDropped1, 60);
                    }
                }
            }
        }
    }

    void DropAllOnDeath()
    {
        foreach (GameObject weaponToDrop in weaponDropSpawnPoint1.GetComponent<ItemDropScript>().weapons)
        {
            if (weaponToDrop != null)
            {
                if (pInventory.weaponsEquiped[0].name == weaponToDrop.name)
                {
                    var weaponDropped1 = Instantiate(weaponToDrop, weaponDropSpawnPoint1.transform.position, weaponDropSpawnPoint1.transform.rotation);
                    weaponDropped1.name = weaponDropped1.name.Replace("(Clone)", "");
                    weaponDropped1.GetComponent<Rigidbody>().AddForce(transform.forward * 250);
                    weaponDropped1.GetComponent<LootableWeapon>().ammoInThisWeapon = pInventory.weaponsEquiped[0].GetComponent<WeaponProperties>().currentAmmo;
                    Destroy(weaponDropped1, 30);
                }
                else if (pInventory.weaponsEquiped[1] != null && pInventory.weaponsEquiped[1].name == weaponToDrop.name)
                {
                    var weaponDropped2 = Instantiate(weaponToDrop, weaponDropSpawnPoint2.transform.position, weaponDropSpawnPoint2.transform.rotation);
                    weaponDropped2.name = weaponDropped2.name.Replace("(Clone)", "");
                    weaponDropped2.GetComponent<Rigidbody>().AddForce(transform.forward * 250);
                    weaponDropped2.GetComponent<LootableWeapon>().ammoInThisWeapon = pInventory.weaponsEquiped[1].GetComponent<WeaponProperties>().currentAmmo;
                    Destroy(weaponDropped2, 30);
                }
            }
        }
    }

    void PlayHurtSound()
    {
        int randomSound = Random.Range(0, hurtClips.Length);
        playerVoice.clip = hurtClips[randomSound];
        playerVoice.Play();
    }

    void PlayDeathSound()
    {
        int randomSound = Random.Range(0, deathClips.Length);
        playerVoice.clip = deathClips[randomSound];
        playerVoice.Play();
    }

    void PlayShieldHitSound()
    {
        shieldAudioSource.clip = shieldHitClip;
        shieldAudioSource.Play();
    }

    void PlayShieldStartSound()
    {
        shieldAudioSource.clip = shieldStartClip;
        shieldAudioSource.Play();
    }

    void PlayShieldDownSound()
    {
        shieldAudioSource.clip = shieldDownClip;
        shieldAudioSource.Play();
    }

    void PlayHealthStartSound()
    {
        if (shieldAudioSource.isPlaying)
        {
            shieldAudioSource.Stop();
        }
        shieldAudioSource.clip = healthStartClip;
        shieldAudioSource.Play();
    }

    public void PlayMeleeSound()
    {
        int randomSound = Random.Range(0, meleeClips.Length);
        playerVoice.clip = meleeClips[randomSound];
        playerVoice.Play();
    }

    IEnumerator PlayShieldAlarmSound()
    {
        yield return new WaitForSeconds(0.25f);

        if (hasShield)
        {
            if (Shield <= 0)
            {
                if (!shieldAlarmAudioSource.isPlaying)
                {
                    shieldAlarmAudioSource.clip = shieldAlarmClip;
                    shieldAlarmAudioSource.Play();
                }
            }
        }
    }

    void CheckRRIsOn()
    {
        if (cScript != null)
        {
            if (!pController.isAiming)
            {
                if (cScript.RRisActive)
                {
                    activeSensitivity = defaultSensitivity / 5;
                }
                else
                {
                    activeSensitivity = defaultSensitivity;
                }
            }
        }
    }

    void UpdateMPPoints(int playerWhoDied, int playerWhoKilled)
    {
        if (allPlayerScripts.playerMPProperties)
            allPlayerScripts.playerMPProperties.UpdatePoints(playerWhoDied, playerWhoKilled);
    }
}
