using ExitGames.Client.Photon;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

[RequireComponent(typeof(FieldOfView))]
[RequireComponent(typeof(NavMeshAgent))]
abstract public class Actor : Biped
{
    public enum Action { Idle, Roam, Melee, Fireball, Grenade, Seek }

    public int pid { get { return GetComponent<PhotonView>().ViewID; } }
    public int hitPoints
    {
        get { return _hitPoints; }
        protected set
        {
            int pv = _hitPoints;
            int nv = Mathf.Clamp(value, 0, _defaultHitpoints * 3);
            Debug.Log($"LOCAL ACTOR HITPOINTS {pv} -> {nv}");

            if (nv > pv)
                return;

            _hitPoints = nv;




            if (_hitPoints <= 0 && nv != pv)
            {
                Debug.Log($"ACTORD DIE CALLING");
                //target = null; \\DO NOT REMOVE TARGET HERE
                DropRandomWeapon();
                ActorDie(playerWhoShotPDI: _lastPlayerPhotonIdWhoDamagedThis);
                return;
            }


            if (nv < pv)
            {
                _flinchThresholdCount += Mathf.Abs(pv - nv);
                //if (_flinchCooldown <= 0 && nv > 0)
                if (_flinchThresholdCount > _flinchThreshold && nv > 0)
                {

                    Flinch();
                }
                //ChildOnActorDamaged();
            }

            if ((nv <= 0.5f * _defaultHitpoints) && (pv > 0.5f * _defaultHitpoints))
                try
                {
                    //if (_isInRange)
                    //{
                    //    _audioSource.clip = _tauntClip;
                    //    _audioSource.Play();
                    //    _animator.Play("Taunt");
                    //}
                    //else
                    {
                        if (_flinchCooldown <= 0 && nv > 0)
                            Flinch();
                    }
                }
                catch { }




        }
    }
    public Transform targetTransform
    {
        get { return _targetTransform; }
        set
        {
            if (_targetTransform != value)
            {
                Transform pre = _targetTransform;
                _targetTransform = value;

                if (_targetTransform)
                    if (pre != _targetTransform && (_targetTransform.GetComponent<Player>() || _targetTransform.GetComponent<PlayerCapsule>()))
                    {
                        if (hitPoints > 0)
                        {
                            _isCurrentlyAlertingCooldown = 1;
                            _animator.Play("Boost");
                            _audioSource.clip = _tauntClip;
                            _audioSource.Play();
                        }
                    }
            }
        }
    }

    public HitPoints targetHitpoints
    {
        get { return _targetPlayer; }
        set
        {
            if (_targetPlayer != value)
            {
                //if (value)
                //    Debug.Log($"Target Player set {value.name}");
                //else
                //    Debug.Log($"Target Player set to NULL");

                HitPoints pre = _targetPlayer;

                _targetPlayer = value;

                if (_targetPlayer) print($"targetplayer changed: {value.name}");
            }
        }
    }

    public Vector3 destination { get { return _destination; } set { _destination = value; } }
    public Transform losSpawn { get { return _losSpawn; } set { _losSpawn = value; } }
    public virtual FieldOfView fieldOfView { get { return _fieldOfView; } private set { _fieldOfView = value; } }
    public NavMeshAgent nma { get { return _nma; } private set { _nma = value; } }
    public List<ActorHitbox> actorHitboxes { get { return _actorHitboxes; } }

    public int longRange { get { return _longRange; } }
    public int midRange { get { return _midRange; } }
    public int closeRange { get { return _closeRange; } }
    public bool oneShotHeadshot { get { return _oneShotHeadshot; } }
    public bool isDodging { get { return _isDodgingCooldown > 0; } }
    public bool isShooting { get { return _isCurrentlyShootingCooldown > 0; } }
    public bool isThrowingGrenade { get { return _isCurrentlyThrowingGrenadeCooldown > 0; } }
    public bool isSeenByTargetPlayer { get { return _isSeenByTargetPlayerCooldown > 0; } }
    public bool isBoosting { get { return _isCurrentlyAlertingCooldown > 0; } }


    [SerializeField] protected int _hitPoints, _defaultHitpoints;
    [SerializeField] Transform _targetTransform;
    [SerializeField] HitPoints _targetPlayer;
    [SerializeField] Vector3 _destination;
    [SerializeField] Transform _losSpawn;
    [SerializeField] ReticuleFriction _friction;

