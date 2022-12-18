using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWorldUIConeDetector : MonoBehaviour
{
    GameObject obstruction
    {
        get { return _obstruction; }
        set
        {
            _unfilteredHit = value;
            GameObject _previous = _obstruction;

            try
            {
                if (value.GetComponent<ReticuleFriction>())
                {
                    _obstruction = value;
                    if (GameManager.instance.teamMode.ToString().Contains("Classic"))
                    {
                        if (_obstruction.GetComponent<ReticuleFriction>().player.team != _player.team)
                            _obstruction.GetComponent<ReticuleFriction>().player.GetComponent<AllPlayerScripts>().worldUis[_player.controllerId].holder.gameObject.SetActive(true);
                    }
                    else
                    {
                        _obstruction.GetComponent<ReticuleFriction>().player.GetComponent<AllPlayerScripts>().worldUis[_player.controllerId].holder.gameObject.SetActive(true);
                    }
                }
            }
            catch (System.Exception e) { Debug.LogWarning(e); }




            try
            {
                if (value != _previous && _previous.GetComponent<ReticuleFriction>())
                    if (GameManager.instance.teamMode.ToString().Contains("Classic"))
                    {
                        if (_previous.GetComponent<ReticuleFriction>().player.team != _player.team)
                            _previous.GetComponent<ReticuleFriction>().player.GetComponent<AllPlayerScripts>().worldUis[_player.controllerId].holder.gameObject.SetActive(false);
                    }
                    else
                    {
                        _previous.GetComponent<ReticuleFriction>().player.GetComponent<AllPlayerScripts>().worldUis[_player.controllerId].holder.gameObject.SetActive(false);
                    }
            }
            catch (System.Exception e) { /*Debug.LogWarning(e);*/ }
        }
    }

    [SerializeField] Player _player;
    [SerializeField] GameObject _obstruction;
    [SerializeField] GameObject _unfilteredHit;

    [SerializeField] LayerMask _layerMask;

    [SerializeField]  int _distance = 25;

    private void Update()
    {
        try { if (_player.isDead || _player.isRespawning) { obstruction = null; return; } } catch { }
        try { _distance = (int)_player.playerInventory.activeWeapon.currentRedReticuleRange; } catch { _distance = 15; }

        var hit = new RaycastHit();
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out hit, _distance, _layerMask))
        {
            if (hit.transform.gameObject.layer == 0)
            {
                obstruction = hit.transform.gameObject;
            }
            else
            {
                obstruction = hit.transform.gameObject;
            }
        }
        else
        {
            obstruction = null;
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
