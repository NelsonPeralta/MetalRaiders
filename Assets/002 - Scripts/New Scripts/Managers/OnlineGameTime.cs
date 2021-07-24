using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class OnlineGameTime : MonoBehaviourPunCallbacks
{
    [Header("Singleton")]
    public static OnlineGameTime onlineGameTimeInstance;
    public static WeaponPool weaponPoolInstance;

    [Header("Info")]
    public PhotonView PV;
    public int totalTime = 0;
    public List<Text> playerTimerTexts;
    Coroutine timerCoroutine;
    ExitGames.Client.Photon.Hashtable timerCustomProperties = new ExitGames.Client.Photon.Hashtable();

    [Header("Spawn Times")]
    public List<int> ammoPackSpawnTimes = new List<int>();


    private void Awake()
    {
        if (onlineGameTimeInstance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        onlineGameTimeInstance = this;
    }

    private void Start()
    {
        weaponPoolInstance = WeaponPool.weaponPoolInstance;
        if(PhotonNetwork.IsMasterClient)
            timerCoroutine = StartCoroutine(Timer());
    }

    IEnumerator Timer(float delay = 1)
    {
        float newDelay = 1;
        yield return new WaitForSeconds(delay);
        //if (PhotonNetwork.IsMasterClient)
        //{
            totalTime++;
            if (timerCustomProperties.ContainsKey("totaltime"))
                timerCustomProperties.Remove("totaltime");
            timerCustomProperties.Add("totaltime", totalTime);
            PhotonNetwork.SetPlayerCustomProperties(timerCustomProperties);
            //PhotonNetwork.CurrentRoom.SetCustomProperties(timerCustomProperties);
        //}
        //else
        //{
        //    //if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("totaltime"))
        //    //{
        //    //    totalTime = (int)PhotonNetwork.CurrentRoom.CustomProperties["totaltime"];
        //    //    newDelay = 0.1f;
        //    //}
        //    //else Debug.Log("No Custom Properties");
        //}

        
        //if (totalTime % 30 == 0)
        //    RespawnAmmoPacks();

        timerCoroutine = StartCoroutine(Timer(newDelay));
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
        //Debug.Log($"On Properties Updtate: {changedProps}. Player: {targetPlayer}");
        if (changedProps.ContainsKey("totaltime"))
            totalTime = (int)changedProps["totaltime"];

        UpdateTimerTexts();
    }

    void UpdateTimerTexts()
    {
        if (playerTimerTexts.Count > 0)
            foreach (Text text in playerTimerTexts)
                text.text = $"{(totalTime / 60).ToString("00")}:{(totalTime % 60).ToString("00")}";
    }

    void RespawnWeapons()
    {

    }

    void RespawnAmmoPacks()
    {

    }
}
