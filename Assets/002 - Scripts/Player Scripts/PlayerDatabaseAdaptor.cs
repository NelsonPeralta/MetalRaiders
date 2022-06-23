using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDatabaseAdaptor
{
    PlayerUserData playerData;
    PlayerBasicOnlineStats _playerBasicOnlineStats;
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
        this.playerBasicPvPStats = playerData;
        Debug.Log($"Player basic pvp stats are set. Kills: {this.playerBasicPvPStats.kills}. Deaths: {this.playerBasicPvPStats.deaths}. Headshots: {this.playerBasicPvPStats.headshots}");
    }

    public void SetPlayerBasicPvEStats(PlayerBasicPvEStats playerData)
    {
        this.playerBasicPvEStats = playerData;
        Debug.Log($"Player basic pve stats are set. Kills: {this.playerBasicPvEStats.kills}. Deaths: {this.playerBasicPvEStats.deaths}. Headshots: {this.playerBasicPvEStats.headshots}");
    }

    // ********** GETTERS **********
    public int GetId() { return playerData.id; }
    public string GetUsername() { return playerData.username; }
    // Multiplayer
    public int GetPvPKills() { return playerBasicPvPStats.kills; }
    public int GetPvPDeaths() { return playerBasicPvPStats.deaths; }
    public int GetPvPHeadshots() { return playerBasicPvPStats.headshots; }
    // PvE
    public int GetPvEKills() { return playerBasicPvEStats.kills; }
    public int GetPvEDeaths() { return playerBasicPvEStats.deaths; }
    public int GetPvEHeadshots() { return playerBasicPvEStats.headshots; }
    public int GetPvEHighestPoints() { return playerBasicPvEStats.highest_points; }
    public bool PlayerDataIsSet() { return playerData != null; }


    // **************** Properties **************** //
    public int level
    {
        get { return _playerBasicOnlineStats.level; }
    }

    public int xp
    {
        get { return _playerBasicOnlineStats.xp; }
    }

    public int credits
    {
        set { _playerBasicOnlineStats.credits = value; }
        get { return playerBasicOnlineStats.credits; }
    }

    public string armorDataString
    {
        set { playerBasicOnlineStats.armor_data_string = value; }
        get { return playerBasicOnlineStats.armor_data_string; }
    }

    public string unlockedArmorDataString
    {
        set { playerBasicOnlineStats.unlocked_armor_data_string = value; }
        get { return playerBasicOnlineStats.unlocked_armor_data_string; }
    }

    public PlayerBasicOnlineStats playerBasicOnlineStats
    {
        get { return _playerBasicOnlineStats; }
        set
        {
            _playerBasicOnlineStats = value;

            //if(_playerBasicOnlineStats == null)
            //    _playerBasicOnlineStats = value;
            //else
            //    Debug.LogError("PlayerBasicOnlineStats already set");
        }
    }

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
    public class PlayerBasicOnlineStats
    {

        int _id;
        public int level, xp, credits;
        public string armor_data_string, unlocked_armor_data_string;

        public static PlayerBasicOnlineStats CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<PlayerBasicOnlineStats>(jsonString);
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
        public int kills, deaths, headshots, highest_points;

        public static PlayerBasicPvEStats CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<PlayerBasicPvEStats>(jsonString);
        }
    }
}
