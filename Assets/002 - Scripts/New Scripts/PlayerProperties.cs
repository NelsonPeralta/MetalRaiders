using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class PlayerProperties : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Singletons")]
    public SpawnManager spawnManager;
    public AllPlayerScripts allPlayerScripts;
    public PlayerManager playerManager;
    public MultiplayerManager multiplayerManager;
    public GameObjectPool gameObjectPool;
    public WeaponPool weaponPool;
    public OnlineSwarmManager onlineSwarmManager;

    [Header("Models")]
    public GameObject firstPersonModels;
    public GameObject thirdPersonModels;

    [Header("Player Info")]
    public int maxHealth;
    public int maxShield;
    public float Health;
    public float Shield;
    public float meleeDamage; // default: 150
    public bool isDead;
    public bool isRespawning;
    public Coroutine respawnCoroutine;
    public float respawnTime = 5;
    public int playerRewiredID;
    public int lastPlayerWhoDamagedThisPlayerPVID; // Revenge Medal
    public bool hasShield = false;
    public bool needsHealthPack = false;
    public bool needsShieldPack = false;
    public bool hasMotionTracker = false;

    [Header("Networked Variables")]
    public float networkedHealth;
    public Vector3 networkedPosition;
    public Vector3 lagDistance;
    public float lagDistanceMagnitude;

    [Header("Other Scripts")]
    public PlayerInventory pInventory;
    public WeaponProperties wProperties;
    //public ChildManager cManager;
    public PlayerController pController;
    public SwarmMode swarmMode;
    public Movement movement;
    public CrosshairScript cScript;
    public AimAssist aimAssist;
    public WeaponPickUp wPickup;
    public AimAssist raycastScript;
    public PlayerSurroundings pSurroundings;

    [Header("Camera Options")]
    [Tooltip("Default value for camera field of view (40 is recommended).")]
    public float defaultFov = 60.0f;

    [Header("UI Components Text")]
    public Text currentAmmoText;
    public Text dualWieldedWeaponAmmo;
    public Text totalAmmoText;
    public Text totalAmmoText2;
    public Text fragGrenadeText;
    public Text InformerText;
    public Text playerLivesText;
    public Text HealthDebuggerText;
    public Text readHealthDebuggerText;

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
    public bool triggerHealthRecharge = false;

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
    public AudioClip sprintingClip;
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

    public PhotonView PV;

    public void UpdateHealthTextDebugger()
    {
        HealthDebuggerText.text = Health.ToString();
        readHealthDebuggerText.text = networkedHealth.ToString();
    }

    private void Start()
    {
        spawnManager = SpawnManager.spawnManagerInstance;
        playerManager = PlayerManager.playerManagerInstance;
        multiplayerManager = MultiplayerManager.multiplayerManagerInstance;
        onlineSwarmManager = OnlineSwarmManager.onlineSwarmManagerInstance;
        gameObjectPool = GameObjectPool.gameObjectPoolInstance;
        weaponPool = WeaponPool.weaponPoolInstance;
        playerManager.allPlayers.Add(this);
        PV = GetComponent<PhotonView>();
        gameObject.name = $"Player ({PV.Owner.NickName})";
        //PhotonNetwork.SendRate = 100;
        //PhotonNetwork.SerializationRate = 50;
        Health = maxHealth;
        HealthDebuggerText.text = $"Health: {Health.ToString()}";
        networkedHealth = Health;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;

        if (onlineSwarmManager)
        {
            needsHealthPack = true;
            allPlayerScripts.playerUIComponents.swarmLivesHolder.SetActive(true);
            allPlayerScripts.playerUIComponents.swarmLivesText.text = onlineSwarmManager.playerLives.ToString();
            allPlayerScripts.playerUIComponents.multiplayerPoints.SetActive(false);
            allPlayerScripts.playerUIComponents.swarmPoints.SetActive(true);
            allPlayerScripts.playerUIComponents.swarmPointsText.text = 0.ToString();
        }

        if (respawnTime <= healthRegenerationDelay)
            respawnTime = healthRegenerationDelay + 0.5f; // To avoid the health regen sound going off

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
        //StartCoroutine(SlightlyIncreaseHealth());
    }


    private void Update()
    {
        UpdateLagDistance();
    }

    void UpdateLagDistance()
    {
        if (PV.IsMine)
            return;
        lagDistance = networkedPosition - transform.position;
        lagDistanceMagnitude = lagDistance.magnitude;

        //if(lagDistance.magnitude > 0.001f)
        //{
        //    transform.position = networkedPosition;
        //    lagDistance = Vector3.zero;
        //}
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
        pController.ScopeOut();
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

    public void Damage(int healthDamage, bool headshot, int playerWhoShotThisPlayerPhotonId)
    {
        PV.RPC("Damage_RPC", RpcTarget.All, Health - healthDamage, headshot, playerWhoShotThisPlayerPhotonId);
        //Damage_RPC(Health - healthDamage, playerWhoShotThisPlayerPhotonId);
        //if (!PhotonNetwork.IsMasterClient)
        //    return;

    }

    [PunRPC]
    void Damage_RPC(float newHealth, bool wasHeadshot, int playerWhoShotThisPlayerPhotonId)
    {
        if (PV.IsMine)
            allPlayerScripts.damageIndicatorManager.SpawnNewDamageIndicator(playerWhoShotThisPlayerPhotonId);
        lastPlayerWhoDamagedThisPlayerPVID = playerWhoShotThisPlayerPhotonId;
        Health = newHealth;
        healthSlider.value = Health;

        GameObject bloodHit = allPlayerScripts.playerController.objectPool.SpawnPooledBloodHit();
        bloodHit.transform.position = gameObject.transform.position + new Vector3(0, -0.4f, 0);
        bloodHit.SetActive(true);

        triggerHealthRecharge = true;
        healthRegenerationCountdown = healthRegenerationDelay;
        PlayHurtSound();
        UpdateHealthTextDebugger();
        pController.ScopeOut();

        if (Health <= 0)
            isDead = true;

        Die(wasHeadshot);
    }

    public void BleedthroughDamage(float damage, bool headshot, int playerWhoKilledThisPlayer)
    {
        //Debug.Log("Bleedthrough Damage");
        //pController.ScopeOut();
        //shieldRechargeCountdown = shieldRechargeDelay;
        //healthRegenerationCountdown = healthRegenerationDelay;

        //if (!headshot)
        //{
        //    if (hasShield)
        //    {
        //        if (Shield > 0)
        //        {
        //            triggerHealthRecharge = true;
        //            armorHasBeenHit = true;

        //            float damageLeft = damage - Shield;

        //            if (damageLeft < 0)
        //            {
        //                damageLeft = 0;
        //            }

        //            Shield = Shield - damage;
        //            shieldSlider.value = Shield;
        //            PlayShieldHitSound();

        //            if (Shield < 0)
        //            {
        //                Shield = 0;
        //                shieldSlider.value = 0;
        //                PlayShieldDownSound();
        //                StartCoroutine(PlayShieldAlarmSound());
        //            }

        //            if (Shield == 0)
        //            {
        //                Health = Health - damageLeft;
        //                healthSlider.value = healthSlider.value - damageLeft;
        //                PlayHurtSound();
        //            }

        //            if (Health <= 0)
        //            {
        //                //Death(true, playerWhoKilledThisPlayer);
        //                pController.PV.RPC("Die", RpcTarget.All, true, playerWhoKilledThisPlayer);
        //                PlayDeathSound();
        //            }
        //        }
        //        else if (Shield <= 0)
        //        {
        //            triggerHealthRecharge = true;
        //            Health = Health - damage;
        //            healthSlider.value = Health;

        //            PlayHurtSound();

        //            if (Health <= 0)
        //            {
        //                //Death(true, playerWhoKilledThisPlayer);
        //                pController.PV.RPC("Die", RpcTarget.All, true, playerWhoKilledThisPlayer);
        //                PlayDeathSound();
        //            }
        //        }
        //    }
        //    else
        //    {
        //        triggerHealthRecharge = true;
        //        Health = Health - damage;
        //        healthSlider.value = Health;

        //        PlayHurtSound();

        //        if (Health <= 0)
        //        {
        //            //Death(true, playerWhoKilledThisPlayer);
        //            pController.PV.RPC("Die", RpcTarget.All, true, playerWhoKilledThisPlayer);
        //            PlayDeathSound();
        //        }
        //    }
        //}
        //else
        //{
        //    Health = Health - damage;
        //    healthSlider.value = Health;

        //    Shield = 0;
        //    shieldSlider.value = 0;
        //    PlayShieldDownSound();
        //    StartCoroutine(PlayShieldAlarmSound());

        //    triggerHealthRecharge = true;
        //    armorHasBeenHit = true;

        //    if (Health <= 0)
        //    {
        //        //Death(true, playerWhoKilledThisPlayer);
        //        pController.PV.RPC("Die", RpcTarget.All, true, playerWhoKilledThisPlayer);
        //        PlayDeathSound();
        //    }
        //}
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

        if (triggerHealthRecharge)
        {
            healthRegenerationAllowed = false;
            healthRegenerationCountdown -= Time.deltaTime;

            if (healthRegenerationCountdown < 0 && !needsHealthPack)
            {
                healthRegenerationAllowed = true;
                triggerHealthRecharge = false;
            }
            else if (healthRegenerationCountdown < 0 && !hasShield && !needsHealthPack)
            {
                healthRegenerationAllowed = true;
                triggerHealthRecharge = false;
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
                PlayHealthRechargeSound();
                healthRegenerating = true;
            }
        }

        if (Health == maxHealth)
        {
            healthRegenerating = false;
        }
    }

    void Die(bool wasHeadshot)
    {
        if (!isDead || respawnCoroutine != null || isRespawning)
            return;
        if (lastPlayerWhoDamagedThisPlayerPVID != 0 && multiplayerManager)
            multiplayerManager.AddToScore(lastPlayerWhoDamagedThisPlayerPVID, PV.ViewID, wasHeadshot);
        if (onlineSwarmManager)
            onlineSwarmManager.RemovePlayerLife();
        pInventory.holsteredWeapon = null;
        isRespawning = true;
        Debug.Log($"{PhotonNetwork.LocalPlayer.NickName} died");
        pController.DisableCrouch();
        PlayDeathSound();
        allPlayerScripts.playerUIComponents.scoreboard.CloseScoreboard();
        respawnCoroutine = StartCoroutine(Respawn_Coroutine());
        StartCoroutine(MidRespawnAction());
    }

    IEnumerator MidRespawnAction()
    {
        yield return new WaitForSeconds(respawnTime / 2);
        Health = maxHealth;
        networkedHealth = Health;
        Transform spawnPoint = spawnManager.GetGenericSpawnpoint();
        transform.position = spawnPoint.position + new Vector3(0, 2, 0);
        transform.rotation = spawnPoint.rotation;
        isDead = false;
    }

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

    IEnumerator Respawn_Coroutine()
    {
        gameObject.GetComponent<ScreenEffects>().orangeScreen.SetActive(false);

        pController.isShooting = false;

        mainCamera.gameObject.GetComponent<Transform>().transform.Rotate(30, 0, 0);
        mainCamera.gameObject.GetComponent<Transform>().transform.localPosition = new Vector3(mainOriginalCameraPosition.x, 2, -2.5f);

        gunCamera.cullingMask &= ~(1 << 24);

        foreach (GameObject go in hitboxes)
            if (go != null)
            {
                go.layer = 23;
                go.SetActive(false);

                if (go.GetComponent<BoxCollider>() != null)
                    go.GetComponent<BoxCollider>().enabled = false;

                if (go.GetComponent<SphereCollider>() != null)
                    go.GetComponent<SphereCollider>().enabled = false;

                characterController.enabled = false;
            }

        foreach (GameObject go in thirdPersonGO.GetComponent<ChildManager>().allChildren)
            if (go != null)
                go.layer = 23;

        SpawnRagdoll();
        respawnCoroutine = null;
        Health = maxHealth;
        yield return new WaitForSeconds(respawnTime);
        Respawn();
    }

    void Respawn()
    {
        if (!isRespawning)
            return;
        movement.ResetCharacterControllerProperties();
        isRespawning = false;
        respawnCoroutine = null;
        pController.ScopeOut();
        Health = maxHealth;
        healthSlider.value = maxHealth;


        if (hasShield)
        {
            Shield = maxShield;
            shieldSlider.value = maxShield;
        }


        mainCamera.gameObject.GetComponent<Transform>().transform.localRotation = allPlayerScripts.cameraScript.mainCamDefaultLocalRotation;
        mainCamera.gameObject.GetComponent<Transform>().transform.localPosition = allPlayerScripts.cameraScript.mainCamDefaultLocalPosition;

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

        StartCoroutine(MakeThirdPersonModelVisible());
        //foreach (GameObject go in thirdPersonGO.GetComponent<ChildManager>().allChildren)
        //{
        //    if (go != null)
        //    {

        //        if (playerRewiredID == 0)
        //        {
        //            if (pController.PV.IsMine)
        //                go.layer = 28;
        //            else
        //                go.layer = 29;
        //        }
        //        else if (playerRewiredID == 1)
        //        {
        //            go.layer = 29;
        //        }
        //        else if (playerRewiredID == 2)
        //        {
        //            go.layer = 30;
        //        }
        //        else if (playerRewiredID == 3)
        //        {
        //            go.layer = 31;
        //        }
        //    }
        //}



        pInventory.smallAmmo = 72;
        pInventory.heavyAmmo = 60;
        pInventory.powerAmmo = 4;
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

        //if (multiplayerManager != null)
        //{
        //    int randomSpawn = Random.Range(0, multiplayerManager.GenericSpawns.Length + 1);

        //    Debug.Log("Number of spawns = " + multiplayerManager.GenericSpawns.Length);
        //    Debug.Log("Randwom spawn is = " + multiplayerManager.GenericSpawns[randomSpawn].gameObject.name);

        //    gameObject.transform.position = new Vector3(multiplayerManager.GenericSpawns[randomSpawn].gameObject.transform.position.x,
        //        multiplayerManager.GenericSpawns[randomSpawn].gameObject.transform.position.y + 2,
        //        multiplayerManager.GenericSpawns[randomSpawn].gameObject.transform.position.z);

        //    gameObject.transform.rotation = multiplayerManager.GenericSpawns[randomSpawn].gameObject.transform.rotation;
        //}

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

    IEnumerator MakeThirdPersonModelVisible()
    {
        yield return new WaitForSeconds(0.1f);

        foreach (GameObject go in hitboxes)
            if (go != null)
            {
                go.SetActive(true);
                go.layer = 13;

                if (go.GetComponent<BoxCollider>() != null)
                    go.GetComponent<BoxCollider>().enabled = true;

                if (go.GetComponent<SphereCollider>() != null)
                    go.GetComponent<SphereCollider>().enabled = true;

                characterController.enabled = true;
            }

        foreach (GameObject go in thirdPersonGO.GetComponent<ChildManager>().allChildren)
            if (go != null)
                if (playerRewiredID == 0)
                    if (pController.PV.IsMine)
                        go.layer = 28;
                    else
                        go.layer = 29;

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

                        if (!movement.isMovingForward)
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
        if (Health <= 0)
            return;
        int randomSound = Random.Range(0, hurtClips.Length);
        playerVoice.clip = hurtClips[randomSound];
        playerVoice.Play();
    }

    void PlayDeathSound()
    {
        Debug.Log("Playing Death Sound");
        int randomSound = Random.Range(0, deathClips.Length);
        playerVoice.clip = deathClips[randomSound];
        playerVoice.Play();
    }

    public void PlaySprintingSound()
    {
        PV.RPC("PlaySprintingSound_RPC", RpcTarget.All);
    }

    [PunRPC]
    public void PlaySprintingSound_RPC()
    {
        if (playerVoice.isPlaying)
            return;
        playerVoice.loop = true;
        playerVoice.volume = 0.05f;
        playerVoice.clip = sprintingClip;
        playerVoice.Play();
    }

    public void StopPlayingPlayerVoice()
    {
        PV.RPC("StopPlayingPlayerVoice_RPC", RpcTarget.All);
    }

    [PunRPC]
    public void StopPlayingPlayerVoice_RPC()
    {
        playerVoice.loop = false;
        playerVoice.volume = 0.5f;
        playerVoice.Stop();
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

    public void PlayHealthRechargeSound()
    {
        Debug.Log("Health Reachrge Sound");
        if (isDead || isRespawning)
            return;
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


    void UpdateMPPoints(int playerWhoDied, int playerWhoKilled)
    {
        //if (allPlayerScripts.playerMPProperties)
        //    allPlayerScripts.playerMPProperties.UpdatePoints(playerWhoDied, playerWhoKilled);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //Debug.Log("Writing Health");
            stream.SendNext(Health);
            stream.SendNext(transform.position);
        }
        else
        {
            networkedHealth = (float)stream.ReceiveNext();
            networkedPosition = (Vector3)stream.ReceiveNext();
            //Debug.Log($"Reading Health: {readHealth}. Health: + {Health}. NEW Reading Health{readHealth}");// has just respawned not being counted
            //if (newReadHealth != readHealth)
            //{
            //    Health = Mathf.Min(newReadHealth, readHealth);
            //    readHealth = Mathf.Min(newReadHealth, readHealth);
            //    if (PhotonNetwork.IsMasterClient)
            //        PV.RPC("FixHealth", RpcTarget.All, Health);
            //}


            //UpdateHealthTextDebugger();
        }
    }

    public void LeaveRoomWithDelay()
    {
        StartCoroutine(LeaveRoomWithDelay_Coroutine());
    }

    public IEnumerator LeaveRoomWithDelay_Coroutine(int delay = 5)
    {
        yield return new WaitForSeconds(5);

        Cursor.visible = true;
        PhotonNetwork.LeaveRoom();
        //SceneManager.LoadScene("Main Menu");
        PhotonNetwork.LoadLevel(0);
    }

    public void DisableBullet(GameObject bulletGO)
    {
        for (int i = 0; i < gameObjectPool.bullets.Count; i++)
            if (bulletGO == gameObjectPool.bullets[i])
                PV.RPC("DiableBullet_RPC", RpcTarget.All, i);
    }

    [PunRPC]
    void DiableBullet_RPC(int index)
    {
        gameObjectPool.bullets[index].SetActive(false);
    }

}
