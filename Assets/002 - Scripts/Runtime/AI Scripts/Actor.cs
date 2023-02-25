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
        private set
        {
            int pv = _hitPoints;
            int nv = Mathf.Clamp(value, 0, _defaultHitpoints);


            _hitPoints = nv;

            if (_hitPoints <= 0 && nv != pv)
            {
                //target = null; \\DO NOT REMOVE TARGET HERE
                try
                {
                    DropRandomWeapon();
                }
                catch (Exception e) { Debug.LogError(e); }

                GetComponent<AudioSource>().clip = _deathClip;
                GetComponent<AudioSource>().Play();

                foreach (ActorHitbox ah in GetComponentsInChildren<ActorHitbox>())
                    ah.gameObject.SetActive(false);
                _animator.Play("Die");
                nma.enabled = false;
                SwarmManager.instance.InvokeOnAiDeath();
                StartCoroutine(Hide());
                target = null;

            }

        }
    }
    public Transform target
    {
        get { return _target; }
        set
        {
            _target = value;

            if (_target)
                if (_target.GetComponent<Player>())
                    _target.GetComponent<Player>().OnPlayerDeath += OnPlayerDeath;
        }
    }
    public Vector3 destination { get { return _destination; } set { _destination = value; } }
    public Transform losSpawn { get { return _losSpawn; } set { _losSpawn = value; } }
    public virtual FieldOfView fieldOfView { get { return _fieldOfView; } private set { _fieldOfView = value; } }
    public NavMeshAgent nma { get { return _nma; } private set { _nma = value; } }

    public int longRange { get { return _longRange; } }
    public int midRange { get { return _midRange; } }
    public int closeRange { get { return _closeRange; } }

    public string currentAnimatorClipName { get { return _currentAnimatorState; } private set { _currentAnimatorState = value; } }

    [SerializeField] int _hitPoints;
    [SerializeField] Transform _target;
    [SerializeField] Vector3 _destination;
    [SerializeField] Transform _losSpawn;
    [SerializeField] FieldOfView _fieldOfView;
    [SerializeField] NavMeshAgent _nma;
    [SerializeField] protected Animator _animator;
    [SerializeField] protected string _currentAnimatorState;
    [SerializeField] protected Action _action;

    [SerializeField] int _closeRange, _midRange, _longRange;
    [SerializeField] float _analyzeNextActionCooldown, _findNewTargetCooldown;
    [SerializeField] protected AudioClip _attackClip, _deathClip;

    protected int _defaultHitpoints;
    protected bool isIdling, isRunning, isMeleeing;

    private void Awake()
    {
        _defaultHitpoints = _hitPoints;
        _animator = GetComponent<Animator>();
        _analyzeNextActionCooldown = _findNewTargetCooldown = 0.5f;

        _fieldOfView = GetComponent<FieldOfView>();
        _nma = GetComponent<NavMeshAgent>();

        if (_closeRange <= 0)
            _closeRange = 3;

        if (_midRange <= 0)
            _midRange = 12;

        if (_longRange <= 0)
            _longRange = 20;

        foreach (ActorHitbox ah in GetComponentsInChildren<ActorHitbox>()) { ah.actor = this; }
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
                    AnalyzeNextAction();
                    _analyzeNextActionCooldown = 0.3f;
                }
            }

        LookAtTarget();
        FindNewTarget();
    }

    private void OnEnable()
    {

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
        target = PhotonView.Find(targetPhotonId).transform;
        gameObject.SetActive(true);
    }

    protected void Prepare()
    {
        transform.position = new Vector3(0, -10, 0);
        hitPoints = _defaultHitpoints + SwarmManager.instance.currentWave * 2;
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
    }

    void TargetStateCheck()
    {
        if (hitPoints > 0)
            if (target)
                if (target.GetComponent<Player>())
                    if (target.GetComponent<Player>().isRespawning || target.GetComponent<Player>().isDead)
                        target = null;
    }
    protected void LookAtTarget()
    {
        if (PhotonNetwork.IsMasterClient)
            if (target && (isIdling || isMeleeing))
            {
                Vector3 targetPostition = new Vector3(target.position.x,
                                                    this.transform.position.y,
                                                    target.position.z);
                this.transform.LookAt(targetPostition);
            }
    }

    void DropRandomWeapon()
    {
        int ChanceToDrop = UnityEngine.Random.Range(0, 10);

        if (ChanceToDrop <= 4)
        {
            float ranAmmoFactor = UnityEngine.Random.Range(0.2f, 0.9f);
            float ranCapFactor = UnityEngine.Random.Range(0.2f, 0.6f);
            int randomWeaponInd = UnityEngine.Random.Range(0, GameManager.GetMyPlayer().playerInventory.allWeaponsInInventory.Length);

            WeaponProperties wp = GameManager.GetMyPlayer().playerInventory.allWeaponsInInventory[randomWeaponInd].GetComponent<WeaponProperties>();

            if (wp.weaponType == WeaponProperties.WeaponType.LMG || wp.weaponType == WeaponProperties.WeaponType.Launcher || wp.weaponType == WeaponProperties.WeaponType.Shotgun || wp.weaponType == WeaponProperties.WeaponType.Sniper)
                return;
            Debug.Log($"DropRandomWeapon: {wp.cleanName}");




            Dictionary<string, int> param = new Dictionary<string, int>();
            param["ammo"] = (int)(wp.ammoCapacity * ranAmmoFactor);
            param["spareammo"] = (int)(wp.maxAmmo * ranCapFactor);
            Vector3 spp = transform.position;
            Vector3 fDir = losSpawn.transform.forward + new Vector3(0, 2f, 0);
            NetworkGameManager.SpawnNetworkWeapon(randomWeaponInd, spp, fDir, param);
        }
    }

    void FindNewTarget()
    {
        if (_findNewTargetCooldown > 0)
        {
            _findNewTargetCooldown -= Time.deltaTime;

            if (_findNewTargetCooldown <= 0)
            {
                if (!target)
                {
                    int pid = FindObjectOfType<NetworkSwarmManager>().GetRandomAlivePlayerPhotonId();

                    if (pid > 0)
                        SetNewTargetWithPid(pid);
                }

                _findNewTargetCooldown = 1;
            }
        }
    }







    IEnumerator Hide()
    {
        yield return new WaitForSeconds(5);
        gameObject.SetActive(false);
    }











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
                //pp.GetComponent<PlayerSwarmMatchStats>().AddPoints(defaultHealth);

                //SpawnKillFeed(this.GetType().ToString(), playerWhoShotPDI, damageSource: damageSource, isHeadshot: isHeadshot);
            }
            catch { }
        }
    }

    [PunRPC]
    public void SetNewTargetWithPid(int pid, bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("SetNewTargetWithPid", RpcTarget.All, pid, false);
        }
        else
        {
            target = PhotonView.Find(pid).transform;
        }
    }












    public abstract void AnalyzeNextAction();
    public abstract void CooldownsUpdate();
}
