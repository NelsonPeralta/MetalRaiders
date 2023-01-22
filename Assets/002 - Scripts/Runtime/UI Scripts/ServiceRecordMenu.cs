using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ServiceRecordMenu : MonoBehaviour
{
    public TMP_Text levelText;
    public TMP_Text xpText;
    public TMP_Text creditsText;

    public TMP_Text multiplayerStatsText;
    public TMP_Text swarmStatsText;

    public GameObject playerModel;

    private void OnEnable()
    {
        playerModel.SetActive(true);
        PlayerDatabaseAdaptor pda = WebManager.webManagerInstance.pda;

        float kd = 0;

        if (pda.GetPvPDeaths() > 0)
        {
            kd = pda.GetPvPKills() / (float)pda.GetPvPDeaths();
        }
        Debug.Log($"Initializing Service Record Menu. PvE Kills {pda.GetPvEKills()}");
        multiplayerStatsText.text = $"MULTIPLAYER\n----------\n\nKills: {pda.GetPvPKills()}\nDeaths: {pda.GetPvPDeaths()}\nK/D: {kd}\nHeadshots: {pda.GetPvPHeadshots()}\nMelee Kills: {pda.PvPMeleeKills}\nGrenade Kills: {pda.PvPGrenadeKills}";
        swarmStatsText.text = $"SWARM\n-----\n\nKills: {pda.GetPvEKills()}\nDeaths: {pda.GetPvEDeaths()}\nHeadshots: {pda.GetPvEHeadshots()}\nHighest Score: {pda.GetPvEHighestPoints()}";

        levelText.text = $"Level: {pda.level.ToString()}";
        xpText.text = $"Xp: {pda.xp.ToString()}";
        creditsText.text = $"Credits: {pda.credits.ToString()}";
    }

    private void OnDisable()
    {
        playerModel.SetActive(false);
    }
}
