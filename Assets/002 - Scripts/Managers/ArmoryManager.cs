using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ArmoryManager : MonoBehaviour
{
    public GameObject playerModel;

    public TMP_Text armorDataString;
    public TMP_Text newArmorDataString;
    public TMP_Text unlockedArmorDataString;

    private void OnEnable()
    {
        playerModel.SetActive(true);
        PlayerDatabaseAdaptor pda = WebManager.webManagerInstance.playerDatabaseAdaptor;

        armorDataString.text = $"ADS: {pda.armorDataString.ToString()}";
        newArmorDataString.text = $"NADS: {pda.armorDataString.ToString()}";
        unlockedArmorDataString.text = $"UADS: {pda.unlockedArmorDataString.ToString()}";
    }

    private void OnDisable()
    {
        playerModel.SetActive(false);
    }
}
