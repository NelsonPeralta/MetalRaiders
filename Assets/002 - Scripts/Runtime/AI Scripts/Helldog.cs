using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helldog : Actor
{
    public override void Idle(bool callRPC = true)
    {
        HellhoundIdle(callRPC);
    }

    public override void Melee(bool callRPC = true)
    {
        HellhoundMelee(callRPC);
    }

    public override void Run(bool callRPC = true)
    {
        HellhoundRun(callRPC);
    }

    public override void ShootProjectile(bool callRPC = true)
    {
        HellhoundRun(callRPC);
    }

    public override void ThrowExplosive(bool callRPC = true)
    {
        HellhoundRun(callRPC);
    }










    [PunRPC]
    void HellhoundMelee(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("HellhoundMelee", RpcTarget.AllViaServer, false);
            targetTransform.GetComponent<Player>().Damage(8, false, pid);
        }
        else
        {
            GetComponent<AudioSource>().clip = _attackClip;
            GetComponent<AudioSource>().Play();
            _animator.SetBool("Run", false);
            _animator.Play("Bite");
            _meleeCooldown = 0.8f;

        }
    }


    [PunRPC]
    void HellhoundIdle(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("HellhoundIdle", RpcTarget.AllViaServer, false);
        }
        else
        {
            //Debug.Log("UndeadIdle RPC");

            nma.enabled = false;
            _animator.SetBool("Run", false);
        }
    }

    [PunRPC]
    void HellhoundRun(bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("HellhoundRun", RpcTarget.AllViaServer, false);
            if (isRunning && !isFlinching && !isTaunting)
            {
                nma.enabled = true;
                nma.SetDestination(targetTransform.position);
            }
        }
        else
        {
            _animator.SetBool("Run", true);
        }
    }
}
