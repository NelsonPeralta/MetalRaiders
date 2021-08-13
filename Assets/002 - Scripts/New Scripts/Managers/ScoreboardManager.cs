using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreboardManager : MonoBehaviour
{
    [Header("Singletons")]
    public SpawnManager spawnManager;
    public AllPlayerScripts allPlayerScripts;
    public PlayerManager playerManager;
    public MultiplayerManager multiplayerManager;

    [Header("Components")]
    public GameObject scoreboardUIGO;
    public List<ScoreboardRow> scoreboardRows = new List<ScoreboardRow>();

    // Private Variables
    bool scoreboardOpen;

    private void Awake()
    {
        scoreboardUIGO.SetActive(false);
    }

    private void Start()
    {
        multiplayerManager = MultiplayerManager.multiplayerManagerInstance;
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

    void DisableAllRows()
    {
        foreach (ScoreboardRow sr in scoreboardRows)
            sr.gameObject.SetActive(false);
    }
}
