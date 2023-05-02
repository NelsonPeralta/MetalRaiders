using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using UnityEngine.UIElements;
using System;
using Newtonsoft.Json;

public class NetworkGameManager : MonoBehaviourPunCallbacks
{
    public static NetworkGameManager instance;

    Overshield _overshield;
    PhotonView _pv;

    private void Awake()
    {
        Debug.Log("NetworkGameManager Awake");
        _pv = GetComponent<PhotonView>();
        if (instance)
            Destroy(instance.gameObject);

        DontDestroyOnLoad(gameObject);
        instance = this;
    }
    private void Start()
    {
        Debug.Log($"NetworkGameManager Start");
        //if(instance != null)
        //    Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public Overshield overshield
    {
        get { return _overshield; }
        set { _overshield = value; }
    }

    private void OnDestroy()
    {
        Debug.Log($"NetworkGameManager OnDestroy");
    }

    public override void OnLeftRoom()
    {
        Debug.Log($"NetworkGameManager OnLeftRoom");
        //base.OnLeftRoom();

        //Destroy(gameObject);
    }


    // Menu
    #region

    [PunRPC]
    public void SendGameParams(Dictionary<string, string> p = null, bool caller = true)
    {
        if (caller && PhotonNetwork.IsMasterClient)
        {
            Dictionary<string, string> ps = new Dictionary<string, string>();

            ps.Add("gamemode", GameManager.instance.gameMode.ToString());
            ps.Add("gametype", GameManager.instance.gameType.ToString());
            ps.Add("leveltoloadindex", Launcher.instance.levelToLoadIndex.ToString());
            ps.Add("teammode", GameManager.instance.teamMode.ToString());
            //ps.Add("teamdict", string.Join(Environment.NewLine, GameManager.instance.teamDict));
            ps.Add("teamdict", JsonConvert.SerializeObject(GameManager.instance.teamDict));

            Debug.Log($"SendGameParams {GameManager.instance.teamDict}");

            _pv.RPC("SendGameParams", RpcTarget.All, ps, false);
        }
        else if (!caller && !PhotonNetwork.IsMasterClient)
        {
            try { GameManager.instance.gameMode = (GameManager.GameMode)System.Enum.Parse(typeof(GameManager.GameMode), p["gamemode"]); } catch (System.Exception e) { Debug.Log(e); }
            try { GameManager.instance.gameType = (GameManager.GameType)System.Enum.Parse(typeof(GameManager.GameType), p["gametype"]); } catch (System.Exception e) { Debug.Log(e); }
            try { Launcher.instance.levelToLoadIndex = (System.Int16.Parse(p["leveltoloadindex"])); } catch (System.Exception e) { Debug.Log(e); }
            try { GameManager.instance.teamMode = (GameManager.TeamMode)System.Enum.Parse(typeof(GameManager.TeamMode), p["teammode"]); } catch (System.Exception e) { Debug.Log(e); }
            try
            {
                GameManager.instance.teamDict = JsonConvert.DeserializeObject<Dictionary<string, int>>(p["teamdict"]);
                Debug.Log(GameManager.instance.teamDict);
            }
            catch { }
        }
    }

    [PunRPC]

    public void SendNewTeamDict(Dictionary<string, int> d, bool caller = true)
    {
        if (caller)
        {
            _pv.RPC("SendNewTeamDict", RpcTarget.AllViaServer, d, false);
        }
        else
        {
            try
            {
                GameManager.instance.teamDict = d;
            }
            catch { }
        }
    }

    [PunRPC]
    public void EndGame(bool caller = true)
    {
        if (caller && PhotonNetwork.IsMasterClient)
        {
            _pv.RPC("EndGame", RpcTarget.AllViaServer, false);
        }
        else if (!caller)
        {
            if (GameManager.instance.gameMode == GameManager.GameMode.Multiplayer)
                MultiplayerManager.instance.EndGame();
            else if (GameManager.instance.gameMode == GameManager.GameMode.Swarm)
                SwarmManager.instance.EndGame();
        }

    }

    #endregion



    // Methods
    #region
    public void UpdateTeamMode(string tm)
    {
        GameManager.instance.teamMode = (GameManager.TeamMode)System.Enum.Parse(typeof(GameManager.TeamMode), tm);
    }
    public void UpdateTeam(string t, string playerNickName)
    {
        _pv.RPC("UpdateTeam_RPC", RpcTarget.All, t.ToString(), playerNickName);
    }

    public void UpdateSwarmDifficulty(int ei)
    {
        _pv.RPC("UpdateSwarmDifficulty_RPC", RpcTarget.All, ei);

    }


    // Lootable Weapons
    #region
    public void EnableLootableWeapon(Vector3 position)
    {
        Debug.Log("EnableLootableWeapon");
        _pv.RPC("EnableLootableWeapon_RPC", RpcTarget.All, position);
    }
    public void DisableLootableWeapon(Vector3 position)
    {
        _pv.RPC("DisableLootableWeapon_RPC", RpcTarget.All, position);
    }
    public void RelocateLootableWeapon(Vector3 position, Quaternion rotation)
    {
        _pv.RPC("RelocateLootableWeapon_RPC", RpcTarget.All, position, rotation);
    }

    public void UpdateLootableWeaponData(Vector3 spp, Dictionary<string, string> param)
    {
        _pv.RPC("UpdateLootableWeaponData_RPC", RpcTarget.All, spp, param);
    }

    public void AddForceLootableWeapon(Vector3 spp, Vector3 dir)
    {
        _pv.RPC("AddForceLootableWeapon_RPC", RpcTarget.All, spp, dir);
    }

    public static void SpawnNetworkWeapon(int wi, Vector3 spp, Vector3 fDir, Dictionary<string, int> param)
    {
        FindObjectOfType<NetworkGameManager>()._pv.RPC("SpawnNetworkWeapon_RPC", RpcTarget.All, wi, spp, fDir, param);
    }


    #endregion

    // Explosive Barrel
    #region
    public void DamageExplosiveBarrel(Vector3 position, int val)
    {
        _pv.RPC("DamageExplosiveBarrel_RPC", RpcTarget.All, position, val);
    }

    public void ResetAllExplosiveBarrels()
    {
        _pv.RPC("ResetAllExplosiveBarrels_RPC", RpcTarget.All);
    }

    public void EnableExplosiveBarrel(Vector3 position)
    {
        _pv.RPC("EnableExplosiveBarrel_RPC", RpcTarget.All, position);
    }
    public void RelocateExplosiveBarrel(Vector3 position, Quaternion rotation)
    {
        _pv.RPC("RelocateExplosiveBarrel_RPC", RpcTarget.All, position, rotation);
    }
    #endregion

    public void DamageIceChunk(Vector3 position, int val)
    {
        Debug.Log(val);
        _pv.RPC("DamageIceChunk_RPC", RpcTarget.All, position, val);
    }

    public void UpdatePlayerTeam(string t, string pn)
    {
        _pv.RPC("UpdatePlayerTeam_RPC", RpcTarget.All, t, pn);
    }

    public void StartLootableWeaponRespawn(Vector3 v)
    {
        _pv.RPC("StartLootableWeaponRespawn_RPC", RpcTarget.All, v);
    }
    #endregion


    // Explosive Barrel
    #region

    public void DisableAmmoPack(Vector3 sp)
    {
        _pv.RPC("DisableAmmoPack_RPC", RpcTarget.All, sp);
    }

    public void ResetAllAmmoPacks()
    {
        _pv.RPC("ResetAllAmmoPacks_RPC", RpcTarget.All);
    }

    #endregion

    // Multiplayer
    public void AddPlayerPoint(int pid)
    {
        _pv.RPC("AddPlayerPoint_RPC", RpcTarget.All, pid);
    }

    public void NextHillLocation()
    {
        _pv.RPC("NextHillLocation_RPC", RpcTarget.All);
    }











    // RPCs
    #region
    [PunRPC]
    void UpdateTeam_RPC(string t, string playerNickName)
    {

        if (PhotonNetwork.NickName == playerNickName)
        {
            Debug.Log($"UpdateTeam_RPC: {t}, {playerNickName}");
            PlayerMultiplayerMatchStats.Team te = (PlayerMultiplayerMatchStats.Team)System.Enum.Parse(typeof(PlayerMultiplayerMatchStats.Team), t);
            GameManager.instance.onlineTeam = te;
        }
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

    [PunRPC]
    void UpdateLootableWeaponData_RPC(Vector3 spp, Dictionary<string, string> param)
    {
        foreach (LootableWeapon lw in FindObjectsOfType<LootableWeapon>(true).ToList())
        {
            if (lw.spawnPointPosition == spp)
            {
                lw.UpdateData(param);
                //if (param.ContainsKey("ammo"))
                //    lw.ammo = int.Parse(param["ammo"]);

                //if (param.ContainsKey("ttl"))
                //    lw.tts = int.Parse(param["ttl"]);

                break;
            }
        }
    }

    [PunRPC]
    void AddForceLootableWeapon_RPC(Vector3 spp, Vector3 dir)
    {
        foreach (LootableWeapon lw in FindObjectsOfType<LootableWeapon>(true).ToList())
        {
            if (lw.spawnPointPosition == spp)
            {
                lw.GetComponent<Rigidbody>().AddForce(dir * 200);
            }
        }
    }

    [PunRPC]
    void SpawnNetworkWeapon_RPC(int wi, Vector3 spp, Vector3 fDir, Dictionary<string, int> param)
    {
        Debug.Log("SpawnNetworkWeapon_RPC");
        GameObject wo = Instantiate(GameManager.GetRootPlayer().playerInventory.allWeaponsInInventory[wi].GetComponent<WeaponProperties>().weaponRessource, spp, Quaternion.identity);
        wo.name = wo.name.Replace("(Clone)", "");

        Debug.Log(spp);

        try { wo.GetComponent<LootableWeapon>().networkAmmo = param["ammo"]; } catch (System.Exception e) { Debug.Log(e); }
        try { wo.GetComponent<LootableWeapon>().spareAmmo = param["spareammo"]; } catch (System.Exception e) { Debug.Log(e); }
        try { wo.GetComponent<LootableWeapon>().tts = param["tts"]; } catch (System.Exception e) { Debug.Log(e); }
        //wo.GetComponent<LootableWeapon>().GetComponent<Rigidbody>().AddForce(fDir * 200);

        //StartCoroutine(UpdateWeaponSpawnPosition_Coroutine(wo, spp));
        wo.GetComponent<LootableWeapon>().spawnPointPosition = spp;

        if (fDir != Vector3.zero)
            wo.GetComponent<Rigidbody>().AddForce(fDir * 200);
        Debug.Log($"DropWeapon_RPC: {wo.GetComponent<LootableWeapon>().spawnPointPosition}");
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
    void ResetAllExplosiveBarrels_RPC()
    {
        foreach (ExplosiveBarrel ic in FindObjectsOfType<ExplosiveBarrel>(true).ToList())
        {
            ic.GetComponent<Rigidbody>().velocity = Vector3.zero;
            ic.transform.position = ic.spawnPointPosition;
            ic.transform.rotation = ic.spawnPointRotation;
            ic.gameObject.SetActive(true);
        }
        Debug.Log("All Barrels Reset");
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

    [PunRPC]
    void UpdatePlayerTeam_RPC(string t, string pn)
    {
        foreach (PlayerMultiplayerMatchStats pms in FindObjectsOfType<PlayerMultiplayerMatchStats>().ToList())
            if (pms.username == pn)
                pms.networkTeam = (PlayerMultiplayerMatchStats.Team)System.Enum.Parse(typeof(PlayerMultiplayerMatchStats.Team), t);
    }




    [PunRPC]
    void DisableAmmoPack_RPC(Vector3 sp)
    {
        foreach (AmmoPack ap in FindObjectsOfType<AmmoPack>())
            if (ap.spawnPoint == sp)
                ap.enable = false;
    }

    [PunRPC]
    void ResetAllAmmoPacks_RPC()
    {
        foreach (AmmoPack ap in FindObjectsOfType<AmmoPack>())
            ap.enable = true;
    }

    [PunRPC]
    void StartOverShieldRespawn_RPC(int t)
    {
        Debug.Log("sdfaeqwer");
        StartCoroutine(StartOverShieldRespawn_Coroutine(t));
    }

    [PunRPC]
    void StartLootableWeaponRespawn_RPC(Vector3 v)
    {
        Debug.Log("sdfaeqwer");
        MultiplayerManager.instance.StartLootableWeaponRespawn(v);
    }



    // Multiplayer
    [PunRPC]
    void AddPlayerPoint_RPC(int pid)
    {
        Debug.Log("AddPlayerPoint_RPC");
        MultiplayerManager.instance.AddPlayerPoint(pid);
    }

    [PunRPC]
    void NextHillLocation_RPC()
    {
        Debug.Log("NextHillLocation_RPC");
        FindObjectOfType<HillManager>().NextLocation();
    }


    [PunRPC]
    void UpdateSwarmDifficulty_RPC(int ei)
    {
        GameManager.instance.difficulty = (SwarmManager.Difficulty)ei;
    }



    [PunRPC]
    public void LootOvershield(int pid, bool caller = true)
    {
        if (caller)
        {
            _pv.RPC("LootOvershield", RpcTarget.AllViaServer, pid, false);
        }
        else
        {
            GameManager.instance.pid_player_Dict[pid].maxOvershieldPoints = 150;


            int t = overshield.tts;
            int _time = FindObjectOfType<GameTime>().totalTime;

            int timeLeft = 0;

            if (_time < t)
                timeLeft = t - _time;
            else
                timeLeft = t - (_time % t);
            Debug.Log(timeLeft);

            StartCoroutine(StartOverShieldRespawn_Coroutine(timeLeft));
            overshield.gameObject.SetActive(false);
        }
    }







    IEnumerator StartOverShieldRespawn_Coroutine(int t)
    {
        Debug.Log($"Spawning Overshiled in {t} seconds");
        yield return new WaitForSeconds(t);
        overshield.gameObject.SetActive(true);
    }
}
