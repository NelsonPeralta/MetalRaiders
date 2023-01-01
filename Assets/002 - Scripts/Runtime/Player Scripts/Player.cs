using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.CompilerServices;
using System;
using System.Linq;

public class Player : MonoBehaviourPunCallbacks
{
    public delegate void PlayerEvent(Player playerProperties);
    public PlayerEvent OnPlayerDeath, OnPlayerHitPointsChanged, OnPlayerDamaged, OnPlayerHealthDamage,
        OnPlayerHealthRechargeStarted, OnPlayerShieldRechargeStarted, OnPlayerShieldDamaged, OnPlayerShieldBroken,
        OnPlayerRespawnEarly, OnPlayerRespawned, OnPlayerOvershieldPointsChanged, OnPlayerTeamChanged;


    // public variables
    #region
    public bool isInvincible { get { return _isInvincible; } }
    public int controllerId
    {
        get { return GetComponent<PlayerController>().rid; }
    }
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

    public float fullPoints { get { return _hitPoints + _overshieldPoints; } }
    public float hitPoints
    {
        get { return _hitPoints + _overshieldPoints; }

        set
        {
            float _previousValue = hitPoints;
            float _damage = _previousValue - value;

            if (_damage > 0)
            {

                Debug.Log(_previousValue);
                Debug.Log(value);
                Debug.Log(_damage);
            }

            if (_damage > 0 && (_isInvincible || hitPoints <= 0))
                return;

            if (overshieldPoints > 0)
            {
                float _originalOsPoints = overshieldPoints;
                overshieldPoints -= _damage;
                if (_damage > _originalOsPoints)
                {
                    _damage -= _originalOsPoints;
                    Debug.Log(_damage);
                }
                else
                    return;
            }

            float newValue = hitPoints - _damage;

            if (overshieldPoints <= 0)
                _hitPoints = Mathf.Clamp(newValue, 0, (_maxHealthPoints + _maxShieldPoints));

            if (_previousValue > newValue)
                OnPlayerDamaged?.Invoke(this);

            if (_previousValue != newValue)
                OnPlayerHitPointsChanged?.Invoke(this);

            if (_maxShieldPoints > 0)
            {
                if (newValue >= maxHealthPoints && newValue < _previousValue)
                {
                    OnPlayerShieldDamaged?.Invoke(this);
                }

                if (newValue <= maxHealthPoints && _previousValue > maxHealthPoints)
                    OnPlayerShieldBroken?.Invoke(this);
            }

            if (newValue < maxHealthPoints && _previousValue <= maxHealthPoints && _previousValue > newValue)
                OnPlayerHealthDamage?.Invoke(this);



            if (_hitPoints <= 0)
                isDead = true;

            impactPos = null;
        }
    }

    public float overshieldPoints
    {
        get { return _overshieldPoints; }
        private set
        {
            _overshieldPoints = Mathf.Clamp(value, 0, _maxOvershieldPoints);

            if (_overshieldPoints >= _maxOvershieldPoints)
            {
                _isInvincible = false;
                _overshieldRecharge = false;
            }

            if (_overshieldPoints <= 0)
                _overshieldFx.SetActive(false);

            OnPlayerOvershieldPointsChanged?.Invoke(this);
        }
    }

    public int maxHitPoints { get { return _maxHitPoints; } set { _maxHitPoints = value; } }

