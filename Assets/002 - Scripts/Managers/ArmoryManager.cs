using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ArmoryManager : MonoBehaviour
{
    public static ArmoryManager instance;

    public GameObject playerModel;
    public Transform scrollMenuContainer;

    public TMP_Text creditsText;
    public TMP_Text armorDataString;
    public TMP_Text newArmorDataString;
    public TMP_Text unlockedArmorDataString;

    public ArmorPieceListing armorPieceListingPrefab;
    public List<ArmorPieceListing> armorPieceListingList = new List<ArmorPieceListing>();

    void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        instance = this;
    }
    private void OnEnable()
    {
        playerModel.SetActive(true);
        PlayerDatabaseAdaptor pda = WebManager.webManagerInstance.playerDatabaseAdaptor;

        creditsText.text = $"{pda.playerBasicOnlineStats.credits}cr";
        armorDataString.text = $"ADS: {pda.armorDataString.ToString()}";
        newArmorDataString.text = $"NADS: {pda.armorDataString.ToString()}";
        unlockedArmorDataString.text = $"UADS: {pda.unlockedArmorDataString.ToString()}";

        foreach(PlayerArmorPiece playerArmorPiece in playerModel.GetComponent<PlayerArmorManager>().playerArmorPieces)
        {
            GameObject pal = Instantiate(armorPieceListingPrefab.gameObject, scrollMenuContainer);
            armorPieceListingList.Add(pal.GetComponent<ArmorPieceListing>());
            pal.GetComponent<ArmorPieceListing>().playerArmorPiece = playerArmorPiece;
        } 
            
    }

    private void OnDisable()
    {
        playerModel.SetActive(false);

        foreach(ArmorPieceListing armorPieceListing in armorPieceListingList)
            Destroy(armorPieceListing.gameObject);
        armorPieceListingList.Clear();
    }

    public void OnArmorBuy_Delegate()
    {
        foreach (ArmorPieceListing armorPieceListing in armorPieceListingList)
            armorPieceListing.playerArmorPiece = armorPieceListing.playerArmorPiece;

    }
}