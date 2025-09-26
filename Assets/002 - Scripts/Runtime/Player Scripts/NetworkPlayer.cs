using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.CompilerServices;
using System;
using System.Linq;

public class NetworkPlayer : MonoBehaviourPunCallbacks
{
    Player player { get { return GetComponent<Player>(); } }
    PhotonView PV { get { return GetComponent<PhotonView>(); } }






    public void Damage(int damage, bool headshot, int source_pid,
         Vector3? impactPos = null, Vector3? impactDir = null, string damageSource = null,
         bool isGroin = false,
         [CallerMemberName] string memberName = "",
         [CallerFilePath] string sourceFilePath = "",
         [CallerLineNumber] int sourceLineNumber = 0)
    {
        Log.Print(() =>$"NetworkPlayer Damage");

        PV.RPC("Damage_RPC", RpcTarget.All, damage, headshot, source_pid,
            impactPos, impactDir, damageSource,
            isGroin);
    }









    [PunRPC]
    void Damage_RPC(int damage, bool headshot, int sourcePid,
        Vector3? impactPos = null, Vector3? impactDir = null, string damageSource = null,
        bool isGroin = false)
    {
        if (player.isMine)
        {
            if (player.hitPoints <= 0 || player.isRespawning || player.isDead)
                return;

            Log.Print(() =>$"Damage_RPC: {damage}");
            Log.Print(() =>damage);
            Log.Print(() =>player.hitPoints);
            Log.Print(() =>damageSource);

        }
    }
}
