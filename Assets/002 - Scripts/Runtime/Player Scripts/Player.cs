using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.CompilerServices;
using System;
using System.Linq;
using static UnityEngine.ProBuilder.AutoUnwrapSettings;
using System.Text;
using System.Net.Mail;
using TMPro;

public class Player : Biped
{
    public delegate void PlayerEvent(Player playerProperties);
    public PlayerEvent OnPlayerDeath, OnPlayerDeathLate, OnPlayerHitPointsChanged, OnPlayerDamaged, OnPlayerHealthDamage,
        OnPlayerHealthRechargeStarted, OnPlayerShieldRechargeStarted, OnPlayerShieldDamaged, OnPlayerShieldBroken,
        OnPlayerRespawnEarly, OnPlayerRespawned, OnPlayerOvershieldPointsChanged, OnPlayerTeamChanged;

    public enum DeathNature { None, Headshot, Groin, Melee, Grenade, Stuck, Sniped }

    // public variables
    #region
    public bool isInvincible { get { return _isInvincible; } set { _isInvincible = value; } }
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
                maxShieldPoints = 150;
                maxHealthPoints = 100;
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

            if (_damage > 0 && (_isInvincible || hitPoints <= 0))
                return;

            if (overshieldPoints > 0)
            {
                float _originalOsPoints = overshieldPoints;
                overshieldPoints -= _damage;
                if (_damage > _originalOsPoints)
                {
                    _damage -= _originalOsPoints;
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
            {
                if (isMine)
                {
                    //PV.RPC("IsDead_RPC", RpcTarget.All);
                    //PV.RPC("SendHitPointsCheck_RPC", RpcTarget.All, (int)hitPoints, isMine, GameTime.instance.totalTime);
                }
                isDead = true;

            }

            impactPos = null;
        }


        //get { return networkHitPoints; }

        //set
        //{
        //    if (isMine)
        //    {
        //        float _previousValue = hitPoints;
        //        float _damage = _previousValue - value;

        //        if (_damage > 0)
        //        {
        //            Debug.Log(_previousValue);
        //            Debug.Log(value);
        //            Debug.Log(_damage);
        //        }

        //        if (_damage > 0 && (_isInvincible || hitPoints <= 0))
        //            return;

        //        if (overshieldPoints > 0)
        //        {
        //            float _originalOsPoints = overshieldPoints;
        //            overshieldPoints -= _damage;
        //            if (_damage > _originalOsPoints)
        //            {
        //                _damage -= _originalOsPoints;
        //                Debug.Log(_damage);
        //            }
        //            else
        //                return;
        //        }

        //        float newHitPoints = hitPoints - _damage;
        //        if (_isHealing)
        //            if (overshieldPoints <= 0)
        //            {
        //                _hitPoints = Mathf.Clamp(newHitPoints, 0, (_maxHealthPoints + _maxShieldPoints));
        //                if (_previousValue != newHitPoints)
        //                    OnPlayerHitPointsChanged?.Invoke(this);
        //            }

        //        if (!_isHealing)
        //            GetComponent<PhotonView>().RPC("UpdateHitPoints_RPC", RpcTarget.All, newHitPoints, _overshieldPoints);
        //        else if (hitPoints == maxHitPoints)
        //            GetComponent<PhotonView>().RPC("UpdateHitPoints_RPC", RpcTarget.All, newHitPoints, _overshieldPoints);
        //    }
        //}
    }

