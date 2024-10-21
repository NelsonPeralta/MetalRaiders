using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using System;

public class ArmorPieceListing : MonoBehaviour
{
    public GameObject model
    {
        get
        {
            return playerArmorPiece.gameObject;
        }
    }

    public TMP_Text armorPieceNameText;
    public TMP_Text armorPieceBodyPartText;

    public Button buyButton;
    public Button equipButton;
    public Button unequipButton;
    public Button notEnoughCreditsButton;
    public Button lockedButton;

    public ArmorPieceListing(PlayerArmorPiece p_playerArmorPiece)
    {
        playerArmorPiece = p_playerArmorPiece;
    }

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


            PlayerDatabaseAdaptor pda = WebManager.webManagerInstance.pda;
            _playerArmorPiece = value;

            armorPieceNameText.text = playerArmorPiece.cleanName;
            //armorPieceBodyPartText.text = playerArmorPiece.bodyPart.ToString();
            armorPieceBodyPartText.text = string.Concat(playerArmorPiece.bodyPart.ToString().Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
            //armorPieceNameText.text = $"{playerArmorPiece.bodyPart} {playerArmorPiece.cleanName}";

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

            if (playerArmorPiece.minLvl > 0 || playerArmorPiece.minHonor > 0)
            {
                Debug.Log(pda.playerBasicOnlineStats.credits);
                Debug.Log(playerArmorPiece.cost);
                Debug.Log(pda.level);
                Debug.Log(playerArmorPiece.minLvl);
                Debug.Log(pda.honor);
                Debug.Log(playerArmorPiece.minHonor);

                if (pda.playerBasicOnlineStats.credits >= playerArmorPiece.cost)
                    Debug.Log("Here1");
                if (pda.level >= playerArmorPiece.minLvl)
                    Debug.Log("Here2");
                if (pda.honor >= playerArmorPiece.minHonor)
                    Debug.Log("Her3");

                if (pda.playerBasicOnlineStats.credits >= playerArmorPiece.cost &&
                    (pda.level >= playerArmorPiece.minLvl && pda.honor >= playerArmorPiece.minHonor))
                    Debug.Log("Her4");
            }

            if (playerArmorPiece.cost > 0)
                if (pda.playerBasicOnlineStats.credits >= playerArmorPiece.cost &&
                    (pda.level >= playerArmorPiece.minLvl && pda.honor >= playerArmorPiece.minHonor))
                {
                    buyButton.gameObject.SetActive(true);
                    buyButton.GetComponentInChildren<Text>().text = $"{playerArmorPiece.cost}cb";
                    Debug.Log("Her5");
                }
                else
                {
                    Debug.Log("Her6");
                    buyButton.gameObject.SetActive(false);
                    equipButton.gameObject.SetActive(false);


                    notEnoughCreditsButton.gameObject.SetActive(true);
                    notEnoughCreditsButton.GetComponentInChildren<Text>().text = $"{playerArmorPiece.cost}cb";

                    if (pda.honor < playerArmorPiece.minLvl)
                        notEnoughCreditsButton.GetComponentInChildren<Text>().text = $"lvl {playerArmorPiece.minLvl}";

                    if (pda.honor < playerArmorPiece.minHonor)
                        notEnoughCreditsButton.GetComponentInChildren<Text>().text = $"{playerArmorPiece.minHonor}ho";

                }

            if (playerArmorPiece.cost < 0)
            {
                lockedButton.gameObject.SetActive(true);
            }

        }
    }

    [SerializeField] PlayerArmorPiece _playerArmorPiece;


    void BuyArmorPiece()
    {
        print("BuyArmorPiece");
        GameManager.PlayClickSound();
        StartCoroutine(WebManager.webManagerInstance.SaveUnlockedArmorStringData_Coroutine(playerArmorPiece));

        ArmoryManager.instance.creditsText.text = $"{WebManager.webManagerInstance.pda.playerBasicOnlineStats.credits.ToString()}cb";
        ArmoryManager.instance.OnArmorBuy_Delegate();
        buyButton.gameObject.SetActive(false);
        equipButton.gameObject.SetActive(true);
    }

    void EquipArmorPiece()
    {
        print("EquipArmorPiece");
        //Debug.Log($"Previous: {WebManager.webManagerInstance.pda.armorDataString}");
        GameManager.PlayClickSound();
        string newData = WebManager.webManagerInstance.pda.armorDataString;


        foreach (ArmorPieceListing armorPieceListing in ArmoryManager.instance.armorPieceListingList)
            if (armorPieceListing != this)
                if (this.playerArmorPiece.pieceType == armorPieceListing.playerArmorPiece.pieceType && this.playerArmorPiece.bodyPart == armorPieceListing.playerArmorPiece.bodyPart)
                {
                    if (WebManager.webManagerInstance.pda.unlockedArmorDataString.Contains(armorPieceListing.playerArmorPiece.entity))
                    {
                        //Debug.Log($"Replacing string");
                        //Debug.Log(armorPieceListing.playerArmorPiece.entity);

                        armorPieceListing.model.gameObject.SetActive(false);
                        armorPieceListing.equipButton.gameObject.SetActive(true);
                        armorPieceListing.unequipButton.gameObject.SetActive(false);

                        newData = newData.Replace($"{armorPieceListing.playerArmorPiece.entity}", "");
                        Debug.Log(newData);
                    }
                }

        newData += $"-{playerArmorPiece.entity}-";
        newData = newData.Replace($"----", "--");
        newData = newData.Replace($"---", "--");

        WebManager.webManagerInstance.pda.armorDataString = newData;
        StartCoroutine(WebManager.webManagerInstance.SaveEquippedArmorStringData_Coroutine(WebManager.webManagerInstance.pda.armorDataString));

        model.gameObject.SetActive(true);
        equipButton.gameObject.SetActive(false);
        if (playerArmorPiece.pieceType == PlayerArmorPiece.PieceType.Attachment)
            unequipButton.gameObject.SetActive(true);
    }

    public void UnequipArmorPiece()
    {
        print("UnequipArmorPiece");
        GameManager.PlayCancelSound();
        string newData = WebManager.webManagerInstance.pda.armorDataString.Replace($"{playerArmorPiece.entity}", "");
        newData = newData.Replace($"----", "--");
        newData = newData.Replace($"---", "--");

        WebManager.webManagerInstance.pda.armorDataString = newData;
        StartCoroutine(WebManager.webManagerInstance.SaveEquippedArmorStringData_Coroutine(WebManager.webManagerInstance.pda.armorDataString));

        model.gameObject.SetActive(false);
        equipButton.gameObject.SetActive(true);
        unequipButton.gameObject.SetActive(false);
    }

    public void OnButtonMouseEnter()
    {
        print("OnButtonMouseEnter");
        foreach (ArmorPieceListing armorPieceListing in ArmoryManager.instance.armorPieceListingList)
            if (armorPieceListing != this)
                try
                {
                    if (this.playerArmorPiece.pieceType == armorPieceListing.playerArmorPiece.pieceType && this.playerArmorPiece.bodyPart == armorPieceListing.playerArmorPiece.bodyPart && armorPieceListing.model.gameObject.activeSelf)
                        armorPieceListing.model.SetActive(false);
                }
                catch { }

        model.SetActive(true);
    }

    public void OnButtonMouseExit()
    {
        print("OnButtonMouseExit");
        foreach (ArmorPieceListing armorPieceListing in ArmoryManager.instance.armorPieceListingList)
            if (armorPieceListing != this)
                if (WebManager.webManagerInstance.pda.armorDataString.Contains(armorPieceListing.playerArmorPiece.entity))
                {
                    armorPieceListing.model.SetActive(true);
                }


        model.SetActive(false);
    }
}
