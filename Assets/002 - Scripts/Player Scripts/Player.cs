using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviourPunCallbacks
{
    public delegate void PlayerEvent(Player playerProperties);
    public PlayerEvent OnPlayerDeath, OnPlayerHitPointsChanged, OnPlayerDamaged, OnPlayerHealthDamage, OnPlayerHealthRechargeStarted, OnPlayerShieldRechargeStarted, OnPlayerShieldDamaged, OnPlayerShieldBroken, OnPlayerRespawned;

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
    public PlayerInventory playerInventory;
    public CrosshairManager cScript;
    public AimAssist aimAssist;
    public PlayerSurroundings playerSurroundings;

    [Header("Camera Options")]
    [Tooltip("Default value for camera field of view (40 is recommended).")]
    public float defaultFov = 60.0f;

    [Header("Cameras")]
    public Camera mainCamera;
    public Camera gunCamera;
    public Camera deathCamera;

    public Vector3 mainOriginalCameraPosition;

    [Header("Hitboxes")]
    public List<GameObject> hitboxes = new List<GameObject>();

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
    int _maxHealthPoints = 100;
    int _maxShieldPoints = 150;
    float _hitPoints = 250;
    int _meleeDamage = 150;
    bool _isRespawning;
    bool _isDead, _isHealing;
    int _respawnTime = 5;

    int _defaultRespawnTime = 4;

    int _defaultHealingCountdown = 4;
    [SerializeField] float _healingCountdown;
    [SerializeField] float _shieldRechargeCountdown;

    float _healthHealingIncrement = (100 * 2);
    float _shieldHealingIncrement = (150 * 0.5f);

    bool _hasArmor;

    public GameObject bloodImpact;
    Vector3 _impactPos;
    public bool hasArmor // Used to handle armor seller for Swarm Mode
    {
        get { return _hasArmor; }
        set
        {
            if (value)
            {
                _hasArmor = true;
                maxHitPoints = 250;
                hitPoints = 250;

                needsHealthPack = false;

                GetComponent<PlayerUI>().EnableArmorUI();
            }
        }
    }

    bool _hasMeleeUpgrade;
    public bool hasMeleeUpgrade
    {
        get { return _hasMeleeUpgrade; }
        set
        {
            bool previousValue = _hasMeleeUpgrade;
            _hasMeleeUpgrade = value;

            if (value && !_hasMeleeUpgrade)
                _meleeDamage *= 3;
        }
    }
    public float hitPoints
    {
        get { return _hitPoints; }

        set
        {
            float previousValue = _hitPoints;
            _hitPoints = Mathf.Clamp(value, 0, _maxHitPoints);

            if (previousValue > value)
                OnPlayerDamaged?.Invoke(this);

            if (previousValue != value)
                OnPlayerHitPointsChanged?.Invoke(this);

            if (maxHitPoints == 250)
            {
                if (value >= maxHealthPoints && value < previousValue)
                {
                    Debug.Log("OnPlayerShieldDamaged");
                    OnPlayerShieldDamaged?.Invoke(this);
                }

                if (value <= maxHealthPoints && previousValue > maxHealthPoints)
                    OnPlayerShieldBroken?.Invoke(this);
            }

            if (value < maxHealthPoints && previousValue <= maxHealthPoints && previousValue > value)
                OnPlayerHealthDamage?.Invoke(this);



            if (_hitPoints <= 0)
                isDead = true;

            impactPos = null;
        }
    }

    public int maxHitPoints { get { return _maxHitPoints; } set { _maxHitPoints = value; } }

    public int maxHealthPoints
    {
        get { return _maxHealthPoints; }
        private set { _maxHealthPoints = value; }
    }
    public int maxShieldPoints
    {
        get { return _maxShieldPoints; }
        private set { _maxShieldPoints = value; }
    }
    public int meleeDamage
    {
        get { return _meleeDamage; }
    }
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

                GetComponent<CharacterController>().enabled = value;
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
            bool previousValue = _isDead;
            _isDead = value;

            if (value && !previousValue)
                OnPlayerDeath?.Invoke(this);
        }
    }

    public float healthHealingIncrement
    {
        get { return _healthHealingIncrement; }
    }
    public float healingCountdown
    {
        get { return _healingCountdown; }
        private set
        {
            _healingCountdown = Mathf.Clamp(value, 0, _defaultHealingCountdown);
        }
    }

    public float shieldRechargeCountdown
    {
        get { return _shieldRechargeCountdown; }
        private set
        {
            _shieldRechargeCountdown = Mathf.Clamp(value, 0, _defaultHealingCountdown + (maxHealthPoints / _healthHealingIncrement));
        }
    }
    public float defaultHealingCountdown
    {
        get { return _defaultHealingCountdown; }
    }

    public Vector3? impactPos
    {
        protected set
        {
            try
            {
                _impactPos = (Vector3)value;
            }
            catch { }
        }
        get { return _impactPos; }
    }
    private void Awake()
    {
        if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
        {
            hasArmor = true;
        }
        else if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
        {
            maxHitPoints = 100;
            hitPoints = maxHitPoints;
            needsHealthPack = true;
        }
    }

    string _nickName;
    public string nickName
    {
        get { return _nickName; }
        private protected set { _nickName = value; }
    }
    private void Start()
    {
        spawnManager = SpawnManager.spawnManagerInstance;
        gameObjectPool = GameObjectPool.gameObjectPoolInstance;
        weaponPool = WeaponPool.weaponPoolInstance;
        PV = GetComponent<PhotonView>();
        _nickName = PV.Owner.NickName;
        GetComponent<PlayerUI>().isMineText.text = $"IM: {PV.IsMine}";
        gameObject.name = $"Player ({PV.Owner.NickName}. IM: {PV.IsMine})";
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
        OnPlayerHealthDamage += OnPlayerHealthDamaged_Delegate;
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
    public void Damage(int healthDamage, bool headshot, int playerWhoShotThisPlayerPhotonId, Vector3? impactPos = null, string damageSource = null)
    {
        if (hitPoints <= 0 || isDead || isRespawning)
            return;

        PV.RPC("Damage_RPC", RpcTarget.All, hitPoints - healthDamage, headshot, playerWhoShotThisPlayerPhotonId, impactPos, damageSource);
        //Damage_RPC(Health - healthDamage, playerWhoShotThisPlayerPhotonId);
        //if (!PhotonNetwork.IsMasterClient)
        //    return;

    }

    [PunRPC]
    void Damage_RPC(float _newHealth, bool wasHeadshot, int playerWhoShotThisPlayerPhotonId, Vector3? impactPos = null, string damageSource = null)
    {
        int damage = (int)(hitPoints - _newHealth);
        if (PV.IsMine)
        {
            allPlayerScripts.damageIndicatorManager.SpawnNewDamageIndicator(playerWhoShotThisPlayerPhotonId);

            if (damageSource != null && damageSource.Contains("grenade"))
            {
                KillFeedManager killFeedManager = GetComponent<KillFeedManager>();
                int damageSourceSpriteCode = KillFeedManager.killFeedWeaponCodeDict[damageSource];
                string colorCode = KillFeedManager.killFeedColorCodeDict["orange"];

                killFeedManager.EnterNewFeed($"You took {damage} <sprite={damageSourceSpriteCode} color={colorCode}> damage");
            }

        }
        try
        {
            this.impactPos = (Vector3)impactPos;
        }
        catch (System.Exception e) { }

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
            if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
                MultiplayerManager.instance.AddPlayerKill(new MultiplayerManager.AddPlayerKillStruct(playerWhoShotThisPlayerPhotonId, PV.ViewID, wasHeadshot));
            else if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
                GetComponent<PlayerSwarmMatchStats>().deaths++;

            string sourcePlayerName = GameManager.instance.GetPlayerWithPhotonViewId(playerWhoShotThisPlayerPhotonId).nickName;

            int hsCode = KillFeedManager.killFeedSpecialCodeDict["headshot"];
            foreach (KillFeedManager kfm in FindObjectsOfType<KillFeedManager>())
            {
                if (sourcePlayerName != nickName)
                {
                    string feed = $"{sourcePlayerName} killed";
                    if (kfm.GetComponent<Player>() != this)
                    {
                        if (kfm.GetComponent<Player>().nickName == sourcePlayerName)
                        {
                            try
                            {
                                int damageSourceSpriteCode = KillFeedManager.killFeedWeaponCodeDict[damageSource];
                                feed = $"You <sprite={damageSourceSpriteCode}>";
                                if (wasHeadshot)
                                    feed += $"<sprite={hsCode}>";
                                feed += $" <color=\"red\">{nickName}";
                                kfm.EnterNewFeed(feed);
                            }
                            catch
                            {
                                kfm.EnterNewFeed($"You killed {sourcePlayerName}");
                            }
                        }
                        else
                        {
                            try
                            {
                                int damageSourceSpriteCode = KillFeedManager.killFeedWeaponCodeDict[damageSource];
                                feed = $"<color=\"red\">{sourcePlayerName} <color=\"white\"><sprite={damageSourceSpriteCode}>";

                                if(wasHeadshot)
                                    feed += $"<sprite={hsCode}>";

                                feed += $" <color=\"red\">{nickName}";
                                kfm.EnterNewFeed(feed);
                            }
                            catch
                            {
                                kfm.EnterNewFeed($"<color=\"red\">{sourcePlayerName} <color=\"white\">killed <color=\"red\">{nickName}");
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            int damageSourceSpriteCode = KillFeedManager.killFeedWeaponCodeDict[damageSource];
                            feed = $"<color=\"red\">{sourcePlayerName} <color=\"white\"><sprite={damageSourceSpriteCode}>";

                            if (wasHeadshot)
                                feed += $"<sprite={hsCode}>";

                            feed += $" you";
                            kfm.EnterNewFeed(feed);
                        }
                        catch
                        {
                            kfm.EnterNewFeed($"<color=\"red\">{sourcePlayerName} <color=\"white\"> killed you");
                        }
                    }
                }
                else
                {
                    kfm.EnterNewFeed($"<color=\"white\"> {nickName} committed suicide");
                }
            }
        }
    }
    void HitPointsRecharge()
    {
        if (healingCountdown > 0)
        {
            healingCountdown -= Time.deltaTime;
        }

        if (healingCountdown <= 0 && hitPoints < maxHitPoints && !needsHealthPack)
        {
            if (!_isHealing)
                OnPlayerShieldRechargeStarted?.Invoke(this);

            _isHealing = true;
            if (hitPoints < maxHealthPoints)
                hitPoints += (Time.deltaTime * _healthHealingIncrement);
            else
                hitPoints += (Time.deltaTime * _shieldHealingIncrement);

            if (hitPoints == maxHitPoints)
                _isHealing = false;
        }

        if (shieldRechargeCountdown > 0)
        {
            shieldRechargeCountdown -= Time.deltaTime;
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
        var ragdoll = FindObjectOfType<GameObjectPool>().SpawnPooledPlayerRagdoll();

        // LAG with the Head and Chest, unknown cause
        //////////////////////////////

        //ragdoll.GetComponent<RagdollPrefab>().ragdollHead.position = ragdollScript.Head.position;
        //Debug.Log("Player Head Pos: " + ragdollScript.Head.position + "; Ragdoll head position: " + ragdoll.GetComponent<RagdollPrefab>().ragdollHead.position);
        //ragdoll.GetComponent<RagdollPrefab>().ragdollChest.position = ragdollScript.Chest.position;
        ragdoll.GetComponent<RagdollPrefab>().ragdollHips.position = GetComponent<RagdollSpawn>().Hips.position;

        //ragdoll.GetComponent<RagdollPrefab>().ragdollHead.rotation = ragdollScript.Head.rotation;
        //ragdoll.GetComponent<RagdollPrefab>().ragdollChest.rotation = ragdollScript.Chest.rotation;
        ragdoll.GetComponent<RagdollPrefab>().ragdollHips.rotation = GetComponent<RagdollSpawn>().Hips.rotation;



        ragdoll.GetComponent<RagdollPrefab>().ragdollUpperArmLeft.position = GetComponent<RagdollSpawn>().UpperArmLeft.position;
        ragdoll.GetComponent<RagdollPrefab>().ragdollUpperArmRight.position = GetComponent<RagdollSpawn>().UpperArmRight.position;

        ragdoll.GetComponent<RagdollPrefab>().ragdollUpperArmLeft.rotation = GetComponent<RagdollSpawn>().UpperArmLeft.rotation;
        ragdoll.GetComponent<RagdollPrefab>().ragdollUpperArmRight.rotation = GetComponent<RagdollSpawn>().UpperArmRight.rotation;



        ragdoll.GetComponent<RagdollPrefab>().ragdollLowerArmLeft.position = GetComponent<RagdollSpawn>().LowerArmLeft.position;
        ragdoll.GetComponent<RagdollPrefab>().ragdollLowerArmRight.position = GetComponent<RagdollSpawn>().LowerArmRight.position;

        ragdoll.GetComponent<RagdollPrefab>().ragdollLowerArmLeft.rotation = GetComponent<RagdollSpawn>().LowerArmLeft.rotation;
        ragdoll.GetComponent<RagdollPrefab>().ragdollLowerArmRight.rotation = GetComponent<RagdollSpawn>().LowerArmRight.rotation;



        ragdoll.GetComponent<RagdollPrefab>().ragdollUpperLegLeft.position = GetComponent<RagdollSpawn>().UpperLegLeft.position;
        ragdoll.GetComponent<RagdollPrefab>().ragdollUpperLegRight.position = GetComponent<RagdollSpawn>().UpperLegRight.position;

        ragdoll.GetComponent<RagdollPrefab>().ragdollUpperLegLeft.rotation = GetComponent<RagdollSpawn>().UpperLegLeft.rotation;
        ragdoll.GetComponent<RagdollPrefab>().ragdollUpperLegRight.rotation = GetComponent<RagdollSpawn>().UpperLegRight.rotation;



        ragdoll.GetComponent<RagdollPrefab>().ragdollLowerLegLeft.position = GetComponent<RagdollSpawn>().LowerLegLeft.position;
        ragdoll.GetComponent<RagdollPrefab>().ragdollLowerLegRight.position = GetComponent<RagdollSpawn>().LowerLegRight.position;

        ragdoll.GetComponent<RagdollPrefab>().ragdollLowerLegLeft.rotation = GetComponent<RagdollSpawn>().LowerLegLeft.rotation;
        ragdoll.GetComponent<RagdollPrefab>().ragdollLowerLegRight.rotation = GetComponent<RagdollSpawn>().LowerLegRight.rotation;

        ragdoll.SetActive(true);
    }

    IEnumerator Respawn_Coroutine()
    {
        gameObject.GetComponent<ScreenEffects>().orangeScreen.SetActive(false);

        GetComponent<PlayerController>().isShooting = false;

        mainCamera.gameObject.GetComponent<Transform>().transform.Rotate(30, 0, 0);
        mainCamera.gameObject.GetComponent<Transform>().transform.localPosition = new Vector3(mainOriginalCameraPosition.x, 2, -2.5f);

        gunCamera.enabled = false;

        hitboxesEnabled = false;

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

        playerInventory.smallAmmo = 72;
        playerInventory.heavyAmmo = 60;
        playerInventory.powerAmmo = 4;
        playerInventory.grenades = 2;

        StartCoroutine(playerInventory.EquipStartingWeapon());
        playerInventory.weaponsEquiped[1] = null;

        hitboxesEnabled = true;

        OnPlayerRespawned?.Invoke(this);
    }

    IEnumerator MakeThirdPersonModelVisible()
    {
        yield return new WaitForSeconds(0.1f);

        hitboxesEnabled = true;
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
        _isHealing = false;
        if (!needsHealthPack)
        {
            healingCountdown = _defaultHealingCountdown;
            shieldRechargeCountdown = _defaultHealingCountdown;
            if (hitPoints <= maxHealthPoints)
                shieldRechargeCountdown = _defaultHealingCountdown + ((maxHealthPoints - hitPoints) / _healthHealingIncrement);
        }
    }

    void OnPlayerHealthDamaged_Delegate(Player player)
    {
        var a = Instantiate(bloodImpact, _impactPos, Quaternion.identity);
        Destroy(a, 1);
    }
    void OnPlayerDeath_Delegate(Player playerProperties)
    {
        isRespawning = true;

        thirdPersonModels.SetActive(false);
        hitboxesEnabled = false;

        if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
            SwarmManager.instance.livesLeft--;

        playerInventory.holsteredWeapon = null;
        GetComponent<PlayerController>().DisableCrouch();
        //StopShieldAlarmSound();
        PlayDeathSound();
        GetComponent<PlayerUI>().scoreboard.CloseScoreboard();
        StartCoroutine(Respawn_Coroutine());
        StartCoroutine(MidRespawnAction());
    }
}
