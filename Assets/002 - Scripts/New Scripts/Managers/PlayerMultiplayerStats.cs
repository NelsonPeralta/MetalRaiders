using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMultiplayerStats : MonoBehaviour
{
    public PlayerProperties player;
    public int PVID;
    public string playerName;
    public int kills;
    public int deaths;

    public PlayerMultiplayerStats(PlayerProperties pp)
    {
        player = pp;
        PVID = pp.GetComponent<PhotonView>().ViewID;
        playerName = pp.GetComponent<PhotonView>().Owner.NickName;
    }

    public PlayerMultiplayerStats(int pvid, string pName, int k, int d)
    {
        PVID = pvid;
        playerName = pName;
        kills = k;
        deaths = d;
    }
}
