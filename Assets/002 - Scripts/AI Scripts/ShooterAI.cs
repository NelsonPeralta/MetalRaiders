using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ShooterAI : AiAbstractClass
{
    // Start is called before the first frame update
    void Start()
    {
        AIHitbox[] playerHitboxes = GetComponentsInChildren<AIHitbox>();

        foreach (AIHitbox playerHitbox in playerHitboxes)
        {
            playerHitbox.GetComponent<MeshRenderer>().enabled = false;
            //playerHitbox.player = GetComponent<Player>();
            playerHitbox.aiAbstractClass = this;
            playerHitbox.gameObject.layer = 7;
        }

        GoToRandomPoint();
    }

    // Update is called once per frame
    void Update()
    {
        if (DestinationReached())
            GoToRandomPoint();
    }
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
    }

    public override void OnDeathEnd_Delegate(AiAbstractClass aiAbstractClass)
    {
    }

    public override void OnPlayerRangeChange_Delegate(AiAbstractClass aiAbstractClass)
    {
    }

    public override void OnPrepareEnd_Delegate(AiAbstractClass aiAbstractClass)
    {
    }

    public override void OnTargetInLineOfSightChanged_Delegate(AiAbstractClass aiAbstractClass)
    {
    }

    protected override void Damage_Abstract(int damage, int playerWhoShotPDI, string damageSource = null, bool isHeadshot = false)
    {
    }

    void GoToRandomPoint()
    {
        int walkRadius = 10;

        Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, walkRadius, 1);
        Vector3 finalPosition = hit.position;

        GetComponent<NavMeshAgent>().destination = finalPosition;
        Debug.Log($"AI goint to random point: {finalPosition}");
    }

    bool DestinationReached()
    {
        // Check if we've reached the destination
        if (!GetComponent<NavMeshAgent>().pathPending)
        {
            if (GetComponent<NavMeshAgent>().remainingDistance <= GetComponent<NavMeshAgent>().stoppingDistance)
            {
                if (!GetComponent<NavMeshAgent>().hasPath || GetComponent<NavMeshAgent>().velocity.sqrMagnitude == 0f)
                {
                    // Done
                    return true;
                }
            }
        }
        return false;
    }
}
