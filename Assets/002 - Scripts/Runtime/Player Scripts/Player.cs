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
using static Player;

public class Player : MonoBehaviourPunCallbacks
{
    public delegate void PlayerEvent(Player playerProperties);
    public PlayerEvent OnPlayerDeath, OnPlayerHitPointsChanged, OnPlayerDamaged, OnPlayerHealthDamage,
        OnPlayerHealthRechargeStarted, OnPlayerShieldRechargeStarted, OnPlayerShieldDamaged, OnPlayerShieldBroken,
        OnPlayerRespawnEarly, OnPlayerRespawned, OnPlayerOvershieldPointsChanged, OnPlayerTeamChanged;

    public enum DeathNature { None, Headshot, Groin, Melee, Grenade, Stuck }

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
    public Announcer announcer { get { return _announcer; } }
    public DeathNature deathNature { get { return _deathNature; } private set { _deathNature = value; } }

    #endregion

    // serialized variables
    #region

    [SerializeField] NetworkPlayer _networkPlayer;
    [SerializeField] PlayerMultiplayerMatchStats.Team _team;
    [SerializeField] PlayerMedals playerMedals;
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

    string _damageSource;
    Vector3 _impactPos;
    Vector3 _impactDir;
    float _gameStartDelay;
    bool _allPlayersJoined;

    private bool deathByHeadshot { get { if (deathNature == DeathNature.Headshot) return true; else return false; } }
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

