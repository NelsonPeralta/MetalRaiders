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
            GameObject _previous = _rayHit;
            _rayHit = value;


            try
            {
                _retfric = value.GetComponent<ReticuleFriction>(); if (_retfric == null) _retfric = value.GetComponent<Player>().reticuleFriction;
                if (_retfric)
                {
                    if (_retfric.player != _player)
                        _retfric.player.GetComponent<AllPlayerScripts>().worldUis[_player.controllerId].seen = true;
                }
            }
            catch (System.Exception e) { /*Debug.LogWarning(e);*/ }
        }
    }

    [SerializeField] Player _player;
    [SerializeField] GameObject _rayHit;

    [SerializeField] LayerMask _targetLayerMask;
    [SerializeField] LayerMask _obsLayerMask;

    [SerializeField] int _distance = 25;

    ReticuleFriction _retfric;

    private void Update()
    {
        if (!_player.isMine)
            return;

        try { if (_player.isDead || _player.isRespawning) { rayHit = null; return; } } catch { }
        try { _distance = (int)_player.playerInventory.activeWeapon.currentRedReticuleRange; } catch { _distance = 15; }

        var hit = new RaycastHit();
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out hit, _distance, _targetLayerMask))
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
