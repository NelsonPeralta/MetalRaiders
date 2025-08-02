using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public static int SeenResetTime = 1;

    public bool constested { get { return players.Count > 0; } }
    //public bool seen
    //{
    //    get { return _seen; }
    //    set
    //    {
    //        if (value == true)
    //        {
    //            _seen = value;
    //            _seenReset = SeenResetTime;
    //        }
    //    }
    //}

    public bool reserved
    {
        get { return _reservedReset > 0; }
        set
        {
            _reserved = value;
            if (value) _reservedReset = 4;

        }
    }

    public int dangerLevel { get { return _dangerLevel; } }

    public GameManager.Team team { get { return _team; } }


    public List<Player> players = new List<Player>();
    public enum SpawnPointType { Player, Computer }
    public SpawnPointType spawnPointType;
    [SerializeField] GameManager.Team _team;

    [SerializeField] int _radius;

    [SerializeField] bool /*_seen,*/ _reserved;
    //[SerializeField] float _seenReset;
    [SerializeField] int _dangerLevel;
    [SerializeField] List<DangerEntry> _dangerEntries = new List<DangerEntry>();

    float _reservedReset;
    float _evaluateDangerCooldown;

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
        for (int i = _dangerEntries.Count - 1; i >= 0; i--)
        {
            if (_dangerEntries[i].ttl > 0) _dangerEntries[i].ttl -= Time.deltaTime;
            if (_dangerEntries[i].ttl <= 0) _dangerEntries.RemoveAt(i);
        }

        if (_evaluateDangerCooldown > 0)
        {
            _evaluateDangerCooldown -= Time.deltaTime;

            if (_evaluateDangerCooldown <= 0)
            {
                _dangerLevel = 0;

                if (_dangerEntries.Count > 0)
                {
                    for (int i = _dangerEntries.Count - 1; i >= 0; i--)
                        _dangerLevel += _dangerEntries[i].dangerLevel;

                    _evaluateDangerCooldown = SeenResetTime * 0.3f;
                }
            }
        }




        if (_reservedReset > 0)
        {
            _reservedReset -= Time.deltaTime;

            if (_reservedReset < 0)
            {
                reserved = false;
            }
        }




        //if (_seenReset > 0)
        //{
        //    _seenReset -= Time.deltaTime;

        //    if (_seenReset <= 0)
        //    {
        //        _seen = false;
        //    }
        //}
    }

    public void AddDanger(int idd, int levl, int ttll)
    {
        if (_dangerEntries.Count == 0) _evaluateDangerCooldown = SeenResetTime * 0.3f;

        for (int i = 0; i < _dangerEntries.Count; i++)
        {
            if (_dangerEntries[i].id == idd)
            {
                _dangerEntries[i].ttl = SeenResetTime;
                return;
            }
        }

        _dangerEntries.Add(new DangerEntry(idd, levl, ttll));
    }


    [Serializable]
    class DangerEntry
    {
        public int id;
        public int dangerLevel;
        public float ttl;

        public DangerEntry(int idd, int lvl, int tt)
        {
            id = idd;
            dangerLevel = lvl;
            ttl = tt;
        }
    }

    //_players = Physics.OverlapSphere(transform.position, 25, 7).ToList().Select(collider => collider.GetComponent<PlayerCapsule>()).Where(g=>g!= null).ToList();

}