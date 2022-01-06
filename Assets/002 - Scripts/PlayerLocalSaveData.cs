using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerLocalSaveData
{
    public int playerID;

    public string characterName;
    public int kills;
    public int deaths;

    public PlayerLocalSaveData (MyPlayerInfo player)
    {
        playerID = player.playerID;

        characterName = player.name;
    }
}
