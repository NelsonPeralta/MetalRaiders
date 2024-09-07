using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class PlayerMedals : MonoBehaviour
{
    public static int MEDAL_TTL = 4;
    public Player player { get { return transform.root.GetComponent<Player>(); } }

    public int spree { get { return _spree; } }
    public int kills
    {
        get { return shortKillSpree; }
        set
        {
            _spree++;
            shortKillSpree = value;
            _spreeTtl = MEDAL_TTL;

            if (GameManager.instance.gameMode == GameManager.GameMode.Coop)
                _spreeTtl = 2;

            if (GameManager.instance.gameMode == GameManager.GameMode.Versus)
            {

                if (shortKillSpree == 2)
                    SpawnDoubleKillMedal();
                if (shortKillSpree == 3)
                    SpawnTripleKillMedal();
                if (shortKillSpree == 4)
                    SpawnOverKillMedal();

                if (_spree == 5)
                    SpawnKillingSpreeMedal();


                //if (GameManager.instance.gameType != GameManager.GameType.Hill
                //&& GameManager.instance.gameType != GameManager.GameType.Oddball
                //&& GameManager.instance.gameType != GameManager.GameType.GunGame)
                //{
                //    player.playerUI.ShowPointWitness(1);
                //}
            }
        }
    }

    int shortKillSpree
    {
        get { return _shortKillSpree; }
        set
        {
            int _preVal = _shortKillSpree;
            _shortKillSpree = value;

            if (value == 0 && GameManager.instance.gameMode == GameManager.GameMode.Coop)
            {
                if (_preVal == 2)
                    SpawnDoubleKillMedal();
                else if (_preVal == 3)
                    SpawnTripleKillMedal();
                else if (_preVal == 4)
                    SpawnOverKillMedal();
                else if (_preVal > 4)
                    SpawnMultiKillMedal();
            }
        }
    }

    [SerializeField] Announcer announcer;
    [SerializeField] Transform grid;

    [SerializeField] Transform headshotMedalPrefab;
    [SerializeField] Transform sniperHeadshotMedalPrefab;
    [SerializeField] Transform nutshotMedalPrefab;
    [SerializeField] Transform meleeMedalPrefab;
    [SerializeField] Transform assasinationMedalPrefab;
    [SerializeField] Transform grenadeMedalPrefab;
    [SerializeField] Transform stuckMedalPrefab;

    [SerializeField] Transform doubleKillMedalPrefab, tripleKillMedalPrefab, overKillMedalPrefab, _multiKillMedalPrefab;

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
        if (!player.isMine)
            announcer.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_spreeTtl > 0)
        {
            _spreeTtl -= Time.deltaTime;

            if (_spreeTtl <= 0)
                shortKillSpree = 0;
        }
    }

    public void SpawnHeadshotMedal()
    {
        Transform h = Instantiate(headshotMedalPrefab, grid);
        h.SetAsFirstSibling();
        kills++;
    }

    public void SpawnSniperHeadshotMedal()
    {
        Transform h = Instantiate(sniperHeadshotMedalPrefab, grid);
        h.SetAsFirstSibling();
        kills++;
        announcer.AddClip(h.GetComponent<PlayerMedal>().clip);
    }

    public void SpawnNutshotMedal()
    {
        Transform h = Instantiate(nutshotMedalPrefab, grid);
        h.SetAsFirstSibling();
        kills++;
        announcer.AddClip(h.GetComponent<PlayerMedal>().clip);
    }

    public void SpawnMeleeMedal()
    {
        Transform h = Instantiate(meleeMedalPrefab, grid);
        h.SetAsFirstSibling();
        kills++;
    }

    public void SpawnAssasinationMedal()
    {
        Transform h = Instantiate(assasinationMedalPrefab, grid);
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

        if (player.isMine)
            AchievementManager.instance.stickiesThisGame++;
    }





    void SpawnDoubleKillMedal()
    {
        Transform h = Instantiate(doubleKillMedalPrefab, grid);
        h.SetAsFirstSibling();

        announcer.AddClip(h.GetComponent<PlayerMedal>().clip);
    }

    void SpawnTripleKillMedal()
    {
        Transform h = Instantiate(tripleKillMedalPrefab, grid);
        h.SetAsFirstSibling();

        announcer.AddClip(h.GetComponent<PlayerMedal>().clip);
    }

    void SpawnOverKillMedal()
    {
        Transform h = Instantiate(overKillMedalPrefab, grid);
        h.SetAsFirstSibling();

        announcer.AddClip(h.GetComponent<PlayerMedal>().clip);
    }
    void SpawnMultiKillMedal()
    {
        Transform h = Instantiate(_multiKillMedalPrefab, grid);
        h.SetAsFirstSibling();

        announcer.AddClip(h.GetComponent<PlayerMedal>().clip);
    }





    void SpawnKillingSpreeMedal()
    {
        Transform h = Instantiate(killingSpreeMedalPrefab, grid);
        h.SetAsFirstSibling();

        announcer.AddClip(h.GetComponent<PlayerMedal>().clip);



        bool _achUnlocked = false;
    }


    public void SpawnKilljoySpreeMedal()
    {
        Transform h = Instantiate(killjoyMedalPrefab, grid);
        h.SetAsFirstSibling();

        announcer.AddClip(h.GetComponent<PlayerMedal>().clip);
    }




    void OnPlayerDeath_Delegate(Player player)
    {
        Debug.Log("OnPlayerDeath_Delegate");
        _spree = 0;
    }

    void OnPlayerRespawn_Delegate(Player player)
    {
        _spree = 0;
        shortKillSpree = 0;
    }
}
