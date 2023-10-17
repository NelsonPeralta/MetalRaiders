using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class NetworkSwarmManager : MonoBehaviourPun
{
    public static NetworkSwarmManager instance;
    SwarmManager swarmManager
    {
        get
        {
            return SwarmManager.instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }


    [PunRPC]
    void IncreaseWave_RPC()
    {
        swarmManager.IncreaseWave();
    }

    [PunRPC]
    void StartNewWave_RPC()
    {
        Debug.Log($"StartNewWave_RPC");
        swarmManager.StartNewWave();
    }

    [PunRPC]
    void SpawnAi_RPC(int aiPhotonId, int targetPhotonId, Vector3 spawnPointPosition, Quaternion spawnPointRotation, string aiType, int pdelay = -1)
    {
        Debug.Log($"SpawnAi_RPC. AI pdi: {aiPhotonId}");
        swarmManager.SpawnAi(aiPhotonId, targetPhotonId, spawnPointPosition, spawnPointRotation, aiType, pdelay);
    }

    [PunRPC]
    void EndWave_RPC()
    {
        Debug.Log("EndWave_RPC");
        swarmManager.EndWave();
    }

    [PunRPC]
    void RespawnHealthPacks_RPC()
    {
        Debug.Log("Respawn Health Packs RPC");
        swarmManager.RespawnHealthPacks();
    }

    [PunRPC]
    void RespawnHealthPack_RPC(Vector3 hpPosition, int time)
    {
        swarmManager.RespawnHealthPack(hpPosition, time);
    }

    [PunRPC]
    void DisableHealthPack_RPC(Vector3 hpPosition)
    {
        swarmManager.DisableHealthPack(hpPosition);
    }

    [PunRPC]
    void DropRandomLoot_RPC(string ammotype, Vector3 position, Quaternion rotation)
    {
        swarmManager.DropRandomLoot(ammotype, position, rotation);
    }

    [PunRPC]
    public void EnableStartingNetworkWeapons(bool punCall = true)
    {
        if (punCall)
        {
            GetComponent<PhotonView>().RPC("EnableStartingNetworkWeapons", RpcTarget.All, false);
        }
        else
        {
            foreach (NetworkWeaponSpawnPoint nsp in FindObjectsOfType<NetworkWeaponSpawnPoint>().ToList())
            {
                nsp.weaponSpawned.gameObject.SetActive(true);
            }
        }
    }

    [PunRPC]

    public void CreateAIPool()
    {
        FindObjectOfType<SwarmManager>().CreateAIPool(false);
    }
    public int GetRandomAlivePlayerPhotonId()
    {
        //if (punCall)
        //{
        //    GetComponent<PhotonView>().RPC("GetRandomAlivePlayerTransform", RpcTarget.All, false);
        //}
        //else
        {
            foreach (Player nsp in FindObjectsOfType<Player>().ToList())
            {
                if (!nsp.isDead && !nsp.isRespawning)
                    return nsp.GetComponent<PhotonView>().ViewID;

            }
        }

        return -1;
    }



    public int GetRandomAliveActorId()
    {
        {
            foreach (Actor nsp in FindObjectsOfType<Actor>().ToList())
            {
                if (nsp.hitPoints > 0)
                    return nsp.GetComponent<PhotonView>().ViewID;
            }
        }

        return -1;
    }



    [PunRPC]
    public void SetTurretTarget(Vector3 turretpos, int pid, bool caller = true)
    {
        if (caller)
        {
            GetComponent<PhotonView>().RPC("SetTurretTarget", RpcTarget.AllViaServer, turretpos, pid, false);
        }
        else
        {
            foreach (TurretHead th in FindObjectsOfType<TurretHead>().ToList())
            {
                if (th.transform.position == turretpos)
                    th.GetComponent<TurretHead>().hitPoints = PhotonView.Find(pid).GetComponent<HitPoints>();
            }
        }
    }
}
