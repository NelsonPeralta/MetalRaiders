using UnityEngine;
using UnityEngine.Networking;
using System;
using Photon.Pun;
using System.Collections.Generic;
using System.Collections;

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

    public void SaveSwarmStats(OnlinePlayerSwarmScript onlinePlayerSwarmScript)
    {
        StartCoroutine(SaveSwarmStats_Coroutine(onlinePlayerSwarmScript));

    }

    public void SaveMultiplayerStats(PlayerMultiplayerStats playerMultiplayerStats)
    {
        StartCoroutine(SaveMultiplayerStats_Coroutine(playerMultiplayerStats));

    }

    IEnumerator SaveSwarmStats_Coroutine(OnlinePlayerSwarmScript onlinePlayerSwarmScript)
    {
        int playerId = playerDatabaseAdaptor.GetId();
        int newKills = playerDatabaseAdaptor.GetPvEKills() + onlinePlayerSwarmScript.kills;
        int newDeaths = playerDatabaseAdaptor.GetPvEDeaths() + onlinePlayerSwarmScript.deaths;
        int newHeadshots = playerDatabaseAdaptor.GetPvEHeadshots() + onlinePlayerSwarmScript.headshots;
        int newTotalPoints = playerDatabaseAdaptor.GetPvETotalPoints() + onlinePlayerSwarmScript.GetTotalPoints();
        Debug.Log(playerDatabaseAdaptor.GetPvETotalPoints() + " " + onlinePlayerSwarmScript.GetTotalPoints() + " " + newTotalPoints);

        WWWForm form = new WWWForm();
        form.AddField("service", "SaveSwarmStats");
        form.AddField("playerId", playerId);
        form.AddField("newKills", newKills);
        form.AddField("newDeaths", newDeaths);
        form.AddField("newHeadshots", newHeadshots);
        form.AddField("newTotalPoints", newTotalPoints);

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
        StartCoroutine(Login_Coroutine_Set_PvE_Stats(playerId));
    }

    IEnumerator SaveMultiplayerStats_Coroutine(PlayerMultiplayerStats playerMultiplayerStats)
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
                    Launcher.launcherInstance.OnCreateRoomFailed(0, "That username already exists");
                }
                else
                {
                    Launcher.launcherInstance.ShowPlayerMessage("Registered successfully!");
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

                    StartCoroutine(Login_Coroutine_Set_PvP_Stats(playerDatabaseAdaptor.GetId()));
                    StartCoroutine(Login_Coroutine_Set_PvE_Stats(playerDatabaseAdaptor.GetId()));

                    Launcher.launcherInstance.ShowPlayerMessage("Logged in successfully!");
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    if (www.downloadHandler.text.Contains("wrong credentials"))
                    {
                        Launcher.launcherInstance.OnCreateRoomFailed(0, "Wrong credentials");
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
                        Launcher.launcherInstance.OnCreateRoomFailed(0, "Could not fetch pvp stats");
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
                        Launcher.launcherInstance.OnCreateRoomFailed(0, "Could not fetch pve stats");
                    }
                }
            }
        }
    }
}