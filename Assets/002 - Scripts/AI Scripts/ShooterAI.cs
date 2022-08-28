using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ShooterAI : AiAbstractClass
{
    public enum ShooterAIActions { Idle, Roam, Shoot }
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

    private void OnEnable()
    {
        Prepare();

        target = GetRandomDestinationTransform();
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

    int frames = 0;
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

    public override void ChildUpdate()
    {
    }

    public override void Damage_RPC(int damage, int playerWhoShotPDI, string damageSource = null, bool isHeadshot = false)
    {
    }

    public override void DoAction()
    {
        Debug.Log($"{this.GetType()} DoAction");

        ShooterAIActions previousHellhoundAction = shooterAiAction;
        if (!isDead && target)
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
        else if (!isDead && !target)
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
            if (!target.GetComponent<Player>())
            {
                Debug.Log("Got to empty target");
                GetNewTarget(emptyTarget: true);
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
                GetNewTarget(emptyTarget: true);
            }
        }
    }
}
