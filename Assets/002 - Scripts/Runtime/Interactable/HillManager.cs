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

    float _moveTimer;
    bool _allPlayersJoined;
    int c;
    int _hillTtl = 45;

    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.instance.gameType == GameManager.GameType.Hill)
        {
            _hill.gameObject.transform.position = _locations[0].transform.position;
            _moveTimer = _hillTtl;
            FindObjectOfType<GameManagerEvents>().OnAllPlayersJoinedRoom -= OnAllPlayersJoinedRoom_Delegate;
            FindObjectOfType<GameManagerEvents>().OnAllPlayersJoinedRoom += OnAllPlayersJoinedRoom_Delegate;
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


    void OnAllPlayersJoinedRoom_Delegate(GameManagerEvents gme)
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

        GameManager.GetMyPlayer().announcer.AddClip(_hillMoved);
    }
}