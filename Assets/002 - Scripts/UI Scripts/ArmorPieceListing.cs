using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor.UI;

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
            if (_playerArmorPiece != null)
                return;
            PlayerDatabaseAdaptor pda = WebManager.webManagerInstance.playerDatabaseAdaptor;
            _playerArmorPiece = value;

            armorPieceNameText.text = playerArmorPiece.entity;

            if (pda.playerBasicOnlineStats.credits >= playerArmorPiece.cost)
                equipButton.gameObject.SetActive(true);
            else
            {
                notEnoughCreditsText.text = $"Not enough credits (${playerArmorPiece.cost})";
            }

        }
    }
}
