using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(FieldOfView))]
[RequireComponent(typeof(NavMeshAgent))]
abstract public class Actor : MonoBehaviour
{
    public enum Action { Idle, Roam, Melee, Fireball, Grenade, Seek }
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
                foreach (ActorHitbox ah in GetComponentsInChildren<ActorHitbox>())
                    ah.gameObject.SetActive(false);
                _animator.Play("Die");
                nma.enabled = false;
                SwarmManager.instance.InvokeOnAiDeath();
                StartCoroutine(Hide());
            }

        }
    }
    public Transform target { get { return _target; } set { _target = value; } }
    public Vector3 destination { get { return _destination; } set { _destination = value; } }
    public Transform losSpawn { get { return _losSpawn; } set { _losSpawn = value; } }
    public virtual FieldOfView fieldOfView { get { return _fieldOfView; } private set { _fieldOfView = value; } }
    public NavMeshAgent nma { get { return _nma; } private set { _nma = value; } }

    public int longRange { get { return _longRange; } }
    public int midRange { get { return _midRange; } }
    public int closeRange { get { return _closeRange; } }


    [SerializeField] int _hitPoints;
    [SerializeField] Transform _target;
    [SerializeField] Vector3 _destination;
    [SerializeField] Transform _losSpawn;
    [SerializeField] FieldOfView _fieldOfView;
    [SerializeField] NavMeshAgent _nma;
    [SerializeField] protected Animator _animator;
    [SerializeField] protected Action _action;

    [SerializeField] int _closeRange, _midRange, _longRange;

    [SerializeField] float _analyzeNextActionCooldown;

    protected int _defaultHitpoints;

    private void Awake()
    {
        _defaultHitpoints = _hitPoints;
        _animator = GetComponent<Animator>();
        _analyzeNextActionCooldown = 0.5f;

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
        CooldownsUpdate();

        if (hitPoints > 0)
            if (_analyzeNextActionCooldown > 0)
            {
                _analyzeNextActionCooldown -= Time.deltaTime;

                if (_analyzeNextActionCooldown <= 0)
                {
                    AnalyzeNextAction();
                    _analyzeNextActionCooldown = 0.5f;
                }
            }
    }

    private void OnEnable()
    {

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

    public abstract void AnalyzeNextAction();
    public abstract void CooldownsUpdate();
}
