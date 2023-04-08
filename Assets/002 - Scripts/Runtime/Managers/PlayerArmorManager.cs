using Photon.Pun;
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
        try
        {
            Debug.Log("PlayerArmorManager");
            Debug.Log(WebManager.webManagerInstance.pda.playerBasicOnlineStats.armor_data_string);
            Debug.Log(GameManager.instance.roomPlayerData[PhotonNetwork.NickName].armorDataString);
        }
        catch { }

        try
        {
            if (player)
            {
                if (player.isMine)
                {
                    if (player.rid == 0)
                        armorDataString = WebManager.webManagerInstance.pda.playerBasicOnlineStats.armor_data_string;
                    else
                        armorDataString = "helmet1";
                }
                else
                    armorDataString = GameManager.instance.roomPlayerData[player.nickName].armorDataString;
            }
            else
                armorDataString = WebManager.webManagerInstance.pda.playerBasicOnlineStats.armor_data_string;
        }
        catch { }

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
