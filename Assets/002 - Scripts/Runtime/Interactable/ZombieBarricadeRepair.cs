using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieBarricadeRepair : InteractableObject
{
    [SerializeField] ZombieBarricade _zombieBarricade;
    [SerializeField] List<Player> playersInRange = new List<Player>();


    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<PlayerCapsule>() && !other.transform.root.GetComponent<Player>().isDead && !playersInRange.Contains(other.transform.root.GetComponent<Player>()))
        {
            playersInRange.Add(other.transform.root.GetComponent<Player>());

            other.transform.root.GetComponent<PlayerController>().OnPlayerLongInteract -= OnPlayerLongInteract_Delegate;
            other.transform.root.GetComponent<PlayerController>().OnPlayerLongInteract += OnPlayerLongInteract_Delegate;

            other.transform.root.GetComponent<PlayerUI>().ShowInformer($"Hold Interact To Repair");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        try
        {
            other.transform.root.GetComponent<PlayerController>().OnPlayerLongInteract -= OnPlayerLongInteract_Delegate;
            playersInRange.Remove(other.transform.root.GetComponent<Player>());

            other.transform.root.GetComponent<PlayerUI>().HideInformer();
        }
        catch { }
    }



    void OnPlayerLongInteract_Delegate(PlayerController playerController)
    {
        NetworkGameManager.instance.AskHostToTriggerInteractableObject(transform.position, playerController.player.photonId);
    }




    private void OnDisable()
    {
        try
        {
            foreach (Player player in playersInRange)
            {
                player.GetComponent<PlayerController>().OnPlayerLongInteract -= OnPlayerLongInteract_Delegate;
                player.playerUI.ShowInformer($"");
            }

            playersInRange.Clear();
        }
        catch { }
    }



    public override void Trigger(int? pid)
    {
        _zombieBarricade.Repair();

        GameManager.GetPlayerWithPhotonView((int)pid).GetComponent<PlayerSwarmMatchStats>().AddPoints(20);
        GameManager.GetPlayerWithPhotonView((int)pid).playerUI.ShowPointWitness(20);
    }
}
