using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDatabaseAdaptor
{
    PlayerUserData playerData;
    PlayerBasicPvPStats playerBasicPvPStats;
    PlayerBasicPvEStats playerBasicPvEStats;

    // ********* SETTERS **********
    public void SetPlayerData(PlayerUserData playerData)
    {
        if (this.playerData == null)
        {
            this.playerData = playerData;
        }
        else
            Debug.LogError("PlayerData already set");
    }

    public void SetPlayerBasicPvPStats(PlayerBasicPvPStats playerData)
    {
        if (this.playerBasicPvPStats == null)
        {
            this.playerBasicPvPStats = playerData;
            Debug.Log($"Player basic pvp stats are set. Kills: {this.playerBasicPvPStats.kills}. Deaths: {this.playerBasicPvPStats.deaths}. Headshots: {this.playerBasicPvPStats.headshots}");
        }
        else
            Debug.LogError("PlayerData already set");
    }

    public void SetPlayerBasicPvEStats(PlayerBasicPvEStats playerData)
    {
        if (this.playerBasicPvEStats == null)
        {
            this.playerBasicPvEStats = playerData;
            Debug.Log($"Player basic pve stats are set. Kills: {this.playerBasicPvEStats.kills}. Deaths: {this.playerBasicPvEStats.deaths}. Headshots: {this.playerBasicPvEStats.headshots}");
        }
        else
            Debug.LogError("PlayerData already set");
    }

    // ********** GETTERS **********
    public string GetUsername() { return this.playerData.username; }
    public int GetKills() { return this.playerBasicPvEStats.kills; }
    public bool PlayerDataIsSet() { return playerData != null; }


    // ********* INNER CLASSES *********
    [System.Serializable]
    public class PlayerUserData
    {

        public int id;
        public string username;

        public static PlayerUserData CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<PlayerUserData>(jsonString);
        }

    }

    [System.Serializable]
    public class PlayerBasicPvPStats
    {
        int player_id;
        public int kills, deaths, headshots;

        public static PlayerBasicPvPStats CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<PlayerBasicPvPStats>(jsonString);
        }
    }

    [System.Serializable]
    public class PlayerBasicPvEStats
    {
        int player_id;
        public int kills, deaths, headshots;

        public static PlayerBasicPvEStats CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<PlayerBasicPvEStats>(jsonString);
        }
    }
}
