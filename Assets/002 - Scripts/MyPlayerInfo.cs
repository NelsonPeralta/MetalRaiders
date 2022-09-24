using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayerInfo : MonoBehaviour
{
    public int playerID;
    public int characterID;

    public string characterName;
    public int kills;
    public int deaths;

    public void SavePlayer()
    {
        SaveSystem.SavePlayer(this);
    }

    public void LoadPlayer()
    {
        PlayerLocalSaveData data = SaveSystem.LoadPlayer(playerID, characterID, this);
    }
}
