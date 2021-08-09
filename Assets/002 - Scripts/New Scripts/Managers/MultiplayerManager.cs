using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class MultiplayerManager : MonoBehaviour
{
    public PhotonView PV;
    [Header("Singletons")]
    public static MultiplayerManager multiplayerManagerInstance;
    public static PlayerManager playerManager;

    [Header("Gametype")]
    public string gametype;
    public int pointsToWin;

    [Header("Players")]
    public List<PlayerMultiplayerStats> playerMultiplayerStats = new List<PlayerMultiplayerStats>();

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
    }

    private void Start()
    {
        Application.targetFrameRate = 100;
        playerManager = PlayerManager.playerManagerInstance;
        StartCoroutine(CreateMultiplayerStatList());
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

        listCreationRetries--;

        if (listCreationRetries > 0)
            StartCoroutine(CreateMultiplayerStatList());
    }

    public void AddToScore(int playerPhotonIdWhoGotTheKill, int playerWhoDiedPVID, bool wasHeadshot)
    {
        Debug.Log($"Add to Score: {playerPhotonIdWhoGotTheKill} killed {playerWhoDiedPVID}");
        if (gametype == "ffa")
        {
            PlayerMultiplayerStats playerWhoGotKillMS = FindPlayerWithPhotonViewId(playerPhotonIdWhoGotTheKill);
            if (playerPhotonIdWhoGotTheKill != playerWhoDiedPVID)
                playerWhoGotKillMS.kills++;

            PlayerMultiplayerStats playerWhoWasKilledMS = FindPlayerWithPhotonViewId(playerWhoDiedPVID);
            playerWhoWasKilledMS.deaths++;

            foreach (PlayerProperties pp in playerManager.allPlayers)
                if (pp.PV.IsMine && pp)
                    pp.allPlayerScripts.killFeedManager.EnterNewFeed(playerWhoGotKillMS.playerName, playerWhoWasKilledMS.playerName, wasHeadshot);

            UpdateAllPlayerScores();
            CheckForEndGame();
        }
    }


    PlayerMultiplayerStats FindPlayerWithPhotonViewId(int pvid)
    {

        for (int i = 0; i < playerMultiplayerStats.Count; i++)
        {
            Debug.Log(playerMultiplayerStats[i].playerName + "; " + playerMultiplayerStats[i].PVID + "; " + pvid);
            if (playerMultiplayerStats[i].PVID == pvid)
                return playerMultiplayerStats[i];
        }

        if (pvid == 99)
        {
            Debug.Log("No PMS");
            return new PlayerMultiplayerStats(99, "Guardians", 0, 0);
        }

        return null;
    }
    void UpdateAllPlayerScores()
    {
        for (int i = 0; i < playerMultiplayerStats.Count; i++)
        {
            playerMultiplayerStats[i].player.GetComponent<AllPlayerScripts>().playerUIComponents.multiplayerPointsRed.text = playerMultiplayerStats[i].kills.ToString();
        }
    }

    void CheckForEndGame()
    {
        if (gametype == "ffa")
            for (int i = 0; i < playerMultiplayerStats.Count; i++)
            {
                if (playerMultiplayerStats[i].kills >= pointsToWin)
                    EndGame();
            }
    }

    void EndGame()
    {
        playerMultiplayerStats[0].player.GetComponent<AllPlayerScripts>().announcer.PlayGameOverClip();

        playerMultiplayerStats[0].player.LeaveRoomWithDelay();
    }

    private void OnDestroy()
    {
        multiplayerManagerInstance = null;
    }

    public void GetScoresByHighest()
    {
        List<PlayerMultiplayerStats> scores = new List<PlayerMultiplayerStats>();
    }
}
