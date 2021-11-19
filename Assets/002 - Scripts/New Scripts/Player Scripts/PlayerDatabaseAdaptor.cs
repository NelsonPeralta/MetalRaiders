using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDatabaseAdaptor
{
    PlayerUserData playerData;
    public void SetPlayerData(PlayerUserData playerData)
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
    public class PlayerUserData
    {

        public int id;
        public string username;

        public static PlayerUserData CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<PlayerUserData>(jsonString);
        }

    }
}
