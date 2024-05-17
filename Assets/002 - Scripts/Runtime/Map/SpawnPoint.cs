using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public static int SeenResetTime = 1;

    public enum Layer { Alpha, Beta }
    public bool occupied { get { return players.Count > 0; } }
    public bool seen
    {
        get { return _seen; }
        set
        {
            if (value == true)
            {
                _seen = value;
                _seenReset = SeenResetTime;
            }
        }
    }


    public Layer layer;
    public List<Player> players = new List<Player>();
    public enum SpawnPointType { Player, Computer }
    public SpawnPointType spawnPointType;

    [SerializeField] GameObject _witness;
    [SerializeField] int _radius;

    [SerializeField] bool _seen;
    [SerializeField] float _seenReset;

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log($"Spawnpoint OnTriggerEnter: {other.name}");
        if (other.GetComponent<PlayerCapsule>() && other.gameObject.activeInHierarchy)
        {
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
        //Debug.Log($"SpawnPoint OnTriggerExit {other.name}");
        if (other.GetComponent<PlayerCapsule>() && other.gameObject.activeInHierarchy && players.Contains(other.transform.root.GetComponent<Player>()))
        {
            //Debug.Log($"SpawnPoint OnTriggerExit REMOVING PLAYER");
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


    private void Update()
    {
        if (_seenReset > 0)
        {
            _seenReset -= Time.deltaTime;

            if (_seenReset <= 0)
            {
                _seen = false;
            }
        }
    }



    //_players = Physics.OverlapSphere(transform.position, 25, 7).ToList().Select(collider => collider.GetComponent<PlayerCapsule>()).Where(g=>g!= null).ToList();

}