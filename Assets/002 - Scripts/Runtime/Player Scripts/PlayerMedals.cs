using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMedals : MonoBehaviour
{
    public Player player { get { return transform.root.GetComponent<Player>(); } }

    [SerializeField] Transform grid;
    [SerializeField] Transform headshotMedalPrefab;
    [SerializeField] Transform meleeMedalPrefab;
    [SerializeField] Transform grenadeMedalPrefab;

    public void SpawnHeadshotMedal()
    {
        Transform h = Instantiate(headshotMedalPrefab, grid);
        h.SetAsFirstSibling();
    }

    public void SpawnMeleeMedal()
    {
        Transform h = Instantiate(meleeMedalPrefab, grid);
        h.SetAsFirstSibling();
    }

    public void SpawnGrenadeMedal()
    {
        Transform h = Instantiate(grenadeMedalPrefab, grid);
        h.SetAsFirstSibling();
    }
}