    private float networkHitPoints
    {
        get { return _hitPoints + _overshieldPoints; }
        set
        {
            {
                float _previousValue = hitPoints;
                float newHitPoints = value;

                if (overshieldPoints <= 0)
                    _hitPoints = Mathf.Clamp(newHitPoints, 0, (_maxHealthPoints + _maxShieldPoints));

                if (_previousValue > newHitPoints)
                    OnPlayerDamaged?.Invoke(this);

                if (_previousValue != newHitPoints)
                    OnPlayerHitPointsChanged?.Invoke(this);

                if (_maxShieldPoints > 0)
                {
                    if (newHitPoints >= maxHealthPoints && newHitPoints < _previousValue)
                    {
                        OnPlayerShieldDamaged?.Invoke(this);
                    }

                    if (newHitPoints <= maxHealthPoints && _previousValue > maxHealthPoints)
                        OnPlayerShieldBroken?.Invoke(this);
                }

                if (newHitPoints < maxHealthPoints && _previousValue <= maxHealthPoints && _previousValue > newHitPoints)
                    OnPlayerHealthDamage?.Invoke(this);

                if (_hitPoints <= 0)
                    isDead = true;

                impactPos = null;
            }
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
        set { _maxHealthPoints = value; }
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
        set { _maxShieldPoints = value; }
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
            Debug.Log("Is Dead: " + _isDead);

            if (value && !previousValue)
            {
                OnPlayerDeath?.Invoke(this);
                OnPlayerDeathLate?.Invoke(this);
            }
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
                Debug.Log(_impactDir);
            }
            catch { }

            //if (value == Vector3.zero)
            //    deathByHeadshot = false;
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

            _playerArmorManager.HardReloadArmor();
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
    public Announcer announcer { get { return _announcer; } }
    public DeathNature deathNature { get { return _deathNature; } private set { _deathNature = value; } }
    public PlayerMedals playerMedals { get { return _playerMedals; } }
    public float defaultVerticalFov
    {
        get { return _defaultVerticalFov; }
        private set
        {

            //60  91.49
            //40  65.81
            //20  34.81

            //90  31.42
            //65.81   20.63
            //34.81   10.08



            //90  58.72
            //60  35.98
            //30  17.14

            //90  31.42
            //60  18.45
            //30  8.62

            Debug.Log($"localPlayers.Keys.Count: {GameManager.instance.localPlayers.Keys.Count}");
            if (GameManager.instance.nbLocalPlayersPreset % 2 == 0) _defaultVerticalFov = 31.42f;
            else if (GameManager.instance.nbLocalPlayersPreset == 1) _defaultVerticalFov = 58.72f;


            GetComponent<PlayerController>().mainCam.fieldOfView = defaultVerticalFov;
            GetComponent<PlayerController>().uiCam.fieldOfView = defaultVerticalFov;
            GetComponent<PlayerController>().gunCam.fieldOfView = 60;
        }
    }

    public PlayerArmorManager playerArmorManager { get { return _playerArmorManager; } }
    public PlayerThirdPersonModel playerThirdPersonModel { get { return _playerThirdPersonModel; } }
    public PlayerShield playerShield { get { return _playerShield; } }
    public KillFeedManager killFeedManager { get { return _killFeedManager; } }

    #endregion

    // serialized variables
    #region

    [SerializeField] NetworkPlayer _networkPlayer;
    [SerializeField] PlayerMultiplayerMatchStats.Team _team;
    [SerializeField] PlayerMedals _playerMedals;
    [SerializeField] string _nickName;
    [SerializeField] Player _lastPlayerSource;
    [SerializeField] DeathNature _deathNature;
    [SerializeField] int _lastPID;
    [SerializeField] int _maxHitPoints = 250;
    [SerializeField] int _maxHealthPoints = 100;
    [SerializeField] int _maxShieldPoints = 150;
    [SerializeField] int _maxOvershieldPoints = 150;
    [SerializeField] float _networkHitPoints = 250, _hitPoints = 250, _overshieldPoints = 150;
    [SerializeField] bool _isRespawning, _isDead, _isInvincible;
    [SerializeField] GameObject _overshieldFx;
    [SerializeField] Camera _uiCamera;
    [SerializeField] int _defaultRespawnTime = 4;
    [SerializeField] int _pushForce = 7;
    [SerializeField] Announcer _announcer;
    [SerializeField] PlayerArmorManager _playerArmorManager;
    [SerializeField] PlayerThirdPersonModel _playerThirdPersonModel;
    [SerializeField] PlayerShield _playerShield;
    [SerializeField] KillFeedManager _killFeedManager;
    #endregion


    // private variables

    #region

    int _meleeDamage = 125;
    bool _isHealing;
    bool _overshieldRecharge;
    int _respawnTime = 5;

    int _defaultHealingCountdown = 4;

    float _healthHealingIncrement = (100 * 2);
    float _shieldHealingIncrement = (150 * 0.5f);

    bool _hasArmor;
    bool _hasMeleeUpgrade;

    string _damageSource;
    Vector3 _impactPos;
    Vector3 _impactDir;
    float _gameStartDelay;

    private bool deathByHeadshot { get { if (deathNature == DeathNature.Headshot || deathNature == DeathNature.Sniped) return true; else return false; } }
    private bool deathByGroin { get { if (deathNature == DeathNature.Groin) return true; else return false; } }

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
    public PlayerController playerController;
    public CrosshairManager cScript;
    public AimAssist aimAssist;
    public PlayerSurroundings playerSurroundings;
    public Movement movement;

    [Header("Camera Options")]
    [Tooltip("Default value for camera field of view (40 is recommended).")]
    [SerializeField] float _defaultVerticalFov;


    [Header("Cameras")]
    public Camera mainCamera;
    public Camera gunCamera;
    public Camera deathCamera;
    public PlayerCamera playerCamera;

    public Vector3 mainOriginalCameraPosition;

    [Header("Hitboxes")]
    public List<PlayerHitbox> hitboxes = new List<PlayerHitbox>();

    [Header("Player Voice")]
    public AudioListener audioListener;
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

    [SerializeField] GameObject headhunterSkullPrefab;


    private void Awake()
    {
        if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
        {
            hasArmor = true;

            if (GameManager.instance.gameType == GameManager.GameType.Swat
                || GameManager.instance.gameType == GameManager.GameType.Retro)
            {
                hasArmor = false;

                _overshieldPoints = 0;
                _maxShieldPoints = 0;

                _maxHitPoints = 100;
                _networkHitPoints = maxHitPoints;
                _hitPoints = maxHitPoints;
            }
        }
        if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
        {
            _defaultRespawnTime = 7;
            _overshieldPoints = 0;
            _maxShieldPoints = 0;
            _maxHitPoints = _maxHealthPoints = 250;
            _networkHitPoints = maxHitPoints;
            _hitPoints = maxHitPoints;
            needsHealthPack = true;
        }

        hitboxes = GetComponentsInChildren<PlayerHitbox>().ToList();
        _networkPlayer = GetComponent<NetworkPlayer>();
    }
    private void Start()
    {
        lastPID = -1;
        spawnManager = SpawnManager.spawnManagerInstance;
        gameObjectPool = GameObjectPool.instance;
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
        OnPlayerDeathLate += OnPlayerDeath_DelegateLate;
        OnPlayerDamaged += OnPlayerDamaged_Delegate;
        OnPlayerHealthDamage += OnPlayerHealthDamaged_Delegate;
        OnPlayerDeath += GetComponent<PlayerController>().OnDeath_Delegate;



        try
        {
            if (isMine)
            {
                Dictionary<int, Player> t = new Dictionary<int, Player>(GameManager.instance.localPlayers);
                if (!t.ContainsKey(controllerId))
                    t.Add(controllerId, this);
                GameManager.instance.localPlayers = t;
            }
        }
        catch { }

        defaultVerticalFov = 0; GetComponent<PlayerController>().ScopeOut();

        try
        {
            Dictionary<int, Player> t = new Dictionary<int, Player>(GameManager.instance.pid_player_Dict);
            if (!t.ContainsKey(pid))
                t.Add(pid, this);
            GameManager.instance.pid_player_Dict = t;
            //CurrentRoomManager.instance.nbPlayersJoined++;
        }
        catch { }

        try
        {
            if (isMine)
                NetworkGameManager.instance.AddPlayerJoinedCount();
        }
        catch { }
        try { team = GameManager.instance.onlineTeam; } catch { }


        // Bug
        mainCamera.enabled = false;
        gunCamera.enabled = false;
        uiCamera.enabled = false;

        _playerArmorManager.DisableAllArmor();
    }
    private void Update()
    {
        HitPointsRecharge();
        OvershieldPointsRecharge();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //Debug.Log("Player OnControllerColliderHit");
        float movementSpeedRatio = GetComponent<Movement>().speedRatio;
        Rigidbody rb = hit.collider.attachedRigidbody;
        if (rb && !rb.isKinematic)
        {
            rb.velocity = hit.moveDirection * _pushForce * movementSpeedRatio;
        }
    }

    void OnAllPlayersJoinedRoom_Delegate(CurrentRoomManager gme)
    {
        Debug.Log("OnAllPlayersJoinedRoom_Delegate");
        _gameStartDelay = GameManager.GameStartDelay * 0.99f;
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

    public void TriggerGameStartBehaviour()
    {
        Debug.Log("TriggerGameStartBehaviour");
        if (rid == 0)
            audioListener.enabled = true;

        mainCamera.enabled = false;
        mainCamera.enabled = true;

        gunCamera.enabled = false;
        gunCamera.enabled = true;

        uiCamera.enabled = false;
        uiCamera.enabled = true;

        GetComponent<Movement>().playerMotionTracker.minimapCamera.enabled = false;
        GetComponent<Movement>().playerMotionTracker.minimapCamera.enabled = true;
    }
    public bool CanBeDamaged()
    {
        if (hitPoints <= 0 || isDead || isRespawning)
            return false;
        return true;
    }

    // Used to pass melee damage
    public void BasicDamage(int damage,
         [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        if (_isInvincible)
            damage = 0;
        if (overshieldPoints > 0)
            damage -= (int)_overshieldPoints;

        int newHealth = (int)hitPoints - damage;
        PV.RPC("BasicDamage_RPC", RpcTarget.All, newHealth, damage);
    }

    public void Damage(int damage, bool headshot, int source_pid,
        Vector3? impactPos = null, Vector3? impactDir = null, string damageSource = null,
        bool isGroin = false,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        {
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

        //PV.RPC("Damage_RPC", RpcTarget.All, damage, headshot, source_pid,
        //    impactPos, impactDir, damageSource,
        //    isGroin);


        try { this.impactPos = impactPos; this.impactDir = impactDir; } catch { }

        if ((GameManager.instance.pid_player_Dict.ContainsKey(source_pid) && GameManager.GetPlayerWithPhotonViewId(source_pid).isMine) ||
            PhotonView.Find(source_pid).GetComponent<Actor>())
        {
            DeathNature dsn = DeathNature.None;
            if (headshot)
                dsn = DeathNature.Headshot;
            if (headshot && GameManager.GetPlayerWithPhotonViewId(source_pid).playerInventory.activeWeapon.weaponType == WeaponProperties.WeaponType.Sniper)
                dsn = DeathNature.Sniped;
            if (isGroin)
                dsn = DeathNature.Groin;


            byte[] bytes = Encoding.UTF8.GetBytes("");

            if (damageSource != null)
            {
                if (damageSource.Contains("renade"))
                    dsn = DeathNature.Grenade;

                if (damageSource.Contains("tuck"))
                    dsn = DeathNature.Stuck;

                if (damageSource.Contains("elee"))
                    dsn = DeathNature.Melee;
                bytes = Encoding.UTF8.GetBytes(damageSource);
            }


            int newHealth = (int)hitPoints - damage;
            PV.RPC("Damage_RPC", RpcTarget.All, newHealth, damage, source_pid, bytes, (int)dsn, impactDir);
        }
    }

    // https://stackoverflow.com/questions/30294216/unity3d-c-sharp-vector3-as-default-parameter
    public void DropWeaponOnDeath(WeaponProperties firstWeapon, WeaponProperties secondWeapon, Vector3 secondWeaponOffset)
    {
        if (!GetComponent<PhotonView>().IsMine || GameManager.instance.gameType == GameManager.GameType.GunGame)
            return;

        NetworkGameManager.SpawnNetworkWeaponOnPlayerDeath(firstWeapon, secondWeapon,
           weaponDropPoint.position, weaponDropPoint.transform.forward, weaponDropPoint.position + new Vector3(0, 1, 0));

        //GC.Collect(); // LAG SPIKE
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
        Debug.Log($"Spawning Player Ragdoll");
        print(_deathNature);
        var ragdoll = RagdollPool.instance.SpawnPooledPlayerRagdoll();
        ragdoll.transform.position = transform.position + new Vector3(0, -1, 0);
        ragdoll.transform.rotation = transform.rotation;
        ragdoll.GetComponent<PlayerArmorManager>().player = this;

        ragdoll.GetComponent<PlayerRagdoll>().SetPlayerCamera(playerCamera, mainCamera);

        if (!hasArmor)
        {
            ragdoll.GetComponent<PlayerArmorManager>().armorDataString = "";

        }
        else
        {
            Debug.Log($"Player {name} has color palette: {playerArmorManager.colorPalette}");
            ragdoll.GetComponent<PlayerArmorManager>().armorDataString = playerArmorManager.armorDataString;
            ragdoll.GetComponent<PlayerArmorManager>().colorPalette = playerArmorManager.colorPalette;
        }
        ragdoll.SetActive(true);


        if (deathByHeadshot) { ragdoll.GetComponent<PlayerRagdoll>().head.GetComponent<Rigidbody>().AddForce((Vector3)impactDir * 350); }
        else if (deathNature == DeathNature.Grenade) { ragdoll.GetComponent<PlayerRagdoll>().hips.GetComponent<Rigidbody>().AddForce((Vector3)impactDir * 100); }
        else if (deathNature == DeathNature.Melee) { ragdoll.GetComponent<PlayerRagdoll>().hips.GetComponent<Rigidbody>().AddForce((Vector3)impactDir * 2500); }
        else if (!deathByHeadshot) { ragdoll.GetComponent<PlayerRagdoll>().hips.GetComponent<Rigidbody>().AddForce((Vector3)impactDir * 350); }
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
        deathNature = DeathNature.None;
        _damageSource = null;
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
        if (GameManager.instance.gameType == GameManager.GameType.Swat
                || GameManager.instance.gameType == GameManager.GameType.Retro)
            playerInventory.grenades = 1;

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

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PhotonNetwork.LeaveRoom();
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
        playerInventory.transform.localRotation = Quaternion.Euler(0, 0, 0f);
        if (GameManager.instance.gameMode == GameManager.GameMode.Swarm) hasArmor = false;


        if (isDead)
        {
            try
            {
                // Spawn Skull
                //GameObject s = Instantiate(headhunterSkullPrefab, weaponDropPoint.position, Quaternion.identity);
                //s.GetComponent<Rigidbody>().AddForce(new Vector3(0, 800, 0));
            }
            catch { }
            try
            {
                string sourcePlayerName = lastPlayerSource.nickName;

                int hsCode = KillFeedManager.killFeedSpecialCodeDict["headshot"];
                int nsCode = KillFeedManager.killFeedSpecialCodeDict["nutshot"];
                string youColorCode = GameManager.colorDict["blue"];
                string weaponColorCode = playerInventory.activeWeapon.ammoType.ToString().ToLower();


                if (GameManager.instance.gameType == GameManager.GameType.GunGame
                        && deathNature == DeathNature.Melee)
                {
                    if (isMine)
                        playerInventory.playerGunGameManager.index--;
                }

                foreach (KillFeedManager kfm in FindObjectsOfType<KillFeedManager>())
                {
                    string f = $"{lastPlayerSource.nickName} killed {nickName}";


                    if (_damageSource != null && _damageSource != "")
                    {
                        f = $"{lastPlayerSource.nickName} [ {_damageSource} ] {nickName}";
                    }

                    if (GameManager.instance.gameType == GameManager.GameType.GunGame
                        && deathNature == DeathNature.Melee)
                    {

                        f = $"{lastPlayerSource.nickName} <color=\"red\"> Humiliated </color> {nickName}";
                    }
                    else if (deathNature == DeathNature.Sniped)
                        f = $"{lastPlayerSource.nickName} <color=\"yellow\">!!! Sniped !!!</color> {nickName}";
                    else if (deathByHeadshot)
                        f += $" with a <color=\"red\">Headshot</color>!";
                    else if (deathByGroin)
                        f += $" with a <color=\"yellow\">!!! Nutshot !!!</color>!";

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
                                            int damageSourceSpriteCode = KillFeedManager.killFeedWeaponCodeDict[_damageSource];
                                            feed = $"<color={youColorCode}>You <color=\"white\"><sprite={damageSourceSpriteCode}>";

                                            if (deathByHeadshot)
                                                feed += $"<sprite={hsCode}>";

                                            if (deathByGroin)
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
                                            int damageSourceSpriteCode = KillFeedManager.killFeedWeaponCodeDict[_damageSource];
                                            feed = $"<color=\"red\">{sourcePlayerName} <color=\"white\"><sprite={damageSourceSpriteCode}>";

                                            if (deathByHeadshot)
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
                                        int damageSourceSpriteCode = KillFeedManager.killFeedWeaponCodeDict[_damageSource];
                                        feed = $"<color=\"red\">{sourcePlayerName} <color=\"white\"><sprite={damageSourceSpriteCode}>";

                                        if (deathByHeadshot)
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
            }
            catch (Exception e) { }

            try
            {
                if (isMine)
                {
                    Debug.Log("AddPlayerKill_RPC");
                    Debug.Log(_damageSource);
                    PV.RPC("AddPlayerKill_RPC", RpcTarget.AllViaServer, _lastPID, PV.ViewID, (int)_deathNature, _damageSource);

                    //if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
                    //    MultiplayerManager.instance.AddPlayerKill(new MultiplayerManager.AddPlayerKillStruct(_lastPID, PV.ViewID, _deathNature));
                    //else if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
                    //    GetComponent<PlayerSwarmMatchStats>().deaths++;
                }
                if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
                    GetComponent<PlayerSwarmMatchStats>().deaths++;
            }
            catch (Exception e) { }

            try
            {
                PlayerMedals sourcePlayerMedals = lastPlayerSource._playerMedals;
                if (sourcePlayerMedals != this._playerMedals)
                {
                    if (deathNature == DeathNature.Sniped)
                    {
                        if (_lastPID != this.pid)
                            sourcePlayerMedals.SpawnSniperHeadshotMedal();
                    }
                    else if (deathNature == DeathNature.Headshot)
                    {
                        if (_lastPID != this.pid)
                            sourcePlayerMedals.SpawnHeadshotMedal();
                    }
                    else if (deathByGroin)
                    {
                        if (_lastPID != this.pid)
                            sourcePlayerMedals.SpawnNutshotMedal();
                    }
                    else if (deathNature == DeathNature.Melee)
                    {
                        if (_lastPID != this.pid)
                            sourcePlayerMedals.SpawnMeleeMedal();
                    }
                    else if (deathNature == DeathNature.Grenade)
                    {
                        if (_lastPID != this.pid)
                            sourcePlayerMedals.SpawnGrenadeMedal();
                    }
                    else if (deathNature == DeathNature.Stuck)
                    {
                        if (_lastPID != this.pid)
                            sourcePlayerMedals.SpawnStuckKillMedal();
                    }
                    else
                    {
                        if (_lastPID != this.pid)
                            sourcePlayerMedals.kills++;
                    }

                    if (_playerMedals.spree >= 5)
                        if (_lastPID != this.pid)
                            sourcePlayerMedals.SpawnKilljoySpreeMedal();
                }
            }
            catch { }
        }


        isRespawning = true;
        hitboxesEnabled = false;
        thirdPersonModels.SetActive(false);

        if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
            SwarmManager.instance.livesLeft--;

        GetComponent<PlayerController>().DisableCrouch();
        GetComponent<PlayerController>().isHoldingShootBtn = false;
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

        SpawnRagdoll();
        try { StartCoroutine(Respawn_Coroutine()); } catch { }
        try { StartCoroutine(MidRespawnAction()); } catch { }


    }
    void OnPlayerDeath_DelegateLate(Player playerProperties)
    {
        DropWeaponOnDeath(playerInventory.activeWeapon, playerInventory.holsteredWeapon, secondWeaponOffset: new Vector3(0.5f, 0.5f, 0));
    }


    void OnPlayerDamaged_Delegate(Player player)
    {
        try { GetComponent<PlayerController>().ScopeOut(); } catch { }

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
    void BasicDamage_RPC(int newHealth, int damage)
    {
        if (hitPoints <= 0 || isRespawning || isDead)
            return;

        if (!isDead && !isRespawning)
        {
            _impactPos = transform.position;
            hitPoints = newHealth;
        }

        //if (lastPID > -1)
        //    if (isDead)
        //    {
        //        if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
        //        {
        //            MultiplayerManager.instance.AddPlayerKill(new MultiplayerManager.AddPlayerKillStruct(lastPID, PV.ViewID, DeathNature.None));
        //        }

        //        try
        //        {
        //            Debug.Log($"Simple Damage_RPC");
        //            Player sourcePlayer = GameManager.GetPlayerWithPhotonViewId(lastPID);
        //            string feed = $"{sourcePlayer.nickName} killed {nickName}";
        //            foreach (KillFeedManager kfm in FindObjectsOfType<KillFeedManager>())
        //            {
        //                if (sourcePlayer != this)
        //                    kfm.EnterNewFeed(feed);
        //                else
        //                    kfm.EnterNewFeed($"<color=\"white\"> {nickName} committed suicide");
        //            }
        //        }
        //        catch
        //        {
        //            foreach (KillFeedManager kfm in FindObjectsOfType<KillFeedManager>())
        //            {
        //                kfm.EnterNewFeed($"Mistakes were made ({nickName})");
        //            }
        //        }
        //        finally
        //        {
        //            lastPID = -1;
        //        }
        //    }
    }

    [PunRPC]
    void Damage_RPC(int newHealth, int damage, int sourcePid, byte[] bytes, int dn, Vector3 impDir)
    {
        if (hitPoints <= 0 || isRespawning || isDead)
            return;

        try { this.impactDir = impDir; } catch { }
        try { _damageSource = System.Text.Encoding.UTF8.GetString(bytes); } catch { }
        try { deathNature = (DeathNature)dn; } catch { }
        try
        {
            if (lastPID > 0)// If a source already damaged this player
            {
                if (lastPID == pid && hitPoints <= maxHitPoints * 0.1f)
                {
                    // Do nothing, if players tries to kill himself with only 10% health left, the player that damaged this one before will get the kill
                }
                else
                    lastPID = sourcePid;
            }
            else // No source has damaged this player yet
                lastPID = sourcePid;
        }
        catch { }
        //try { this.impactPos = impactPos; this.impactDir = impactDir; } catch { }
        try { if (lastPlayerSource != this) lastPlayerSource.GetComponent<PlayerMultiplayerMatchStats>().damage += damage; } catch { }
        try { allPlayerScripts.damageIndicatorManager.SpawnNewDamageIndicator(sourcePid); } catch { }

        if (newHealth <= 0 && isInvincible) newHealth = 1;

        hitPoints = newHealth;


        try
        { // Hit Marker Handling

            if (isDead)
                lastPlayerSource.GetComponent<PlayerUI>().SpawnHitMarker(PlayerUI.HitMarkerType.Kill);
            else
                lastPlayerSource.GetComponent<PlayerUI>().SpawnHitMarker();
        }
        catch { }

    }

    void SpawnDeathKillFeed()
    {

    }
    void UpdateData()
    {
        if (isMine)
            PV.RPC("UpdateData_RPC", RpcTarget.All, hitPoints, overshieldPoints);
    }

    [PunRPC]
    void UpdateHitPoints_RPC(float h, float o)
    {
        {
            overshieldPoints = o;
            networkHitPoints = h;
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
        try
        {
            playerInventory.allWeaponsInInventory[wIndex].GetComponent<WeaponProperties>().UpdateAmmo(wIndex, ammo, isSpare, false);
        }
        catch { }
    }

    [PunRPC]
    void Teleport_RPC(Vector3 t, Vector3 r)
    {
        GetComponent<CharacterController>().enabled = false;
        transform.position = t;
        GetComponent<CharacterController>().enabled = true;
        transform.eulerAngles = r;
    }

    [PunRPC]
    void IsDead_RPC()
    {
        isDead = true;
    }

    [PunRPC]
    void DropWeapon_RPC(int wi, Vector3 spp, Vector3 fDir, Dictionary<string, int> param)
    {
        GameObject wo = Instantiate(playerInventory.allWeaponsInInventory[wi].GetComponent<WeaponProperties>().weaponRessource, spp, Quaternion.identity);
        wo.name = wo.name.Replace("(Clone)", "");

        try { wo.GetComponent<LootableWeapon>().networkAmmo = param["ammo"]; } catch (System.Exception e) { Debug.Log(e); }
        try { wo.GetComponent<LootableWeapon>().spareAmmo = param["spareammo"]; } catch (System.Exception e) { Debug.Log(e); }
        try { wo.GetComponent<LootableWeapon>().tts = param["tts"]; } catch (System.Exception e) { Debug.Log(e); }
        wo.GetComponent<LootableWeapon>().GetComponent<Rigidbody>().AddForce(fDir * 200);

        //StartCoroutine(UpdateWeaponSpawnPosition_Coroutine(wo, spp));
        wo.GetComponent<LootableWeapon>().spawnPointPosition = spp;
    }

    [PunRPC]
    void SendHitPointsCheck_RPC(int h, bool im, int tt)
    {
        GameManager.report += $"SendHitPointsCheck_RPC<br>===============<br>PLAYER: {nickName}<br>Is mine: {im}<br>Health: {h}<br>Time: {tt}<br><br>";
        if (!im)
            GameManager.report += "<br><br><br>";
    }

    string mmm;

    public void SendEndGameReport()
    {
    }


    [PunRPC]
    void AddPlayerKill_RPC(int wpid, int lpid, int dni, string dSource)
    {
        if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
            MultiplayerManager.instance.AddPlayerKill(new MultiplayerManager.AddPlayerKillStruct(wpid, lpid, (DeathNature)dni, dSource));
    }

    #endregion
    IEnumerator UpdateWeaponSpawnPosition_Coroutine(GameObject wo, Vector3 spp)
    {
        yield return new WaitForEndOfFrame();
        wo.GetComponent<LootableWeapon>().spawnPointPosition = spp;

    }
}
