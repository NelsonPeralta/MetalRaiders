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
                if(!pda.armorDataString.Contains(playerArmorPiece.entity))
                    equipButton.gameObject.SetActive(true);
                else
                    unequipButton.gameObject.SetActive(true);
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
        WebManager.webManagerInstance.playerDatabaseAdaptor.armorDataString += $"\n{playerArmorPiece.entity}";
        StartCoroutine(WebManager.webManagerInstance.SaveEquippedArmorStringData_Coroutine(WebManager.webManagerInstance.playerDatabaseAdaptor.armorDataString));

        model.gameObject.SetActive(true);
        equipButton.gameObject.SetActive(false);
        unequipButton.gameObject.SetActive(true);
    }

    void UnequipArmorPiece()
    {
        string newData = WebManager.webManagerInstance.playerDatabaseAdaptor.armorDataString.Replace(playerArmorPiece.entity, "");
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
        model.SetActive(true);
    }

    public void OnButtonMouseExit()
    {
        Debug.Log("OnButtonMouseExit");
        Debug.Log(model.name);
        model.SetActive(false);
    }
}
