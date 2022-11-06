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

    // Lootable Weapons
    #region
    public void EnableLootableWeapon(Vector3 position)
    {
        GetComponent<PhotonView>().RPC("EnableLootableWeapon_RPC", RpcTarget.All, position);
    }
    public void DisableLootableWeapon(Vector3 position)
    {
        GetComponent<PhotonView>().RPC("DisableLootableWeapon_RPC", RpcTarget.All, position);
    }
    public void RelocateLootableWeapon(Vector3 position, Quaternion rotation)
    {
        GetComponent<PhotonView>().RPC("RelocateLootableWeapon_RPC", RpcTarget.All, position, rotation);
    }
    #endregion

    // Explosive Barrel
    #region
    public void DamageExplosiveBarrel(Vector3 position, int val)
    {
        GetComponent<PhotonView>().RPC("DamageExplosiveBarrel_RPC", RpcTarget.All, position, val);
    }
    public void EnableExplosiveBarrel(Vector3 position)
    {
        GetComponent<PhotonView>().RPC("EnableExplosiveBarrel_RPC", RpcTarget.All, position);
    }
    public void RelocateExplosiveBarrel(Vector3 position, Quaternion rotation)
    {
        GetComponent<PhotonView>().RPC("RelocateExplosiveBarrel_RPC", RpcTarget.All, position, rotation);
    }
    #endregion

    public void DamageIceChunk(Vector3 position, int val)
    {
        GetComponent<PhotonView>().RPC("DamageIceChunk_RPC", RpcTarget.All, position, val);
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
    #endregion

    // Lootable Weapons
    #region
    [PunRPC]
    void DisableLootableWeapon_RPC(Vector3 position)
    {
        foreach (LootableWeapon lw in FindObjectsOfType<LootableWeapon>().ToList())
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
            if (lw.spawnPointPosition == position)
            {
                lw.gameObject.SetActive(true);
                lw.transform.position = lw.spawnPointPosition;
                lw.transform.rotation = lw.spawnPointRotation;
            }
        }
    }

    [PunRPC]
    void RelocateLootableWeapon_RPC(Vector3 position, Quaternion rotation)
    {
        foreach (LootableWeapon lw in FindObjectsOfType<LootableWeapon>(true).ToList())
        {
            if (lw.spawnPointPosition == position)
            {
                lw.GetComponent<Rigidbody>().velocity *= 0;

                lw.transform.position = position;
                lw.transform.rotation = rotation;
            }
        }
    }
    #endregion


    // Explosive Barrel
    #region
    [PunRPC]
    void DamageExplosiveBarrel_RPC(Vector3 sp, int val)
    {
        foreach (ExplosiveBarrel eb in FindObjectsOfType<ExplosiveBarrel>(false).ToList())
        {
            if (eb.spawnPointPosition == sp)
            {
                eb._networkHitPoints = val;
            }
        }
    }
    [PunRPC]
    void EnableExplosiveBarrel_RPC(Vector3 position)
    {
        foreach (ExplosiveBarrel ic in FindObjectsOfType<ExplosiveBarrel>(true).ToList())
        {
            if (ic.spawnPointPosition == position)
            {
                ic.gameObject.SetActive(true);
                ic.transform.position = ic.spawnPointPosition;
                ic.transform.rotation = ic.spawnPointRotation;
            }
        }
    }

    [PunRPC]
    void RelocateExplosiveBarrel_RPC(Vector3 position, Quaternion rotation)
    {
        foreach (ExplosiveBarrel lw in FindObjectsOfType<ExplosiveBarrel>(true).ToList())
        {
            if (lw.spawnPointPosition == position)
            {
                lw.GetComponent<Rigidbody>().velocity *= 0;
                lw.transform.position = position;
                lw.transform.rotation = rotation;
            }
        }
    }
    #endregion

    [PunRPC]
    void DamageIceChunk_RPC(Vector3 position, int val)
    {
        foreach (IceChunk ic in FindObjectsOfType<IceChunk>(true).ToList())
        {
            if (ic.transform.position == position)
            {
                ic._networkHitPoints = val;
            }
        }
    }

}
