using ExitGames.Client.Photon;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(FieldOfView))]
[RequireComponent(typeof(NavMeshAgent))]
abstract public class Actor : MonoBehaviour
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
                ChildOnActorDamaged();

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
                ActorDeath();
            }

        }
    }
    public Transform targetTransform
    {
        get { return _targetPosition; }
        set
        {
            _targetPosition = value;

            //if (_targetPosition)
            //    if (_targetPosition.GetComponent<Player>())
            //    {
            //        _targetPosition.GetComponent<Player>().OnPlayerDeath += OnPlayerDeath;
            //        _targetPlayer = _targetPosition.GetComponent<Player>();
            //    }
            //    else
            //    {
            //        try { _targetPosition.GetComponent<Player>().OnPlayerDeath -= OnPlayerDeath; } catch { }
            //        try { _targetPlayer = _targetPosition.GetComponent<Player>(); } catch { }
            //    }
        }
    }

    public Player targetPlayer
    {
        get { return _targetPlayer; }
        set
        {
            _targetPlayer = value;
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

    [SerializeField] protected int _hitPoints;
    [SerializeField] Transform _targetPosition;
    [SerializeField] Player _targetPlayer;
    [SerializeField] Vector3 _destination;
    [SerializeField] Transform _losSpawn;

    [SerializeField] int _closeRange, _midRange, _longRange;
    [SerializeField] float _analyzeNextActionCooldown, _findNewTargetCooldown;
    [SerializeField] protected AudioClip _attackClip, _deathClip, _tauntClip;
    [SerializeField] bool _oneShotHeadshot;


    protected NavMeshAgent _nma;
    protected FieldOfView _fieldOfView;
    protected Animator _animator;
    [SerializeField] protected int _defaultHitpoints;
    protected bool isIdling, isRunning, isMeleeing, isTaunting, isFlinching;
    protected List<ActorHitbox> _actorHitboxes = new List<ActorHitbox>();

    [SerializeField] protected float _flinchCooldown;

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

        AnimationCheck();
        CooldownsUpdate();

        if (hitPoints > 0)
            if (_analyzeNextActionCooldown > 0)
            {
                _analyzeNextActionCooldown -= Time.deltaTime;

                if (_analyzeNextActionCooldown <= 0)
                {
                    if (PhotonNetwork.InRoom)
                        if (PhotonNetwork.IsMasterClient)
                        {
                            AnalyzeNextAction();
                        }
                        else;
                    else
                        AnalyzeNextAction();

                    _analyzeNextActionCooldown = 0.3f;
                }
            }

        LookAtTarget();
        FindNewTarget();
    }

    private void OnEnable()
    {
        // See Prepare() for resetting hitpoints on Spawn
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

        if (targetPhotonId > 0)
            targetTransform = PhotonView.Find(targetPhotonId).transform;

        gameObject.SetActive(true);
    }

    protected void Prepare()
    {
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
        //currentAnimatorClipName = _animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            isIdling = true;
        else
            isIdling = false;

        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Melee"))
            isMeleeing = true;
        else
            isMeleeing = false;

        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Run"))
            isRunning = true;
        else
            isRunning = false;

        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Taunt"))
            isTaunting = true;
        else
            isTaunting = false;

        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Flinch"))
            isFlinching = true;
        else
            isFlinching = false;
    }

    void TargetStateCheck()
    {
        if (hitPoints > 0)
            if (targetTransform)
                if (targetTransform.GetComponent<Player>())
                    if (targetTransform.GetComponent<Player>().isRespawning || targetTransform.GetComponent<Player>().isDead)
                        targetTransform = null;
    }
    protected void LookAtTarget()
    {
        if (PhotonNetwork.IsMasterClient)
            if (targetTransform && (isIdling || isMeleeing))
            {
                Vector3 targetPostition = new Vector3(targetTransform.position.x,
                                                    this.transform.position.y,
                                                    targetTransform.position.z);
                this.transform.LookAt(targetPostition);
            }
    }

    void DropRandomWeapon()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

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
                Vector3 fDir = losSpawn.transform.forward + new Vector3(0, 2f, 0);

                GameObject[] weapInv = GameManager.GetRootPlayer().playerInventory.allWeaponsInInventory;
                NetworkGameManager.SpawnNetworkWeapon(weapInv[randomWeaponInd].GetComponent<WeaponProperties>(),
                spp, fDir, currAmmo: (int)(wp.ammoCapacity * ranAmmoFactor), spareAmmo: (int)(wp.maxSpareAmmo * ranCapFactor));
            }
            catch { }
        }
    }

    void FindNewTarget()
    {
        if (_findNewTargetCooldown > 0)
        {
            _findNewTargetCooldown -= Time.deltaTime;

            if (_findNewTargetCooldown <= 0)
            {
                if (!targetTransform)
                {
                    Debug.Log("Finding new Target Transform");

                    try
                    {
                        Transform tp = FindObjectOfType<MapAiWaypoints>().GetRandomWaypoint();


                        while (targetTransform == tp)
                        {
                            tp = FindObjectOfType<MapAiWaypoints>().GetRandomWaypoint();
                        }

                        targetTransform = tp;
                    }
                    catch
                    {

                    }

                    if (FindObjectOfType<NetworkSwarmManager>())
                    {
                        int pid = FindObjectOfType<NetworkSwarmManager>().GetRandomAlivePlayerPhotonId();
                        if (pid > 0)
                            SetNewTargetPlayerWithPid(pid);
                    }
                }

                _findNewTargetCooldown = 2;
            }
        }
    }

    public virtual void OnHealthHitEarly() { }





    IEnumerator Hide()
    {
        yield return new WaitForSeconds(5);
        gameObject.SetActive(false);
    }







    public abstract void AnalyzeNextAction();
    public abstract void CooldownsUpdate();
    public abstract void ChildPrepare();


    protected virtual void ChildOnActorDamaged() { }
    protected virtual void ChildOnEnable() { }






    [PunRPC]
    public void DamageActor(int damage, int playerWhoShotPDI, string damageSource = null, bool isHeadshot = false)
    {
        if (hitPoints <= 0)
            return;

        Player pp = GameManager.GetPlayerWithPhotonViewId(playerWhoShotPDI);
        pp.GetComponent<PlayerSwarmMatchStats>().AddPoints(damage);

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
    public void SetNewTargetPlayerWithPid(int pid, bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("SetNewTargetPlayerWithPid", RpcTarget.AllViaServer, pid, false);
        }
        else
        {
            targetPlayer = PhotonView.Find(pid).GetComponent<Player>();
        }
    }


    [PunRPC]
    public void ActorDeath(bool caller = true)
    {
        if (caller && PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC("ActorDeath", RpcTarget.All, false);
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
        }
    }
}
