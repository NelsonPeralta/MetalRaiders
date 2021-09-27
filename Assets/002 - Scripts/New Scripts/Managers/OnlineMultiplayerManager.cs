using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class OnlineMultiplayerManager : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    [Header("Singletons")]
    public static OnlineMultiplayerManager multiplayerManagerInstance;
    public static PlayerManager playerManager;

    [Header("Gametype")]
    public string gametype;
    public int pointsToWin;

    [Header("Players")]
    public List<PlayerMultiplayerStats> playerMultiplayerStats = new List<PlayerMultiplayerStats>();

    ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();


    // private variables
    int listCreationRetries = 10;

    private void Awake()
    {
        if (multiplayerManagerInstance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        multiplayerManagerInstance = this;

        if (!PhotonNetwork.IsMasterClient)
            return;
        customProperties["score"] = 0;
        PhotonNetwork.SetPlayerCustomProperties(customProperties);
    }

    private void Start()
    {
        playerManager = PlayerManager.playerManagerInstance;
        //if (PhotonNetwork.IsMasterClient)
        //    StartCoroutine(CreateMultiplayerStatList());
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
        if (changedProps.ContainsKey("score"))
            Debug.Log($"On Properties Updtate: {changedProps}. Player: {targetPlayer}");
        //    playerMultiplayerStats = (List<PlayerMultiplayerStats>)changedProps["totaltime"];

        //UpdateTimerTexts();
    }

    IEnumerator CreateMultiplayerStatList()
    {
        yield return new WaitForSeconds(0.5f);
        if (playerManager.allPlayers.Count > playerMultiplayerStats.Count)
        {
            playerMultiplayerStats.Clear();
            for (int i = 0; i < playerManager.allPlayers.Count; i++)
            {
                PlayerMultiplayerStats pms = new PlayerMultiplayerStats(playerManager.allPlayers[i]);

                playerMultiplayerStats.Add(pms);
                Debug.Log(playerMultiplayerStats.Count);
            }
        }
    }


    public void AddToScore(int playerPhotonIdWhoGotTheKill, int playerWhoDiedPVID, bool wasHeadshot)
    {
            List<PlayerProperties> allPlayers = new List<PlayerProperties>();
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("player"))
                allPlayers.Add(go.GetComponent<PlayerProperties>());

            PlayerMultiplayerStats playerWhoGotKilledMS = PhotonView.Find(playerWhoDiedPVID).GetComponent<PlayerMultiplayerStats>();
        if (playerPhotonIdWhoGotTheKill != 99)
        {
            PlayerMultiplayerStats playerWhoGotTheKillMS = PhotonView.Find(playerPhotonIdWhoGotTheKill).GetComponent<PlayerMultiplayerStats>();



            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("Add to Score Method");
                if (playerPhotonIdWhoGotTheKill != playerWhoDiedPVID)
                {
                    Debug.Log($"Player who will get kill: {playerPhotonIdWhoGotTheKill}");

                    playerWhoGotTheKillMS.AddKill(pointsToWin);
                }

                Debug.Log($"Player who will get death: {playerWhoDiedPVID}");

                playerWhoGotKilledMS.AddDeath();
            }

            if (playerPhotonIdWhoGotTheKill != playerWhoDiedPVID)
            {
                foreach (PlayerProperties pp in allPlayers)
                    if (pp.PV.IsMine && pp)
                        pp.allPlayerScripts.killFeedManager.EnterNewFeed(playerWhoGotTheKillMS.playerName, playerWhoGotKilledMS.playerName, wasHeadshot);
            }
            else
            {
                foreach (PlayerProperties pp in allPlayers)
                    if (pp.PV.IsMine && pp)
                        pp.allPlayerScripts.killFeedManager.EnterNewFeed(playerWhoGotKilledMS.playerName);
            }

            CheckForEndGame(playerWhoGotTheKillMS.playerName);
        }
        else
        {
            playerWhoGotKilledMS.AddDeath();
            foreach (PlayerProperties pp in allPlayers)
                if (pp.PV.IsMine && pp)
                    pp.allPlayerScripts.killFeedManager.EnterNewFeed("Guardians", playerWhoGotKilledMS.playerName, false);
        }

        //Debug.Log($"Add to Score: {playerPhotonIdWhoGotTheKill} killed {playerWhoDiedPVID}");
        //if (gametype == "ffa")
        //{
        //    PlayerMultiplayerStats playerWhoGotKillMS = FindPlayerWithPhotonViewId(playerPhotonIdWhoGotTheKill);
        //    if (playerPhotonIdWhoGotTheKill != playerWhoDiedPVID)
        //        playerWhoGotKillMS.kills++;

        //    PlayerMultiplayerStats playerWhoWasKilledMS = FindPlayerWithPhotonViewId(playerWhoDiedPVID);
        //    playerWhoWasKilledMS.deaths++;

        //    PV.RPC("SpawnNewFeedForClients_RPC", RpcTarget.All, playerWhoGotKillMS.playerName, playerWhoWasKilledMS.playerName, wasHeadshot);

        //    //multiplayerManagerCustomProperties.Add("score", playerMultiplayerStats);
        //    //PhotonNetwork.SetPlayerCustomProperties(multiplayerManagerCustomProperties);

        //    UpdateClientScores();
        //    UpdateAllPlayerScores();
        //}
    }

    [PunRPC]
    void SpawnNewFeedForClients_RPC(string playerWhoGotKillName, string playerWhoWasKilledName, bool wasHeadshot)
    {
        List<PlayerProperties> allPlayers = new List<PlayerProperties>();
        foreach (GameObject pp in GameObject.FindGameObjectsWithTag("player"))
        {
            Debug.Log(pp);
            allPlayers.Add(pp.GetComponent<PlayerProperties>());
        }
        foreach (PlayerProperties pp in allPlayers)
            if (pp.PV.IsMine && pp)
                pp.allPlayerScripts.killFeedManager.EnterNewFeed(playerWhoGotKillName, playerWhoWasKilledName, wasHeadshot);
    }

    void UpdateClientScores()
    {
        PV.RPC("ClearClientScores", RpcTarget.All);

        for (int i = 0; i < playerMultiplayerStats.Count; i++)
        {
            int pvid = playerMultiplayerStats[i].PVID;
            string name = playerMultiplayerStats[i].playerName;
            int kills = playerMultiplayerStats[i].kills;
            int deaths = playerMultiplayerStats[i].deaths;

            PV.RPC("UpdateClientScores_RPC", RpcTarget.All, pvid, name, kills, deaths);
        }
    }

    [PunRPC]
    void ClearClientScores()
    {
        if (PhotonNetwork.IsMasterClient)
            return;

        playerMultiplayerStats.Clear();
    }

    [PunRPC]
    void UpdateClientScores_RPC(int pvid, string pName, int kills, int deaths)
    {
        if (PhotonNetwork.IsMasterClient)
            return;

        var newPMS = new PlayerMultiplayerStats(pvid, pName, kills, deaths);
        playerMultiplayerStats.Add(newPMS);
    }
    PlayerMultiplayerStats FindPlayerWithPhotonViewId(int pvid)
    {
        PlayerMultiplayerStats pms = new PlayerMultiplayerStats(0, "Empty", 0, 0);
        for (int i = 0; i < playerMultiplayerStats.Count; i++)
        {
            Debug.Log(playerMultiplayerStats[i].playerName + "; " + playerMultiplayerStats[i].PVID + "; " + pvid);
            if (playerMultiplayerStats[i].PVID == pvid)
            {
                Debug.Log("Found existing PMS");
                return playerMultiplayerStats[i];
            }
        }

        if (pvid == 99)
        {
            pms = new PlayerMultiplayerStats(99, "Guardians", 0, 0);
            return pms;
        }

        if (pms.PVID <= 0) // If theres is no such player and it wasnt the "Guardians", create player stats
        {
            AddPlayerToLists(pvid);
            pms = FindPlayerWithPhotonViewId(pvid); // Potential Loop
        }

        Debug.Log(pms);
        return pms;
    }
    void UpdateAllPlayerScores()
    {
        for (int i = 0; i < playerMultiplayerStats.Count; i++)
        {
            playerMultiplayerStats[i].player.GetComponent<AllPlayerScripts>().playerUIComponents.multiplayerPointsRed.text = playerMultiplayerStats[i].kills.ToString();
        }
    }

    public void CheckForEndGame(string _playerName)
    {
        Debug.Log("Checking for End Game");
        List<PlayerMultiplayerStats> allPlayersMS = new List<PlayerMultiplayerStats>();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("player"))
            allPlayersMS.Add(go.GetComponent<PlayerMultiplayerStats>());

        if (gametype == "ffa")
            for (int i = 0; i < allPlayersMS.Count; i++)
            {
                Debug.Log(allPlayersMS[i].kills);
                if (allPlayersMS[i].kills >= pointsToWin - 1 && allPlayersMS[i].playerName == _playerName)// Due to latency, the kill variable for the player is not updated before this method triggers
                {
                    Debug.Log("Hit game kill cap");
                    EndGame();
                }
            }
    }

    void EndGame()
    {
        Debug.Log("Ending Game");
        List<PlayerProperties> allPlayers = new List<PlayerProperties>();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("player"))
            allPlayers.Add(go.GetComponent<PlayerProperties>());

        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (!allPlayers[i].PV.IsMine)
                return;
                allPlayers[i].allPlayerScripts.announcer.PlayGameOverClip();
                allPlayers[i].LeaveRoomWithDelay();
        }
    }

    private void OnDestroy()
    {
        multiplayerManagerInstance = null;
    }

    public void GetScoresByHighest()
    {
        List<PlayerMultiplayerStats> scores = new List<PlayerMultiplayerStats>();
    }

    void AddPlayerToLists(int pvid)
    {
        Debug.Log("No such Multiplayer Stat Script. Creating one now.");
        var newPlayer = PhotonView.Find(pvid).GetComponent<PlayerProperties>();
        PlayerMultiplayerStats pms = new PlayerMultiplayerStats(newPlayer);
        playerMultiplayerStats.Add(pms);

        for (int i = 0; i < playerManager.allPlayers.Count; i++)
            if (playerManager.allPlayers[i].PV.ViewID == pvid)
                return;
        playerManager.allPlayers.Add(newPlayer);
    }
}
