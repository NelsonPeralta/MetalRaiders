using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public static int SeenResetTime = 1;

    public enum Layer { Alpha, Beta }
    public bool constested { get { return players.Count > 0; } }
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

    public bool reserved
    {
        get { return _reservedReset > 0; }
        set
        {
            _reserved = value;
            if (value) _reservedReset = 4;

        }
    }

    public GameManager.Team team { get { return _team; } }


    public Layer layer;
    public List<Player> players = new List<Player>();
    public enum SpawnPointType { Player, Computer }
    public SpawnPointType spawnPointType;
    [SerializeField] GameManager.Team _team;

    [SerializeField] int _radius;

    [SerializeField] bool _seen, _reserved;
    [SerializeField] float _seenReset;

    float _reservedReset;

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log($"Spawnpoint OnTriggerEnter: {other.name}");
        if (other.GetComponent<PlayerCapsule>() && other.gameObject.activeInHierarchy)
        {
            Player player = other.transform.root.GetComponent<Player>();

            if (!players.Contains(player))
            {

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


            //if (players.Count == 0)
            //{
            //    print("Unreserve spawn point");
            //    _reserved = false;
            //}
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
        if (_reservedReset > 0)
        {
            _reservedReset -= Time.deltaTime;

            if (_reservedReset < 0)
            {
                reserved = false;
            }
        }




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