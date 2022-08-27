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
        }
    }

    void OnPLayerDeath(Player p)
    {
        p.OnPlayerDeath -= OnPLayerDeath;
        players.Remove(p);
    }
}