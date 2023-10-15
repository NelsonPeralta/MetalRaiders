using ExitGames.Client.Photon;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

            if (nv > pv)
                return;

            _hitPoints = nv;

            if (nv < pv)
            {
                if (_flinchCooldown <= 0)
                    Flinch();
                //ChildOnActorDamaged();
            }

            if ((nv <= 0.5f * _defaultHitpoints) && (pv > 0.5f * _defaultHitpoints))
                try
                {
                    GetComponent<AudioSource>().clip = _tauntClip;
                    GetComponent<AudioSource>().Play();
                    _animator.Play("Taunt");
                }
                catch { }


            if (_hitPoints <= 0 && nv != pv)
            {
                //target = null; \\DO NOT REMOVE TARGET HERE
                DropRandomWeapon();
                ActorDie();
            }

        }
    }
    public Transform targetTransform
    {
        get { return _targetTransform; }
        set
        {
            if (_targetTransform != value)
            {
                //if (value)
                //    Debug.Log($"Target Transform set {value.name}");
                //else
                //    Debug.Log($"Target Transform set to NULL");

                _targetTransform = value;
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

    [SerializeField] protected int _hitPoints, _defaultHitpoints;
    [SerializeField] Transform _targetTransform;
    [SerializeField] HitPoints _targetPlayer;
    [SerializeField] Vector3 _destination;
    [SerializeField] Transform _losSpawn;

    [SerializeField] int _closeRange, _midRange, _longRange;
    [SerializeField] float _analyzeNextActionCooldown, _findNewTargetCooldown, _defaultFlinchCooldown, _lostTargetBipedStopwatch;
    [SerializeField] protected AudioClip _attackClip, _deathClip, _tauntClip, _hurtClip;
    [SerializeField] bool _oneShotHeadshot;


    protected NavMeshAgent _nma;
    protected FieldOfView _fieldOfView;
    protected Animator _animator;
    [SerializeField] protected bool isIdling, isRunning, isMeleeing, isTaunting, isFlinching, isShooting, isThrowing;
    protected List<ActorHitbox> _actorHitboxes = new List<ActorHitbox>();

    [SerializeField] protected float _flinchCooldown, _meleeCooldown, _shootProjectileCooldown, _throwExplosiveCooldown, _switchPlayerCooldown;
    [SerializeField] protected bool _isInRange;

    protected float _diffHpMult, _diffAttMult;

    private void Awake()
    {
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
            _actorHitboxes.Add(ah);
        }
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
    }

    private void OnEnable()
    {
        // See Prepare() for resetting hitpoints on Spawn
        _flinchCooldown = _defaultFlinchCooldown;
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

        try
        {
            GetComponent<PhotonView>().RPC("DamageActor", RpcTarget.All, damage, playerWhoShotPDI, damageSource, isHeadshot);
        }
        catch { }
    }


    void AnimationCheck()
    {
        // Not networked

        isIdling = _animator.GetCurrentAnimatorStateInfo(0).IsName("Idle");
        isMeleeing = _animator.GetCurrentAnimatorStateInfo(0).IsName("Melee");
        isRunning = _animator.GetCurrentAnimatorStateInfo(0).IsName("Run");
        isTaunting = _animator.GetCurrentAnimatorStateInfo(0).IsName("Taunt");
        isFlinching = _animator.GetCurrentAnimatorStateInfo(0).IsName("Flinch");
        isShooting = _animator.GetCurrentAnimatorStateInfo(0).IsName("Shoot");
        isThrowing = _animator.GetCurrentAnimatorStateInfo(0).IsName("Throw");
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
            if (targetTransform.GetComponent<Player>() && (isIdling || isMeleeing || isShooting))
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
                    wp.weaponType == WeaponProperties.WeaponType.DMR)
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
                            SetNewTargetPlayerWithPid(pid);
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

                if (_meleeCooldown <= 0 && !isFlinching)
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
                    int ran = UnityEngine.Random.Range(0, 6);

                    if (ran != 0)
                    {
                        if (_shootProjectileCooldown <= 0 && !isTaunting && !isFlinching)
                        {
                            Debug.Log("Throw Fireball to Player");

                            if (!targetTransform.GetComponent<HitPoints>())
                                targetTransform = null;
                            else
                                ShootProjectile(PhotonNetwork.InRoom);

                        }
                    }
                    else
                    {
                        if (_throwExplosiveCooldown <= 0 && !isTaunting && !isFlinching)
                        {
                            Debug.Log("Throw Fireball to Player");

                            if (!targetTransform.GetComponent<HitPoints>())
                                targetTransform = null;
                            else
                                ThrowExplosive(PhotonNetwork.InRoom);
                        }
                    }
                }
                else
                {
                    if (!isRunning && !isFlinching && !isTaunting)
                    {
                        Debug.Log("Chase Player");

                        Run(PhotonNetwork.InRoom);
                    }

                    if (isRunning && !isFlinching && !isTaunting)
                    {
                        nma.enabled = true;
                        nma.SetDestination(targetTransform.position);
                    }
                    else if (isFlinching || isTaunting)
                        nma.enabled = false;
                }
            }
            else if (distanceToTarget > longRange)
            {
                if (_isInRange)
                    _isInRange = false;

                if (!isRunning)
                {
                    //Debug.Log("Chase Player");
                    Run(PhotonNetwork.InRoom);
                }

                if (isRunning && !isFlinching && !isTaunting)
                {
                    nma.enabled = true;
                    nma.SetDestination(targetTransform.position);
                }
                else if (isFlinching || isTaunting)
                    nma.enabled = false;
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
        pp.GetComponent<PlayerSwarmMatchStats>().AddPoints(damage);

        //if (!targetPlayer)
        if (_switchPlayerCooldown <= 0)
        {
            targetTransform = pp.transform;
            targetHitpoints = pp.GetComponent<HitPoints>();

            _switchPlayerCooldown = 5;
        }

        hitPoints -= damage;
        if (hitPoints <= 0)
        {
            try
            {
                pp.GetComponent<PlayerSwarmMatchStats>().kills++;
                pp.playerMedals.kills++;
                //pp.GetComponent<PlayerSwarmMatchStats>().AddPoints(defaultHealth);

                //SpawnKillFeed(this.GetType().ToString(), playerWhoShotPDI, damageSource: damageSource, isHeadshot: isHeadshot);
            }
            catch { }
        }
    }

    [PunRPC]
    public void Flinch(bool callRPC = true)
    {
        if (callRPC)
        {
            GetComponent<PhotonView>().RPC("Flinch", RpcTarget.All, false);
        }
        else
        {
            try
            {
                GetComponent<AudioSource>().clip = _hurtClip;
                GetComponent<AudioSource>().Play();

                nma.enabled = false;
                _animator.Play("Flinch");
                _flinchCooldown = _defaultFlinchCooldown;
            }
            catch { }
        }
    }

    [PunRPC]
    public void SetNewTargetPlayerWithPid(int pid, bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("SetNewTargetPlayerWithPid", RpcTarget.AllViaServer, pid, false);
        }
        else
        {
            targetHitpoints = PhotonView.Find(pid).GetComponent<Player>().GetComponent<HitPoints>();
        }
    }


    [PunRPC]
    public void ActorDie(bool caller = true)
    {
        if (caller && PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC("ActorDie", RpcTarget.All, false);
        }
        else if (!caller)
        {
            GetComponent<AudioSource>().clip = _deathClip;
            GetComponent<AudioSource>().Play();

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
}
