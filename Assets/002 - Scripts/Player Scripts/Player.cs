using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Player : MonoBehaviourPunCallbacks
{
    public delegate void PlayerEvent(Player playerProperties);
    public PlayerEvent OnPlayerDeath, OnPlayerHitPointsChanged, OnPlayerDamaged, OnPlayerHealthDamage, OnPlayerHealthRechargeStarted, OnPlayerShieldRechargeStarted, OnPlayerShieldDamaged, OnPlayerShieldBroken, OnPlayerRespawnEarly, OnPlayerRespawned;

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
    public Transform weaponDropPoint;
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

    public float shieldPoints
    {
        get { return Mathf.Clamp((hitPoints - maxHealthPoints), 0, maxShieldPoints); }
    }

    public float healthPoints
    {
        get { return (hitPoints - shieldPoints); }
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

    public bool isLocal
    {
        get { return PV.IsMine; }
    }

    public string localNickName
    {
        get
        {
            if (rid > 0)
                return $"{nickName} ({rid})";
            return nickName;
        }
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

    [SerializeField] string _nickName;
    public string nickName
    {
        get { return _nickName; }
        private protected set
        {
            if (PV.IsMine)
            {
                _nickName = value;
                if (rid > 0)
                    _nickName += $" ({rid})";

                PV.RPC("UpdateNickName_RPC", RpcTarget.All, nickName);
            }
        }

    }

    [PunRPC]
    void UpdateNickName_RPC(string nn)
    {
        _nickName = nn;
    }

    public int rid
    {
        get { return GetComponent<PlayerController>().rid; }
    }
    private void Start()
    {
        spawnManager = SpawnManager.spawnManagerInstance;
        gameObjectPool = GameObjectPool.gameObjectPoolInstance;
        weaponPool = FindObjectOfType<WeaponPool>();
        PV = GetComponent<PhotonView>();

        if (rid > 0)
            nickName = GameManager.GetLocalMasterPlayer().PV.Owner.NickName;
        else
            nickName = PV.Owner.NickName;
        GetComponent<PlayerUI>().isMineText.text = $"IM: {PV.IsMine}";
        gameObject.name = $"{PV.Owner.NickName} ({rid}). IM: {PV.IsMine}";
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
    public bool CanBeDamaged()
    {
        if (hitPoints <= 0 || isDead || isRespawning)
            return false;
        return true;
    }
    public void Damage(int healthDamage, bool headshot, int playerWhoShotThisPlayerPhotonId, Vector3? impactPos = null, string damageSource = null, bool isGroin = false)
    {
        if (hitPoints <= 0 || isDead || isRespawning)
            return;

        try
        { // Hit Marker Handling
            Player p = GameManager.GetPlayerWithPhotonViewId(playerWhoShotThisPlayerPhotonId);

            Debug.Log($"{hitPoints} vs {maxShieldPoints}");
            Debug.Log($"{healthPoints} vs {healthDamage}");
            if (headshot)
            {
                if (healthPoints <= healthDamage)
                    p.GetComponent<PlayerUI>().SpawnHitMarker(PlayerUI.HitMarkerType.HeadshotKill);
                else
                    p.GetComponent<PlayerUI>().SpawnHitMarker(PlayerUI.HitMarkerType.Headshot);
            }
            else
            {
                if (healthPoints <= healthDamage)
                    p.GetComponent<PlayerUI>().SpawnHitMarker(PlayerUI.HitMarkerType.Kill);
                else
                    p.GetComponent<PlayerUI>().SpawnHitMarker();
            }
        }
        catch { }

        PV.RPC("Damage_RPC", RpcTarget.All, hitPoints - healthDamage, headshot, playerWhoShotThisPlayerPhotonId, impactPos, damageSource, isGroin);
    }

    [PunRPC]
    void Damage_RPC(float _newHealth, bool wasHeadshot, int playerWhoShotThisPlayerPhotonId, Vector3? impactPos = null, string damageSource = null, bool isGroin = false)
    {
        int damage = (int)(hitPoints - _newHealth);
        if (PV.IsMine)
        {
            GetComponent<PlayerController>().ScopeOut();
            allPlayerScripts.damageIndicatorManager.SpawnNewDamageIndicator(playerWhoShotThisPlayerPhotonId);

            try
            {
                KillFeedManager killFeedManager = GetComponent<KillFeedManager>();
                int damageSourceSpriteCode = KillFeedManager.killFeedWeaponCodeDict[damageSource];

                if (damageSource.Contains("grenade"))
                {
                    string colorCode = KillFeedManager.killFeedColorCodeDict["orange"];
                    killFeedManager.EnterNewFeed($"You took {damage} <sprite={damageSourceSpriteCode} color={colorCode}> damage");
                }
                else if (damageSource.Contains("melee"))
                {
                    string colorCode = KillFeedManager.killFeedColorCodeDict["yellow"];
                    killFeedManager.EnterNewFeed($"You took {meleeDamage} <sprite={damageSourceSpriteCode} color={colorCode}> damage");
                }
            }
            catch { }
        }
        try
        {
            this.impactPos = (Vector3)impactPos;
        }
        catch (System.Exception e) { }

        hitPoints = _newHealth;

        if (isDead)
        {
            Player sourcePlayer = GameManager.GetPlayerWithPhotonViewId(playerWhoShotThisPlayerPhotonId);
            string sourcePlayerName = sourcePlayer.nickName;

            int hsCode = KillFeedManager.killFeedSpecialCodeDict["headshot"];
            int nsCode = KillFeedManager.killFeedSpecialCodeDict["nutshot"];
            string youColorCode = KillFeedManager.killFeedColorCodeDict["blue"];
            string weaponColorCode = playerInventory.activeWeapon.ammoType.ToString().ToLower();

            foreach (KillFeedManager kfm in FindObjectsOfType<KillFeedManager>())
            {
                if (this != sourcePlayer)
                {
                    string feed = $"{sourcePlayer.nickName} killed";
                    if (kfm.GetComponent<Player>() != this)
                    {
                        if (kfm.GetComponent<Player>().nickName == sourcePlayerName)
                        {
                            try
                            {
                                int damageSourceSpriteCode = KillFeedManager.killFeedWeaponCodeDict[damageSource];
                                feed = $"<color={youColorCode}>You <color=\"white\"><sprite={damageSourceSpriteCode}>";

                                if (wasHeadshot)
                                    feed += $"<sprite={hsCode}>";

                                if (isGroin)
                                    feed += $"<sprite={nsCode}>";

                                feed += $" <color=\"red\">{nickName}";
                                kfm.EnterNewFeed(feed);
                            }
                            catch
                            {
                                kfm.EnterNewFeed($"<color={youColorCode}>You <color=\"white\"> killed {sourcePlayerName}");
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

                            feed += $" <color={youColorCode}>You";
                            kfm.EnterNewFeed(feed);
                        }
                        catch
                        {
                            kfm.EnterNewFeed($"<color=\"red\">{sourcePlayerName} <color=\"white\"> killed <color={youColorCode}>You");
                        }
                    }
                }
                else
                {
                    kfm.EnterNewFeed($"<color=\"white\"> {nickName} committed suicide");
                }
            }

            if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
                MultiplayerManager.instance.AddPlayerKill(new MultiplayerManager.AddPlayerKillStruct(playerWhoShotThisPlayerPhotonId, PV.ViewID, wasHeadshot));
            else if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
                GetComponent<PlayerSwarmMatchStats>().deaths++;
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
    }
    IEnumerator MidRespawnAction()
    {
        yield return new WaitForSeconds(_defaultRespawnTime / 2);
        hitPoints = maxHitPoints;
        Transform spawnPoint = spawnManager.GetRandomSafeSpawnPoint();
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
        OnPlayerRespawnEarly?.Invoke(this);

        GetComponent<Movement>().ResetCharacterControllerProperties();
        isRespawning = false;
        GetComponent<PlayerController>().ScopeOut();
        hitPoints = maxHitPoints;

        mainCamera.gameObject.GetComponent<Transform>().transform.localRotation = allPlayerScripts.cameraScript.mainCamDefaultLocalRotation;
        mainCamera.gameObject.GetComponent<Transform>().transform.localPosition = allPlayerScripts.cameraScript.mainCamDefaultLocalPosition;
        gunCamera.enabled = true;

        StartCoroutine(MakeThirdPersonModelVisible());

        playerInventory.grenades = 2;

        //StartCoroutine(playerInventory.EquipStartingWeapon());
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
        Debug.Log("LeaveRoomWithDelay_Coroutine");
        yield return new WaitForSeconds(delay);


        int levelToLoad = 0;
        Debug.Log(PhotonNetwork.CurrentRoom.Name);

        if (PhotonNetwork.CurrentRoom.Name == Launcher.instance.quickMatchRoomName)
            levelToLoad = Launcher.instance.waitingRoomLevelIndex;
        else
        {
            Cursor.visible = true;
            PhotonNetwork.LeaveRoom();
        }
        PhotonNetwork.LoadLevel(levelToLoad);
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

        //playerInventory.holsteredWeapon = null;
        GetComponent<PlayerController>().DisableCrouch();
        //StopShieldAlarmSound();
        PlayDeathSound();
        GetComponent<PlayerUI>().scoreboard.CloseScoreboard();
        StartCoroutine(Respawn_Coroutine());
        StartCoroutine(MidRespawnAction());
        DropWeapon(playerInventory.activeWeapon);
        DropWeapon(playerInventory.holsteredWeapon, offset: new Vector3(0.5f, 0.5f, 0));
    }

    // https://stackoverflow.com/questions/30294216/unity3d-c-sharp-vector3-as-default-parameter
    public void DropWeapon(WeaponProperties weapon, Vector3? offset = null)
    {
        if (!GetComponent<PhotonView>().IsMine || weapon.currentAmmo <= 0)
            return;

        WeaponProperties wp = null;

        if (weapon.codeName == null)
            return;

        if (offset == null)
            offset = new Vector3(0, 0, 0);

        foreach (GameObject w in playerInventory.allWeaponsInInventory)
            if (w.GetComponent<WeaponProperties>().codeName == weapon.codeName)
                wp = w.GetComponent<WeaponProperties>();

        try
        {
            GameObject wo = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/Weapons", wp.weaponRessource.name), weaponDropPoint.position + (Vector3)offset, Quaternion.identity);
            wo.name = wo.name.Replace("(Clone)", "");
            wo.GetComponent<LootableWeapon>().ammoInThisWeapon = wp.currentAmmo;
            wo.GetComponent<LootableWeapon>().spareAmmo = wp.spareAmmo;
            wo.GetComponent<LootableWeapon>().ttl = 60;
            wo.GetComponent<Rigidbody>().AddForce(weaponDropPoint.transform.forward * 200);
            Debug.Log($"Spawned {wo.name}");

            wp.currentAmmo = 0;
            wp.spareAmmo = 0;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e);
        }
    }
}
