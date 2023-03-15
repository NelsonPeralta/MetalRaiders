using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMedals : MonoBehaviour
{
    public Player player { get { return transform.root.GetComponent<Player>(); } }

    public int spree { get { return _spree; } }
    public int kills
    {
        get { return _shortKillSpree; }
        set
        {
            _spree++;
            _shortKillSpree = value;
            _spreeTtl = 4;

            if (_shortKillSpree == 2)
                SpawnDoubleKillMedal();
            if (_shortKillSpree == 3)
                SpawnTripleKillMedal();

            if (_spree == 5)
                SpawnKillingSpreeMedal();
        }
    }

    [SerializeField] Announcer announcer;
    [SerializeField] Transform grid;

    [SerializeField] Transform headshotMedalPrefab;
    [SerializeField] Transform nutshotMedalPrefab;
    [SerializeField] Transform meleeMedalPrefab;
    [SerializeField] Transform grenadeMedalPrefab;
    [SerializeField] Transform stuckMedalPrefab;

    [SerializeField] Transform doubleKillMedalPrefab;
    [SerializeField] Transform trippleKillMedalPrefab;

    [SerializeField] Transform killingSpreeMedalPrefab;

    [SerializeField] Transform killjoyMedalPrefab;


    [SerializeField] int _spree;
    [SerializeField] int _shortKillSpree;
    [SerializeField] float _spreeTtl;

    private void Start()
    {
        player.OnPlayerDeath += OnPlayerDeath_Delegate;
        player.OnPlayerRespawned += OnPlayerRespawn_Delegate;

        announcer = player.allPlayerScripts.announcer;
        if(!player.isMine)
            announcer.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_spreeTtl > 0)
        {
            _spreeTtl -= Time.deltaTime;

            if (_spreeTtl <= 0)
                _shortKillSpree = 0;
        }
    }

    public void SpawnHeadshotMedal()
    {
        Transform h = Instantiate(headshotMedalPrefab, grid);
        h.SetAsFirstSibling();
        kills++;
    }

    public void SpawnNutshotMedal()
    {
        Transform h = Instantiate(nutshotMedalPrefab, grid);
        h.SetAsFirstSibling();
        kills++;
    }

    public void SpawnMeleeMedal()
    {
        Transform h = Instantiate(meleeMedalPrefab, grid);
        h.SetAsFirstSibling();
        kills++;
    }

    public void SpawnGrenadeMedal()
    {
        Transform h = Instantiate(grenadeMedalPrefab, grid);
        h.SetAsFirstSibling();
        kills++;
    }

    public void SpawnStuckKillMedal()
    {
        Transform h = Instantiate(stuckMedalPrefab, grid);
        h.SetAsFirstSibling();
        kills++;
    }





    void SpawnDoubleKillMedal()
    {
        Transform h = Instantiate(doubleKillMedalPrefab, grid);
        h.SetAsFirstSibling();

        announcer.AddClip(h.GetComponent<PlayerMedal>().clip);
    }

    void SpawnTripleKillMedal()
    {
        Transform h = Instantiate(trippleKillMedalPrefab, grid);
        h.SetAsFirstSibling();

        announcer.AddClip(h.GetComponent<PlayerMedal>().clip);
    }






    void SpawnKillingSpreeMedal()
    {
        Transform h = Instantiate(killingSpreeMedalPrefab, grid);
        h.SetAsFirstSibling();

        announcer.AddClip(h.GetComponent<PlayerMedal>().clip);
    }


    public void SpawnKilljoySpreeMedal()
    {
        Transform h = Instantiate(killjoyMedalPrefab, grid);
        h.SetAsFirstSibling();

        announcer.AddClip(h.GetComponent<PlayerMedal>().clip);
    }




    void OnPlayerDeath_Delegate(Player player)
    {
        _spree = 0;
    }

    void OnPlayerRespawn_Delegate(Player player)
    {
        _spree = 0;
        _shortKillSpree = 0;
    }
}
