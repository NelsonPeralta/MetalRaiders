using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor.UI;
using UnityEngine.UI;

public class ArmorPieceListing : MonoBehaviour
{
    public TMP_Text armorPieceNameText;
    public TMP_Text notEnoughCreditsText;

    public UnityEngine.UI.Button buyButton;
    public UnityEngine.UI.Button equipButton;

    public ArmorPieceListing(PlayerArmorPiece p_playerArmorPiece)
    {
        playerArmorPiece = p_playerArmorPiece;
    }

    PlayerArmorPiece _playerArmorPiece;

    public PlayerArmorPiece playerArmorPiece
    {

        get { return _playerArmorPiece; }
        set
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(BuyArmorPiece);

            PlayerDatabaseAdaptor pda = WebManager.webManagerInstance.playerDatabaseAdaptor;
            _playerArmorPiece = value;

            armorPieceNameText.text = playerArmorPiece.entity;

            if (pda.playerBasicOnlineStats.unlocked_armor_data_string.Contains(playerArmorPiece.entity))
            {
                equipButton.gameObject.SetActive(true);
                return;
            }


            if (pda.playerBasicOnlineStats.credits >= playerArmorPiece.cost)
            {
                buyButton.gameObject.SetActive(true);
                buyButton.GetComponentInChildren<Text>().text = $"{playerArmorPiece.cost}cr";
            }

            else
            {
                buyButton.gameObject.SetActive(false);
                equipButton.gameObject.SetActive(false);


                notEnoughCreditsText.gameObject.SetActive(true);
                notEnoughCreditsText.text = $"{playerArmorPiece.cost}cr";
            }

        }
    }

    void BuyArmorPiece()
    {
        StartCoroutine(WebManager.webManagerInstance.SaveUnlockedArmorStringData_Coroutine(playerArmorPiece));

        ArmoryManager.instance.creditsText.text = $"{WebManager.webManagerInstance.playerDatabaseAdaptor.playerBasicOnlineStats.credits.ToString()}cr";
        ArmoryManager.instance.OnArmorBuy_Delegate();
        buyButton.gameObject.SetActive(false);
        equipButton.gameObject.SetActive(true);
    }
}
