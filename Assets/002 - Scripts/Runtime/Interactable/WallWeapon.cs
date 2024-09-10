using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WallWeapon : InteractableObject
{
    [SerializeField] int _cost, _ammoCost;
    [SerializeField] WeaponProperties.KillFeedOutput _killFeedOutput;
    [SerializeField] List<Player> playersInRange = new List<Player>();





    public int cost { get { return _cost; } }








    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<PlayerCapsule>() && !other.transform.root.GetComponent<Player>().isDead && !playersInRange.Contains(other.transform.root.GetComponent<Player>()))
        {
            playersInRange.Add(other.transform.root.GetComponent<Player>());

            other.transform.root.GetComponent<PlayerController>().OnPlayerLongInteract -= OnPlayerLongInteract_Delegate;
            other.transform.root.GetComponent<PlayerController>().OnPlayerLongInteract += OnPlayerLongInteract_Delegate;

            if (other.transform.root.GetComponent<Player>().playerInventory.activeWeapon.killFeedOutput != _killFeedOutput && other.transform.root.GetComponent<Player>().playerInventory.holsteredWeapon.killFeedOutput != _killFeedOutput)
            {
                if (other.transform.root.GetComponent<PlayerSwarmMatchStats>().points >= _cost)
                    other.transform.root.GetComponent<PlayerUI>().ShowInformer($"Buy {_killFeedOutput.ToString().Replace("_", " ")} [Cost: {_cost}]");
                else
                    other.transform.root.GetComponent<PlayerUI>().ShowInformer($"Not enough points [Cost: {_cost}]");
            }
            else
            {
                other.transform.root.GetComponent<PlayerUI>().ShowInformer($"You ammo for [Cost: {_ammoCost}]");
            }
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
        if (playerController.GetComponent<PlayerSwarmMatchStats>().points >= _cost)
        {
            NetworkGameManager.instance.AskHostToTriggerInteractableObject(transform.position, playerController.player.photonId);
        }
    }


    public override void Trigger(int? pid)
    {
        Player p = GameManager.GetPlayerWithPhotonView((int)pid);




        if (p.playerInventory.activeWeapon.killFeedOutput != _killFeedOutput && p.playerInventory.holsteredWeapon.killFeedOutput != _killFeedOutput)
        {
            GameObject _preActWeap = p.playerInventory.activeWeapon.gameObject;


            p.playerInventory.activeWeapon = p.playerInventory.allWeaponsInInventory.Where(item => item.GetComponent<WeaponProperties>().killFeedOutput == _killFeedOutput).FirstOrDefault().GetComponent<WeaponProperties>();
            p.playerInventory.activeWeapon.loadedAmmo = p.playerInventory.activeWeapon.ammoCapacity;
            p.playerInventory.activeWeapon.spareAmmo = p.playerInventory.activeWeapon.maxSpareAmmo;
            p.killFeedManager.EnterNewFeed($"You purchased a {_killFeedOutput.ToString().Replace("_", " ")}");

            _preActWeap.gameObject.SetActive(false);
        }
        else
        {
            if (p.playerInventory.activeWeapon.killFeedOutput == _killFeedOutput)
            {
                p.playerInventory.activeWeapon.loadedAmmo = p.playerInventory.activeWeapon.ammoCapacity;
                p.playerInventory.activeWeapon.spareAmmo = p.playerInventory.activeWeapon.maxSpareAmmo;
            }
            else
            {
                p.playerInventory.holsteredWeapon.loadedAmmo = p.playerInventory.holsteredWeapon.ammoCapacity;
                p.playerInventory.holsteredWeapon.spareAmmo = p.playerInventory.holsteredWeapon.maxSpareAmmo;
            }
            p.killFeedManager.EnterNewFeed($"You purchased {_killFeedOutput.ToString().Replace("_", " ")} ammo");
        }



        playersInRange.Remove(p);
        p.transform.GetComponent<PlayerUI>().HideInformer();
        p.GetComponent<PlayerSwarmMatchStats>().RemovePoints(_cost);






        return;
        p.hasArmor = true;

        p.playerArmorManager.ReloadArmorData();
        p.playerArmorManager.ReloadFpsArmor();
        p.playerShield.ShowShieldRechargeEffect();
        p.playerShield.PlayShieldStartSound(p);



        playersInRange.Remove(p);
        p.transform.GetComponent<PlayerUI>().HideInformer();
        p.GetComponent<PlayerSwarmMatchStats>().RemovePoints(_cost);



        bool achUnl;
        Steamworks.SteamUserStats.GetAchievement("UPGRADE", out achUnl);
        if (achUnl == false)
        {
            Steamworks.SteamUserStats.SetAchievement("UPGRADE");
        }
    }
}
