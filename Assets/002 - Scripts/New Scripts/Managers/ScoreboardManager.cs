using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ScoreboardManager : MonoBehaviour
{
    [Header("Singletons")]
    public SpawnManager spawnManager;
    public AllPlayerScripts allPlayerScripts;
    public PlayerManager playerManager;
    public MultiplayerManager multiplayerManager;
    public OnlineSwarmManager onlineSwarmManager;

    [Header("Components")]
    public GameObject scoreboardUIGO;
    public GameObject multiplayerScoreboard;
    public GameObject swarmScoreboard;
    public List<ScoreboardRow> scoreboardRows = new List<ScoreboardRow>();

    // Private Variables
    bool scoreboardOpen;

    private void Awake()
    {
    }

    private void Start()
    {
        multiplayerManager = MultiplayerManager.multiplayerManagerInstance;
        onlineSwarmManager = OnlineSwarmManager.onlineSwarmManagerInstance;

        if (multiplayerManager)
        {
            multiplayerScoreboard.transform.parent = scoreboardUIGO.transform;
            swarmScoreboard.SetActive(false);
        }
        else if (swarmScoreboard)
        {
            swarmScoreboard.transform.parent = scoreboardUIGO.transform;
            multiplayerScoreboard.SetActive(false);
        }
        scoreboardUIGO.SetActive(false);
    }

    public void OpenScoreboard()
    {
        UpdateScoreboard();
        if (!scoreboardOpen)
        {
            scoreboardUIGO.SetActive(true);
            scoreboardOpen = true;
        }
    }

    public void CloseScoreboard()
    {
        scoreboardUIGO.SetActive(false);
        scoreboardOpen = false;
    }

    public void UpdateScoreboard()
    {
        DisableAllRows();
        multiplayerManager = MultiplayerManager.multiplayerManagerInstance;
        onlineSwarmManager = OnlineSwarmManager.onlineSwarmManagerInstance;
        if (multiplayerManager)
        {
            List<PlayerMultiplayerStats> allPlayersMS = new List<PlayerMultiplayerStats>();

            foreach (GameObject go in GameObject.FindGameObjectsWithTag("player"))
                allPlayersMS.Add(go.GetComponent<PlayerMultiplayerStats>());

            for (int i = 0; i < allPlayersMS.Count; i++)
            {
                scoreboardRows[i].playerNameText.text = allPlayersMS[i].playerName;

                scoreboardRows[i].playerKillsText.text = allPlayersMS[i].kills.ToString();

                scoreboardRows[i].playerDeathsText.text = allPlayersMS[i].deaths.ToString();

                scoreboardRows[i].gameObject.SetActive(true);
            }
        }
        else if (onlineSwarmManager)
        {
            Debug.Log("In Swarm Scoreboard");
            List<OnlinePlayerSwarmScript> allPlayersSS = new List<OnlinePlayerSwarmScript>();

            foreach (GameObject go in GameObject.FindGameObjectsWithTag("player"))
                allPlayersSS.Add(go.GetComponent<OnlinePlayerSwarmScript>());

            for (int i = 0; i < allPlayersSS.Count; i++)
            {
                scoreboardRows[i].playerNameText.text = allPlayersSS[i].GetComponent<PhotonView>().Owner.NickName;

                scoreboardRows[i].playerKillsText.text = allPlayersSS[i].GetPoints().ToString();

                scoreboardRows[i].playerDeathsText.text = allPlayersSS[i].GetTotalPoints().ToString();

                scoreboardRows[i].gameObject.SetActive(true);
            }
        }
    }

    void DisableAllRows()
    {
        foreach (ScoreboardRow sr in scoreboardRows)
            sr.gameObject.SetActive(false);
    }
}
