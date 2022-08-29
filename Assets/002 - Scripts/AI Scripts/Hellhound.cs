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
        if (!isDead && destination)
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
                    _voice.clip = _attackClip;
                    _voice.Play();
                    animator.Play("Bite");
                    destination.GetComponent<Player>().Damage(meleeDamage, false, 99);
                    nextActionCooldown = defaultNextActionCooldown;
                }
            }
            else
                seek = true;

            //Debug.Log($"Hellhound do action: {hellhoundAction}");
        }
        else if (!isDead && !destination)
        {
            hellhoundAction = HellhoundActions.Idle;
            seek = false;
        }
    }

    public override void ChildUpdate()
    {
        if (!destination)
            return;

        Vector3 targetPostition = new Vector3(destination.position.x,
                                        this.transform.position.y,
                                        destination.position.z);
        this.transform.LookAt(targetPostition);
    }

    protected override void Damage_Abstract(int damage, int playerWhoShotPDI, string damageSource = null, bool isHeadshot = false)
    {
        if (isDead)
            return;
        photonView.RPC("Damage_RPC", RpcTarget.All, damage, playerWhoShotPDI, damageSource, isHeadshot);
    }

    [PunRPC]
    public override void Damage_RPC(int damage, int playerWhoShotPDI, string damageSource = null, bool isHeadshot = false)
    {
        if (isDead)
            return;
        health -= damage;

        try
        {
            Player pp = GameManager.GetPlayerWithPhotonViewId(playerWhoShotPDI);
            pp.GetComponent<PlayerSwarmMatchStats>().AddPoints(damage);
            if (isDead)
            {
                pp.GetComponent<PlayerSwarmMatchStats>().kills++;
                pp.GetComponent<PlayerSwarmMatchStats>().AddPoints(defaultHealth);
            }
        }
        catch (System.Exception e)
        {

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
        hellhoundAction = (HellhoundActions)System.Enum.Parse(typeof(HellhoundActions), actionString);
    }

    public override void OnDeathEnd_Delegate(AiAbstractClass aiAbstractClass)
    {

    }

    public override void OnPrepareEnd_Delegate(AiAbstractClass aiAbstractClass)
    {
        projectileDamage += SwarmManager.instance.currentWave * 2;
    }
}
