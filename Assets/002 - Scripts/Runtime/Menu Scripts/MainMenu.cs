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
            int unlockedCount = GameManager.instance.GetHowManyAchievementsPlayerHasUnlocked().Item1;
            int totalAchievements = GameManager.instance.GetHowManyAchievementsPlayerHasUnlocked().Item2;

            if (unlockedCount == totalAchievements)
            {
                if (!CurrentRoomManager.instance.playerDataCells[0].playerExtendedPublicData.unlocked_armor_data_string.Contains("flaming_helmet"))
                    WebManager.webManagerInstance.StartCoroutine(WebManager.UnlockArmorPiece_Coroutine("flaming_helmet"));
            }

            if (unlockedCount >= 12)
            {
                if (!CurrentRoomManager.instance.playerDataCells[0].playerExtendedPublicData.unlocked_armor_data_string.Contains("katana"))
                    WebManager.webManagerInstance.StartCoroutine(WebManager.UnlockArmorPiece_Coroutine("-katana-"));
            }
        }
        catch { }
    }
}
