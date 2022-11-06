using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class NetworkGameManager : MonoBehaviourPun
{
    public static NetworkGameManager instance;
    private void Start()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }


    // Methods
    #region
    public void UpdateTeamMode(string tm)
    {
        GameManager.instance.teamMode = (GameManager.TeamMode)System.Enum.Parse(typeof(GameManager.TeamMode), tm);
    }
    public void UpdateTeam(string t, string playerNickName)
    {
        GetComponent<PhotonView>().RPC("UpdateTeam_RPC", RpcTarget.All, t.ToString(), playerNickName);
    }
    public void DisableLootableWeapon(Vector3 position)
    {
        GetComponent<PhotonView>().RPC("DisableLootableWeapon_RPC", RpcTarget.All, position);
    }
    public void EnableLootableWeapon(Vector3 position)
    {
        GetComponent<PhotonView>().RPC("EnableLootableWeapon_RPC", RpcTarget.All, position);
    }

    public void RelocateLootableWeapon(Vector3 position, Quaternion rotation)
    {
        GetComponent<PhotonView>().RPC("RelocateLootableWeapon_RPC", RpcTarget.All, position, rotation);
    }
    #endregion

    // RPCs
    #region
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

    [PunRPC]
    void DisableLootableWeapon_RPC(Vector3 position)
    {
        foreach(LootableWeapon lw in FindObjectsOfType<LootableWeapon>().ToList())
        {
            if (lw.spawnPointPosition == position)
                lw.gameObject.SetActive(false);
        }
    }

    [PunRPC]
    void EnableLootableWeapon_RPC(Vector3 position)
    {
        foreach (LootableWeapon lw in FindObjectsOfType<LootableWeapon>(true).ToList())
        {
            Debug.Log($"Weapon: {lw.cleanName}. SP: {lw.spawnPointPosition}. Is Active: {lw.gameObject.activeSelf}");
            if (lw.spawnPointPosition == position)
                lw.gameObject.SetActive(true);
        }
    }

    [PunRPC]
    void RelocateLootableWeapon_RPC(Vector3 position, Quaternion rotation)
    {
        foreach (LootableWeapon lw in FindObjectsOfType<LootableWeapon>(true).ToList())
        {
            if (lw.spawnPointPosition == position)
            {
                lw.transform.position = position;
                lw.transform.rotation = rotation;
            }
        }
    }
    #endregion
}
