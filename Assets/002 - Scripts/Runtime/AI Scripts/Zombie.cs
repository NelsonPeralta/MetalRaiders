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
            if (value == ZombieActions.Seek)
                staticAnimationPlaying = false;
            else
                staticAnimationPlaying = true;

            if (_zombieAction != value)
            {
                _zombieAction = value;
                Debug.Log($"ZOMBIE New Action: {_zombieAction}");
                InvokeOnActionChanged();
            }
        }
    }

    public override void OnEnable()
    {
        _health += FindObjectOfType<SwarmManager>().currentWave * 5;
        zombieAction = ZombieActions.Seek;
        seek = true;

        foreach (GameObject obj in skins) { obj.SetActive(false); }

        int ran = Random.Range(0, skins.Count);
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
        if (!isDead && targetPlayer)
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
                    targetPlayer.GetComponent<Player>().Damage(meleeDamage);
                    nextActionCooldown = defaultNextActionCooldown;
                }
            }
            else
                seek = true;

            //Debug.Log($"Hellhound do action: {hellhoundAction}");
        }
        else if (!isDead && !targetPlayer)
        {
            zombieAction = ZombieActions.Idle;
            seek = false;
        }
    }

    public override void ChildUpdate()
    {
        if (!targetPlayer)
            return;


        // DO NOT MIX NMA.SETDESTINATION WITH AI POSITION OR ROTATION
        // USE ONE OR THE OTHER, NOT BOTH AT THE SAME TIME
        // TODO: Use following code only when AI is static


        //Vector3 targetPostition = new Vector3(targetPlayer.position.x,
        //                                this.transform.position.y,
        //                                targetPlayer.position.z);
        //this.transform.LookAt(targetPostition);
    }

    protected override void Damage_Abstract(int damage, int playerWhoShotPDI, string damageSource = null, bool isHeadshot = false)
    {
        if (isDead)
            return;

        int nh = _health - damage;
        photonView.RPC("Damage_RPC", RpcTarget.All, nh, playerWhoShotPDI, damageSource, isHeadshot);
    }

    [PunRPC]
    public override void Damage_RPC(int nh, int playerWhoShotPDI, string damageSource = null, bool isHeadshot = false)
    {
        if (isDead)
            return;

        int _damage = health - nh;

        Player pp = GameManager.GetPlayerWithPhotonViewId(playerWhoShotPDI);
        try
        {
            pp.GetComponent<PlayerSwarmMatchStats>().AddPoints(_damage);
        }
        catch (System.Exception e)
        {

        }

        health = nh;
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
        {
            Debug.Log($"OnTargetInLineOfSightChanged_Delegate TARGET NOT IN LOF");
            zombieAction = ZombieActions.Seek;
        }
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
        //SwarmManager.instance.DropRandomLoot(transform.position, transform.rotation);
    }

    public override void OnPrepareEnd_Delegate(AiAbstractClass aiAbstractClass)
    {
        meleeDamage += SwarmManager.instance.currentWave * 2;
    }
}
