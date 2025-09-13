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

    public int blockingLevel { get { return _blockingLevel; } }

    public GameManager.Team team { get { return _team; } }


    public List<Player> players = new List<Player>();
    public enum SpawnPointType { Player, Computer }
    public SpawnPointType spawnPointType;
    [SerializeField] GameManager.Team _team;

    [SerializeField] int _radius;

    [SerializeField] bool /*_seen,*/ _reserved;
    //[SerializeField] float _seenReset;
    [SerializeField] int _blockingLevel;
    [SerializeField] List<BlockingLevelEntry> _blockingLevelEntries = new List<BlockingLevelEntry>();

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
            //    PrintOnlyInEditor.Log("Unreserve spawn point");
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
        for (int i = _blockingLevelEntries.Count - 1; i >= 0; i--)
        {
            if (_blockingLevelEntries[i].ttl > 0) _blockingLevelEntries[i].ttl -= Time.deltaTime;
            if (_blockingLevelEntries[i].ttl <= 0) _blockingLevelEntries.RemoveAt(i);
        }

        if (_evaluateDangerCooldown > 0)
        {
            _evaluateDangerCooldown -= Time.deltaTime;

            if (_evaluateDangerCooldown <= 0)
            {
                _blockingLevel = 0;

                if (_blockingLevelEntries.Count > 0)
                {
                    for (int i = _blockingLevelEntries.Count - 1; i >= 0; i--)
                        _blockingLevel += _blockingLevelEntries[i].blockingLevel;

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


    int MAX_BLOCKING_ENTRIES = 5;
    public void AddBlockingLevelEntry(int idd, int levl, float ttll)
    {
        if (_blockingLevelEntries.Count == 0) _evaluateDangerCooldown = SeenResetTime * 0.3f;

        for (int i = 0; i < _blockingLevelEntries.Count; i++)
        {
            if (_blockingLevelEntries[i].id == idd)
            {
                _blockingLevelEntries[i].ttl = SeenResetTime;
                return;
            }
        }


        if (_blockingLevelEntries.Count < MAX_BLOCKING_ENTRIES)
        {
            _blockingLevelEntries.Add(new BlockingLevelEntry(idd, levl, ttll));
            _blockingLevelEntries.Sort((a, b) => b.blockingLevel.CompareTo(a.blockingLevel));
        }
        else
        {
            if (_blockingLevelEntries[MAX_BLOCKING_ENTRIES - 1].blockingLevel < levl)
            {
                _blockingLevelEntries[MAX_BLOCKING_ENTRIES - 1].id = idd;
                _blockingLevelEntries[MAX_BLOCKING_ENTRIES - 1].blockingLevel = levl;
                _blockingLevelEntries[MAX_BLOCKING_ENTRIES - 1].ttl = ttll;

                _blockingLevelEntries.Sort((a, b) => b.blockingLevel.CompareTo(a.blockingLevel));
            }
            else
            {
                // do nothing
            }
        }
    }


    [Serializable]
    class BlockingLevelEntry
    {
        public int id;
        public int blockingLevel;
        public float ttl;

        public BlockingLevelEntry(int idd, int lvl, float tt)
        {
            id = idd;
            blockingLevel = lvl;
            ttl = tt;
        }
    }

    //_players = Physics.OverlapSphere(transform.position, 25, 7).ToList().Select(collider => collider.GetComponent<PlayerCapsule>()).Where(g=>g!= null).ToList();

}