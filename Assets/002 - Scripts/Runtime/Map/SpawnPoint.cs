using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public List<Player> players = new List<Player>();
    public enum SpawnPointType { Player, Computer }
    public SpawnPointType spawnPointType;

    [SerializeField] GameObject _witness;
    [SerializeField] int _radius;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerCapsule>() && other.gameObject.activeInHierarchy)
        {
            //Debug.Log($"Spawnpoint OnTriggerEnter: {other.name}");
            Player player = other.transform.root.GetComponent<Player>();

            if (!players.Contains(player))
            {
                if (GameManager.instance.isDev)
                    _witness.SetActive(true);

                player.OnPlayerDeath -= OnPLayerDeath;
                player.OnPlayerDeath += OnPLayerDeath;
                players.Add(player);
            }
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerCapsule>() && other.gameObject.activeInHierarchy && players.Contains(other.transform.root.GetComponent<Player>()))
        {
            //Debug.Log($"SpawnPoint {other.name}");
            other.transform.root.GetComponent<Player>().OnPlayerDeath -= OnPLayerDeath;
            players.Remove(other.transform.root.GetComponent<Player>());

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

        if (players.Count == 0)
        {
            GameObject wit = gameObject.transform.Find("Witness").gameObject;
            wit.SetActive(false);
        }
    }




    //_players = Physics.OverlapSphere(transform.position, 25, 7).ToList().Select(collider => collider.GetComponent<PlayerCapsule>()).Where(g=>g!= null).ToList();

}