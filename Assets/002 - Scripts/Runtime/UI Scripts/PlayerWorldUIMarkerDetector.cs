using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWorldUIMarkerDetector : MonoBehaviour
{
    GameObject rayHit
    {
        get { return _rayHit; }
        set
        {
            _rayHit = value;
            if (value == null) return;


            // try to get ReticuleFriction on the target; if not present, fall back to Player.reticuleFriction
            _retfric = _rayHit.GetComponent<ReticuleFriction>();
            if (_retfric == null)
            {
                var p = _rayHit.GetComponent<Player>();
                if (p != null) _retfric = p.reticuleFriction;
            }

            // apply 'seen' flag if appropriate
            if (_retfric != null && _retfric.player != _player)
            {
                var aps = _retfric.player.GetComponent<AllPlayerScripts>();
                if (aps != null)
                    aps.worldUis[_player.controllerId].seen = true;
            }
        }
    }

    [SerializeField] Player _player;
    [SerializeField] GameObject _rayHit;

    [SerializeField] LayerMask _targetLayerMask;
    [SerializeField] LayerMask _obsLayerMask;

    [SerializeField] int _distance = 25;

    ReticuleFriction _retfric;

    RaycastHit hit;

    private void Update()
    {
        if (!_player.isMine)
            return;

        if (_player.isDead || _player.isRespawning)
        {
            rayHit = null;
            return;
        }

        // safer access to active weapon range without exceptions
        _distance = 15;
        if (_player.playerInventory != null && _player.playerInventory.activeWeapon != null)
        {
            _distance = (int)_player.playerInventory.activeWeapon.currentRedReticuleRange;
        }



        if (Physics.Raycast(transform.position, transform.forward, out hit, _distance, _targetLayerMask))
        {
            bool canSeePlayer = false;

            Transform target = hit.transform;
            Transform or = transform;

            Vector3 directionToTarget = (target.position - or.position).normalized;
            float distanceToTarget = Vector3.Distance(or.position, target.position);

            if (!Physics.Raycast(or.position, directionToTarget, distanceToTarget, _obsLayerMask))
                canSeePlayer = true;

            if (canSeePlayer)
            {
                //Debug.Log($"PlayerWorldUIMarkerDetector: {hit.transform.gameObject.name}");
                rayHit = hit.transform.gameObject;
            }
            else
            {
                rayHit = null;

            }


        }
        else
        {
            rayHit = null;
        }



    }

    void OnPlayerDeath(Player player)
    {

    }
}
