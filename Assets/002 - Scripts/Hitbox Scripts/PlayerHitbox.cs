using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerHitbox : Hitbox, IDamageable
{
    public Player player;

    public void Damage(int damage)
    {
        player.hitPoints -= damage;
    }

    public void Damage(int healthDamage, bool headshot, int playerWhoShotThisPlayerPhotonId)
    {
        throw new System.NotImplementedException();
    }

    public void Damage(int healthDamage, bool headshot, int playerWhoShotThisPlayerPhotonId, Vector3? impactPos = null, string damageSource = null, bool isGroin = false)
    {
        if (player.hitPoints <= 0 || player.isDead || player.isRespawning)
            return;

        try
        { // Hit Marker Handling
            Player p = GameManager.GetPlayerWithPhotonViewId(playerWhoShotThisPlayerPhotonId);

            if (headshot)
            {
                if (player.healthPoints <= healthDamage)
                    p.GetComponent<PlayerUI>().SpawnHitMarker(PlayerUI.HitMarkerType.HeadshotKill);
                else
                    p.GetComponent<PlayerUI>().SpawnHitMarker(PlayerUI.HitMarkerType.Headshot);
            }
            else
            {
                if (player.healthPoints <= healthDamage)
                    p.GetComponent<PlayerUI>().SpawnHitMarker(PlayerUI.HitMarkerType.Kill);
                else
                    p.GetComponent<PlayerUI>().SpawnHitMarker();
            }
        }
        catch { }

        player.PV.RPC("Damage_RPC", RpcTarget.All, player.hitPoints - healthDamage, headshot, playerWhoShotThisPlayerPhotonId, impactPos, damageSource, isGroin);

    }
}
