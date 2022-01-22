using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviourPunCallbacks
{
    public delegate void PlayerEvent(Player playerProperties);
    public PlayerEvent OnPlayerDeath, OnPlayerHitPointsChanged, OnPlayerDamaged;

    [Header("Singletons")]
    public SpawnManager spawnManager;
    public AllPlayerScripts allPlayerScripts;
    public PlayerManager playerManager;
    public GameObjectPool gameObjectPool;
    public WeaponPool weaponPool;

    [Header("Models")]
    public GameObject firstPersonModels;
    public GameObject thirdPersonModels;

    [Header("Other Scripts")]
    public PlayerInventory pInventory;
    public CrosshairManager cScript;
    public AimAssist aimAssist;
    public PlayerSurroundings pSurroundings;

    [Header("Camera Options")]
    [Tooltip("Default value for camera field of view (40 is recommended).")]
    public float defaultFov = 60.0f;

    [Header("Cameras")]
    public Camera mainCamera;
    public Camera gunCamera;
    public Camera deathCamera;

    public Vector3 mainOriginalCameraPosition;

    [Header("Hitboxes")]
    public GameObject[] hitboxes = new GameObject[10];
    public CharacterController characterController;

    [Header("Ragdoll")]
    public RagdollSpawn ragdollScript;

    [Header("Player Voice")]
    public AudioSource playerVoice;
    public AudioClip sprintingClip;
    public AudioClip[] meleeClips;
    public AudioClip[] hurtClips;
    public AudioClip[] deathClips;

    public PhotonView PV;
    
    [Header("Player Info")]
    public int playerRewiredID;
    public bool needsHealthPack;

    // Private Variables
    int _maxHitPoints = 250;
    float _hitPoints = 250;
    int _meleeDamage = 150;
    bool _isRespawning;
    bool _isDead;
    int _respawnTime = 5;

    int _defaultRespawnTime = 4;

    int _defaultHealingCountdown = 4;
    float _healingCountdown;
    public float hitPoints
    {
        get { return _hitPoints; }

        set
        {
            float previousValue = _hitPoints;
            _hitPoints = Mathf.Ceil(value);

            if (previousValue > value)
                OnPlayerDamaged?.Invoke(this);

            if (previousValue != value)
                OnPlayerHitPointsChanged?.Invoke(this);

            if (_hitPoints <= 0)
                isDead = true;
        }
    }

    public int maxHitPoints { get { return _maxHitPoints; } set { _maxHitPoints = value; } }
    public int meleeDamage { get { return _meleeDamage; } }
    bool hitboxesEnabled
    {
        set
        {
            thirdPersonModels.SetActive(value);
            foreach (GameObject go in hitboxes)
            {
                if (!value)
                    go.layer = 31;
                else
                    go.layer = 7;
                go.SetActive(value);

                if (go.GetComponent<BoxCollider>() != null)
                    go.GetComponent<BoxCollider>().enabled = value;

                if (go.GetComponent<SphereCollider>() != null)
                    go.GetComponent<SphereCollider>().enabled = value;

                characterController.enabled = value;
            }
        }
    }
    public bool isRespawning
    {
        get { return _isRespawning; }
        set { _isRespawning = value; }
    }

    public bool isDead
    {
        get { return _isDead; }
        set
        {
            _isDead = value;
            if (_isDead)
                OnPlayerDeath?.Invoke(this);
        }
    }

    public float healingCountdown
    {
        get { return _healingCountdown; }
        private set { _healingCountdown = value; }
    }
    private void Awake()
    {
        if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
        {
            maxHitPoints = 250;
            hitPoints = maxHitPoints;
        }
        else if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
        {
            maxHitPoints = 100;
            hitPoints = maxHitPoints;
            needsHealthPack = true;
        }
    }
    private void Start()
    {
        spawnManager = SpawnManager.spawnManagerInstance;
        playerManager = PlayerManager.playerManagerInstance;
        gameObjectPool = GameObjectPool.gameObjectPoolInstance;
        weaponPool = WeaponPool.weaponPoolInstance;
        playerManager.allPlayers.Add(this);
        PV = GetComponent<PhotonView>();
        gameObject.name = $"Player ({PV.Owner.NickName}. Is mine: {PV.IsMine})";
        //PhotonNetwork.SendRate = 100;
        //PhotonNetwork.SerializationRate = 50;

        mainOriginalCameraPosition = new Vector3(mainCamera.transform.localPosition.x, mainCamera.transform.localPosition.y, mainCamera.transform.localPosition.z);


        if (GetComponent<PlayerController>().PV.IsMine)
        {

        }
        else
        {
            firstPersonModels.layer = 23; // 24 = P1 FPS
            thirdPersonModels.layer = 0; // 0 = Default
        }
        //StartCoroutine(SlightlyIncreaseHealth());

        OnPlayerDeath += OnPlayerDeath_Delegate;
        OnPlayerDamaged += OnPlayerDamaged_Delegate;
    }
    private void Update()
    {
        HitPointsRecharge();
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


    public bool CanBeDamaged()
    {
        if (hitPoints <= 0 || isDead || isRespawning)
            return false;
        return true;
    }
    public void Damage(int healthDamage, bool headshot, int playerWhoShotThisPlayerPhotonId)
    {
        if (hitPoints <= 0 || isDead || isRespawning)
            return;

        PV.RPC("Damage_RPC", RpcTarget.All, hitPoints - healthDamage, headshot, playerWhoShotThisPlayerPhotonId);
        //Damage_RPC(Health - healthDamage, playerWhoShotThisPlayerPhotonId);
        //if (!PhotonNetwork.IsMasterClient)
        //    return;

    }

    [PunRPC]
    void Damage_RPC(float _newHealth, bool wasHeadshot, int playerWhoShotThisPlayerPhotonId)
    {
        if (PV.IsMine)
            allPlayerScripts.damageIndicatorManager.SpawnNewDamageIndicator(playerWhoShotThisPlayerPhotonId);
        hitPoints = _newHealth;

        ////float newHealth = Mathf.Clamp(hitPoints, 0f, (float)(maxHitPoints - maxShield));
        //float newShield = 0;



        //if (newHealth >= (maxHitPoints - maxShield))
        //{
        //    newShield = Mathf.Clamp(hitPoints - (maxHitPoints - maxShield), 0f, (float)maxShield);
        //}

        //if (newHealth < maxHitPoints - maxShield)
        //{

        //    GameObject bloodHit = allPlayerScripts.playerController.objectPool.SpawnPooledBloodHit();
        //    bloodHit.transform.position = gameObject.transform.position + new Vector3(0, -0.4f, 0);
        //    bloodHit.SetActive(true);
        //    PlayHurtSound();
        //}
        //pController.ScopeOut();

        if (isDead)
        {
            MultiplayerManager.instance.AddPlayerKill(new MultiplayerManager.AddPlayerKillStruct(playerWhoShotThisPlayerPhotonId, PV.ViewID, wasHeadshot));
        }
    }
    void HitPointsRecharge()
    {
        if(healingCountdown > 0)
        {
            healingCountdown -= Time.deltaTime;
        }

        if(healingCountdown <= 0 && hitPoints < maxHitPoints)
        {
            hitPoints += (Time.deltaTime);
        }
        //if (armorHasBeenHit && hasShield)
        //{
        //    shieldRechargeAllowed = false;
        //    shieldRechargeCountdown -= Time.deltaTime;

        //    if (shieldRechargeCountdown < 0 && hasShield && !needsShieldPack)
        //    {
        //        shieldRechargeAllowed = true;
        //        armorHasBeenHit = false;

        //    }
        //}

        //if (triggerHealthRecharge)
        //{
        //    healthRegenerationAllowed = false;
        //    healthRegenerationCountdown -= Time.deltaTime;

        //    if (healthRegenerationCountdown < 0 && !needsHealthPack)
        //    {
        //        healthRegenerationAllowed = true;
        //        triggerHealthRecharge = false;
        //    }
        //    else if (healthRegenerationCountdown < 0 && !hasShield && !needsHealthPack)
        //    {
        //        healthRegenerationAllowed = true;
        //        triggerHealthRecharge = false;
        //    }
        //}

        //if (shieldRechargeAllowed && shieldSlider.value < maxShield)
        //{
        //    shieldSlider.value = shieldSlider.value + (shieldRechargeRate * 0.01f);
        //    Shield = shieldSlider.value;

        //    if (!shieldAudioSource.isPlaying)
        //    {
        //        PlayShieldStartSound();
        //        shieldAlarmAudioSource.Stop();
        //    }
        //}

        //if (healthRegenerationAllowed && hitPoints < maxHitPoints)
        //{
        //    hitPoints += healthRegenerationRate * 0.01f;

        //    float newHealth = Mathf.Clamp(hitPoints, 0f, (float)(maxHitPoints - maxShield));
        //    float newShield = 0;

        //    if (hitPoints >= (maxHitPoints - maxShield))
        //    {
        //        newShield = Mathf.Clamp(hitPoints - (maxHitPoints - maxShield), 0f, (float)maxShield);
        //    }


        //    healthSlider.value = newHealth;
        //    shieldSlider.value = newShield;


        //    if (!healthRegenerating)
        //    {
        //        if (maxShield > 0 && newShield > 0)
        //        {
        //            StopShieldAlarmSound();
        //            HideThirdPersionShieldElectricityModel();
        //            ShowThirdPersonShieldRechargeModel();
        //            PlayHealthRechargeSound();
        //            healthRegenerating = true;
        //        }
        //        else if (maxShield <= 0)
        //        {
        //            PlayHealthRechargeSound();
        //            healthRegenerating = true;
        //        }
        //    }
        //}

        //if (hitPoints == maxHitPoints)
        //{
        //    healthRegenerating = false;
        //}
    }
    IEnumerator MidRespawnAction()
    {
        yield return new WaitForSeconds(_defaultRespawnTime / 2);
        hitPoints = maxHitPoints;
        Transform spawnPoint = spawnManager.GetGenericSpawnpoint();
        transform.position = spawnPoint.position + new Vector3(0, 2, 0);
        transform.rotation = spawnPoint.rotation;
        isDead = false;
    }

    void SpawnRagdoll()
    {
        var ragdoll = GetComponent<PlayerController>().objectPool.SpawnPooledPlayerRagdoll();

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

        GetComponent<PlayerController>().isShooting = false;

        mainCamera.gameObject.GetComponent<Transform>().transform.Rotate(30, 0, 0);
        mainCamera.gameObject.GetComponent<Transform>().transform.localPosition = new Vector3(mainOriginalCameraPosition.x, 2, -2.5f);

        gunCamera.enabled = false;

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

        SpawnRagdoll();
        hitPoints = maxHitPoints;
        yield return new WaitForSeconds(_defaultRespawnTime);
        Respawn();
    }

    void Respawn()
    {
        if (!isRespawning)
            return;
        GetComponent<Movement>().ResetCharacterControllerProperties();
        isRespawning = false;
        GetComponent<PlayerController>().ScopeOut();
        hitPoints = maxHitPoints;

        //float newHealth = Mathf.Clamp(hitPoints, 0f, (float)(maxHitPoints - maxShield));
        //float newShield = 0;

        //if (newHealth >= (maxHitPoints - maxShield))
        //    newShield = Mathf.Clamp(hitPoints - (maxHitPoints - maxShield), 0f, (float)maxShield);

        //shieldSlider.value = newShield;
        //healthSlider.value = newHealth;

        mainCamera.gameObject.GetComponent<Transform>().transform.localRotation = allPlayerScripts.cameraScript.mainCamDefaultLocalRotation;
        mainCamera.gameObject.GetComponent<Transform>().transform.localPosition = allPlayerScripts.cameraScript.mainCamDefaultLocalPosition;
        gunCamera.enabled = true;

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

        pInventory.smallAmmo = 72;
        pInventory.heavyAmmo = 60;
        pInventory.powerAmmo = 4;
        pInventory.grenades = 2;

        StartCoroutine(pInventory.EquipStartingWeapon());
        pInventory.weaponsEquiped[1] = null;

        hitboxesEnabled = true;
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
    }
    void PlayHurtSound()
    {
        if (hitPoints <= 0)
            return;
        for (int i = 0; i < hurtClips.Length; i++)
            if (playerVoice.isPlaying && playerVoice.clip == hurtClips[i])
                return;
        int randomSound = Random.Range(0, hurtClips.Length);
        playerVoice.clip = hurtClips[randomSound];
        playerVoice.Play();
    }

    void PlayDeathSound()
    {
        Debug.Log("Playing Death Sound");
        for (int i = 0; i < deathClips.Length; i++)
            if (playerVoice.isPlaying && playerVoice.clip == deathClips[i])
                return;
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

    public void PlayMeleeSound()
    {
        int randomSound = Random.Range(0, meleeClips.Length);
        playerVoice.clip = meleeClips[randomSound];
        playerVoice.Play();
    }
    public void LeaveRoomWithDelay()
    {
        StartCoroutine(LeaveRoomWithDelay_Coroutine());
    }

    public IEnumerator LeaveRoomWithDelay_Coroutine(int delay = 5)
    {
        yield return new WaitForSeconds(delay);

        Cursor.visible = true;
        PhotonNetwork.LeaveRoom();
        //SceneManager.LoadScene("Main Menu");
        PhotonNetwork.LoadLevel(0);
    }

    void OnPlayerDamaged_Delegate(Player player)
    {
        healingCountdown = (float)_defaultHealingCountdown;
    }
    void OnPlayerDeath_Delegate(Player playerProperties)
    {
        if (!isDead || isRespawning)
            return;
        isRespawning = true;

        thirdPersonModels.SetActive(false);
        hitboxesEnabled = false;

        pInventory.holsteredWeapon = null;
        GetComponent<PlayerController>().DisableCrouch();
        //StopShieldAlarmSound();
        PlayDeathSound();
        GetComponent<PlayerUI>().scoreboard.CloseScoreboard();
        StartCoroutine(MidRespawnAction());
    }
}