    [SerializeField] int _closeRange, _midRange, _longRange;
    [SerializeField] float _analyzeNextActionCooldown, _findNewTargetCooldown, _defaultFlinchCooldown, _lostTargetBipedStopwatch;
    [SerializeField] protected AudioClip _attackClip, _deathClip, _tauntClip, _hurtClip, _gruntClip;
    [SerializeField] bool _oneShotHeadshot;


    protected NavMeshAgent _nma;
    protected FieldOfView _fieldOfView;
    protected Animator _animator;
    [SerializeField] protected bool isIdling, isRunning, isMeleeing, isTaunting, isFlinching, _isShooting, _isThrowingGrenade;
    protected List<ActorHitbox> _actorHitboxes = new List<ActorHitbox>();

    [SerializeField]
    protected float _flinchCooldown, _meleeCooldown, _shootProjectileCooldown, _throwExplosiveCooldown,
        _switchPlayerCooldown, _isDodgingCooldown, _isCurrentlyShootingCooldown, _isCurrentlyThrowingGrenadeCooldown,
        _isSeenByTargetPlayerCooldown, _isCurrentlyAlertingCooldown;
    [SerializeField] protected bool _isInRange;
    [SerializeField] AudioSource _walkingAudioSource;


    [SerializeField] LayerMask _overlapSphereMask;
    [SerializeField] List<Transform> _leftChecks = new List<Transform>();
    [SerializeField] List<Transform> _rightChecks = new List<Transform>();
    [SerializeField] Explosion _ultraMergeExPrefab;

    [SerializeField] protected int _flinchThreshold, _lastPlayerPhotonIdWhoDamagedThis;

    [SerializeField] float _flinchThresholdCount;

    protected float _diffHpMult, _diffAttMult, _gruntDelay, _defGruntDelay;
    AudioSource _audioSource;


