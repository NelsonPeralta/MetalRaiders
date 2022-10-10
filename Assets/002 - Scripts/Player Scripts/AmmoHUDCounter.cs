using Photon.Pun;
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

    public void ChangeToHolstered()
    {
        ammoTextHolstered.text = ammoTextDrawn.text;
        ammoTextDrawn.gameObject.SetActive(false);
        ammoTextHolstered.gameObject.SetActive(true);
        useThisCounter = false;
    }

    public void ChangeToDrawn()
    {
        ammoTextDrawn.gameObject.SetActive(true);
        ammoTextHolstered.gameObject.SetActive(false);
        useThisCounter = true;
    }

    public void UpdateExtraAmmo()
    {
        // TODO: Deprecate this
    }
}
