using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class AssignActorPlayerTargetOnShootingSphere : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] LayerMask _layerMask;



    float _cooldown;

    private void Update()
    {
        if (_cooldown > 0)
            _cooldown -= Time.time;
    }

    public void TriggerBehaviour()
    {
        Log.Print("TriggerBehaviour");

        if (_cooldown > 0 || !PhotonNetwork.IsMasterClient || GameManager.instance.gameMode == GameManager.GameMode.Versus) return;

        Log.Print("TriggerBehaviour");

        RaycastHit[] _hits_ = Physics.RaycastAll(transform.position, transform.forward, player.playerInventory.activeWeapon.range * 0.7f, layerMask: _layerMask, queryTriggerInteraction: QueryTriggerInteraction.Collide);

        if (_hits_.Length > 0)
            for (int i = 0; i < _hits_.Length; i++)
            {
                Log.Print($"AssignActorPlayerTargetOnShootingSphere {_hits_[i].transform.name}");
                if (_hits_[i].transform.GetComponent<AssignActorPlayerTargetOnShootingSphereTarget>())
                    _hits_[i].transform.GetComponent<AssignActorPlayerTargetOnShootingSphereTarget>().actor.AssignPlayerOnBulletNearby(player.photonId);

                break;
            }

        _cooldown = 0.3f;
    }
}
