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

            //try
            //{
            //    if (value.GetComponent<ReticuleFriction>() || value.GetComponent<Player>().reticuleFriction)
            //    {
            //        _rayHit = value;
            //        if (GameManager.instance.teamMode.ToString().Contains("Classic"))
            //        {
            //            if (_rayHit.GetComponent<Player>().team != _player.team)
            //                _rayHit.GetComponent<AllPlayerScripts>().worldUis[_player.controllerId].holder.gameObject.SetActive(true);
            //        }
            //        else
            //        {
            //            _rayHit.GetComponent<AllPlayerScripts>().worldUis[_player.controllerId].holder.gameObject.SetActive(true);
            //        }
            //    }
            //}
            //catch (System.Exception e) { /*Debug.LogWarning(e);*/ }




            //try
            //{
            //    if (value != _previous && (_previous.GetComponent<ReticuleFriction>() || value.GetComponent<Player>().reticuleFriction))
            //        if (GameManager.instance.teamMode.ToString().Contains("Classic"))
            //        {
            //            if (_previous.GetComponent<Player>().team != _player.team)
            //                _previous.GetComponent<AllPlayerScripts>().worldUis[_player.controllerId].holder.gameObject.SetActive(false);
            //        }
            //        else
            //        {
            //            _previous.GetComponent<AllPlayerScripts>().worldUis[_player.controllerId].holder.gameObject.SetActive(false);
            //        }
            //}
            //catch (System.Exception e) { /*Debug.LogWarning(e);*/ }

            //if (value == null)
            //{
            //    try
            //    {
            //        _previous.GetComponent<AllPlayerScripts>().worldUis[_player.controllerId].holder.gameObject.SetActive(false);
            //    }
            //    catch { }
            //}



            try
            {
                _retfric = value.GetComponent<ReticuleFriction>(); if (_retfric == null) _retfric = value.GetComponent<Player>().reticuleFriction;
                if (_retfric)
                    _retfric.player.GetComponent<AllPlayerScripts>().worldUis[_player.controllerId].seen = true;
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
                Debug.Log($"PlayerWorldUIMarkerDetector: {hit.transform.gameObject.name}");
                rayHit = hit.transform.gameObject;
            }
            else
            {
                rayHit = null;

            }

            //if (hit.transform.gameObject.layer == 0)
            //{
            //    obstruction = hit.transform.gameObject;
            //}
            //else
            //{
            //    obstruction = hit.transform.gameObject;
            //}
        }
        else
        {
            rayHit = null;
        }



    }

    void OnPlayerDeath(Player player)
    {
        //try
        //{
        //    if (GameManager.instance.teamMode.ToString().Contains("Classic"))
        //    {
        //        if (player.GetComponent<Player>().team != _player.team)
        //            player.GetComponent<AllPlayerScripts>().worldUis[_player.controllerId].holder.gameObject.SetActive(false);
        //    }
        //    else
        //    {
        //        player.GetComponent<AllPlayerScripts>().worldUis[_player.controllerId].holder.gameObject.SetActive(false);
        //    }

        //    player.OnPlayerDeath -= OnPlayerDeath;
        //}
        //catch { }
    }
}
