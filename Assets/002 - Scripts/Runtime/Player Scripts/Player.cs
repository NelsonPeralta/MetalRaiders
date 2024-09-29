using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.CompilerServices;
using System;
using System.Linq;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;

public class Player : Biped
{
    public static int RESPAWN_TIME
    {
        get
        {
            if (GameManager.instance.gameMode == GameManager.GameMode.Coop) return 7;
            return 5;
        }
    }

    public static int MELEE_PUSH = 8;
    public static float MELEE_DISTANCE = 3;




    public delegate void PlayerEvent(Player playerProperties);
    public PlayerEvent OnPlayerDeath, OnPlayerDeathLate, OnPlayerHitPointsChanged, OnPlayerDamaged, OnPlayerHealthDamage,
        OnPlayerHealthRechargeStarted, OnPlayerShieldRechargeStarted, OnPlayerShieldDamaged, OnPlayerShieldBroken,
        OnPlayerRespawnEarly, OnPlayerRespawned, OnPlayerOvershieldPointsChanged, OnPlayerTeamChanged, OnPlayerIdAssigned;

    public enum DeathNature { None, Barrel, Headshot, Groin, Melee, Grenade, RPG, Stuck, Sniped, UltraBind }

    // public variables
    #region

    public ScriptObjPlayerData playerDataCell;



    public bool isInvincible { get { return _isInvincible; } set { _isInvincible = value; } }
    public int controllerId
    {
        get { return GetComponent<PlayerController>().rid; }
    }

    public bool isAlive { get { return (!isDead && !isRespawning); } }


