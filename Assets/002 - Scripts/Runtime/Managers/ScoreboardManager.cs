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

        scoreboardUIGO.SetActive(false);
    }

    public void OpenScoreboard()
    {
        UpdateScoreboard();
        if (!scoreboardOpen)
        {
            scoreboardUIGO.SetActive(true);
            scoreboardOpen = true;

            if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
                swarmScoreboard.SetActive(false);
            if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
                multiplayerScoreboard.SetActive(false);
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
        MultiplayerManager multiplayerManager = MultiplayerManager.instance;
        //onlineSwarmManager = OnlineSwarmManager.onlineSwarmManagerInstance;

        if (!multiplayerManager)
            multiplayerManager = FindObjectOfType<MultiplayerManager>();

        //if (!onlineSwarmManager)
        //    onlineSwarmManager = FindObjectOfType<OnlineSwarmManager>();

        if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
        {
            List<PlayerMultiplayerMatchStats> allPlayersMS = new List<PlayerMultiplayerMatchStats>();

            foreach (GameObject go in GameObject.FindGameObjectsWithTag("player"))
                allPlayersMS.Add(go.GetComponent<PlayerMultiplayerMatchStats>());

            for (int i = 0; i < allPlayersMS.Count; i++)
            {
                scoreboardRows[i].playerNameText.text = allPlayersMS[i].GetComponent<Player>().nickName;
                scoreboardRows[i].playerKillsText.text = allPlayersMS[i].kills.ToString();
                scoreboardRows[i].playerDeathsText.text = allPlayersMS[i].deaths.ToString();
                scoreboardRows[i].playerHeadshotsText.text = allPlayersMS[i].headshots.ToString();
                if (allPlayersMS[i].deaths > 0)
                    scoreboardRows[i].playerCurrentPointsText.text = (allPlayersMS[i].kills / (float)allPlayersMS[i].deaths).ToString();
                else
                    scoreboardRows[i].playerCurrentPointsText.text = "0";

                scoreboardRows[i].gameObject.SetActive(true);
            }
        }
        else if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
        {
            List<PlayerSwarmMatchStats> allPlayersMS = new List<PlayerSwarmMatchStats>();

            foreach (GameObject go in GameObject.FindGameObjectsWithTag("player"))
                allPlayersMS.Add(go.GetComponent<PlayerSwarmMatchStats>());

            for (int i = 0; i < allPlayersMS.Count; i++)
            {
                scoreboardRows[i].playerNameText.text = allPlayersMS[i].GetComponent<Player>().nickName;
                scoreboardRows[i].playerKillsText.text = allPlayersMS[i].kills.ToString();
                scoreboardRows[i].playerDeathsText.text = allPlayersMS[i].deaths.ToString();
                scoreboardRows[i].playerHeadshotsText.text = allPlayersMS[i].headshots.ToString();
                scoreboardRows[i].playerCurrentPointsText.text = allPlayersMS[i].points.ToString();

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