using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

[Serializable]
public struct WinningPlayerStruct
{
    public long steamId => _steamId;
    public Player playerScript => _playerScript;
    public string message { get { return _message; } set { _message = value; } }
    public int rewiredId { get { return _rewiredId; } }

    [SerializeField] Player _playerScript;
    [SerializeField] long _steamId;
    [SerializeField] string _playerName;
    [SerializeField] int _rewiredId; 
    [SerializeField] string _message;


    public WinningPlayerStruct(Player player, long steamId, string playerName, int rewiredId, string message)
    {
        _playerScript = player;
        _steamId = steamId;
        _rewiredId = rewiredId;
        _playerName = playerName;
        _message = message;
    }
}