    private void Awake()
    {
        if (_flinchThreshold == 0) _flinchThreshold = 100;

        _audioSource = GetComponent<AudioSource>();
        _gruntDelay = _defGruntDelay = UnityEngine.Random.Range(4f, 8f);


        _defaultHitpoints = _hitPoints;
        if (GameManager.instance.difficulty == SwarmManager.Difficulty.Heroic) _defaultHitpoints = (int)(_defaultHitpoints * 1.4f);
        if (GameManager.instance.difficulty == SwarmManager.Difficulty.Legendary) _defaultHitpoints = (int)(_defaultHitpoints * 1.8f);


        _analyzeNextActionCooldown = _findNewTargetCooldown = 0.5f;

        _animator = GetComponent<Animator>();
        _fieldOfView = GetComponent<FieldOfView>();
        _nma = GetComponent<NavMeshAgent>();

        if (_closeRange <= 0)
            _closeRange = 3;

        if (_midRange <= 0)
            _midRange = 12;

        if (_longRange <= 0)
            _longRange = 20;

        foreach (ActorHitbox ah in GetComponentsInChildren<ActorHitbox>())
        {
            ah.actor = this;
            ah.biped = this;
            _actorHitboxes.Add(ah);
        }

        ChildAwake();
        transform.parent = SwarmManager.instance.transform;
        gameObject.SetActive(false);

        print($"Adding {name} {transform.position} to instantiation_position_Biped_Dict");
        originalSpawnPosition = transform.position;
        GameManager.instance.instantiation_position_Biped_Dict.Add(transform.position, this); GameManager.instance.instantiation_position_Biped_Dict = GameManager.instance.instantiation_position_Biped_Dict;
    }



    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.instance.difficulty == SwarmManager.Difficulty.Heroic) _flinchThreshold = (int)(_flinchThreshold * 1.5f);
        if (GameManager.instance.difficulty == SwarmManager.Difficulty.Legendary) _flinchThreshold = (int)(_flinchThreshold * 2);
    }

    // Update is called once per frame
    void Update()
    {
        if (_flinchThresholdCount > 0) _flinchThresholdCount -= Time.deltaTime * 3;


        TargetStateCheck();
        LostTarget();

        AnimationCheck();
        ActionCooldowns();

        if (hitPoints > 0)
            if (_analyzeNextActionCooldown > 0)
            {
                _analyzeNextActionCooldown -= Time.deltaTime;

                if (_analyzeNextActionCooldown <= 0)
                {
                    if (!SwarmManager.instance.editMode) CalculateNextAction();
                    _analyzeNextActionCooldown = 0.3f;
                }
            }

        LookAtTarget();
        FindNewTarget();
        PlayGruntClip();
        PlayWalkingSound();
    }

    private void OnEnable()
    {
        // See Prepare() for resetting hitpoints on Spawn
        _flinchCooldown = _defaultFlinchCooldown;
        _isDodgingCooldown = 0;
        _switchPlayerCooldown = 0;
        _isInRange = false;
        ChildOnEnable();
    }

    void OnPlayerDeath(Player p)
    {
        //Debug.Log("OnPlayerDeath");
        //target = null;

        //p.OnPlayerDeath -= OnPlayerDeath;
    }


    public void Spawn(int targetPhotonId, Vector3 spawnPointPosition, Quaternion spawnPointRotation)
    {
        Prepare();

        transform.position = spawnPointPosition;
        transform.rotation = spawnPointRotation;

        //if (targetPhotonId > 0)
        //    targetTransform = PhotonView.Find(targetPhotonId).transform;

        _friction.gameObject.SetActive(true);
        gameObject.SetActive(true);
        SwarmManager.instance.actorsAliveList.Add(this.GetComponent<Biped>());

        foreach (Player p in GameManager.instance.GetAllPhotonPlayers())
            if (p && p.isMine)
                p.SetupMotionTracker();
    }

    protected void Prepare()
    {
        _lastPlayerPhotonIdWhoDamagedThis = -1;
        _ultraMergeCount = 0;
        _isSeenByTargetPlayerCooldown = _isDodgingCooldown = _isCurrentlyAlertingCooldown = 0;
        _switchPlayerCooldown = 0;
        _isInRange = false;
        transform.position = new Vector3(0, -10, 0);
        _hitPoints = (int)(_defaultHitpoints);
        foreach (ActorHitbox hitbox in GetComponentsInChildren<ActorHitbox>(true))
            hitbox.gameObject.SetActive(true);
    }

    public void Damage(int damage, int playerWhoShotPDI, string damageSource = null, bool isHeadshot = false, int weIndx = -1)
    {
        print($"Actor Damage: {damageSource}");
        //{ // Hit Marker Handling
        //    Player p = GameManager.GetPlayerWithPhotonView(playerWhoShotPDI);

        //    if (hitPoints <= damage)
        //        p.GetComponent<PlayerUI>().SpawnHitMarker(PlayerUI.HitMarkerType.Kill);
        //    else
        //        p.GetComponent<PlayerUI>().SpawnHitMarker();
        //}

        GetComponent<PhotonView>().RPC("DamageActor", RpcTarget.AllViaServer, damage, playerWhoShotPDI, damageSource, isHeadshot, weIndx);
    }

    void PlayGruntClip()
    {
        if (_gruntDelay > 0)
        {
            _gruntDelay -= Time.deltaTime;

            if (_gruntDelay <= 0)
            {
                try
                {
                    if (!_audioSource.isPlaying)
                    {
                        _audioSource.clip = _gruntClip;
                        _audioSource.Play();
                        _gruntDelay = _defGruntDelay;
                    }
                }
                catch
                {

                }
            }
        }
    }

    void PlayWalkingSound()
    {
        _walkingAudioSource.gameObject.SetActive(isRunning);
    }

    void AnimationCheck()
    {
        // Not networked

        isIdling = _animator.GetCurrentAnimatorStateInfo(0).IsName("Idle");
        isMeleeing = _animator.GetCurrentAnimatorStateInfo(0).IsName("Melee");
        isRunning = _animator.GetCurrentAnimatorStateInfo(0).IsName("Run");
        isTaunting = _animator.GetCurrentAnimatorStateInfo(0).IsName("Taunt");
        isFlinching = _animator.GetCurrentAnimatorStateInfo(0).IsName("Flinch");
        //isShooting = _animator.GetCurrentAnimatorStateInfo(0).IsName("Shoot");
        //isThrowing = _animator.GetCurrentAnimatorStateInfo(0).IsName("Throw");
    }

    void TargetStateCheck()
    {
        // Not networked

        if (hitPoints > 0)
            if (targetTransform)
                if (targetTransform.GetComponent<Player>())
                    if (targetTransform.GetComponent<Player>().isRespawning || targetTransform.GetComponent<Player>().isDead)
                        targetTransform = null;
    }
    protected void LookAtTarget()
    {
        // If we are in a room and we are not the Host, stop
        if (PhotonNetwork.InRoom)
            if (!PhotonNetwork.IsMasterClient)
                return;

        try
        {
            if (targetTransform.GetComponent<Player>() && (isIdling || isMeleeing || isShooting || isBoosting))
            {
                Vector3 targetPostition = new Vector3(targetTransform.position.x,
                                                    this.transform.position.y,
                                                    targetTransform.position.z);
                this.transform.LookAt(targetPostition);
            }
        }
        catch { }
    }

    void DropRandomWeapon()
    {

        // If we are in a room and we are not the Host, stop
        if (PhotonNetwork.InRoom) if (!PhotonNetwork.IsMasterClient) return;

        int ChanceToDrop = UnityEngine.Random.Range(0, 10);
        int cap = 7;

        if (ChanceToDrop <= cap)
        {
            float ranAmmoFactor = UnityEngine.Random.Range(0.2f, 0.9f);
            float ranCapFactor = UnityEngine.Random.Range(0.3f, 0.5f);
            int randomWeaponInd = UnityEngine.Random.Range(0, GameManager.GetRootPlayer().playerInventory.allWeaponsInInventory.Length);

            try
            {
                WeaponProperties wp = GameManager.GetRootPlayer().playerInventory.allWeaponsInInventory[randomWeaponInd].GetComponent<WeaponProperties>();

                {// Splinter is too strong. Check 2x before truly spawning one
                    if (wp.killFeedOutput == WeaponProperties.KillFeedOutput.Splinter)
                    {
                        randomWeaponInd = UnityEngine.Random.Range(0, GameManager.GetRootPlayer().playerInventory.allWeaponsInInventory.Length);
                        wp = GameManager.GetRootPlayer().playerInventory.allWeaponsInInventory[randomWeaponInd].GetComponent<WeaponProperties>();
                    }
                    if (wp.killFeedOutput == WeaponProperties.KillFeedOutput.Splinter)
                    {
                        randomWeaponInd = UnityEngine.Random.Range(0, GameManager.GetRootPlayer().playerInventory.allWeaponsInInventory.Length);
                        wp = GameManager.GetRootPlayer().playerInventory.allWeaponsInInventory[randomWeaponInd].GetComponent<WeaponProperties>();
                    }
                }



                if (/*wp.weaponType == WeaponProperties.WeaponType.LMG ||*/
                    wp.weaponType == WeaponProperties.WeaponType.Launcher ||
                    wp.weaponType == WeaponProperties.WeaponType.Shotgun ||
                    wp.weaponType == WeaponProperties.WeaponType.Sniper ||
                    wp.weaponType == WeaponProperties.WeaponType.LMG || wp.killFeedOutput == WeaponProperties.KillFeedOutput.Sword)
                    return;
                Debug.Log($"DropRandomWeapon: {wp.cleanName}");




                Vector3 spp = transform.position + new Vector3(0, 1, 0);
                Vector3 fDir = losSpawn.transform.forward + new Vector3(0, 1f, 0);

                GameObject[] weapInv = GameManager.GetRootPlayer().playerInventory.allWeaponsInInventory;
                NetworkGameManager.SpawnNetworkWeapon(weapInv[randomWeaponInd].GetComponent<WeaponProperties>(),
                spp, fDir, currAmmo: (int)(wp.ammoCapacity * ranAmmoFactor), spareAmmo: (int)(wp.maxSpareAmmo * ranCapFactor));
            }
            catch { }
        }
    }

    void FindNewTarget()
    {
        // If we are in a room and we are not the Host, stop
        if (PhotonNetwork.InRoom) if (!PhotonNetwork.IsMasterClient) return;

        if (_findNewTargetCooldown > 0)
        {
            _findNewTargetCooldown -= Time.deltaTime;

            if (_findNewTargetCooldown <= 0)
            {
                if (!targetTransform && hitPoints > 0)
                {
                    Debug.Log("Finding new Target Transform");

                    try
                    {
                        Transform tp = MapAiWaypoints.instance.GetRandomWaypoint();


                        while (targetTransform == tp)
                        {
                            tp = MapAiWaypoints.instance.GetRandomWaypoint();
                        }

                        targetTransform = tp;
                    }
                    catch
                    {

                    }

                    if (NetworkSwarmManager.instance)
                    {
                        int pid = NetworkSwarmManager.instance.GetRandomAlivePlayerPhotonId();
                        if (pid > 0) SetNewTargetPlayerWithPhotonId(pid);
                    }
                }

                _findNewTargetCooldown = 2;
            }
        }
    }



    IEnumerator Hide()
    {
        yield return new WaitForSeconds(5);
        gameObject.SetActive(false);
    }

    void LostTarget()
    {
        try
        {
            if (!_fieldOfView.canSeePlayer && targetHitpoints.transform == targetTransform && _isInRange)
                _lostTargetBipedStopwatch = Mathf.Clamp(_lostTargetBipedStopwatch + Time.deltaTime, 0, 3);
            else if (_lostTargetBipedStopwatch > 0)
                _lostTargetBipedStopwatch = Mathf.Clamp(_lostTargetBipedStopwatch - Time.deltaTime, 0, 3);
        }
        catch { }

        if (_lostTargetBipedStopwatch >= 3)
        {
            // This will trigger FindNewTarget()
            targetTransform = null;
            targetHitpoints = null;
            //FindNewTarget(true);
        }
    }

    void ActionCooldowns()
    {
        // Not networked

        _isShooting = isShooting; _isThrowingGrenade = isThrowingGrenade;

        if (_isSeenByTargetPlayerCooldown > 0)
            _isSeenByTargetPlayerCooldown -= Time.deltaTime;

        if (_isCurrentlyAlertingCooldown > 0)
            _isCurrentlyAlertingCooldown -= Time.deltaTime;

        if (_isCurrentlyShootingCooldown > 0 && hitPoints < _defaultHitpoints)
            _isCurrentlyShootingCooldown -= Time.deltaTime;

        if (_isCurrentlyThrowingGrenadeCooldown > 0 && hitPoints < _defaultHitpoints)
            _isCurrentlyThrowingGrenadeCooldown -= Time.deltaTime;

        if (_meleeCooldown > 0)
            _meleeCooldown -= Time.deltaTime;

        if (_shootProjectileCooldown > 0)
            _shootProjectileCooldown -= Time.deltaTime;

        if (_throwExplosiveCooldown > 0)
            _throwExplosiveCooldown -= Time.deltaTime;

        if (_flinchCooldown > 0 && hitPoints < _defaultHitpoints)
            _flinchCooldown -= Time.deltaTime;

        if (_switchPlayerCooldown > 0 && hitPoints < _defaultHitpoints)
            _switchPlayerCooldown -= Time.deltaTime;

        if (_isDodgingCooldown > -5 && hitPoints < _defaultHitpoints)
            _isDodgingCooldown -= Time.deltaTime;
    }


    /// <summary>
    /// ///////////////////////////////////////// CalculateNextAction
    /// </summary>
    void CalculateNextAction()
    {
        // If we are in a room and we are not the Host, stop
        if (PhotonNetwork.InRoom) if (!PhotonNetwork.IsMasterClient) return;

        // This is where the Nav Mesh Agent is mainly controlled

        if (targetTransform)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);
            if (distanceToTarget <= closeRange)
            {
                nma.enabled = false;

                if (_meleeCooldown <= 0 && !isFlinching && !isBoosting && !isDodging)
                {
                    print($"trying to melee {targetTransform.name}");
                    if (targetTransform.GetComponent<Player>() || targetTransform.root.GetComponent<Player>())
                        Melee();
                }
                else
                {

                }
            }
            else if (distanceToTarget > closeRange && distanceToTarget <= longRange)
            {
                if (distanceToTarget > closeRange && distanceToTarget <= midRange)
                {
                    if (!_isInRange)
                        _isInRange = true;
                }


                if (_isInRange)
                {
                    //print($"{_isDodgingCooldown} {targetTransform.GetComponent<HitPoints>()} {CheckIfSideIsClear()} {isSeenByTargetPlayer}");

                    if (_isDodgingCooldown <= -2 && targetTransform.GetComponent<HitPoints>())
                    {
                        int l = UnityEngine.Random.Range(0, 2);


                        if (l == 1 && CheckIfSideIsClear() && isSeenByTargetPlayer)
                        {
                            if (!SwarmManager.instance.editMode) Dodge(l);
                            return;
                        }
                        else if (l == 0 && CheckIfSideIsClear(true) && isSeenByTargetPlayer)
                        {
                            if (!SwarmManager.instance.editMode) Dodge(l);
                            return;
                        }
                    }



                    int ran = UnityEngine.Random.Range(0, 6);
                    //print($"{_shootProjectileCooldown} {isTaunting} {isFlinching} {isBoosting} {isDodging} {ran}");

                    if (ran != 0)
                    {
                        if (_shootProjectileCooldown <= 0 && !isTaunting && !isFlinching && !isBoosting && !isDodging)
                        {

                            if (!targetTransform.GetComponent<HitPoints>())
                                targetTransform = null;
                            else
                            {
                                //print("Throw Fireball to Player");
                                if (!SwarmManager.instance.editMode) ShootProjectile(PhotonNetwork.InRoom);
                            }

                        }
                    }
                    else
                    {
                        if (_throwExplosiveCooldown <= 0 && !isTaunting && !isFlinching && !isBoosting && !isDodging)
                        {
                            //print("Throw Fireball to Player");

                            if (!targetTransform.GetComponent<HitPoints>())
                                targetTransform = null;
                            else if (SwarmManager.instance.globalActorGrenadeCooldown <= 0)
                            {
                                if (!SwarmManager.instance.editMode) ThrowExplosive(PhotonNetwork.InRoom);
                            }
                            else
                            {
                                if (!SwarmManager.instance.editMode) ShootProjectile(PhotonNetwork.InRoom);
                            }
                        }
                    }
                }
                else
                {
                    if (!isRunning && !isFlinching && !isTaunting && !isBoosting && !isDodging)
                    {
                        //Debug.Log("Chase Player");

                        Run(PhotonNetwork.InRoom);
                    }

                    if (isRunning && !isFlinching && !isTaunting && !isBoosting && !isDodging)
                    {
                        //Debug.Log("Chase Player");
                        nma.enabled = true;
                        nma.SetDestination(targetTransform.position);
                    }
                    else if (isFlinching || isTaunting || isBoosting || isDodging)
                    {
                        //print("disabling NavMeshAgent");
                        nma.enabled = false;
                    }
                }
            }
            else if (distanceToTarget > longRange)
            {
                if (_isInRange)
                    _isInRange = false;

                //Debug.Log($"Out of range {isFlinching} {isTaunting} {isBoosting} {isDodging} {_isDodgingCooldown}");


                if (!isRunning)
                {
                    //Debug.Log("Chase Player");
                    Run(PhotonNetwork.InRoom);
                }

                if (isRunning && !isFlinching && !isTaunting && !isBoosting && !isDodging)
                {
                    //print($"Going to waypoint {targetTransform.name}");
                    nma.enabled = true;
                    nma.SetDestination(targetTransform.position);
                }
                else if (isFlinching || isTaunting || isBoosting || isDodging)
                {
                    //print("disabling NavMeshAgent");
                    nma.enabled = false;
                }
            }


        }
        else // Stop Chasing
        {
            if (hitPoints > 0)
                if (!isIdling)
                    Idle(PhotonNetwork.InRoom);
            //nma.isStopped = true;
        }
    }



    protected abstract void ChildAwake();

    public abstract void Idle(bool callRPC = true);
    public abstract void Run(bool callRPC = true);
    public abstract void Melee(bool callRPC = true);
    public abstract void ShootProjectile(bool callRPC = true);
    public abstract void ThrowExplosive(bool callRPC = true);




    protected virtual void ChildOnActorDamaged() { }
    protected virtual void ChildOnEnable() { }









    [PunRPC]
    public void DamageActor(int damage, int playerWhoShotPDI, string damageSource = null, bool isHeadshot = false, int weapIndx = -1)
    {
        if (hitPoints <= 0)
            return;

        if (playerWhoShotPDI > 0) _lastPlayerPhotonIdWhoDamagedThis = playerWhoShotPDI;

        if (weapIndx == 555)
            SoundManager.instance.PlayAudioClip(transform.position, SoundManager.instance.successfulPunch);



        Player pp = GameManager.GetPlayerWithPhotonView(playerWhoShotPDI);
        pp.PlayShootingEnemyClip();
        //pp.GetComponent<PlayerSwarmMatchStats>().AddPoints(damage);

        //if (!targetPlayer)
        if (_switchPlayerCooldown <= 0)
        {
            targetTransform = pp.transform;
            targetHitpoints = pp.GetComponent<HitPoints>();

            _switchPlayerCooldown = 5;
        }

        Debug.Log($"DAMAGE ACTOR {hitPoints} -> {hitPoints - damage} {damageSource} {weapIndx}");
        hitPoints -= damage;

        if (hitPoints > 0)
        {
            if (weapIndx >= 0)
            {
                if (GameManager.GetRootPlayer().playerInventory.allWeaponsInInventory[weapIndx].GetComponent<WeaponProperties>().ultraBind)
                    ultraMergeCount++;
            }

            pp.GetComponent<PlayerUI>().SpawnHitMarker();
        }
    }

    [PunRPC]
    public void Flinch(bool callRPC = true)
    {
        Debug.Log($"Flinch");
        if (callRPC && PhotonNetwork.IsMasterClient)
        {
            if (!isBoosting && !isDodging)
            {
                Debug.Log($"ACTORD FLINCH CALL");
                GetComponent<PhotonView>().RPC("Flinch", RpcTarget.All, false);
            }
        }
        else if (!callRPC)
        {
            Debug.Log($"ACTORD FLINCH CALL PROCESSING");
            _flinchThresholdCount = 0;
            _audioSource.clip = _hurtClip;
            _audioSource.Play();

            nma.enabled = false;
            _animator.Play("Flinch");
            _flinchCooldown = _defaultFlinchCooldown;
        }
    }

    [PunRPC]
    public void SetNewTargetPlayerWithPhotonId(int pid, bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("SetNewTargetPlayerWithPhotonId", RpcTarget.AllViaServer, pid, false);
        }
        else
        {
            print($"SetNewTargetPlayerWithPid {pid}");
            targetHitpoints = PhotonView.Find(pid).GetComponent<Player>().GetComponent<HitPoints>();
        }
    }


    [PunRPC]
    public void ActorDie(bool caller = true, int playerWhoShotPDI = -1)
    {
        Debug.Log($"ActorDie");
        if (caller && PhotonNetwork.IsMasterClient)
        {
            Debug.Log($"ACTORD DIE CALL");
            GetComponent<PhotonView>().RPC("ActorDie", RpcTarget.All, false, playerWhoShotPDI);
        }
        else if (!caller)
        {
            if (playerWhoShotPDI > 0)
            {
                GameManager.GetPlayerWithPhotonView(playerWhoShotPDI).GetComponent<PlayerSwarmMatchStats>().kills++;
                GameManager.GetPlayerWithPhotonView(playerWhoShotPDI).playerMedals.kills++;
                GameManager.GetPlayerWithPhotonView(playerWhoShotPDI).GetComponent<PlayerSwarmMatchStats>().AddPoints(_defaultHitpoints * 8 + SwarmManager.instance.currentWave * 33);
                GameManager.GetPlayerWithPhotonView(playerWhoShotPDI).playerUI.ShowPointWitness(_defaultHitpoints * 8 + SwarmManager.instance.currentWave * 33);
                GameManager.GetPlayerWithPhotonView(playerWhoShotPDI).PlayEnemyDownClip();
                GameManager.GetPlayerWithPhotonView(playerWhoShotPDI).playerUI.SpawnHitMarker(PlayerUI.HitMarkerType.Kill);
            }





            Debug.Log($"ACTORD DIE PROCESSING");
            _animator.Play("Die");

            GetComponent<HitPoints>().OnDeath?.Invoke(GetComponent<HitPoints>());
            _friction.gameObject.SetActive(false);
            _audioSource.clip = _deathClip;
            _audioSource.Play();

            foreach (ActorHitbox ah in GetComponentsInChildren<ActorHitbox>())
                ah.gameObject.SetActive(false);

            nma.enabled = false;
            SwarmManager.instance.InvokeOnAiDeath();
            StartCoroutine(Hide());
            targetTransform = null;
            targetHitpoints = null;
        }
    }

    [PunRPC]
    public void Dodge(int left, bool callRPC = true)
    {
        //print("Dodge RPC");

        if (callRPC && PhotonNetwork.IsMasterClient)
        {
            //print("Dodge RPC call");
            GetComponent<PhotonView>().RPC("Dodge", RpcTarget.All, left, false);
        }
        else if (!callRPC)
        {
            //print("Dodge RPC processing");
            _isDodgingCooldown = 1;

            if (GameManager.instance.difficulty == SwarmManager.Difficulty.Heroic) _isDodgingCooldown = 0.9f;
            if (GameManager.instance.difficulty == SwarmManager.Difficulty.Legendary) _isDodgingCooldown = 0.8f;



            if (left == 1)
            {
                //_audioSource.clip = _hurtClip;
                //_audioSource.Play();

                //nma.enabled = false;
                _animator.Play("dodge_l");
                //_isDodgingCooldown = _defaultFlinchCooldown;
            }
            else if (left == 0)
            {
                _animator.Play("dodge_r");
            }
        }
    }


    public static bool CanAgentReachDestination(NavMeshAgent nma, Vector3 pos)
    {
        NavMeshPath navMeshPath = new NavMeshPath();
        if (nma.CalculatePath(pos, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
            return true;

        return false;
    }

    bool CheckIfSideIsClear(bool right = false)
    {
        print("CheckIfSideIsClear");
        if (!right)
        {
            foreach (Collider c in Physics.OverlapSphere(_leftChecks[2].position, 1, _overlapSphereMask))
                print(c.name);

            if (Physics.OverlapSphere(_leftChecks[2].position, 1, _overlapSphereMask).Length > 0) return false;
        }
        else
        {
            foreach (Collider c in Physics.OverlapSphere(_leftChecks[2].position, 1, _overlapSphereMask))
                print(c.name);

            if (Physics.OverlapSphere(_rightChecks[2].position, 1, _overlapSphereMask).Length > 0) return false;
        }


        return true;
    }

    public void ResetSeenCooldown(Player p)
    {
        if (p.GetComponent<HitPoints>() == targetHitpoints)
            _isSeenByTargetPlayerCooldown = SpawnPoint.SeenResetTime;
    }

    public override void SpawnUltraBindExplosion()
    {
        base.SpawnUltraBindExplosion();

        print("Actor SpawnUltraBindExplosion");
        GrenadePool.SpawnExplosion(_targetPlayer.GetComponent<Player>(), damage: 999, radius: 2, GameManager.DEFAULT_EXPLOSION_POWER, damageCleanNameSource: "Ultra Bind", targetTrackingCorrectTarget.position, Explosion.Color.Purple, Explosion.Type.UltraBind, GrenadePool.instance.ultraBindClip, WeaponProperties.KillFeedOutput.Ultra_Bind);



        //Explosion e = Instantiate(_ultraMergeExPrefab, transform.position, Quaternion.identity).GetComponent<Explosion>();
        //e.player = targetHitpoints.GetComponent<Player>();
        //e.gameObject.SetActive(true);
        //e.DisableIn3Seconds();


        //_ultraMergeCount = 0;
    }


    [PunRPC]
    public void AssignPlayerOnBulletNearby(int playerPhotonId, bool callRPC = true)
    {
        //print("AssignPlayerOnBulletNearby");
        if (callRPC && PhotonNetwork.IsMasterClient)
        {
            if (_switchPlayerCooldown <= 0 && hitPoints > 0)
                GetComponent<PhotonView>().RPC("AssignPlayerOnBulletNearby", RpcTarget.AllViaServer, playerPhotonId, false);
        }
        else if (!callRPC)
        {
            print("AssignPlayerOnBulletNearby Processing");

            targetTransform = PhotonView.Find(playerPhotonId).GetComponent<Player>().transform;
            targetHitpoints = PhotonView.Find(playerPhotonId).GetComponent<Player>().GetComponent<HitPoints>();

            _switchPlayerCooldown = 5;
        }
    }
}
