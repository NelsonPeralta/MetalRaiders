using UnityEngine;
using UnityEngine.Networking;
using System;
using Photon.Pun;
using System.Collections.Generic;
using System.Collections;
using Random = UnityEngine.Random;

public partial class WebManager : MonoBehaviour
{
    public delegate void WebManagerEvent();
    public event WebManagerEvent OnSuccessfulLogin;

    public static WebManager webManagerInstance;
    public PlayerDatabaseAdaptor pda = new PlayerDatabaseAdaptor();

    private void OnEnable() { }
    private void OnDisable() { }
    private void Awake()
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

    public void UpdatePlayerCommonData()
    {
        StartCoroutine(Login_Coroutine_Set_Online_Stats(pda.id));
    }

    public void UpdatePlayerCommonPVPData()
    {
        StartCoroutine(Login_Coroutine_Set_PvP_Stats(pda.id));
    }

    public void UpdatePlayerCommonPVEData()
    {
        StartCoroutine(Login_Coroutine_Set_PvE_Stats(pda.id));
    }






    public void SaveSwarmStats(PlayerSwarmMatchStats onlinePlayerSwarmScript)
    {
        StartCoroutine(SaveSwarmStats_Coroutine(onlinePlayerSwarmScript));
        StartCoroutine(SaveXp_Coroutine(onlinePlayerSwarmScript));

        UpdatePlayerCommonData();
        UpdatePlayerCommonPVEData();
    }

    public void SaveMultiplayerStats(PlayerMultiplayerMatchStats playerMultiplayerStats)
    {
        StartCoroutine(SaveMultiplayerStats_Coroutine(playerMultiplayerStats));
        StartCoroutine(SaveXp_Coroutine(playerMultiplayerStats: playerMultiplayerStats));

        UpdatePlayerCommonData();
        UpdatePlayerCommonPVPData();
    }

    public void SaveArmorData(string newDataString)
    {

    }
}