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
        ReloadArmor();
    }

    private void Awake()
    {
        //ReloadArmor();
    }

    private void Start()
    {
        //ReloadArmor();
    }

    void ReloadArmor()
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
            {
                armorDataString = WebManager.webManagerInstance.pda.playerBasicOnlineStats.armor_data_string;
                Debug.Log(armorDataString);
            }
        }
        catch { }

        DisableAllArmor();
        EnableAllArmorsInDataString();
    }

    void DisableAllArmor()
    {
        try
        {
            foreach (PlayerArmorPiece piece in playerArmorPieces)
                piece.gameObject.SetActive(false);
        }
        catch { }
    }

    void EnableAllArmorsInDataString()
    {
        foreach (PlayerArmorPiece piece in playerArmorPieces)
            try
            {
                piece.gameObject.SetActive(armorDataString.Contains(piece.entity));
            }
            catch { piece.gameObject.SetActive(false); }
    }
}
