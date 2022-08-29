using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Photon.Pun;

public class Zombie : AiAbstractClass
{
    [Header("Combat")]
    public int meleeDamage;
    public enum ZombieActions { Melee, Seek, Idle }
    [SerializeField] ZombieActions _zombieAction;

    public List<GameObject> skins = new List<GameObject>();

    public ZombieActions zombieAction
    {
        get { return _zombieAction; }
        set
        {
            if (_zombieAction != value)
            {
                _zombieAction = value;
                InvokeOnActionChanged();
            }
        }
    }

    public override void OnEnable()
    {
        zombieAction = ZombieActions.Seek;
        seek = true;


        int ran = Random.Range(0, skins.Count - 1);
        skins[0].SetActive(false);
        skins[ran].SetActive(true);
    }
    public override void OnPlayerRangeChange_Delegate(AiAbstractClass aiAbstractClass)
    {
        ZombieActions previousAction = zombieAction;
        int ran = Random.Range(0, 3);

        if (targetInLineOfSight)
        {
            if (aiAbstractClass.playerRange == PlayerRange.Close)
                previousAction = ZombieActions.Melee;
            else
                previousAction = ZombieActions.Seek;
        }
        else
            previousAction = ZombieActions.Seek;

        if (aiAbstractClass.playerRange != PlayerRange.Close)
            previousAction = ZombieActions.Seek;

        zombieAction = previousAction;
    }

    public override void DoAction()
    {
        ZombieActions previousHellhoundAction = zombieAction;
        if (!isDead && destination)
        {
            if (previousHellhoundAction != ZombieActions.Seek)
                seek = false;
            else
                seek = true;

            if (playerRange == PlayerRange.Out)
                previousHellhoundAction = ZombieActions.Seek;

            if (previousHellhoundAction == ZombieActions.Melee)
            {
                if (canDoAction)
                {
                    _voice.clip = _attackClip;
                    _voice.Play();
                    animator.Play("Attack");
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
            zombieAction = ZombieActions.Idle;
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

        Player pp = GameManager.GetPlayerWithPhotonViewId(playerWhoShotPDI);
        try
        {
            pp.GetComponent<PlayerSwarmMatchStats>().AddPoints(damage);
        }
        catch (System.Exception e)
        {

        }

        health -= damage;
        if (isDead)
        {
            try
            {
                pp.GetComponent<PlayerSwarmMatchStats>().kills++;
                pp.GetComponent<PlayerSwarmMatchStats>().AddPoints(defaultHealth);
            }
            catch (System.Exception e)
            {

            }
        }
    }

    public override void OnTargetInLineOfSightChanged_Delegate(AiAbstractClass aiAbstractClass)
    {
        if (!targetInLineOfSight)
            zombieAction = ZombieActions.Seek;
        else
        {
            Debug.Log($"Target in line of sight. Player range: {playerRange}");
            if (playerRange == PlayerRange.Close)
                zombieAction = ZombieActions.Melee;
        }
    }

    [PunRPC]
    public override void ChangeAction_RPC(string actionString)
    {

    }

    public override void OnDeathEnd_Delegate(AiAbstractClass aiAbstractClass)
    {

    }

    public override void OnPrepareEnd_Delegate(AiAbstractClass aiAbstractClass)
    {
        meleeDamage += SwarmManager.instance.currentWave * 2;
    }
}
