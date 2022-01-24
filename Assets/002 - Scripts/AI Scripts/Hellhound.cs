using Photon.Pun;
using UnityEngine;

public class Hellhound : AiAbstractClass
{
    [Header("Combat")]
    public int meleeDamage;
    public int projectileDamage;
    public int projectileSpeed;

    [Header("Prefabs")]
    public Fireball projectile;

    public enum HellhoundActions { Bite, Seek, Idle }
    [SerializeField] HellhoundActions _hellhoundAction;

    public HellhoundActions hellhoundAction
    {
        get { return _hellhoundAction; }
        set
        {
            if (_hellhoundAction != value)
            {
                _hellhoundAction = value;
                InvokeOnActionChanged();
            }
        }
    }

    public override void OnEnable()
    {
        hellhoundAction = HellhoundActions.Seek;
        seek = true;
    }
    public override void OnPlayerRangeChange_Delegate(AiAbstractClass aiAbstractClass)
    {
        HellhoundActions previousAction = hellhoundAction;
        int ran = Random.Range(0, 3);

        if (targetInLineOfSight)
        {
            if (aiAbstractClass.playerRange == PlayerRange.Close)
                previousAction = HellhoundActions.Bite;
            else
                previousAction = HellhoundActions.Seek;
        }
        else
            previousAction = HellhoundActions.Seek;

        if (aiAbstractClass.playerRange != PlayerRange.Close)
            previousAction = HellhoundActions.Seek;

        hellhoundAction = previousAction;
    }

    public override void DoAction()
    {
        HellhoundActions previousHellhoundAction = hellhoundAction;
        if (!isDead && target)
        {
            if (previousHellhoundAction != HellhoundActions.Seek)
                seek = false;
            else
                seek = true;

            if (playerRange == PlayerRange.Out)
                previousHellhoundAction = HellhoundActions.Seek;

            if (previousHellhoundAction == HellhoundActions.Bite)
            {
                if (canDoAction)
                {
                    animator.Play("Bite");
                    target.GetComponent<Player>().Damage(meleeDamage, false, 99);
                    nextActionCooldown = defaultNextActionCooldown;
                }
            }
            else
                seek = true;

            //Debug.Log($"Hellhound do action: {hellhoundAction}");
        }
        else if (!isDead && !target)
        {
            hellhoundAction = HellhoundActions.Idle;
            seek = false;
        }
    }

    public override void ChildUpdate()
    {
        if (!target)
            return;

        Vector3 targetPostition = new Vector3(target.position.x,
                                        this.transform.position.y,
                                        target.position.z);
        this.transform.LookAt(targetPostition);
    }

    public override void Damage(int damage, int playerWhoShotPDI)
    {
        if (isDead)
            return;
        photonView.RPC("Damage_RPC", RpcTarget.All, damage, playerWhoShotPDI);
    }

    [PunRPC]
    public override void Damage_RPC(int damage, int playerWhoShotPDI)
    {
        if (isDead)
            return;

        Player pp = GameManager.instance.GetPlayerWithPhotonViewId(playerWhoShotPDI);
        pp.GetComponent<PlayerSwarmMatchStats>().AddPoints(damage);

        health -= damage;
        if (isDead)
        {
            pp.GetComponent<PlayerSwarmMatchStats>().kills++;
            pp.GetComponent<PlayerSwarmMatchStats>().AddPoints(defaultHealth);
        }
    }

    public override void OnTargetInLineOfSightChanged_Delegate(AiAbstractClass aiAbstractClass)
    {
        if (!targetInLineOfSight)
            hellhoundAction = HellhoundActions.Seek;
        else
        {
            Debug.Log($"Target in line of sight. Player range: {playerRange}");
            if (playerRange == PlayerRange.Close)
                hellhoundAction = HellhoundActions.Bite;
        }
    }

    [PunRPC]
    public override void ChangeAction_RPC(string actionString)
    {
        throw new System.NotImplementedException();
    }

    public override void OnDeathEnd_Delegate(AiAbstractClass aiAbstractClass)
    {

    }
}