            if(GameManager.instance.gameType == GameManager.GameType.Swat)
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
            _maxHitPoints = 100;
            _networkHitPoints = maxHitPoints;
            _hitPoints = maxHitPoints;
            needsHealthPack = true;
        }

        hitboxes = GetComponentsInChildren<PlayerHitbox>().ToList();
        _networkPlayer = GetComponent<NetworkPlayer>();
    }
    private void Start()
    {
        Debug.Log("Player Start");
        Debug.Log(FindObjectOfType<GameManagerEvents>());
        FindObjectOfType<GameManagerEvents>().OnAllPlayersJoinedRoom -= OnAllPlayersJoinedRoom_Delegate;
        FindObjectOfType<GameManagerEvents>().OnAllPlayersJoinedRoom += OnAllPlayersJoinedRoom_Delegate;

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

        try
        {
            Debug.Log($"Trying to Assigning Player to Player Dict: {GameManager.instance.pid_player_Dict.Count}");
            Dictionary<int, Player> t = new Dictionary<int, Player>(GameManager.instance.pid_player_Dict);
            if (!t.ContainsKey(pid))
            {
                Debug.Log($"Player Dict does NOT contain Player {nickName}({pid})");
                t.Add(pid, this);
            }
            GameManager.instance.pid_player_Dict = t;
        }
        catch { }
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


        // Bug
        mainCamera.enabled = false;
        gunCamera.enabled = false;
        uiCamera.enabled = false;
    }
    private void Update()
    {
        HitPointsRecharge();
        OvershieldPointsRecharge();

        if (_gameStartDelay > 0)
        {
            _gameStartDelay -= Time.deltaTime;

            if (_gameStartDelay < 0)
            {
                mainCamera.enabled = false;
                mainCamera.enabled = true;

                gunCamera.enabled = false;
                gunCamera.enabled = true;

                uiCamera.enabled = false;
                uiCamera.enabled = true;
            }
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //Debug.Log("Player OnControllerColliderHit");
        float movementSpeedRatio = GetComponent<Movement>().playerSpeedPercent;
        Rigidbody rb = hit.collider.attachedRigidbody;
        if (rb && !rb.isKinematic)
        {
            Debug.Log(_pushForce);
            Debug.Log(movementSpeedRatio);
            rb.velocity = hit.moveDirection * _pushForce * movementSpeedRatio;
        }
    }

    void OnAllPlayersJoinedRoom_Delegate(GameManagerEvents gme)
    {
        Debug.Log("OnAllPlayersJoinedRoom_Delegate");

        _allPlayersJoined = true;
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

        int newHealth = (int)hitPoints - damage;
        PV.RPC("Damage_RPC", RpcTarget.All, newHealth, damage);
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
            PV.RPC("Damage_RPC", RpcTarget.All, newHealth, damage, source_pid, bytes, (int)dsn);
        }
    }

    // https://stackoverflow.com/questions/30294216/unity3d-c-sharp-vector3-as-default-parameter
    public void DropWeapon(WeaponProperties weapon, Vector3? offset = null)
    {
        if (!GetComponent<PhotonView>().IsMine || weapon.currentAmmo <= 0)
            return;

        if (weapon.codeName == null)
            return;

        WeaponProperties wp = null;
        int wi = 0;

        if (offset == null)
            offset = new Vector3(0, 0, 0);

        foreach (GameObject w in playerInventory.allWeaponsInInventory)
        {
            if (w.GetComponent<WeaponProperties>().codeName == weapon.codeName)
            {
                wp = w.GetComponent<WeaponProperties>();
                break;
            }
            wi++;
        }

        try
        {
            //GameObject wo = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/Weapons", wp.weaponRessource.name), weaponDropPoint.position + (Vector3)offset, Quaternion.identity);
            //wo.name = wo.name.Replace("(Clone)", "");
            ////wo.GetComponent<LootableWeapon>().UpdateSpawnPointPosition(weaponDropPoint.position);

            //wo.GetComponent<LootableWeapon>().ammo = wp.currentAmmo;
            //wo.GetComponent<LootableWeapon>().spareAmmo = wp.spareAmmo;
            //wo.GetComponent<LootableWeapon>().AddForce(weaponDropPoint.transform.forward);

            //wp.currentAmmo = 0;
            //wp.spareAmmo = 0;


            Dictionary<string, int> param = new Dictionary<string, int>();
            param["ammo"] = wp.currentAmmo;
            param["spareammo"] = wp.spareAmmo;
            Vector3 spp = weaponDropPoint.position + (Vector3)offset;
            Vector3 fDir = weaponDropPoint.transform.forward;

            wp.currentAmmo = 0;
            wp.spareAmmo = 0;

            NetworkGameManager.SpawnNetworkWeapon(wi, spp, fDir, param);
            //GetComponent<PhotonView>().RPC("DropWeapon_RPC", RpcTarget.All, wi, spp, fDir, param);
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

        Debug.Log("SpawnRagdoll");
        Debug.Log(deathByHeadshot);

        if (!deathByHeadshot)
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
        if (GameManager.instance.gameType == GameManager.GameType.Swat)
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

        if (isDead)
        {
            try
            {
                string sourcePlayerName = lastPlayerSource.nickName;

                int hsCode = KillFeedManager.killFeedSpecialCodeDict["headshot"];
                int nsCode = KillFeedManager.killFeedSpecialCodeDict["nutshot"];
                string youColorCode = KillFeedManager.killFeedColorCodeDict["blue"];
                string weaponColorCode = playerInventory.activeWeapon.ammoType.ToString().ToLower();




                foreach (KillFeedManager kfm in FindObjectsOfType<KillFeedManager>())
                {
                    string f = $"{lastPlayerSource.nickName} killed {nickName}";

                    if (_damageSource != null && _damageSource != "")
                    {
                        f = $"{lastPlayerSource.nickName} [ {_damageSource} ] {nickName}";
                    }
                    if (deathByHeadshot)
                        f += $" with a <color=\"red\">Headshot</color>!";

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
                DeathNature dn = DeathNature.None;

                if (deathByHeadshot)
                    dn = DeathNature.Headshot;

                if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
                {
                    //if(isMine)
                    //    PV.RPC("AddPlayerKill_RPC", RpcTarget.All, _lastPID, PV.ViewID, (int)_deathNature);
                    MultiplayerManager.instance.AddPlayerKill(new MultiplayerManager.AddPlayerKillStruct(_lastPID, PV.ViewID, _deathNature));

                }
                else if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
                    GetComponent<PlayerSwarmMatchStats>().deaths++;

            }
            catch (Exception e) { }

            try
            {
                Debug.Log("Processing medals");
                Debug.Log(deathNature);
                PlayerMedals sourcePlayerMedals = lastPlayerSource.playerMedals;
                if (sourcePlayerMedals != this.playerMedals)
                {
                    Debug.Log(_lastPID);
                    Debug.Log(this.pid);
                    if (deathNature == DeathNature.Headshot)
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
                        Debug.Log(_lastPID);
                        Debug.Log(this.pid);
                        if (_lastPID != this.pid)
                            sourcePlayerMedals.SpawnStuckKillMedal();
                    }
                    else
                    {
                        if (_lastPID != this.pid)
                            sourcePlayerMedals.kills++;
                    }

                    if (playerMedals.spree >= 3)
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
    void Damage_RPC(int newHealth, int damage)
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
    void Damage_RPC(int newHealth, int damage, int sourcePid, byte[] bytes, int dn)
    {
        if (hitPoints <= 0 || isRespawning || isDead)
            return;

        try { _damageSource = System.Text.Encoding.UTF8.GetString(bytes); } catch { }
        try { deathNature = (DeathNature)dn; } catch { }
        try { lastPID = sourcePid; } catch { }
        //try { this.impactPos = impactPos; this.impactDir = impactDir; } catch { }
        try { if (lastPlayerSource != this) lastPlayerSource.GetComponent<PlayerMultiplayerMatchStats>().damage += damage; } catch { }
        try { allPlayerScripts.damageIndicatorManager.SpawnNewDamageIndicator(sourcePid); } catch { }

        try { hitPoints = newHealth; } catch (Exception e) { GameManager.SendErrorEmailReport(e.ToString()); }


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

    [PunRPC]
    void IsDead_RPC()
    {
        isDead = true;
    }

    [PunRPC]
    void DropWeapon_RPC(int wi, Vector3 spp, Vector3 fDir, Dictionary<string, int> param)
    {
        Debug.Log("DropWeapon_RPC");
        GameObject wo = Instantiate(playerInventory.allWeaponsInInventory[wi].GetComponent<WeaponProperties>().weaponRessource, spp, Quaternion.identity);
        wo.name = wo.name.Replace("(Clone)", "");

        Debug.Log(spp);

        try { wo.GetComponent<LootableWeapon>().ammo = param["ammo"]; } catch (System.Exception e) { Debug.Log(e); }
        try { wo.GetComponent<LootableWeapon>().spareAmmo = param["spareammo"]; } catch (System.Exception e) { Debug.Log(e); }
        try { wo.GetComponent<LootableWeapon>().tts = param["tts"]; } catch (System.Exception e) { Debug.Log(e); }
        wo.GetComponent<LootableWeapon>().GetComponent<Rigidbody>().AddForce(fDir * 200);

        //StartCoroutine(UpdateWeaponSpawnPosition_Coroutine(wo, spp));
        wo.GetComponent<LootableWeapon>().spawnPointPosition = spp;
        Debug.Log($"DropWeapon_RPC: {wo.GetComponent<LootableWeapon>().spawnPointPosition}");

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
    void AddPlayerKill_RPC(int lpid, int tpid, int dni)
    {
        MultiplayerManager.instance.AddPlayerKill(new MultiplayerManager.AddPlayerKillStruct(lpid, tpid, (DeathNature)dni));
    }

    #endregion
    IEnumerator UpdateWeaponSpawnPosition_Coroutine(GameObject wo, Vector3 spp)
    {
        yield return new WaitForEndOfFrame();
        wo.GetComponent<LootableWeapon>().spawnPointPosition = spp;

    }
}
