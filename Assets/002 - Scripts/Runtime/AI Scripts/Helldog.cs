using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helldog : Actor
{
    float _targetLostCooldown;

    protected override void ChildOnEnable()
    {
        _hitPoints += FindObjectOfType<SwarmManager>().currentWave * 4;
    }


    [PunRPC]
    void HelldogAttack(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("HelldogAttack", RpcTarget.All, false);
            targetTransform.GetComponent<Player>().Damage(4, false, pid);
        }
        else
        {
            //Debug.Log("Punch Player RPC");

            GetComponent<AudioSource>().clip = _attackClip;
            GetComponent<AudioSource>().Play();

            _animator.SetBool("Run", false);
            _animator.Play("Bite");
            _meleeCooldown = 1;
        }
    }








    [PunRPC]
    void HelldogIdle(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("HelldogIdle", RpcTarget.All, false);
        }
        else
        {
            //Debug.Log("UndeadIdle RPC");

            nma.enabled = false;
            _animator.SetBool("Run", false);
        }
    }

    [PunRPC]
    void HelldogRun(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("HelldogRun", RpcTarget.All, false);
        }
        else
        {
            //Debug.Log("UndeadRun RPC");

            //_animator.Play("Run");
            _animator.SetBool("Run", true);
        }
    }

    public override void Idle(bool callRPC = true)
    {
        throw new System.NotImplementedException();
    }

    public override void Run(bool callRPC = true)
    {
        throw new System.NotImplementedException();
    }

    public override void Melee(bool callRPC = true)
    {
        throw new System.NotImplementedException();
    }

    public override void ShootProjectile(bool callRPC = true)
    {
        throw new System.NotImplementedException();
    }

    public override void ThrowExplosive(bool callRPC = true)
    {
        throw new System.NotImplementedException();
    }
}
