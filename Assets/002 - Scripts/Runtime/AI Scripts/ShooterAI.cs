using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ShooterAI : AiAbstractClass
{
    public PlayerShooting playerShooting;
    public PlayerInventory playerInventory;
    public enum ShooterAIActions { Idle, Roam, Shoot, Strafe }
    [SerializeField] ShooterAIActions _shooterAiAction;

    public ShooterAIActions shooterAiAction
    {
        get { return _shooterAiAction; }
        set
        {
            if (_shooterAiAction != value)
            {
                _shooterAiAction = value;
                InvokeOnActionChanged();
            }
        }
    }

    public override bool seek
    {
        get { return _seek; }
        set
        {
            _seek = value;

            if (!seek)
            {
                nma.speed = 0;
                nma.velocity = Vector3.zero;
                animator.SetFloat("Vertical", 0);
            }
            else
            {
                nma.speed = speed;
                animator.SetFloat("Vertical", 1);
            }
        }
    }


    [SerializeField] Transform _playerTarget;
    [SerializeField] bool _lookAtPlayerTarget;
    [SerializeField] bool _shoot;
    public bool shoot
    {
        set { _shoot = value; }
        get { return _shoot; }
    }
    public bool lookAtPlayerTarget
    {
        get { return _lookAtPlayerTarget; }
        set { _lookAtPlayerTarget = value; }
    }
    public Transform playerTarget
    {
        get { return _playerTarget; }
        set { _playerTarget = value; }
    }

    public override Transform targetPlayer
    {
        get { return null; }
        //set
        //{
        //    try { Destroy(_destination.gameObject); } catch { }
        //    _destination = value;
        //}
    }
    float ogAngularSpeed;

    private void OnEnable()
    {
        OnDestinationNull += OnDestinationNull_Delegate;
        OnDestinationChanged += OnDestinationChanged_Delegate;
        OnPrepareEnd += OnPrepareEnd_Delegate;
        ogAngularSpeed = GetComponent<NavMeshAgent>().angularSpeed;
        Prepare();

        GetNewTarget(emptyTarget: true);
        gameObject.SetActive(true);

        OnPlayerRangeChange?.Invoke(this);
    }

    // Start is called before the first frame update
    //void Start()
    //{
    //    AIHitbox[] playerHitboxes = GetComponentsInChildren<AIHitbox>();

    //    foreach (AIHitbox playerHitbox in playerHitboxes)
    //    {
    //        playerHitbox.GetComponent<MeshRenderer>().enabled = false;
    //        //playerHitbox.player = GetComponent<Player>();
    //        playerHitbox.aiAbstractClass = this;
    //        playerHitbox.gameObject.layer = 7;
    //    }

    //    shooterAiAction = ShooterAIActions.Roam;
    //    seek = true;
    //}

    // Update is called once per frame
    //void Update()
    //{
    //    frames++;

    //    if (frames < 5)
    //        return;

    //    //if (DestinationReached())
    //    //    GoToRandomPoint();

    //    //if (GetComponent<NavMeshAgent>().velocity.magnitude > 0.1f)
    //    //    animator.SetFloat("Vertical", 1);

    //    if (frames >= 5)
    //        frames = 0;
    //}
    public override void ChangeAction_RPC(string actionString)
    {
    }

    int frames = 0;
    public override void ChildUpdate()
    {
        if (shoot)
        {

            frames++;

            if (frames >= 30 && frames <= 60)
                GetComponent<PlayerShooting>().Shoot();
            if (frames > 90)
                frames = 0;

        }

        if (lookAtPlayerTarget)
        {
            Vector3 t = new Vector3(playerTarget.position.x,
                                        this.transform.position.y,
                                        playerTarget.position.z);
            this.transform.LookAt(t);
        }
    }

    public override void Damage_RPC(int damage, int playerWhoShotPDI, string damageSource = null, bool isHeadshot = false)
    {
    }

    public override void DoAction()
    {
        Debug.Log($"{this.GetType()} DoAction");

        ShooterAIActions previousHellhoundAction = shooterAiAction;
        if (!isDead && targetPlayer)
        {
            seek = true;
            //if (previousHellhoundAction != ShooterAIActions.Roam)
            //    seek = false;
            //else
            //    seek = true;

            //if (playerRange == PlayerRange.Out)
            //    previousHellhoundAction = ShooterAIActions.Roam;

            //if (previousHellhoundAction == HellhoundActions.Bite)
            //{
            //    if (canDoAction)
            //    {
            //        _voice.clip = _attackClip;
            //        _voice.Play();
            //        animator.Play("Bite");
            //        target.GetComponent<Player>().Damage(meleeDamage, false, 99);
            //        nextActionCooldown = defaultNextActionCooldown;
            //    }
            //}
            //else
            //    seek = true;

            //Debug.Log($"Hellhound do action: {hellhoundAction}");
        }
        else if (!isDead && !targetPlayer)
        {
            shooterAiAction = ShooterAIActions.Idle;
            seek = false;
        }
    }

    public override void OnDeathEnd_Delegate(AiAbstractClass aiAbstractClass)
    {
    }

    public override void OnPlayerRangeChange_Delegate(AiAbstractClass aiAbstractClass)
    {
        ShooterAIActions previousAction = shooterAiAction;
        int ran = Random.Range(0, 3);

        if (aiAbstractClass.playerRange == PlayerRange.Close)
        {
            if (!targetPlayer.GetComponent<Player>())
            {
                Debug.Log("Got to empty target");
                //GetNewTarget(emptyTarget: true);
            }
        }

        //if (targetInLineOfSight)
        //{
        //    if (aiAbstractClass.playerRange == PlayerRange.Close)
        //        previousAction = ShooterAIActions.Bite;
        //    else
        //        previousAction = ShooterAIActions.Seek;
        //}
        //else
        //    previousAction = ShooterAIActions.Seek;

        //if (aiAbstractClass.playerRange != PlayerRange.Close)
        //    previousAction = ShooterAIActions.Seek;

        //shooterAiAction = previousAction;
    }

    public override void OnPrepareEnd_Delegate(AiAbstractClass aiAbstractClass)
    {
        Debug.Log("OnPrepareEnd_Delegate");
        AIHitbox[] playerHitboxes = GetComponentsInChildren<AIHitbox>();

        foreach (AIHitbox playerHitbox in playerHitboxes)
        {
            playerHitbox.GetComponent<MeshRenderer>().enabled = false;
            //playerHitbox.player = GetComponent<Player>();
            playerHitbox.aiAbstractClass = this;
            playerHitbox.gameObject.layer = 7;
        }

        shooterAiAction = ShooterAIActions.Roam;
        seek = true;
    }

    public override void OnTargetInLineOfSightChanged_Delegate(AiAbstractClass aiAbstractClass)
    {
    }

    protected override void Damage_Abstract(int damage, int playerWhoShotPDI, string damageSource = null, bool isHeadshot = false)
    {
    }

    public override void NewTargetCountdown()
    {
        if (!gameObject.activeSelf || isDead)
            return;
        if (_newTargetCountdown > 0)
        {
            _newTargetCountdown -= Time.deltaTime;

            if (_newTargetCountdown <= 0)
            {
                //GetNewTarget(emptyTarget: true);
            }
        }
    }

    float fireInterval = 0;
    public override void OnRangeTriggerEnter_Delegate(AiRangeTrigger aiRangeCollider, Collider triggerObj = null)
    {
        if (triggerObj.GetComponent<Player>())
        {
            Debug.Log("Here 2: " + triggerObj.name);
            if (aiRangeCollider.range == PlayerRange.Medium)
            {
                Debug.Log("Here 2: " + triggerObj.name);
                if (!playerTarget)
                {
                    Debug.Log("Here 3: " + triggerObj.name);
                    // Strafe
                    {
                        foreach (AiRangeTrigger rt in rangeColliders)
                            if (rt.range == PlayerRange.Close)
                            {
                                targetPlayer = null;
                                GetNewTarget(walkRadius: rt.GetComponent<SphereCollider>().radius * 1.5f,
                                    emptyTarget: true);
                                shooterAiAction = ShooterAIActions.Strafe;
                            }
                    }
                    playerTarget = triggerObj.GetComponent<Transform>();
                    shoot = true;
                    lookAtPlayerTarget = true;
                }
            }
        }
        else if (!targetPlayer.GetComponent<Player>() && triggerObj.GetComponent<Transform>() == targetPlayer)
        {
            if (aiRangeCollider.range == PlayerRange.Close)
            {
                if (shooterAiAction == ShooterAIActions.Roam)
                {
                    foreach (AiRangeTrigger rt in rangeColliders)
                        if (rt.range == PlayerRange.Close)
                        {
                            targetPlayer = null;
                            GetNewTarget(emptyTarget: true);
                        }
                }
                else if (shooterAiAction == ShooterAIActions.Strafe)
                {
                    // Strafe
                    foreach (AiRangeTrigger rt in rangeColliders)
                        if (rt.range == PlayerRange.Close)
                        {
                            targetPlayer = null;
                            GetNewTarget(walkRadius: rt.GetComponent<SphereCollider>().radius * 1.5f,
                                        emptyTarget: true);
                        }
                }
            }
        }



        {
            //Debug.Log("Here 1: " + triggerObj.name);
            //if (triggerObj.GetComponent<Player>() && !target.GetComponent<Player>())
            //{
            //    Debug.Log("Here 2");

            //    if (aiRangeCollider.range == PlayerRange.Medium)
            //    {
            //        Debug.Log("Here 3 ");
            //        GetComponent<NavMeshAgent>().angularSpeed = 0;

            //        //shooterAiAction = ShooterAIActions.Idle;
            //        //seek = false;

            //        playerTarget = triggerObj.transform;
            //        shoot = true;
            //        lookAtPlayerTarget = true;
            //        GetNewTarget(walkRadius: 2, emptyTarget: true);
            //    }
            //}

            //if (!triggerObj.GetComponent<Player>() && !target.GetComponent<Player>())
            //{
            //    Debug.Log("Here 4 ");

            //    if (!playerTarget)
            //    {
            //        Debug.Log("Here 5");
            //        GetNewTarget(emptyTarget: true);
            //    }
            //}

            //if (triggerObj == target)
            //    Destroy(triggerObj.gameObject);
        }
    }

    public override void OnRangeTriggerExit_Delegate(AiRangeTrigger aiRangeCollider, Collider triggerObj = null)
    {
        //if (aiRangeCollider.range == PlayerRange.Long && triggerObj.GetComponent<Player>())
        //{
        //    Debug.Log("OnRangeTriggerExit_Delegate Player in Range: " + seek);

        //    shooterAiAction = ShooterAIActions.Roam;
        //    seek = true;



        //    GetComponent<NavMeshAgent>().angularSpeed = ogAngularSpeed;
        //    playerTarget = null;
        //    shoot = false;
        //    lookAtPlayerTarget = false;
        //    //GetNewTarget(emptyTarget: true);
        //}




        //bool playerInRange = false;
        //foreach (AiRangeTrigger a in rangeColliders)
        //    if (a.playersInRange.Count > 0)
        //        playerInRange = true;

        //if(!playerInRange)
        //{
        //    shooterAiAction = ShooterAIActions.Roam;
        //    seek = true;
        //}

        Debug.Log("OnRangeTriggerExit_Delegate Player in Range: " + seek);
        //if (!target.GetComponent<Player>())
        //    GetNewTarget(emptyTarget: true);
        //else
        //{
        //    shooterAiAction = ShooterAIActions.Idle;
        //    seek = false;
        //}
        //if (target && aiRangeCollider.playersInRange.Contains(target.GetComponent<Player>()))
        //{
        //    PlayerRange exitingRange = aiRangeCollider.range;
        //    PlayerRange newPlayerRange = playerRange;

        //    if (exitingRange == PlayerRange.Close)
        //        newPlayerRange = PlayerRange.Medium;
        //    else if (exitingRange == PlayerRange.Medium)
        //        newPlayerRange = PlayerRange.Long;
        //    else if (exitingRange == PlayerRange.Long)
        //        newPlayerRange = PlayerRange.Out;

        //    playerRange = newPlayerRange;
        //}
    }

    void OnDestinationNull_Delegate(AiAbstractClass aiAbstractClass)
    {
        //shooterAiAction = ShooterAIActions.Roam;
    }

    void OnDestinationChanged_Delegate(AiAbstractClass aiAbstractClass)
    {
        //shooterAiAction = ShooterAIActions.Roam;
    }
}
