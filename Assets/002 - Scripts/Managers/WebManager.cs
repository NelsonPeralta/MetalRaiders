using UnityEngine;
using UnityEngine.Networking;
using System;
using Photon.Pun;
using System.Collections.Generic;
using System.Collections;
using Random = UnityEngine.Random;

public class WebManager : MonoBehaviour
{
    public static WebManager webManagerInstance;
    public PlayerDatabaseAdaptor playerDatabaseAdaptor = new PlayerDatabaseAdaptor();
    private void Start()
    {
        if (webManagerInstance)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            webManagerInstance = this;
        }
    }
    public void Register(string username, string password)
    {
        StartCoroutine(Register_Coroutine(username, password));
    }

    public void Login(string username, string password)
    {
        StartCoroutine(Login_Coroutine(username, password));
    }

    public void SaveSwarmStats(PlayerSwarmMatchStats onlinePlayerSwarmScript)
    {
        StartCoroutine(SaveSwarmStats_Coroutine(onlinePlayerSwarmScript));
    }

    public void SaveMultiplayerStats(PlayerMultiplayerMatchStats playerMultiplayerStats)
    {
        StartCoroutine(SaveMultiplayerStats_Coroutine(playerMultiplayerStats));
    }

    public void SaveArmorData(string newDataString)
    {

    }

    IEnumerator SaveArmorData_Coroutine(string newDataString)
    {
        int xpAndCreditGain = Random.Range(160, 240);

        int playerId = playerDatabaseAdaptor.GetId();
        int newLevel = playerDatabaseAdaptor.playerBasicOnlineStats.level;
        int newXp = playerDatabaseAdaptor.playerBasicOnlineStats.xp + xpAndCreditGain;
        int newCredits = playerDatabaseAdaptor.playerBasicOnlineStats.credits + xpAndCreditGain;

        int dbXpToLevel = PlayerProgressionManager.instance.playerLevelToXpDic[playerDatabaseAdaptor.playerBasicOnlineStats.level];

        if (dbXpToLevel > playerDatabaseAdaptor.playerBasicOnlineStats.level)
            newLevel = playerDatabaseAdaptor.playerBasicOnlineStats.level + 1;


        WWWForm form = new WWWForm();
        form.AddField("service", "SaveBasicOnlineStats");

        form.AddField("playerId", playerId);
        form.AddField("newLevel", newLevel);
        form.AddField("newXp", newXp);
        form.AddField("newCredits", newCredits);

        using (UnityWebRequest www = UnityWebRequest.Post("https://metalraiders.com/database.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.result);
                Debug.Log(www.downloadHandler.text);

                if (www.downloadHandler.text.Contains("Could not save swarm stats"))
                {
                    Debug.LogError("Could not save swarm stats");

                }
                else if (www.downloadHandler.text.Contains("Swarm stats saved"))
                {
                    Debug.Log("Swarm stats saved successfully");
                }
            }
        }
        StartCoroutine(Login_Coroutine_Set_PvE_Stats(playerId));
    }

    IEnumerator SaveBasicOnlineStats_Coroutine(PlayerSwarmMatchStats onlinePlayerSwarmScript)
    {
        int xpAndCreditGain = Random.Range(160, 240) + (SwarmManager.instance.currentWave * Random.Range(8, 12));

        int playerId = playerDatabaseAdaptor.GetId();
        int newLevel = playerDatabaseAdaptor.playerBasicOnlineStats.level;
        int newXp = playerDatabaseAdaptor.playerBasicOnlineStats.xp + xpAndCreditGain;
        int newCredits = playerDatabaseAdaptor.playerBasicOnlineStats.credits + xpAndCreditGain;

        int dbXpToLevel = PlayerProgressionManager.instance.playerLevelToXpDic[playerDatabaseAdaptor.playerBasicOnlineStats.level];

        if(dbXpToLevel > playerDatabaseAdaptor.playerBasicOnlineStats.level)
            newLevel = playerDatabaseAdaptor.playerBasicOnlineStats.level + 1;

        //GameManager.instance.GetMyPlayer().GetComponent<PlayerUI>().killFeedManager.EnterNewFeed($"Player Id: {playerId}. New Xp: {newXp}. New Credits: {newCredits}");
        GameManager.instance.GetMyPlayer().GetComponent<KillFeedManager>().EnterNewFeed($"Gained {xpAndCreditGain} Xp and Credits");

        WWWForm form = new WWWForm();
        form.AddField("service", "SaveBasicOnlineStats");

        form.AddField("playerId", playerId);
        form.AddField("newLevel", newLevel);
        form.AddField("newXp", newXp);
        form.AddField("newCredits", newCredits);

        using (UnityWebRequest www = UnityWebRequest.Post("https://metalraiders.com/database.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.result);
                Debug.Log(www.downloadHandler.text);
                //GameManager.instance.GetMyPlayer().GetComponent<PlayerUI>().killFeedManager.EnterNewFeed($"{www.downloadHandler.text}");


                if (www.downloadHandler.text.Contains("Could not save swarm stats"))
                {
                    Debug.LogError("Could not save swarm stats");

                }
                else if (www.downloadHandler.text.Contains("Swarm stats saved"))
                {
                    Debug.Log("Swarm stats saved successfully");
                }
            }
        }
        StartCoroutine(Login_Coroutine_Set_PvE_Stats(playerId));
    }

    IEnumerator SaveSwarmStats_Coroutine(PlayerSwarmMatchStats onlinePlayerSwarmScript)
    {
        int playerId = playerDatabaseAdaptor.GetId();
        int newKills = playerDatabaseAdaptor.GetPvEKills() + onlinePlayerSwarmScript.kills;
        int newDeaths = playerDatabaseAdaptor.GetPvEDeaths() + onlinePlayerSwarmScript.deaths;
        int newHeadshots = playerDatabaseAdaptor.GetPvEHeadshots() + onlinePlayerSwarmScript.headshots;
        int newHighestScore = playerDatabaseAdaptor.GetPvEHighestPoints();
        if(onlinePlayerSwarmScript.GetTotalPoints() > newHighestScore)
            newHighestScore = onlinePlayerSwarmScript.GetTotalPoints();

        Debug.Log("bababooey " + playerDatabaseAdaptor.GetPvEHighestPoints() + " " + onlinePlayerSwarmScript.GetTotalPoints() + " " + newHighestScore +"\nXp to Level: ");

        //GameManager.instance.GetMyPlayer().GetComponent<PlayerUI>().killFeedManager.EnterNewFeed("Saving");


        WWWForm form = new WWWForm();
        form.AddField("service", "SaveSwarmStats");
        form.AddField("playerId", playerId);
        form.AddField("newKills", newKills);
        form.AddField("newDeaths", newDeaths);
        form.AddField("newHeadshots", newHeadshots);
        form.AddField("newHighestPoints", newHighestScore);

        using (UnityWebRequest www = UnityWebRequest.Post("https://metalraiders.com/database.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.result);
                Debug.Log(www.downloadHandler.text);

                if (www.downloadHandler.text.Contains("Could not save swarm stats"))
                {
                    Debug.LogError("Could not save swarm stats");

                }
                else if(www.downloadHandler.text.Contains("Swarm stats saved"))
                {
                    Debug.Log("Swarm stats saved successfully");
                }
            }
        }

        try
        {
            StartCoroutine(SaveBasicOnlineStats_Coroutine(onlinePlayerSwarmScript));
        }catch (Exception ex)
        {

        }
        StartCoroutine(Login_Coroutine_Set_PvE_Stats(playerId));
    }

    IEnumerator SaveMultiplayerStats_Coroutine(PlayerMultiplayerMatchStats playerMultiplayerStats)
    {
        int playerId = playerDatabaseAdaptor.GetId();
        int newKills = playerDatabaseAdaptor.GetPvPKills() + playerMultiplayerStats.kills;
        int newDeaths = playerDatabaseAdaptor.GetPvPDeaths() + playerMultiplayerStats.deaths;
        int newHeadshots = playerDatabaseAdaptor.GetPvPHeadshots() + playerMultiplayerStats.headshots;

        WWWForm form = new WWWForm();
        form.AddField("service", "SaveMultiplayerStats");
        form.AddField("playerId", playerId);
        form.AddField("newKills", newKills);
        form.AddField("newDeaths", newDeaths);
        form.AddField("newHeadshots", newHeadshots);

        using (UnityWebRequest www = UnityWebRequest.Post("https://metalraiders.com/database.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.result);
                Debug.Log(www.downloadHandler.text);

                if (www.downloadHandler.text.Contains("Could not save swarm stats"))
                {
                    Debug.LogError("Could not save swarm stats");

                }
                else if (www.downloadHandler.text.Contains("Swarm stats saved"))
                {
                    Debug.Log("Swarm stats saved successfully");
                }
            }
        }
        StartCoroutine(Login_Coroutine_Set_PvE_Stats(playerId));
    }


    IEnumerator Register_Coroutine(string username, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("service", "register");
        form.AddField("username", username);
        form.AddField("password", password);

        //using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/unity/database.php", form))
        using (UnityWebRequest www = UnityWebRequest.Post("https://metalraiders.com/database.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                //Debug.Log(www.result);
                //Debug.Log(www.downloadHandler.text);

                if (www.downloadHandler.text.Contains("Duplicate"))
                {
                    Launcher.instance.OnCreateRoomFailed(0, "That username already exists");
                }
                else
                {
                    Launcher.instance.ShowPlayerMessage("Registered successfully!");
                }
            }
        }
    }

    IEnumerator Login_Coroutine(string username, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("service", "login");
        form.AddField("username", username);
        form.AddField("password", password);
        Debug.Log($"Password: {password}");

        //using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/unity/database.php", form))
        using (UnityWebRequest www = UnityWebRequest.Post("https://metalraiders.com/database.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                //Debug.Log(www.result);
                //Debug.Log(www.downloadHandler.text);

                string jsonarray = www.downloadHandler.text;

                try
                {
                    PlayerDatabaseAdaptor.PlayerUserData pd = PlayerDatabaseAdaptor.PlayerUserData.CreateFromJSON(jsonarray);
                    playerDatabaseAdaptor.SetPlayerData(pd);
                    PhotonNetwork.NickName = playerDatabaseAdaptor.GetUsername();

                    StartCoroutine(Login_Coroutine_Set_Online_Stats(playerDatabaseAdaptor.GetId()));
                    StartCoroutine(Login_Coroutine_Set_PvP_Stats(playerDatabaseAdaptor.GetId()));
                    StartCoroutine(Login_Coroutine_Set_PvE_Stats(playerDatabaseAdaptor.GetId()));

                    Launcher.instance.ShowPlayerMessage("Logged in successfully!");
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    if (www.downloadHandler.text.Contains("wrong credentials"))
                    {
                        Launcher.instance.OnCreateRoomFailed(0, "Wrong credentials");
                    }
                }
            }
        }
    }

    IEnumerator Login_Coroutine_Set_Online_Stats(int playerId)
    {
        WWWForm form = new WWWForm();
        form.AddField("service", "getBasicOnlineData");
        form.AddField("playerId", playerId);

        using (UnityWebRequest www = UnityWebRequest.Post("https://metalraiders.com/database.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                //Debug.Log(www.result);
                //Debug.Log(www.downloadHandler.text);

                string jsonarray = www.downloadHandler.text;

                try
                {
                    PlayerDatabaseAdaptor.PlayerBasicOnlineStats pd = PlayerDatabaseAdaptor.PlayerBasicOnlineStats.CreateFromJSON(jsonarray);
                    playerDatabaseAdaptor.playerBasicOnlineStats = pd;
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    if (www.downloadHandler.text.Contains("Could not fetch pvp stats"))
                    {
                        Launcher.instance.OnCreateRoomFailed(0, "Could not fetch pvp stats");
                    }
                }
            }
        }
    }

    IEnumerator Login_Coroutine_Set_PvP_Stats(int playerId)
    {
        WWWForm form = new WWWForm();
        form.AddField("service", "getBasicPvPStats");
        form.AddField("playerId", playerId);

        using (UnityWebRequest www = UnityWebRequest.Post("https://metalraiders.com/database.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                //Debug.Log(www.result);
                //Debug.Log(www.downloadHandler.text);

                string jsonarray = www.downloadHandler.text;

                try
                {
                    PlayerDatabaseAdaptor.PlayerBasicPvPStats pd = PlayerDatabaseAdaptor.PlayerBasicPvPStats.CreateFromJSON(jsonarray);
                    playerDatabaseAdaptor.SetPlayerBasicPvPStats(pd);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    if (www.downloadHandler.text.Contains("Could not fetch pvp stats"))
                    {
                        Launcher.instance.OnCreateRoomFailed(0, "Could not fetch pvp stats");
                    }
                }
            }
        }
    }

    IEnumerator Login_Coroutine_Set_PvE_Stats(int playerId)
    {
        WWWForm form = new WWWForm();
        form.AddField("service", "getBasicPvEStats");
        form.AddField("playerId", playerId);

        using (UnityWebRequest www = UnityWebRequest.Post("https://metalraiders.com/database.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.result);
                Debug.Log(www.downloadHandler.text);

                string jsonarray = www.downloadHandler.text;

                try
                {
                    PlayerDatabaseAdaptor.PlayerBasicPvEStats pd = PlayerDatabaseAdaptor.PlayerBasicPvEStats.CreateFromJSON(jsonarray);
                    playerDatabaseAdaptor.SetPlayerBasicPvEStats(pd);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    if (www.downloadHandler.text.Contains("Could not fetch pve stats"))
                    {
                        Launcher.instance.OnCreateRoomFailed(0, "Could not fetch pve stats");
                    }
                }
            }
        }
    }











    // Armory
    public IEnumerator SaveUnlockedArmorStringData_Coroutine(PlayerArmorPiece playerArmorPiece)
    {
        playerDatabaseAdaptor.unlockedArmorDataString += $"{playerArmorPiece.entity}\n";
        playerDatabaseAdaptor.unlockedArmorDataString.Replace("\n\n", "\n");
        playerDatabaseAdaptor.credits -= playerArmorPiece.cost;

        WWWForm form = new WWWForm();
        form.AddField("service", "SaveUnlockedArmorStringData");
        form.AddField("playerId", playerDatabaseAdaptor.GetId());

        form.AddField("newUnlockedArmorStringData", playerDatabaseAdaptor.unlockedArmorDataString);
        form.AddField("newPlayerCredits", playerDatabaseAdaptor.credits);

        using (UnityWebRequest www = UnityWebRequest.Post("https://metalraiders.com/database.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.result);
                Debug.Log(www.downloadHandler.text);

                if (www.downloadHandler.text.Contains("Could not save UnlockedArmorStringData"))
                {
                    Debug.LogError("Could not save UnlockedArmorStringData");

                }
                else if (www.downloadHandler.text.Contains("UnlockedArmorStringData saved successfully"))
                {
                    Debug.Log("UnlockedArmorStringData saved successfully");
                }
            }

            ArmoryManager.instance.OnArmorBuy_Delegate();
        }
    }

    public IEnumerator SaveEquippedArmorStringData_Coroutine(string data)
    {
        WWWForm form = new WWWForm();
        form.AddField("service", "SaveEquippedArmorDataString");
        form.AddField("playerId", playerDatabaseAdaptor.GetId());

        form.AddField("newEquippedArmorStringData", data);

        using (UnityWebRequest www = UnityWebRequest.Post("https://metalraiders.com/database.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.result);
                Debug.Log(www.downloadHandler.text);

                if (www.downloadHandler.text.Contains("Could not save SaveEquippedArmorDataString"))
                {
                    Debug.LogError("Could not save SaveEquippedArmorDataString");

                }
                else if (www.downloadHandler.text.Contains("SaveEquippedArmorDataString saved successfully"))
                {
                    Debug.Log("SaveEquippedArmorDataString saved successfully");
                }
            }
        }
    }
}