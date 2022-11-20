using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public List<Player> players = new List<Player>();
    public enum SpawnPointType { Player, Computer }
    public SpawnPointType spawnPointType;

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<Player>())
        {
            if (!players.Contains(other.GetComponent<Player>()))
            {
                if (GameManager.instance.isDev)
                {
                    GameObject wit = gameObject.transform.Find("Witness").gameObject;
                    wit.SetActive(true);
                }

                other.GetComponent<Player>().OnPlayerDeath -= OnPLayerDeath;
                other.GetComponent<Player>().OnPlayerDeath += OnPLayerDeath;
                players.Add(other.GetComponent<Player>());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Player>() && players.Contains(other.GetComponent<Player>()))
        {
            other.GetComponent<Player>().OnPlayerDeath -= OnPLayerDeath;
            players.Remove(other.GetComponent<Player>());

            if (players.Count == 0 && GameManager.instance.isDev)
            {
                GameObject wit = gameObject.transform.Find("Witness").gameObject;
                wit.SetActive(false);
            }
        }
    }

    void OnPLayerDeath(Player p)
    {
        try
        {
            p.OnPlayerDeath -= OnPLayerDeath;
            players.Remove(p);
        }
        catch { }

        //if (players.Count == 0)
        //{
        //    GameObject wit = gameObject.transform.Find("Witness").gameObject;
        //    wit.SetActive(false);
        //}
    }
}