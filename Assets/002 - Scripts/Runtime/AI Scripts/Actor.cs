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
                ActorDie();
                return;
            }


            if (nv < pv)
            {
                if (_flinchCooldown <= 0 && nv > 0)
                    Flinch();
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
                        _animator.Play("Boost");
                        _audioSource.clip = _tauntClip;
                        _audioSource.Play();
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


    [SerializeField] protected int _hitPoints, _defaultHitpoints;
    [SerializeField] Transform _targetTransform;
    [SerializeField] HitPoints _targetPlayer;
    [SerializeField] Vector3 _destination;
    [SerializeField] Transform _losSpawn;

    [SerializeField] int _closeRange, _midRange, _longRange;
    [SerializeField] float _analyzeNextActionCooldown, _findNewTargetCooldown, _defaultFlinchCooldown, _lostTargetBipedStopwatch;
    [SerializeField] protected AudioClip _attackClip, _deathClip, _tauntClip, _hurtClip, _gruntClip;
    [SerializeField] bool _oneShotHeadshot;


    protected NavMeshAgent _nma;
    protected FieldOfView _fieldOfView;
    protected Animator _animator;
    [SerializeField] protected bool isIdling, isRunning, isMeleeing, isTaunting, isFlinching, _isShooting, _isThrowingGrenade, isBoosting;
    protected List<ActorHitbox> _actorHitboxes = new List<ActorHitbox>();

    [SerializeField]
    protected float _flinchCooldown, _meleeCooldown, _shootProjectileCooldown, _throwExplosiveCooldown,
        _switchPlayerCooldown, _isDodgingCooldown, _isCurrentlyShootingCooldown, _isCurrentlyThrowingGrenadeCooldown;
    [SerializeField] protected bool _isInRange;
    [SerializeField] AudioSource _walkingAudioSource;


    [SerializeField] LayerMask _overlapSphereMask;
    [SerializeField] List<Transform> _leftChecks = new List<Transform>();
    [SerializeField] List<Transform> _rightChecks = new List<Transform>();

    protected float _diffHpMult, _diffAttMult, _gruntDelay, _defGruntDelay;
    AudioSource _audioSource;


    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _gruntDelay = _defGruntDelay = UnityEngine.Random.Range(4f, 8f);

        _diffHpMult = _diffAttMult = 1;

        try
        {

            if (GameManager.instance.difficulty == SwarmManager.Difficulty.Heroic)
            {
                _diffHpMult = _diffAttMult = 1.5f;
            }
            else if (GameManager.instance.difficulty == SwarmManager.Difficulty.Legendary)
            {
                _diffHpMult = _diffAttMult = 2f;
            }
        }
        catch (Exception e) { Debug.LogWarning(e); }

        _defaultHitpoints = _hitPoints;
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

        GameManager.instance.orSpPos_Biped_Dict.Add(transform.position, this); GameManager.instance.orSpPos_Biped_Dict = GameManager.instance.orSpPos_Biped_Dict;
    }



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
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
                    CalculateNextAction();
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

        gameObject.SetActive(true);
    }

    protected void Prepare()
    {
        _isDodgingCooldown = 0;
        _switchPlayerCooldown = 0;
        _isInRange = false;
        transform.position = new Vector3(0, -10, 0);
        _hitPoints = (int)(_defaultHitpoints * _diffHpMult);
        foreach (ActorHitbox hitbox in GetComponentsInChildren<ActorHitbox>(true))
            hitbox.gameObject.SetActive(true);
    }

    public void Damage(int damage, int playerWhoShotPDI, string damageSource = null, bool isHeadshot = false)
    {
        { // Hit Marker Handling
            Player p = GameManager.GetPlayerWithPhotonViewId(playerWhoShotPDI);

            if (hitPoints <= damage)
                p.GetComponent<PlayerUI>().SpawnHitMarker(PlayerUI.HitMarkerType.Kill);
            else
                p.GetComponent<PlayerUI>().SpawnHitMarker();
        }

        GetComponent<PhotonView>().RPC("DamageActor", RpcTarget.All, damage, playerWhoShotPDI, damageSource, isHeadshot);
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
        isBoosting = _animator.GetCurrentAnimatorStateInfo(0).IsName("Boost");
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
        int cap = 6;

        if (ChanceToDrop <= cap)
        {
            float ranAmmoFactor = UnityEngine.Random.Range(0.2f, 0.9f);
            float ranCapFactor = UnityEngine.Random.Range(0.3f, 0.5f);
            int randomWeaponInd = UnityEngine.Random.Range(0, GameManager.GetRootPlayer().playerInventory.allWeaponsInInventory.Length);

            try
            {
                WeaponProperties wp = GameManager.GetRootPlayer().playerInventory.allWeaponsInInventory[randomWeaponInd].GetComponent<WeaponProperties>();

                if (/*wp.weaponType == WeaponProperties.WeaponType.LMG ||*/
                    wp.weaponType == WeaponProperties.WeaponType.Launcher ||
                    wp.weaponType == WeaponProperties.WeaponType.Shotgun ||
                    wp.weaponType == WeaponProperties.WeaponType.Sniper ||
                    wp.weaponType == WeaponProperties.WeaponType.LMG)
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
                        if (pid > 0)
                            SetNewTargetPlayerWithPhotonId(pid);
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

        if (_isDodgingCooldown > -3 && hitPoints < _defaultHitpoints)
            _isDodgingCooldown -= Time.deltaTime;
    }


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
                    if (_isDodgingCooldown <= -2 && targetTransform.GetComponent<HitPoints>())
                    {
                        int l = UnityEngine.Random.Range(0, 2);


                        if (l == 1 && CheckIfSideIsClear())
                        {
                            Dodge(l);
                            return;
                        }
                        else if (l == 0 && CheckIfSideIsClear(true))
                        {
                            Dodge(l);
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
                                ShootProjectile(PhotonNetwork.InRoom);
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
                            else
                                ThrowExplosive(PhotonNetwork.InRoom);
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
    public void DamageActor(int damage, int playerWhoShotPDI, string damageSource = null, bool isHeadshot = false)
    {
        if (hitPoints <= 0)
            return;


        Player pp = GameManager.GetPlayerWithPhotonViewId(playerWhoShotPDI);
        //pp.GetComponent<PlayerSwarmMatchStats>().AddPoints(damage);

        //if (!targetPlayer)
        if (_switchPlayerCooldown <= 0)
        {
            targetTransform = pp.transform;
            targetHitpoints = pp.GetComponent<HitPoints>();

            _switchPlayerCooldown = 5;
        }

        Debug.Log($"DAMAGE ACTOR {hitPoints} -> {hitPoints - damage}");
        hitPoints -= damage;
        if (hitPoints <= 0)
        {
            try
            {
                pp.GetComponent<PlayerSwarmMatchStats>().kills++;
                pp.playerMedals.kills++;
                pp.GetComponent<PlayerSwarmMatchStats>().AddPoints(_defaultHitpoints * 8 + SwarmManager.instance.currentWave * 33);

                //SpawnKillFeed(this.GetType().ToString(), playerWhoShotPDI, damageSource: damageSource, isHeadshot: isHeadshot);
            }
            catch { }
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
    public void ActorDie(bool caller = true)
    {
        Debug.Log($"ActorDie");
        if (caller && PhotonNetwork.IsMasterClient)
        {
            Debug.Log($"ACTORD DIE CALL");
            GetComponent<PhotonView>().RPC("ActorDie", RpcTarget.All, false);
        }
        else if (!caller)
        {
            Debug.Log($"ACTORD DIE PROCESSING");
            _audioSource.clip = _deathClip;
            _audioSource.Play();

            foreach (ActorHitbox ah in GetComponentsInChildren<ActorHitbox>())
                ah.gameObject.SetActive(false);

            _animator.Play("Die");
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
        //print("CheckIfSideIsClear");
        if (!right)
        {
            //foreach (Collider c in Physics.OverlapSphere(_leftChecks[2].position, 1, _overlapSphereMask))
            //    print(c.name);

            if (Physics.OverlapSphere(_leftChecks[2].position, 1, _overlapSphereMask).Length > 0) return false;
        }
        else
        {
            //foreach (Collider c in Physics.OverlapSphere(_leftChecks[2].position, 1, _overlapSphereMask))
            //    print(c.name);

            if (Physics.OverlapSphere(_rightChecks[2].position, 1, _overlapSphereMask).Length > 0) return false;
        }


        return true;
    }


    [PunRPC]
    public void AssignPlayerOnBulletNearby(int playerId, bool callRPC = true)
    {
        //print("AssignPlayerOnBulletNearby");
        if (callRPC && PhotonNetwork.IsMasterClient)
        {
            if (_switchPlayerCooldown <= 0)
                GetComponent<PhotonView>().RPC("AssignPlayerOnBulletNearby", RpcTarget.AllViaServer, playerId, false);
        }
        else if (!callRPC)
        {
            print("AssignPlayerOnBulletNearby Processing");

            targetTransform = PhotonView.Find(playerId).GetComponent<Player>().transform;
            targetHitpoints = PhotonView.Find(playerId).GetComponent<Player>().GetComponent<HitPoints>();

            _switchPlayerCooldown = 5;
        }
    }
}
