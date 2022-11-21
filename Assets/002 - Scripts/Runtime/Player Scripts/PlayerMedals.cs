using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMedals : MonoBehaviour
{
    public Player player { get { return transform.root.GetComponent<Player>(); } }

    public int spree
    {
        get { return _spree; }
        set
        {
            _spree = value; _spreeTtl = 4;
            if (_spree == 2)
                SpawnDoubleKillMedal();
            if (_spree == 3)
                SpawnTripleKillMedal();
        }
    }

    [SerializeField] Transform grid;

    [SerializeField] Transform headshotMedalPrefab;
    [SerializeField] Transform meleeMedalPrefab;
    [SerializeField] Transform grenadeMedalPrefab;

    [SerializeField] Transform doubleKillMedalPrefab;
    [SerializeField] Transform trippleKillMedalPrefab;

    [SerializeField] int _spree;
    [SerializeField] float _spreeTtl;

    private void Update()
    {
        if (_spreeTtl > 0)
        {
            _spreeTtl -= Time.deltaTime;

            if (_spreeTtl <= 0)
                _spree = 0;
        }
    }

    public void SpawnHeadshotMedal()
    {
        Transform h = Instantiate(headshotMedalPrefab, grid);
        h.SetAsFirstSibling();
        spree++;
    }

    public void SpawnMeleeMedal()
    {
        Transform h = Instantiate(meleeMedalPrefab, grid);
        h.SetAsFirstSibling();
        spree++;
    }

    public void SpawnGrenadeMedal()
    {
        Transform h = Instantiate(grenadeMedalPrefab, grid);
        h.SetAsFirstSibling();
        spree++;
    }







    void SpawnDoubleKillMedal()
    {
        Transform h = Instantiate(doubleKillMedalPrefab, grid);
        h.SetAsFirstSibling();
    }

    void SpawnTripleKillMedal()
    {
        Transform h = Instantiate(trippleKillMedalPrefab, grid);
        h.SetAsFirstSibling();
    }
}
