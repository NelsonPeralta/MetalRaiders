using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMPProperties : MonoBehaviour
{
    public AllPlayerScripts allPlayerScripts;

    [Header("Team")]
    public string team;
    [Header("Kills")]
    public int kills;

    [Header("Game Types")]
    public SlayerManager slayerManager;

    private void Start()
    {
        StartCoroutine(GivePlayerTeam());
    }

    public void UpdatePoints(int playerWhoDied, int playerWhoKilled)
    {
        if (slayerManager)
            slayerManager.UpdatePoints(playerWhoDied, playerWhoKilled);
    }

    public void AddKill(bool Slayer)
    {
        kills++;
        if(Slayer)
            allPlayerScripts.playerUIComponents.multiplayerPointsRed.text = kills.ToString();
    }

    IEnumerator GivePlayerTeam()
    {
        yield return new WaitForEndOfFrame();

        if (slayerManager && slayerManager.gameSettings.loadSlayer)
            if (allPlayerScripts.playerProperties.playerRewiredID == 0)
            {
                team = "Red";
                allPlayerScripts.playerInventory.StartingWeapon = "M4";
            }
    }
}
