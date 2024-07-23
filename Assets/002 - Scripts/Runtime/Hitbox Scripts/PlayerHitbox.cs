using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Runtime.CompilerServices;
using UnityEngine.SceneManagement;

public class PlayerHitbox : Hitbox, IDamageable
{
    public Player player;

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
            Destroy(gameObject);
    }

    public void Damage(int damage)
    {
        if (player.hitPoints <= 0 || player.isDead || player.isRespawning)
            return;

        Debug.Log("SIMPLER PLAYER HITBOX DAMAGE");
        player.BasicDamage(damage);
        //player.PV.RPC("Damage_RPC", RpcTarget.All, damage);
    }

    public void Damage(int healthDamage, bool headshot, int playerWhoShotThisPlayerPhotonId)
    {
        Debug.Log($"PlayerHitbox: {healthDamage}");
        Damage(healthDamage, headshot, playerWhoShotThisPlayerPhotonId, impactPos: null);
    }

    public void Damage(int damage, bool headshot, int playerWhoShotThisPlayerPhotonId,
        Vector3? impactPos = null, Vector3? impactDir = null, string damageSource = null,
        bool isGroin = false, int weaponIndx = -1, WeaponProperties.KillFeedOutput kfo = WeaponProperties.KillFeedOutput.Unassigned,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        if (player.hitPoints <= 0 || player.isDead || player.isRespawning)
            return;

        //if (GameManager.instance.teamMode.ToString().Contains("Classic"))
        //    if (GameManager.instance.pid_player_Dict.ContainsKey(playerWhoShotThisPlayerPhotonId) && GameManager.GetPlayerWithPhotonViewId(playerWhoShotThisPlayerPhotonId).team == player.team)
        //        return;

        //Debug.Log(healthDamage);
        //Debug.Log(player.overshieldPoints);

        //if (player.isInvincible)
        //    healthDamage = 0;
        //if (player.overshieldPoints > 0)
        //    healthDamage -= (int)player.overshieldPoints;

        //Debug.Log("PLAYER HITBOX DAMAGE");
        //Debug.Log("member name: " + memberName);
        //Debug.Log("source file path: " + sourceFilePath);
        //Debug.Log("source line number: " + sourceLineNumber);

        //try
        //{ // Hit Marker Handling
        //    Player p = GameManager.GetPlayerWithPhotonViewId(playerWhoShotThisPlayerPhotonId);

        //    if (player.isInvincible)
        //        healthDamage = 0;
        //    if (player.overshieldPoints > 0)
        //        healthDamage -= (int)player.overshieldPoints;

        //    if (player.hitPoints <= healthDamage)
        //        p.GetComponent<PlayerUI>().SpawnHitMarker(PlayerUI.HitMarkerType.Kill);
        //    else
        //        p.GetComponent<PlayerUI>().SpawnHitMarker();
        //}
        //catch { }
        Debug.Log(damage);

        player.Damage((int)damage, headshot, playerWhoShotThisPlayerPhotonId, impactPos, impactDir, damageSource, isGroin, weaponIndx: weaponIndx, kfo: kfo); ;
        //player.PV.RPC("Damage_RPC", RpcTarget.All, damage, headshot, playerWhoShotThisPlayerPhotonId, impactPos, impactDir, damageSource, isGroin);
    }
}
