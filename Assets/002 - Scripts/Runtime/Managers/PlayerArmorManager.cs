using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerArmorManager : MonoBehaviour
{
    public Player player;
    public List<PlayerArmorPiece> playerArmorPieces;

    private void OnEnable()
    {
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
        PlayerDatabaseAdaptor pda = WebManager.webManagerInstance.pda;

        foreach (PlayerArmorPiece piece in playerArmorPieces)
            piece.gameObject.SetActive(pda.playerBasicOnlineStats.armor_data_string.Contains(piece.entity));
    }
}
