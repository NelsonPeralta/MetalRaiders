using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkGameManager : MonoBehaviourPun
{
    public static NetworkGameManager instance;
    private void Start()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void UpdateTeamMode(string tm)
    {
        GameManager.instance.teamMode = (GameManager.TeamMode)System.Enum.Parse(typeof(GameManager.TeamMode), tm);
    }
    public void UpdateTeam(string t, string playerNickName)
    {
        GetComponent<PhotonView>().RPC("UpdateTeam_RPC", RpcTarget.All, t.ToString(), playerNickName);
    }

    [PunRPC]
    void UpdateTeam_RPC(string t, string playerNickName)
    {
        Debug.Log($"UpdateTeam_RPC: {t}, {playerNickName}");

        PlayerMultiplayerMatchStats.Team te = (PlayerMultiplayerMatchStats.Team)System.Enum.Parse(typeof(PlayerMultiplayerMatchStats.Team), t);

        GameManager.instance.onlineTeam = te;
    }

    [PunRPC]
    void UpdateTeamMode_RPC(string t)
    {
        GameManager.TeamMode tm = (GameManager.TeamMode)System.Enum.Parse(typeof(GameManager.TeamMode), t);

        GameManager.instance.teamMode = tm;
    }
}
