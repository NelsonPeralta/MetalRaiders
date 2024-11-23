using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.ProBuilder.Shapes;

public class ArmorSeller : InteractableObject
{
    public GameObject armorModel;

    [Header("Seller Info")]
    public int cost = -1;


    private void Start()
    {
#if UNITY_EDITOR
        cost = 1;
#endif

        if (GameManager.instance.devMode)
            cost = 1000;
    }



    public override void Trigger(int? pid)
    {
        StartCoroutine(DisableThenEnableCollider());
        Player p = GameManager.GetPlayerWithPhotonView((int)pid);

        p.hasArmor = true;

        p.playerArmorManager.ReloadArmorData();
        p.playerArmorManager.ReloadFpsArmor();
        p.playerShield.ShowShieldRechargeEffect();
        p.playerShield.PlayShieldStartSound(p);



        p.GetComponent<PlayerSwarmMatchStats>().RemovePoints(cost);



        bool achUnl;
        Steamworks.SteamUserStats.GetAchievement("UPGRADE", out achUnl);
        if (achUnl == false)
        {
            Steamworks.SteamUserStats.SetAchievement("UPGRADE");
        }
    }



    IEnumerator DisableThenEnableCollider()
    {
        GetComponent<Collider>().enabled = false;
        yield return new WaitForEndOfFrame();
        GetComponent<Collider>().enabled = true;
    }
}
