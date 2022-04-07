using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerArmorManager : MonoBehaviour
{
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
        PlayerDatabaseAdaptor pda = WebManager.webManagerInstance.playerDatabaseAdaptor;

        foreach (PlayerArmorPiece piece in playerArmorPieces)
            piece.gameObject.SetActive(pda.playerBasicOnlineStats.armor_data_string.Contains(piece.entity));
        Debug.Log("here");
    }
}
