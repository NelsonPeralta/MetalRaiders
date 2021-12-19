using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using Photon.Pun;

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

                    StartCoroutine(Login_Coroutine_Set_PvP_Stats(username));
                    StartCoroutine(Login_Coroutine_Set_PvE_Stats(username));

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

    IEnumerator Login_Coroutine_Set_PvP_Stats(string username)
    {
        WWWForm form = new WWWForm();
        form.AddField("service", "getBasicPvPStats");
        form.AddField("username", username);

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
                    Launcher.launcherInstance.ShowPlayerMessage($"Fetched PvP Stats! Kills: {playerDatabaseAdaptor.GetKills()}");
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

    IEnumerator Login_Coroutine_Set_PvE_Stats(string username)
    {
        WWWForm form = new WWWForm();
        form.AddField("service", "getBasicPvEStats");
        form.AddField("username", username);

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