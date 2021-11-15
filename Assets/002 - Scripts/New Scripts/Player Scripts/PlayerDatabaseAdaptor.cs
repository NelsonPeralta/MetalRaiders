using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDatabaseAdaptor
{
    PlayerData playerData;
    public void SetPlayerData(PlayerData playerData)
    {
        if (this.playerData == null)
        {
            Debug.Log("Setting player data");
            this.playerData = playerData;
        }
        else
            Debug.LogError("PlayerData already set");
    }

    public string GetUsername() { return this.playerData.username; }
    public bool PlayerDataIsSet() { return playerData != null; }

    [System.Serializable]
    public class PlayerData
    {

        public int id;
        public string username;

        public static PlayerData CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<PlayerData>(jsonString);
        }

    }
}
