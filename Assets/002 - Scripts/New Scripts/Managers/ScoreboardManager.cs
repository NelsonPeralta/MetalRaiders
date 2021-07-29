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

    public void ToggleScoreboard()
    {
        UpdateScoreboard();
        if (!scoreboardOpen)
        {
            scoreboardUIGO.SetActive(true);
            scoreboardOpen = true;
        }
        else
        {
            scoreboardUIGO.SetActive(false);
            scoreboardOpen = false;
        }
    }

    public void UpdateScoreboard()
    {
        DisableAllRows();
        for (int i = 0; i < multiplayerManager.playerMultiplayerStats.Count; i++)
        {
            scoreboardRows[i].playerNameText.text = multiplayerManager.playerMultiplayerStats[i].playerName;

            scoreboardRows[i].playerKillsText.text = multiplayerManager.playerMultiplayerStats[i].kills.ToString();

            scoreboardRows[i].playerDeathsText.text = multiplayerManager.playerMultiplayerStats[i].deaths.ToString();

            scoreboardRows[i].gameObject.SetActive(true);
        }
    }

    void DisableAllRows()
    {
        foreach (ScoreboardRow sr in scoreboardRows)
            sr.gameObject.SetActive(false);
    }
}
