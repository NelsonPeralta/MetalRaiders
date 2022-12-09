using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Runtime.CompilerServices;

public class PlayerHitbox : Hitbox, IDamageable
{
    public Player player;

    public void Damage(int damage)
    {
        if (player.hitPoints <= 0 || player.isDead || player.isRespawning)
            return;

        Debug.Log("SIMPLER PLAYER HITBOX DAMAGE");
        player.Damage(damage);
        //player.PV.RPC("Damage_RPC", RpcTarget.All, damage);
    }

    public void Damage(int healthDamage, bool headshot, int playerWhoShotThisPlayerPhotonId)
    {
        Damage(healthDamage, headshot, playerWhoShotThisPlayerPhotonId, impactPos: null);
    }

    public void Damage(int healthDamage, bool headshot, int playerWhoShotThisPlayerPhotonId, 
        Vector3? impactPos = null, Vector3? impactDir = null, string damageSource = null, bool isGroin = false,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        if (player.hitPoints <= 0 || player.isDead || player.isRespawning)
            return;

        if (player.isInvincible)
            healthDamage = 0;
        if (player.overshieldPoints > 0)
            healthDamage -= (int)player.overshieldPoints;

        Debug.Log("PLAYER HITBOX DAMAGE");
        Debug.Log("member name: " + memberName);
        Debug.Log("source file path: " + sourceFilePath);
        Debug.Log("source line number: " + sourceLineNumber);

        try
        { // Hit Marker Handling
            Player p = GameManager.GetPlayerWithPhotonViewId(playerWhoShotThisPlayerPhotonId);

            if (player.isInvincible)
                healthDamage = 0;
            if (player.overshieldPoints > 0)
                healthDamage -= (int)player.overshieldPoints;

            if (player.hitPoints <= healthDamage)
                p.GetComponent<PlayerUI>().SpawnHitMarker(PlayerUI.HitMarkerType.Kill);
            else
                p.GetComponent<PlayerUI>().SpawnHitMarker();
        }
        catch { }

        //player.Damage((int)player.hitPoints - healthDamage, headshot, playerWhoShotThisPlayerPhotonId, impactPos, impactDir, damageSource, isGroin); ;
        player.PV.RPC("Damage_RPC", RpcTarget.All, player.hitPoints - healthDamage, headshot, playerWhoShotThisPlayerPhotonId, impactPos, impactDir, damageSource, isGroin);
    }
}
