using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct WinningPlayerStruct
{
    [SerializeField] Player _playerScript;
    [SerializeField] long _steamId;
    [SerializeField] string _playerName;


    public WinningPlayerStruct(Player player, long steamId, string playerName)
    {
        _playerScript = player;
        _steamId = steamId;
        _playerName = playerName;
    }
}