    public bool hasArmor // Used to handle armor seller for Swarm Mode
    {
        get { return _hasArmor; }
        set
        {
            _hasArmor = value;
            if (value)
            {
                maxHitPoints = 250;
                maxShieldPoints = 150;
                maxHealthPoints = 100;
                hitPoints = 250;

                needsHealthPack = false;

                GetComponent<PlayerUI>().EnableArmorUI();
            }
            else
            {
                GetComponent<PlayerUI>().DisableArmorUI();
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
            {
                Debug.Log($"Player Damaged");
                _isTakingDamageForIndicator = 0.3f;
                OnPlayerDamaged?.Invoke(this);
            }

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
                    //PV.RPC("SetIsDead_RPC", RpcTarget.All);
                    //PV.RPC("SendHitPointsCheck_RPC", RpcTarget.All, (int)hitPoints, isMine, GameTime.instance.totalTime);
                }
                //isDead = true;
                PV.RPC("SetIsDead_RPC", RpcTarget.AllViaServer);

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
                    PV.RPC("SetIsDead_RPC", RpcTarget.AllViaServer);


                //isDead = true;

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
        get { if (playerInventory.activeWeapon.codeName == "oddball") return 250; return _meleeDamage; }
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

            }
        }
    }

    public bool isRespawning
    {
        get { return _isRespawning; }
        set
        {
            _isRespawning = value;
        }
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
                playerVoice.Stop(); playerVoice.clip = null;

                if (playerInventory.playerOddballActive)
                    NetworkGameManager.instance.DropOddball(weaponDropPoint.position, weaponDropPoint.forward);

                _rb.isKinematic = true;
                GetComponent<HitPoints>().OnDeath?.Invoke(GetComponent<HitPoints>()); // Needed for melee script
                _gameplayerRecordingPointsHolder.transform.parent = null;
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

    public int photonId
    {
        get { return PV.ViewID; }
    }

    public string localNickName
    {
        get
        {
            if (rid > 0)
                return $"{playerId} ({rid})";
            return "Player {rid}";
        }
    }

    public int playerId
    {
        get { return _playerId; }
        //private protected set
        //{
        //    if (PV.IsMine)
        //    {
        //        if (_playerId != value)
        //        {
        //            Debug.Log($"Changing player Id: {_playerId}");
        //            _playerId = value;
        //            //if (rid > 0)
        //            //    _playerId += $" ({rid})";

        //            PV.RPC("UpdatePlayerId_RPC", RpcTarget.All, playerId);
        //        }
        //    }

        //    //_playerArmorManager.HardReloadArmor();
        //}

    }

    public string username
    {
        get
        {
            return _username;
        }
        private set
        {
            _username = value;
        }
    }
    [SerializeField] string _username;

    public bool isMine { get { return GetComponent<PhotonView>().IsMine; } }

    public int rid
    {
        get { return GetComponent<PlayerController>().rid; }
    }

    public GameManager.Team team
    {
        get
        {
            return playerDataCell.team;
        }
    }
    public int lastPID
    {
        get { return _lastPID; }
        set
        {
            //if(GameManager.instance.pid_player_Dict.ContainsKey(value)) _lastPID = value;else _lastPID = 0;
            _lastPID = value;

            try { playerThatKilledMe = GameManager.GetPlayerWithPhotonView(_lastPID); } catch { _lastPID = 0; }
        }
    }

    public Player playerThatKilledMe { get { return _lastPlayerSource; } private set { _lastPlayerSource = value; } }

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


            // 1080p, 1 player
            //hor to ver
            //90  58.72
            //60  35.98
            //30  17.14

            // 1080p, 2 player
            //hor to ver
            //90  31.42
            //60  18.45
            //30  8.62

            //if (GameManager.instance.nbLocalPlayersPreset % 2 == 0) _defaultVerticalFov = 31.42f; // too fish eye
            if (GameManager.instance.nbLocalPlayersPreset % 2 == 0) _defaultVerticalFov = 25;
            else if (GameManager.instance.nbLocalPlayersPreset == 1) _defaultVerticalFov = 58.72f;
            else if (GameManager.instance.nbLocalPlayersPreset == 3)
            {
                //if (rid == 2) _defaultVerticalFov = 31.42f; // too fish eye
                if (rid == 2) _defaultVerticalFov = 25;
                else _defaultVerticalFov = 58.72f;
            }


            GetComponent<PlayerController>().mainCam.fieldOfView = defaultVerticalFov;
            GetComponent<PlayerController>().uiCam.fieldOfView = defaultVerticalFov;
            GetComponent<PlayerController>().gunCam.fieldOfView = 60;
        }
    }

    public PlayerArmorManager playerArmorManager { get { return _playerArmorManager; } }
    public PlayerThirdPersonModel playerThirdPersonModel { get { return _playerThirdPersonModel; } }
    public PlayerShield playerShield { get { return _playerShield; } }
    public KillFeedManager killFeedManager { get { return _killFeedManager; } }
    public ReticuleFriction reticuleFriction { get { return _reticuleFriction; } }
    public PlayerCapsule playerCapsule { get { return _playerCapsule; } }
    public PlayerShooting playerShooting { get { return _playerShooting; } }
    public AssignActorPlayerTargetOnShootingSphere assignActorPlayerTargetOnShootingSphere { get { return _assignActorPlayerTargetOnShootingSphere; } }
    public PlayerUI playerUI { get { return _playerUi; } }

    #endregion

    // serialized variables
    #region

    [SerializeField] NetworkPlayer _networkPlayer;
    [SerializeField] PlayerMedals _playerMedals;
    [SerializeField] int _playerId; // Player ID MUST be a number of equal value set to PhotonNetwork.Nickname which is determined by the player's id in the spacewackos.com database
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
    [SerializeField] int _pushForce = 7;
    [SerializeField] Announcer _announcer;
    [SerializeField] PlayerArmorManager _playerArmorManager;
    [SerializeField] PlayerThirdPersonModel _playerThirdPersonModel;
    [SerializeField] PlayerShield _playerShield;
    [SerializeField] KillFeedManager _killFeedManager;
    [SerializeField] ReticuleFriction _reticuleFriction;
    [SerializeField] PlayerCapsule _playerCapsule;
    [SerializeField] PlayerShooting _playerShooting;
    [SerializeField] PlayerUI _playerUi;
    [SerializeField] AssignActorPlayerTargetOnShootingSphere _assignActorPlayerTargetOnShootingSphere;
    [SerializeField] Explosion _ultraMergeExPrefab;
    [SerializeField] Transform _gameplayerRecordingPointsHolder;

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
    public PlayerMovement movement;
    public PlayerInteractableObjectHandler playerInteractableObjectHandler;

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
    public AudioSource respawnBeepAudioSource;
    public AudioClip sprintingClip;
    public AudioClip[] meleeClips;
    public AudioClip[] hurtClips;
    public AudioClip[] shootingEnemy;
    public AudioClip[] reloadingClips;
    public AudioClip[] allyDownClips;
    public AudioClip[] throwGrenadeClips;
    public AudioClip[] enemyDownClips;
    public AudioClip[] outOfAmmoClips;
    public AudioClip[] stuckClips;


    public PhotonView PV;

    [Header("Player Info")]
    public bool needsHealthPack;

    // Private Variables

    [SerializeField] float _healingCountdown;
    [SerializeField] float _shieldRechargeCountdown;


    public GameObject bloodImpact;
    public Transform weaponDropPoint;

    [SerializeField] GameObject headhunterSkullPrefab;

    Rigidbody _rb;

    public bool isTakingDamage { get { return _isTakingDamageForIndicator > 0; } }

    float _respawnCountdown, _isTakingDamageForIndicator;
    int _respawnBeepCount;

    private void Awake()
    {
        OnPlayerIdAssigned -= OnPlayerIdAssigned_Delegate;
        OnPlayerIdAssigned += OnPlayerIdAssigned_Delegate;

        Debug.Log($"Player Awake {GameManager.instance.GetAllPhotonPlayers().Count()}");
        _playerId = -99999; _playerId = int.Parse(PV.Owner.NickName);
        if (GameManager.instance.connection == GameManager.Connection.Local)
            _playerId = GameManager.instance.GetAllPhotonPlayers().Count();

        _rb = GetComponent<Rigidbody>(); if (!PV.IsMine) _rb.isKinematic = true;

        if (GameManager.instance.gameMode == GameManager.GameMode.Versus)
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
        if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
        {
            DisableArmorComponentsOnRespawn();
        }



        hitboxes = GetComponentsInChildren<PlayerHitbox>().ToList();
        _networkPlayer = GetComponent<NetworkPlayer>();
        GameManager.instance.AddToPhotonToPlayerDict(photonId, this);
    }
    private void Start()
    {
        OnPlayerDeath += OnPlayerDeath_Delegate;
        OnPlayerDeathLate += OnPlayerDeath_DelegateLate;
        OnPlayerDamaged += OnPlayerDamaged_Delegate;
        OnPlayerHealthDamage += OnPlayerHealthDamaged_Delegate;
        OnPlayerDeath += GetComponent<PlayerController>().OnDeath_Delegate;

        _lastPID = -1;
        spawnManager = SpawnManager.spawnManagerInstance;
        gameObjectPool = GameObjectPool.instance;
        weaponPool = FindObjectOfType<WeaponPool>();
        PV = GetComponent<PhotonView>();

        //playerId = int.Parse(GameManager.GetLocalMasterPlayer().PV.Owner.NickName);
        //if (rid > 0)
        //else
        //    username = CurrentRoomManager.instance.GetPlayerDataWithId(int.Parse(PV.Owner.NickName)).playerExtendedPublicData.username;
        GetComponent<PlayerUI>().isMineText.text = $"IM: {PV.IsMine}";
        gameObject.name = $"Player {PV.Owner.NickName} - {username} - ({rid})"; if (PV.IsMine) gameObject.name += " - IM";
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





        originalSpawnPosition = transform.position;
        GameManager.instance.instantiation_position_Biped_Dict.Add(originalSpawnPosition, this); GameManager.instance.instantiation_position_Biped_Dict = GameManager.instance.instantiation_position_Biped_Dict;


        defaultVerticalFov = 0; GetComponent<PlayerController>().Descope();

        //GameManager.instance.AddToPhotonToPlayerDict(photonId, this);
        if (isMine) StartCoroutine(AddPlayerJoinedCount_Coroutine());


        // Bug
        mainCamera.enabled = false;
        gunCamera.enabled = false;
        uiCamera.enabled = false;



        //playerArmorManager.playerDataCell = CurrentRoomManager.GetPlayerDataWithId(playerId, rid);

    }
    private void Update()
    {
        if (_hurtCooldown > 0) _hurtCooldown -= Time.deltaTime;
        if (_isTakingDamageForIndicator > 0) _isTakingDamageForIndicator -= Time.deltaTime;


        if (!PV.IsMine) _rb.isKinematic = true;
        HitPointsRecharge();
        OvershieldPointsRecharge();
    }


    IEnumerator AddPlayerJoinedCount_Coroutine()
    {
        yield return new WaitForEndOfFrame();
        if (isMine) NetworkGameManager.instance.AddPlayerJoinedCount();
    }


    //private void OnControllerColliderHit(ControllerColliderHit hit)
    //{
    //    //Debug.Log("Player OnControllerColliderHit");
    //    float movementSpeedRatio = GetComponent<PlayerMovement>().speedRatio;
    //    Rigidbody rb = hit.collider.attachedRigidbody;
    //    if (rb && !rb.isKinematic)
    //    {
    //        rb.velocity = hit.moveDirection * _pushForce * movementSpeedRatio;
    //    }
    //}

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

    public void TriggerAllPlayersJoinedBehaviour()
    {
        UpdateRewiredId(rid);
    }


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
        Vector3? impactPos = null, Vector3? impactDir = null, string damageSourceCleanName = null,
        bool isGroin = false, int weaponIndx = -1, WeaponProperties.KillFeedOutput kfo = WeaponProperties.KillFeedOutput.Unassigned,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        print($"Damage: ({damage}) {damageSourceCleanName}");


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


        //try { this.impactPos = impactPos; this.impactDir = impactDir; } catch { }
        //this.impactPos = impactPos; this.impactDir = impactDir;

        //if ((GameManager.PlayerDictContainsPhotonId(source_pid) && GameManager.GetPlayerWithPhotonView(source_pid).isMine) ||
        //    PhotonView.Find(source_pid).GetComponent<Actor>())
        if ((GameManager.GetPlayerWithPhotonView(source_pid) && GameManager.GetPlayerWithPhotonView(source_pid).isMine) ||
        PhotonView.Find(source_pid).GetComponent<Actor>())
        {
            DeathNature dsn = DeathNature.None;
            if (headshot)
                dsn = DeathNature.Headshot;
            if (headshot && GameManager.GetPlayerWithPhotonView(source_pid).playerInventory.activeWeapon.weaponType == WeaponProperties.WeaponType.Sniper)
                dsn = DeathNature.Sniped;
            if (isGroin)
                dsn = DeathNature.Groin;


            byte[] bytes = Encoding.UTF8.GetBytes("");

            if (damageSourceCleanName != null)
            {
                if (damageSourceCleanName.Contains("renade"))
                    dsn = DeathNature.Grenade;
                else if (damageSourceCleanName.Contains("RPG"))
                    dsn = DeathNature.RPG;
                else if (damageSourceCleanName.Contains("tuck"))
                    dsn = DeathNature.Stuck;
                else if (damageSourceCleanName.Contains("elee"))
                    dsn = DeathNature.Melee;
                else if (damageSourceCleanName.Contains("arrel"))
                    dsn = DeathNature.Barrel;
                else if (damageSourceCleanName.Contains("ltra"))
                    dsn = DeathNature.UltraBind;
                //else
                //    //Debug.LogError($"UNHANDLEDED DEATH NATURE: {dsn}");

                //    bytes = Encoding.UTF8.GetBytes(damageSourceCleanName);
            }
            //Debug.LogError($"EMPTY DEATH NATURE");



            int newHealth = (int)hitPoints - damage;
            PV.RPC("Damage_RPC", RpcTarget.All, newHealth, damage, source_pid, (int)dsn, impactPos, impactDir, weaponIndx, (int)kfo);
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



    int randomSound, ranClipChance;
    int _playerVoiceCooldownTime { get { return 12 + (GameManager.instance.GetAllPhotonPlayers().Count * 3); } }
    int _ranClipChanceOdds { get { return UnityEngine.Random.Range(0, 90 + (GameManager.instance.GetAllPhotonPlayers().Count * 12)); } }
    int _maxRanClipChance = 30;

    public void PlayMeleeSound()
    {
        randomSound = UnityEngine.Random.Range(0, meleeClips.Length);
        playerVoice.clip = meleeClips[randomSound];
        playerVoice.Play();
    }

    float _hurtCooldown;

    public void PlayHurtSound()
    {
        //if (hitPoints > 15)
        if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
        {
            if (_hurtCooldown <= 0)
            {
                _hurtCooldown = 4;


                randomSound = UnityEngine.Random.Range(0, hurtClips.Length);
                playerVoice.clip = hurtClips[randomSound];

                if (!playerVoice.isPlaying)
                    playerVoice.Play();
            }
        }
    }

    public void PlayShootingEnemyClip()
    {
        if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
        {
            ranClipChance = _ranClipChanceOdds;

            if (ranClipChance < _maxRanClipChance && GameManager.instance.commonPlayerVoiceCooldown <= 0 && SwarmManager.instance.TimeSinceEnemiesDropped <= 10)
            {
                GameManager.instance.commonPlayerVoiceCooldown = _playerVoiceCooldownTime;


                randomSound = UnityEngine.Random.Range(0, shootingEnemy.Length);
                playerVoice.clip = shootingEnemy[randomSound];

                if (!playerVoice.isPlaying)
                    playerVoice.Play();
            }
        }
    }

    public void PlayReloadingClip()
    {
        if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
        {
            ranClipChance = _ranClipChanceOdds;

            if (ranClipChance < _maxRanClipChance && GameManager.instance.commonPlayerVoiceCooldown <= 0)
            {
                GameManager.instance.commonPlayerVoiceCooldown = _playerVoiceCooldownTime;


                randomSound = UnityEngine.Random.Range(0, reloadingClips.Length);
                playerVoice.clip = reloadingClips[randomSound];

                if (!playerVoice.isPlaying)
                    playerVoice.Play();
            }
        }
    }

    void PlayAllyDownClip(Player p)
    {
        if (p != this)
            if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
            {
                ranClipChance = _ranClipChanceOdds;

                if (ranClipChance < _maxRanClipChance)
                {
                    GameManager.instance.commonPlayerVoiceCooldown = _playerVoiceCooldownTime;


                    randomSound = UnityEngine.Random.Range(0, allyDownClips.Length);
                    playerVoice.clip = allyDownClips[randomSound];

                    if (!playerVoice.isPlaying)
                        playerVoice.Play();
                }
            }
    }

    public void PlayThrowingGrenadeClip()
    {
        if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
        {
            ranClipChance = _ranClipChanceOdds;

            if (ranClipChance < _maxRanClipChance && GameManager.instance.commonPlayerVoiceCooldown <= 0)
            {
                GameManager.instance.commonPlayerVoiceCooldown = _playerVoiceCooldownTime;


                randomSound = UnityEngine.Random.Range(0, throwGrenadeClips.Length);
                playerVoice.clip = throwGrenadeClips[randomSound];

                if (!playerVoice.isPlaying)
                    playerVoice.Play();
            }
        }
    }

    public void PlayEnemyDownClip()
    {
        if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
        {
            ranClipChance = _ranClipChanceOdds;

            if (ranClipChance < _maxRanClipChance && GameManager.instance.commonPlayerVoiceCooldown <= 0)
            {
                GameManager.instance.commonPlayerVoiceCooldown = _playerVoiceCooldownTime;


                randomSound = UnityEngine.Random.Range(0, enemyDownClips.Length);
                playerVoice.clip = enemyDownClips[randomSound];

                if (!playerVoice.isPlaying)
                    playerVoice.Play();
            }
        }
    }

    public void PlayOutOfAmmoClip()
    {
        if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
        {
            ranClipChance = _ranClipChanceOdds;

            if (ranClipChance < _maxRanClipChance && GameManager.instance.commonPlayerVoiceCooldown <= 0)
            {
                GameManager.instance.commonPlayerVoiceCooldown = _playerVoiceCooldownTime;


                randomSound = UnityEngine.Random.Range(0, outOfAmmoClips.Length);
                playerVoice.clip = outOfAmmoClips[randomSound];

                if (!playerVoice.isPlaying)
                    playerVoice.Play();
            }
        }
    }


    public void PlayStuckClip()
    {
        ranClipChance = _ranClipChanceOdds;

        if (ranClipChance < 19 && GameManager.instance.commonPlayerVoiceCooldown <= 0)
        {
            GameManager.instance.commonPlayerVoiceCooldown = _playerVoiceCooldownTime;


            randomSound = UnityEngine.Random.Range(0, stuckClips.Length);
            playerVoice.clip = stuckClips[randomSound];

            if (!playerVoice.isPlaying)
                playerVoice.Play();
        }
    }


    public void LeaveRoomWithDelay()
    {
        if (controllerId == 0)
            StartCoroutine(LeaveRoomWithDelay_Coroutine());
    }

    void SpawnRagdoll()
    {
        var ragdoll = RagdollPool.instance.GetPooledPlayerRagdoll(Array.IndexOf(CurrentRoomManager.instance.playerDataCells.ToArray(), playerDataCell), isMine);
        //var ragdoll = RagdollPool.instance.GetRa
        ragdoll.transform.position = transform.position + new Vector3(0, -1, 0);
        ragdoll.transform.rotation = transform.rotation;
        ragdoll.GetComponent<PlayerArmorManager>().player = this;


        Debug.Log($"SPAWNING PLAYER RAGDOLL {ragdoll.name} {deathNature} {impactDir} {impactPos}");


        if (GameManager.instance.connection == GameManager.Connection.Online)
            ragdoll.GetComponent<PlayerArmorManager>().playerDataCell = CurrentRoomManager.GetDataCellWithDatabaseIdAndRewiredId(playerId, rid);
        else
            ragdoll.GetComponent<PlayerArmorManager>().playerDataCell = CurrentRoomManager.GetLocalPlayerData(rid);


        ragdoll.GetComponent<PlayerRagdoll>().SetPlayerCamera(playerCamera, mainCamera);
        ragdoll.SetActive(true);



        //ragdoll.GetComponent<Animator>().enabled = false;

        impactDir = Vector3.Normalize((Vector3)impactDir);
        Debug.Log($"PLAYER RAGDOLL {ragdoll.name} {deathNature} {impactDir} {impactPos}");

        StartCoroutine(GiveRagdollPush_Coroutine((Vector3)impactDir, deathByHeadshot, deathNature, ragdoll.GetComponent<PlayerRagdoll>()));

        //if (deathByHeadshot)
        //    ragdoll.GetComponent<PlayerRagdoll>().head.GetComponent<Rigidbody>().AddForce((Vector3)impactDir * 6000);
        //else if (deathNature == DeathNature.Grenade || deathNature == DeathNature.Stuck || deathNature == DeathNature.RPG
        //    || deathNature == DeathNature.Barrel || deathNature == DeathNature.UltraBind)
        //    ragdoll.GetComponent<PlayerRagdoll>().hips.GetComponent<Rigidbody>().AddForce((Vector3)impactDir * 9000);
        //else if (deathNature == DeathNature.Melee)
        //    ragdoll.GetComponent<PlayerRagdoll>().hips.GetComponent<Rigidbody>().AddForce((Vector3)impactDir * 8000);
        //else/* if (!deathByHeadshot || deathNature == DeathNature.None)*/
        //    ragdoll.GetComponent<PlayerRagdoll>().hips.GetComponent<Rigidbody>().AddForce((Vector3)impactDir * 5000);
    }


    IEnumerator GiveRagdollPush_Coroutine(Vector3 imdir, bool dbh, DeathNature dn, PlayerRagdoll playerRagdoll)
    {
        yield return new WaitForEndOfFrame();

        playerRagdoll.GetComponent<Animator>().enabled = false;

        if (dbh)
            playerRagdoll.head.GetComponent<Rigidbody>().AddForce((Vector3)imdir * 6000);
        else if (dn.ToString().Contains("renade") || dn == DeathNature.Stuck || dn == DeathNature.RPG
            || dn == DeathNature.Barrel || dn == DeathNature.UltraBind)
            playerRagdoll.hips.GetComponent<Rigidbody>().AddForce((Vector3)imdir * 9000);
        else if (dn == DeathNature.Melee)
            playerRagdoll.hips.GetComponent<Rigidbody>().AddForce((Vector3)imdir * 8000);
        else/* if (!deathByHeadshot || deathNature == DeathNature.None)*/
            playerRagdoll.hips.GetComponent<Rigidbody>().AddForce((Vector3)imdir * 5000);

    }



    void HitPointsRecharge()
    {

        if (healingCountdown > 0)
        {
            healingCountdown -= Time.deltaTime;
        }

        if (healingCountdown <= 0 && hitPoints < maxHitPoints)
        {
            if (!_isHealing && hasArmor)
                OnPlayerShieldRechargeStarted?.Invoke(this);

            _isHealing = true;
            if (hitPoints < maxHealthPoints)
            {
                if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
                {
                    if (hasArmor)
                        hitPoints += (Time.deltaTime * _shieldHealingIncrement);
                    else if (hitPoints < (maxHealthPoints * 0.45f))
                        hitPoints = Mathf.Clamp(hitPoints + (Time.deltaTime * 20), 0, maxHealthPoints * 0.45f);
                }
                else
                {
                    hitPoints += (Time.deltaTime * _healthHealingIncrement);
                }
            }
            else
            {
                if (hasArmor)
                    hitPoints += (Time.deltaTime * _shieldHealingIncrement);
            }

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
        Debug.Log("Respawn");
        _gameplayerRecordingPointsHolder.parent = transform; _gameplayerRecordingPointsHolder.transform.localPosition = Vector3.zero; _gameplayerRecordingPointsHolder.transform.localRotation = Quaternion.identity;
        _ultraMergeExPrefab.gameObject.SetActive(false); _ultraMergeCount = 0;

        if (!isRespawning)
            return;
        try { GetComponent<AllPlayerScripts>().scoreboardManager.CloseScoreboard(); } catch { }
        _lastPID = -1;
        deathNature = DeathNature.None;
        _killFeedOutput = WeaponProperties.KillFeedOutput.Unassigned;
        OnPlayerRespawnEarly?.Invoke(this);
        if (GameManager.instance.gameMode == GameManager.GameMode.Coop) DisableArmorComponentsOnRespawn();

        isRespawning = false;
        GetComponent<PlayerController>().Descope();

        hitPoints = maxHitPoints;

        mainCamera.gameObject.GetComponent<Transform>().transform.localRotation = allPlayerScripts.cameraScript.mainCamDefaultLocalRotation;
        mainCamera.gameObject.GetComponent<Transform>().transform.localPosition = allPlayerScripts.cameraScript.mainCamDefaultLocalPosition;
        gunCamera.enabled = true;
        GetComponent<PlayerUI>().ToggleUIExtremities(true);

        StartCoroutine(MakeThirdPersonModelVisible());

        playerInventory.fragGrenades = 2;
        if (GameManager.instance.gameType == GameManager.GameType.Swat
                || GameManager.instance.gameType == GameManager.GameType.Retro)
            playerInventory.fragGrenades = 1;

        if (GameManager.instance.gameType == GameManager.GameType.Hill) playerInventory.fragGrenades = 1;

        //StartCoroutine(playerInventory.EquipStartingWeapon());
        playerInventory.weaponsEquiped[1] = null;

        hitboxesEnabled = true;
        impactDir = Vector3.zero;
        OnPlayerRespawned?.Invoke(this);

        _rb.isKinematic = false;
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        _respawnBeepCount = 0;
        if (this.isMine) respawnBeepAudioSource.Play();

        if (_lastSpawnPointIsRandom) killFeedManager.EnterNewFeed("<color=\"red\">Spawned Randomly"); _lastSpawnPointIsRandom = false;
    }

    void DisableArmorComponentsOnRespawn()
    {
        if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
        {
            hasArmor = false;

            _overshieldPoints = 0;
            _maxShieldPoints = 0;
            _maxHitPoints = _maxHealthPoints = 250;
            _networkHitPoints = maxHitPoints;
            _hitPoints = maxHitPoints;
            needsHealthPack = true;
        }
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

        GameManager.instance.LeaveCurrentRoomAndLoadLevelZero();
    }

    #endregion

    // private functions
    #region

    #endregion

    // private coroutines
    #region



    bool _lastSpawnPointIsRandom;



    IEnumerator ShowScoreboardOnDeath_Coroutine()
    {
        yield return new WaitForSeconds(RESPAWN_TIME - 2);

        GetComponent<AllPlayerScripts>().scoreboardManager.OpenScoreboard();

    }

    IEnumerator MidRespawn_Coroutine()
    {
        Debug.Log("MidRespawnAction");
        yield return new WaitForSeconds(RESPAWN_TIME - 1);
        NetworkGameManager.instance.AskMasterToReserveSpawnPoint(photonId, rid);
    }
    IEnumerator LateRespawnAction()
    {
        Debug.Log("LateRespawnAction");
        yield return new WaitForSeconds(RESPAWN_TIME * 0.99f);
        try { allPlayerScripts.damageIndicatorManager.HideAllIndicators(); } catch { }

        hitPoints = maxHitPoints;
        transform.position = _reservedSpawnPointTrans.position + new Vector3(0, 2, 0);
        transform.rotation = SpawnManager.spawnManagerInstance.GetSpawnPointAtPos(_reservedSpawnPointTrans.position).rotation;
        isDead = false;
    }



    IEnumerator Respawn_Coroutine()
    {
        Debug.Log("Respawn_Coroutine");
        yield return new WaitForSeconds(RESPAWN_TIME);

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
        GameManager.GetRootPlayer().PlayAllyDownClip(this);



        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;


        playerInventory.transform.localRotation = Quaternion.Euler(0, 0, 0f);


        if (isDead)
        {
            if (isMine && GameManager.instance.gameMode == GameManager.GameMode.Versus)
                PV.RPC("AddPlayerKill_RPC", RpcTarget.AllViaServer, _lastPID, PV.ViewID, (int)_deathNature, (int)_killFeedOutput);

            {
                // Spawn Skull
                //GameObject s = Instantiate(headhunterSkullPrefab, weaponDropPoint.position, Quaternion.identity);
                //s.GetComponent<Rigidbody>().AddForce(new Vector3(0, 800, 0));
            }

            if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
                GetComponent<PlayerSwarmMatchStats>().deaths++;
        }

        isRespawning = true;
        hitboxesEnabled = false;
        thirdPersonModels.SetActive(false);

        if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
            SwarmManager.instance.livesLeft--;

        GetComponent<PlayerController>().DisableCrouch();
        GetComponent<PlayerController>().isHoldingShootBtn = false;
        GetComponent<PlayerUI>().scoreboard.CloseScoreboard();
        GetComponent<PlayerUI>().ToggleUIExtremities(false);

        gameObject.GetComponent<ScreenEffects>().orangeScreen.SetActive(false);

        try
        {
            mainCamera.gameObject.GetComponent<Transform>().transform.Rotate(30, 0, 0);
            mainCamera.gameObject.GetComponent<Transform>().transform.localPosition = new Vector3(mainOriginalCameraPosition.x, 2, -2.5f);


            gunCamera.enabled = false;
        }
        catch (System.Exception e) { Debug.LogException(e); }
        finally { gunCamera.enabled = false; }

        hitboxesEnabled = false;





        if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
            foreach (PlayerArmorPiece pap in playerInventory.activeWeapon.GetComponentsInChildren<PlayerArmorPiece>(true))
                pap.gameObject.SetActive(false);




        SpawnRagdoll();
        StartCoroutine(ShowScoreboardOnDeath_Coroutine());
        StartCoroutine(Respawn_Coroutine());
        StartCoroutine(MidRespawn_Coroutine());
        StartCoroutine(LateRespawnAction());

    }
    void OnPlayerDeath_DelegateLate(Player playerProperties)
    {
        DropWeaponOnDeath(playerInventory.activeWeapon, playerInventory.holsteredWeapon, secondWeaponOffset: new Vector3(0.5f, 0.5f, 0));
    }


    void OnPlayerDamaged_Delegate(Player player)
    {
        Debug.Log($"OnPlayerDamaged_Delegate {needsHealthPack}");
        try { GetComponent<PlayerController>().Descope(); } catch { }

        _isHealing = false;
        healingCountdown = _defaultHealingCountdown;
        if (!needsHealthPack)
        {
            Debug.Log($"OnPlayerDamaged_Delegate {_defaultHealingCountdown}");
            shieldRechargeCountdown = _defaultHealingCountdown;
            if (hitPoints <= maxHealthPoints)
                shieldRechargeCountdown = _defaultHealingCountdown + ((maxHealthPoints - hitPoints) / _healthHealingIncrement);
        }
    }


    GameObject _bloodHit;

    void OnPlayerHealthDamaged_Delegate(Player player)
    {
        PlayHurtSound();
        _bloodHit = gameObjectPool.SpawnPooledBloodHit();
        _bloodHit.transform.position = _impactPos;
        _bloodHit.SetActive(true);
    }

    #endregion

    // rpc functions
    #region

    [PunRPC]
    void UpdateTeam_RPC(string nn)
    {
        if (!PV.IsMine)
        {
            //Debug.Log(nn);
            //_team = (PlayerMultiplayerMatchStats.Team)Enum.Parse(typeof(PlayerMultiplayerMatchStats.Team), nn);
            //OnPlayerTeamChanged?.Invoke(this);
            //name += $" {_team.ToString()} team";
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




    WeaponProperties.KillFeedOutput _killFeedOutput;



    [PunRPC]
    void Damage_RPC(int newHealth, int damage, int sourcePid, int dn, Vector3 impPos, Vector3 impDir, int weaponIndx, int kfo)
    {
        if (hitPoints <= 0 || isRespawning || isDead)
            return;

        print($"Damage_RPC {impPos} {impDir}");
        this.impactPos = transform.position; this.impactDir = Vector3.zero;
        try { this.impactDir = impDir; } catch { }
        try { this.impactPos = impPos; } catch { }
        if (impactPos == Vector3.zero) impactPos = transform.position;
        print($"Damage_RPC {impPos} {impDir}");



        playerController.Descope();
        _killFeedOutput = (WeaponProperties.KillFeedOutput)kfo;
        print($"Damage_RPC {_killFeedOutput}");
        try { deathNature = (DeathNature)dn; } catch (System.Exception e) { Debug.LogError($"COULD NOT ASSIGN DEATH NATURE {dn}. {e}"); }

        print($"COULD NOT ASSIGN DEATH NATURE {dn} {deathNature = (DeathNature)dn}");

        if (deathNature == DeathNature.Melee)
            SoundManager.instance.PlayAudioClip((Vector3)this.impactPos, SoundManager.instance.successfulPunch);


        try
        {
            //if (GameManager.PlayerDictContainsPhotonId(sourcePid))
            {

                if (lastPID > 0)// If a source already damaged this player
                {
                    if (lastPID == photonId && hitPoints <= maxHitPoints * 0.1f)
                    {
                        // Do nothing, if players tries to kill himself with only 10% health left, the player that damaged this one before will get the kill
                    }
                    else
                        lastPID = sourcePid;
                }
                else // No source has damaged this player yet
                    lastPID = sourcePid;
            }
        }
        catch { }
        //try { this.impactPos = impactPos; this.impactDir = impactDir; } catch { }
        try
        {
            if ((GameManager.instance.teamMode == GameManager.TeamMode.Classic && playerThatKilledMe.team != this.team) || GameManager.instance.teamMode == GameManager.TeamMode.None)
                if (playerThatKilledMe != this) playerThatKilledMe.GetComponent<PlayerMultiplayerMatchStats>().damage += Mathf.Clamp(damage, 0, (int)hitPoints);
        }
        catch { }
        try { allPlayerScripts.damageIndicatorManager.SpawnNewDamageIndicator(sourcePid); } catch { }

        if (newHealth <= 0 && isInvincible) newHealth = 1;

        hitPoints = newHealth;

        if (weaponIndx >= 0)
        {
            if (playerInventory.allWeaponsInInventory[weaponIndx].GetComponent<WeaponProperties>().ultraBind)
                ultraMergeCount++;
        }


        if (!isDead && playerThatKilledMe)
        {
            playerThatKilledMe.GetComponent<PlayerUI>().SpawnHitMarker();


            // grenade jumping
            if (_deathNature == DeathNature.RPG || _deathNature == DeathNature.Grenade ||
                _deathNature == DeathNature.Barrel || _deathNature == DeathNature.UltraBind)
                if (movement.blockedMovementType != PlayerMovement.BlockedMovementType.ManCannon)
                {
                    movement.blockPlayerMoveInput = 0.2f;
                    movement.blockedMovementType = PlayerMovement.BlockedMovementType.Other;
                    _rb.velocity = Vector3.zero;
                    _rb.angularVelocity = Vector3.zero;
                    _rb.useGravity = true;
                    _rb.drag = 0;
                    _impactDir.y *= 3;
                    _rb.AddForce(_impactDir.normalized * 4f, ForceMode.Impulse);

                }
        }
    }


    void AchievementCheck(int sourcePid)
    {
        if (!PV.IsMine && sourcePid == GameManager.GetRootPlayer().photonId)
            if (_killFeedOutput.ToString().ToLower().Contains("plasma"))
            {
                AchievementManager.instance.plasmaKillsInThisGame++;
            }
            else if (deathNature == DeathNature.Barrel)
            {
                AchievementManager.instance.gotAKillByBlowingUpABarrel = true;
            }
            else if (deathNature == DeathNature.Groin)
            {
                AchievementManager.instance.gotANutshotKill = true;
            }
    }

    void SpawnDeathKillFeed()
    {

    }
    void UpdateData()
    {
        if (isMine)
            PV.RPC("UpdateData_RPC", RpcTarget.All, hitPoints, overshieldPoints);
    }


    public void ChangePlayerIdLocalMode(int id) // Only works when no internet connection
    {
        //if (GameManager.instance.connection == GameManager.Connection.Local)
        //{
        //    print("ChangePlayerIdLocalMode 1");
        //    _playerId = id;
        //    //OnPlayerIdAssigned?.Invoke(this);
        //    print("ChangePlayerIdLocalMode 2");
        //}
    }


    void OnPlayerIdAssigned_Delegate(Player p)
    {
        print("OnPlayerIdAssigned_Delegate");
        playerDataCell = CurrentRoomManager.GetDataCellWithDatabaseIdAndRewiredId(_playerId, rid);
        username = CurrentRoomManager.GetDataCellWithDatabaseIdAndRewiredId(_playerId, rid).playerExtendedPublicData.username;
        foreach (PlayerWorldUIMarker pw in allPlayerScripts.worldUis) pw.text.text = _username;

        playerUI.SetScoreWitnesses();
        playerArmorManager.playerDataCell = CurrentRoomManager.GetDataCellWithDatabaseIdAndRewiredId(playerId, rid);

        gameObject.name = $"Player {playerDataCell.playerExtendedPublicData.username}"; if (PV.IsMine) gameObject.name += " - IM";
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
    void Teleport_RPC(Vector3 t, Vector3 r)
    {
        Debug.Log("Teleport_RPC");
        transform.position = t;
        transform.eulerAngles = r;
    }

    [PunRPC]
    void SetIsDead_RPC()
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
        //wo.GetComponent<LootableWeapon>().spawnPointPosition = spp;
    }

    [PunRPC]
    void SendHitPointsCheck_RPC(int h, bool im, int tt)
    {
        GameManager.report += $"SendHitPointsCheck_RPC<br>===============<br>PLAYER: {playerId}<br>Is mine: {im}<br>Health: {h}<br>Time: {tt}<br><br>";
        if (!im)
            GameManager.report += "<br><br><br>";
    }

    string mmm;

    public void SendEndGameReport()
    {
    }


    [PunRPC]
    void AddPlayerKill_RPC(int wpid, int lpid, int dni, int kfo)
    {
        Debug.Log("AddPlayerKill_RPC");

        playerThatKilledMe = GameManager.GetPlayerWithPhotonView(wpid); _killFeedOutput = (WeaponProperties.KillFeedOutput)kfo; deathNature = (DeathNature)dni; _lastPID = wpid;

        playerThatKilledMe.GetComponent<PlayerUI>().SpawnHitMarker(PlayerUI.HitMarkerType.Kill);


        if (GameManager.instance.gameType == GameManager.GameType.GunGame
                    && deathNature == DeathNature.Stuck)
        {
            if (isMine)
                playerInventory.playerGunGameManager.index--;
        }

        try
        {
            foreach (KillFeedManager kfm in GameManager.instance.GetAllPhotonPlayers().Select(obj => obj.killFeedManager))
            {
                if (kfm)
                {
                    string f = $"<color=#31cff9>{playerThatKilledMe.username} killed {username}";

                    if (_killFeedOutput != WeaponProperties.KillFeedOutput.Unassigned)
                        f = $"<color=#31cff9>{playerThatKilledMe.username} [ {_killFeedOutput.ToString().Replace("_", " ")} ] {username}";

                    if (GameManager.instance.gameType == GameManager.GameType.GunGame
                        && deathNature == DeathNature.Stuck)
                    {
                        f = $"<color=#31cff9>{playerThatKilledMe.username} <color=\"red\"> Humiliated </color> <color=#31cff9>{username}";
                        GetComponent<PlayerMultiplayerMatchStats>().score--;
                    }
                    else if (deathNature == DeathNature.Sniped)
                        f = $"<color=#31cff9>{playerThatKilledMe.username} <color=\"yellow\">!!! Sniped !!!</color> <color=#31cff9>{username}";
                    else if (deathByHeadshot)
                        f += $" with a <color=\"red\">Headshot</color>!";
                    else if (deathByGroin)
                        f += $" with a <color=\"yellow\">!!! Nutshot !!!</color>!";
                    else if (deathNature == DeathNature.Stuck)
                        f = $"<color=#31cff9>{playerThatKilledMe.username} [ Stuck ] {username}";

                    if (GameManager.instance.teamMode == GameManager.TeamMode.Classic && playerThatKilledMe.team == this.team)
                        f = $"<color=#31cff9>{playerThatKilledMe.username} <color=\"red\"> Buddyfucked </color> {username}";

                    if (deathNature == DeathNature.UltraBind)
                        f = $"<color=#31cff9>{playerThatKilledMe.username} [ Splinter ] {username}";

                    if (_killFeedOutput == WeaponProperties.KillFeedOutput.Assasination)
                        f = $"<color=#31cff9>{playerThatKilledMe.username} [ <color=\"yellow\"> Assasination! </color> ] {username}";

                    if (this != playerThatKilledMe)
                    {
                        kfm.EnterNewFeed(f);
                        continue;
                    }
                    else
                        kfm.EnterNewFeed($"<color=#31cff9>{username} committed suicide");
                }
            }
        }
        catch (System.Exception e) { Debug.LogException(e); }


        if (GameManager.instance.gameMode == GameManager.GameMode.Versus)
            if (GameManager.instance.teamMode == GameManager.TeamMode.None ||
                                (GameManager.instance.teamMode == GameManager.TeamMode.Classic && playerThatKilledMe.team != this.team))
            {

                PlayerMedals sourcePlayerMedals = playerThatKilledMe._playerMedals;
                if (sourcePlayerMedals != this._playerMedals && _lastPID != this.photonId)
                {
                    print($"Spawning medal: {deathByGroin} {deathNature}");
                    if (deathByGroin)
                    {
                        deathNature = DeathNature.Groin;
                        sourcePlayerMedals.SpawnNutshotMedal();
                    }
                    else if (deathNature == DeathNature.Sniped)
                        sourcePlayerMedals.SpawnSniperHeadshotMedal();
                    else if (deathNature == DeathNature.Headshot)
                        sourcePlayerMedals.SpawnHeadshotMedal();
                    else if (_killFeedOutput == WeaponProperties.KillFeedOutput.Assasination)
                        sourcePlayerMedals.SpawnAssasinationMedal();
                    else if (deathNature == DeathNature.Melee)
                        sourcePlayerMedals.SpawnMeleeMedal();
                    else if (deathNature.ToString().Contains("renade") && _killFeedOutput != WeaponProperties.KillFeedOutput.Grenade_Launcher)
                        sourcePlayerMedals.SpawnGrenadeMedal();
                    else if (deathNature == DeathNature.Stuck)
                        sourcePlayerMedals.SpawnStuckKillMedal();
                    else
                        sourcePlayerMedals.kills++;

                    //if (_playerMedals.spree >= 5)
                    //    sourcePlayerMedals.SpawnKilljoySpreeMedal();
                }

                MultiplayerManager.instance.AddPlayerKill(new MultiplayerManager.AddPlayerKillStruct(wpid, lpid, (DeathNature)dni, (WeaponProperties.KillFeedOutput)kfo));
                AchievementCheck(lastPID);
            }

    }

    #endregion


    public override void SpawnUltraBindExplosion()
    {
        if (PV.IsMine)
        {
            PV.RPC("SpawnUltraBindExplosion_RPC", RpcTarget.AllViaServer);
        }
    }



    Transform _reservedSpawnPointTrans;
    public void UpdateReservedSpawnPoint(Vector3 t, bool isRandom)
    {
        _reservedSpawnPointTrans = SpawnManager.spawnManagerInstance.GetSpawnPointAtPos(t);
        _lastSpawnPointIsRandom = isRandom;


        transform.position = _reservedSpawnPointTrans.position + new Vector3(0, 2, 0);
    }


    public void SetupMotionTracker()
    {
        print("SetupMotionTracker");
        movement.playerMotionTracker.Setup();
        //List<MotionTrackerDot> l = GetComponentsInChildren<MotionTrackerDot>().ToList();

        //if (l.Count > 0)
        //    for (int i = 0; i < l.Count; i++)
        //    {
        //        l[i].targetPlayerController = GameManager.instance.pid_player_Dict.ElementAt(i).Value.playerController;
        //    }
    }


    [PunRPC]
    void SpawnUltraBindExplosion_RPC()
    {
        base.SpawnUltraBindExplosion();

        print("Player SpawnUltraBindExplosion");

        GrenadePool.SpawnExplosion(_lastPlayerSource, damage: 700, radius: 2, GameManager.DEFAULT_EXPLOSION_POWER, damageCleanNameSource: "Ultra Bind", targetTrackingCorrectTarget.position, Explosion.Color.Purple, Explosion.Type.UltraBind, GrenadePool.instance.ultraBindClip, WeaponProperties.KillFeedOutput.Ultra_Bind);


        ultraMergeCount = 0;
    }


    [PunRPC]
    void UpdateRewiredId(int i, bool send = true)
    {
        if (send && PV.IsMine)
        {
            PV.RPC("UpdateRewiredId", RpcTarget.AllViaServer, i, false);
        }
        else
        {
            print($"UpdateRewiredId: {name} {playerId}.  {playerController.rid} -> {i}");
            playerController.rid = i;

            if (playerId != -99999 && playerController.rid != -99999)
            {
                if (PV.IsMine) NetworkGameManager.instance.AddPlayerSetCount();


                print($"UpdateRewiredId - OnPlayerIdAssigned: {name} {playerId} {playerController.rid}");
                OnPlayerIdAssigned?.Invoke(this);
            }
        }
    }
}
