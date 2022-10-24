using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Photon.Pun;
public class AmmoUpgradeSeller : PerkSeller
{
    protected override void OnTriggerStay_Abstract(Player player)
    {
        if (!player.playerInventory.hasAmmoUpgrade)
        {
            if (player.GetComponent<PlayerSwarmMatchStats>().points >= cost)
            {
                player.GetComponent<PlayerController>().OnPlayerLongInteract -= OnPlayerLongInteract_Delegate;
                player.GetComponent<PlayerController>().OnPlayerLongInteract += OnPlayerLongInteract_Delegate;
                player.GetComponent<PlayerUI>().weaponInformerText.text = $"Buy {perkName} for {cost} points";
            }
            else
                player.GetComponent<PlayerUI>().weaponInformerText.text = $"Not enough points ({cost})";
        }
        else
            player.GetComponent<PlayerUI>().weaponInformerText.text = $"You already have this perk ({perkName})";
    }
    protected override void OnPlayerLongInteract_Abstract(Player player)
    {
        player.playerInventory.hasAmmoUpgrade = true;

        playersInRange.Remove(player);
        player.GetComponent<PlayerUI>().weaponInformerText.text = $"";
    }
}
