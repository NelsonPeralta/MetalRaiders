using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerArmorManager : MonoBehaviour
{
    public Player player;
    public List<PlayerArmorPiece> playerArmorPieces;
    string armorDataString;

    private void OnEnable()
    {
        if (player.isMine)
            armorDataString = WebManager.webManagerInstance.pda.playerBasicOnlineStats.armor_data_string;

        DisableAllArmor();
        EnableAllArmorsInDataString();
    }

    void DisableAllArmor()
    {
        foreach (PlayerArmorPiece piece in playerArmorPieces)
            piece.gameObject.SetActive(false);
    }

    void EnableAllArmorsInDataString()
    {
        foreach (PlayerArmorPiece piece in playerArmorPieces)
            piece.gameObject.SetActive(armorDataString.Contains(piece.entity));
    }
}
