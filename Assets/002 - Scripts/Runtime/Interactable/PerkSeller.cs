using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;
using System;
abstract public class PerkSeller : MonoBehaviour
{
    public string perkName;
    public GameObject model;

    [Header("Seller Info")]
    public int cost = -1;

    [Header("Players in Range")]
    public List<Player> playersInRange = new List<Player>();
    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<Player>() && !other.GetComponent<Player>().isDead && !playersInRange.Contains(other.GetComponent<Player>()))
        {
            playersInRange.Add(other.GetComponent<Player>());
            OnTriggerStay_Abstract(other.GetComponent<Player>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Player>() && playersInRange.Contains(other.GetComponent<Player>()))
        {
            playersInRange.Remove(other.GetComponent<Player>());
            other.GetComponent<PlayerUI>().weaponInformerText.text = $"";
        }
    }

    protected void OnPlayerLongInteract_Delegate(PlayerController playerController)
    {
        if (playerController.GetComponent<PlayerSwarmMatchStats>().points >= cost)
        {
            OnPlayerLongInteract_Abstract(playerController.GetComponent<Player>());

            playersInRange.Remove(playerController.GetComponent<Player>());
            playerController.GetComponent<PlayerUI>().weaponInformerText.text = $"";
        }
    }

    protected abstract void OnTriggerStay_Abstract(Player player);
    protected abstract void OnPlayerLongInteract_Abstract(Player player);
}
