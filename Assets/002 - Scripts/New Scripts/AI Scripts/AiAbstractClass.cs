using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;

abstract public class AiAbstractClass : MonoBehaviourPunCallbacks
{
    // events
    public delegate void AiEvent(AiAbstractClass aiAbstractClass);
    public AiEvent OnHealthChange, OnDeath;

    // enums
    public enum PlayerRange { Out, Close, Medium, Long }

    // private variables
    PhotonView PV;
    int _health;
    float newTargetSwitchingDelay;

    // public variables
    public PlayerRange playerRange;
    public Hitboxes hitboxes;

    [Header("Properties")]
    [SerializeField]
    int defaultHealth;
    public int speed;

    [Header("Combat")]
    public Transform target;
    public float maxCloseRange;
    public float maxMediumRange;
    public float maxLongRange;
    public float nextActionCooldown;

    [Header("Player Switching")]
    public int newTargetPhotonId;

    public int health
    {
        get
        {
            return _health;
        }

        private set
        {
            _health = value;
            OnHealthChange?.Invoke(this);

            if (_health <= 0)
                OnDeath?.Invoke(this);
        }
    }

    public bool isDead
    {
        get { return health <= 0; }
    }
    void Start()
    {
        PV = GetComponent<PhotonView>();
        OnDeath += OnDeath_Delegate;
    }

    public void Damage(int damage, int playerWhoShotPDI)
    {
        if (isDead)
            return;
        PV.RPC("Damage_RPC", RpcTarget.All, damage, playerWhoShotPDI);
    }

    [PunRPC]
    void Damage_RPC(int damage, int playerWhoShotPDI)
    {
        if (isDead)
            return;

        PlayerProperties pp = GameManager.instance.GetPlayerWithPhotonViewId(playerWhoShotPDI);
        pp.GetComponent<OnlinePlayerSwarmScript>().AddPoints(damage);

        health -= damage;
        if (isDead)
        {
            pp.GetComponent<OnlinePlayerSwarmScript>().kills++;
            pp.GetComponent<OnlinePlayerSwarmScript>().AddPoints(defaultHealth);
        }
    }

    void OnDeath_Delegate(AiAbstractClass aiAbstractClass)
    {
        StartCoroutine(Die_Coroutine());
    }

    IEnumerator Die_Coroutine()
    {
        gameObject.name = $"{gameObject.name} (DEAD)";
        target = null;
        GetComponent<NavMeshAgent>().speed = 0;
        GetComponent<Animator>().Play("Die");


        foreach (AIHitbox hitbox in hitboxes.AIHitboxes)
            hitbox.gameObject.SetActive(false);

        SwarmManager.instance.OnAiDeath();
        yield return new WaitForSeconds(5);
        gameObject.SetActive(false);
    }
}
