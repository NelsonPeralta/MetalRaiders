using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ServiceRecordMenu : MonoBehaviour
{
    public TMP_Text multiplayerStatsText;
    public TMP_Text swarmStatsText;

    private void OnEnable()
    {
        PlayerDatabaseAdaptor pda = WebManager.webManagerInstance.playerDatabaseAdaptor;

        float kd = 0;

        if (pda.GetPvPDeaths() > 0)
        {
            kd = pda.GetPvPKills() / (float)pda.GetPvPDeaths();
        }
        Debug.Log($"Initializing Service Record Menu. PvE Kills {pda.GetPvEKills()}");
        multiplayerStatsText.text = $"MULTIPLAYER\n----------\n\nKills: {pda.GetPvPKills()}\nDeaths: {pda.GetPvPDeaths()}\nHeadshots: {pda.GetPvPHeadshots()}\nK/D: {kd}";
        swarmStatsText.text = $"SWARM\n-----\n\nKills: {pda.GetPvEKills()}\nDeaths: {pda.GetPvEDeaths()}\nHeadshots: {pda.GetPvEHeadshots()}\nTotal Points: {pda.GetPvETotalPoints()}";
    }
}
