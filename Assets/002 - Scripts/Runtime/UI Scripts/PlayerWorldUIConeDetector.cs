using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWorldUIConeDetector : MonoBehaviour
{
    [SerializeField] Player _player;
    [SerializeField] GameObject _obstruction;

    [SerializeField] LayerMask _layerMask;

    int distance = 10;

    private void OnTriggerEnter(Collider other)
    {
        try
        {
            Debug.Log(other.name);

            other.GetComponent<Player>().OnPlayerDeath += OnPlayerDeath;


            other.GetComponent<AllPlayerScripts>().worldUis[_player.controllerId].holder.gameObject.SetActive(true);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        try
        {
            Debug.Log(other.name);

            other.GetComponent<Player>().OnPlayerDeath -= OnPlayerDeath;

            if (GameManager.instance.teamMode.ToString().Contains("Classic"))
            {
                if (other.GetComponent<Player>().team != _player.team)
                    other.GetComponent<AllPlayerScripts>().worldUis[_player.controllerId].holder.gameObject.SetActive(false);
            }
            else
            {
                other.GetComponent<AllPlayerScripts>().worldUis[_player.controllerId].holder.gameObject.SetActive(false);
            }

        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    private void Update()
    {
        var hit = new RaycastHit();
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out hit, distance, _layerMask))
        {
            if (hit.transform.gameObject.layer == 0)
            {
                _obstruction = hit.transform.gameObject;
            }
            else
            {
                _obstruction = hit.transform.gameObject;
            }
        }
        else
        {
            _obstruction = null;
        }



    }

    void OnPlayerDeath(Player player)
    {
        try
        {
            if (GameManager.instance.teamMode.ToString().Contains("Classic"))
            {
                if (player.GetComponent<Player>().team != _player.team)
                    player.GetComponent<AllPlayerScripts>().worldUis[_player.controllerId].holder.gameObject.SetActive(false);
            }
            else
            {
                player.GetComponent<AllPlayerScripts>().worldUis[_player.controllerId].holder.gameObject.SetActive(false);
            }

            player.OnPlayerDeath -= OnPlayerDeath;
        }
        catch { }
    }
}
