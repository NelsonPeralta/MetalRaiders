using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Rewired;
using Steamworks;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject _quickMatchBtn;


    private void OnEnable()
    {
        try
        {
            if (GameManager.instance.connection == GameManager.Connection.Online)
            {
                int totalAchievements = (int)SteamUserStats.GetNumAchievements();
                int unlockedCount = 0;

                for (uint i = 0; i < totalAchievements; i++)
                {
                    string achievementName = SteamUserStats.GetAchievementName(i);
                    bool achieved;

                    if (SteamUserStats.GetAchievement(achievementName, out achieved) && achieved)
                    {
                        unlockedCount++;
                    }
                }

                Debug.Log($"Player has unlocked {unlockedCount} out of {totalAchievements} achievements.");

                if (unlockedCount == totalAchievements)
                {
                    if (!CurrentRoomManager.instance.playerDataCells[0].playerExtendedPublicData.unlocked_armor_data_string.Contains("katana_ca"))
                        WebManager.webManagerInstance.StartCoroutine(WebManager.UnlockArmorPiece_Coroutine("-katana_ca-"));
                }
            }
        }
        catch { }
    }
}
