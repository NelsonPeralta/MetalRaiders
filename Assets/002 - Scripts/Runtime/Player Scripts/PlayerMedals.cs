using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMedals : MonoBehaviour
{
    public Player player { get { return transform.root.GetComponent<Player>(); } }

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

            if (_spree == 3)
                SpawnKillingSpreeMedal();
        }
    }

    [SerializeField] Announcer announcer;
    [SerializeField] Transform grid;

    [SerializeField] Transform headshotMedalPrefab;
    [SerializeField] Transform nutshotMedalPrefab;
    [SerializeField] Transform meleeMedalPrefab;
    [SerializeField] Transform grenadeMedalPrefab;

    [SerializeField] Transform doubleKillMedalPrefab;
    [SerializeField] Transform trippleKillMedalPrefab;

    [SerializeField] Transform killingSpreeMedalPrefab;


    [SerializeField] int _spree;
    [SerializeField] int _shortKillSpree;
    [SerializeField] float _spreeTtl;

    private void Start()
    {
        player.OnPlayerDeath += OnPlayerDeath_Delegate;
        player.OnPlayerRespawned += OnPlayerRespawn_Delegate;

        announcer = player.allPlayerScripts.announcer;
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
        Transform h = Instantiate(headshotMedalPrefab, grid);
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
