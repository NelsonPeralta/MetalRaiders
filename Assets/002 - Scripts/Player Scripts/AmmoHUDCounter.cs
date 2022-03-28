using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum AmmoType { Small, Heavy, Power};
public class AmmoHUDCounter : MonoBehaviour
{
    public AllPlayerScripts allPlayerScripts;
    PlayerInventory playerInventory;
    public AmmoType ammoType;
    public Text ammoTextDrawn;
    public Text ammoTextHolstered;
    public Text extraAmmoText;
    public int drawnFontSize;
    public int holsteredFontSize;
    public bool useThisCounter;

    private void Start(){
        playerInventory = allPlayerScripts.playerInventory;

        ammoTextDrawn.text = ".";
        ammoTextHolstered.text = ".";
        extraAmmoText.text = ".";
    }

    private void Update()
    {
        if (useThisCounter)
        {
            ammoTextDrawn.text = playerInventory.currentAmmo.ToString();
            extraAmmoText.text = playerInventory.currentExtraAmmo.ToString();
        }
    }

    public void changeToHolstered()
    {
        ammoTextHolstered.text = ammoTextDrawn.text;
        ammoTextDrawn.gameObject.SetActive(false);
        ammoTextHolstered.gameObject.SetActive(true);
        useThisCounter = false;
    }

    public void changeToDrawn()
    {
        ammoTextDrawn.gameObject.SetActive(true);
        ammoTextHolstered.gameObject.SetActive(false);
        useThisCounter = true;
    }

    public void UpdateExtraAmmo()
    {
        if (this == playerInventory.smallAmmoHudCounter)
        {
            extraAmmoText.text = playerInventory.smallAmmo.ToString();
        }
        else if (this == playerInventory.heavyAmmoHudCounter)
        {
            extraAmmoText.text = playerInventory.heavyAmmo.ToString();
        }
        else if (this == playerInventory.powerAmmoHudCounter)
        {
            extraAmmoText.text = playerInventory.powerAmmo.ToString();
        }
    }
}
