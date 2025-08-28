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

    [SerializeField] Player _playerScript;
    [SerializeField] long _steamId;
    [SerializeField] string _playerName, _message;


    public WinningPlayerStruct(Player player, long steamId, string playerName, string message)
    {
        _playerScript = player;
        _steamId = steamId;
        _playerName = playerName;
        _message = message;
    }
}