    public int maxHealthPoints
    {
        get { return _maxHealthPoints; }
        private set { _maxHealthPoints = value; }
    }
    public int maxOvershieldPoints
    {
        get { return _maxOvershieldPoints; }
        set
        {
            _maxOvershieldPoints = value;
            if (_maxOvershieldPoints > 0)
            {
                hitPoints = _maxHitPoints;
                _overshieldRecharge = true;
                _overshieldFx.SetActive(true);
                GetComponent<PlayerShield>().PlayShieldStartSound(this);
            }
        }
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
            foreach (PlayerHitbox ph in hitboxes)
            {
                if (!value)
                    ph.gameObject.layer = 31;
                else
                    ph.gameObject.layer = 7;
                ph.gameObject.SetActive(value);

                if (ph.GetComponent<BoxCollider>() != null)
                    ph.GetComponent<BoxCollider>().enabled = value;

                if (ph.GetComponent<SphereCollider>() != null)
                    ph.GetComponent<SphereCollider>().enabled = value;

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

    public Vector3? impactDir
    {
        protected set
        {
            try
            {
                _impactDir = (Vector3)value;
            }
            catch { }

            if (value == Vector3.zero)
                _deathByHeadshot = false;
        }
        get { return _impactDir; }
    }

    public bool isLocal
    {
        get { return PV.IsMine; }
    }

    public int pid
    {
        get { return PV.ViewID; }
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

    public bool isMine { get { return GetComponent<PhotonView>().IsMine; } }

    public int rid
    {
        get { return GetComponent<PlayerController>().rid; }
    }

    public PlayerMultiplayerMatchStats.Team team
    {
        get { return _team; }
        private set
        {
            if (isMine)
            {
                _team = value;
                OnPlayerTeamChanged?.Invoke(this);
                Debug.Log(_team);
                PV.RPC("UpdateTeam_RPC", RpcTarget.All, _team.ToString());

                name += $" {_team} team";
            }
        }
    }
    public int lastPID
    {
        get { return _lastPID; }
        set
        {
            _lastPID = value;

            try { lastPlayerSource = GameManager.GetPlayerWithPhotonViewId(_lastPID); } catch { _lastPID = 0; }
        }
    }

    public Player lastPlayerSource { get { return _lastPlayerSource; } private set { _lastPlayerSource = value; } }

    public Camera uiCamera { get { return _uiCamera; } }

    private NetworkPlayer _player { get { return _networkPlayer; } }

    #endregion

    // serialized variables
    #region

    [SerializeField] NetworkPlayer _networkPlayer;
    [SerializeField] PlayerMultiplayerMatchStats.Team _team;
    [SerializeField] PlayerMedals playerMedals;
    [SerializeField] string _nickName;
    [SerializeField] Player _lastPlayerSource;
    [SerializeField] int _lastPID;
    [SerializeField] int _maxHitPoints = 250;
    [SerializeField] int _maxHealthPoints = 100;
    [SerializeField] int _maxShieldPoints = 150;
    [SerializeField] int _maxOvershieldPoints = 150;
    [SerializeField] float _hitPoints = 250, _overshieldPoints = 150;
    [SerializeField] bool _isRespawning, _isDead, _isInvincible;
    [SerializeField] GameObject _overshieldFx;
    [SerializeField] Camera _uiCamera;
    [SerializeField] int _defaultRespawnTime = 4;
    [SerializeField] int _pushForce = 10;
    #endregion


    // private variables

    #region

    int _meleeDamage = 150;
    bool _isHealing;
    bool _overshieldRecharge;
    int _respawnTime = 5;

    int _defaultHealingCountdown = 4;

    float _healthHealingIncrement = (100 * 2);
    float _shieldHealingIncrement = (150 * 0.5f);

    bool _hasArmor;
    bool _hasMeleeUpgrade;

    bool _deathByHeadshot;
    Vector3 _impactPos;
    Vector3 _impactDir;



    #endregion

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
    public List<PlayerHitbox> hitboxes = new List<PlayerHitbox>();

    [Header("Player Voice")]
    public AudioSource playerVoice;
    public AudioClip sprintingClip;
    public AudioClip[] meleeClips;
    public AudioClip[] hurtClips;
    public AudioClip[] deathClips;

    public PhotonView PV;

    [Header("Player Info")]
    public bool needsHealthPack;

    // Private Variables

    [SerializeField] float _healingCountdown;
    [SerializeField] float _shieldRechargeCountdown;


    public GameObject bloodImpact;
    public Transform weaponDropPoint;

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

        hitboxes = GetComponentsInChildren<PlayerHitbox>().ToList();
        _networkPlayer = GetComponent<NetworkPlayer>();
    }
    private void Start()
    {
        lastPID = -1;
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


        //if (GetComponent<PlayerController>().PV.IsMine)
        //{

        //}
        //else
        //{
        //    firstPersonModels.layer = 23; // 24 = P1 FPS
        //    thirdPersonModels.layer = 0; // 0 = Default
        //}
        //StartCoroutine(SlightlyIncreaseHealth());

        //foreach (PlayerMarker pm in GetComponentsInChildren<PlayerMarker>())
        //    OnPlayerTeamChanged += pm.OnPlayerTeamDelegate;
        OnPlayerDeath += OnPlayerDeath_Delegate;
        OnPlayerDamaged += OnPlayerDamaged_Delegate;
        OnPlayerHealthDamage += OnPlayerHealthDamaged_Delegate;
        OnPlayerDeath += GetComponent<PlayerController>().OnDeath_Delegate;

        try { GameManager.instance.pid_player_Dict.Add(pid, this); } catch { }
        try
        {
            if (isMine)
            {
                Debug.Log($"Adding local player {controllerId}");
                GameManager.instance.localPlayers.Add(controllerId, this);
            }
        }
        catch { }
        try { team = GameManager.instance.onlineTeam; } catch { }
    }
    private void Update()
    {
        HitPointsRecharge();
        OvershieldPointsRecharge();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        float movementSpeedRatio = GetComponent<Movement>().playerSpeedPercent;
        Rigidbody rb = hit.collider.attachedRigidbody;
        if (rb && !rb.isKinematic)
        {
            rb.velocity = hit.moveDirection * _pushForce * movementSpeedRatio;
        }
    }

    void OvershieldPointsRecharge()
    {
        if (_overshieldRecharge && overshieldPoints < _maxOvershieldPoints)
        {
            hitPoints = _maxHitPoints;
            _isInvincible = true;
            overshieldPoints += (Time.deltaTime * _shieldHealingIncrement);
        }
    }


    // public functions
    #region

    public bool CanBeDamaged()
    {
        if (hitPoints <= 0 || isDead || isRespawning)
            return false;
        return true;
    }

    // Used to pass melee damage
    public void Damage(int damage,
         [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        if (_isInvincible)
            damage = 0;
        if (overshieldPoints > 0)
            damage -= (int)_overshieldPoints;

        Debug.Log("member name: " + memberName);
        Debug.Log("source file path: " + sourceFilePath);
        Debug.Log("source line number: " + sourceLineNumber);

        PV.RPC("Damage_RPC", RpcTarget.All, damage);
    }

    public void Damage(int damage, bool headshot, int source_pid,
        Vector3? impactPos = null, Vector3? impactDir = null, string damageSource = null,
        bool isGroin = false,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        {
            //Debug.Log("member name: " + memberName);
            //Debug.Log("source file path: " + sourceFilePath);
            //Debug.Log("source line number: " + sourceLineNumber);


            //Debug.Log(healthDamage);
            //Debug.Log(_isInvincible);
            //Debug.Log(overshieldPoints);

            //try
            //{ // Hit Marker Handling
            //    lastPID = playerWhoShotThisPlayerPhotonId;
            //    Player p = GameManager.GetPlayerWithPhotonViewId(playerWhoShotThisPlayerPhotonId);

            //    if (_isInvincible)
            //        healthDamage = 0;
            //    if (overshieldPoints > 0)
            //        healthDamage -= (int)_overshieldPoints;

            //    if (hitPoints <= healthDamage)
            //        p.GetComponent<PlayerUI>().SpawnHitMarker(PlayerUI.HitMarkerType.Kill);
            //    else
            //        p.GetComponent<PlayerUI>().SpawnHitMarker();
            //}
            //catch (System.Exception e) { Debug.LogWarning(e); }
        }
        Debug.Log(damage);

        PV.RPC("Damage_RPC", RpcTarget.All, damage, headshot, source_pid,
            impactPos, impactDir, damageSource,
            isGroin);
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
            wo.GetComponent<LootableWeapon>().ammo = wp.currentAmmo;
            wo.GetComponent<LootableWeapon>().spareAmmo = wp.spareAmmo;
            wo.GetComponent<Rigidbody>().AddForce(weaponDropPoint.transform.forward * 200);

            wp.currentAmmo = 0;
            wp.spareAmmo = 0;
        }
        catch (System.Exception e)
        {
#if UNITY_EDITOR
            Debug.LogWarning(e);
#endif
        }
    }

    public void PlayMeleeSound()
    {
        int randomSound = UnityEngine.Random.Range(0, meleeClips.Length);
        playerVoice.clip = meleeClips[randomSound];
        playerVoice.Play();
    }
    public void LeaveRoomWithDelay()
    {
        if (controllerId == 0)
            StartCoroutine(LeaveRoomWithDelay_Coroutine());
    }

    void SpawnRagdoll()
    {
        var ragdoll = FindObjectOfType<GameObjectPool>().SpawnPooledPlayerRagdoll();
        Debug.Log(ragdoll.name);
        ragdoll.transform.position = transform.position + new Vector3(0, -1, 0);
        ragdoll.transform.rotation = transform.rotation;
        ragdoll.SetActive(true);

        if (!_deathByHeadshot)
            ragdoll.GetComponent<RagdollPrefab>().ragdollHips.GetComponent<Rigidbody>().AddForce((Vector3)impactDir * 300);
        else
            ragdoll.GetComponent<RagdollPrefab>().ragdollHead.GetComponent<Rigidbody>().AddForce((Vector3)impactDir * 300);
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

    void Respawn()
    {
        if (!isRespawning)
            return;
        try { GetComponent<AllPlayerScripts>().scoreboardManager.CloseScoreboard(); } catch { }
        lastPID = -1;
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
        impactDir = Vector3.zero;
        OnPlayerRespawned?.Invoke(this);
    }

    public void PlaySprintingSound()
    {
        PV.RPC("PlaySprintingSound_RPC", RpcTarget.All);
    }
    public void StopPlayingPlayerVoice()
    {
        PV.RPC("StopPlayingPlayerVoice_RPC", RpcTarget.All);
    }

    public void Teleport(Vector3 v, Vector3 r)
    {
        PV.RPC("Teleport_RPC", RpcTarget.All, v, r);
    }

    #endregion

    // public coroutines
    #region
    public IEnumerator LeaveRoomWithDelay_Coroutine(int delay = 5)
    {
        yield return new WaitForSeconds(delay);


        int levelToLoad = 0;

        if (PhotonNetwork.CurrentRoom.Name == Launcher.instance.quickMatchRoomName)
            levelToLoad = Launcher.instance.waitingRoomLevelIndex;
        else
        {
            Cursor.visible = true;
            PhotonNetwork.LeaveRoom();
        }
        PhotonNetwork.LoadLevel(levelToLoad);
    }

    #endregion

    // private functions
    #region

    #endregion

    // private coroutines
    #region


    IEnumerator MidRespawnAction()
    {
        yield return new WaitForSeconds(_defaultRespawnTime * 0.7f);
        GetComponent<AllPlayerScripts>().scoreboardManager.OpenScoreboard();
        hitPoints = maxHitPoints;
        Transform spawnPoint = spawnManager.GetRandomSafeSpawnPoint(controllerId);
        transform.position = spawnPoint.position + new Vector3(0, 2, 0);
        transform.rotation = spawnPoint.rotation;
        isDead = false;
    }



    IEnumerator Respawn_Coroutine()
    {

        yield return new WaitForSeconds(_defaultRespawnTime);
        Respawn();
    }

    IEnumerator MakeThirdPersonModelVisible()
    {
        yield return new WaitForSeconds(0.1f);

        hitboxesEnabled = true;
    }

    #endregion

    // delegate functions
    #region

    void OnPlayerDeath_Delegate(Player playerProperties)
    {
        isRespawning = true;
        hitboxesEnabled = false;
        thirdPersonModels.SetActive(false);

        if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
            SwarmManager.instance.livesLeft--;

        GetComponent<PlayerController>().DisableCrouch();
        GetComponent<PlayerController>().isShooting = false;
        GetComponent<PlayerUI>().scoreboard.CloseScoreboard();
        gameObject.GetComponent<ScreenEffects>().orangeScreen.SetActive(false);

        try
        {
            mainCamera.gameObject.GetComponent<Transform>().transform.Rotate(30, 0, 0);
            mainCamera.gameObject.GetComponent<Transform>().transform.localPosition = new Vector3(mainOriginalCameraPosition.x, 2, -2.5f);
            gunCamera.enabled = false;
        }
        catch { }
        finally { gunCamera.enabled = false; }

        hitboxesEnabled = false;

        try { SpawnRagdoll(); } catch (System.Exception e) { Debug.Log(e); }
        try { StartCoroutine(Respawn_Coroutine()); } catch { }
        try { StartCoroutine(MidRespawnAction()); } catch { }

        DropWeapon(playerInventory.activeWeapon);
        DropWeapon(playerInventory.holsteredWeapon, offset: new Vector3(0.5f, 0.5f, 0));
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

    #endregion

    // rpc functions
    #region
    [PunRPC]
    void UpdateNickName_RPC(string nn)
    {
        _nickName = nn;
    }

    [PunRPC]
    void UpdateTeam_RPC(string nn)
    {
        if (!PV.IsMine)
        {
            Debug.Log(nn);
            _team = (PlayerMultiplayerMatchStats.Team)Enum.Parse(typeof(PlayerMultiplayerMatchStats.Team), nn);
            OnPlayerTeamChanged?.Invoke(this);
            name += $" {_team.ToString()} team";
        }
    }

    [PunRPC]
    void Damage_RPC(int damage)
    {
        if (hitPoints <= 0 || isRespawning || isDead)
            return;

        if (!isDead && !isRespawning)
            hitPoints -= damage;

        if (lastPID > -1)
            if (isDead)
            {
                if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
                    MultiplayerManager.instance.AddPlayerKill(new MultiplayerManager.AddPlayerKillStruct(lastPID, PV.ViewID, false));

                try
                {
                    Debug.Log($"Simple Damage_RPC");
                    Player sourcePlayer = GameManager.GetPlayerWithPhotonViewId(lastPID);
                    string feed = $"{sourcePlayer.nickName} killed {nickName}";
                    foreach (KillFeedManager kfm in FindObjectsOfType<KillFeedManager>())
                    {
                        if (sourcePlayer != this)
                            kfm.EnterNewFeed(feed);
                        else
                            kfm.EnterNewFeed($"<color=\"white\"> {nickName} committed suicide");
                    }
                }
                catch
                {
                    foreach (KillFeedManager kfm in FindObjectsOfType<KillFeedManager>())
                    {
                        kfm.EnterNewFeed($"Mistakes were made ({nickName})");
                    }
                }
                finally
                {
                    lastPID = -1;
                }
            }
    }

    public bool deathByHeadshot { get { return _deathByHeadshot; } }

    [PunRPC]
    void Damage_RPC(int damage, bool headshot, int sourcePid,
        Vector3? impactPos = null, Vector3? impactDir = null, string damageSource = null,
        bool isGroin = false)
    {
        if (hitPoints <= 0 || isRespawning || isDead)
            return;

        Debug.Log($"Damage_RPC: {damage}");
        Debug.Log(damage);
        Debug.Log(hitPoints);
        Debug.Log(damageSource);

        _deathByHeadshot = headshot;
        lastPID = sourcePid;
        try { this.impactPos = impactPos; this.impactDir = impactDir; } catch { }
        try { if (lastPlayerSource != this) lastPlayerSource.GetComponent<PlayerMultiplayerMatchStats>().damage += damage; } catch { }

        if (PV.IsMine)
        {
            GetComponent<PlayerController>().ScopeOut();
            allPlayerScripts.damageIndicatorManager.SpawnNewDamageIndicator(sourcePid);

            {
                try
                {
                    KillFeedManager killFeedManager = GetComponent<KillFeedManager>();
                    int damageSourceSpriteCode = KillFeedManager.killFeedWeaponCodeDict[damageSource];

                    if (damageSource.Contains("grenade"))
                    {
                        string colorCode = KillFeedManager.killFeedColorCodeDict["orange"];
                        killFeedManager.EnterNewFeed($"You took {damage} <color={colorCode}>grenade damage");
                    }
                    //else if (damageSource.Contains("melee"))
                    //{
                    //    string colorCode = KillFeedManager.killFeedColorCodeDict["yellow"];
                    //    killFeedManager.EnterNewFeed($"You took {meleeDamage} melee  damage");
                    //}
                }
                catch { }
            }
        }

        if (isDead)
        {
            //Player sourcePlayer = GameManager.GetPlayerWithPhotonViewId(playerWhoShotThisPlayerPhotonId);
            //string sourcePlayerName = sourcePlayer.nickName;

            //int hsCode = KillFeedManager.killFeedSpecialCodeDict["headshot"];
            //int nsCode = KillFeedManager.killFeedSpecialCodeDict["nutshot"];
            //string youColorCode = KillFeedManager.killFeedColorCodeDict["blue"];
            //string weaponColorCode = playerInventory.activeWeapon.ammoType.ToString().ToLower();

            //foreach (KillFeedManager kfm in FindObjectsOfType<KillFeedManager>())
            //{
            //    string f = $"{sourcePlayer.nickName} killed {nickName}";
            //    kfm.EnterNewFeed(f);

            //    continue;
            //    if (this != sourcePlayer)
            //    {

            //        string feed = $"{sourcePlayer.nickName} killed";
            //        if (kfm.GetComponent<Player>() != this)
            //        {
            //            if (kfm.GetComponent<Player>().nickName == sourcePlayerName)
            //            {
            //                try
            //                {
            //                    int damageSourceSpriteCode = KillFeedManager.killFeedWeaponCodeDict[damageSource];
            //                    feed = $"<color={youColorCode}>You <color=\"white\"><sprite={damageSourceSpriteCode}>";

            //                    if (wasHeadshot)
            //                        feed += $"<sprite={hsCode}>";

            //                    if (isGroin)
            //                        feed += $"<sprite={nsCode}>";

            //                    feed += $" <color=\"red\">{nickName}";
            //                    kfm.EnterNewFeed(feed);
            //                }
            //                catch
            //                {
            //                    kfm.EnterNewFeed($"<color={youColorCode}>You <color=\"white\"> killed {sourcePlayerName}");
            //                }
            //            }
            //            else
            //            {
            //                try
            //                {
            //                    int damageSourceSpriteCode = KillFeedManager.killFeedWeaponCodeDict[damageSource];
            //                    feed = $"<color=\"red\">{sourcePlayerName} <color=\"white\"><sprite={damageSourceSpriteCode}>";

            //                    if (wasHeadshot)
            //                        feed += $"<sprite={hsCode}>";

            //                    feed += $" <color=\"red\">{nickName}";
            //                    kfm.EnterNewFeed(feed);
            //                }
            //                catch
            //                {
            //                    kfm.EnterNewFeed($"<color=\"red\">{sourcePlayerName} <color=\"white\">killed <color=\"red\">{nickName}");
            //                }
            //            }
            //        }
            //        else
            //        {
            //            try
            //            {
            //                int damageSourceSpriteCode = KillFeedManager.killFeedWeaponCodeDict[damageSource];
            //                feed = $"<color=\"red\">{sourcePlayerName} <color=\"white\"><sprite={damageSourceSpriteCode}>";

            //                if (wasHeadshot)
            //                    feed += $"<sprite={hsCode}>";

            //                feed += $" <color={youColorCode}>You";
            //                kfm.EnterNewFeed(feed);
            //            }
            //            catch
            //            {
            //                kfm.EnterNewFeed($"<color=\"red\">{sourcePlayerName} <color=\"white\"> killed <color={youColorCode}>You");
            //            }
            //        }
            //    }
            //    else
            //    {
            //        kfm.EnterNewFeed($"<color=\"white\"> {nickName} committed suicide");
            //    }
            //}

            //if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
            //{
            //    MultiplayerManager.instance.AddPlayerKill(new MultiplayerManager.AddPlayerKillStruct(playerWhoShotThisPlayerPhotonId, PV.ViewID, wasHeadshot));

            //}
            //else if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
            //    GetComponent<PlayerSwarmMatchStats>().deaths++;

            //PlayerMedals sourcePlayerMedals = null;
            //foreach (PlayerMedals pm in FindObjectsOfType<PlayerMedals>())
            //    if (pm.player.pid == playerWhoShotThisPlayerPhotonId)
            //        sourcePlayerMedals = pm;

            //if (wasHeadshot)
            //{
            //    if (playerWhoShotThisPlayerPhotonId != this.pid)
            //        sourcePlayerMedals.SpawnHeadshotMedal();
            //}
            //else if (isGroin)
            //{
            //    if (playerWhoShotThisPlayerPhotonId != this.pid)
            //        sourcePlayerMedals.SpawnNutshotMedal();
            //}
            //else if (damageSource == "melee")
            //{
            //    if (playerWhoShotThisPlayerPhotonId != this.pid)
            //        sourcePlayerMedals.SpawnMeleeMedal();
            //}
            //else if (damageSource.Contains("grenade"))
            //{
            //    if (playerWhoShotThisPlayerPhotonId != this.pid)
            //        sourcePlayerMedals.SpawnGrenadeMedal();
            //}
            //else
            //{
            //    if (playerWhoShotThisPlayerPhotonId != this.pid)
            //        sourcePlayerMedals.kills++;
            //}

            //if (playerMedals.spree >= 3)
            //    if (playerWhoShotThisPlayerPhotonId != this.pid)
            //        sourcePlayerMedals.SpawnKilljoySpreeMedal();
        }


        hitPoints -= damage;
        //UpdateData();

        try
        { // Hit Marker Handling

            if (isDead)
                lastPlayerSource.GetComponent<PlayerUI>().SpawnHitMarker(PlayerUI.HitMarkerType.Kill);
            else
                lastPlayerSource.GetComponent<PlayerUI>().SpawnHitMarker();
        }
        catch { }

        if (isDead)
        {
            string sourcePlayerName = lastPlayerSource.nickName;

            int hsCode = KillFeedManager.killFeedSpecialCodeDict["headshot"];
            int nsCode = KillFeedManager.killFeedSpecialCodeDict["nutshot"];
            string youColorCode = KillFeedManager.killFeedColorCodeDict["blue"];
            string weaponColorCode = playerInventory.activeWeapon.ammoType.ToString().ToLower();




            foreach (KillFeedManager kfm in FindObjectsOfType<KillFeedManager>())
            {
                string f = $"{lastPlayerSource.nickName} killed {nickName}";

                if(damageSource != null)
                {
                    f += $" with an {damageSource}";
                }

                {
                    if (this != lastPlayerSource)
                    {
                        kfm.EnterNewFeed(f);
                        continue;

                        {
                            string feed = $"{lastPlayerSource.nickName} killed";
                            if (kfm.GetComponent<Player>() != this)
                            {
                                if (kfm.GetComponent<Player>().nickName == sourcePlayerName)
                                {
                                    try
                                    {
                                        int damageSourceSpriteCode = KillFeedManager.killFeedWeaponCodeDict[damageSource];
                                        feed = $"<color={youColorCode}>You <color=\"white\"><sprite={damageSourceSpriteCode}>";

                                        if (headshot)
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

                                        if (headshot)
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

                                    if (headshot)
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
                    }
                    else
                    {
                        kfm.EnterNewFeed($"<color=\"white\"> {nickName} committed suicide");
                    }
                }
            }

            if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
            {
                MultiplayerManager.instance.AddPlayerKill(new MultiplayerManager.AddPlayerKillStruct(sourcePid, PV.ViewID, headshot));

            }
            else if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
                GetComponent<PlayerSwarmMatchStats>().deaths++;

            PlayerMedals sourcePlayerMedals = lastPlayerSource.playerMedals;
            if (sourcePlayerMedals != this.playerMedals)
            {
                if (headshot)
                {
                    if (sourcePid != this.pid)
                        sourcePlayerMedals.SpawnHeadshotMedal();
                }
                else if (isGroin)
                {
                    if (sourcePid != this.pid)
                        sourcePlayerMedals.SpawnNutshotMedal();
                }
                else if (damageSource == "melee")
                {
                    if (sourcePid != this.pid)
                        sourcePlayerMedals.SpawnMeleeMedal();
                }
                else if (damageSource.Contains("grenade"))
                {
                    if (sourcePid != this.pid)
                        sourcePlayerMedals.SpawnGrenadeMedal();
                }
                else
                {
                    if (sourcePid != this.pid)
                        sourcePlayerMedals.kills++;
                }

                if (playerMedals.spree >= 3)
                    if (sourcePid != this.pid)
                        sourcePlayerMedals.SpawnKilljoySpreeMedal();
            }
        }
    }

    void UpdateData()
    {
        if (isMine)
            PV.RPC("UpdateData_RPC", RpcTarget.All, hitPoints, overshieldPoints);
    }

    [PunRPC]
    void UpdateData_RPC(int h, int o)
    {
        if (!isMine)
        {
            overshieldPoints = o;
            hitPoints = h;
        }
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


    [PunRPC]
    public void StopPlayingPlayerVoice_RPC()
    {
        playerVoice.loop = false;
        playerVoice.volume = 0.5f;
        playerVoice.Stop();
    }

    [PunRPC]
    public void UpdateAmmo(int wIndex, int ammo, bool isSpare = false, bool sender = true)
    {
        Debug.Log("UpdateAmmo");
        try
        {
            playerInventory.allWeaponsInInventory[wIndex].GetComponent<WeaponProperties>().UpdateAmmo(wIndex, ammo, isSpare, false);
        }
        catch { }
    }

    [PunRPC]
    void Teleport_RPC(Vector3 t, Vector3 r)
    {
        Debug.Log("Teleport_RPC");
        Debug.Log(t);
        GetComponent<CharacterController>().enabled = false;
        transform.position = t;
        GetComponent<CharacterController>().enabled = true;
        transform.eulerAngles = r;
    }
    #endregion
}
