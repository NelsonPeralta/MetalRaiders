using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class HillManager : MonoBehaviour
{
    [SerializeField] AudioClip _hillMoved;
    [SerializeField] Hill _hill;
    [SerializeField] List<GameObject> _locations;

    [SerializeField] float _moveTimer;
    bool _allPlayersJoined;
    int c;
    int _hillTtl = 45;

    private void Awake()
    {
        _hill = FindObjectOfType<Hill>();

        _locations.Clear();
        foreach (Transform child in transform)
        {
            _locations.Add(child.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.instance.gameType == GameManager.GameType.Hill)
        {
            _hill.gameObject.transform.position = _locations[0].transform.position;
            _moveTimer = _hillTtl;
            CurrentRoomManager.instance.OnGameIsReady -= OnGameIsReady_Delegate;
            CurrentRoomManager.instance.OnGameIsReady += OnGameIsReady_Delegate;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_allPlayersJoined)
        {
            if (_moveTimer > 0)
            {
                _moveTimer -= Time.deltaTime;

                if (_moveTimer <= 0)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        NetworkGameManager.instance.NextHillLocation();
                    }
                }
            }
        }
    }


    void OnGameIsReady_Delegate(CurrentRoomManager gme)
    {
        Debug.Log("OnAllPlayersJoinedRoom_Delegate");

        _allPlayersJoined = true;
        _moveTimer = _hillTtl;
    }


    public void NextLocation()
    {
        c++;
        _hill.gameObject.transform.position = _locations[c].transform.position;

        if (c == _locations.Count - 1)
            c = -1;

        _moveTimer = _hillTtl;

        GameManager.GetRootPlayer().announcer.AddClip(_hillMoved);
    }
}
