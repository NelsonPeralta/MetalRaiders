using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor.UI;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ArmorPieceListing : MonoBehaviour
{
    public GameObject model;

    public TMP_Text armorPieceNameText;

    public UnityEngine.UI.Button buyButton;
    public UnityEngine.UI.Button equipButton;
    public UnityEngine.UI.Button unequipButton;
    public UnityEngine.UI.Button notEnoughCreditsButton;

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
            equipButton.onClick.RemoveAllListeners();
            unequipButton.onClick.RemoveAllListeners();
            buyButton.onClick.RemoveAllListeners();

            equipButton.onClick.AddListener(EquipArmorPiece);
            unequipButton.onClick.AddListener(UnequipArmorPiece);
            buyButton.onClick.AddListener(BuyArmorPiece);


            PlayerDatabaseAdaptor pda = WebManager.webManagerInstance.playerDatabaseAdaptor;
            _playerArmorPiece = value;
            model = playerArmorPiece.gameObject;

            armorPieceNameText.text = playerArmorPiece.entity;

            if (pda.playerBasicOnlineStats.unlocked_armor_data_string.Contains(playerArmorPiece.entity))
            {
                if (!pda.armorDataString.Contains(playerArmorPiece.entity))
                {

                    equipButton.gameObject.SetActive(true);
                }
                else
                {
                    if (playerArmorPiece.pieceType != PlayerArmorPiece.PieceType.Core)
                        unequipButton.gameObject.SetActive(true);
                }
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


                notEnoughCreditsButton.gameObject.SetActive(true);
                notEnoughCreditsButton.GetComponentInChildren<Text>().text = $"{playerArmorPiece.cost}cr";
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

    void EquipArmorPiece()
    {
        foreach (ArmorPieceListing armorPieceListing in ArmoryManager.instance.armorPieceListingList)
            if (armorPieceListing != this)
                if (this.playerArmorPiece.pieceType == armorPieceListing.playerArmorPiece.pieceType && this.playerArmorPiece.bodyPart == armorPieceListing.playerArmorPiece.bodyPart)
                {
                    if (WebManager.webManagerInstance.playerDatabaseAdaptor.unlockedArmorDataString.Contains(armorPieceListing.playerArmorPiece.entity))
                        armorPieceListing.UnequipArmorPiece();
                }

        WebManager.webManagerInstance.playerDatabaseAdaptor.armorDataString += $"{playerArmorPiece.entity}\n";
        WebManager.webManagerInstance.playerDatabaseAdaptor.armorDataString.Replace("\n\n", "\n");
        StartCoroutine(WebManager.webManagerInstance.SaveEquippedArmorStringData_Coroutine(WebManager.webManagerInstance.playerDatabaseAdaptor.armorDataString));

        //if (this.playerArmorPiece.pieceType == PlayerArmorPiece.PieceType.Core)
        //    foreach (ArmorPieceListing armorPieceListing in ArmoryManager.instance.armorPieceListingList)
        //        if (armorPieceListing != this)
        //            if (armorPieceListing.playerArmorPiece.pieceType == PlayerArmorPiece.PieceType.Core)
        //                if (this.playerArmorPiece.pieceType == armorPieceListing.playerArmorPiece.pieceType && armorPieceListing.model.gameObject.activeSelf)
        //                {
        //                    armorPieceListing.equipButton.gameObject.SetActive(true);
        //                }


        model.gameObject.SetActive(true);
        equipButton.gameObject.SetActive(false);
        if (playerArmorPiece.pieceType == PlayerArmorPiece.PieceType.Attachment)
            unequipButton.gameObject.SetActive(true);
    }

    public void UnequipArmorPiece()
    {
        string newData = WebManager.webManagerInstance.playerDatabaseAdaptor.armorDataString.Replace(playerArmorPiece.entity, "");
        newData.Replace("\n\n", "\n");
        WebManager.webManagerInstance.playerDatabaseAdaptor.armorDataString = newData;
        StartCoroutine(WebManager.webManagerInstance.SaveEquippedArmorStringData_Coroutine(WebManager.webManagerInstance.playerDatabaseAdaptor.armorDataString));

        model.gameObject.SetActive(false);
        equipButton.gameObject.SetActive(true);
        unequipButton.gameObject.SetActive(false);
    }

    public void OnButtonMouseEnter()
    {
        Debug.Log("OnButtonMouseEnter");
        Debug.Log(model.name);

        foreach (ArmorPieceListing armorPieceListing in ArmoryManager.instance.armorPieceListingList)
            if (armorPieceListing != this)
                if (this.playerArmorPiece.pieceType == armorPieceListing.playerArmorPiece.pieceType && this.playerArmorPiece.bodyPart == armorPieceListing.playerArmorPiece.bodyPart && armorPieceListing.model.gameObject.activeSelf)
                {
                    armorPieceListing.model.SetActive(false);
                }

        model.SetActive(true);
    }

    public void OnButtonMouseExit()
    {
        Debug.Log("OnButtonMouseExit");
        Debug.Log(model.name);

        foreach (ArmorPieceListing armorPieceListing in ArmoryManager.instance.armorPieceListingList)
            if (armorPieceListing != this)
                if (WebManager.webManagerInstance.playerDatabaseAdaptor.armorDataString.Contains(armorPieceListing.playerArmorPiece.entity))
                {
                    armorPieceListing.model.SetActive(true);
                }


        model.SetActive(false);
    }
}
